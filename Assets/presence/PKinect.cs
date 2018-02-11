
using System;
using UnityEngine;
using System.Collections;




public  class PKinect {

	// Wrapper to hold or simulate a Kinect.

	string me = "PKinect: ";

	// KINECT INFO
	public bool centered = true;

	public  GameObject kinectObject;

	public  Vector3 kinectPosition;
	public  Quaternion kinectRotation;

	public bool live = false;

	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

	public KinectManager kinectManager;

	#endif



	public PKinect (){



	}


	public void InitLive (GameObject obj){

		kinectObject=obj;

		#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

		if (kinectObject==null){
			Debug.LogWarning (me + "Object is null, returning.");
			return;
		}
		GameObject kinectManagerObject = obj.transform.Find("KinectManager").gameObject;

		kinectManagerObject.SetActive (true);

		kinectManager=kinectManagerObject.GetComponent<KinectManager>();

		Debug.Log(me + "Activating Kinectmanager.");

		#else


		Debug.LogError (me + "Unable to run live kinect on non windows platform.");

		#endif


	}

	public bool IsLive (){

		live = false;

		#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

		live = kinectManager!=null ? kinectManager.IsInitialized() : false;

		#else


		Debug.LogError (me + "Unable to run live kinect on non windows platform.");


		#endif

		return live;

	}

	public void InitDummy (GameObject obj){

		kinectObject=obj;
		live = false;

	}

	public Vector3 getJoint (uint playerID, int joint){

		Vector3 posJoint = Vector3.zero;


		if (live) {

			#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN


			bool MirroredMovement = true;

			posJoint = kinectManager.GetJointPosition (playerID, joint);

			posJoint.z = !MirroredMovement ? -posJoint.z : posJoint.z;


		

			if (MirroredMovement) {
				posJoint.x = -posJoint.x;
			}

			#endif

			if (!centered) {

				posJoint = kinectRotation * posJoint;
				posJoint += new Vector3 (kinectPosition.x,0,kinectPosition.z);

		//		posJoint.y -= PRESENCE.kinectHeight; // correct for sensorheigh because kinect takes it into account
			}



			return posJoint;

		} 


		return posJoint;





	}




}







