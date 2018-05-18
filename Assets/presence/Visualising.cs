
using UnityEngine;


namespace PresenceEngine
{

    public interface iVisualiser
    {
        bool IsInitialised();

        void Initialise(GameObject presenceObject);

        void Update(UncompressedFrame Frame);






    }


    public class ShowSkeleton : iVisualiser
    {
        //        ParticleCloud cloud;

        GameObject PresenceObject;
        GameObject Head, Body, HandLeft, HandRight;
        ParticleCloud Cloud;
        bool __isInitialised = false;

        public bool IsInitialised()
        {
            return __isInitialised;
        }

        public void Initialise(GameObject presenceObject)
        {
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

            Cloud = new ParticleCloud(1000, "CloudDyn", false);



            __isInitialised = true;
        }


        Vector3 HandLeftP, HandRightP;


        public void Update(UncompressedFrame Frame)
        {
            if (__isInitialised)
            {
                if (Frame != null && Frame.Joints != null)
                {
                  

                    HandLeft.transform.position = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft];
                    HandRight.transform.position = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight];

                    Body.transform.position = Frame.Body;

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                      //  if (Frame.Tracked
                        Head.transform.position = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head];
                        Head.transform.rotation = Frame.HeadOrientation;
                    }


                    Vector3 last = HandLeftP;
                    Vector3 current = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft];

                    if (Frame.Tracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft])
                    {

                        for (float i = 0; i < 1; i += 0.5f)
                        {
                            Cloud.Emit(Vector3.Lerp(last, current, i));
                        }
                    }
                    HandLeftP = current;

                    last = HandRightP;
                    current = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight];

                    if (Frame.Tracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight])
                    {
                        for (float i = 0; i < 1; i += 0.5f)
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