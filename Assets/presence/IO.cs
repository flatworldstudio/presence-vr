using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Linq;
//using NUnit.Framework;

namespace PresenceEngine
{

    public struct PFile
    {

        public string Name; // name only

        public string Path; // subpath from localstorage using forward slashes.


    }


    public struct PFolder
    {

        public string Name;
        public string Path; // subpath from localstorage using forward slashes.


    }

    public static class IO
    {


        public static string localStorageFolder = "";

        // Reference to currently visible folder

        static string _selectedFolder = ""; // "/subpath"
        static string _selectedFile = ""; // "/subpath/name"


        static List<PFile> _filesInSelectedFolder;


        public static void SetDataPath()
        {
            localStorageFolder = Application.persistentDataPath + "/data";
            Debug.Log("IO data path: " + localStorageFolder);
            Directory.CreateDirectory(localStorageFolder);
            Debug.LogWarning("Creating directory: " + localStorageFolder);
        }

        // Public save/load methods.

        public static void SaveFileToSelected(FileformatBase presenceFile)
        {

            BinaryFormatter bf = new BinaryFormatter();

            Directory.CreateDirectory(localStorageFolder + _selectedFolder);

            Debug.LogWarning("Creating directory: " + localStorageFolder + _selectedFolder);

            FileStream file = File.Create(localStorageFolder + _selectedFile);

            bf.Serialize(file, presenceFile);
            file.Close();

            _filesInSelectedFolder = null;

        }

        public static FileformatBase LoadFile(string filePath)
        {

            FileformatBase Buffered = FindBufferFileInScene(filePath);

            if (Buffered == null)
            {
                // try loading it from disk

                Buffered = IO.LoadFromFile(filePath);  // returns null and logs error on fail.

                if (Buffered == null)
                {

                    // create it
                    //Debug.Log("CREATING FILE " + filePath);

                    //Buffered = new FileformatBase();

                }


            }

            return Buffered;

        }





        // Public browsing methods.

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

                PFolder f = new PFolder();

                f.Name = name[name.Length - 1];
                f.Path = "/" + f.Name;

                FolderList[i] = f;

                Debug.Log("folder: " + f.Name);
                Debug.Log("folderpath: " + f.Path);

            }

            _filesInSelectedFolder = null;

            return FolderList;


        }

        public static string SelectedFolder
        {

            get
            {
                return _selectedFolder;
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

        //public static void SelectFile(string folderName, string fileName)
        //{
        //    folderName = Strip(folderName);
        //    fileName = Strip(fileName);

        //    if (folderName == "" || fileName == "")
        //        return;

        //    _selectedFile = "/" + folderName + "/" + fileName;

        //}

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

        public static int CheckedOutFileIndex()
        {

            // Returns the index for the file that is currently selected.

            for (int i = 0; i < FilesInSelectedFolder.Count; i++)
            {
                if (FilesInSelectedFolder[i].Path == IO.CheckedOutFile)
                    return i;
            }

            return 0;

        }


        public static string GetFilePath(int index)
        {

            if (index < FilesInSelectedFolder.Count)
                return FilesInSelectedFolder[index].Path;

            return "";
        }

        public static void MakeNewFile(string path)
        {

            SelectFile(path);
            SaveCheckedOutFileAsPlaceholder();
            _filesInSelectedFolder = null;

        }

        public static void MakeNewFolder(string path)
        {


            Directory.CreateDirectory(localStorageFolder + path);

            SelectFolder(path);
            _filesInSelectedFolder = null;
        }



        // Internal methods.

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


        static FileformatBase FindBufferFileInScene(string fileName)
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



        static List<PFile> GetFileList(string LocalFolder)
        {

            LocalFolder = "/" + Strip(LocalFolder);

            //     Debug.Log("listing files for " + LocalFolder);

            if (!Directory.Exists(localStorageFolder))
                Directory.CreateDirectory(localStorageFolder);

            if (!Directory.Exists(localStorageFolder + LocalFolder))
                return new List<PFile>();

            // Get files and sort last modified first.

            List<FileInfo> sortedFiles = new DirectoryInfo(localStorageFolder + LocalFolder).GetFiles().OrderByDescending(f => f.LastWriteTime).ToList();

            // Go over them and keep the .prs files.

            List<PFile> FileList = new List<PFile>();

            foreach (FileInfo fi in sortedFiles)
            {

                if (fi.Extension == ".prs")
                {

                    PFile pFile = new PFile

                    {
                        Path = LocalFolder + "/" + fi.Name,
                        Name = fi.Name

                    };

                    FileList.Add(pFile);
                    //Debug.Log("filename " + pFile.Name);
                    //Debug.Log("filesubpath " + pFile.Path);

                }

            }

            return FileList;

        }




        static void SaveCheckedOutFileAsPlaceholder()
        {

            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Create(localStorageFolder + CheckedOutFile);

            bf.Serialize(file, new FileformatBase());

            file.Close();

            _filesInSelectedFolder = null;


        }


        static FileformatBase LoadFromFile(string filePath)
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