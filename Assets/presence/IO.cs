using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


namespace PresenceEngine
{


    [System.Serializable]
    public class CloudSequence
    {
        public static CloudSequence current;

        public CloudFrame[] Frames;

        public CloudSequence(int size)
        {
            Frames = new CloudFrame[size];
        }

        public void Wrap()
        {

            for (int f = 0; f < Frames.Length; f++)
            {
                Frames[f].Wrap();
            }

        }

        public void UnWrap()
        {

            for (int f = 0; f < Frames.Length; f++)
            {
                Frames[f].UnWrap();
            }

        }



    }




    [System.Serializable]
    public class CloudFrame
    {


        [System.NonSerialized] public Vector3[] Points;

        public float[] pointsX;
        public float[] pointsY;
        public float[] pointsZ;


        public CloudFrame(int size)
        {

            Points = new Vector3[size];
        }

        public void Wrap()
        {

            pointsX = new float[Points.Length];
            pointsY = new float[Points.Length];
            pointsZ = new float[Points.Length];

            for (int p = 0; p < Points.Length; p++)
            {

                pointsX[p] = Points[p].x;
                pointsY[p] = Points[p].y;
                pointsZ[p] = Points[p].z;

            }

        }

        public void UnWrap()
        {

            Points = new Vector3[pointsX.Length];

            for (int p = 0; p < Points.Length; p++)
            {

                Points[p] = new Vector3(pointsX[p], pointsY[p], pointsZ[p]);

            }

        }


    }






    [System.Serializable]
    public class DepthCapture
    {
        public static DepthCapture current;

        ushort[] data;
        int userDepthWidth, userDepthHeight;

        public DepthCapture(int width, int height)
        {
            userDepthWidth = width;
            userDepthHeight = height;

        }

        public void put(ushort[] frame)
        {

            data = frame;
        }

        public ushort[] GetRawDepthMap()
        {
            return data;
        }

        public int getUserDepthWidth()
        {
            return userDepthWidth;
        }

        public int getUserDepthHeight()
        {
            return userDepthHeight;
        }



    }





    [System.Serializable]

    public class Capture
    {

        public static Capture current;
        public static bool capturing = false;
        public static bool playing = false;

        //	public Character knight;
        //	public Character rogue;
        //	public Character wizard;

        //	public Vector3[] position;
        //	public Quaternion[] orientation;
        public Frame[] frames;
        int size = 250;

        int i;

        public Capture()
        {
            //		i = 0;

            frames = new Frame[size];

            //		orientation = new Quaternion [1000];

            //		knight = new Character ();
            //		rogue = new Character ();
            //		wizard = new Character ();
        }

        public void capture()
        {
            Capture.capturing = true;
            i = 0;

        }

        public void play()
        {
            Capture.playing = true;
            i = 0;

        }

        public bool read(out Frame f)
        {

            if (i == size)
            {
                f = new Frame();
                return false;

            }
            else
            {

                f = frames[i];
                i++;
                return true;
            }


        }

        public bool log(Vector3 pos, Quaternion orient)
        {

            if (i == size)
            {
                return false;
            }
            else
            {
                Frame f = new Frame(pos, orient);
                //			f.position = pos;
                //			f.orientation = orient;

                frames[i] = f;
                i++;
                return true;
            }

        }

    }


    [System.Serializable]
    public class Frame
    {
        public float[] position;
        public float[] orientation;

        //	public string name;

        public Frame(Vector3 pos, Quaternion orient)
        {
            position = new float[3];

            position[0] = pos.x;
            position[1] = pos.y;
            position[2] = pos.z;

            orientation = new float[4];

            orientation[0] = orient[0];
            orientation[1] = orient[1];
            orientation[2] = orient[2];
            orientation[3] = orient[3];


        }

        public Vector3 getPosition()
        {
            return new Vector3(position[0], position[1], position[2]);

        }

        public Quaternion getRotation()
        {
            return new Quaternion(orientation[0], orientation[1], orientation[2], orientation[3]);

        }

        public Frame()
        {

        }
    }

    public struct PFile
    {

        public string Name;
        public string LocalPath;


    }


    public struct PFolder
    {

        public string Name;
        public string LocalPath;
        

    }

    public static class IO
    {
        static string me = "IO: ";

        public static List<Capture> savedCaptures = new List<Capture>();
        public static List<DepthCapture> savedDepthCaptures = new List<DepthCapture>();

        static string depthCaptureFile = "/savedDepthCaptures.pdc";
        public static int depthIndex = -1;

        public static string localStorageFolder="";
        static string _checkedOutFolder="";

           public static string checkedOutFile="";




        public static void SetCheckedOutFile (string name)
        {
            
            checkedOutFile =CheckedOutFolder + "/" + (name != "" ? name : "_default");
          

        }
        public static string MakeDefaultFile()
        {

            CheckedOutFolder =  "/_default"; 

            if (!Directory.Exists(localStorageFolder+ CheckedOutFolder))
                Directory.CreateDirectory(localStorageFolder + CheckedOutFolder);

                  
          
          
            SetCheckedOutFile("_default");

            return checkedOutFile;

        }
        public static void MakeNewFile(string name)
        {

            SetCheckedOutFile(name);
            SaveCheckedOutFileAsPlaceholder();

        }

        public static void MakeNewFolder(string folder)
        {

            folder =  "/" + folder;

                Directory.CreateDirectory(localStorageFolder + folder);

            CheckedOutFolder = folder;

        }


        static string Strippath (string path)
        {

            int storagePathLength = localStorageFolder.Length;

            return path.Substring(storagePathLength, path.Length - storagePathLength);



        }
        public static string CheckedOutFolder
        {
            get 
            {
                // If nothing select we select the first folder. If that doesn't exist we create a default one.

                if (_checkedOutFolder == "")
                {
                    PFolder[] pFolders = GetLocalFolders();
                    if (pFolders.Length > 0){
                        _checkedOutFolder = pFolders[0].LocalPath;
                    }
                    else
                    {
                        _checkedOutFolder =  "/_default";

                        if (!Directory.Exists(localStorageFolder + _checkedOutFolder))
                            Directory.CreateDirectory(localStorageFolder + _checkedOutFolder);

                    }

                }
                return _checkedOutFolder;
            }

            set 
            {
                _checkedOutFolder = value;
            }

        }


        
        public static PFolder[] GetLocalFolders()
        {

            if (!Directory.Exists(localStorageFolder))
                Directory.CreateDirectory(localStorageFolder);

            string[] FolderPaths =  Directory.GetDirectories(localStorageFolder);

            PFolder[] FolderList = new PFolder[FolderPaths.Length];

            for (int i=0;i<FolderPaths.Length;i++)
            {
                string subFolder = FolderPaths[i];

                char[] delimiter = new char[] { '/', '\\' };
                string[] name = subFolder.Split(delimiter);

                PFolder f = new PFolder
                {
                    LocalPath = Strippath (subFolder),

                    Name = name[name.Length - 1]
                };

                FolderList[i] = f;

                Debug.Log(f.Name);

            }

            return FolderList;


        }

        public static PFile[] GetLocalFiles(string LocalFolder)
        {

            if (!Directory.Exists(localStorageFolder) || LocalFolder=="")
                return new PFile[0];


            string[] FilePaths = Directory.GetFiles(localStorageFolder+LocalFolder);
                        
            PFile[] FileList = new PFile[FilePaths.Length];

            for (int i = 0; i < FilePaths.Length; i++)
            {
                string file = FilePaths[i];

                char[] delimiter = new char[] { '/', '\\' };
                string[] name = file.Split(delimiter);

                PFile f = new PFile
                {
                    LocalPath = Strippath(file),

                    Name = name[name.Length - 1]
                };

                FileList[i] = f;

                Debug.Log(f.Name);

            }

            return FileList;


        }



        public static void LoadDepthCapturesResource()
        {

            // Load data from resources. The textasset route is a bit of a trick but allows us to get a raw stream.

            TextAsset asset = Resources.Load("savedDepthCaptures") as TextAsset;

            if (asset != null)
            {

                Stream stream = new MemoryStream(asset.bytes);
                BinaryFormatter bf = new BinaryFormatter();
                List<DepthCapture> loaded = (List<DepthCapture>)bf.Deserialize(stream);
                IO.savedDepthCaptures.AddRange(loaded);

                Debug.Log(me + "Local depth captures loaded.");

            }
            else
            {

                Debug.Log(me + "No local depth captures found.");
            }

        }

        public static void LoadDepthCaptures()
        {
            if (File.Exists(Application.persistentDataPath + "/savedDepthCaptures.pdc"))
            {

                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.Open(Application.persistentDataPath + "/savedDepthCaptures.pdc", FileMode.Open);
                List<DepthCapture> loaded = (List<DepthCapture>)bf.Deserialize(stream);
                IO.savedDepthCaptures.AddRange(loaded);

                stream.Close();
            }
        }

        public static void SaveCloudSequence(CloudSequence sequence)
        {

            sequence.Wrap();


            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Create(Application.persistentDataPath + "/savedSequence.csq");

            bf.Serialize(file, sequence);

            file.Close();

        }

        public static CloudSequence LoadCloudSequence()
        {
            CloudSequence sequence = null;

            if (File.Exists(Application.persistentDataPath + "/savedSequence.csq"))
            {

                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.Open(Application.persistentDataPath + "/savedSequence.csq", FileMode.Open);

                sequence = (CloudSequence)bf.Deserialize(stream);

                stream.Close();

                sequence.UnWrap();
            }

            return sequence;

        }

        public static CloudSequence LoadCloudSequenceFromResources()
        {

            // Load data from resources. The textasset route is a bit of a trick but allows us to get a raw stream.

            TextAsset asset = Resources.Load("savedSequence") as TextAsset;
            CloudSequence sequence = null;

            if (asset != null)
            {

                Stream stream = new MemoryStream(asset.bytes);
                BinaryFormatter bf = new BinaryFormatter();
                sequence = (CloudSequence)bf.Deserialize(stream);

                sequence.UnWrap();
                Debug.Log(me + "Local seq loaded.");

            }
            else
            {

                Debug.Log(me + "No local seq  found.");
            }

            return sequence;
        }


        public static void SaveCheckedOutFileAsPlaceholder()
        {

            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Create(localStorageFolder+ checkedOutFile);

            bf.Serialize(file,"Presence placeholder file.");

            file.Close();

        }

        public static void SaveToCheckedOutFile(FileformatBase presenceFile)
        {

            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Create(localStorageFolder + checkedOutFile);

            bf.Serialize(file, presenceFile);

            file.Close();

        }

        public static FileformatBase LoadFromCheckedOutFileAsync()
        {


            FileformatBase loaded = null;

            // Open the stream and read it back.
            using (FileStream fs = File.Open(localStorageFolder + checkedOutFile, FileMode.Open))
            {
                /*
                byte[] b = new byte[1024];
             //   UTF8Encoding temp = new UTF8Encoding(true);

                while (fs.Read(b, 0, b.Length) > 0)
                {

                   // Console.WriteLine(temp.GetString(b));
                }
                */
                BinaryFormatter bf = new BinaryFormatter();
                 loaded = (FileformatBase)bf.Deserialize(fs);
                fs.Close();
            }




         

           

            return loaded;

        }

       



      

        public static void SaveDepthCaptures()
        {

            savedDepthCaptures.Add(DepthCapture.current);

            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Create(Application.persistentDataPath + "/savedDepthCaptures.pdc");

            bf.Serialize(file, IO.savedDepthCaptures);

            file.Close();

        }


        public static void SaveUserCaptures()
        {

            savedCaptures.Add(Capture.current);

            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Create(Application.persistentDataPath + "/savedGames.gd");

            bf.Serialize(file, IO.savedCaptures);

            file.Close();

        }


        public static void LoadUserCaptures()
        {
            if (File.Exists(Application.persistentDataPath + "/savedGames.gd"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
                IO.savedCaptures = (List<Capture>)bf.Deserialize(file);
                file.Close();
            }
        }


    }
}