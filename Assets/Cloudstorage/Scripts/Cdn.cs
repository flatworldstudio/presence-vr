using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//using  JimEngine;


namespace CloudStorage
{
	
	public class Cdn : MonoBehaviour
	{

		public CloudStorageClient client;

		float progress;
		public bool cdnProcessAborted = false;
		public bool cdnProcessCompleted = false;
		public bool cleanupCompleted =false;

		int fileDownloads = 0;
		float fileProgress = 0;
		string me = "CDN: ";

		// This starts the entire flow

		public void BeginUpdate ()
		{
		
			cdnProcessAborted = false;
			cdnProcessCompleted = false;

			client.BeginRetrieveManifest (AddDateToManifest);

		}

		public void OnManifestReceivedHandler (string data)
		{

			Debug.Log (me+ "Manifest received successfully, retrieving files.");

			//JIMDATA.populateFromJson (data);

			//string[] files = JIMDATA.GetAllFiles ();
            string[] files = new string[0];
			client.GetFilesByName (files);

		}

		// This function is called when there is an error with S3
		// Error message is in the message
		public void OnErrorHandler (string data)
		{


//			if (data == "contentfailed") {
			
				cdnProcessAborted = true;

//			}

			Debug.LogError ("Process failed: "+data);

		}

		// This function is called with every file progress
		public void OnProgressHandler (string filename, string status, int downloaded, int failed)
		{
		
			// Name of the file just finished downloading or failed
			Debug.Log (filename);

			// Status what happened to the file (complete or error)
			Debug.Log (status);

			// How many files are downloaded so far
			Debug.Log (downloaded);

			// How many files have failed so far
			Debug.Log (failed);

		}

		// This function is called when download of all files are finished

		public void OnAllCompleteHandler (int downloaded, int failed)
		{
		
			Debug.Log ("All completed");
			// How many files are downloaded
			Debug.Log (downloaded);
			// How many files have failed so far
			Debug.Log (failed);

			if (failed == 0) {
			
				Debug.Log ("Taking live.");

				// Take temp folder to live.
				if (client.TakeLive ()) {

					cdnProcessCompleted = true;

				} else {
					
					// TakeLive aborts before moving anything. So fallback content should still be ok.

					cdnProcessAborted = true;
				}


			} else {

				cdnProcessAborted = true;

			}

		}


		private string AddDateToManifest (string data, long lastModified)
		{

            //JIMDATA.populateFromJson (data);
            //JIMDATA.StoryData.EngineInfo.LastModified = lastModified;

            //return JsonUtility.ToJson (JIMDATA.StoryData);

            return data;
		}

		public float GetFileProgress ()
		{

			return client.currentFileProgress;
		}

		public float GetCleanupProgress()
		{

			if (client._total > 0) {
				
				return client._cleaned / client._total;

			} else {
				
				return 0;

			}

		}

		public string GetProgressReport ()
		{
			if (client._total > 0) {
				
				int count = client._downloaded + client._error + 1;
				if (count > client._total)
					count--;

				return "Updating " + count + " of " + client._total;

			} 

			return "Updating";


		}

		public void CleanLocalStorage(){

			cdnProcessAborted = false;
			cdnProcessCompleted = false;

//			cleanupCompleted =false;
			client.CleanLocalStorage();
		}

		public void OnCleanCompleteHandler(string data){
			
			cdnProcessCompleted =true;

			Debug.Log("Cleaning complete.");
		}

	}

}