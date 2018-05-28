

using System.Collections.Generic;
using UnityEngine;
using System.Text;




namespace PresenceEngine
{




    public interface iTransCoder
    {

        void SetBufferFile(FileformatBase TargetFile);

        FileformatBase GetBufferFile();

        FileformatBase CreateBufferFile(string name);

        string GetName();

        bool Encode(UncompressedFrame Uframe, StoryEngine.StoryTask task, string prefix, bool recording = false);

        bool Decode(out UncompressedFrame Uframe, StoryEngine.StoryTask task, string prefix, bool recording = false);

        bool PlayFrame(int frameNumber, out UncompressedFrame Uframe);
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
    public class SkeletonAndDepthFrame : FrameBase
    {

        public Point[] Points;
        public bool[] Tracked;
        public Point Body;

        public byte[] Data;
        public int Min, Max;
        public int DepthSampling;

        public SkeletonAndDepthFrame()
        {
            Points = new Point[(int)KinectWrapper.NuiSkeletonPositionIndex.Count];
            Tracked = new bool[Points.Length];


        }



    }


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

            RawDepth = new ushort[160 * 120];

        }
        public void Clear()
        {

            for (int j = 0; j < Joints.Length; j++)
            {
                Joints[j] = Vector3.zero;
                Tracked[j] = false;

            }


        }

        public int DepthSampling
        {
            get
            {
                switch (RawDepth.Length)
                {
                    case 640 * 480:
                        return 1;
                    case 320 * 240:
                        return 2;
                    case 160 * 120:
                        return 4;
                    default:
                        return 0;
                }

            }
            set
            {

                switch (value)
                {
                    case 1:
                        RawDepth = new ushort[640 * 480];
                        break;
                    case 2:
                        RawDepth = new ushort[320 * 240];
                        break;
                    case 4:
                        RawDepth = new ushort[160 * 120];
                        break;
                    default:
                        Debug.LogError("Invalid depthsampling.");
                        break;
                }


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
                Name = name,
                TransCoderName = _name
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

        public bool Decode(out UncompressedFrame Uframe, StoryEngine.StoryTask task, string prefix, bool recording = false)
        {

            Uframe = new UncompressedFrame();

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

        public bool PlayFrame(int frameNumber, out UncompressedFrame Uframe)
        {

            Uframe = new UncompressedFrame();

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


    // ****************************************

    public class SkeletonAndDepth : iTransCoder
    {

        string _name = "SkeletonAndDepth";

        FileformatBase _bufferFile;


        public SkeletonAndDepth()
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
                Name = name,
                TransCoderName = _name
            };

            Debug.Log("Created buffer file " + _bufferFile.Name);

            return _bufferFile;
        }




        public bool Encode(UncompressedFrame Uframe, StoryEngine.StoryTask task, string prefix, bool recording = false)
        {

            // Write info to task.
            int Min, Max, DepthSampling;
            byte[] Data;

            EncodeDepth(Uframe, out Data, out DepthSampling, out Min, out Max);

                task.SetStringValue("debug", "" +Mathf.Round(Data.Length/1024f) +" ");


         //   task.SetStringValue("debug", "" + debugString);

            task.SetVector3ArrayValue(prefix + "_skeleton", Uframe.Joints);
            task.SetBoolArrayValue(prefix + "_tracked", Uframe.Tracked);

            task.SetVector3Value(prefix + "_body", Uframe.Body);
            task.SetIntValue(prefix + "_frame", Uframe.FrameNumber);

            if (Data.Length > 20000)
            {
                Debug.LogWarning("message size over 20k");


            }
            else
            {

                task.SetIntValue(prefix + "_sampling", DepthSampling);
                task.SetByteValue(prefix + "_data", Data);
                task.SetIntValue(prefix + "_min", Min);
                task.SetIntValue(prefix + "_max", Max);
            }
 

            if (recording)
            {

                RecordFrame(Uframe, Data, DepthSampling, Min, Max);

            }

            return true;
        }


        public bool Decode(out UncompressedFrame Uframe, StoryEngine.StoryTask task, string prefix, bool recording = false)
        {

            Uframe = new UncompressedFrame();

            task.GetVector3ArrayValue(prefix + "_skeleton", out Uframe.Joints);
            task.GetBoolArrayValue(prefix + "_tracked", out Uframe.Tracked);
            task.GetVector3Value(prefix + "_body", out Uframe.Body);


            byte[] Data;

            if (!task.GetByteValue(prefix + "_data", out Data))
                return false;


            int Sampling;
            task.GetIntValue(prefix + "_sampling", out Sampling);
            Uframe.DepthSampling = Sampling;

            int Min, Max;
            task.GetIntValue(prefix + "_min", out Min);
            task.GetIntValue(prefix + "_max", out Max);

            DecodeDepth(Uframe, Data, Sampling, Min, Max);

            if (recording)
            {

                RecordFrame(Uframe, Data, Sampling, Min, Max);

            }


            return true;
        }

        string debugString;

        void EncodeDepth(UncompressedFrame Uframe, out byte[] Data, out int Sampling, out int Min, out int Max)
        {

            // Encode depth

            // First go over all data to get min and max values and find zeroed blocks.

            Sampling = Uframe.DepthSampling;

            int BlockSize = 8;

            int Rows = Uframe.Height / BlockSize;
            int Columns = Uframe.Width / BlockSize;
            int NumberOfBlocks = Rows * Columns;
            Block[] Blocks = new Block[NumberOfBlocks];

            Min = 999999;
            Max = -999999;

            int ZeroedBlocks = 0;
            int FullBlocks = 0;
            int SingleByte = 0;

            // Go over blocks.

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    // Scan block.

                    int BlockIndex = r * Columns + c;
                    Block block = new Block(BlockSize);

                    for (int y = 0; y < BlockSize; y++)
                    {

                        int RawIndexBase = (r * BlockSize + y) * Uframe.Width + (c * BlockSize);
                        int BlockIndexBase = y * BlockSize;

                        for (int x = 0; x < BlockSize; x++)
                        {

                            // Go through data, copy depth and user data into block, establish if block is zeroed.

                            block.userMap[BlockIndexBase + x] = (ushort)(Uframe.RawDepth[RawIndexBase + x] & 7);
                            block.depthMap[BlockIndexBase + x] = (ushort)(Uframe.RawDepth[RawIndexBase + x] >> 3);

                            if (block.userMap[BlockIndexBase + x] != 0)
                            {
                                block.isZero = false;
                                block.Min = Mathf.Min(block.Min, block.depthMap[BlockIndexBase + x]);
                                block.Max = Mathf.Max(block.Max, block.depthMap[BlockIndexBase + x]);

                            }
                            else
                            {
                                block.isFull = false;
                            }

                        }
                    }

                    if (block.isZero)
                        ZeroedBlocks++;

                    if (block.isFull)
                    {
                        if (block.Max - block.Min < 16)
                            SingleByte++;

                        FullBlocks++;

                    }

                    Min = Mathf.Min(Min, block.Min);
                    Max = Mathf.Max(Max, block.Max);


                    Blocks[BlockIndex] = block;

                }
            }

            // Define data size. First a series of bytes to say zero or non zero. (can be /8)

            int NonZeroBlocks = NumberOfBlocks - ZeroedBlocks;
            int DepthDataIndex = NumberOfBlocks;
            //int ZeroDataIndex=1;
            int BlockDataSize = BlockSize * BlockSize;
            Data = new byte[DepthDataIndex + NonZeroBlocks * BlockSize * BlockSize];

            //Data[0]=(byte)Uframe.DepthSampling;


            // Now go over blocks again and write data.

            int Span = Max - Min;

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    int BlockIndex = r * Columns + c;

                    if (Blocks[BlockIndex].isZero)
                    {

                        Data[BlockIndex] = 0;

                    }
                    else
                    {

                        Data[BlockIndex] = 1;
                        Block block = Blocks[BlockIndex];

                        for (int y = 0; y < BlockSize; y++)
                        {
                            for (int x = 0; x < BlockSize; x++)
                            {
                                int index = y * BlockSize + x;
                                
                                Data[DepthDataIndex + index] =
                                    block.userMap[index] != 0 ?
                                    (byte)(((block.depthMap[index] - Min) * 254) / Span + 1) : (byte) 0;
                               

                            }
                        }

                        DepthDataIndex += BlockDataSize;
                    }

                }

            }


            debugString = "" + FullBlocks + " " + SingleByte;


        }






        void DecodeDepth(UncompressedFrame Uframe, byte[] Data, int Sampling, int Min, int Max)
        {

            int BlockSize = 8;
            int Rows, Columns;
            
            Uframe.DepthSampling = Sampling; // also creates buffer!

            Columns = (640 / Sampling) / BlockSize;
            Rows = (480 / Sampling) / BlockSize;

            int Span = Max - Min;

            int NumberOfBlocks = Rows * Columns;
            int DepthDataIndex = NumberOfBlocks;
            int BlockDataSize = BlockSize * BlockSize;
      int      BlockDataIndex = 0;

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {

                    // Populate block

                    int BlockIndex = r * Columns + c;

                    byte isZero = Data[BlockIndex];


                    for (int y = 0; y < BlockSize; y++)
                    {
                        int RawIndexBase = (r * BlockSize + y) * Uframe.Width + (c * BlockSize);
                        int DataIndexBase = BlockDataIndex + y * BlockSize;

                        for (int x = 0; x< BlockSize; x++)
                        {
                            
                            if (isZero == 0)
                            {
                                Uframe.RawDepth[RawIndexBase + x] = 0;
                            }
                            else
                            {
                                int depthMap = Data[DepthDataIndex + DataIndexBase + x];

                                int userMap = (depthMap == 0  ? 0 : 1);

                                Uframe.RawDepth[RawIndexBase + x] = (ushort)(((depthMap*Span)/254+Min) << 3 | userMap);

                            }
                
                            
                        }

                    }

                    if (isZero != 0)
                    {

                        BlockDataIndex += BlockDataSize;
                        //if (DepthDataIndex+BlockDataIndex > Data.Length - BlockDataSize)
                        //{
                        //    BlockDataIndex = 0;

                        //}
                    }
                    
                }

            }






        }


        //Decode



        void RecordFrame(UncompressedFrame Uframe, byte[] Data, int Sampling, int Min, int Max)
        {

            SkeletonAndDepthFrame storeFrame = new SkeletonAndDepthFrame();

            storeFrame.FrameNumber = Uframe.FrameNumber;

            for (int p = 0; p < Uframe.Joints.Length; p++)
            {
                storeFrame.Points[p] = new Point(Uframe.Joints[p]);
                storeFrame.Tracked[p] = Uframe.Tracked[p];
            }

            storeFrame.Body = new Point(Uframe.Body);
            storeFrame.Data = Data;
            storeFrame.DepthSampling = Sampling;
            storeFrame.Min = Min;
            storeFrame.Max = Max;

            _bufferFile.Frames.Add(storeFrame);
            _bufferFile.FirstFrame = Mathf.Min(_bufferFile.FirstFrame, storeFrame.FrameNumber);
            _bufferFile.LastFrame = Mathf.Max(_bufferFile.LastFrame, storeFrame.FrameNumber);

        }

        public bool PlayFrame(int frameNumber, out UncompressedFrame Uframe)
        {

            Uframe = new UncompressedFrame();

            if (_bufferFile != null)
            {

                int index = frameNumber - _bufferFile.FirstFrame;

                //   Debug.Log("Getting frame "+frameNumber + " of "+ _bufferFile.Frames.Count)

                if (index < _bufferFile.Frames.Count && index >= 0)
                {
                    SkeletonAndDepthFrame storeFrame = (SkeletonAndDepthFrame)_bufferFile.Frames[index];

                    for (int p = 0; p < Uframe.Joints.Length; p++)
                    {
                        Uframe.Joints[p] = storeFrame.Points[p].ToVector3();
                        Uframe.Tracked[p] = storeFrame.Tracked[p];

                    }

                    Uframe.Body = storeFrame.Body.ToVector3();
                    Uframe.FrameNumber = frameNumber;

                    DecodeDepth(Uframe, storeFrame.Data, storeFrame.DepthSampling, storeFrame.Min, storeFrame.Max);

                    return true;
                }


            }

            return false;


        }


    }


    public class Block
    {


        public bool isZero = true;
        public bool isFull = true;
        public ushort[] userMap;
        public ushort[] depthMap;
        public int Min, Max;

        public Block(int size)
        {

            userMap = new ushort[size * size];
            depthMap = new ushort[size * size];
            Min = 999999;
            Max = -999999;

        }

    }










}