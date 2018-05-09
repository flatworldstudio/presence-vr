using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CloudStorage;
//using JimEngine;

public class SampleSceneManager : MonoBehaviour {

	public CloudStorageClient client;

	// Function linked to the UI button
	// This starts the entire flow
	public void GetContent(){
		client.BeginRetrieveManifest();
	}

	// This function is called when the content file is received
	public void OnContentReceivedHandler(string data){
		// If this event is called it means the content file is received successfully
		// and saved to [data_path]/content/content.json
		// For this event the message contains the file content for content.json
		Debug.Log(data);

        // Here needs the function to get the content file and spit out the list of files to download
        // For debug purpose below is a hard coded version of this string array with the test files


        //JIMDATA.populateFromJson (data);

        //string[] files = JIMDATA.GetAllFiles ();

        //		Debug.Log (files.ToString());

        //		string[] test_files = {"audio/test_audio.mp3" , "video/test_video.mp4" , "icons/test_icon.png"};



        string[] files = new string[0];


		// Call to get the files in the string array
		client.GetFilesByName(files);
	}

	// This function is called when there is an error with S3
	// Error message is in the message
	public void OnErrorHandler(string data){
		Debug.Log(data);
	}

	// This function is called with every file progress
	public void OnProgressHandler(string filename , string status , int downloaded, int failed){
		// Name of the file just finished downloading or failed
		Debug.Log(filename);

		// Status what happened to the file (complete or error)
		Debug.Log(status);

		// How many files are downloaded so far
		Debug.Log(downloaded);

		// How many files have failed so far
		Debug.Log(failed);
	}

	// This function is called when download of all files are finished
	public void OnAllCompleteHandler(int downloaded, int failed){
		Debug.Log("All completed");
		// How many files are downloaded
		Debug.Log(downloaded);
		// How many files have failed so far
		Debug.Log(failed);

		if(downloaded > 0){
			// Take temp folder to live
			client.TakeLive();
		}

	}

	public void CleanLocalStorage(){
		client.CleanLocalStorage();
	}

	public void OnCleanCompleteHandler(string data){
		Debug.Log("Cleaning complete.");
	}
}
