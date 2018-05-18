

using System.Collections.Generic;
using UnityEngine;




namespace PresenceEngine
{




    public interface iTransCoder
    {

        void SetBufferFile(FileformatBase TargetFile);

        FileformatBase GetBufferFile();

        FileformatBase CreateBufferFile(string name);

        string Name();

        bool Encode(UncompressedFrame Uframe, StoryEngine.StoryTask task, bool recording = false);

        bool Decode(ref UncompressedFrame Uframe, StoryEngine.StoryTask task, bool recording = false);

        bool PlayFrame(int frameNumber, ref UncompressedFrame Uframe);
    }

    [System.Serializable]
    public class FileformatBase
    {
        public List<FrameBase> Frames;
        public string TransCoderName, Name;
        public int FirstFrame=99999999, LastFrame=-1;

        public FileformatBase()
        {
            Frames = new List<FrameBase>();

        }

        //public void SetFrameRange()
        //{
        //    if (Frames != null && Frames.Count > 0)
        //    {

        //        FirstFrame = Frames[0].FrameNumber;
        //        LastFrame = Frames[Frames.Count - 1].FrameNumber;

        //    }

        //}


    }

    [System.Serializable]
    public class FrameBase
    {
        public int FrameNumber;

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
    public class SkeletonOnlyFrame : FrameBase
    {

        public Point[] Points;
        public bool[] Tracked;
        public Point Body;

        public SkeletonOnlyFrame()
        {
            Points = new Point[(int)KinectWrapper.NuiSkeletonPositionIndex.Count];
            Tracked = new bool[Points.Length];
        }


    }

    [System.Serializable]
    public class Point
    {
        public float x, y, z;

        public Point(Vector3 vector)
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
        public int FrameNumber;

        public UncompressedFrame()
        {

            Joints = new Vector3[(int)KinectWrapper.NuiSkeletonPositionIndex.Count];

            Tracked = new bool[(int)KinectWrapper.NuiSkeletonPositionIndex.Count];


        }
        public void Clear()
        {

            for (int j = 0; j < Joints.Length; j++)
            {
                Joints[j] = Vector3.zero;
                Tracked[j] = false;

            }


        }
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
        FileformatBase _bufferFile;


        public SkeletonOnly()
        {

        }

        public string Name()
        {
            return _name;
        }

        public void SetBufferFile(FileformatBase target)
        {
            _bufferFile = target;
        }

        public FileformatBase GetBufferFile()
        {
            return _bufferFile;
        }

        public FileformatBase CreateBufferFile(string name)
        {
            _bufferFile = new FileformatBase
            {
                Name = name
            };

            Debug.Log("Created buffer file " + _bufferFile.Name);

            return _bufferFile;
        }

        public bool Encode(UncompressedFrame Uframe, StoryEngine.StoryTask task, bool recording = false)
        {

            // Streaming
            task.SetVector3ArrayValue("skeleton", Uframe.Joints);
            task.SetBoolArrayValue("tracked", Uframe.Tracked);

            //task.SetVector3Value("head", Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head]);
            //task.SetVector3Value("lefthand", Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft]);
            //task.SetVector3Value("righthand", Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight]);

            task.SetVector3Value("body", Uframe.Body);
            task.SetIntValue("frame", Uframe.FrameNumber);

            //    Debug.Log(Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head]);

            if (recording)
                RecordFrame(Uframe);


            return true;
        }

        public bool Decode(ref UncompressedFrame Uframe, StoryEngine.StoryTask task, bool recording = false)
        {

            //    Uframe = new UncompressedFrame();
            task.GetVector3ArrayValue("skeleton",out Uframe.Joints);
            task.GetBoolArrayValue("tracked",out Uframe.Tracked);
            //task.GetVector3Value("head", out Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head]);
            //task.GetVector3Value("lefthand", out Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft]);
            //task.GetVector3Value("righthand", out Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight]);

            task.GetVector3Value("body", out Uframe.Body);

            //     Debug.Log(Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head]);

            if (!task.GetIntValue("frame", out Uframe.FrameNumber))
                return false; // Simple check: if one of the values isn't present something's wrong.


            if (recording)
                RecordFrame(Uframe);

            return true;
        }

        void RecordFrame(UncompressedFrame Uframe)
        {

            SkeletonOnlyFrame storeFrame = new SkeletonOnlyFrame();
            storeFrame.FrameNumber = Uframe.FrameNumber;

            for (int p = 0; p < Uframe.Joints.Length; p++)
            {
                storeFrame.Points[p] = new Point(Uframe.Joints[p]);
                storeFrame.Tracked[p] = Uframe.Tracked[p];
            }


            //storeFrame.Points[0] = new Point(Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head]);
            //storeFrame.Points[1] = new Point(Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft]);
            //storeFrame.Points[2] = new Point(Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight]);


            storeFrame.Body = new Point(Uframe.Body);

            _bufferFile.Frames.Add(storeFrame);
            _bufferFile.FirstFrame = Mathf.Min(_bufferFile.FirstFrame, storeFrame.FrameNumber);
            _bufferFile.LastFrame = Mathf.Max(_bufferFile.LastFrame, storeFrame.FrameNumber);


        }

        public bool PlayFrame(int frameNumber, ref UncompressedFrame Uframe)
        {


            if (_bufferFile != null)
            {

                int index = frameNumber- _bufferFile.FirstFrame;
             //   Debug.Log("Getting frame "+frameNumber + " of "+ _bufferFile.Frames.Count)
                if (index < _bufferFile.Frames.Count && index >=0)
                {
                    SkeletonOnlyFrame storeFrame = (SkeletonOnlyFrame)_bufferFile.Frames[index];

                    for (int p = 0; p < Uframe.Joints.Length; p++)
                    {
                        Uframe.Joints[p] = storeFrame.Points[p].ToVector3();
                        Uframe.Tracked[p] = storeFrame.Tracked[p];

                    }




                    //Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head] = storeFrame.Points[0].ToVector3();
                    //Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft] = storeFrame.Points[1].ToVector3();
                    //Uframe.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight] = storeFrame.Points[2].ToVector3();

                    Uframe.Body = storeFrame.Body.ToVector3();

                    return true;
                }



            }

            return false;


        }


    }
















}