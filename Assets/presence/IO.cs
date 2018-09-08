using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Linq;
using StoryEngine;
using Logger = StoryEngine.Logger;

namespace PresenceEngine
{

    //public struct PFile
    //{

    //    public string Name; // name only
    //    public string Folder;

    //    public string Path; // subpath from localstorage using forward slashes.



    //}


    //public struct PFolder
    //{

    //    public string Name;
    //    public string Path; // subpath from localstorage using forward slashes.


    //}

    public class IO : MonoBehaviour
    {
        string ID = "IO";

        string localStorageFolder = "";
        Dictionary<string, FileformatBase> PresenceCache;
        public static IO Instance;

        bool busy = false;
        //string AsyncFilePath;
        //StoryTask AsyncTaskRef;
        //string _selectedFolderName = ""; // "/subpath"
        // string _selectedFileName = ""; // "/subpath/name"

        //  string _selected = "";

        //    static List<PFile> _filesInSelectedFolder;

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.

        void Log(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.NORMAL);
        }
        void Warning(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.WARNINGS);
        }
        void Error(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.ERRORS);
        }
        void Verbose(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.VERBOSE);
        }

        private void Start()
        {
            SetLocalStorage();
            ListStorageContents();
            Instance = this;
            PresenceCache = new Dictionary<string, FileformatBase>();
        }

        private void Update()
        {

        }

        void SetLocalStorage()
        {


            localStorageFolder = Application.persistentDataPath + "/data";

            Log("Data path: " + localStorageFolder);

            if (!Directory.Exists(localStorageFolder))
            {
                Warning("Data path doesn't exist, creating it.");
                Directory.CreateDirectory(localStorageFolder);
            }

            // Double checking.

            if (Directory.Exists(localStorageFolder))
            {
                Warning("Data path created successfully.");
            }


        }

        public string RebuildPath(string path)
        {
            string[] parts = Split(path);

            if (parts.Length != 2)
            {
                Error("Path depth not 2.");
                return "";
            }

            return "/" + parts[0] + "/" + parts[1];



        }

        public string FolderFromPath(string path)
        {
            string[] parts = Split(path);

            if (parts.Length > 2)
            {
                Warning("Path depth exceeds 2, just returning first item as folder.");

            }

            return parts[0];


        }

        public string FileFromPath(string path)
        {
            string[] parts = Split(path);

            if (parts.Length != 2)
            {
                Warning("Path depth not 2, invalid path.");
                return "";
            }

            return "/" + parts[1];


        }

        /*
        public string SelectedFolder
        {

            get
            {
                string[] parts = Split(_selected);

                if (parts.Length > 2)
                    Error("Path depth exceeds 2");

                return parts[0];

                //  return _selectedFolder;
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
                return _selectedFile;
            }
            set
            {
                Debug.LogWarning("Can't set CheckedOutFile directly, use SetCheckedoutFile methods.");
            }

        }
        */


        /*
        public static void Test()
        {
            if (Directory.Exists(localStorageFolder + "/test"))
            {
                Debug.LogWarning("Test folder exists.");

            }

                Directory.CreateDirectory(localStorageFolder + "/test");
            Debug.LogWarning("Creating test directory: " + localStorageFolder + "/test");


            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Create(localStorageFolder + "test.prs");

            bf.Serialize(file, new FileformatBase());

            file.Close();

            if (File.Exists(localStorageFolder + "test.prs"))
            {
                Debug.LogWarning("test file exists");
            }

        }
        */

        public void ListStorageContents()
        {

            Log("Listing local storage contents: ");

            string[] FolderPaths = Directory.GetDirectories(localStorageFolder);

            foreach (string folder in FolderPaths)
            {
                Log("Folder: " + folder);
                string[] FilePaths = Directory.GetFiles(folder);
                foreach (string file in FilePaths)
                {
                    Log(file);
                }
            }


        }


        // Public save/load methods.
        public void SaveManual(FileformatBase presenceFile, string path, StoryTask taskRef,string prefix)
        {

            Directory.CreateDirectory(localStorageFolder + "/" + FolderFromPath(path));

            path = RebuildPath(path);

            if (!busy)
            {
                StartCoroutine(SaveManualAsync(presenceFile, path, taskRef,prefix));
            }



        }
        IEnumerator SaveManualAsync(FileformatBase file, string fileName, StoryTask taskRef,string prefix)
        {
            Log("Saving " + fileName + " on " + prefix);

            busy = true;

            // Store a ref to the frames.
            List<FrameBase> frames = file.Frames;
            Log("number of frames " + frames.Count);

            file.Frames = new List<FrameBase>();// clear out the main file for serialisation.

            FileStream fs = File.Create(localStorageFolder + fileName + SETTINGS.Ext);
            BinaryFormatter bf = new BinaryFormatter();

            MemoryStream ContentSize = new MemoryStream();
            MemoryStream Content = new MemoryStream();

            // serialise content
            bf.Serialize(Content, file);
            Verbose("main file serialised  " + Content.Length);

            // serialise length of content
            bf.Serialize(ContentSize, Content.Length);
            Verbose("mainfile length integer " + ContentSize.Length);

            ContentSize.WriteTo(fs);
            Content.WriteTo(fs);

            // Now iterate over frames
            ContentSize = new MemoryStream();

            long FrameCount = frames.Count;

            // serialise number of frames of content
            bf.Serialize(ContentSize, FrameCount);
            Verbose("framecount integer " + ContentSize.Length);
            ContentSize.WriteTo(fs);


            int f = 0;
            while (f < FrameCount)
            {
                FrameBase frame = frames[f];

                Verbose("serialising frame " + f);

                // serialise frame

                Content = new MemoryStream();
                bf.Serialize(Content, frame);
                Verbose("frame serialised  " + Content.Length);

                // serialise frame size

                ContentSize = new MemoryStream();
                bf.Serialize(ContentSize, Content.Length);
                Verbose("frame length integer " + ContentSize.Length);

                ContentSize.WriteTo(fs);
                Content.WriteTo(fs);

                f++;

                if (f % 8 == 0)
                {
                    taskRef.SetStringValue(prefix+"State", "" + (FrameCount - f));
                    yield return null;
                }

            }

            Verbose("Closing file");
            fs.Close();

            // Put the frames back.
            file.Frames = frames;

            AddToCache(file, fileName);

            taskRef.SetStringValue(prefix + "State", "done");
            busy = false;

            yield return null;
        }

        void AddToCache(FileformatBase file, string fileName)
        {
            Log("Adding to cache: " + fileName);

            FileformatBase entry;
            if (PresenceCache.TryGetValue(fileName, out entry))
            {
                PresenceCache[fileName] = file;
            }
            else
            {
                PresenceCache.Add(fileName, file);
            }


        }
        /*
        IEnumerator SaveManualAsync (FileInfo file, string fileName)
        {
            Log("saving a file");


            //     Directory.CreateDirectory(localStorageFolder +"test");

            FileStream fs = File.Create(fileName);

            // First some basics.
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream info = new MemoryStream();
            MemoryStream content = new MemoryStream();


            bf.Serialize(content, "something can be serialized here");


            Log("content length " + content.Length);

            bf.Serialize(info, content.Length);

            Log("info length " + info.Length);

            info.WriteTo(fs);
            content.WriteTo(fs);

            //  bf.Serialize(fs, "something");

            // bf.Serialize(fs, "something else");


            //     byte[] buffer = new byte[1024];

            //    fs.Write(buffer, 0, 1024);

            fs.Close();
          

            yield return null;
        }
        */
        public FileformatBase fileref;

        IEnumerator LoadManualAsync(string fileName, StoryTask taskRef,string prefix)
        {
            busy = true;
            FileformatBase loaded = null;

            using (FileStream rf = File.Open(localStorageFolder + fileName + SETTINGS.Ext, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();

                //   byte[] bytes = new byte[rf.Length];

                byte[] infoBuffer = new byte[58];
                rf.Read(infoBuffer, 0, 58);
                MemoryStream info = new MemoryStream(infoBuffer);
                System.Int64 contentLength = (System.Int64)bf.Deserialize(info);
                Verbose("content length " + contentLength);

                byte[] contentBuffer = new byte[contentLength];
                rf.Read(contentBuffer, 0, (int)contentLength);
                MemoryStream content = new MemoryStream(contentBuffer);
                FileformatBase file = (FileformatBase)bf.Deserialize(content);

                Verbose("content " + file.Name + " " + file.TransCoderName);

                infoBuffer = new byte[58];
                rf.Read(infoBuffer, 0, 58);
                info = new MemoryStream(infoBuffer);
                System.Int64 frameCount = (System.Int64)bf.Deserialize(info);
                Verbose("frame counnt " + frameCount);

                file.Frames = new List<FrameBase>(); // should already be there

                int f = 0;
                while (f < frameCount)
                {

                    Verbose("deserialising frame " + f);

                    infoBuffer = new byte[58];
                    rf.Read(infoBuffer, 0, 58);
                    info = new MemoryStream(infoBuffer);
                    contentLength = (System.Int64)bf.Deserialize(info);
                    Verbose("frame length " + contentLength);

                    contentBuffer = new byte[contentLength];
                    rf.Read(contentBuffer, 0, (int)contentLength);
                    content = new MemoryStream(contentBuffer);
                    FrameBase frame = (FrameBase)bf.Deserialize(content);



                    file.Frames.Add(frame);

                    f++;

                    if (f % 8 == 0)
                    {
                        taskRef.SetStringValue(prefix+"State", "" + (frameCount - f));
                        yield return null;
                    }


                }

                AddToCache(file, fileName);
                //   PresenceCache.Add(fileName, file);
                Verbose("cache size " + PresenceCache.Keys.ToArray().Length);
                // Log("filename " + fileName);

                loaded = file;
                fileref = file;



                rf.Close();
            }
            //     Directory.CreateDirectory(localStorageFolder +"test");
            //     FileStream file = File.Create(localStorageFolder + "test.tst");



            if (loaded == null)
                taskRef.SetStringValue(prefix+"State", "failed");
            else
                taskRef.SetStringValue(prefix+"State", "done");

            busy = false;

            yield return null;



        }

        public void LoadManual(string path, StoryTask taskRef,string prefix)
        {
            path = RebuildPath(path);


            FileformatBase Buffered = FindInCache(RebuildPath(path));

            if (Buffered != null)
            {
                taskRef.SetStringValue(prefix+"State", "done");

            }
            else
            {


                if (!busy)
                {
                    Log("loading async from disk");
                    StartCoroutine(LoadManualAsync(path, taskRef,prefix));
                }



            }






        }


        public void SaveFile(FileformatBase presenceFile, string path)
        {

            BinaryFormatter bf = new BinaryFormatter();

            Directory.CreateDirectory(localStorageFolder + "/" + FolderFromPath(path));

            //     Debug.Log("Creating directory: " + localStorageFolder + _selectedFolder);

            FileStream file = File.Create(localStorageFolder + RebuildPath(path) + SETTINGS.Ext);

            bf.Serialize(file, presenceFile);

            file.Close();

            Debug.Log("Saving file: " + localStorageFolder + RebuildPath(path));

            //     _filesInSelectedFolder = null;

        }




        // Public browsing methods.

        public string[] GetFolders()
        {

            //if (!Directory.Exists(localStorageFolder)
            //    Directory.CreateDirectory(localStorageFolder);

            string[] FolderPaths = Directory.GetDirectories(localStorageFolder);

            string[] FolderList = new string[FolderPaths.Length];

            for (int i = 0; i < FolderPaths.Length; i++)
            {
                string subFolder = FolderPaths[i];

                string[] parts = Split(subFolder);

                //   PFolder f = new PFolder();


                string Name = parts[parts.Length - 1];
                //    f.Path = "/" + f.Name;

                FolderList[i] = Name;

                Debug.Log("folder: " + Name);
                //   Debug.Log("folderpath: " + f.Path);

            }

            //  _filesInSelectedFolder = null;

            return FolderList;


        }


        public string[] GetFiles(string folder)
        {

            folder = FolderFromPath(folder);


            //     Debug.Log("listing files for " + LocalFolder);


            if (!Directory.Exists(localStorageFolder + "/" + folder))
                return new string[0];


            // Get files and sort last modified first.

            List<FileInfo> sortedFiles = new DirectoryInfo(localStorageFolder + "/" + folder).GetFiles().OrderByDescending(f => f.LastWriteTime).ToList();

            // Go over them and keep the .prs files.

            List<string> FileList = new List<string>();

            foreach (FileInfo fi in sortedFiles)
            {

                if (fi.Extension == SETTINGS.Ext)
                {

                    FileList.Add(StripExtention(fi.Name));

                }

            }

            return FileList.ToArray();

        }

        string StripExtention(string file)
        {
            char[] delimiter = { '.' };

            return file.Split(delimiter)[0];


        }

        /*
        public static List<PFile> FilesInSelectedFolder
        {

            get
            {
                if (_filesInSelectedFolder == null)
                {
                    _filesInSelectedFolder = GetFileList(_selectedFolder);
                }

                return _filesInSelectedFolder;
            }
            set
            {
                Debug.LogWarning("Can't set FilesInFolder.");
            }

        }
        */
        /*
        public static void SelectFolder(string folderName)
        {
            folderName = Strip(folderName);

            if (folderName == "")
                return;

            _selectedFolder = "/" + folderName;

            //  Debug.Log("IO " + localStorageFolder + folderName);

            if (!Directory.Exists(localStorageFolder + _selectedFolder))
                Directory.CreateDirectory(localStorageFolder + _selectedFolder);

            _filesInSelectedFolder = null;

        }
        */
        //public static void SelectFile(string folderName, string fileName)
        //{
        //    folderName = Strip(folderName);
        //    fileName = Strip(fileName);

        //    if (folderName == "" || fileName == "")
        //        return;

        //    _selectedFile = "/" + folderName + "/" + fileName;

        //}
        /*
        public static void SelectFile(string path)
        {

            // Set via /folder/name

            char[] delimiter = { '\\', '/' };

            string[] parts = path.Split(delimiter);

            if (parts.Length == 3)
            {

                _selectedFile = "/" + parts[1] + "/" + parts[2];
                _selectedFolder = "/" + parts[1];

            }
            else
            {

                Debug.LogWarning("Path " + path + " incorrect ");
            }


        }
        */

        //public int FileIndex(string fileName, string folderName)
        //{

        //    // Returns the index for the file that is currently selected.

        //    for (int i = 0; i < FilesInSelectedFolder.Count; i++)
        //    {
        //        if (FilesInSelectedFolder[i].Path == IO.CheckedOutFile)
        //            return i;
        //    }

        //    return 0;

        //}


        //public static string GetFilePath(int index)
        //{

        //    if (index < FilesInSelectedFolder.Count)
        //        return FilesInSelectedFolder[index].Path;

        //    return "";
        //}

        public void MakeNewFile(string path)
        {
            Log("Make file " + path);

            FileformatBase placeholder = new FileformatBase();


            SaveFile(placeholder, RebuildPath(path));

            //   SelectFile(path);
            //   SaveCheckedOutFileAsPlaceholder();
            //   _filesInSelectedFolder = null;

        }

        public void MakeNewFolder(string name)
        {


            Directory.CreateDirectory(localStorageFolder + "/" + name);

            //SelectFolder(path);
            //_filesInSelectedFolder = null;
        }



        // Internal methods.

        string[] Split(string name)
        {
            // Remove all slashes to prevent PC/MAC mixups.
            // Returns an array of all non-zero strings.

            char[] delimiter = { '\\', '/' };
            string[] split = name.Split(delimiter);
            List<string> parts = new List<string>();

            for (int p = 0; p < split.Length; p++)
            {

                if (split[p].Length > 0)
                {
                    parts.Add(split[p]);

                }

            }

            return parts.ToArray();


        }

        /*
        static string Strip(string name)
        {
            // Remove all slashes to prevent PC/MAC mixups.

            char[] delimiter = { '\\', '/' };

            string[] parts = name.Split(delimiter);

            for (int p = 0; p < parts.Length; p++)
            {

                if (parts[p].Length > 0)
                {

                    return parts[p];
                }

            }

            Debug.LogWarning("Stripping failed.");
            return "";

        }
        */

        // LOAD SAVE
        public FileformatBase GetFromCache(string filePath)
        {

            filePath = RebuildPath(filePath);

            FileformatBase Buffered = FindInCache(RebuildPath(filePath));
                   

            return Buffered;

        }


        public FileformatBase LoadFile(string filePath)
        {

            filePath = RebuildPath(filePath);

            FileformatBase Buffered = FindInCache(RebuildPath(filePath));

            if (Buffered == null)
            {



              //  Buffered = LoadFromFile(RebuildPath(filePath) + SETTINGS.Ext);  // returns null and logs error on fail.



                if (Buffered != null)
                {
                    PresenceCache.Add(filePath, Buffered);

                    Log("Added to cache: " + filePath);

                    //Buffered = new FileformatBase();

                }


            }

            return Buffered;

        }

        public void LoadFileAsync(string filePath, StoryTask taskRef)
        {

            if (filePath == "")
            {
                taskRef.SetStringValue("loadingstate", "failed");
                return;
            }

            filePath = RebuildPath(filePath);
            //    AsyncFilePath =
            //    AsyncTaskRef = taskRef;

            FileformatBase Buffered = FindInCache(filePath);

            if (Buffered == null)
            {
                // try loading it from disk
                Log("try loading from disk");

                //if (!File.Exists(localStorageFolder + filePath+SETTINGS.Ext))
                //{
                //    Log("File not found " + localStorageFolder + AsyncFilePath + SETTINGS.Ext);
                //    taskRef.SetStringValue("loadingstate", "failed");
                //    return;
                //}


                if (!busy)
                {

                    StartCoroutine(Load(filePath, taskRef));
                }




            }



        }

        IEnumerator Load(string filePath, StoryTask taskRef)
        {
            busy = true;

            FileformatBase loaded = null;

            using (FileStream fs = File.Open(localStorageFolder + filePath + SETTINGS.Ext, FileMode.Open))
            {

                //
                Debug.Log("Loading buffer from file: " + filePath);

                byte[] bytes = new byte[fs.Length];

                int bytesToRead = (int)fs.Length;
                int bytesRead = 0;
                //    string DEBUG = "";
                int MaxBuffer = 1024 * 1024;


                while (bytesToRead > 0)
                {

                    int n = fs.Read(bytes, bytesRead, Mathf.Min(bytesToRead, MaxBuffer));

                    if (n == 0)
                        break;

                    // loaded = (FileformatBase)bf.Deserialize(stream);


                    bytesToRead -= n;
                    bytesRead += n;

                    //DEBUG += " " + n;
                    Log("" + n);

                    taskRef.SetStringValue("debug", "Remaining: " + bytesToRead);

                    yield return null;
                }
                //     Debug.Log(DEBUG);

                Log("loading done");

                fs.Close();



                //   fs.Read

                Stream stream = new MemoryStream(bytes);
                BinaryFormatter bf = new BinaryFormatter();


                loaded = (FileformatBase)bf.Deserialize(stream);
                PresenceCache.Add(filePath, loaded);


                // }
                //catch (Exception e)
                //{

                //    Debug.LogError(e);


                //}

            }
            //  return;

            if (loaded == null)
                taskRef.SetStringValue("loadingstate", "failed");
            else
                taskRef.SetStringValue("loadingstate", "done");

            busy = false;
            yield return null;

        }



        FileformatBase FindBufferFileInScene(string fileName)
        {

            //  Debug.Log("Trying to find buffer " + fileName);

            foreach (KeyValuePair<string, Presence> entry in SETTINGS.Presences)
            {

                // do something with entry.Value or entry.Key

                if (entry.Value != null && entry.Value.DepthTransport != null && entry.Value.DepthTransport.TransCoder != null)
                {
                    FileformatBase bufferFile = entry.Value.DepthTransport.TransCoder.GetBufferFile();

                    if (bufferFile != null && bufferFile.Name == fileName)
                    {
                        Debug.Log("found buffer file in scene");
                        return bufferFile;
                    }

                }

            }


            //     Debug.Log("Didn't find buffer file in scene.");

            return null;
        }




        FileformatBase FindInCache(string fileName)
        {



            FileformatBase r;
            PresenceCache.TryGetValue(fileName, out r);

            if (r == null)
                Log("Not found in cache " + fileName);
            else
                Log("Found in cache " + fileName);

            return r;


            //     Debug.Log("Didn't find buffer file in scene.");

            //            return null;
        }




        /*

        static void SaveCheckedOutFileAsPlaceholder()
        {

            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Create(localStorageFolder + CheckedOutFile);

            bf.Serialize(file, new FileformatBase());

            file.Close();

            _filesInSelectedFolder = null;


        }
        */



        // FileformatBase LoadAsync(string filePath)
        //{

        //    if (filePath == "")
        //        return null;

        //    if (!File.Exists(localStorageFolder + filePath))
        //        return null;


        //    StartCoroutine("Load");


        //    return null;


        //}


        FileformatBase LoadAsyncTest(string filePath)
        {

            if (filePath == "")
                return null;

            if (!File.Exists(localStorageFolder + filePath + SETTINGS.Ext))
                return null;


            FileformatBase loaded;

            using (FileStream fs = File.Open(localStorageFolder + filePath + SETTINGS.Ext, FileMode.Open))
            {

                try
                {
                    Debug.Log("Loading buffer from file: " + filePath);

                    byte[] bytes = new byte[fs.Length];

                    int bytesToRead = (int)fs.Length;
                    int bytesRead = 0;
                    string DEBUG = "";
                    int MaxBuffer = 16 * 1024;

                    while (bytesToRead > 0)
                    {

                        int n = fs.Read(bytes, bytesRead, Mathf.Min(bytesToRead, MaxBuffer));

                        if (n == 0)
                            break;

                        bytesToRead -= n;
                        bytesRead += n;

                        DEBUG += " " + n;

                    }
                    Debug.Log(DEBUG);

                    fs.Close();

                    Stream stream = new MemoryStream(bytes);

                    //   fs.Read


                    BinaryFormatter bf = new BinaryFormatter();
                    loaded = (FileformatBase)bf.Deserialize(stream);
                    //  loaded = (FileformatBase)bf.Deserialize(fs);


                }
                catch (Exception e)
                {

                    Debug.LogError(e);
                    return null;

                }

            }

            return loaded;

        }

        FileformatBase LoadFromFile(string filePath)
        {

            if (filePath == "")
                return null;

            if (!File.Exists(localStorageFolder + filePath))
                return null;


            FileformatBase loaded;

            using (FileStream fs = File.Open(localStorageFolder + filePath, FileMode.Open))
            {

                try
                {
                    Debug.Log("Loading buffer from file: " + filePath);
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


    }
}