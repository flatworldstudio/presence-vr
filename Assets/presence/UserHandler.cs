using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserHandler : MonoBehaviour
{

	public UserController userController;
	UxInterface overviewInterface, viewInterface;

	public GameObject uxCanvas;

	public GameObject overviewObject, viewerObject,headSet;





	string me = "Task handler: ";

	UxController uxController;

	void Start ()
	{

		userController.addTaskHandler (TaskHandler);

		uxController = new UxController ();


		#if UNITY_IOS

		Input.compass.enabled = true;

		PRESENCE.mobileInitialHeading = Input.compass.magneticHeading;

		#endif




	

//		viewerInterface.camera.cameraReference.SetActive (false);

		/*
		overviewInterface.camera.cameraReference.SetActive (true);


		UiConstraint simpleConstraint = new UiConstraint ();

		simpleConstraint.edgeSprings = true;
		simpleConstraint.edgeSpringMin = new Vector3 (0, 0);
		simpleConstraint.edgeSpringMax = new Vector3 (0, 0);

		simpleConstraint.hardClamp = true;
		simpleConstraint.hardClampMin = new Vector3 (-100, -50);
		simpleConstraint.hardClampMax = new Vector3 (100, 50);

		GameObject menu = GameObject.Find ("menu");

		UiButton but = new UiButton ("record", menu, simpleConstraint);
		but.callback = "recorddepth";
		overviewInterface.uiButtons.Add ("record", but);
		viewerInterface.uiButtons.Add ("record", but);


		but = new UiButton ("stop", menu, simpleConstraint);
		but.callback = "stopdepth";
		overviewInterface.uiButtons.Add ("stop", but);
		viewerInterface.uiButtons.Add ("stop", but);


		but = new UiButton ("play", menu, simpleConstraint);
		but.callback = "playdepth";
		overviewInterface.uiButtons.Add ("play", but);
		viewerInterface.uiButtons.Add ("play", but);

*/


	}

	private static Quaternion GyroToUnity(Quaternion q)
	{

		return new Quaternion(q.x, q.y, -q.z, -q.w);
		//		return new Quaternion(q.y, -q.x, q.z, q.w);

	}

//	static readonly Quaternion baseIdentity =  Quaternion.Euler(90, 0, 0);
	static readonly Quaternion landscapeLeft =  Quaternion.Euler(0, 0, -90);
	static readonly Quaternion baseIdentity =  Quaternion.Euler(90, 0, 0);

	public bool TaskHandler (StoryTask task)
	{
		
		bool done = false;

		switch (task.description) {





		case "createview":

		viewInterface = new UxInterface ();

		UxMapping uxMap = new UxMapping ();

		#if UNITY_EDITOR || UNITY_STANDALONE

		uxMap.ux_none += UxMethods.none;


		uxMap.ux_tap_2d += UxMethods.highlightButton2d;
		uxMap.ux_tap_3d += UxMethods.select3dObject;
		uxMap.ux_tap_none += UxMethods.clearSelectedObjects;
		uxMap.ux_tap_none += UxMethods.stopControls;

		uxMap.ux_single_2d += UxMethods.drag2d;
		uxMap.ux_single_3d += UxMethods.rotateCamera;
		uxMap.ux_single_none += UxMethods.rotateCamera;

		uxMap.ux_double_2d += UxMethods.drag2d;
		uxMap.ux_double_3d += UxMethods.panCamera;
		uxMap.ux_double_3d += UxMethods.zoomCamera;
		uxMap.ux_double_none += UxMethods.panCamera;
		uxMap.ux_double_none += UxMethods.zoomCamera;

		viewInterface.defaultUxMap = uxMap;

		viewInterface.camera = new UxCamera (viewerObject);
		viewInterface.camera.control = CAMERACONTROL.TURN;
		viewInterface.camera.constraint = new UiConstraint ();

		#endif

		#if UNITY_IOS

		uxMap.ux_none += UxMethods.rotateCamera;

		uxMap.ux_tap_2d += UxMethods.highlightButton2d;
		uxMap.ux_tap_3d += UxMethods.select3dObject;
		uxMap.ux_tap_none += UxMethods.clearSelectedObjects;
		uxMap.ux_tap_none += UxMethods.stopControls;

		uxMap.ux_single_2d += UxMethods.drag2d;
		uxMap.ux_single_3d += UxMethods.panCamera;
		uxMap.ux_single_none += UxMethods.panCamera;

		uxMap.ux_double_2d += UxMethods.drag2d;
		uxMap.ux_double_3d += UxMethods.zoomCamera;
		uxMap.ux_double_none += UxMethods.zoomCamera;

			viewInterface.defaultUxMap = uxMap;

			viewInterface.camera = new UxCamera (viewerObject);
			viewInterface.camera.control = CAMERACONTROL.GYRO;
			viewInterface.camera.constraint = new UiConstraint ();


		#endif

		viewInterface.canvasObject = uxCanvas;




		done = true;

		break;






	


		case "createoverview":



			overviewInterface = new UxInterface ();



			UxMapping overviewMap = new UxMapping ();

			overviewMap.ux_none += UxMethods.none;

			overviewMap.ux_tap_2d += UxMethods.highlightButton2d;
			overviewMap.ux_tap_3d += UxMethods.none;
			overviewMap.ux_tap_none += UxMethods.none;

			overviewMap.ux_single_2d += UxMethods.drag2d;
			overviewMap.ux_single_3d += UxMethods.rotateCamera;
			overviewMap.ux_single_none += UxMethods.rotateCamera;

			overviewMap.ux_double_2d += UxMethods.drag2d;
			overviewMap.ux_double_3d += UxMethods.panCamera;

			overviewMap.ux_double_3d += UxMethods.zoomCamera;

			overviewMap.ux_double_none += UxMethods.panCamera;
			overviewMap.ux_double_none += UxMethods.zoomCamera;

			overviewInterface.defaultUxMap = overviewMap;

			overviewInterface.camera = new UxCamera (overviewObject);
			overviewInterface.camera.control = CAMERACONTROL.ORBIT;
			overviewInterface.camera.constraint = new UiConstraint ();

			overviewInterface.canvasObject = uxCanvas;




			done = true;

			break;


		case "createoverviewdebug":

			GameObject newNullObject = DebugObject.getNullObject (0.1f, 0.1f, 0.2f);
			newNullObject.transform.parent = GameObject.Find ("overviewObject").transform;
			newNullObject.transform.localPosition = Vector3.zero;
			newNullObject.transform.localRotation = Quaternion.identity;

			 newNullObject = DebugObject.getNullObject (0.05f, 0.05f, 0.1f);
			newNullObject.transform.parent = GameObject.Find ("overviewInterest").transform;
			newNullObject.transform.localPosition = Vector3.zero;
			newNullObject.transform.localRotation = Quaternion.identity;
		


			done = true;

			break;


		case "createcallibrationdebug":

			newNullObject = DebugObject.getNullObject (1f, 1f, 1f);
			newNullObject.name = "northnull";
			newNullObject.transform.parent = GameObject.Find ("SetHandler").transform;
			newNullObject.transform.localPosition = Vector3.zero;
			newNullObject.transform.localRotation = Quaternion.identity;


			newNullObject = DebugObject.getNullObject (1f, 1f, 1f);
			newNullObject.name = "kinectnull";
			newNullObject.transform.parent = GameObject.Find ("Kinect").transform;
			newNullObject.transform.localPosition = Vector3.zero;
			newNullObject.transform.localRotation =  Quaternion.identity;

			newNullObject = DebugObject.getNullObject (1f, 1f, 1f);
			newNullObject.name = "compassoffsetnull";
			newNullObject.transform.parent = GameObject.Find ("SetHandler").transform;
			newNullObject.transform.localPosition = Vector3.zero;
			newNullObject.transform.localRotation =  Quaternion.identity;

			newNullObject = DebugObject.getNullObject (1f, 1f, 1f);
			newNullObject.name = "viewernull";
			newNullObject.transform.parent = viewerObject.transform;
			newNullObject.transform.localPosition = Vector3.zero;
			newNullObject.transform.localRotation =  Quaternion.identity;


			done = true;

			break;


		case "callibratekinect":


			GameObject k = GameObject.Find ("Kinect");

			Vector3 p = k.transform.position;

			p.y = PRESENCE.kinectHeight;

			k.transform.position = p;

			k.transform.localRotation = Quaternion.Euler (0, PRESENCE.kinectHeading, 0);


			p = PRESENCE.kinectHomeDistance * (k.transform.localRotation * Vector3.forward);

			p.y = 1.8f;

			viewerObject.transform.localPosition = p;
			viewerObject.transform.localRotation = Quaternion.Euler (0, PRESENCE.kinectHeading+180f, 0);



			done = true;

			break;
		


		case "overview":

			uxController.update (overviewInterface);


			break;


		case "view":

			uxController.update (viewInterface);


			break;

		case "syncviewer":

			#if OSX || WIN

			Quaternion deviceRotation;
			float mobileInitialHeading;

			if (task.getQuaternionValue("deviceRotation", out deviceRotation)){
				headSet.transform.localRotation = deviceRotation;

			}

			if (task.getFloatValue("mobileInitialHeading", out mobileInitialHeading)){
				viewerObject.transform.localRotation = Quaternion.Euler (0, Mathf.Rad2Deg * mobileInitialHeading, 0);
//			task.setStringValue("debug",""+PRESENCE.mobileInitialHeading);


			}





			#endif

			#if IOS

			task.setQuaternionValue("deviceRotation",headSet.transform.rotation);
			task.setFloatValue ("mobileInitialHeading",PRESENCE.mobileInitialHeading);

			#endif




			break;

		case "gyro":
			
			Input.gyro.enabled = true;

			

			Quaternion attitude = baseIdentity * GyroToUnity (Input.gyro.attitude);

			Vector3 z = attitude * Vector3.forward;



			float angle = Mathf.Atan2 (z.x, z.z);


//			float gyroYaw = gyroAttitude.eulerAngles.y;


			string gyroOn = Input.gyro.enabled ? "on " : "off ";


		
			task.setStringValue ("debug", gyroOn + " a: " + angle+ "z: "+ z.ToString());





			break;


		case "callibrateheadset":

//			Input.compass.enabled = true;
//
//			float compassYaw = Input.compass.magneticHeading;
//			string compassOn = Input.compass.enabled ? "on " : "off ";
//
//			viewerObject.transform.localRotation = Quaternion.Euler (0, Mathf.Rad2Deg * compassYaw);
//			task.setFloatValue("initialHeading"

			done = true;

			break;

		case "compass":


			Input.compass.enabled = true;

			float compassYaw=Input.compass.magneticHeading;
			string compassOn = Input.compass.enabled ? "on " : "off ";

			task.setStringValue ("debug", compassOn + compassYaw);




			break;

		case "userview":

			if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL ) {
				GENERAL.STORYMODE = STORYMODE.OVERVIEW;


			}

			if (GENERAL.AUTHORITY == AUTHORITY.LOCAL) {
				GENERAL.STORYMODE = STORYMODE.VIEWER;


			}



			if (GENERAL.STORYMODE == STORYMODE.VIEWER) {

				viewInterface.camera.cameraReference.SetActive (true);

				overviewInterface.camera.cameraReference.SetActive (false);


//				uxController.update (viewerInterface);

				// get overviewer
			
				Quaternion viewrotation;
				Vector3 viewinterest;
				float viewzoom;

				if (task.getQuaternionValue ("viewrotation", out viewrotation) && task.getVector3Value ("viewinterest", out viewinterest) && task.getFloatValue ("viewzoom", out viewzoom)) {

					overviewInterface.camera.cameraInterest.transform.localRotation = viewrotation;
					overviewInterface.camera.cameraInterest.transform.position = viewinterest;
					Vector3 temp = overviewInterface.camera.cameraObject.transform.localPosition;
					temp.z = viewzoom;
					overviewInterface.camera.cameraObject.transform.localPosition = temp;

				}


				// send viewer

				Quaternion userrotation = viewInterface.camera.cameraInterest.transform.localRotation;
				Vector3 userinterest = viewInterface.camera.cameraInterest.transform.position;
				float userzoom = viewInterface.camera.cameraObject.transform.localPosition.z;

				task.setQuaternionValue ("userrotation", userrotation);
				task.setVector3Value ("userinterest", userinterest);
				task.setFloatValue ("userzoom", userzoom);

				string callBackName = uxController.update (viewInterface);

				if (!callBackName.Equals ("")) {

					task.setCallBack (callBackName);

					//				Debug.Log("calling callback " +callBack);

				}



//				Debug.Log ("quat " + userrotation.ToString());


//				Vector3 forward =userrotation * Vector3.forward;
//				Vector3 up = userrotation * Vector3.up;
//				Vector3 right = userrotation * Vector3.right;
//
//				Debug.Log ("f " + forward.ToString () + "u " + up.ToString () + "r " + right.ToString ());


			}


			if (GENERAL.STORYMODE == STORYMODE.OVERVIEW) {

				viewInterface.camera.cameraReference.SetActive (false);

				overviewInterface.camera.cameraReference.SetActive (true);


				// get viewer

				Quaternion userrotation;
				Vector3 userinterest;
				float userzoom;

				if (task.getQuaternionValue ("userrotation", out userrotation) && task.getVector3Value ("userinterest", out userinterest) && task.getFloatValue ("userzoom", out userzoom)) {
									
					viewInterface.camera.cameraInterest.transform.localRotation = userrotation;
					viewInterface.camera.cameraInterest.transform.position = userinterest;
					Vector3 temp = viewInterface.camera.cameraObject.transform.localPosition;
					temp.z = userzoom;
					viewInterface.camera.cameraObject.transform.localPosition = temp;

				}

				// send overviewer

				Quaternion viewrotation = overviewInterface.camera.cameraInterest.transform.localRotation;
				Vector3 viewinterest = overviewInterface.camera.cameraInterest.transform.position;
				float viewzoom = overviewInterface.camera.cameraObject.transform.localPosition.z;

				task.setQuaternionValue ("viewrotation", viewrotation);
				task.setVector3Value ("viewinterest", viewinterest);
				task.setFloatValue ("viewzoom", viewzoom);

				//

				string callBackName = uxController.update (overviewInterface);

				if (!callBackName.Equals ("")) {

					task.setCallBack (callBackName);

					//				Debug.Log("calling callback " +callBack);

				}
			}

			break;


		default:

			done = true;

			break;

		}

		return done;

	}

	void Update ()
	{
				
	}

}
