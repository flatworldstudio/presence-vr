using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using System.IO;
using System;
using Amazon.S3.Util;
using Amazon.CognitoIdentity;
using Amazon;

namespace CloudStorage
{
	public class S3Transport : MonoBehaviour
	{

		public enum Response{
			TRUE,
			FALSE,
			ERROR
		}

		private List<string> RemoteFiles;

		public string IdentityPoolId = "eu-central-1:c4445541-69c9-4f63-90ef-55442b6728b6";
		public string CognitoIdentityRegion = RegionEndpoint.EUCentral1.SystemName;

		private RegionEndpoint _CognitoIdentityRegion {
			get { return RegionEndpoint.GetBySystemName (CognitoIdentityRegion); }
		}

		public string S3Region = RegionEndpoint.EUWest2.SystemName;

		private RegionEndpoint _S3Region {
			get { return RegionEndpoint.GetBySystemName (S3Region); }
		}

		public string S3BucketName = null;
		
		private IAmazonS3 _s3Client;
		private AWSCredentials _credentials;

		private AWSCredentials Credentials {
			get {
				if (_credentials == null)
					_credentials = new CognitoAWSCredentials (IdentityPoolId, _CognitoIdentityRegion);
				return _credentials;
			}
		}


		

		private IAmazonS3 Client {
			get {
				if (_s3Client == null) {
					_s3Client = new AmazonS3Client (Credentials, _S3Region);
				}
				//test comment
				return _s3Client;
			}
		}

		void Start ()
		{
			UnityInitializer.AttachToGameObject (this.gameObject);
			AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;


		}

		public void GetTextFile (string fileName, CS_Internal_Event completeEvent, CS_Internal_Event errorEvent)
		{
            
			Client.GetObjectAsync (S3BucketName, fileName, (responseObj) => {
				string data = null;
				var response = responseObj.Response;

				if (responseObj.Exception != null) {
					errorEvent.Invoke (responseObj.Exception.Message);
					return;
				}

				if (response.ResponseStream != null) {
					using (StreamReader reader = new StreamReader (response.ResponseStream)) {
						data = reader.ReadToEnd ();
					}

					completeEvent.Invoke (data);

				} else {

					errorEvent.Invoke ("Cannot establish connection.");

				}
			});
		}

		public void GetTextFileAndDate (string fileName, CS_TextFile_Event completeEvent, CS_Internal_Event errorEvent)
		{



//			GetFileList ();

			Client.GetObjectAsync (S3BucketName, fileName, (responseObj) => {

				string data = null;
				var response = responseObj.Response;

				long lastModified = responseObj.Response.LastModified.ToUniversalTime().ToBinary();

				if (responseObj.Exception != null) {
					errorEvent.Invoke (responseObj.Exception.Message);
					return;
				}

				if (response.ResponseStream != null) {
					using (StreamReader reader = new StreamReader (response.ResponseStream)) {
						data = reader.ReadToEnd ();
					}

					completeEvent.Invoke (data,lastModified);

				} else {

					errorEvent.Invoke ("Cannot establish connection.");

				}
			});
		}

		public void GetBinaryFile (string fileName, CS_BinaryFile_Event completeEvent, CS_Internal_Event errorEvent, CS_BinaryProgress_Event downloadEvent)
		{

			Client.GetObjectAsync (S3BucketName, fileName, (responseObj) => {

//					Debug.Log("file size: "+responseObj.Response.Headers.ContentLength);
				float fileSize = responseObj.Response.Headers.ContentLength;

				byte[] data = null;
				var response = responseObj.Response;
				Stream input = response.ResponseStream;

				if (responseObj.Exception != null) {
					errorEvent.Invoke (responseObj.Exception.Message);
					return;
				}

				if (response.ResponseStream != null) {
					byte[] buffer = new byte[16 * 1024];
					using (MemoryStream ms = new MemoryStream ()) {
						int read;
						int totalRead = 0;
						while ((read = input.Read (buffer, 0, buffer.Length)) > 0) {
							totalRead += read;
//							Debug.Log (totalRead/fileSize);
							downloadEvent.Invoke (totalRead / fileSize);
							ms.Write (buffer, 0, read);
						}
						data = ms.ToArray ();
//							Debug.Log(totalRead);
					}

					completeEvent.Invoke (data);

				} else {

					errorEvent.Invoke ("Cannot establish connection.");

				}
			});
		}

		public void GetBinaryFileNonblock (string fileName, CS_BinaryFile_Event completeEvent, CS_Internal_Event errorEvent, CS_BinaryProgress_Event downloadEvent)
		{

			Client.GetObjectAsync (S3BucketName, fileName, (responseObj) => {

				//					Debug.Log("file size: "+responseObj.Response.Headers.ContentLength);
			
			

				if (responseObj.Exception != null) {
					errorEvent.Invoke (responseObj.Exception.Message);
					return;
				}

				var response = responseObj.Response;

				if (response.ResponseStream != null) {

					StartCoroutine (PullData (response, completeEvent, downloadEvent));
						
				

				} else {

					errorEvent.Invoke ("Cannot establish connection.");

				}
			});
		}



		IEnumerator PullData (GetObjectResponse responseObj, CS_BinaryFile_Event completeEvent, CS_BinaryProgress_Event downloadEvent)
		{

//			GetObjectResponse
			float fileSize = responseObj.Headers.ContentLength;

//			float fileSize = responseObj.Response.Headers.ContentLength;

//			responseObj.ResponseStream
//			var response = responseObj.Response;

			byte[] data = null;

			Stream input = responseObj.ResponseStream;

			byte[] buffer = new byte[16 * 1024];

			using (MemoryStream ms = new MemoryStream ()) {
				int read;
				int totalRead = 0;



				while ((read = input.Read (buffer, 0, buffer.Length)) > 0) {
					totalRead += read;
//												Debug.Log (totalRead/fileSize);
					downloadEvent.Invoke (totalRead / fileSize);
					ms.Write (buffer, 0, read);
				
						yield return null;

				}
				data = ms.ToArray ();
				//							Debug.Log(totalRead);
			}

			completeEvent.Invoke (data);





		}

		// Gets the remote list of the files on the S3 and stores in local variable
		public void GetRemoteFileList(UnityEvent successEvent, CS_Event errorEvent){

			Debug.Log ("Retrieving S3 file list.");

			RemoteFiles = new List<string>();
			var request = new ListObjectsRequest()
			{
				BucketName = S3BucketName
			};
			Client.ListObjectsAsync(request, (responseObject) =>
				{
					if (responseObject.Exception == null)
					{
						responseObject.Response.S3Objects.ForEach((o) =>
							{
								RemoteFiles.Add(o.Key);
							});

                    Debug.Log("Retrieved file list.");

						successEvent.Invoke();
					}
					else
					{
                    Debug.Log("Fail. "+responseObject.Exception.Message);
						errorEvent.Invoke(responseObject.Exception.Message);
					}
				});


		}

		public void EmptyRemoteFileList(){
			RemoteFiles = null;
		}

		// Checks if file exist on S3
		public Response FileExistsInRemote(string fileName)
		{

			if(RemoteFiles == null) return Response.ERROR;

			// Check against the file locally stored RemoteFile list
			if(RemoteFiles.Contains("video/" + fileName) || RemoteFiles.Contains("audio/" + fileName) || RemoteFiles.Contains("icons/" + fileName)|| RemoteFiles.Contains(fileName)){
				return Response.TRUE;
			} else {
				return Response.FALSE;
			}

		}

		public bool FileListReady{
			get {
				return (RemoteFiles != null);
			}
		}





	}
}