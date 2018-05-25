using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Linq;

namespace PresenceEngine
{
    
    public struct PFile
    {

        public string Name;
        public string Path;


    }


    public struct PFolder
    {

        public string Name;
        public string Path;


    }

    public static class IO
    {
        static string me = "IO: ";

        static string depthCaptureFile = "/savedDepthCaptures.pdc";
        public static int depthIndex = -1;

        public static string localStorageFolder = "";

        // Reference to currently visible folder
        static string _browseFolder = "";
        static string _checkedOutFile = ""; // Path including folder!


        static PFile[] FilesInBrowseFolder;

        public static string BrowseFolder
        {

            get
            {
                return _browseFolder;
            }
            set
            {
                Debug.LogWarning("Can't set BrowseFolder directly, use SetBrowseFolder methods.");
            }


        }

        public static string CheckedOutFile
        {

            get
            {
                return _checkedOutFile;
            }
            set
            {
                Debug.LogWarning("Can't set CheckedOutFile directly, use SetCheckedoutFile methods.");
            }


        }

        public static void SetBrowseFolder(string folder)
        {

            folder = (folder != "" ? folder : "/noname");

            if (!Directory.Exists(localStorageFolder + folder))
                Directory.CreateDirectory(localStorageFolder + folder);

            _browseFolder = folder;

            FilesInBrowseFolder = GetLocalFiles(_browseFolder);

        }



        public static int CheckedOutFileIndex (){
            
          return  Array.FindIndex(FilesInBrowseFolder, f => f.Path == IO.CheckedOutFile);



        }


        public static string GetFilePath (int index){

            if (index<FilesInBrowseFolder.Length)
            return FilesInBrowseFolder[index].Path;

            return "";
        }

        public static void SetCheckedOutFile(string folderName, string fileName)
        {

            string folder = (folderName != "" ? "/" + folderName : "/noname");

            if (!Directory.Exists(localStorageFolder + folder))
                Directory.CreateDirectory(localStorageFolder + folder);

            _checkedOutFile = folder + "/" + (fileName != "" ? fileName : "noname");

        }


        public static void SetCheckedOutFile(string path)
        {

            // Set via /folder/name

            char[] delimiter = { '\\', '/' };

            string[] parts = path.Split(delimiter);

            if (parts.Length == 3)
            {

                SetCheckedOutFile(parts[1], parts[2]);

            }
            else
            {

                Debug.LogWarning("Path " + path + " incorrect ");
            }


        }



        public static   FileformatBase FindBufferFileInScene(string fileName)
        {

            Debug.Log("Trying to find buffer " + fileName);

            foreach (KeyValuePair<string, Presence> entry in SETTINGS.Presences)
            {

                // do something with entry.Value or entry.Key

                if (entry.Value != null && entry.Value.DepthTransport != null && entry.Value.DepthTransport.TransCoder != null)
                {
                    FileformatBase bufferFile = entry.Value.DepthTransport.TransCoder.GetBufferFile();

                    if (bufferFile != null && bufferFile.Name == fileName)
                    {
                        Debug.Log("found buffer file");
                        return bufferFile;
                    }

                }

            }


            Debug.Log("Didn't find buffer file");

            return null;
        }

        public static      FileformatBase GetFileBuffer(string filePath)
        {

            FileformatBase Buffered = FindBufferFileInScene(filePath);

            if (Buffered == null)
            {
                // try loading it from disk
             
                Buffered = IO.LoadFromFile(filePath);  // returns null and logs error on fail.
                               
            }


            return Buffered;


        }

        public static void MakeNewFile(string path)
        {

            SetCheckedOutFile(path);
            SaveCheckedOutFileAsPlaceholder();

        }

        public static void MakeNewFolder(string path)
        {


            Directory.CreateDirectory(localStorageFolder + path);

            SetBrowseFolder(path);

        }

        public static PFolder[] GetLocalFolders()
        {

            if (!Directory.Exists(localStorageFolder))
                Directory.CreateDirectory(localStorageFolder);

            string[] FolderPaths = Directory.GetDirectories(localStorageFolder);

            PFolder[] FolderList = new PFolder[FolderPaths.Length];

            for (int i = 0; i < FolderPaths.Length; i++)
            {
                string subFolder = FolderPaths[i];

                char[] delimiter = new char[] { '/', '\\' };
                string[] name = subFolder.Split(delimiter);

                PFolder f = new PFolder
                {
                    Path = Strippath(subFolder),

                    Name = name[name.Length - 1]
                };

                FolderList[i] = f;

                Debug.Log("folder " + f.Name);

            }

            return FolderList;


        }

        static string Strippath(string path)
        {

            int storagePathLength = localStorageFolder.Length;

            return path.Substring(storagePathLength, path.Length - storagePathLength);



        }

        public static PFile[] GetLocalFiles(string LocalFolder)
        {
            Debug.Log("listing files for " + LocalFolder);

            if (!Directory.Exists(localStorageFolder))
                Directory.CreateDirectory(localStorageFolder);

            if (!Directory.Exists(localStorageFolder + LocalFolder))
                return new PFile[0];


            //string[] FilePaths = Directory.GetFiles(localStorageFolder + LocalFolder);



            //string[] fileNames = Directory.GetFiles("directory ", "*.*");

            //DateTime[] creationTimes = new DateTime[FilePaths.Length];

            //for (int i = 0; i < FilePaths.Length; i++)
            //    creationTimes[i] = new FileInfo(FilePaths[i]).CreationTime;
            
            //Array.Sort(creationTimes, fileNames);



            // Get files and sort last modified first.

            List<FileInfo> sortedFiles = new DirectoryInfo(localStorageFolder + LocalFolder).GetFiles().OrderByDescending(f => f.LastWriteTime).ToList();


            //sortedFiles[0].


            PFile[] FileList = new PFile[sortedFiles.Count];


            for (int i = 0; i < sortedFiles.Count; i++)
            {

                FileInfo fileInfo = sortedFiles[i];

                //string file = FilePaths[i];


                //char[] delimiter = new char[] { '/', '\\' };
                //string[] name = file.Split(delimiter);

                FileList[i] = new PFile
                {
                    Path = LocalFolder+"/"+fileInfo.Name,

                    Name =fileInfo.Name

                        
                    //Path = Strippath(file),

                    //Name = name[name.Length - 1]
                };

           

                Debug.Log("filename " +  FileList[i].Name);
                Debug.Log("filesubpath " +  FileList[i].Path);


            }

            return FileList;


        }


        /*
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
*/

        public static void SaveCheckedOutFileAsPlaceholder()
        {

            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Create(localStorageFolder + CheckedOutFile);

            bf.Serialize(file, "Presence placeholder file.");

            file.Close();

        }

        public static void SaveToCheckedOutFile(FileformatBase presenceFile)
        {

            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Create(localStorageFolder + CheckedOutFile);

            bf.Serialize(file, presenceFile);

            file.Close();

        }

        public static FileformatBase LoadFromFile(string filePath)
        {

            if (filePath=="")
                return null;
            
            FileformatBase loaded;

            using (FileStream fs = File.Open(localStorageFolder + filePath, FileMode.Open))
            {

                try
                {
                    Debug.Log("Loading buffer from file");
                    BinaryFormatter bf = new BinaryFormatter();
                     loaded = (FileformatBase)bf.Deserialize(fs);
                    fs.Close();

                }
                catch (Exception e)
                {

                    Debug.LogError(e);
                    return null;

                }

            }

            return loaded;

        }




        /*

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
  */

    }
}