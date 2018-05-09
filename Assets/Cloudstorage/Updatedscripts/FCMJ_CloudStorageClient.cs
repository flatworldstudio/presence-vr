using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System;

namespace FMCJ{
	public class FCMJ_CloudStorageClient : MonoBehaviour {

		public string JSONFileName = "content.json";
		public string localStorageFolder = "content";
		public string localTempFolder = "_temp";
		public bool ShowLog = true;

		public FCMJ_Event onContentReceived;
		public FCMJ_Progress_Event onProgress;
		public FCMJ_Complete_Event onAllComplete;
		public FCMJ_Event onCleanComplete;
		public FCMJ_Event onError;
		
		private S3Manager s3;
		private List<string> downloadList;
		private FCMJ_Internal_Event onTextComplete;
		private FCMJ_BinaryFile_Event onBinaryComplete;
		private FCMJ_Internal_Event onErrorInternal;
		private bool isDownloading = false;
		private int _downloaded = 0, _error = 0 , _currentFileIndex = 0;

		void Start () {
			onTextComplete = new FCMJ_Internal_Event();
			onErrorInternal = new FCMJ_Internal_Event();
			onBinaryComplete = new FCMJ_BinaryFile_Event();

			s3 = GetComponent<S3Manager>();
		}
		
		public void GetContent(){
			if(isDownloading) {
				onError.Invoke("Another download is in progress.");
				return;
			}

			LogMessage("Downloadings the " + JSONFileName + " file.");

			isDownloading = true;
			onTextComplete.AddListener(onContentComplete);
			onErrorInternal.AddListener(onContentError);

			// Try to get config file from S3
			s3.GetTextFile(JSONFileName , onTextComplete , onErrorInternal);
		}

		private void onContentComplete(string data){
			// Remove event listeners
			onTextComplete.RemoveListener(onContentComplete);
			onErrorInternal.RemoveListener(onContentError);

			// Write file to the [data_path]/content/content.json
			WriteFile(data , JSONFileName);
			isDownloading = false;
			onContentReceived.Invoke(data);
		}

		private void onContentError(string data){
			// Remove event listeners
			onTextComplete.RemoveListener(onContentComplete);
			onErrorInternal.RemoveListener(onContentError);

			isDownloading = false;
			onError.Invoke(data);
		}

		public void TakeLive(){
			// Stop if another download is in progress
			if(isDownloading) {
				onError.Invoke("There is a download in progress.");
				return;
			}

			// Stop if Temp folder does not exist
			if(!Directory.Exists(Application.dataPath + "/" + localTempFolder)) {
				onError.Invoke("Temo folder does not exist");
				return;
			}

			// Stop if Temp folder is empty
			if(Directory.GetFiles(Application.dataPath + "/" + localTempFolder).Length <= 0) {
				onError.Invoke("Temo folder is empty");
				return;
			}

			// Create content folder is does not exist
			// Create the local storage directory if it does not exist
			if(!Directory.Exists(Application.dataPath + "/" + localStorageFolder)){
				Directory.CreateDirectory(Application.dataPath + "/" + localStorageFolder);
			}

			// Get array of file names from the temp folder
			string[] files = Directory.GetFiles(Application.dataPath + "/" + localTempFolder);

			// Copy files to the local storage folder
			foreach(string file in files){
				string target = file.Replace(localTempFolder , localStorageFolder);
				LogMessage("Copying " + file + " to " + target);
				File.Copy(file , target , true);
			}

			// Delete local temp folder
			Directory.Delete(Application.dataPath + "/" + localTempFolder , true);
		}

		public void GetFilesByName(string[] fileList){
			if(isDownloading) {
				onError.Invoke("Another download is in progress.");
				return;
			}

			isDownloading = true;

			downloadList = new List<string>();

			LogMessage("Checking for existing file");
			foreach(string fileURL in fileList){
				// Convert fileURL to file name
				string fileName = GetFileNameFromURL(fileURL);
				LogMessage("checking for: " + fileName);
				// Check if the file already exists
				if(!FileExists(fileName)){
					// Else add to download list
					downloadList.Add(fileURL);
					LogMessage(fileURL + " is added to the download list.");
				}
			}

			_downloaded = 0;

			_downloaded = 0;
			_currentFileIndex = 0;
			_error = 0;

			onBinaryComplete.AddListener(OnDownloadCompleteHandler);
			onErrorInternal.AddListener(OnDownloadErrorHandler);

			if(!s3.FileListReady){
				UnityEvent e = new UnityEvent();
				e.AddListener(ProcessDownloadList);
				s3.GetRemoteFileList(e , onError);
			} else {
				ProcessDownloadList();
			}
		}

		private void OnDownloadCompleteHandler(byte[] data){
			_downloaded++;
			onProgress.Invoke(downloadList[_currentFileIndex] , FCMJ_Events.FILE_COMPLETED , _downloaded , _error);
			
			// Write to file
			WriteFile(data , GetFileNameFromURL(downloadList[_currentFileIndex]));

			_currentFileIndex++;
			ProcessDownloadList();
		}

		private void OnDownloadErrorHandler(string data){
			LogMessage("Error message: " + data);
			_error++;
			onProgress.Invoke(downloadList[_currentFileIndex] , FCMJ_Events.FILE_ERROR , _downloaded , _error);
			_currentFileIndex++;
			ProcessDownloadList();
		}

		private void ProcessDownloadList(){
			if(_currentFileIndex >= downloadList.Count){

				// Remove event listeners
				onBinaryComplete.RemoveListener(OnDownloadCompleteHandler);
				onErrorInternal.RemoveListener(OnDownloadErrorHandler);

				isDownloading = false;

				// Invoke all complete event
				onAllComplete.Invoke(_downloaded , _error);
				s3.EmptyRemoteFileList();
				return;
			}

			// Start download via S3 for the currentFile
			LogMessage("Start the downloading: " + downloadList[_currentFileIndex]);
			if(s3.FileExistsInRemote(downloadList[_currentFileIndex]) == S3Manager.Response.TRUE){

				s3.GetBinaryFile(downloadList[_currentFileIndex] , onBinaryComplete , onErrorInternal);

			} else {

				_error++;
				onProgress.Invoke(downloadList[_currentFileIndex] , FCMJ_Events.FILE_ERROR , _downloaded , _error);
				_currentFileIndex++;
				ProcessDownloadList();

			}
		}

		private bool FileExists(string fileName){
			return File.Exists(Application.dataPath + "/" + localStorageFolder + "/" + fileName);
		}

		private void WriteFile(string content , string fileName){

			// Create the local storage directory if it does not exist
			if(!Directory.Exists(Application.dataPath + "/" + localTempFolder)){
				Directory.CreateDirectory(Application.dataPath + "/" + localTempFolder);
			}

			// Write the text file with the given file name
			File.WriteAllText(Application.dataPath + "/" + localTempFolder + "/" + fileName , content);

		}

		private void WriteFile(byte[] content , string fileName){

			// Create the local storage directory if it does not exist
			if(!Directory.Exists(Application.dataPath + "/" + localTempFolder)){
				Directory.CreateDirectory(Application.dataPath + "/" + localTempFolder);
			}

			// Write the text file with the given file name
			File.WriteAllBytes(Application.dataPath + "/" + localTempFolder + "/" + fileName , content);

		}

		private void LogMessage(string message){
			if(!ShowLog) return;

			Debug.Log(message);
		}

		private string GetFileNameFromURL(string fileURL){
			return fileURL.Replace("audio/","").Replace("video/","").Replace("icons/","");
		}

		private string[] filesToClean;

		// Deletes any local file that does not exist on the server anymore
		public void CleanLocalStorage(){
			if(isDownloading) {
				onError.Invoke("A download is in progress.");
				return;
			}

			if(!s3.FileListReady){
				UnityEvent e = new UnityEvent();
				e.AddListener(Clean);
				s3.GetRemoteFileList(e , onError);
			} else {
				Clean();
			}
		}

		private void Clean(){
			filesToClean = Directory.GetFiles(Application.dataPath + "/" + localStorageFolder);

			LogMessage(filesToClean.Length + " files to clean");

			if(filesToClean.Length <= 0) {
				onError.Invoke("There are no files in the " + localStorageFolder + " folder");
				return;
			}

			foreach(string fileName in filesToClean){
				if(s3.FileExistsInRemote(GetFileNameFromFullPath(fileName)) == S3Manager.Response.FALSE){
					Debug.Log("Delete " + fileName);
					try{
						File.Delete(Application.dataPath + "/" + localStorageFolder + "/" + GetFileNameFromFullPath(fileName));
					} catch (Exception e){
						Debug.Log(e.Message);
					}
				}
			}

			onCleanComplete.Invoke("Cleaning complete");
			s3.EmptyRemoteFileList();
		}

		private string GetFileNameFromFullPath(string filePath){
			return filePath.Replace(Application.dataPath + "/" + localStorageFolder + "/" , "");
		}
		

	}
}


