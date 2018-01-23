using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserHandler : MonoBehaviour
{

	public UserController userController;
	UxInterface overviewInterface, viewerInterface;

	public GameObject uxCanvas;

	public GameObject camA_object, camB_object;





	string me = "Task handler: ";

	UxController uxController;

	void Start ()
	{

		userController.addTaskHandler (TaskHandler);

		uxController = new UxController ();

		viewerInterface = new UxInterface ();

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

		viewerInterface.defaultUxMap = uxMap;

		viewerInterface.camera = new UxCamera (camB_object);
		viewerInterface.camera.control = CAMERACONTROL.TURN;
		viewerInterface.camera.constraint = new UiConstraint ();

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

		viewerInterface.defaultUxMap = uxMap;

		viewerInterface.camera = new UxCamera (camB_object);
		viewerInterface.camera.control = CAMERACONTROL.GYRO;
		viewerInterface.camera.constraint = new UiConstraint ();



		#endif

		viewerInterface.canvasObject = uxCanvas;




		overviewInterface = new UxInterface ();

		uxMap = new UxMapping ();

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

//		uxMap.ux_double_3d += UxMethods.moveCamera;
		uxMap.ux_double_3d += UxMethods.zoomCamera;

		uxMap.ux_double_none += UxMethods.panCamera;
//		uxMap.ux_double_none += UxMethods.moveCamera;
		uxMap.ux_double_none += UxMethods.zoomCamera;



		overviewInterface.defaultUxMap = uxMap;

		overviewInterface.camera = new UxCamera (camA_object);
		overviewInterface.camera.control = CAMERACONTROL.ORBIT;
		overviewInterface.camera.constraint = new UiConstraint ();

		overviewInterface.canvasObject = uxCanvas;

		viewerInterface.camera.cameraReference.SetActive (false);
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




	}

	public bool TaskHandler (StoryTask task)
	{
		
		bool done = false;

		switch (task.description) {

		case "userview":

			if (GENERAL.SCOPE == SCOPE.GLOBAL || GENERAL.SCOPE == SCOPE.SOLO ) {
				GENERAL.STORYMODE = STORYMODE.OVERVIEW;


			}

			if (GENERAL.SCOPE == SCOPE.LOCAL) {
				GENERAL.STORYMODE = STORYMODE.VIEWER;


			}



			if (GENERAL.STORYMODE == STORYMODE.VIEWER) {

				viewerInterface.camera.cameraReference.SetActive (true);

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

				Quaternion userrotation = viewerInterface.camera.cameraInterest.transform.localRotation;
				Vector3 userinterest = viewerInterface.camera.cameraInterest.transform.position;
				float userzoom = viewerInterface.camera.cameraObject.transform.localPosition.z;

				task.setQuaternionValue ("userrotation", userrotation);
				task.setVector3Value ("userinterest", userinterest);
				task.setFloatValue ("userzoom", userzoom);

				string callBackName = uxController.update (viewerInterface);

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

				viewerInterface.camera.cameraReference.SetActive (false);

				overviewInterface.camera.cameraReference.SetActive (true);


				// get viewer

				Quaternion userrotation;
				Vector3 userinterest;
				float userzoom;

				if (task.getQuaternionValue ("userrotation", out userrotation) && task.getVector3Value ("userinterest", out userinterest) && task.getFloatValue ("userzoom", out userzoom)) {
									
					viewerInterface.camera.cameraInterest.transform.localRotation = userrotation;
					viewerInterface.camera.cameraInterest.transform.position = userinterest;
					Vector3 temp = viewerInterface.camera.cameraObject.transform.localPosition;
					temp.z = userzoom;
					viewerInterface.camera.cameraObject.transform.localPosition = temp;

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
