
using UnityEngine;
using UnityScript.Lang;
using System;

namespace PresenceEngine
{

    public interface iVisualiser
    {
        bool IsInitialised();

        void Initialise(GameObject presenceObject);
        void Deinitialise();
        void SetTransform(Vector3 pos, Quaternion rot);

        void Update(UncompressedFrame Frame);

        string GetName();

        Vector3 GetPosition();
        Quaternion GetRotation();


        void SettingsToTask(StoryEngine.StoryTask task, string prefix);


         void SettingsFromTask(StoryEngine.StoryTask task, string prefix);
       

    }


    public class ShowSkeleton : iVisualiser
    {
        //        ParticleCloud cloud;

        GameObject PresenceObject;
        GameObject Head, Body, HandLeft, HandRight;
        ParticleCloud Cloud;
        bool __isInitialised = false;

        string _name = "ShowSkeleton";
        UncompressedFrame lastFrame;

        public void SettingsToTask(StoryEngine.StoryTask task, string prefix)
        {
         

        }
        public void SettingsFromTask(StoryEngine.StoryTask task, string prefix)
        {

           
        }


        public string GetName()
        {

            return _name;

        }


        public bool IsInitialised()
        {
            return __isInitialised;
        }
        public void Deinitialise()
        {

            foreach (Transform child in PresenceObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            __isInitialised = false;
        }
        public void Initialise(GameObject presenceObject)
        {
            // Parent object for any visualisation objects.

            PresenceObject = presenceObject;

            foreach (Transform child in PresenceObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            GameObject n;

            if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
            {
                n = DebugObject.getNullObject(0.25f, 0.25f, 0.5f);
                n.transform.SetParent(PresenceObject.transform, false);
                Head = n;
            }

            n = DebugObject.getNullObject(0.25f);
            n.transform.SetParent(PresenceObject.transform, false);
            HandLeft = n;

            n = DebugObject.getNullObject(0.25f);
            n.transform.SetParent(PresenceObject.transform, false);
            HandRight = n;

            n = DebugObject.getNullObject(0.5f);
            n.transform.SetParent(PresenceObject.transform, false);
            Body = n;


            Cloud = new ParticleCloud(5000, "CloudDyn", false);
            Cloud.CloudObject.transform.SetParent(PresenceObject.transform, false);

            lastFrame = new UncompressedFrame();


            __isInitialised = true;
        }


        public void SetTransform(Vector3 pos, Quaternion rot)
        {

            if (PresenceObject != null)
            {
                PresenceObject.transform.localPosition = pos;
                //PresenceObject.transform.localScale=scale;
                PresenceObject.transform.localRotation = rot;
            }
        }


        public Vector3 GetPosition()
        {

            if (PresenceObject != null)
                return PresenceObject.transform.localPosition;

            return Vector3.zero;
        }

        public Quaternion GetRotation()
        {

            if (PresenceObject != null)
                return PresenceObject.transform.localRotation;

            return Quaternion.identity;
        }


        Vector3 HandLeftP, HandRightP;


        public void Update(UncompressedFrame Frame)
        {
            if (__isInitialised)
            {

                // Check validity.
                if (!(Frame != null && Frame.Joints != null && Frame.Tracked != null))
                {
                    Debug.LogWarning("Frame invalid");
                    return;
                }

                HandLeft.transform.localPosition = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft];
                HandRight.transform.localPosition = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight];

                Body.transform.localPosition = Frame.UserPosition;

                if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                {
                    //  if (Frame.Tracked
                    Head.transform.localPosition = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head];
                    Head.transform.localRotation = Frame.HeadOrientation;
                }

                // Check if frame is new.

                //Debug.Log(Frame.FrameNumber);


                if (lastFrame != Frame)

                {

                    Vector3 last = lastFrame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft];
                    Vector3 current = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft];

                    if (lastFrame.Tracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft] &&
                        Frame.Tracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft])
                    {

                        for (float i = 0; i < 1; i += 0.25f)
                        {
                            Cloud.Emit(Vector3.Lerp(last, current, i));
                        }

                    }

                    //HandLeftP = current;

                    last = lastFrame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight];
                    current = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight];

                    if (lastFrame.Tracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight] &&
                        Frame.Tracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight])
                    {
                        for (float i = 0; i < 1; i += 0.25f)
                        {
                            Cloud.Emit(Vector3.Lerp(last, current, i));
                        }

                    }


                    //HandRightP = current;


                    //     Cloud.Emit(Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight]);


                    //  Debug.Log(Body.transform.position = Frame.Body);
                }
                lastFrame = Frame;


                //       System.Array.Copy(Frame.Points,lastFrame.Points,Points.Length);

                //System.Array.Copy(Frame.Tracked,Tracked,Tracked.Length);
                //FrameNumber=reference.FrameNumber;

                //}



            }



        }




    }

    public class PointCloud : iVisualiser
    {
        string _name = "PointCloud";

        GameObject PresenceObject;
        GameObject Head, Body, HandLeft, HandRight;
        ParticleCloud Cloud;
        bool __isInitialised = false;
        UncompressedFrame lastFrame;
        GameObject PLight;

        public float CloudVisible = 0;

        public void SettingsToTask (StoryEngine.StoryTask task, string prefix)
        {
            task.SetFloatValue(prefix + "_cloudvisible", CloudVisible);
          
        }
        public void SettingsFromTask(StoryEngine.StoryTask task, string prefix)
        {

            task.GetFloatValue(prefix + "_cloudvisible", out CloudVisible);

            //int cloudcontrol;
            //if (task.GetIntValue(prefix+"_cloudcontrol", out cloudcontrol))
            //{

            //    CloudOn = cloudcontrol == 1 ? true : false;
            //}





        }

        public string GetName()
        {

            return _name;

        }

        public bool IsInitialised()
        {
            return __isInitialised;
        }

        public void Deinitialise()
        {

            foreach (Transform child in PresenceObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            __isInitialised = false;
        }

        public void Initialise(GameObject presenceObject)
        {
            // Parent object for any visualisation objects.

            PresenceObject = presenceObject;

            foreach (Transform child in PresenceObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            GameObject n;

            if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
            {
                n = DebugObject.getNullObject(0.25f, 0.25f, 0.5f);
                n.transform.SetParent(PresenceObject.transform, false);
                Head = n;
            }

            n = DebugObject.getNullObject(0.25f);
            n.transform.SetParent(PresenceObject.transform, false);
            HandLeft = n;

            n = DebugObject.getNullObject(0.25f);
            n.transform.SetParent(PresenceObject.transform, false);
            HandRight = n;

            n = DebugObject.getNullObject(0.5f);
            n.transform.SetParent(PresenceObject.transform, false);
            Body = n;

            PLight = new GameObject("PLight");
            Light lightComp = PLight.AddComponent<Light>();
            lightComp.color = new Color(137f / 256f, 223f / 256f, 249f / 256f);
            lightComp.intensity = 0.35f;
            PLight.transform.localPosition = new Vector3(0, 1.5f, 0);


            //PLight = new Light();

            PLight.transform.SetParent(PresenceObject.transform, false);
            //pLight.transform.localPosition = Vector3.zero;
            //pLight.type = LightType.Point;



            Cloud = new ParticleCloud(20000, "Cloud", true);
            Cloud.CloudObject.transform.SetParent(PresenceObject.transform, false);

            lastFrame = new UncompressedFrame();

            __isInitialised = true;
        }


        public void SetTransform(Vector3 pos, Quaternion rot)
        {

            if (PresenceObject != null)
            {
                PresenceObject.transform.localPosition = pos;
                //PresenceObject.transform.localScale=scale;
                PresenceObject.transform.localRotation = rot;
            }
        }


        public Vector3 GetPosition()
        {

            if (PresenceObject != null)
                return PresenceObject.transform.localPosition;

            return Vector3.zero;
        }

        public Quaternion GetRotation()
        {

            if (PresenceObject != null)
                return PresenceObject.transform.localRotation;

            return Quaternion.identity;
        }


        Vector3 HandLeftP, HandRightP;

        // default callibration values. c being a correction presumably and f the fov


        double fx_d = 1.0d / 5.9421434211923247e+02;
        double fy_d = 1.0d / 5.9104053696870778e+02;
        double cx_d = 3.3930780975300314e+02;
        double cy_d = 2.4273913761751615e+02;


        Vector3 depthToWorld(int x, int y, int depthValue)
        {

            Vector3 result = Vector3.zero;

            //double depth = depthLookUp [depthValue];
            //float depth = rawDepthToMeters(depthValue);

            float depth = depthValue / 1000f;

            result.x = (float)((x - cx_d) * depth * fx_d);
            result.y = (float)((y - cy_d) * depth * fy_d);
            result.z = (float)(depth);

            return result;

        }

        public void Update(UncompressedFrame Frame)
        {
            if (__isInitialised)
            {

                // Check validity.
                if (!(Frame != null && Frame.Joints != null && Frame.Tracked != null))
                {
                    Debug.LogWarning("Frame invalid");
                    return;
                }

                if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    Head.SetActive(Frame.UserTracked);


                Body.SetActive(Frame.UserTracked);
                HandLeft.SetActive(Frame.UserTracked);
                HandRight.SetActive(Frame.UserTracked);
                PLight.SetActive(Frame.UserTracked);
                
                HandLeft.transform.localPosition = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft];
                HandRight.transform.localPosition = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight];

                Body.transform.localPosition = Frame.UserPosition;
                PLight.transform.localPosition = new Vector3(Frame.UserPosition.x, 1, Frame.UserPosition.z);

                if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                {
                    Head.transform.localPosition = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head];
                    Head.transform.localRotation = Frame.HeadOrientation;
                }

                // takes a kinect styled uint[] RawDepthMap
                // and plots the points into a Particle Cloud, with scale corrected if the frame was downsampled.



                int ParticleIndex = 0;

                int Width = Frame.Width;
                int Height = Frame.Height;
                int Scale = 640 / Width;

                Vector3 point;

                // Downsampling is handled in depthtransport.

                for (int y = 0; y < Height; y++)
                {

                    for (int x = 0; x < Width; x++)
                    {
                        int i = y * Width + x;

                        ushort userMap = (ushort)(Frame.RawDepth[i] & 7);
                        ushort userDepth = (ushort)(Frame.RawDepth[i] >> 3);

                        if (userMap != 0)
                        {

                            point = depthToWorld(x * Scale, y * Scale, userDepth);
                            point.x = -point.x;
                            point.y = -point.y + SETTINGS.kinectHeight;
                            //     point += SETTINGS.kinectHeight;

                            Cloud.allParticles[ParticleIndex].position = point;

                            ParticleIndex++;
                            if (ParticleIndex == Cloud.allParticles.Length)
                            {
                                // abort
                                x = Width;
                                y = Height;

                            }

                            // ParticleIndex = ParticleIndex % Cloud.allParticles.Length;

                        }

                    }

                }

                Cloud.ApplyParticles(CloudVisible>0 ? ParticleIndex : 0);

                // Check if frame is new.

                //if (lastFrame != Frame)

                //{


                //}
                lastFrame = Frame;


            }


        }




    }




}