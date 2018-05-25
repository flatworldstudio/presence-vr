

using System.Collections.Generic;
using UnityEngine;




namespace PresenceEngine
{




    public interface iTransCoder
    {

        void SetBufferFile(FileformatBase TargetFile);

        FileformatBase GetBufferFile();

        FileformatBase CreateBufferFile(string name);

        string GetName();

        bool Encode(UncompressedFrame Uframe, StoryEngine.StoryTask task, string prefix, bool recording = false);

        bool Decode(ref UncompressedFrame Uframe, StoryEngine.StoryTask task, string prefix, bool recording = false);

        bool PlayFrame(int frameNumber, ref UncompressedFrame Uframe);
    }

    [System.Serializable]
    public class FileformatBase
    {
        public List<FrameBase> Frames;

        public string TransCoderName, Name;
        public int FirstFrame = 99999999, LastFrame = -1;

        public float[] Transform;


        public FileformatBase()
        {
            Frames = new List<FrameBase>();
            //Transform = new float[3 + 4 + 3];
            //SetTransform(Vector3.zero, Vector3.one, Quaternion.identity);// set default

        }


      

        //public static   FileformatBase FindBufferFileInScene(string fileName)
        //{

        //    Debug.Log("Trying to find buffer " + fileName);

        //    foreach (KeyValuePair<string, Presence> entry in SETTINGS.Presences)
        //    {

        //        // do something with entry.Value or entry.Key

        //        if (entry.Value != null && entry.Value.DepthTransport != null && entry.Value.DepthTransport.TransCoder != null)
        //        {
        //            FileformatBase bufferFile = entry.Value.DepthTransport.TransCoder.GetBufferFile();

        //            if (bufferFile != null && bufferFile.Name == fileName)
        //            {
        //                Debug.Log("found buffer file");
        //                return bufferFile;
        //            }

        //        }

        //    }


        //    Debug.Log("Didn't find buffer file");

        //    return null;
        //}

        //public static      FileformatBase GetFileBuffer(string filePath)
        //{

        //    FileformatBase Buffered = FindBufferFileInScene(filePath);

        //    if (Buffered == null)
        //    {
        //        // try loading it from disk
        //        //Debug.Log("loading from disk");

        //        Buffered = IO.LoadFromFile(filePath);  // returns null and logs error on fail.

        //        //if (Buffered == null)
        //        //{
        //        //    Log.Error("loading file failed");

        //        //    return null;
        //        //}
        //    }


        //    return Buffered;


        //}



        //public void SetTransform(Vector3 position, Vector3 scale, Quaternion rotation)
        //{

        //    // Pass a transform and store it in a serialisable format.

        //    Transform[0] = position.x;
        //    Transform[1] = position.y;
        //    Transform[2] = position.z;
        //    Transform[3] = scale.x;
        //    Transform[4] = scale.y;
        //    Transform[5] = scale.z;
        //    Transform[6] = rotation.x;
        //    Transform[7] = rotation.y;
        //    Transform[8] = rotation.z;
        //    Transform[9] = rotation.w;

        //}

        //public void GetTransform(out Vector3 position, out Vector3 scale, out Quaternion rotation)
        //{

        //    // Get a transform and return it as pos, rot, scale.

        //    position = new Vector3(Transform[0], Transform[1], Transform[2]);
        //    scale = new Vector3(Transform[3], Transform[4], Transform[5]);
        //    rotation = new Quaternion(Transform[6], Transform[7], Transform[8], Transform[9]);


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

        public string GetName()
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

        public bool Encode(UncompressedFrame Uframe, StoryEngine.StoryTask task, string prefix, bool recording = false)
        {

            task.SetVector3ArrayValue(prefix + "_skeleton", Uframe.Joints);
            task.SetBoolArrayValue(prefix + "_tracked", Uframe.Tracked);

            task.SetVector3Value(prefix + "_body", Uframe.Body);
            task.SetIntValue(prefix + "_frame", Uframe.FrameNumber);

            if (recording)
                RecordFrame(Uframe);


            return true;
        }

        public bool Decode(ref UncompressedFrame Uframe, StoryEngine.StoryTask task, string prefix, bool recording = false)
        {

            task.GetVector3ArrayValue(prefix + "_skeleton", out Uframe.Joints);
            task.GetBoolArrayValue(prefix + "_tracked", out Uframe.Tracked);

            task.GetVector3Value(prefix + "_body", out Uframe.Body);
            if (!task.GetIntValue(prefix + "_frame", out Uframe.FrameNumber))
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

            storeFrame.Body = new Point(Uframe.Body);

            _bufferFile.Frames.Add(storeFrame);
            _bufferFile.FirstFrame = Mathf.Min(_bufferFile.FirstFrame, storeFrame.FrameNumber);
            _bufferFile.LastFrame = Mathf.Max(_bufferFile.LastFrame, storeFrame.FrameNumber);


        }

        public bool PlayFrame(int frameNumber, ref UncompressedFrame Uframe)
        {


            if (_bufferFile != null)
            {

                int index = frameNumber - _bufferFile.FirstFrame;

                //   Debug.Log("Getting frame "+frameNumber + " of "+ _bufferFile.Frames.Count)

                if (index < _bufferFile.Frames.Count && index >= 0)
                {
                    SkeletonOnlyFrame storeFrame = (SkeletonOnlyFrame)_bufferFile.Frames[index];

                    for (int p = 0; p < Uframe.Joints.Length; p++)
                    {
                        Uframe.Joints[p] = storeFrame.Points[p].ToVector3();
                        Uframe.Tracked[p] = storeFrame.Tracked[p];

                    }

                    Uframe.Body = storeFrame.Body.ToVector3();
                    Uframe.FrameNumber = frameNumber;

                    return true;
                }



            }

            return false;


        }


    }
















}