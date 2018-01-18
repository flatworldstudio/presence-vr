using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class UxController
{


	GameObject emptyObject, TPObject;


	Camera targetCamera;
	// TO DO !!!!
	GraphicRaycaster targetRaycaster;
	// TO DO !!!!!

	string me = "UxHandler: ";


	UiEvent activeUiEvent, emptyUiEvent;


	UiConstraint emptyConstraint;


	int cycle = 0;

	List <UiEvent> uiEventStack;



	public UxController ()
	{
		initialise ();
	}

	// 	high level


	void initialise ()
	{
		
		activeUiEvent = new UiEvent ();
		uiEventStack = new List <UiEvent> ();
		uiEventStack.Add (activeUiEvent);

		emptyUiEvent = new UiEvent ();

		emptyConstraint = new UiConstraint ();

	}

	public string update (UxInterface activeInterface)
	{

		// log current stack count to track changes.

		int stackSize = uiEventStack.Count;

		// apply this frame's user interaction to the 'active' ui event.

		applyUserInteraction (activeUiEvent, activeInterface);

		// if a user action started, perform additional handling before processing. (If it ended we process first, then perform additional handling, see below).

		if (activeUiEvent.touch == UITOUCH.BEGAN) {
			
			// a user action just began. find any objects and buttons the event is targeting. 

			setUiTargetObjects (activeUiEvent, activeInterface);

			// if this new action is targeting the same object or button as an event already on the stack, the old event should be removed.

			int e = uiEventStack.Count - 1;

			while (e >= 0) {

				UiEvent checkEvent = uiEventStack [e];

				if (checkEvent != activeUiEvent) {
					
					bool removeThis = false;

					if (activeUiEvent.targetButton != null) {

						// if same interface target, remove old.

						if (activeUiEvent.targetButton.dragTarget == checkEvent.targetButton.dragTarget) {
							
							removeThis = true;
						}

					} else if (activeUiEvent.target3D != null) {

						// if same 3d target, remove old.

						if (checkEvent.target3D == checkEvent.target3D) {
							
							removeThis = true;
						}
													

					} else {

						// if both have no targets at all, remove old. 

						if (checkEvent.targetButton == null && checkEvent.target2D == null && checkEvent.target3D == null) {

							removeThis = true;

						}

					}
					if (removeThis) {

						uiEventStack.RemoveAt (e);

					}

				}

				e--;
			}

		}

		// now handle the stack of ui events. this way, events can play out (inertia, springing) while the user starts new interaction.

		string callbackResult = ""; // this works with story engine -> should be more agnostic?

		int i = 0;

		while (i < uiEventStack.Count) {

			UiEvent uiEvent = uiEventStack [i];

			//				Debug.Log (me + "handling event stack of size " + uiEventStack.Count);

			processUiEvent (uiEvent, activeInterface);

			// If an old event is no longer inert or springing, remove it.

			if (uiEvent != activeUiEvent && uiEvent.isInert == false && uiEvent.isSpringing == false) {
					
				uiEventStack.RemoveAt (i);

				i--;

			}

			i++;

		}

		if (activeUiEvent.callback != "") {
		
			callbackResult = activeUiEvent.callback;

			Debug.Log ("callbackResult: " + callbackResult);

		}

		if (activeUiEvent.touch == UITOUCH.ENDED) {

			// the active event just ended. set inert and springing to true.

			activeUiEvent.touch = UITOUCH.NONE;
			activeUiEvent.isInert = true;


			if (activeUiEvent.targetButton != null) {
				
				activeUiEvent.isSpringing = true;

			}

			if (activeUiEvent.action == UIACTION.TAP) {

				// tap must only be executed once, after that it becomes an inert singledrag event.

				activeUiEvent.action = UIACTION.SINGLEDRAG;

			}

			UiEvent newEvent = new UiEvent ();
			uiEventStack.Add (newEvent);
			activeUiEvent = newEvent;

		}

		int stackSizeNew = uiEventStack.Count;

		if (stackSize != stackSizeNew) {
			
//			Debug.Log (me + cycle + " Event stack size changed: " + stackSizeNew);

		}

		return callbackResult;

	}

	// lower level

	void applyUserInteraction (UiEvent ui, UxInterface theUiState)
	{


		cycle++;
		cycle = cycle % 1000;

		// check user mouse/touch interaction and populate this passed-in UIEVENT accordingly

		UiEvent storeUi = ui.clone ();

//		ui.action = UIACTION.VOID;
		ui.touch = UITOUCH.NONE;

		ui.dd = 0;


		#if UNITY_EDITOR || UNITY_STANDALONE

//		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) {

			// if we're on macos .... or windows?

			if (Input.GetButton ("Fire1")) {
				
				ui.x = Input.mousePosition.x;
				ui.y = Input.mousePosition.y;
				ui.dx = ui.x - ui.px;
				ui.dx = ui.dx / Screen.height * 720f; // normalise to the 1280 x 720 frame we're using for the ui. 
				ui.dy = ui.y - ui.py;
				ui.dy = ui.dy / Screen.height * 720f; // normalise to the 1280 x 720 frame we're using for the ui. 
				ui.px = ui.x;
				ui.py = ui.y;

				if (!ui.firstFrame) { // skip first frame because delta value will jump.
					
					ui.action = UIACTION.SINGLEDRAG;
					ui.touch = UITOUCH.TOUCHING;

					if (Input.GetKey (KeyCode.Space)) {
						// equivalent to doubletouch dragging only
						ui.action = UIACTION.DOUBLEDRAG;
					} 
					if (Input.GetKey (KeyCode.LeftAlt) || Input.GetKey (KeyCode.RightAlt)) {
						// equivalent to doubletouch pinching only
						ui.action = UIACTION.DOUBLEDRAG;
						ui.dd = ui.dx;
						ui.dx = 0;
						ui.dy = 0;
					} 
				} else {
					ui.firstFrame = false;
					ui.touch = UITOUCH.BEGAN;
					ui.dx = 0;
					ui.dy = 0;
				}
				trackTap (ui);
			} 

			if (Input.GetButtonUp ("Fire1")) {
				
				ui.touch = UITOUCH.ENDED;

				if (wasTap (ui)) {

					ui.action = UIACTION.TAP;

				} else {

				}

				ui.firstFrame = true;

			}
//		}

		#endif


		#if UNITY_IOS

//		if (Application.platform == RuntimePlatform.IPhonePlayer) {

			// if we're on ios

			if (Input.touchCount == 1) {
				// review single touch
				Vector2 tp = Input.GetTouch (0).position;

				switch (Input.GetTouch (0).phase) {
				case TouchPhase.Began:
					ui.x = tp.x;
					ui.y = tp.y;
					trackTap (ui);
					ui.touch = UITOUCH.BEGAN;
					break;

				case TouchPhase.Moved:
					Vector2 touchDelta = Input.GetTouch (0).deltaPosition;
					ui.dx = touchDelta.x / Screen.height * 720f;
					ui.dy = touchDelta.y / Screen.height * 720f;
					ui.action = UIACTION.SINGLEDRAG;
					ui.touch = UITOUCH.TOUCHING;
					trackTap (ui);
					break;

				case TouchPhase.Stationary:
					ui.dx = 0;
					ui.dy = 0;
					ui.action = UIACTION.SINGLEDRAG;
					ui.touch = UITOUCH.TOUCHING;
					trackTap (ui);
					break;

				case TouchPhase.Ended:

					ui.touch = UITOUCH.ENDED;

					if (wasTap (ui)) {
						ui.action = UIACTION.TAP;
					} else {

					}
					break;

				default:
					break;
				}
			}

			if (Input.touchCount == 2) {
				// review double touch 
				TouchPhase phase0 = Input.GetTouch (0).phase;
				TouchPhase phase1 = Input.GetTouch (1).phase;

				Vector2 tp0 = Input.GetTouch (0).position;
				Vector2 tp1 = Input.GetTouch (1).position;

				if ((phase0 == TouchPhase.Moved || phase0 == TouchPhase.Stationary) && phase1 == TouchPhase.Began) {
					// if one finger was touching, we leave the targets untouched, and initialise the d value
					ui.d = Vector2.Distance (tp0, tp1);
					ui.touch = UITOUCH.TOUCHING;

				} else if (phase0 == TouchPhase.Began && phase1 == TouchPhase.Began) {
					// if both start at the same time, aim in between to set targets and initialise the d value
					ui.x = (tp0.x + tp1.x) / 2f;
					ui.y = (tp0.y + tp1.y) / 2f;
					ui.d = Vector2.Distance (tp0, tp1);
					ui.touch = UITOUCH.BEGAN;

				} else if (phase0 == TouchPhase.Ended && phase1 == TouchPhase.Began) {
					// unlikely but could happen: flicking fingers in a single frame. catch the start of a new single touch.
					ui.x = tp1.x;
					ui.y = tp1.y;
					ui.touch = UITOUCH.BEGAN;

				} else if ((phase0 == TouchPhase.Moved || phase0 == TouchPhase.Stationary) && (phase0 == TouchPhase.Moved || phase0 == TouchPhase.Stationary)) {
					// dragging
					Vector2 touchDelta0 = Input.GetTouch (0).deltaPosition;
					Vector2 touchDelta1 = Input.GetTouch (1).deltaPosition;

					ui.dx = (touchDelta0.x + touchDelta1.x) / 2f / Screen.height * 720f; 
					ui.dy = (touchDelta0.y + touchDelta1.y) / 2f / Screen.height * 720f; 
					ui.d = Vector2.Distance (tp0, tp1);
					ui.dd = ui.d - storeUi.d;

					ui.action = UIACTION.DOUBLEDRAG;
					ui.touch = UITOUCH.TOUCHING;
				}
			}

//		}
		#endif
	}

	void trackTap (UiEvent ui)
	{
		ui.tapCount += Time.deltaTime;
	}

	bool wasTap (UiEvent ui)
	{
		bool result = false;

		if (ui.dx > -10f && ui.dx < 10f && ui.dy > -10f && ui.dy < 10f && ui.tapCount > 0 && ui.tapCount < 0.25f) {
			result = true;
			//				Debug.Log (me + cycle + " tap detected");
		}
		ui.tapCount = 0;
		return result;
	}


	void setUiTargetObjects (UiEvent ui, UxInterface theInterface)
	{

		// finds gameobject (2D and 3D) for possible manipulation, by raycasting. objects are registred in the UiEvent.

		RaycastHit hit;
		Vector3 uiPosition = new Vector3 (ui.x, ui.y, 0f);
					
		// cast a 3d ray from the camera we are controlling to find any 3D objects.

		Camera activeCamera = theInterface.camera.camera;

		Ray ray = activeCamera.ScreenPointToRay (uiPosition);

		int layerMask = 1 << 8; // only check colliders in layer '8'. 

		if (Physics.Raycast (ray, out hit, Mathf.Infinity, layerMask)) {
			
			ui.target3D = hit.transform.gameObject;

		} else {
			
			ui.target3D = null;

		}

		// cast a 2d ray in the canvas we're controlling.

		GraphicRaycaster gfxRayCaster = theInterface.canvasObject.GetComponent<GraphicRaycaster> ();

		//Create the PointerEventData with null for the EventSystem

		PointerEventData pointerEventData = new PointerEventData (null);

		//Set required parameters, in this case, mouse position

		pointerEventData.position = uiPosition;

		//Create list to receive all results

		List<RaycastResult> results = new List<RaycastResult> ();

		//Raycast it

		gfxRayCaster.Raycast (pointerEventData, results);

		if (results.Count > 0) {
			
			ui.target2D = results [0].gameObject;

			// find out if this 2d object is a button.

			UiButton checkButton = null;
			theInterface.uiButtons.TryGetValue (ui.target2D.transform.name, out checkButton);
			ui.targetButton = checkButton;


		} else {
			
			ui.target2D = null;

		}
	}



	// ------------------------------------------------------------------------------------------------------------------------------------------


	void processUiEvent (UiEvent ui, UxInterface activeInterface)
	{

		// this handles the result of user interaction, taking the info in the active UIEVENT and taking action by checking it against the UISTATE

		// could potentially be moved into interface class

		ui.callback = "";

		Vector2 delta;

		UxArgs args = new UxArgs ();

		args.activeInterface = activeInterface;
		args.delta = Vector3.zero;
		args.uiEvent = ui;

		switch (ui.action) {



		case UIACTION.TAP:

			if (ui.target2D != null) {

				activeInterface.tap_2d (this, args);

			} else if (ui.target3D != null) {

				activeInterface.tap_3d (this, args);

			} else {

				activeInterface.tap_none (this, args);

			}

			break;

		case UIACTION.SINGLEDRAG:
			
			delta = new Vector2 (ui.dx, ui.dy);

//			Debug.Log ("single drag");

			args.delta = new Vector3 (ui.dx, ui.dy, 0);


			if (ui.target2D != null) {
				
				activeInterface.single_2d (this, args);


			} else if (ui.target3D != null) {

				activeInterface.single_3d (this, args);

			} else {

				activeInterface.single_none (this, args);

			}

			break;

		case UIACTION.DOUBLEDRAG:

			delta = new Vector2 (ui.dx, ui.dy);

			args.delta = new Vector3 (ui.dx, ui.dy, ui.dd);


			if (ui.target2D != null) {

				activeInterface.double_2d (this, args);


			} else if (ui.target3D != null) {

				activeInterface.double_3d (this, args);

			} else {

				activeInterface.double_none (this, args);

			}



//
//			if (ui.target2D != null) {
//
//				activeInterface.double_2d (this, args);
//
////				singleDrag2D (activeInterface, ui, delta);
//
//			} else {
//
//
//
//				delta = -1f * delta;
//
//				panCamera (activeInterface, delta, activeInterface.cameraConstraint, false); // simplify??
//
//				zoomCamera (activeInterface, ui.dd);
//
//			}
			break;

		default:

			activeInterface.none (this, args);

			break;
		}

		if (ui.isInert) {

//			Debug.Log (ui.toString ());

			// Event is inert.

//			Debug.Log (me + cycle + "inertia " + ui.dx + " " + ui.dy);

//			Debug.Log ("inertia creeps..." + ui.dx+" "+ui.dy);

			float iVel = 0f;
			ui.dx = Mathf.SmoothDamp (ui.dx, 0, ref iVel, 0.075f);

			if (Mathf.Abs (ui.dx) < 1f) {
				ui.dx = 0;
			}

			ui.dy = Mathf.SmoothDamp (ui.dy, 0, ref iVel, 0.075f);

			if (Mathf.Abs (ui.dy) < 1f) {
				ui.dy = 0;
			}

			if (ui.dx == 0 && ui.dy == 0) {
				// inertia ended
				if (ui.isInert) {
//					Debug.Log ("Inertia stops");
				}
				ui.isInert = false;

			}

		}

		// apply brightness for all button objects

		Dictionary<string, UiButton>.ValueCollection allButtons =
			activeInterface.uiButtons.Values;

		foreach (UiButton button in allButtons) {
			button.applyColour ();
		}

		//			if (ui.touch == UITOUCH.NONE && ui.target2D != null) {
		//				// no touch, lettings springs run out while we have a target: apply a singledrag with delta 0
		//				singleDrag2D (state, ui, Vector2.zero);
		//			}

		//			if (ui.action == UIACTION.INERTIA && ui.target2D != null) {
		//				// no touch, lettings springs run out while we have a target: apply a singledrag with delta 0
		//				singleDrag2D (state, ui, Vector2.zero);
		//			}


	}


	// ------------------------------------------------------------------------------------------------------------------------------------------
	// execution of ui events


	void setRaycastActive (string name, bool value, UxInterface activeInterface)
	{
		UiButton theButton;
		activeInterface.uiButtons.TryGetValue (name, out theButton);

		if (theButton != null)
			theButton.image.raycastTarget = value;

	}

	bool brightnessIsChanging (string name, UxInterface activeInterface)
	{
		bool result = false;
		UiButton theButton;
		activeInterface.uiButtons.TryGetValue (name, out theButton);

		if (theButton != null) {
			if (theButton.brightness != theButton.targetBrightness) {
				result = true;

			}
		}

		return result;
	}









}







