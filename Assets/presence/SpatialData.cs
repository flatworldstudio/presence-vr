
using System;
using UnityEngine;
using System.Collections;




public static class SpatialData {


	static string me = "Spatial data: ";

	// KINECT INFO
	static public bool centered = true;

static	public  GameObject kinectObject;
	static	public  Vector3 kinectPosition;
	static	public  Quaternion kinectRotation;

	static	public bool live = false;

	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	static	public GameObject km;

	static	public KinectManager kinectManager;

	#endif




	public static void SetActive (bool state){
		
		if (state) {

			#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

			 km = GameObject.Find( "KinectManager");
			kinectManager=km.GetComponent<KinectManager>();
			Debug.Log(me + "Waking up Kinectmanager.");

			kinectManager.WakeUp();

			if (kinectManager.IsInitialized()){
				Debug.Log(me + "Kinectmanager initialised.");

			}else{

				Debug.LogWarning(me + "Kinectmanager not initialised.");
			}

			#endif

		} else {

			#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

			km = GameObject.Find( "KinectManager");
			kinectManager=km.GetComponent<KinectManager>();

			kinectManager.shutDown();

			Debug.Log(me + "Shutting down Kinectmanager.");

			#endif

		}

//		kinectObject=obj;

	

	}

	public static  bool IsLive (){

		live = false;

		#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

		live = kinectManager!=null ? kinectManager.IsInitialized() : false;

		#else


		Debug.LogError (me + "Unable to run live kinect on non windows platform.");


		#endif

		return live;

	}

	public static void InitDummy (GameObject obj){

		kinectObject=obj;
		live = false;

	}

	public static Vector3 getJoint (uint playerID, int joint){

		Vector3 posJoint = Vector3.zero;


		if (live) {

			#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN


			bool MirroredMovement = true;

			posJoint = kinectManager.GetJointPosition (playerID, joint);

			posJoint.z = !MirroredMovement ? -posJoint.z : posJoint.z;


			posJoint.y += PRESENCE.kinectHeight;

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







