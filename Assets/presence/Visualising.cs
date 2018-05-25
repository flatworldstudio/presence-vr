
using UnityEngine;


namespace PresenceEngine
{

    public interface iVisualiser
    {
        bool IsInitialised();

        void Initialise(GameObject presenceObject);

        void SetTransform (Vector3 pos,Quaternion rot);

        void Update(UncompressedFrame Frame);

        string GetName();

        Vector3 GetPosition();
        Quaternion GetRotation();


    }


    public class ShowSkeleton : iVisualiser
    {
        //        ParticleCloud cloud;

        GameObject PresenceObject;
        GameObject Head, Body, HandLeft, HandRight;
        ParticleCloud Cloud;
        bool __isInitialised = false;

        string _name = "ShowSkeleton";
        int lastFrame;

        public string GetName(){
           
            return _name;
                     
        }


        public bool IsInitialised()
        {
            return __isInitialised;
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
            Cloud.CloudObject.transform.SetParent(PresenceObject.transform,false);

            lastFrame=-1;
            __isInitialised = true;
        }


      public void SetTransform (Vector3 pos,Quaternion rot){

            if (PresenceObject!=null){
            PresenceObject.transform.localPosition=pos;
            //PresenceObject.transform.localScale=scale;
            PresenceObject.transform.localRotation=rot;
            }
        }


      public  Vector3 GetPosition(){
            
            if (PresenceObject!=null)
                return PresenceObject.transform.localPosition;
            
            return Vector3.zero;
        }

        public  Quaternion GetRotation(){

            if (PresenceObject!=null)
                return PresenceObject.transform.localRotation;

            return Quaternion.identity;
        }
             

        Vector3 HandLeftP, HandRightP;


        public void Update(UncompressedFrame Frame)
        {
            if (__isInitialised)
            {

                // Check validity.
                if (!(Frame != null && Frame.Joints != null && Frame.Tracked !=null))
                    return;

                // Check if frame is new.

                //Debug.Log(Frame.FrameNumber);


                if (Frame.FrameNumber>lastFrame)
                {
                 
                    lastFrame=Frame.FrameNumber;

                    HandLeft.transform.localPosition = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft];
                    HandRight.transform.localPosition = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight];

                    Body.transform.localPosition = Frame.Body;

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                      //  if (Frame.Tracked
                        Head.transform.localPosition = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head];
                        Head.transform.localRotation = Frame.HeadOrientation;
                    }


                    Vector3 last = HandLeftP;
                    Vector3 current = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft];

                    if (Frame.Tracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft])
                    {

                        for (float i = 0; i < 1; i += 0.25f)
                        {
                            Cloud.Emit(Vector3.Lerp(last, current, i));
                        }
                    }
                    HandLeftP = current;

                    last = HandRightP;
                    current = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight];

                    if (Frame.Tracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight])
                    {
                        for (float i = 0; i < 1; i += 0.25f)
                        {
                            Cloud.Emit(Vector3.Lerp(last, current, i));
                        }
                    }
                    HandRightP = current;


                    //     Cloud.Emit(Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight]);


                    //  Debug.Log(Body.transform.position = Frame.Body);
                }
            }



        }




    }





}