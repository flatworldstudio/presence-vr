using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;


// ------------------------------------------------------------------------------------------------------------------------------------------

// data objects

public enum UITOUCH
{
	BEGAN,
	TOUCHING,
	ENDED,
	NONE
}

public enum UIACTION
{
	// describes the action outcome. note that this is independent of UITOUCH which describes actual touch state.

	SINGLEDRAG,
//	INERTIA,
	DOUBLEDRAG,
	TAP,
	VOID,
	DELETE
}


//public enum UIRESULT
//{
//	NONE,
//	ADDNEW,
//	REMOVEME
//
//}

public class UiEvent
{
	// holds user interaction description. note that x and y are NOT continuously updated for touch.

	public float dx, dy, dd, x, y, d, px, py;
	public bool firstFrame;

	//			public bool touchState;

	public UIACTION action;
	public UITOUCH touch;
	public GameObject target2D, target3D;
	public UiButton targetButton;



	public bool isInert, isSpringing;



	public float tapCount;
	//			public bool inertia;
	public string callback;

	public UiEvent ()
	{
		target2D = null;
		target3D = null;
		targetButton = null;
		px = 0;
		py = 0;
		firstFrame = true;
		action = UIACTION.VOID;
		touch = UITOUCH.NONE;
		isInert = false;
		isSpringing = false;
		tapCount = 0;

	}

	public UiEvent clone ()
	{
				
		UiEvent result = new UiEvent ();
		result.dx = this.dx;
		result.dy = this.dy;
		result.dd = this.dd;
		result.x = this.x;
		result.y = this.y;
		result.d = this.d;
		result.px = this.px;
		result.py = this.py;

		result.firstFrame = this.firstFrame;
		result.action = this.action;
		result.touch = this.touch;
		result.target2D = this.target2D;
		result.target3D = this.target3D;
		result.targetButton = this.targetButton;
		result.isInert = this.isInert;
		result.isSpringing = this.isSpringing;
		result.tapCount = this.tapCount;
		return result;
	}

	public string toString ()
	{
		string result = "UI event: ";

		switch (action) {
		case UIACTION.SINGLEDRAG:
			result += "single ";
			break;
		case UIACTION.DOUBLEDRAG:
			result += "double ";
			break;
		case UIACTION.TAP:
			result += "tap ";
			break;

		case UIACTION.VOID:
		default:
			result += "void ";
			break;
		}

		result += "dx: " + dx + " dy: " + dy + " dd: " + dd;
		if (target2D != null) {
			result += " 2d: " + target2D.transform.name;
		}

//				r += " x: " + x + " y: " + y + " dd: " + dd;
//
//				if (target2D != null) {
//					r += " 2d: " + target2D.transform.name;
//				}
//				if (target3D != null) {
//					r += " 3d: " + target3D.transform.name;
//				}
		return result;
	}
}



public static class UxMethods {



	public static void someAction (object sender, UxArgs uxArgs)
	{

		Debug.Log ("SOME ACTION TRIGGERRED");
	}


	public static void clearSelectedObjects (object sender, UxArgs uxArgs)
	{
		foreach (GameObject go in uxArgs.activeInterface.selectedObjects) {

			go.GetComponent<MeshRenderer> ().material = uxArgs.activeInterface.defaultMat;

		}
		uxArgs.activeInterface.selectedObjects.Clear ();
	}

	public static void select3dObject (object sender, UxArgs uxArgs)
	{

		Debug.Log ( "3d target: " + uxArgs.uiEvent.target3D.transform.name);

		if (uxArgs.activeInterface.selectedObjects.IndexOf (uxArgs.uiEvent.target3D) != -1) {

			// object was selected, deselect
			uxArgs.uiEvent.target3D.GetComponent<MeshRenderer> ().material = uxArgs.activeInterface.defaultMat;
			uxArgs.activeInterface.selectedObjects.Remove (uxArgs.uiEvent.target3D);

		} else {

			// object was not selected, select
			uxArgs.uiEvent.target3D.GetComponent<MeshRenderer> ().material = uxArgs.activeInterface.editMat;
			uxArgs.activeInterface.selectedObjects.Add (uxArgs.uiEvent.target3D);

		}

	}

	public static void highlightButton2d (object sender, UxArgs uxArgs)
	{


		string name = uxArgs.uiEvent.target2D.transform.name;



		setTargetBrightness (uxArgs.activeInterface.getButtonNames(), 0.5f,uxArgs.activeInterface);

		setTargetBrightness (name, 1f,uxArgs.activeInterface);

//		UiButton theButton;
//		uxArgs.activeInterface.uiButtons.TryGetValue (name, out theButton);
//


		uxArgs.uiEvent.callback = uxArgs.uiEvent.targetButton.callback; 



//		uxArgs.uiEvent.callback = theButton.callback; // to be retrieved from button object..
//
//		Debug.Log ("tap callback: " + theButton.callback);
	}


	public static void stopControls (object sender, UxArgs uxArgs)
	{
//		uxArgs.uiEvent.callback = "stopControls";


		setTargetBrightness (uxArgs.activeInterface.getButtonNames(), 1f,uxArgs.activeInterface);


	}

	 static void setTargetBrightness (string name, float value,UxInterface activeInterface	)
	{
		UiButton theButton;
		activeInterface.uiButtons.TryGetValue (name, out theButton);

		if (theButton != null)
			theButton.targetBrightness = value;
	}

	static void setTargetBrightness (string[] names, float value, UxInterface activeInterface)
	{
		foreach (string name in names)
			setTargetBrightness (name, value,activeInterface);
	}

	static bool applyGUISprings (GameObject target, UiConstraint constraint)
	{
		// apply springs to gui object. returns true if it did anything.
		bool result = false;
		Vector3 anchor = target.GetComponent<RectTransform> ().anchoredPosition;

		if (constraint.edgeSprings) {
			result = true;
			float eVel0 = 0;
			float eVel1 = 0;
			float eVel2 = 0;
			float eVel3 = 0;

			if (anchor.x < constraint.edgeSpringMin.x) {
				anchor.x = Mathf.SmoothDamp (anchor.x, constraint.edgeSpringMin.x, ref eVel0, 0.15f); 
			} else if (anchor.x > constraint.edgeSpringMax.x) {
				anchor.x = Mathf.SmoothDamp (anchor.x, constraint.edgeSpringMax.x, ref eVel1, 0.15f); 
			}

			if (anchor.y < constraint.edgeSpringMin.y) {
				anchor.y = Mathf.SmoothDamp (anchor.y, constraint.edgeSpringMin.y, ref eVel2, 0.15f); 
			} else if (anchor.y > constraint.edgeSpringMax.y) {
				anchor.y = Mathf.SmoothDamp (anchor.y, constraint.edgeSpringMax.y, ref eVel3, 0.15f); 
			}

			float maxVel = Mathf.Max (Mathf.Abs (eVel0), Mathf.Abs (eVel1), Mathf.Abs (eVel2), Mathf.Abs (eVel3));

			if (maxVel < 1f) {
				// maxvel 0 implies springs not actually doing anything, so return should be false
				result = false;

			}
		}

		if (constraint.springs) {
			result = true;
			int closestSpring = -1;
			float closest = 9999999999f;

			for (int i = 0; i < constraint.springPositions.Length; i++) {
				// find nearest spring
				Vector3 theSpringPosition = constraint.springPositions [i];
				float distance = Vector2.Distance (anchor, theSpringPosition);
				if (distance < closest) {
					closest = distance;
					closestSpring = i;
				}
			}

			float eVel0 = 0;
			float eVel1 = 0;

			anchor.x = Mathf.SmoothDamp (anchor.x, constraint.springPositions [closestSpring].x, ref eVel0, 0.15f); 
			anchor.y = Mathf.SmoothDamp (anchor.y, constraint.springPositions [closestSpring].y, ref eVel1, 0.15f); 

			float maxVel = Mathf.Max (Mathf.Abs (eVel0), Mathf.Abs (eVel1));

			//				float maxVel = Mathf.Max (eVel0, eVel1);
			//				if (maxVel > 0f && maxVel < 1f) {

			if (maxVel < 1f) {
				// maxvel 0 implies springs not actually doing anything, so return should be false
				result = false;
			}
		}
		target.GetComponent<RectTransform> ().anchoredPosition = anchor;

		return result;
	}

//	void panGUI (GameObject target, Vector2 delta)
//	{
//		// pan gui without constraints.
//		panGUI (target, delta, emptyConstraint, emptyUiEvent);
//	}

	static void panGUI (GameObject target, Vector2 delta, UiConstraint constraint, UiEvent ui)
	{
		Vector3 anchor = target.GetComponent<RectTransform> ().anchoredPosition;
		anchor.y += delta.y;
		anchor.x += delta.x;

		if (constraint.hardClamp) {
			if (anchor.x < constraint.hardClampMin.x) {
				anchor.x = constraint.hardClampMin.x;
				ui.dx = 0;

			} else if (anchor.x > constraint.hardClampMax.x) {
				anchor.x = constraint.hardClampMax.x;
				ui.dx = 0;

			}
			if (anchor.y < constraint.hardClampMin.y) {
				anchor.y = constraint.hardClampMin.y;
				ui.dy = 0;

			} else if (anchor.y > constraint.hardClampMax.y) {
				anchor.y = constraint.hardClampMax.y;
				ui.dy = 0;
			}
		}

		if (constraint.radiusClamp) {
			Vector2 relativePosition = new Vector2 (anchor.x, anchor.y) - constraint.anchor;

			float magnitude = relativePosition.magnitude;

			magnitude = Mathf.Clamp (magnitude, constraint.radiusClampMin, constraint.radiusClampMax);

			relativePosition = relativePosition.normalized * magnitude;

			//				relativePosition = Vector2.ClampMagnitude (relativePosition, constraint.radiusClampMax);

			anchor = constraint.anchor + relativePosition;

		}


		target.GetComponent<RectTransform> ().anchoredPosition = anchor;

//		if (ui.action == UIACTION.INERTIA) {


//						if (ui.isInert || ui.isSpringing) {
			if (ui.isSpringing) {
				

			bool springing = applyGUISprings (target, constraint);

			if (ui.isSpringing && springing == false) {
				//					Debug.Log ("Springing stopped");
				ui.isSpringing = false;
			}


			// only apply springs when user not touching. reset 2d target only when springs have put object in place. 
			//				if (applyGUISprings (target, constraint)) {
			////					ui.target2D = null;
			////					ui.action = UIACTION.DELETE;
			//					ui.isSpringing
			//					Debug.Log ("DELETE ME");
			//				}
		}
	}

//	static public  void orbitCamera (UxInterface state, Vector2 delta)

	static public void rotateCamera (object sender, UxArgs uxArgs)
	{
		
		if (uxArgs.activeInterface.camera.control == CAMERACONTROL.ORBIT) {

			orbitCamera (sender, uxArgs);

		}

		if (uxArgs.activeInterface.camera.control == CAMERACONTROL.TURN) {

			turnCamera (sender, uxArgs);

		}

		if (uxArgs.activeInterface.camera.control == CAMERACONTROL.GYRO) {

			gyroCamera (sender, uxArgs);

		}

	}

	static readonly Quaternion baseIdentity =  Quaternion.Euler(90, 0, 0);
	static readonly Quaternion landscapeRight =  Quaternion.Euler(0, 0, 90);
	static readonly Quaternion landscapeLeft =  Quaternion.Euler(0, 0, -90);
	static readonly Quaternion upsideDown =  Quaternion.Euler(0, 0, 180);

	static public void gyroCamera (object sender, UxArgs uxArgs)
	{

		GameObject co, ci;
		co = uxArgs.activeInterface.camera.cameraObject;
		ci =  uxArgs.activeInterface.camera.cameraInterest;

		Vector3 startPosition = co.transform.position;

		Quaternion gyro = GyroToUnity(Input.gyro.attitude);

//		Debug.Log ("gyro " + gyro.ToString());

//		ci.transform.rotation = gyro * Quaternion.Euler (0, 0, -90); // landscape left
		ci.transform.rotation = baseIdentity * gyro;

//		Quaternion landscapeLeft =  Quaternion.Euler(0, 0, -90);

//		Debug.Log ("gyroToUnity " + ci.transform.rotation.ToString());

//		ci.transform.rotation = Input.gyro.attitude;

		Vector3 endPosition = co.transform.position;

		ci.transform.position -= (endPosition - startPosition);



//		Vector3 forward = ci.transform.rotation * Vector3.forward;
//		Vector3 up = ci.transform.rotation * Vector3.up;
//		Vector3 right = ci.transform.rotation * Vector3.right;
//
//		Debug.Log ("f " + forward.ToString () + "u " + forward.ToString () + "r " + forward.ToString ());

	}



	private static Quaternion GyroToUnity(Quaternion q)
	{

		return new Quaternion(q.x, q.y, -q.z, -q.w);
//		return new Quaternion(q.y, -q.x, q.z, q.w);

	}



	static public void turnCamera (object sender, UxArgs uxArgs)

	{

		// Create direct references for ease of use.

		GameObject co, ci;
		co = uxArgs.activeInterface.camera.cameraObject;
		ci =  uxArgs.activeInterface.camera.cameraInterest;


		if (ci != null && co != null) {

			Vector3 startPosition = co.transform.position;


			// we need a gameobject to be the interest, and a gameobject that holds the camera.

			float degreesPerPixel = 360f / Screen.height;
			float distanceToInterest = -1f * co.transform.localPosition.z;

			// delta.x -> YAW. rotate around (world) y axis.
			Quaternion orientation = co.transform.rotation;
			Quaternion yawRotation = Quaternion.AngleAxis (uxArgs.delta.x * degreesPerPixel, Vector3.up);
			orientation = yawRotation * orientation;

			// delta.y -> PITCH. rotate around (relative) x axis 
			Vector3 cameraDirection = co.transform.rotation * Vector3.forward;
			Vector3 pitchAxis = Vector3.Cross (cameraDirection, Vector3.up);
			Quaternion pitchRotation = Quaternion.AngleAxis (uxArgs.delta.y * degreesPerPixel, pitchAxis);
			orientation = pitchRotation * orientation;

			// neutralise roll, clamp pitch, leave yaw unchanged
			Vector3 euler = orientation.eulerAngles;

			euler.z = 0; 

			if (euler.x > 70 && euler.x <= 180) {
				euler.x = 70;
			}
			if (euler.x < 290 && euler.x > 180) {
				euler.x = 290;
			}


			if (false) {
					

			}

			orientation = Quaternion.Euler (euler);

			ci.transform.rotation = orientation;

			Vector3 endPosition = co.transform.position;

			ci.transform.position -= (endPosition - startPosition);

//			co.transform.position = startPosition;

		}
	}



	static public void orbitCamera (object sender, UxArgs uxArgs)

	{

		// Create direct references for ease of use.

		GameObject co, ci;
		co = uxArgs.activeInterface.camera.cameraObject;
		ci =  uxArgs.activeInterface.camera.cameraInterest;

		
		if (ci != null && co != null) {

			// we need a gameobject to be the interest, and a gameobject that holds the camera.

			float degreesPerPixel = 360f / Screen.height;
			float distanceToInterest = -1f * co.transform.localPosition.z;

			// delta.x -> YAW. rotate around (world) y axis.
			Quaternion orientation = ci.transform.rotation;
			Quaternion yawRotation = Quaternion.AngleAxis (uxArgs.delta.x * degreesPerPixel, Vector3.up);
			orientation = yawRotation * orientation;

			// delta.y -> PITCH. rotate around (relative) x axis 
			Vector3 cameraDirection = co.transform.rotation * Vector3.forward;
			Vector3 pitchAxis = Vector3.Cross (cameraDirection, Vector3.up);
			Quaternion pitchRotation = Quaternion.AngleAxis (uxArgs.delta.y * degreesPerPixel, pitchAxis);
			orientation = pitchRotation * orientation;

			// neutralise roll, clamp pitch, leave yaw unchanged
			Vector3 euler = orientation.eulerAngles;

			euler.z = 0; 

			if (euler.x > 70 && euler.x <= 180) {
				euler.x = 70;
			}
			if (euler.x < 290 && euler.x > 180) {
				euler.x = 290;
			}

//			if (isSpringing) {
//				isSpringing = false;
//			}

			if (false) {

				// this clamps the camera to a lowest point. based on geometry. however, it's not generic right now -> refactor.

//				Vector3 newCameraPosition = state.cameraInterest.transform.position + orientation * new Vector3 (0, 0, -distanceToInterest);

				Vector3 newCameraPosition = ci.transform.position + orientation * new Vector3 (0, 0, -distanceToInterest);


				float hh = 0;	
				//			float hh = cc.getActiveLandscape ().getHeight (cp.x, cp.z) + minimumHeight;

				if (newCameraPosition.y < hh) {
//					float pitch = Mathf.Asin ((hh - TPObject.transform.position.y) / distanceToInterest) * Mathf.Rad2Deg;
					float pitch = Mathf.Asin ((hh - 0) / distanceToInterest) * Mathf.Rad2Deg;

					//Debug.Log ("orbitcam under hull, locking pitch to: " + pitch);
					euler.x = pitch;
				}

			}

			orientation = Quaternion.Euler (euler);
			ci.transform.rotation = orientation;

		}
	}


	public static void drag2d (object sender, UxArgs uxArgs)
	{

		// REFACTORING


		string name = uxArgs.uiEvent.target2D.transform.name;

		UiButton draggedButton;
		uxArgs.activeInterface.uiButtons.TryGetValue (name, out draggedButton);

		// catch exception: when constructing a uibutton the constructor searches for a gameobject by name, which may fail.
		if (draggedButton != null && (draggedButton.gameObject == null || draggedButton.dragTarget == null)) {
			Debug.Log ( "ERROR: uiButton object reference not found. Is the gameobject active?");
//			draggedButton.dragTarget = emptyObject;
		}


		panGUI (draggedButton.dragTarget, uxArgs.delta, draggedButton.constraint, uxArgs.uiEvent);
//
//
//		switch (name) {
//		case "block":
//			ui.isSpringing = false;
//			break;
//		case "back":
//		case "add":
//		case "cut":
//		case "copy":
//		case "paste":
//		case "stretch":
//		case "distribute":
//		case "adjust":
//		case "motion":
//		case "store":
//		case "circleHandle":
//		case "circleHandleRing":
//			//				panGUI (draggedButton.dragTarget, delta, draggedButton.constraint, ui);
//			//				break;
//		case "lineHandle":
//			panGUI (draggedButton.dragTarget, delta, draggedButton.constraint, ui);
//			break;
//		case "cardboard":
//		case "phone":
//		case "editor":
//			panGUI (draggedButton.dragTarget, delta, draggedButton.constraint, ui);
//			delta = new Vector2 (-delta.x, 0f);
//			panCamera (activeInterface, delta, activeInterface.cameraConstraint, true); // 3D event. in main menu we pan 3d cam along with gui.
//			break;
//		default:
//			break;
//		}
	}





//	public static void panCamera (UxInterface state, Vector2 delta)
//	{
//		panCamera (state, delta, emptyConstraint, false);
//
//	}


	public static void panCamera  (object sender, UxArgs uxArgs)


//	public static void panCamera (UxInterface state, Vector2 delta, UiConstraint constraint, bool lockY)

	{

		// working from orbit assumption

		GameObject co, ci;
		co = uxArgs.activeInterface.camera.cameraObject;
		ci =  uxArgs.activeInterface.camera.cameraInterest;
		UiConstraint constraint = uxArgs.activeInterface.camera.constraint;
		Vector3 delta = -1f* uxArgs.delta;

		// Consider the camera but apply the effect to the user object instead! ?!?!?!

		Vector3 cameraPositionIn = ci.transform.position; 

		// get current details

		Vector3 cameraPositionOut = ci.transform.position;  
		Quaternion cameraOrientation = ci.transform.rotation;
		Vector3 horizontal = cameraOrientation * Vector3.right;
		Vector3 vertical = cameraOrientation * Vector3.up;

		float unitsPerPixel = 10f / Screen.height; // feels random

		float zoomDistance = -1f* ci.transform.localPosition.z;

		// calculate intended out position
		cameraPositionOut += (delta.x * unitsPerPixel * 1f) * horizontal;
		cameraPositionOut += (delta.y * unitsPerPixel * 1f) * vertical;

		// apply hard constraint
		if (constraint.hardClamp) {
			cameraPositionOut.x = Mathf.Clamp (cameraPositionOut.x, constraint.hardClampMin.x, constraint.hardClampMax.x);
			cameraPositionOut.y = Mathf.Clamp (cameraPositionOut.y, constraint.hardClampMin.y, constraint.hardClampMax.y);
			cameraPositionOut.z = Mathf.Clamp (cameraPositionOut.z, constraint.hardClampMin.z, constraint.hardClampMax.z);
		}

		// control for camera hitting the hull. camera will be pushed up and when possible it'll smoothly lower back along the hull towards its original y.

		// !!!!!!!!!!!!!!!!!!!!!
		//			float hullHeight = cc.getActiveLandscape ().getHeight (cameraPositionOut.x, cameraPositionOut.z) + minimumHeight;


		if (false) {

			float lockValueY = 0;
			bool lockY = false;
			float hullHeight = 0;




			bool yUnlocked = false;
			bool yIsTooLow = false;
			bool hullHigherThanLockvalue = false;
			float modifY = 0f;

			if (cameraPositionOut.y > lockValueY)
				yUnlocked = true;

			if (cameraPositionOut.y <= hullHeight)
				yIsTooLow = true;

			if (lockValueY <= hullHeight)
				hullHigherThanLockvalue = true;

			if (yIsTooLow) {
				// find out how much y needs to go up and create a correction vector along the vertical axis to do it. (thus controlling x and z as well)
				modifY = hullHeight - cameraPositionOut.y;
				float factor = modifY / vertical.y;
				Vector3 yCorrection = factor * vertical;
				cameraPositionOut += yCorrection;

				if (!yUnlocked) {
					// store unmodified y value. effectively this stores the lock value only the first time the cam gets bumped up
					lockValueY = cameraPositionIn.y;
				} 
			}

			if (!yIsTooLow && yUnlocked && hullHigherThanLockvalue && lockY) { 
				
				// smooth towards hull y
				float cameraVel = 0f;
				cameraPositionOut.y = Mathf.SmoothDamp (cameraPositionIn.y, hullHeight, ref cameraVel, 0.25f); 
			}

			if (!yIsTooLow && yUnlocked && !hullHigherThanLockvalue && lockY) {
				// smooth towards unmodified y value
				float cameraVel = 0f;
				cameraPositionOut.y = Mathf.SmoothDamp (cameraPositionIn.y, lockValueY, ref cameraVel, 0.25f); 
				if (Mathf.Abs (cameraPositionOut.y - lockValueY) < 0.1f) {
					// snap value 
					cameraPositionOut.y = lockValueY;
					yUnlocked = false;// this is redundant
				}
			}

			//			Debug.Log (me + "ytoolow " + yIsTooLow.ToString () + " y modded " + yUnlocked.ToString ());
			//			Debug.Log (me + "y: " + cameraPositionOut.y + " unmod y: " + state.lockValueY);

		}


		ci.transform.position += (cameraPositionOut - cameraPositionIn);
	}

	public static void none  (object sender, UxArgs uxArgs)
	{
		
	}


	static void panCameraAbs  (object sender, UxArgs uxArgs)

//	public static void panCamera (UxInterface state, Vector3 delta, UiConstraint constraint)
	{

		GameObject co, ci;
		co = uxArgs.activeInterface.camera.cameraObject;
		ci =  uxArgs.activeInterface.camera.cameraInterest;
		UiConstraint constraint = uxArgs.activeInterface.camera.constraint;
		Vector3 delta = uxArgs.delta;


		// Consider the camera but apply the effect to the user object instead! Pans camera by delta, simply pushing the camera up when needed.
		Vector3 cameraPositionIn = co.transform.position; 
		Vector3 cameraPositionOut = co.transform.position;  

		// calculate intended out position
		cameraPositionOut += delta;

		// apply hard constraint
		if (constraint.hardClamp) {
			cameraPositionOut.x = Mathf.Clamp (cameraPositionOut.x, constraint.hardClampMin.x, constraint.hardClampMax.x);
			cameraPositionOut.y = Mathf.Clamp (cameraPositionOut.y, constraint.hardClampMin.y, constraint.hardClampMax.y);
			cameraPositionOut.z = Mathf.Clamp (cameraPositionOut.z, constraint.hardClampMin.z, constraint.hardClampMax.z);
		}

		// control for camera hitting the hull. camera will be pushed up.
		float hullHeight = 0;	
		//		float hullHeight = cc.getActiveLandscape ().getHeight (cameraPositionOut.x, cameraPositionOut.z) + minimumHeight;

		if (cameraPositionOut.y <= hullHeight) {
			cameraPositionOut.y = hullHeight;
		}

		ci.transform.position += (cameraPositionOut - cameraPositionIn);
	}



//	public static void zoomCamera (UxInterface state, float delta)
	public static void zoomCamera  (object sender, UxArgs uxArgs)

	{
		GameObject co, ci;
		co = uxArgs.activeInterface.camera.cameraObject;
		ci =  uxArgs.activeInterface.camera.cameraInterest;
		UiConstraint constraint = uxArgs.activeInterface.camera.constraint;

		float delta = uxArgs.delta.z;

		if (ci != null && co != null) {
			// dolly forward of backward
			float unitsPerPixel = 10f / Screen.height;

			Quaternion cameraOrientation = co.transform.rotation;
			Vector3 forward = cameraOrientation * Vector3.forward;

//			panCameraAbs (state, unitsPerPixel * delta * forward, state.cameraConstraint);

			uxArgs.delta = unitsPerPixel * delta * forward;

			panCameraAbs ( sender,  uxArgs);

		}
	}






}

public class UxCamera {

	public GameObject cameraObject,cameraReference, cameraInterest;
	public Camera camera;

	public UiConstraint constraint;
	public CAMERACONTROL control;


	public  UxCamera () {

	}

	public  UxCamera (GameObject theCameraObject) {

		// assumes that reference's parent is interest and that camera is component on reference

		cameraObject = theCameraObject;
		cameraInterest = theCameraObject.transform.parent.gameObject;

		camera = cameraObject.GetComponentInChildren<Camera> ();

		cameraReference = camera.gameObject;


	}



}


public class UxMapping {

	public event UxEventHandler ux_none,ux_tap_2d,ux_tap_3d,ux_tap_none,ux_single_2d,ux_single_3d,ux_single_none,ux_double_2d,ux_double_3d,ux_double_none;


	public UxMapping (){

	}

	public void none (object sender, UxArgs args){

		ux_none (sender, args);

	}


	public void tap_2d (object sender, UxArgs args){

		ux_tap_2d (sender, args);

	}

	public void tap_3d (object sender, UxArgs args){

		ux_tap_3d (sender, args);

	}

	public void tap_none (object sender, UxArgs args){

		ux_tap_none (sender, args);

	}

	public void single_2d (object sender, UxArgs args){

		ux_single_2d (sender, args);

	}

	public void single_3d (object sender, UxArgs args){

		ux_single_3d (sender, args);

	}

	public void single_none (object sender, UxArgs args){

		ux_single_none (sender, args);

	}

	public void double_2d (object sender, UxArgs args){

		ux_double_2d (sender, args);

	}

	public void double_3d (object sender, UxArgs args){

		ux_double_3d (sender, args);

	}

	public void double_none (object sender, UxArgs args){

		ux_double_none (sender, args);

	}



}



public class UxInterface
{
	// holds info about the state of the UI ie what camera is active, how is it controlled.

	public List <GameObject> selectedObjects;

	// a dictionary of buttons for this interface

	public Dictionary<string,UiButton> uiButtons;


//	public event UxEventHandler ux_tap_2d,ux_tap_3d,ux_tap_none,ux_single_2d,ux_single_3d,ux_single_none;

	public Material editMat, defaultMat;



	public UxMapping defaultUxMap;

	public Dictionary<string,UxMapping> uxMappings;

		public GameObject  canvasObject;


//	public GameObject cameraObject, canvasObject, cameraInterest;

	public UxCamera camera;

//	public CAMERACONTROL cameraControl;

//	public UiConstraint cameraConstraint;

//	public float lockValueY;

	public void none (object sender, UxArgs args){

		defaultUxMap.none (sender, args);

	}


	public void tap_2d (object sender, UxArgs args){
		
		defaultUxMap.tap_2d (sender, args);

	}

	public void tap_3d (object sender, UxArgs args){

		defaultUxMap.tap_3d (sender, args);

	}

	public void tap_none (object sender, UxArgs args){

		defaultUxMap.tap_none (sender, args);

	}

	public void single_2d (object sender, UxArgs args){

		defaultUxMap.single_2d (sender, args);

	}

	public void single_3d (object sender, UxArgs args){

		defaultUxMap.single_3d (sender, args);

	}

	public void single_none (object sender, UxArgs args){

		defaultUxMap.single_none (sender, args);

	}

	public void double_2d (object sender, UxArgs args){

		defaultUxMap.double_2d (sender, args);

	}

	public void double_3d (object sender, UxArgs args){

		defaultUxMap.double_3d (sender, args);

	}

	public void double_none (object sender, UxArgs args){

		defaultUxMap.double_none (sender, args);

	}


	public string[] getButtonNames (){

		Dictionary<string, UiButton>.KeyCollection allButtons = uiButtons.Keys;

		string[] allButtonNames = new string[allButtons.Count];

		allButtons.CopyTo (allButtonNames,0);

		return allButtonNames;
	}

	public UxInterface ()
	{

//		UxBegan2d += 

		editMat = Resources.Load ("materials/white", typeof(Material)) as Material;
		defaultMat = Resources.Load ("materials/black", typeof(Material)) as Material;

		uiButtons = new Dictionary<string, UiButton> ();
		selectedObjects = new List<GameObject> ();

//
//
//
//		cameraControl = CAMERACONTROL.VOID;
//
////				camera = null;
////				cameraInterest = null;
//
//		cameraConstraint = new UiConstraint ();
//
//		lockValueY = 9999999999f;


	}
}

public enum CAMERACONTROL
{
	ORBIT,
	GYRO,
	TURN,
	PAN,
	VOID
}


//public class UiEventMap
//
//
//{
//
//	public event[][] UxEventHandler UxEventMap;
//
//
//
//	public UiEventMap() {
//
//	}
//
//}

public class UiConstraint
{
	public Vector3 hardClampMin, hardClampMax;
	public bool hardClamp;

	public Vector3 edgeSpringMin, edgeSpringMax;
	public bool edgeSprings;

	public Vector2[] springPositions;
	public bool springs;

	public Vector2 anchor;
	public float radiusClampMin, radiusClampMax;
	public bool radiusClamp;



	public UiConstraint ()
	{
		hardClamp = false;
		edgeSprings = false;
		springs = false;
		radiusClamp = false;
	}
}

public class UiButton
{
	public string name;
	public string callback;
	public GameObject gameObject, dragTarget;
	public UiConstraint constraint;
	public Image image;
	public Color color;
	public float brightness, targetBrightness, stepBrightness;

	Vector2 lastPosition, deltaPosition;
	float lastAngle, deltaAngle;

	public UiButton ()
	{
		lastPosition = Vector2.zero;
	}

	public UiButton (string theName)
	{
		setReferences (theName);
		constraint = new UiConstraint ();
		dragTarget = gameObject;
		lastPosition = Vector2.zero;
	}

	public UiButton (string theName, GameObject theDragTarget)
	{
		setReferences (theName);
		dragTarget = theDragTarget;
		constraint = new UiConstraint ();
		lastPosition = Vector2.zero;
	}

	public UiButton (string theName, GameObject theDragTarget, UiConstraint theConstraint)
	{
		setReferences (theName);
		dragTarget = theDragTarget;
		constraint = theConstraint;
		lastPosition = Vector2.zero;
	}

	public float getDeltaAngle ()
	{
//				float result = 0;

		Vector3 anchor = dragTarget.GetComponent<RectTransform> ().anchoredPosition;

		Vector2 relativePosition = new Vector2 (anchor.x, anchor.y) - constraint.anchor;

		float angle = Mathf.Atan2 (relativePosition.y, relativePosition.x);

		deltaAngle = lastAngle - angle;
		lastAngle = angle;



		return deltaAngle;
	}

	public Vector2 getDeltaPosition ()
	{



		deltaPosition.x = gameObject.transform.position.x - lastPosition.x;
		deltaPosition.y = gameObject.transform.position.y - lastPosition.y;

		lastPosition.x = gameObject.transform.position.x;
		lastPosition.y = gameObject.transform.position.y;

		return deltaPosition;
	}

	void setReferences (string theName)
	{
		callback = "NOCALLBACK";
		name = theName;
		color = new Color (1, 1, 1, 1);
		brightness = 0f;
		targetBrightness = 0.5f;
		stepBrightness = 1f / 0.25f;

		gameObject = GameObject.Find (theName);
		if (gameObject != null) {
			image = gameObject.GetComponent<Image> ();
			image.color = brightness * color;
		} else {
			// catch exception
			Debug.Log ("ERROR: uibutton gameobject not found");
		}
	}

	//			public void setBrightness (float theBrightness)
	//			{
	//				brightness = theBrightness;
	//				targetBrightness = theBrightness;
	//				setColor ();
	//			}

	public void applyColour ()
	{
		if (brightness < targetBrightness) {
			brightness += stepBrightness * Time.deltaTime;
			if (brightness >= targetBrightness) {
				brightness = targetBrightness;
			}
		}
		if (brightness > targetBrightness) {
			brightness -= stepBrightness * Time.deltaTime;
			if (brightness <= targetBrightness) {
				brightness = targetBrightness;
			}
		}

		image.color = brightness * color;
	}
}

// ------------------------------------------------------------------------------------------------------------------------------------------
// turn physical mouse/touch interaction into a ui event




public delegate void UxEventHandler (object sender, UxArgs args);


public class UxArgs : EventArgs
{

	public UiEvent uiEvent;
	public UxInterface activeInterface;
	public Vector3 delta; // also in uievent

	public UxArgs ( ) : base () // extend the constructor 
	{ 

	}




}
