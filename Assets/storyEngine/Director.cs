using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

#if NETWORKED

using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

#endif


public class Director
{

	List <StoryPointer> pointerStack;
	public DIRECTORSTATUS status;
	string me = "Director: ";

	public Director ()
	{

		GENERAL.ALLPOINTERS = new List <StoryPointer> ();
		status = DIRECTORSTATUS.NOTREADY;

	}

	public void evaluatePointers ()
	{

		// Create a stack of pointers for processing.

		pointerStack = new List <StoryPointer> ();

		for (int p = GENERAL.ALLPOINTERS.Count - 1; p >= 0; p--) {
				
			StoryPointer sp = GENERAL.ALLPOINTERS [p];

			if (sp.getStatus () == POINTERSTATUS.KILLED) {
					
				// if a pointer was killed, remove it now.

				Debug.Log (me + "Removing pointer uuid: " + sp.ID);

				GENERAL.ALLPOINTERS.RemoveAt (p);

			}

			if (sp.getStatus () == POINTERSTATUS.EVALUATE || sp.getStatus () == POINTERSTATUS.TASKUPDATED) {
				
				pointerStack.Add (sp);

			}

		}

		if (pointerStack.Count > 0)
			Debug.Log (me + "Evaluating " + pointerStack.Count + " of " + GENERAL.ALLPOINTERS.Count + " storypointers.");
		
		while (pointerStack.Count > 0) {

			// Keep processing items on the stack untill empty.
								
			StoryPointer pointer;
			string targetPointerName, targetValue;
			StoryPointer newPointer, targetPointer;
			StoryPoint targetPoint;

			pointer = pointerStack [0];

			Debug.Log (me + "Evaluating pointer uid: " + pointer.ID + " on storyline " + pointer.currentPoint.storyLineName);

			switch (pointer.currentPoint.taskType) {

			case TASKTYPE.ROUTING:

				string type = pointer.currentPoint.task [0];

				switch (type) {

				case "hold":

					// Put this pointer on hold. Remove from stack.

					Debug.Log (me + "Pausing pointer.");

					pointer.setStatus (POINTERSTATUS.PAUSED);

					pointerStack.RemoveAt (0);

					break;

				case "tell":
						
					// Control another pointer. Finds a/the(!) pointer on the given storyline and moves it to the given storypoint, marking the pointer for evaluation.
					// Progress this pointer, keeping it on the stack

					targetPointerName = pointer.currentPoint.task [1];

					targetValue = pointer.currentPoint.task [2];

					targetPointer = GENERAL.getPointerOnStoryline (targetPointerName);

					if (targetPointer != null) {
							
						targetPoint = GENERAL.getStoryPointByID (targetValue);

						if (targetPoint != null) {
							
							targetPointer.currentPoint = targetPoint;

						} else {

							Debug.LogWarning (me + "Tell was unable to find the indicated storypoint.");

						}

						targetPointer.setStatus (POINTERSTATUS.EVALUATE);

						Debug.Log (me + "Telling pointer on storyline " + targetPointerName + " to move to point " + targetValue);

					} else {
							
						Debug.LogWarning (me + "Tell was unable to find the indicated storypointer.");

					}

					moveToNextPoint (pointer);

					break;

				case "goto":
						
					// Moves this pointer to another point anywhere in the script. Mark for evaluation, keep on stack.

					targetValue = pointer.currentPoint.task [1];

					targetPoint = GENERAL.getStoryPointByID (targetValue);

					if (targetPoint != null) {

						pointer.currentPoint = targetPoint;

						Debug.Log (me + "Go to point " + targetValue);

					} else {
							
						Debug.LogWarning (me + "Goto point not found.");

					}

					pointer.setStatus (POINTERSTATUS.EVALUATE);

					break;

				case "start":
						
					// Start a new pointer on the given storypoint.
					// Create a new pointer, add it to the list of pointers and add it to the stack.
					// Progress the current pointer, keeping it on the stack.

					targetPointerName = pointer.currentPoint.task [1];

					targetPoint = GENERAL.getStoryPointByID (targetPointerName);

					if (GENERAL.getPointerOnStoryline (targetPointerName) == null) {
														
						Debug.Log (me + "Starting new pointer for storypoint: " + targetPointerName);

						newPointer = new StoryPointer (targetPoint);

						pointerStack.Add (newPointer); 

					} else {

						Debug.Log (me + "Storyline already active for storypoint " + targetPointerName);
					}

					moveToNextPoint (pointer);

					break;

				case "stop":
						
					// Stop another storypointer by storyline name, or all other storylines with 'all'.
													
					targetPointerName = pointer.currentPoint.task [1];

					if (targetPointerName == "all") {

						foreach (StoryPointer stp in GENERAL.ALLPOINTERS) {

							if (stp != pointer) {
									
								Debug.Log (me + "Stopping pointer " + pointer.ID + " on " + stp.currentPoint.storyLineName);

								stp.end ();

							}

						}

						// Remove all pointers from stack, re-adding the one we're on.

						pointerStack.Clear ();
						pointerStack.Add (pointer);

					} else {

						// Stop a single storypointer on given storyline.

						targetPointer = GENERAL.getPointerOnStoryline (targetPointerName);

						if (targetPointer != null) {
												
							Debug.Log (me + "Stopping pointer " + targetPointer.ID + " on " + targetPointer.currentPoint.storyLineName);

							pointerStack.Remove (targetPointer);

							targetPointer.end ();

						} else {

							Debug.Log (me + "No pointer found for " + targetPointerName);
							
						}

					}
										
					moveToNextPoint (pointer);

					break;

				default:
					
					break;

				}

				break;

			case TASKTYPE.BASIC:
			case TASKTYPE.END:
									
				if (pointer.getStatus () == POINTERSTATUS.EVALUATE) {

					// A normal task to be executed. Assistant director will generate task.

					Debug.Log (me + "Task to be executed: " + pointer.currentPoint.task [0]);

					pointer.setStatus (POINTERSTATUS.NEWTASK);

					pointerStack.RemoveAt (0);

				}

				if (pointer.getStatus () == POINTERSTATUS.TASKUPDATED) {

					// Something has happened in the task that we need to evaluate.
						
					if (pointer.currentTask.getStatus () == TASKSTATUS.COMPLETE) {

						// Task was completed. Check if there's a callback before moving on.

						checkForCallBack (pointer);

						// Task was completed, progress to the next point.

						Debug.Log (me + "task completed: " + pointer.currentTask.description);

						pointer.setStatus (POINTERSTATUS.EVALUATE);

						moveToNextPoint (pointer);

					}

					if (pointer.currentTask.getStatus () == TASKSTATUS.ACTIVE) {

						// See if there's a callback.

						checkForCallBack (pointer);

						// Return pointerstatus to paused and stop evaluating it for now.

//						Debug.LogWarning (me + "Pointerstatus says taskupdated, but taskstatus for task " + pointer.currentTask.description + " is active.");

						pointer.setStatus (POINTERSTATUS.PAUSED);

						pointerStack.RemoveAt (0);

					}



				}

				break;

			default:

				// This shouldn't occur.

				Debug.LogWarning (me + "Error: unkown storypoint type. ");

				pointerStack.RemoveAt (0);

				break;

			}

		} 

	}

	bool checkForCallBack (StoryPointer pointer){

		// checks and trigger callback on the current task for given pointer. does not touch the pointer itself.

		string callBackValue = pointer.currentTask.getCallBack ();

		if (callBackValue != "") {

			pointer.currentTask.clearCallBack (); // clear value

			// A callback is equivalent to 'start name', launching a new storypointer on the given point.

			StoryPoint targetPoint = GENERAL.getStoryPointByID (callBackValue);

			if (GENERAL.getPointerOnStoryline (pointer.currentTask.getCallBack ()) == null) {

				Debug.Log (me + "New callback storyline: " + callBackValue);

				StoryPointer newStoryPointer = new StoryPointer (targetPoint);

				pointerStack.Add (newStoryPointer);

			} else {

				Debug.Log (me + "Callback storyline already started: " + callBackValue);
			}

			return true;

		} else {

			return false;
		}

	}


	public void loadScript (string fileName)
	{
		Script theScript = new Script (fileName);

		while (!theScript.isReady) {

		}

		status = DIRECTORSTATUS.READY;

	}

	public void beginStoryLine (string beginName)
	{

		StoryPointer newStoryPointer = new StoryPointer (GENERAL.getStoryPointByID (beginName));

	}

	void moveToNextPoint (StoryPointer thePointer)
	{
		if (!thePointer.moveToNextPoint ()) {
			
			Debug.Log (me + "Error: killing pointer ");
			thePointer.setStatus (POINTERSTATUS.KILLED);

			pointerStack.RemoveAt (0);

		}
	}

	float getValue (string[] instructions, string var)
	{
		string r = "0";
		Char delimiter = '=';
		foreach (string e in instructions) {

			string[] splitElement = e.Split (delimiter);
			if (splitElement [0] == var && splitElement.Length > 1) {
				r = splitElement [1];
			}

		}

		return float.Parse (r);

	}

}

public enum DIRECTORSTATUS
{
	NOTREADY,
	READY,
	ACTIVE,
	PASSIVE,
	PAUSED
	//	ASSISTANT
}


#if NETWORKED

public class PointerUpdate : MessageBase
{

	public string pointerUuid;
	public string storyPoint;
	public int pointerStatus;
	// note that this actually gets overruled on the receiving client.

}
#endif


public enum POINTERSTATUS
{
	EVALUATE,
	NEWTASK,
	TASKUPDATED,
	KILLED,
	PAUSED
}

public class StoryPointer
{
	public string ID;
	public StoryPoint currentPoint;

	public SCOPE scope;

	POINTERSTATUS status;

	public StoryTask currentTask;

	public StoryTask persistantData; // can hold generic data which will be passed onto new task.... WIP

	public	Text deusText;
	public	Text deusTextSuper;
	public	GameObject pointerObject, pointerTextObject;
	public int position;

	#if NETWORKED
	public bool hasChanged;
	#endif

	string me = "Storypointer says: ";

	public StoryPointer ()


	{
		

	}


	public StoryPointer (StoryPoint startingPoint)
	{
		
		currentPoint = startingPoint;
		status = POINTERSTATUS.EVALUATE;
		ID = UUID.getGlobalID ();
		scope = SCOPE.LOCAL;
		GENERAL.ALLPOINTERS.Add (this);

		persistantData = new StoryTask ();

	}

	public StoryPointer (StoryPoint startingPoint, string setID)
	{
		
		currentPoint = startingPoint;
		status = POINTERSTATUS.EVALUATE;
		ID = setID;
		scope = SCOPE.GLOBAL;
		GENERAL.ALLPOINTERS.Add (this);

		persistantData = new StoryTask ();

	}

	public PointerUpdate getUpdateMessage ()
	{

		PointerUpdate r = new PointerUpdate ();
		r.pointerUuid = ID;
		r.storyPoint = currentPoint.ID;
		r.pointerStatus = (int)status;

		return r;

	}

	public void end ()
	{

		currentPoint = GENERAL.getStoryPointByID ("end");

		setStatus (POINTERSTATUS.NEWTASK);

	}

	public POINTERSTATUS getStatus ()
	{
		return status;
	}

	public void setStatus (POINTERSTATUS theStatus)
	{


		if (status != POINTERSTATUS.KILLED) {
		
			// The end task sets pointerstatus to killed, then calls this method when the end task is complete. If it was killed, keep it killed for removal. 
		
//					setStatus (POINTERSTATUS.TASKUPDATED);

			status = theStatus;
		}



		#if NETWORKED
		hasChanged = true;
		#endif

	}

	//	public void taskStatusChanged ()
	//
	//	{
	//
	//		if (status != POINTERSTATUS.KILLED) {
	//
	//			// The end task sets pointerstatus to killed, then calls this method when the end task is complete. If it was killed, keep it killed for removal.
	//
	//			setStatus (POINTERSTATUS.TASKUPDATED);
	//
	//		}
	//	}

	public Boolean moveToNextPoint ()
	{
		
		Boolean r = false;

		if (currentPoint.getNextStoryPoint () == null) {
			
			Debug.Log (me + "No next point");

		} else {
			
			currentPoint = currentPoint.getNextStoryPoint ();

			r = true;
		}

		return r;

	}

}







