
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
//using NUnit.Framework.Constraints;


namespace Presence
{

    /*
     * 
     *       private Vector3[] player1JointsPos, player2JointsPos;
     *       
     *         public Vector3 GetJointPosition(uint UserId, int joint) -> retrieves.
     *             public bool IsJointTracked(uint UserId, int joint)

     *         
     *  HipCenter = 0,
        Spine = 1,
        ShoulderCenter = 2,
        Head = 3,
        ShoulderLeft = 4,
        ElbowLeft = 5,
        WristLeft = 6,
        HandLeft = 7,
        ShoulderRight = 8,
        ElbowRight = 9,
        WristRight = 10,
        HandRight = 11,
        HipLeft = 12,
        KneeLeft = 13,
        AnkleLeft = 14,
        FootLeft = 15,
        HipRight = 16,
        KneeRight = 17,
        AnkleRight = 18,
        FootRight = 19,
        Count = 20
        */








    public enum DEPTHMODE
    {
        OFF,
        LIVE,
        RECORD,
        PLAYBACK


    }




    public class DepthTransport
    {

        // transports depth data from kinect or file

        string me = "Depthtransport: ";

        int[] DEPTHMAPSIZE = { 0, 640 * 480, 320 * 240, 0, 160, 120 };

        int CurrentFrame = 0;

      public  iTransCoder TransCoder;
        public bool IsRecording = false;

        public UncompressedFrame ActiveUncompressedFrame;

        //  public int Min, Max;

        //  public GameObject kinectObject;

        //  public bool live = false;

        //    public int DepthWidth, DepthHeight; // W & H for raw depth.

        //    public RawDepthSequence depthSequence;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        //   public GameObject km;

        static public DepthTransport OwnsKinect;

#endif

        //   static public KinectManager kinectManager;


        //   public static iTransCoder[] TransCoders;

        public void SetTranscoder (string name)
        {
           
               
                switch (name)
                {
                    case "SkeletonOnly":
                        TransCoder = new SkeletonOnly();
                    break;
                    default:
                      break;
                }

            }

           



        



        // static public int DepthMin, DepthMax;
        private DEPTHMODE __mode = DEPTHMODE.OFF;

        public DEPTHMODE Mode
        {
            get
            {
                return __mode;
            }
            set
            {

                __mode = value;

                switch (__mode)
                {

                    case DEPTHMODE.LIVE:
                    case DEPTHMODE.RECORD:

                        // Any instance can be 'live', which means it'll be working with live buffer data.
                        // On windows, the first instance to go live will assume control over the kinect.

                        __mode = DEPTHMODE.LIVE;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

                        // On server / windows we try to fire up the kinect.
                        // Making a coroutine would prevent blocking...?

                        if (DepthTransport.OwnsKinect == null)
                        {

                            if (!KinectManager.Instance.IsInitialized())

                            {
                                Debug.Log(me + "Waking up Kinectmanager.");

                                KinectManager.Instance.WakeUp();

                                if (KinectManager.Instance.IsInitialized() && KinectManager.Instance.getUserDepthWidth() == 640 && KinectManager.Instance.getUserDepthHeight() == 480)
                                {
                                    Debug.Log(me + "Kinectmanager initialised and at 640 x 480.");

                                    DepthTransport.OwnsKinect = this;

                                }
                                else
                                {

                                    Debug.LogError(me + "Kinectmanager not initialised or not at 640 480.");

                                }

                            }

                        }

#endif
                        break;

                    case DEPTHMODE.PLAYBACK:
                    case DEPTHMODE.OFF:

                        __mode = DEPTHMODE.OFF;

                        //      DepthHeight = 480;
                        //    DepthWidth = 640;


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

                        if (KinectManager.Instance.IsInitialized())
                        {

                            KinectManager.Instance.shutDown();

                            Debug.Log(me + "Shutting down Kinectmanager.");

                        }

#endif

                        break;

                    default:
                        break;



                }

            }

        }

        // Get joint positions



        // Check if a user has been detected.

        public bool IsUserDetected()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            return (KinectManager.Instance.IsInitialized() ? KinectManager.Instance.IsUserDetected() : false);

#else
            return false;
#endif
            //      return (__mode == DEPTHMODE.LIVE || __mode == DEPTHMODE.RECORD) ? KinectManager.Instance.IsUserDetected() : false;
        }

        // Retrieve the latest uncompressed frame.

        public int GetNewFrame(out UncompressedFrame Frame)
        {

            // Get new frame can only be called on the instance that owns the kinect. 

            UncompressedFrame NewFrame = new UncompressedFrame();

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            if (OwnsKinect == this && KinectManager.Instance.IsInitialized())
            {
                CurrentFrame++;

                NewFrame.RawDepth = GetRawDepthMap();
                getSkeleton(ref NewFrame);

                Frame = NewFrame;

                return CurrentFrame;
            }



#endif
            // For debugging we generate skeletondata.


            CurrentFrame++;
            
            float hx = 6f * Mathf.PerlinNoise(0.1f * Time.time, 0);
            float hz = 6f * Mathf.PerlinNoise(0, 0.1f * Time.time);

            NewFrame.Body = new Vector3(hx, PRESENCE.kinectHeight, hz);

            NewFrame.Joints = new Vector3[(int)KinectWrapper.NuiSkeletonPositionIndex.Count];
            NewFrame.Tracked = new bool[(int)KinectWrapper.NuiSkeletonPositionIndex.Count];


          for (int j=0;j< NewFrame.Joints.Length; j++)
            {
                NewFrame.Joints[j] = Vector3.zero;
                NewFrame.Tracked[j] = false;

            }


            NewFrame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft] = new Vector3(hx - 0.5f, PRESENCE.kinectHeight / 2, hz);
            NewFrame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight] = new Vector3(hx + 0.5f, PRESENCE.kinectHeight / 2, hz);

            Frame = NewFrame;
            return CurrentFrame;



        }

        public bool Encode (int NewFrameNumber,UncompressedFrame NewFrame, StoryEngine.StoryTask task)
        {


            return TransCoder.PutFrame(NewFrameNumber, NewFrame, task, IsRecording);
        }

        public bool Decode (int NewFrameNumber, out UncompressedFrame NewFrame, StoryEngine.StoryTask task)
        {
            
            return TransCoder.GetFrame(NewFrameNumber, out NewFrame, task);
        }


        public Vector3 getPosition()

        {

            Vector3 position = Vector3.zero;

            if (__mode == DEPTHMODE.LIVE || __mode == DEPTHMODE.RECORD)

            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

                uint playerID = KinectManager.Instance != null ? KinectManager.Instance.GetPlayer1ID() : 0;

                //   Debug.Log(playerID);


                if (playerID >= 0)
                {

                    bool MirroredMovement = true;

                    position = KinectManager.Instance.GetUserPosition(playerID);

                    position.z = !MirroredMovement ? -position.z : position.z;
                    position.x = MirroredMovement ? -position.x : position.x;
                    position.y += PRESENCE.kinectHeight;

                    //    if (MirroredMovement)
                    //   {
                    //       position.x = -position.x;
                    //   }



                }


#endif
            }

            return position;

        }

        //        public Vector3 getJoint(int joint)
        //        {

        //            Vector3 posJoint = Vector3.zero;

        //            if (__mode == DEPTHMODE.LIVE || __mode == DEPTHMODE.RECORD)

        //            {


        //#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        //                uint playerID = KinectManager.Instance != null ? KinectManager.Instance.GetPlayer1ID() : 0;

        //                if (playerID >= 0)
        //                {

        //                    bool MirroredMovement = true;

        //                    posJoint = KinectManager.Instance.GetJointPosition(playerID, joint);

        //                    posJoint.z = !MirroredMovement ? -posJoint.z : posJoint.z;
        //                    posJoint.x = MirroredMovement ? -posJoint.x : posJoint.x;
        //                    posJoint.y += PRESENCE.kinectHeight;

        //                    ///     if (MirroredMovement)
        //                    //    {
        //                    //      posJoint.x = -posJoint.x;
        //                    //}



        //                }
        //                else
        //                {

        //                    // playerid is 0 : no player.

        //                    //  Debug.Log("playerid is 0");
        //                }

        //#endif


        //            }



        //            return posJoint;

        //        }

        Vector3 getJoint(int joint)
        {

            Vector3 posJoint = Vector3.zero;


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            if (OwnsKinect == this && KinectManager.Instance.IsInitialized())
            {

                uint playerID = KinectManager.Instance != null ? KinectManager.Instance.GetPlayer1ID() : 0;

                if (playerID >= 0)
                {

                    bool MirroredMovement = true;

                    posJoint = KinectManager.Instance.GetJointPosition(playerID, joint);

                    posJoint.z = !MirroredMovement ? -posJoint.z : posJoint.z;
                    posJoint.x = MirroredMovement ? -posJoint.x : posJoint.x;
                    posJoint.y += PRESENCE.kinectHeight;

                }
                else
                {

                    // playerid is 0 : no player.

                    //  Debug.Log("playerid is 0");
                }
            }
#endif

            
            return posJoint;

        }

        void getSkeleton(ref UncompressedFrame Frame)
        {
            
            // Retrieves skeleton data from kinect if possible. Works by reference, so data will only be changed if possible.

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            

            if (OwnsKinect == this && KinectManager.Instance.IsInitialized())
            {

            

                uint playerID = KinectManager.Instance != null ? KinectManager.Instance.GetPlayer1ID() : 0;

                if (playerID >= 0)
                {
                    Frame.Joints = new Vector3[(int)KinectWrapper.NuiSkeletonPositionIndex.Count];
                    Frame.Tracked = new bool[(int)KinectWrapper.NuiSkeletonPositionIndex.Count];

                    bool MirroredMovement = true;

                    for (int joint = 0; joint < Frame.Joints.Length; joint++)
                    {

                        if ( KinectManager.Instance.IsJointTracked(playerID, joint))
                        {

                            Frame.Tracked[joint] = true;
                            Frame.Joints[joint] = KinectManager.Instance.GetJointPosition(playerID, joint);

                            Frame.Joints[joint].z = !MirroredMovement ? -Frame.Joints[joint].z : Frame.Joints[joint].z;
                            Frame.Joints[joint].x = MirroredMovement ? -Frame.Joints[joint].x : Frame.Joints[joint].x;
                            Frame.Joints[joint].y += PRESENCE.kinectHeight;

                        }
                        else
                        {
                            Frame.Tracked[joint] = false;
                                                   }


                    }

                }
               
            }
#endif

           
        }


        public RovingValue DepthMin, DepthMax;

        //public void 

        public void ApplyEdge(ushort[] depthMap, Texture2D texture, int min, int max)
        {

            // should be same size.

            int width = texture.width;
            int height = texture.height;

            Color[] ImageMap = texture.GetPixels();

            for (int i = 0; i < ImageMap.Length; i++)
            {
                //ushort userMap = (ushort)(DepthMap[i] & 7);


                if ((depthMap[i] & 7) != 0)
                {

                    // non empty pixel.
                    int x = i & width;
                    int y = i / width;


                    if ((x > 0 && (depthMap[i - 1] & 7) == 0) ||
                        (x < width - 1 && (depthMap[i + 1] & 7) == 0) ||
                        (y > 0 && (depthMap[i - width] & 7) == 0) ||
                        (y < height - 1 && (depthMap[i + width] & 7) == 0))
                    {

                        int userDepth = depthMap[i] >> 3;


                        float value = (float)(userDepth - min) / (float)(max - min);
                        value = Mathf.Clamp01(value);


                        ImageMap[i] = new Color(0, value, 1);






                    }





                }




            }

            texture.SetPixels(ImageMap);
            texture.Apply();


        }


        ushort[] GetRawDepthMap()
        {

            // Get raw depth map, so depth plus user map in a ushort[]

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            if (KinectManager.Instance.IsInitialized())
                return KinectManager.Instance.GetRawDepthMap();


#endif


            return new ushort[DEPTHMAPSIZE[0]];
        }


        //switch (Mode)
        //{

        //    case DEPTHMODE.LIVE:

        //        if (KinectManager.Instance.IsInitialized())
        //        {

        //            return KinectManager.Instance.GetRawDepthMap();

        //        }
        //        else
        //        {

        //            return new ushort[DEPTHMAPSIZE[0]);

        //        }

        //    case DEPTHMODE.RECORD:

        //        if (depthSequence == null)
        //        {
        //            depthSequence = new RawDepthSequence();
        //        }

        //        ushort[] data;

        //        if (KinectManager.Instance.IsInitialized())
        //        {

        //            data = KinectManager.Instance.GetRawDepthMap();

        //        }
        //        else
        //        {

        //            data = new ushort[DepthWidth * DepthHeight];

        //        }

        //        depthSequence.Put(data);

        //        return data;


        //    case DEPTHMODE.PLAYBACK:

        //        if (depthSequence == null)
        //        {
        //            depthSequence = new RawDepthSequence();
        //            depthSequence.LoadFrom("depthCapture");
        //        }

        //        return depthSequence.GetNext();


        //    case DEPTHMODE.OFF:
        //    default:

        //        return new ushort[DepthWidth * DepthHeight];

        //}




        //if (IsLive())
        //{

        //    return KinectManager.Instance.GetRawDepthMap();

        //}










        //static Texture2D DepthTexture;

            /*
        public ushort[] TextureToRawDepth(Texture2D Texture, int min, int max)
        {



            if (Texture == null)
            {
                return new ushort[0];
            }




            Color[] ImageMap = Texture.GetPixels();
            //  int sample = (Width * Height) / ImageMap.Length ; // texture can be 640 320 160

            // Debug.Log("sample " + sample);


            //   ushort[] DepthMap = new ushort[Width * Height];
            ushort[] DepthMap = new ushort[ImageMap.Length];
            ushort userMap, depthValue;



            for (int i = 0; i < ImageMap.Length; i++)
            {
                float u = ImageMap[i].b;
                float v = ImageMap[i].g;

                if (u > 0.9f)
                {
                    userMap = 1; // user 1

                    int userDepth = min + (int)((max - min) * v);


                    depthValue = (ushort)((userDepth << 3) | userMap);

                    //                DepthMap[i*sample] = depthValue;
                    DepthMap[i] = depthValue;

                }






            }



            return DepthMap;



        }


        public ushort[] TextureToRawDepthV3(Texture2D Texture)
        {







            if (Texture == null)
            {
                return new ushort[0];
            }




            Color[] ImageMap = Texture.GetPixels();
            ushort[] DepthMap = new ushort[ImageMap.Length];

            ushort userMap, depthValue;

            for (int i = 0; i < ImageMap.Length; i++)
            {
                float a = ImageMap[i].g;
                float b = ImageMap[i].b;

                if (a > 0)
                {
                    userMap = 1; // user 1
                    int userDepth;

                    if (a > 0.5f)
                    {

                        userDepth = 512 - (int)(b * 256);

                    }
                    else
                    {

                        userDepth = (int)(b * 256);

                    }
                    userDepth *= 8;
                    userDepth = 4096 - userDepth;


                    depthValue = (ushort)((userDepth << 3) | userMap);

                    DepthMap[i] = depthValue;

                }






            }



            return DepthMap;



        }


        public ushort[] TextureToRawDepthV2(Texture2D Texture)
        {







            if (Texture == null)
            {
                return new ushort[0];
            }




            Color[] ImageMap = Texture.GetPixels();
            ushort[] DepthMap = new ushort[ImageMap.Length];

            ushort userMap, depthValue;

            for (int i = 0; i < ImageMap.Length; i++)
            {
                float v = ImageMap[i].r;

                if (v > 0)
                {
                    userMap = 1; // user 1


                    int userDepth = (int)((1f - v) * 4096f);

                    depthValue = (ushort)((userDepth << 3) | userMap);

                    DepthMap[i] = depthValue;

                }






            }



            return DepthMap;



        }
        public ushort[] TextureToRawDepthV1(Texture2D Texture)
        {







            if (Texture == null)
            {
                return new ushort[0];
            }




            Color[] ImageMap = Texture.GetPixels();
            ushort[] DepthMap = new ushort[ImageMap.Length];

            ushort userMap, depthValue;

            for (int i = 0; i < ImageMap.Length; i++)
            {

                float h, s, v;

                Color.RGBToHSV(ImageMap[i], out h, out s, out v);

                if (v > 0)
                {
                    userMap = 1; // user 1

                    int cycle = (int)(v * 16f) * 256;
                    int userDepth = cycle + (int)(h * 256);

                    depthValue = (ushort)((userDepth << 3) | userMap);

                    DepthMap[i] = depthValue;

                }






            }



            return DepthMap;



        }



        public Texture2D RawDepthToTexture(ushort[] RawDepth, Texture2D Texture)
        {



            //ushort[] DepthMap = KinectManager.Instance.GetRawDepthMap();

            if (Texture == null)
            {
                Texture = new Texture2D(DepthWidth, DepthHeight);
            }


            int min = 999999;
            int max = -999999;

            ushort[] UserChannel = new ushort[RawDepth.Length];
            ushort[] DepthChannel = new ushort[RawDepth.Length];




            for (int i = 0; i < RawDepth.Length; i++)
            {
                UserChannel[i] = (ushort)(RawDepth[i] & 7);
                DepthChannel[i] = (ushort)((RawDepth[i] >> 3));


                if (UserChannel[i] != 0)
                {
                    //   ushort userDepth = (ushort)((DepthMap[i] >> 3));
                    min = Mathf.Min(min, DepthChannel[i]);
                    max = Mathf.Max(max, DepthChannel[i]);
                }

            }

            if (DepthMin == null)
            {
                DepthMin = new RovingValue(10);
                DepthMax = new RovingValue(10);


            }
            DepthMin.Rove(min);
            DepthMax.Rove(max);

            //min = DepthMin.Int;
            //max = DepthMax.Int;

            Min = min;
            Max = max;

            Color[] ImageMap = new Color[RawDepth.Length];


            for (int i = 0; i < RawDepth.Length; i++)
            {


                //     ushort userMap = (ushort)(DepthMap[i] & 7);
                //   ushort userDepth = (ushort)((DepthMap[i] >> 3));
                ushort userMap = UserChannel[i];
                ushort userDepth = DepthChannel[i];




                if (userMap != 0)
                {

                    float value = (float)(userDepth - min) / (float)(max - min);
                    value = Mathf.Clamp01(value);

                    Color col = new Color(0, value, 1);

                    ImageMap[i] = col;
                }
                else
                {
                    
                    //int x = i & Width;
                    //int y = i / Width;

                    //int t = 0;
                    //int w = 0;

                    //if (x > 0 && UserChannel[i - 1] != 0)
                    //{
                    //    t += DepthChannel[i - 1];
                    //    w++;
                    //}
                    //if (y > 0 && UserChannel[i - Width] != 0)
                    //{
                    //    t += DepthChannel[i - Width];
                    //    w++;
                    //}
                    //if (x < Width - 1 && UserChannel[i + 1] != 0)
                    //{
                    //    t += DepthChannel[i + 1];
                    //    w++;
                    //}
                    //if (y < Height - 1 && UserChannel[i + Width] != 0)
                    //{
                    //    t += DepthChannel[i + Width];
                    //    w++;
                    //}

                    //if (w > 0)
                    //{

                    //    float cud = t / w;
                    //    float value = (float)(cud - min) / (float)(max - min);

                    //    Color col = new Color(0, value, 0);

                    //    ImageMap[i] = col;



                    //}
                    //else
                    //{

                    //    ImageMap[i] = Color.black;


                    //}
                    


                    ImageMap[i] = Color.black;

                    //ImageMap[i] =new Color32(0, 0, 0,255);


                }




            }


            Texture.SetPixels(ImageMap);
            Texture.Apply();

            return Texture;



        }


        /*
        public static Texture2D GetDepthTextureV3()
        {

            if (IsLive())
            {

                ushort[] DepthMap = KinectManager.Instance.GetRawDepthMap();

                if (DepthTexture == null)
                {
                    DepthTexture = new Texture2D(Width, Height);
                }


                //  Color32[] ImageMap = new Color32[DepthMap.Length];
                Color[] ImageMap = new Color[DepthMap.Length];


                for (int i = 0; i < DepthMap.Length; i++)
                {


                    ushort userMap = (ushort)(DepthMap[i] & 7);
                    ushort userDepth = (ushort)((DepthMap[i] >> 3));

                    if (userMap != 0)
                    {


                        int depth = (4096 - userDepth);
                        int value = depth / 8;// map to 512


                        //    float angle = depth % 256;
                        //   int level = depth / 256;



                        float a = value / 512f;
                        float b = (value % 256) / 256f;
                        if (a > 0.5f)
                            b = 1 - b;



                        Color col = new Color(0, a, b);

                        ImageMap[i] = col;
                    }
                    else
                    {

                        //ImageMap[i] =new Color32(0, 0, 0,255);
                        ImageMap[i] = Color.black;

                    }




                }


                DepthTexture.SetPixels(ImageMap);
                DepthTexture.Apply();

                return DepthTexture;

                //return KinectManager.Instance.GetUsersLblTex();

            }

            return new Texture2D(1, 1);

        }


        public static Texture2D GetDepthTextureV2()
        {

            if (IsLive())
            {

                ushort[] DepthMap = KinectManager.Instance.GetRawDepthMap();

                if (DepthTexture == null)
                {
                    DepthTexture = new Texture2D(Width, Height);
                }


                //  Color32[] ImageMap = new Color32[DepthMap.Length];
                Color[] ImageMap = new Color[DepthMap.Length];


                for (int i = 0; i < DepthMap.Length; i++)
                {


                    ushort userMap = (ushort)(DepthMap[i] & 7);
                    ushort userDepth = (ushort)((DepthMap[i] >> 3));

                    if (userMap != 0)
                    {

                        float v = 1 - (userDepth / 4096f);


                        Color c = new Color(v, 0, 0);

                        ImageMap[i] = c;
                    }
                    else
                    {

                        //ImageMap[i] =new Color32(0, 0, 0,255);
                        ImageMap[i] = Color.black;

                    }




                }


                DepthTexture.SetPixels(ImageMap);
                DepthTexture.Apply();

                return DepthTexture;

                //return KinectManager.Instance.GetUsersLblTex();

            }

            return new Texture2D(1, 1);

        }


        public static Texture2D GetDepthTextureV1()
        {

            if (IsLive())
            {

                ushort[] DepthMap = KinectManager.Instance.GetRawDepthMap();

                if (DepthTexture == null)
                {
                    DepthTexture = new Texture2D(Width, Height);
                }


                //  Color32[] ImageMap = new Color32[DepthMap.Length];
                Color[] ImageMap = new Color[DepthMap.Length];


                for (int i = 0; i < DepthMap.Length; i++)
                {


                    ushort userMap = (ushort)(DepthMap[i] & 7);
                    ushort userDepth = (ushort)((DepthMap[i] >> 3));

                    if (userMap != 0)
                    {

                    //    byte value = (byte)(userDepth / 16);
        //
               //         ImageMap[i].r = value;
           //             ImageMap[i].g = 0;
           //             ImageMap[i].b = 0;
           //             ImageMap[i].a = 255;


                        float h = (userDepth % 256) / 256f;
                        //     h = 0;
                        float s = 1;
                        float v = 1 - userDepth / 4096f;
                        // v = 1;

                        Color c = Color.HSVToRGB(h, s, v);

                        ImageMap[i] = c;
                    }
                    else
                    {

                        //ImageMap[i] =new Color32(0, 0, 0,255);
                        ImageMap[i] = Color.black;

                    }




                }


                DepthTexture.SetPixels(ImageMap);
                DepthTexture.Apply();

                return DepthTexture;

                //return KinectManager.Instance.GetUsersLblTex();

            }

            return new Texture2D(1, 1);

        }

    */





        /*
        public static void SetActive(bool state)
        {

            if (state)
            {

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

                // km = GameObject.Find("KinectManager");
                // kinectManager = km.GetComponent<KinectManager>();


                Debug.Log(me + "Waking up Kinectmanager.");

                KinectManager.Instance.WakeUp();

                // kinectManager.WakeUp();

                if (KinectManager.Instance.IsInitialized())
                {
                    Debug.Log(me + "Kinectmanager initialised.");
                    Height = KinectManager.Instance.getUserDepthHeight();
                    Width = KinectManager.Instance.getUserDepthWidth();


                }
                else
                {

                    Debug.LogWarning(me + "Kinectmanager not initialised.");
                }

#endif

            }
            else
            {

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

                //       km = GameObject.Find("KinectManager");
                //     kinectManager = km.GetComponent<KinectManager>();

                //   kinectManager.shutDown();

                KinectManager.Instance.shutDown();

                Debug.Log(me + "Shutting down Kinectmanager.");

#endif

            }

            //		kinectObject=obj;



        }

        public static bool IsLive()
        {

            live = false;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            live = KinectManager.Instance != null ? KinectManager.Instance.IsInitialized() : false;

#else


            Debug.LogError(me + "Unable to run live kinect on non windows platform.");


#endif

            return live;

        }

        public static void InitDummy(GameObject obj)
        {

            kinectObject = obj;
            live = false;

        }
    */





    }

    [System.Serializable]

    public class RawDepthSequence
    {

        // raw ushort depth data - sequence container

        List<ushort[]> Frames;

        [NonSerialized]
        int Index;

        public RawDepthSequence()
        {

            Frames = new List<ushort[]>();
            Index = 0;

        }

        public void Put(ushort[] data)
        {

            Frames.Add(data);

        }


        public ushort[] Get(int index)
        {

            if (index >= 0 && index < Frames.Count)
            {

                return Frames[index];
            }

            return null;

        }


        public ushort[] GetNext()
        {
            Index++;

            if (Index == Frames.Count)
            {
                Index = 0;

            }

            return Frames[Index];

        }

        public void SaveAs(string fileName)
        {

            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Create(Application.persistentDataPath + "/" + fileName + ".dep");

            bf.Serialize(file, this);

            file.Close();

        }

        public void LoadFrom(string fileName)
        {
            string fullPath = Application.persistentDataPath + "/" + fileName + ".dep";

            if (File.Exists(fullPath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(fullPath, FileMode.Open);

                RawDepthSequence loaded = (RawDepthSequence)bf.Deserialize(file);
                file.Close();

                this.Frames = loaded.Frames;

            }

        }




    }





}