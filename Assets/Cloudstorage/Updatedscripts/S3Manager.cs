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

namespace FMCJ{
	public class S3Manager : MonoBehaviour {

		public enum Response{
			TRUE,
			FALSE,
			ERROR
		}

		public string IdentityPoolId = "eu-central-1:c4445541-69c9-4f63-90ef-55442b6728b6";
		public string CognitoIdentityRegion = RegionEndpoint.EUCentral1.SystemName;
		private RegionEndpoint _CognitoIdentityRegion
		{
			get { return RegionEndpoint.GetBySystemName(CognitoIdentityRegion); }
		}
		public string S3Region = RegionEndpoint.EUWest2.SystemName;
		private RegionEndpoint _S3Region
		{
			get { return RegionEndpoint.GetBySystemName(S3Region); }
		}
		public string S3BucketName = null;
		
		private IAmazonS3 _s3Client;
		private AWSCredentials _credentials;

		private List<string> RemoteFiles;

		public bool FileListReady{
			get {
				return (RemoteFiles != null);
			}
		}

		private AWSCredentials Credentials
		{
			get
			{
				if (_credentials == null)
					_credentials = new CognitoAWSCredentials(IdentityPoolId, _CognitoIdentityRegion);
				return _credentials;
			}
		}

		

		private IAmazonS3 Client
		{
			get
			{
				if (_s3Client == null)
				{
					_s3Client = new AmazonS3Client(Credentials, _S3Region);
				}
				//test comment
				return _s3Client;
			}
		}
		void Start () {
			UnityInitializer.AttachToGameObject(this.gameObject);
			AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
		}
		
		public void GetTextFile(string fileName , FCMJ_Internal_Event completeEvent, FCMJ_Internal_Event errorEvent)
        {

            Client.GetObjectAsync(S3BucketName, fileName, (responseObj) =>
            {
                string data = null;
                var response = responseObj.Response;

				if(responseObj.Exception != null){
					errorEvent.Invoke(responseObj.Exception.Message);
					return;
				}

                if (response.ResponseStream != null)
                {
                    using (StreamReader reader = new StreamReader(response.ResponseStream))
                    {
                        data = reader.ReadToEnd();
                    }

					completeEvent.Invoke(data);

                } else {

					errorEvent.Invoke("Cannot extablish connection.");

				}
            });
        }

		public void GetBinaryFile(string fileName , FCMJ_BinaryFile_Event completeEvent, FCMJ_Internal_Event errorEvent)
        {

            Client.GetObjectAsync(S3BucketName, fileName, (responseObj) =>
            {
                byte[] data = null;
                var response = responseObj.Response;
				Stream input = response.ResponseStream;

				if(responseObj.Exception != null){
					errorEvent.Invoke(responseObj.Exception.Message);
					return;
				}

                if (response.ResponseStream != null)
                {
					byte[] buffer = new byte[16 * 1024];
					using (MemoryStream ms = new MemoryStream())
					{
						int read;
						while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
						{
							ms.Write(buffer, 0, read);
						}
						data = ms.ToArray();
					}

					completeEvent.Invoke(data);

                } else {

					errorEvent.Invoke("Cannot extablish connection.");

				}
            });
        }

		// Gets the remote list of the files on the S3 and stores in local variable
		public void GetRemoteFileList(UnityEvent successEvent, FCMJ_Event errorEvent){
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
					
					successEvent.Invoke();
				}
				else
				{
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
	}
}