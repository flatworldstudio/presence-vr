
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

        void SetTransform(Vector3 pos, Vector3 scale, Quaternion rot);

        void Update(UncompressedFrame Frame);

        string GetName();

        Vector3 GetPosition();

        Vector3 GetScale();

        Quaternion GetRotation();

        void SettingsToTask(StoryEngine.StoryTask task, string prefix);

        void SettingsFromTask(StoryEngine.StoryTask task, string prefix);

    }

    public class ShowSkeleton : iVisualiser
    {

        //    GameObject PresenceObject;
        GameObject ParentObject;
        GameObject Head, Body, HandLeft, HandRight;
        ParticleCloud Cloud;
        bool __isInitialised = false;
        string _name = "ShowSkeleton";
        UncompressedFrame lastFrame;
        bool IsDrawing = false;

        public void SettingsToTask(StoryEngine.StoryTask task, string prefix)
        {
            task.SetIntValue(prefix + "_isdrawing", IsDrawing ? 1 : 0);

        }
        public void SettingsFromTask(StoryEngine.StoryTask task, string prefix)
        {
            int v;

            if (task.GetIntValue(prefix + "_isdrawing", out v))
            {
                IsDrawing = (v == 1);
            }


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
            GameObject.Destroy(ParentObject);

            //foreach (Transform child in PresenceObject.transform)
            //{
            //    GameObject.Destroy(child.gameObject);
            //}

            __isInitialised = false;
        }

        public void Initialise(GameObject presenceObject)
        {
            // Parent object for any visualisation objects.
            // First we add an object for ourselves.

            if (__isInitialised)
                Deinitialise();

            //    PresenceObject = presenceObject;

            ParentObject = new GameObject();
            ParentObject.name = _name;
            ParentObject.transform.SetParent(presenceObject.transform, false);

            Debug.Log("Adding visualiser " + _name);



            //foreach (Transform child in ParentObject.transform)
            //{
            //    GameObject.Destroy(child.gameObject);
            //}

            GameObject n;

#if SERVER
            
                n = DebugObject.getNullObject(0.25f, 0.25f, 0.5f);
                n.transform.SetParent(ParentObject.transform, false);
                Head = n;
            
#endif

            n = DebugObject.getNullObject(0.25f);
            n.transform.SetParent(ParentObject.transform, false);
            HandLeft = n;

            n = DebugObject.getNullObject(0.25f);
            n.transform.SetParent(ParentObject.transform, false);
            HandRight = n;

            n = DebugObject.getNullObject(0.5f);
            n.transform.SetParent(ParentObject.transform, false);
            Body = n;


            Cloud = new ParticleCloud(5000, "CloudDyn", false);
            Cloud.CloudObject.transform.SetParent(ParentObject.transform, false);

            lastFrame = new UncompressedFrame();


            __isInitialised = true;
        }


        public void SetTransform(Vector3 pos, Vector3 scale, Quaternion rot)
        {

            if (ParentObject != null)
            {
                ParentObject.transform.localPosition = pos;
                ParentObject.transform.localScale = scale;
                ParentObject.transform.localRotation = rot;
            }
        }


        public Vector3 GetPosition()
        {

            if (ParentObject != null)
                return ParentObject.transform.localPosition;

            return Vector3.zero;
        }

        public Vector3 GetScale()
        {

            if (ParentObject != null)
                return ParentObject.transform.localScale;

            return Vector3.one;
        }

        public Quaternion GetRotation()
        {

            if (ParentObject != null)
                return ParentObject.transform.localRotation;

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

                //    Vector3 offset = new Vector3(0, Frame.SensorY, 0);\
                Vector3 offset = Vector3.zero;
                // no offset. we're using kinect height which means that whatever height it was at, joints will be correct.

                HandLeft.transform.localPosition = Frame.Joints[(int)NuiSkeletonPositionIndex.HandLeft] + offset;
                HandRight.transform.localPosition = Frame.Joints[(int)NuiSkeletonPositionIndex.HandRight] + offset;

                Body.transform.localPosition = Frame.UserPosition + offset;

#if SERVER
               
                    //  if (Frame.Tracked
                    Head.transform.localPosition = Frame.Joints[(int)NuiSkeletonPositionIndex.Head] + offset;
                    Head.transform.localRotation = Frame.HeadOrientation;
                
#endif

                // Check if frame is new.

                //Debug.Log(Frame.FrameNumber);


                if (lastFrame != Frame && IsDrawing)

                {

                    Vector3 last = lastFrame.Joints[(int)NuiSkeletonPositionIndex.HandLeft] + offset;
                    Vector3 current = Frame.Joints[(int)NuiSkeletonPositionIndex.HandLeft] + offset;

                    if (lastFrame.Tracked[(int)NuiSkeletonPositionIndex.HandLeft] &&
                        Frame.Tracked[(int)NuiSkeletonPositionIndex.HandLeft])
                    {

                        for (float i = 0; i < 1; i += 0.25f)
                        {
                            Cloud.Emit(Vector3.Lerp(last, current, i));
                        }

                    }

                    //HandLeftP = current;

                    last = lastFrame.Joints[(int)NuiSkeletonPositionIndex.HandRight] + offset;
                    current = Frame.Joints[(int)NuiSkeletonPositionIndex.HandRight] + offset;

                    if (lastFrame.Tracked[(int)NuiSkeletonPositionIndex.HandRight] &&
                        Frame.Tracked[(int)NuiSkeletonPositionIndex.HandRight])
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

    public class PointShaded : iVisualiser
    {
        string _name = "PointShaded";

        GameObject ParentObject;
        GameObject Head, Body, HandLeft, HandRight;
        ParticleCloud Cloud;
        bool __isInitialised = false;
        UncompressedFrame lastFrame;
        GameObject PLight;

        bool CloudVisible = false;

        public void SettingsToTask(StoryEngine.StoryTask task, string prefix)
        {
            task.SetIntValue(prefix + "_cloudvisible", CloudVisible?1:0);

        }
        public void SettingsFromTask(StoryEngine.StoryTask task, string prefix)
        {

            int v;

            if (        task.GetIntValue(prefix + "_cloudvisible", out v))
            {

                CloudVisible = (v == 1);

            }




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

            GameObject.Destroy(ParentObject);

            //foreach (Transform child in PresenceObject.transform)
            //{
            //    GameObject.Destroy(child.gameObject);
            //}

            __isInitialised = false;
        }

        public void Initialise(GameObject presenceObject)
        {
            // Parent object for any visualisation objects.

            if (__isInitialised)
                Deinitialise();

            //  PresenceObject = presenceObject;

            ParentObject = new GameObject();
            ParentObject.name = _name;
            ParentObject.transform.SetParent(presenceObject.transform, false);

            Debug.Log("Adding visualiser " + _name);

            //PresenceObject = presenceObject;

            //foreach (Transform child in PresenceObject.transform)
            //{
            //    GameObject.Destroy(child.gameObject);
            //}

            GameObject n;

#if SERVER
           
                n = DebugObject.getNullObject(0.1f, 0.1f, 0.2f);
                n.transform.SetParent(ParentObject.transform, false);
                Head = n;
            
#endif

            n = DebugObject.getNullObject(0.1f);
            n.transform.SetParent(ParentObject.transform, false);
            HandLeft = n;

            n = DebugObject.getNullObject(0.1f);
            n.transform.SetParent(ParentObject.transform, false);
            HandRight = n;

            n = DebugObject.getNullObject(0.2f);
            n.transform.SetParent(ParentObject.transform, false);
            Body = n;

            PLight = new GameObject("PLight");
            Light lightComp = PLight.AddComponent<Light>();


            //lightComp.color = new Color(137f / 256f, 223f / 256f, 249f / 256f);

            //lightComp.color= new Color32 (137,223,255,255);
            //lightComp.color= new Color32 (137,223,161,255);

            lightComp.color = new Color32(255, 255, 255, 255);

            lightComp.intensity = 0.15f;
            PLight.transform.localPosition = new Vector3(0, 0.5f, 0);


            //PLight = new Light();

            PLight.transform.SetParent(ParentObject.transform, false);
            //pLight.transform.localPosition = Vector3.zero;
            //pLight.type = LightType.Point;



            Cloud = new ParticleCloud(20000, "Cloud", true);
            Cloud.CloudObject.transform.SetParent(ParentObject.transform, false);

            lastFrame = new UncompressedFrame();

            __isInitialised = true;
        }


        public void SetTransform(Vector3 pos, Vector3 scale, Quaternion rot)
        {

            if (ParentObject != null)
            {
                ParentObject.transform.localPosition = pos;
                ParentObject.transform.localScale = scale;
                ParentObject.transform.localRotation = rot;
            }
        }


        public Vector3 GetPosition()
        {

            if (ParentObject != null)
                return ParentObject.transform.localPosition;

            return Vector3.zero;
        }


        public Vector3 GetScale()
        {

            if (ParentObject != null)
                return ParentObject.transform.localScale;

            return Vector3.one;
        }

        public Quaternion GetRotation()
        {

            if (ParentObject != null)
                return ParentObject.transform.localRotation;

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

                #if SERVER
              
                    Head.SetActive(Frame.UserTracked);
#endif

                Body.SetActive(Frame.UserTracked);
                HandLeft.SetActive(Frame.UserTracked);
                HandRight.SetActive(Frame.UserTracked);
                PLight.SetActive(Frame.UserTracked);

                Vector3 offset = new Vector3(0, Frame.SensorY, 0);

                HandLeft.transform.localPosition = Frame.Joints[(int)NuiSkeletonPositionIndex.HandLeft] + offset;
                HandRight.transform.localPosition = Frame.Joints[(int)NuiSkeletonPositionIndex.HandRight] + offset;

                Body.transform.localPosition = Frame.UserPosition + offset;
                PLight.transform.localPosition = new Vector3(Frame.UserPosition.x, 1, Frame.UserPosition.z);

                #if SERVER
               
                    Head.transform.localPosition = Frame.Joints[(int)NuiSkeletonPositionIndex.Head] + offset;
                    Head.transform.localRotation = Frame.HeadOrientation;
                
#endif

                // takes a kinect styled uint[] RawDepthMap
                // and plots the points into a Particle Cloud, with scale corrected if the frame was downsampled.

                int ParticleIndex = 0;

                int Width = Frame.Width;
                int Height = Frame.Height;
                int Scale = 640 / Width;

                Vector3 point;

                Vector3[] AllPoints = new Vector3[Frame.RawDepth.Length];

                // First calculate all the points.

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
                            point.y = -point.y + Frame.SensorY;

                            AllPoints[i] = point;

                        }

                    }

                }

                // Adjust point size for distance from camera. Currently it's squaring. Might want to go linear.

                Vector3 CameraPosition = SETTINGS.ActiveCamera.transform.position;
                float distance = Vector3.Distance(CameraPosition, Frame.UserPosition);
                float factor = (Mathf.Clamp(distance, 0.5f, 4f) - 0.5f) / 3.5f; //result is normalised to 0-1

                factor = Mathf.Pow((1f - factor), 2f);

                //     float size = 0.001f + factor * 0.005f;
                float size = 0.001f + factor * 0.0065f;
                Cloud.SetPointSize(size);

                // Now calculate normals, lighting and assign colours accordingly

                Color32 Color01 = new Color32(137, 223, 255, 255);
                Color32 Color02 = new Color32(137, 223, 161, 255);

                Gradient g;
                GradientColorKey[] gck;
                GradientAlphaKey[] gak;
                g = new Gradient();
                gck = new GradientColorKey[2];
                gck[0].color = Color01;
                gck[0].time = 0.0F;
                gck[1].color = Color02;
                gck[1].time = 1.0F;
                gak = new GradientAlphaKey[2];
                gak[0].alpha = 1.0F;
                gak[0].time = 0.0F;
                gak[1].alpha = 0.1F;
                gak[1].time = 1.0F;
                g.SetKeys(gck, gak);

                Vector3 light = new Vector3(0, -1, 1);
                light.Normalize();


                // Downsampling is handled in depthtransport.

                for (int y = 0; y < Height - 1; y++)
                {

                    Color32 ColorBase = g.Evaluate((float)y / Height);


                    for (int x = 0; x < Width - 1; x++)
                    {
                        int i = y * Width + x;

                        ushort userMap = (ushort)(Frame.RawDepth[i] & 7);


                        if (userMap != 0)
                        {


                            point = AllPoints[i];

                            Vector3 point2 = AllPoints[i + 1];
                            Vector3 point3 = AllPoints[i + Width];

                            // Test for drawing wireframe.

                            //Debug.DrawLine(point, point2, Color.grey);
                            //Debug.DrawLine(point2, point3, Color.grey);
                            //Debug.DrawLine(point3, point, Color.grey);

                            Vector3 V1 = point2 - point;
                            Vector3 v2 = point3 - point;
                            Vector3 normal = Vector3.Cross(V1, v2);
                            normal.Normalize();

                            //         float angle = Vector3.Angle(V1, v2);

                            float Cos = normal.x * light.x + normal.y * light.y + normal.z * light.z;

                            Color32 PointColour;

                            if (Cos > 0)
                            {

                                PointColour = (Color)ColorBase * (0.5f + 0.5f * Cos);

                            }
                            else
                            {
                                PointColour = (Color)ColorBase * (0.5f);
                            }

                            PointColour.a = ColorBase.a;

                            Cloud.allParticles[ParticleIndex].position = point;
                            Cloud.allParticles[ParticleIndex].startColor = PointColour;


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

                Cloud.ApplyParticles(CloudVisible  ? ParticleIndex : 0);

                // Check if frame is new.

                //if (lastFrame != Frame)

                //{


                //}
                lastFrame = Frame;


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

        public void SettingsToTask(StoryEngine.StoryTask task, string prefix)
        {
            task.SetFloatValue(prefix + "_cloudvisible", CloudVisible);

        }
        public void SettingsFromTask(StoryEngine.StoryTask task, string prefix)
        {

            task.GetFloatValue(prefix + "_cloudvisible", out CloudVisible);




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

            #if SERVER
          
                n = DebugObject.getNullObject(0.1f, 0.1f, 0.2f);
                n.transform.SetParent(PresenceObject.transform, false);
                Head = n;
            
#endif

            n = DebugObject.getNullObject(0.1f);
            n.transform.SetParent(PresenceObject.transform, false);
            HandLeft = n;

            n = DebugObject.getNullObject(0.1f);
            n.transform.SetParent(PresenceObject.transform, false);
            HandRight = n;

            n = DebugObject.getNullObject(0.2f);
            n.transform.SetParent(PresenceObject.transform, false);
            Body = n;

            PLight = new GameObject("PLight");
            Light lightComp = PLight.AddComponent<Light>();


            //lightComp.color = new Color(137f / 256f, 223f / 256f, 249f / 256f);

            //lightComp.color= new Color32 (137,223,255,255);
            //lightComp.color= new Color32 (137,223,161,255);

            lightComp.color = new Color32(255, 255, 255, 255);

            lightComp.intensity = 0.15f;
            PLight.transform.localPosition = new Vector3(0, 0.5f, 0);


            //PLight = new Light();

            PLight.transform.SetParent(PresenceObject.transform, false);
            //pLight.transform.localPosition = Vector3.zero;
            //pLight.type = LightType.Point;



            Cloud = new ParticleCloud(20000, "Cloud", true);
            Cloud.CloudObject.transform.SetParent(PresenceObject.transform, false);

            lastFrame = new UncompressedFrame();

            __isInitialised = true;
        }


        public void SetTransform(Vector3 pos, Vector3 scale, Quaternion rot)
        {

            if (PresenceObject != null)
            {
                PresenceObject.transform.localPosition = pos;
                PresenceObject.transform.localScale = scale;
                PresenceObject.transform.localRotation = rot;
            }
        }


        public Vector3 GetPosition()
        {

            if (PresenceObject != null)
                return PresenceObject.transform.localPosition;

            return Vector3.zero;
        }


        public Vector3 GetScale()
        {

            if (PresenceObject != null)
                return PresenceObject.transform.localScale;

            return Vector3.one;
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

                #if SERVER
              
                    Head.SetActive(Frame.UserTracked);
#endif

                Body.SetActive(Frame.UserTracked);
                HandLeft.SetActive(Frame.UserTracked);
                HandRight.SetActive(Frame.UserTracked);
                PLight.SetActive(Frame.UserTracked);

                Vector3 offset = new Vector3(0, Frame.SensorY, 0);

                HandLeft.transform.localPosition = Frame.Joints[(int)NuiSkeletonPositionIndex.HandLeft] + offset;
                HandRight.transform.localPosition = Frame.Joints[(int)NuiSkeletonPositionIndex.HandRight] + offset;

                Body.transform.localPosition = Frame.UserPosition + offset;
                PLight.transform.localPosition = new Vector3(Frame.UserPosition.x, 1, Frame.UserPosition.z);

                #if SERVER
               
                    Head.transform.localPosition = Frame.Joints[(int)NuiSkeletonPositionIndex.Head] + offset;
                    Head.transform.localRotation = Frame.HeadOrientation;
                
#endif

                // takes a kinect styled uint[] RawDepthMap
                // and plots the points into a Particle Cloud, with scale corrected if the frame was downsampled.



                int ParticleIndex = 0;

                int Width = Frame.Width;
                int Height = Frame.Height;
                int Scale = 640 / Width;

                Vector3 point;

                Color32 Color01 = new Color32(137, 223, 255, 255);
                Color32 Color02 = new Color32(137, 223, 161, 255);

                Gradient g;
                GradientColorKey[] gck;
                GradientAlphaKey[] gak;
                g = new Gradient();
                gck = new GradientColorKey[2];
                gck[0].color = Color01;
                gck[0].time = 0.0F;
                gck[1].color = Color02;
                gck[1].time = 1.0F;
                gak = new GradientAlphaKey[2];
                gak[0].alpha = 1.0F;
                gak[0].time = 0.0F;
                gak[1].alpha = 0.1F;
                gak[1].time = 1.0F;
                g.SetKeys(gck, gak);



                // Downsampling is handled in depthtransport.

                for (int y = 0; y < Height; y++)
                {

                    Color32 pointColor = g.Evaluate((float)y / Height);


                    for (int x = 0; x < Width; x++)
                    {
                        int i = y * Width + x;

                        ushort userMap = (ushort)(Frame.RawDepth[i] & 7);
                        ushort userDepth = (ushort)(Frame.RawDepth[i] >> 3);

                        if (userMap != 0)
                        {

                            point = depthToWorld(x * Scale, y * Scale, userDepth);
                            point.x = -point.x;
                            point.y = -point.y + Frame.SensorY;

                            Cloud.allParticles[ParticleIndex].position = point;

                            Cloud.allParticles[ParticleIndex].startColor = pointColor;


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

                Cloud.ApplyParticles(CloudVisible > 0 ? ParticleIndex : 0);




                lastFrame = Frame;


            }


        }




    }


    public class ShowMesh : iVisualiser
    {
        string _name = "ShowMesh";

        GameObject PresenceObject, MeshObject;
        GameObject Head, Body, HandLeft, HandRight;
        ParticleCloud Cloud;
        bool __isInitialised = false;
        UncompressedFrame lastFrame;
        GameObject PLight;
        Mesh mesh;

        public float CloudVisible = 0;

        public void SettingsToTask(StoryEngine.StoryTask task, string prefix)
        {
            task.SetFloatValue(prefix + "_cloudvisible", CloudVisible);

        }
        public void SettingsFromTask(StoryEngine.StoryTask task, string prefix)
        {

            task.GetFloatValue(prefix + "_cloudvisible", out CloudVisible);

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

#if SERVER
           
                n = DebugObject.getNullObject(0.1f, 0.1f, 0.2f);
                n.transform.SetParent(PresenceObject.transform, false);
                Head = n;
            
#endif

            n = DebugObject.getNullObject(0.1f);
            n.transform.SetParent(PresenceObject.transform, false);
            HandLeft = n;

            n = DebugObject.getNullObject(0.1f);
            n.transform.SetParent(PresenceObject.transform, false);
            HandRight = n;

            n = DebugObject.getNullObject(0.2f);
            n.transform.SetParent(PresenceObject.transform, false);
            Body = n;

            PLight = new GameObject("PLight");
            Light lightComp = PLight.AddComponent<Light>();


            //lightComp.color = new Color(137f / 256f, 223f / 256f, 249f / 256f);

            //lightComp.color= new Color32 (137,223,255,255);
            //lightComp.color= new Color32 (137,223,161,255);

            lightComp.color = new Color32(255, 255, 255, 255);

            lightComp.intensity = 0.15f;
            PLight.transform.localPosition = new Vector3(0, 0.5f, 0);


            //PLight = new Light();

            PLight.transform.SetParent(PresenceObject.transform, false);

            MeshObject = new GameObject("Meshobject");
            MeshObject.transform.SetParent(PresenceObject.transform, false);

            MeshObject.AddComponent<MeshFilter>();
            MeshObject.AddComponent<MeshRenderer>();

            mesh = new Mesh();
            MeshObject.GetComponent<MeshFilter>().mesh = mesh;


            MeshObject.GetComponent<Renderer>().material = Resources.Load("Default") as Material;



            lastFrame = new UncompressedFrame();

            __isInitialised = true;
        }


        public void SetTransform(Vector3 pos, Vector3 scale, Quaternion rot)
        {

            if (PresenceObject != null)
            {
                PresenceObject.transform.localPosition = pos;
                PresenceObject.transform.localScale = scale;
                PresenceObject.transform.localRotation = rot;
            }
        }


        public Vector3 GetPosition()
        {

            if (PresenceObject != null)
                return PresenceObject.transform.localPosition;

            return Vector3.zero;
        }


        public Vector3 GetScale()
        {

            if (PresenceObject != null)
                return PresenceObject.transform.localScale;

            return Vector3.one;
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

#if SERVER
              
                    Head.SetActive(Frame.UserTracked);
#endif

                Body.SetActive(Frame.UserTracked);
                HandLeft.SetActive(Frame.UserTracked);
                HandRight.SetActive(Frame.UserTracked);
                PLight.SetActive(Frame.UserTracked);

                Vector3 offset = new Vector3(0, Frame.SensorY, 0);

                HandLeft.transform.localPosition = Frame.Joints[(int)NuiSkeletonPositionIndex.HandLeft] + offset;
                HandRight.transform.localPosition = Frame.Joints[(int)NuiSkeletonPositionIndex.HandRight] + offset;

                Body.transform.localPosition = Frame.UserPosition + offset;
                PLight.transform.localPosition = new Vector3(Frame.UserPosition.x, 1, Frame.UserPosition.z);

#if SERVER
              
                    Head.transform.localPosition = Frame.Joints[(int)NuiSkeletonPositionIndex.Head] + offset;
                    Head.transform.localRotation = Frame.HeadOrientation;
                
#endif

                // takes a kinect styled uint[] RawDepthMap


                int Width = Frame.Width;
                int Height = Frame.Height;
                int Scale = 640 / Width;

                Vector3 point;

                Color32 Color01 = new Color32(137, 223, 255, 255);
                Color32 Color02 = new Color32(137, 223, 161, 255);

                Gradient g;
                GradientColorKey[] gck;
                GradientAlphaKey[] gak;
                g = new Gradient();
                gck = new GradientColorKey[2];
                gck[0].color = Color01;
                gck[0].time = 0.0F;
                gck[1].color = Color02;
                gck[1].time = 1.0F;
                gak = new GradientAlphaKey[2];
                gak[0].alpha = 1.0F;
                gak[0].time = 0.0F;
                gak[1].alpha = 0.1F;
                gak[1].time = 1.0F;
                g.SetKeys(gck, gak);



                // Downsampling is handled in depthtransport.

                // First calculate all points?

                /*
                 * 
                 * abc
                 * def
                 * ghi
                 * 
                */

                Vector3[] vertices = new Vector3[Height * Width];

                for (int y = 0; y < Height; y++)
                {

                    Color32 pointColor = g.Evaluate((float)y / Height);


                    for (int x = 0; x < Width; x++)
                    {
                        int i = y * Width + x;

                        ushort userMap = (ushort)(Frame.RawDepth[i] & 7);
                        ushort userDepth = (ushort)(Frame.RawDepth[i] >> 3);

                        if (userMap != 0)
                        {
                            point = depthToWorld(x * Scale, y * Scale, userDepth);
                            point.x = -point.x;
                            point.y = -point.y + Frame.SensorY;

                            vertices[i] = point;

                        }
                        else
                        {

                            vertices[i] = Vector3.zero;
                        }

                    }

                }

                int[] triangles = new int[(Height - 1) * (Width - 1) * 2 * 3];

                for (int y = 0; y < Height - 1; y++)
                {

                    for (int x = 0; x < Width - 1; x++)
                    {
                        int a = y * Width + x;
                        int b = a + 1;
                        int c = a + Width;
                        int d = a + 1 + Width;

                        int i = (y * (Width - 1) + x) * 6;

                        triangles[i + 0] = a;
                        triangles[i + 2] = b;
                        triangles[i + 1] = c;
                        triangles[i + 3] = c;
                        triangles[i + 5] = b;
                        triangles[i + 4] = d;


                    }

                }

                mesh.Clear();
                mesh.vertices = vertices;
                //mesh.triangles=triangles;
                mesh.SetTriangles(triangles, 0);


                mesh.RecalculateNormals();


                // Cloud.ApplyParticles(CloudVisible>0 ? ParticleIndex : 0);

                // Check if frame is new.

                //if (lastFrame != Frame)

                //{


                //}
                lastFrame = Frame;


            }


        }




    }



}