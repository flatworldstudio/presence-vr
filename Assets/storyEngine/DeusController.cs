using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class DeusController : MonoBehaviour
{
//	float smoothMouseX, smoothMouseY;

	GameObject DeusCanvas, StoryEngineObject, PointerBlock;

//	public STORYMODE storyMode;


	 AssitantDirector ad;

	string me = "Deus Controller: ";
	List <StoryTask> taskList;
	List <StoryPointer> pointerList;
	StoryPointer[] pointerPositions;
	public bool storyBoard;

	void Start ()
	{
		
		Debug.Log (me + "Starting...");

		taskList = new List <StoryTask> ();
		pointerList = new List <StoryPointer> ();
		pointerPositions = new StoryPointer[10];

//		smoothMouseX = 0;
//		smoothMouseY = 0;

		StoryEngineObject = GameObject.Find ("StoryEngineObject");

		if (StoryEngineObject == null) {

			Debug.LogWarning ("StoryEngineObject with central command script not found.");

		} else {
			ad = StoryEngineObject.GetComponent <AssitantDirector> ();
			ad.newTasksEvent += new NewTasksEvent (newTasksHandler); // registrer for task events
		}

		DeusCanvas = GameObject.Find ("DeusCanvas");

		if (DeusCanvas == null) {
			Debug.LogWarning ("DeusCanvas not found.");
		} 

		DeusCanvas = GameObject.Find ("DeusCanvas");

		if (DeusCanvas == null) {
			Debug.LogWarning ("DeusCanvas not found.");
		} 

		PointerBlock = GameObject.Find ("PointerBlock");


		if (PointerBlock == null) {
			Debug.LogWarning ("PointerBlock not found.");
		} else {
//			PointerBlock.SetActive (false);
		}


	}

	void newTasksHandler (object sender, TaskArgs e)

	{

		addTasks (e.theTasks);

//		if (e.storyMode != 0) {
//		
//			storyMode = e.storyMode;
//			Debug.Log (me+"storymode "+e.storyMode);
//
//		}

	}

	public void addTasks (List<StoryTask> theTasks)

	{
		taskList.AddRange (theTasks);
	}

	void handleTasks ()
	{
		
		int t = 0;

		while (t < taskList.Count) {

			StoryTask task = taskList [t];

			if (task.pointer.getStatus () == POINTERSTATUS.KILLED && task.description != "end") {

				Debug.Log (me + "removing task:"+ task.description );

				taskList.RemoveAt (t);

			} else {

				switch (task.description) {

				case "end":

					// after finishing the end task, we mark the pointer as killed, so it gets removed.

					task.pointer.setStatus (POINTERSTATUS.KILLED);



//					Debug.Log (me + "Destroying ui for pointer for storyline " + task.pointer.currentPoint.storyLineName);
//
//					// update first. some pointers reach end in a single go - we want those to be aligned.
//
//					updateTaskDisplay (task);
//
//					pointerList.Remove (task.pointer);
//
//					pointerPositions [task.pointer.position] = null;
//
//					Destroy (task.pointer.pointerObject);
//
//					// after finishing the end task, we mark the pointer as killed, so it gets removed.
//
//					task.pointer.setStatus (POINTERSTATUS.KILLED);

					task.signOff (me);
					taskList.RemoveAt (t);
					break;

				default:

					updateTaskDisplay (task);

					task.signOff (me);
					taskList.RemoveAt (t);
					break;

				}
					
			}

		}

	}

	void updateTaskDisplays (){

//		Debug.Log (GENERAL.ALLTASKS.Count);

		foreach (StoryTask task in GENERAL.ALLTASKS) {

			updateTaskDisplay (task);

			if (task.description == "end") {
				
				Debug.Log (me + "Destroying ui for pointer for storyline " + task.pointer.currentPoint.storyLineName);

				// update first. some pointers reach end in a single go - we want those to be aligned.

//				updateTaskDisplay (task);

				pointerList.Remove (task.pointer);

				pointerPositions [task.pointer.position] = null;

				Destroy (task.pointer.pointerObject);


			}

//			updateTaskDisplay (task);


		}


	}


	void updateTaskDisplay (StoryTask theTask)
	{

		if (!pointerList.Contains (theTask.pointer)) {
			
			Debug.Log (me + "Pointer is new, added display for storyline " + theTask.pointer.currentPoint.storyLineName);

			pointerList.Add (theTask.pointer);

			createNewPointerUi (theTask.pointer);

		}

//			Debug.Log (me + "Setting UI info for task: " + theTask.description);


		if (theTask.description != "wait") {

			string displayText ;

			// If a task has a value for "debug" we display it along with task description.

			if (theTask.getStringValue ("debug", out displayText)) {
				
				displayText = theTask.description + " | " + displayText;

			} else {

				displayText = theTask.description;
			}
						
			theTask.pointer.deusText.text = displayText;
			theTask.pointer.deusTextSuper.text = theTask.pointer.currentPoint.storyLineName;

		} else {
			theTask.pointer.deusTextSuper.text = theTask.pointer.currentPoint.storyLineName;


		}

//		Debug.Log (me + "Updated pointer displays ");
//		for (int i = 0; i < 10; i++) {
//			if (pointerPositions [i] != null) {
//				Debug.Log (me + "position: " + i + " : " + pointerPositions [i].currentPoint.storyLineName);
//			}
//		}
	}


	void createNewPointerUi (StoryPointer targetPointer)
	{
		GameObject newPointerUi;

		newPointerUi = Instantiate (PointerBlock);

		newPointerUi.transform.SetParent (DeusCanvas.transform);

		newPointerUi.GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (0, 360, 0);
		newPointerUi.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);


		targetPointer.pointerObject = newPointerUi;
		targetPointer.pointerTextObject = newPointerUi.transform.Find ("textObject").gameObject;

		targetPointer.deusText = newPointerUi.transform.Find ("textObject/Text").GetComponent<Text> ();
		targetPointer.deusTextSuper = newPointerUi.transform.Find ("textObject/TextSuper").GetComponent<Text> ();

		// find empty spot
		int p = 0;
		while (pointerPositions [p] != null) {
			p++;
		}

//		Debug.Log ("found point position: " + p);


		pointerPositions [p] = targetPointer;
		targetPointer.position = p;

		int maxPosition = 0;

		for (int i = 0; i < 10; i++) {
			if (pointerPositions [i] != null) {
				maxPosition = i;
			}
		}
		maxPosition++;
		maxPosition = Mathf.Max (maxPosition, 4);

		float scalar = 4f / maxPosition;

//		Debug.Log (scalar + " " + maxPosition);

		float xSize = 320f * scalar;
		float xAnchor = 160f * scalar;
		float yAnchor = -0.5f * Screen.height + 80f * scalar;

		for (int i = 0; i < 10; i++) {
			if (pointerPositions [i] != null) {
					
				pointerPositions [i].pointerTextObject.GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (-640f + xAnchor + i * xSize, yAnchor, 0);
				pointerPositions [i].pointerTextObject.GetComponent<RectTransform> ().localScale = new Vector3 (scalar, scalar, 1);

			}
		}
				
	}

	void handleUi ()
	{
			
		string inputString = Input.inputString;
		if (inputString.Length > 0) {
				
			int storyPointerIndex = -1;

			switch (inputString) {

			case "1":
				storyPointerIndex = 0;
				break;
			case "2":
				storyPointerIndex = 1;
				break;
			case "3":
				storyPointerIndex = 2;
				break;
			case "4":
				storyPointerIndex = 3;
				break;
			case "5":
				storyPointerIndex = 4;
				break;
			case "6":
				storyPointerIndex = 5;
				break;

			case "7":
				storyPointerIndex = 6;
				break;
			case "8":
				storyPointerIndex = 7;
				break;
			case "9":
				storyPointerIndex = 8;
				break;


			default:
				break;
			}

			if (pointerPositions.Length > storyPointerIndex && storyPointerIndex != -1) {
				if (pointerPositions [storyPointerIndex] != null) {
					Debug.Log (me + "Progressing storyline" + pointerPositions [storyPointerIndex].currentPoint.storyLineName);

//					cc.progressPointer (pointerPositions [storyPointerIndex].uid);

					pointerPositions [storyPointerIndex].currentTask.setStatus (TASKSTATUS.COMPLETE);

					pointerPositions [storyPointerIndex].setStatus (POINTERSTATUS.TASKUPDATED);


				}
			}

		}


	}


	void Update ()
	{
			
		handleTasks ();
		handleUi ();
		updateTaskDisplays ();
	
	}
}
