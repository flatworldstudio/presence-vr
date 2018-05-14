

using System.Collections.Generic;
using UnityEngine;




namespace Presence
{
   



    public interface iTransCoder
    {

        void SetTargetFile(FileformatBase TargetFile);

        FileformatBase GetTargetFile();

         FileformatBase CreateFile();
        
        string Name();

        bool PutFrame(int FrameNumber, UncompressedFrame Uframe, StoryEngine.StoryTask task, bool recording = false);

        bool GetFrame(int FrameNumber, out UncompressedFrame Uframe, StoryEngine.StoryTask task);

    }

    [System.Serializable]
    public class FileformatBase
    {
        public List<FrameBase> Frames;
        public string TransCoderName;

        public FileformatBase()
        {
            Frames = new List<FrameBase>();
           
        }


    }

    [System.Serializable]
    public class FrameBase
    {
            

    }

    //[System.Serializable]

    //public class SkeletonOnlyFile:FileformatBase
    //{

    //    public SkeletonOnlyFile()
    //    {
    //        Frames = new List<FrameBase>();
    //        Frames.Add(new SkeletonOnlyFrame());
    //    }

    //}
    [System.Serializable]
    public class SkeletonOnlyFrame:FrameBase
    {
        public int FrameNumber;
        public Point[] Points;

       public SkeletonOnlyFrame()
        {
            Points = new Point[4];
        }
             

    }

    [System.Serializable]
    public class Point
    {
        public float x, y, z;

     public Point (Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;

        }
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
            
        }

    }


    public class UncompressedFrame
    {

        public Vector3[] Joints;
        public Vector3 Body;
        public bool[] Tracked;
        public ushort[] RawDepth;
        public Quaternion HeadOrientation;

        public int Width
        {
            get
            {
                switch (RawDepth.Length)
                {
                    case 640 * 480:
                        return 640;
                    case 320 * 240:
                        return 320;
                    case 160 * 120:
                        return 160;
                    default:
                        return 0;
                }

            }
            set
            {
                Debug.LogError("Can't set width.");
            }
        }
        public int Height
        {
            get
            {
                switch (RawDepth.Length)
                {
                    case 640 * 480:
                        return 480;
                    case 320 * 240:
                        return 240;
                    case 160 * 120:
                        return 120;
                    default:
                        return 0;
                }

            }
            set
            {
                Debug.LogError("Can't set height.");
            }
        }

    }




    // TRANSCODERS AND FILEFORMATS


    public class SkeletonOnly : iTransCoder
    {

         string _name = "SkeletonOnly";
        FileformatBase _targetFile;


        public SkeletonOnly()
        {

        }

        public string Name()
        {
            return _name;
        }

       public void SetTargetFile (FileformatBase target)
        {
            _targetFile = target;
        }

        public FileformatBase GetTargetFile()
        {
            return _targetFile;
        }

        public FileformatBase CreateFile()
        {
            _targetFile = new FileformatBase();
            return _targetFile;
        }

        public bool PutFrame(int FrameNumber, UncompressedFrame Uframe, StoryEngine.StoryTask task,bool recording=false)
        {

            // Streaming

            task.setVector3Value("head", Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head]);
            task.setVector3Value("lefthand", Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft]);
            task.setVector3Value("righthand", Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight]);
            task.setVector3Value("body", Uframe.Body);
            
            if (recording)
            {

                SkeletonOnlyFrame storeFrame = new SkeletonOnlyFrame();
                storeFrame.FrameNumber = FrameNumber;
                storeFrame.Points[0] = new Point( Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head]);
                storeFrame.Points[1] = new Point(Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft]);
                storeFrame.Points[2] = new Point(Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight]);
                storeFrame.Points[3] = new Point(Uframe.Body);

                _targetFile.Frames.Add(storeFrame);


            }


            return true;
        }

        public bool GetFrame(int FrameNumber, out UncompressedFrame Uframe, StoryEngine.StoryTask task)
        {

            Uframe = new UncompressedFrame();
            
            task.getVector3Value("head", out Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head]);
            task.getVector3Value("lefthand", out Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft]);
            task.getVector3Value("righthand", out Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight]);
            task.getVector3Value("body", out Uframe.Body);

            return true;
        }


    }
















}