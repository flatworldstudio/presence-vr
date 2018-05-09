using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System;

namespace CloudStorage
{

	public delegate string ManifestProcessor (string data, long lastModified);

	public class CloudStorageClient : MonoBehaviour
	{

		private ManifestProcessor manifestProcessor;

		public string ManifestName = "manifest.json";
		public string localDataPath = "";
		public string localStorageFolder = "content";
		public string localTempFolder = "_temp";
		public bool ShowLog = true;

		public CS_Event onManifestReceived;
		public CS_Progress_Event onProgress;
		public CS_Complete_Event onAllComplete;
		public CS_Event onCleanComplete;
		public CS_Event onError;

		public float currentFileProgress = 0;

		private CS_BinaryProgress_Event onBinaryProgress;

		private S3Transport s3;
		private List<string> downloadList;

		private CS_TextFile_Event onTextFileComplete;

		private CS_BinaryFile_Event onBinaryComplete;
		private CS_Internal_Event onErrorInternal;
		//		private FCMJ_Exist_Event onExistCheck;
		private bool isDownloading = false;
		private int _currentFileIndex = 0;
		public int _downloaded = 0, _error = 0, _total = 0, _cleaned=0;

		float benchMarkStart;

		void Start ()
		{

			localDataPath = Application.persistentDataPath;

			onTextFileComplete = new CS_TextFile_Event ();
			onErrorInternal = new CS_Internal_Event ();
			onBinaryComplete = new CS_BinaryFile_Event ();
			onBinaryProgress = new CS_BinaryProgress_Event ();

			onBinaryProgress.AddListener (OnBinaryProgressHandler);

			s3 = GetComponent<S3Transport> ();

		}

		// ----------------------------------------------------------
	
		// Manifest methods.
		
		public void BeginRetrieveManifest (ManifestProcessor processor = null)
		{
			// Begins process of getting manifest. Delete local s3 filelist so it'll be fetched anew.

			s3.EmptyRemoteFileList ();

			// The processor is a delegate that makes adjustments to the manifest before storing it locally. Ie. add lastmodified date.

			manifestProcessor = processor;

			if (isDownloading) {
				onError.Invoke ("Another download is active.");
				return;
			}

			LogMessage ("Begin downloading the manifest: " + ManifestName);
			isDownloading = true;

			// Try getting S3 filelist, if success, proceed to downloading manifest.

			if (!s3.FileListReady) {
				UnityEvent e = new UnityEvent ();
				e.AddListener (RetrieveManifest);
				s3.GetRemoteFileList (e, onError);
			} else {
				RetrieveManifest ();
			}

		}

		public void RetrieveManifest ()
		{

			// See if manifest exists on S3, if so, try downloading it.

			if (s3.FileExistsInRemote (ManifestName) == S3Transport.Response.TRUE) {

				// Get textfile (and date) async from s3, passing in io events to be called (which we'll remove again).

				onTextFileComplete.AddListener (onManifestCompleteHandler);
				onErrorInternal.AddListener (onManifestErrorHandler);

				LogMessage ("Downloading the manifest: " + ManifestName);
				s3.GetTextFileAndDate (ManifestName, onTextFileComplete, onErrorInternal);

			} else {
				
				onError.Invoke ("Manifest file missing from S3.");

			}

		}

		private void onManifestCompleteHandler (string data, long lastModified)
		{

			// Remove specific event listeners from generic io events.

			onTextFileComplete.RemoveListener (onManifestCompleteHandler);
			onErrorInternal.RemoveListener (onManifestErrorHandler);

			// Write file to the [data_path]/temp/content.json 

			Debug.Log ("Content last modified: " + System.DateTime.FromBinary (lastModified).ToString ());

			data = manifestProcessor (data, lastModified);
			WriteFileToTemp (data, ManifestName);

			isDownloading = false;

			// Invoke the public manifest ready event, which'll control what happens next.

			onManifestReceived.Invoke (data);

		}

		private void onManifestErrorHandler (string data)
		{
			
			// Remove specific event listeners from generic io events.

			onTextFileComplete.RemoveListener (onManifestCompleteHandler);
			onErrorInternal.RemoveListener (onManifestErrorHandler);

			isDownloading = false;

			onError.Invoke ("Downloading manifest failed.");

		}

		// ----------------------------------------------------------

		// File downloading methods.

		public void GetFilesByName (string[] fileList)
		{
			if (isDownloading) {
				onError.Invoke ("Another download is active.");
				return;
			}

			isDownloading = true;

			downloadList = new List<string> ();

			LogMessage ("Checking for existing local files.");

			foreach (string fileURL in fileList) {

				// Convert fileURL to file name

				string fileName = GetFileNameFromURL (fileURL);
				LogMessage ("Checking for: " + fileName);

				// Check if the file already exists

				if (!LocalFileExists (fileName)) {
					// Else add to download list
					downloadList.Add (fileURL);
					LogMessage (fileURL + " is added to the download list.");
				}
			}

			_total = downloadList.Count;
			_downloaded = 0;
			_currentFileIndex = 0;
			_error = 0;
				

			benchMarkStart = Time.time;

			// Check if we have an S3 filelist, if not get it first.

			if (!s3.FileListReady) {
				UnityEvent e = new UnityEvent ();
				e.AddListener (BeginProcessDownloadList);
				s3.GetRemoteFileList (e, onError);
			} else {
				BeginProcessDownloadList ();
			}

		}

		private void BeginProcessDownloadList () {

			// Set handlers (to be removed later) and begin downloading process.

			onBinaryComplete.AddListener (OnDownloadCompleteHandler);
			onErrorInternal.AddListener (OnDownloadErrorHandler);

			ProcessDownloadList ();

		}

		private void ProcessDownloadList ()
		{

			// Main file downloading process.

			if (_currentFileIndex >= downloadList.Count) {

				// Done. Delete s3 file list copy.

				s3.EmptyRemoteFileList ();

				// Remove event listeners.

				onBinaryComplete.RemoveListener (OnDownloadCompleteHandler);
				onErrorInternal.RemoveListener (OnDownloadErrorHandler);

				isDownloading = false;

				Debug.LogWarning ("DOWNLOAD TIME: " + (Time.time - benchMarkStart));

				// Invoke all complete event which'll control what happen next.

				onAllComplete.Invoke (_downloaded, _error);

				return;
			}

			// Start download via S3 for the currentFile

			LogMessage ("Downloading: " + downloadList [_currentFileIndex]);

			if (s3.FileExistsInRemote (downloadList [_currentFileIndex]) == S3Transport.Response.TRUE) {

				//s3.GetBinaryFile(downloadList[_currentFileIndex] , onBinaryComplete , onErrorInternal);
				s3.GetBinaryFileNonblock (downloadList [_currentFileIndex], onBinaryComplete, onErrorInternal, onBinaryProgress);


			} else {

				_error++;
				onProgress.Invoke (downloadList [_currentFileIndex], CS_Events.FILE_ERROR, _downloaded, _error);
				_currentFileIndex++;
				ProcessDownloadList ();

			}

		}


		private void OnDownloadCompleteHandler (byte[] data)
		{
			_downloaded++;
			onProgress.Invoke (downloadList [_currentFileIndex], CS_Events.FILE_COMPLETED, _downloaded, _error);
			
			// Write to file
			WriteFileToTemp (data, GetFileNameFromURL (downloadList [_currentFileIndex]));

			_currentFileIndex++;
			ProcessDownloadList ();
		}

		private void OnDownloadErrorHandler (string data)
		{
			LogMessage ("Error message: " + data);
			_error++;
			onProgress.Invoke (downloadList [_currentFileIndex], CS_Events.FILE_ERROR, _downloaded, _error);
			_currentFileIndex++;
			ProcessDownloadList ();
		}


		private void OnBinaryProgressHandler (float binaryProgress)
		{

			// This is called from the downloading coroutine to inform us about the current file's progress. currentFileProgress is a public float.

			currentFileProgress = binaryProgress;

		}

		private bool LocalFileExists (string fileName)
		{
			bool ExistsInContent = File.Exists (localDataPath + "/" + localStorageFolder + "/" + fileName);
			bool ExistsInTemp = File.Exists (localDataPath + "/" + localTempFolder + "/" + fileName);
			return ExistsInTemp || ExistsInContent;
		}

		private void WriteFileToTemp (string content, string fileName)
		{

			// Create the local storage directory if it does not exist

			if (!Directory.Exists (localDataPath + "/" + localTempFolder)) {
				Directory.CreateDirectory (localDataPath + "/" + localTempFolder);
			}

			// Write the text file with the given file name

			File.WriteAllText (localDataPath + "/" + localTempFolder + "/" + fileName, content);

		}

		private void WriteFileToTemp (byte[] content, string fileName)
		{

			// Create the local storage directory if it does not exist

			if (!Directory.Exists (localDataPath + "/" + localTempFolder)) {
				Directory.CreateDirectory (localDataPath + "/" + localTempFolder);
			}

			// Write the text file with the given file name

			File.WriteAllBytes (localDataPath + "/" + localTempFolder + "/" + fileName, content);

		}

		private void LogMessage (string message)
		{
			if (!ShowLog)
				return;

			Debug.Log (message);
		}

		private string GetFileNameFromURL (string fileURL)
		{
			return fileURL.Replace ("audio/", "").Replace ("video/", "").Replace ("icons/", "");
		}

		public bool TakeLive ()
		{
			// Move all files from temp to content.

			if (isDownloading) {
				onError.Invoke ("A download is (still?) active.");
				return false;
			}

			// Stop if Temp folder does not exist.

			if (!Directory.Exists (localDataPath + "/" + localTempFolder)) {
				onError.Invoke ("Temp folder does not exist");
				return false;
			}

			// Stop if Temp folder is empty.

			if (Directory.GetFiles (localDataPath + "/" + localTempFolder).Length <= 0) {
				onError.Invoke ("Temo folder is empty");
				return false;
			}

			// Create local content folder if it does not exist.

			if (!Directory.Exists (localDataPath + "/" + localStorageFolder)) {
				Directory.CreateDirectory (localDataPath + "/" + localStorageFolder);
			}

			// Get array of file names from the temp folder

			string[] files = Directory.GetFiles (localDataPath + "/" + localTempFolder);

			// Copy files to the local storage folder.

			foreach (string file in files) {
				string target = file.Replace (localTempFolder, localStorageFolder);
				LogMessage ("Copying " + file + " to " + target);
				File.Copy (file, target, true);
			}

			// Delete the local temp folder.

			Directory.Delete (localDataPath + "/" + localTempFolder, true);

			return true;
		}



		// ----------------------------------------------------------

		// File cleaning methods.

		private string[] filesToClean;


		public void CleanLocalStorage ()
		{

			Debug.Log ("Begin cleaning");

			// Deletes any local file that does not exist on the server anymore

			s3.EmptyRemoteFileList (); // force reload.

			if (isDownloading) {
				onError.Invoke ("A download is in progress.");
				return;
			}

			if (!s3.FileListReady) {
				UnityEvent e = new UnityEvent ();
				e.AddListener (Clean);
				s3.GetRemoteFileList (e, onError);
			} else {
				Clean ();
			}
		}

		private void Clean ()
		{
//			filesToClean = Directory.GetFiles (localDataPath + "/" + localStorageFolder);

			filesToClean = Directory.GetFiles (localDataPath + "/" + localStorageFolder);

			_total = filesToClean.Length;

			LogMessage (_total + " files to clean");

			if (_total <= 0) {
				onError.Invoke ("There are no files in the " + localStorageFolder + " folder");
				return;
			}

			for (int f = 0; f < _total; f++) {

				_cleaned = f;

				string filePath = filesToClean [f];
				string fileName =GetFileNameFromFullPath(filePath);

				Debug.Log("Cleaning file "+fileName+" at "+filePath);
					

				if (s3.FileExistsInRemote (fileName) == S3Transport.Response.FALSE) {

					Debug.Log ("Delete " + filePath);

					try {
					//	File.Delete (localDataPath + "/" + localStorageFolder + "/" +  (fileName));
						File.Delete (filePath);

					} catch (Exception e) {
						Debug.Log (e.Message);
					}
				}


			}

//			foreach (string fileName in filesToClean) {
//				if (s3.FileExistsInRemote (GetFileNameFromFullPath (fileName)) == S3Manager.Response.FALSE) {
//					Debug.Log ("Delete " + fileName);
//					try {
//						File.Delete (localDataPath + "/" + localStorageFolder + "/" + GetFileNameFromFullPath (fileName));
//					} catch (Exception e) {
//						Debug.Log (e.Message);
//					}
//				}
//			}

			onCleanComplete.Invoke ("Cleaning complete");

			s3.EmptyRemoteFileList ();

		}

		//		private void OnFileExistCheckEventHandler (string fileName, bool doesExist)
		//		{
		//
		//			if (!doesExist) {
		//				LogMessage ("File doesn't exist: " + fileName);
		////				File.Delete(localDataPath + "/" + localStorageFolder + "/" + fileName);
		//			} else {
		//				LogMessage ("File exists: " + fileName);
		//			}
		//		
		//		}
		//
		//
		//		private void OnFileExistEventHandler (string fileName, bool doesExist)
		//		{
		//			
		//			if (!doesExist) {
		//				
		//				if (fileName != ".DS_Store") {
		//
		//					LogMessage ("Deleting " + fileName);
		//					File.Delete (localDataPath + "/" + localStorageFolder + "/" + fileName);
		//
		//				} 
		//
		//			} else {
		//				LogMessage ("File exists: " + fileName);
		//			}
		//			filesToCleanIndex++;
		//			Clean ();
		//		}

		private string GetFileNameFromFullPath (string filePath)
		{
			filePath = filePath.Replace("\\","/");

			return filePath.Replace (localDataPath + "/" + localStorageFolder + "/", "");
		}
		

	}
}


