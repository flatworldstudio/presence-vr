
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
        GameObject Head,Body,HandLeft,HandRight;
        bool __isInitialised = false;

        public bool IsInitialised()
        {
            return __isInitialised;
        }

        public  void Initialise(GameObject presenceObject)
        {
            PresenceObject = presenceObject;

            foreach (Transform child in PresenceObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            GameObject n;
                      
            n = DebugObject.getNullObject(0.25f, 0.25f, 0.5f);
            n.transform.SetParent(PresenceObject.transform, false);
            Head = n;

            n = DebugObject.getNullObject(0.25f);
            n.transform.SetParent(PresenceObject.transform, false);
            HandLeft = n;

            n = DebugObject.getNullObject(0.25f);
            n.transform.SetParent(PresenceObject.transform, false);
            HandRight = n;

            n = DebugObject.getNullObject(0.5f);
            n.transform.SetParent(PresenceObject.transform, false);
            Body = n;

            __isInitialised = true;
        }

      

        public void Update(UncompressedFrame Frame)
        {
            if (__isInitialised)
            {
                if (Frame != null && Frame.Joints != null)
                {
                    Head.transform.position = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head];

                    HandLeft.transform.position = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft];
                    HandRight.transform.position = Frame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight];

                    Body.transform.position = Frame.Body;

                    Head.transform.rotation = Frame.HeadOrientation;


                    //  Debug.Log(Body.transform.position = Frame.Body);
                }
            }



        }

       


    }





}