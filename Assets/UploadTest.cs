using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CloudStorage;

public class UploadTest : MonoBehaviour {
    S3Transport s3;

	// Use this for initialization
	void Start () {
        s3 = GetComponent<S3Transport>();
	}
	
    public void GetList(){

        s3.GetRemoteFileList(null,null);

    }
	// Update is called once per frame
	void Update () {
		
	}
}
