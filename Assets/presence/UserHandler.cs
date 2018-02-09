using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserHandler : MonoBehaviour
{

	public UserController userController;
	UxInterface overviewInterface, viewInterface;

	public GameObject uxCanvas;

	public GameObject overviewObject, viewerObject, headSet, setObject, handl,handr;





	string me = "Task handler: ";

	UxController uxController;

	void Start ()
	{

		userController.addTaskHandler (TaskHandler);

		uxController = new UxController ();


		#if UNITY_IOS

		// Callibration: rotate headset so that north is always north.

		Input.compass.enabled = true;

//		PRESENCE.mobileInitialHeading1 = Input.compass.magneticHeading;
//		PRESENCE.mobileInitialHeading = Input.compass.magneticHeading;

//		viewerObject.transform.parent.transform.localRotation = Quaternion.Euler (0, -1f* PRESENCE.mobileInitialHeading, 0);
	

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

	private static Quaternion GyroToUnity (Quaternion q)
	{

		return new Quaternion (q.x, q.y, -q.z, -q.w);
		//		return new Quaternion(q.y, -q.x, q.z, q.w);

	}

	//	static readonly Quaternion baseIdentity =  Quaternion.Euler(90, 0, 0);
	static readonly Quaternion landscapeLeft = Quaternion.Euler (0, 0, -90);
	static readonly Quaternion baseIdentity = Quaternion.Euler (90, 0, 0);

	public bool TaskHandler (StoryTask task)
	{
		
		bool done = false;

		switch (task.description) {



		case "createviewvr":
			PRESENCE.isOverview = false;
			viewInterface = new UxInterface ();

			UxMapping uxMap = new UxMapping ();


			uxMap.ux_none += UxMethods.none;


			uxMap.ux_tap_2d += UxMethods.highlightButton2d;
			uxMap.ux_tap_3d += UxMethods.select3dObject;
			uxMap.ux_tap_none += UxMethods.clearSelectedObjects;
			uxMap.ux_tap_none += UxMethods.stopControls;

			uxMap.ux_single_2d += UxMethods.drag2d;
			uxMap.ux_single_3d += UxMethods.none;
			uxMap.ux_single_none += UxMethods.none;

			uxMap.ux_double_2d += UxMethods.drag2d;
			uxMap.ux_double_3d += UxMethods.none;
			uxMap.ux_double_none += UxMethods.none;

			viewInterface.defaultUxMap = uxMap;

			viewInterface.camera = new UxCamera (viewerObject);

			viewInterface.camera.control = CAMERACONTROL.VOID;

			viewInterface.camera.constraint = new UiConstraint ();

			viewInterface.canvasObject = uxCanvas;


			done = true;
			break;


		case "createview":

			PRESENCE.isOverview = false;


			viewInterface = new UxInterface ();

			uxMap = new UxMapping ();

		

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


		#else 

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

			viewInterface.canvasObject = uxCanvas;




			done = true;

			break;






	


		case "createoverview":


			PRESENCE.isOverview = true;


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
			newNullObject.transform.localRotation = Quaternion.identity;

			newNullObject = DebugObject.getNullObject (1f, 1f, 1f);
			newNullObject.name = "compassoffsetnull";
			newNullObject.transform.parent = GameObject.Find ("SetHandler").transform;
			newNullObject.transform.localPosition = Vector3.zero;
			newNullObject.transform.localRotation = Quaternion.identity;

			newNullObject = DebugObject.getNullObject (1f, 1f, 1f);
			newNullObject.name = "viewernull";
			newNullObject.transform.parent = viewerObject.transform;
			newNullObject.transform.localPosition = Vector3.zero;
			newNullObject.transform.localRotation = Quaternion.identity;


			done = true;

			break;


		
		


		case "overview":

			uxController.update (overviewInterface);


			break;


		case "view":

			uxController.update (viewInterface);


			break;

		case "syncviewer":

			#if SERVER

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


			Quaternion headRotation;



//			#if IOS && !UNITY_EDITOR
//
//			headRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);
//
//			#endif
//
//
//			#if IOS && UNITY_EDITOR
//
////			headRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);
//
//			headRotation = headSet.transform.rotation;
//
//
//			#endif

			#if CLIENT

			headRotation = headSet.transform.rotation;

			task.setQuaternionValue("headRotation",headRotation);

//			task.setFloatValue ("mobileInitialHeading",PRESENCE.mobileInitialHeading);

			Vector3 dp;

			if (task.getVector3Value("devicePosition",out dp))
			{
				viewerObject.transform.localPosition=dp;
			}
				


//			task.setStringValue("debug",""+PRESENCE.mobileInitialHeading);

			#endif




			break;

		case "gyro":
			
			Input.gyro.enabled = true;

			

			Quaternion attitude = baseIdentity * GyroToUnity (Input.gyro.attitude);

			Vector3 z = attitude * Vector3.forward;



			float angle = Mathf.Atan2 (z.x, z.z);


//			float gyroYaw = gyroAttitude.eulerAngles.y;


			string gyroOn = Input.gyro.enabled ? "on " : "off ";


		
			task.setStringValue ("debug", gyroOn + " a: " + angle + "z: " + z.ToString ());





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

			#if UNITY_IOS

		case "compass":


	//		Input.compass.enabled = true;


			if (Input.compass.rawVector.magnitude != 0) {

				// wait for a reading.

				PRESENCE.mobileInitialHeading = Input.compass.magneticHeading;

				viewerObject.transform.parent.transform.localRotation = Quaternion.Euler (0,  PRESENCE.mobileInitialHeading, 0);

				done = true;

			}

//			float compassYaw=Input.compass.magneticHeading;
//			string compassOn = Input.compass.enabled ? "on " : "off ";
//
//			task.setStringValue ("debug", compassOn + compassYaw);



			break;

			#endif

		case "interfaceactive":

			if (PRESENCE.isOverview) {

				float comp;
				float head;

				if (task.getFloatValue("compass", out comp) && task.getFloatValue("headyaw", out head)){

				//	Debug.Log (me+"compass value: "+ comp);
					PRESENCE.vrHeadOffset =  head-comp;

					float vel = 0;

					PRESENCE.vrHeadOffset = Mathf.SmoothDamp (PRESENCE.vrHeadOffset, head - comp, ref vel, 0.1f);

					viewerObject.transform.parent.transform.localRotation = Quaternion.Euler (0,  PRESENCE.vrHeadOffset, 0);

				


				
				}



			


				KinectManager manager = KinectManager.Instance;
					

				// get 1st player
				uint playerID = manager != null ? manager.GetPlayer1ID () : 0;

				if (playerID >= 0) {


				//	bool MirroredMovement = true;
				//	Quaternion initialRotation = Quaternion.identity;

					// set the user position in space
				//	Vector3 posPointMan = manager.GetUserPosition (playerID);
				//	posPointMan.z = !MirroredMovement ? -posPointMan.z : posPointMan.z;

			//		int joint = 3; // head


				//	Vector3 posJoint = manager.GetJointPosition (playerID, joint);
				//	posJoint.z = !MirroredMovement ? -posJoint.z : posJoint.z;


				//	posJoint.y -= PRESENCE.kinectHeight; // correct for sensorheigh because kinect takes it into account

		//	//		if (MirroredMovement) {
	//				posJoint.x = -posJoint.x;
	//		}





				//	Vector3

					// project from kinect

			//		Vector3 projected = PRESENCE.kinectRotation * posJoint;
			//		projected += PRESENCE.kinectPosition;



					viewerObject.transform.parent.transform.position = getJoint(playerID,3); // head

					handl.transform.position = getJoint (playerID, 7);
					handr.transform.position = getJoint (playerID, 11);



				}













//				task.setQuaternionValue ("setOrientation", setObject.transform.localRotation);

		task.setQuaternionValue ("viewerOrientation", viewerObject.transform.parent.transform.localRotation);


				task.setVector3Value ("viewerPosition", viewerObject.transform.parent.transform.position);
				task.setVector3Value ("hlPosition", handl.transform.position);
				task.setVector3Value ("hrPosition",handr.transform.position);



				string callBackName = uxController.update (overviewInterface);

				if (!callBackName.Equals ("")) {

					task.setCallBack (callBackName);

				}


			}

			if (!PRESENCE.isOverview) {


				float compassHeading = Input.compass.magneticHeading;
				float headYaw =  headSet.transform.localRotation.eulerAngles.y;



				task.setFloatValue ("compass", compassHeading);

				task.setFloatValue ("headyaw", headYaw);

				task.setStringValue ("debug", "c: " + compassHeading + " h: " + headYaw + " d: " +( headYaw - compassHeading));


				Quaternion viewerOrientationQ;

				if (task.getQuaternionValue ("viewerOrientation", out viewerOrientationQ)) {

					viewerObject.transform.parent.transform.localRotation = viewerOrientationQ;


				}

				Vector3 viewerPositionV;

				if (task.getVector3Value ("viewerPosition", out viewerPositionV)) {

					viewerObject.transform.parent.transform.localPosition = viewerPositionV;


				}

				Vector3 hl;

				if (task.getVector3Value ("hlPosition", out hl)) {

					handl.transform.localPosition = hl;


				}

				Vector3 hr;

				if (task.getVector3Value ("hrPosition", out hr)) {

					handr.transform.localPosition = hr;


				}



//				Debug.Log ("viewer");
				
				string callBackName = uxController.update (viewInterface);

				if (!callBackName.Equals ("")) {

					task.setCallBack (callBackName);

				}

			}


			break;

		case "interfaceactive2":






			if (!PRESENCE.isOverview) {

				// get testrotation



				// get overviewer

				/*
			
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


*/

				string callBackName = uxController.update (viewInterface);

				if (!callBackName.Equals ("")) {

					task.setCallBack (callBackName);

					//				Debug.Log("calling callback " +callBack);

				}


			}


			if (PRESENCE.isOverview) {


				// send testrotation


				// get viewer

				/*

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
*/
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

	Vector3 getJoint (uint playerID, int joint){

		KinectManager manager = KinectManager.Instance;
		bool MirroredMovement = true;

		Vector3 posJoint = manager.GetJointPosition (playerID, joint);
		posJoint.z = !MirroredMovement ? -posJoint.z : posJoint.z;


		posJoint.y -= PRESENCE.kinectHeight; // correct for sensorheigh because kinect takes it into account

		if (MirroredMovement) {
			posJoint.x = -posJoint.x;
		}





		//	Vector3

		// project from kinect

		Vector3 projected = PRESENCE.kinectRotation * posJoint;
		projected += PRESENCE.kinectPosition;

		return projected;

	}

	void Update ()
	{
				
	}

}
