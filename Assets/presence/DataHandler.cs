﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class DataHandler : MonoBehaviour
{

	float startListening = 0f;
	bool listening = false;

	Capture capture;
	GameObject captureTarget;


	public DataController dataController;

	string me = "Data handler: ";

	void Start ()
	{

		dataController.addTaskHandler (TaskHandler);

	}

	public bool TaskHandler (StoryTask task)
	{
		
		bool done = false;

		switch (task.description) {

		case "startdiscover":

			dataController.networkBroadcastInit ();
			dataController.networkBroadcastStartAsClient ();

			startListening = Time.time;
			listening = true;

			done = true;

			break;

		case "listenforserver":

			if (listening)  
			{

				if (dataController.foundServer ()) {

					Debug.Log (me + "found server, now use callback!");

					task.setCallBack ("foundserver");

					dataController.networkBroadcastStop ();

					listening = false;
				
				}

				if (Time.time - startListening > 1f) {

					dataController.networkBroadcastStop ();

					done = true;

				}

			}

			break;

		case "startserver":
			
//			Debug.Log (me + "start server");

			dataController.networkBroadcastInit ();
			dataController.networkBroadcastStartAsServer ();

			dataController.networkStartServer ();

			done = true;

			break;

		case "listenforclients":

//			int newClient = dataController.getAD ().newClientConnection ();

			int newClient = GENERAL.GETNEWCONNECTION();

			if (newClient != -1) {

				Debug.Log (me + "New client on connection "+newClient);

				task.setCallBack ("newclient");

			}

			break;

		case "sync":

			if (GENERAL.SCOPE == SCOPE.GLOBAL) {

				// this should be the case, since we're only calling this on the server...
			
				StoryPointer targetPointer = GENERAL.getPointerOnStoryline ("userviewing");
				targetPointer.scope = SCOPE.GLOBAL;
				targetPointer.hasChanged = true;


				Debug.Log (me + "sync pointer: " + targetPointer.ID);


				targetPointer.currentTask.scope = SCOPE.GLOBAL;

				targetPointer.currentTask.hasChanged = true;

				Debug.Log (me + "sync task: " + targetPointer.currentTask.ID);


			}

			done = true;

			break;

		case "monitorconnection":

			if (GENERAL.LOSTCONNECTION) {


				Debug.Log (me + "Lost connection to server.");

				task.setCallBack ("network");


			}

			break;

		case "someothertask":
		case "someothertask2":
			
		case "globaltask":

			break;


		case "playback":

			if (Capture.playing) {
				
				Frame f;

				if (!capture.read (out f)) {


					done = true;


				} else {
					captureTarget.transform.position = f.getPosition();
					captureTarget.transform.rotation = f.getRotation();

				}


			} else {

				capture = Capture.current;

				capture.play();

				captureTarget = GameObject.Find ("camB_object");


			}




			break;

		case "capture":

			if (Capture.capturing) {

				Vector3 pos = captureTarget.transform.position;
				Quaternion orient =captureTarget.transform.rotation;

				if (!capture.log (pos, orient)) {

					// if log filled, end task

					done = true;

				}


			} else {
				
				capture = new Capture ();
				Capture.current = capture;
				capture.capture();

				captureTarget = GameObject.Find ("camB_object");


				Debug.Log (me + "Starting new capture.");



			}


			break;


		case "load":

			SaveLoad.Load ();


			Capture.current = SaveLoad.savedCaptures [0];


//			Debug.Log (me + "loaded with name " + Capture.current.knight.name);

			//					Game.current = testgame;

			//					SaveLoad.Save ();

			done = true;

			break;



		case "save":


//			Capture testgame = new Capture ();

//			testgame.knight.name = "My uuid: " + UUID.getID ();

//			Debug.Log (me + "saving with name " + testgame.knight.name);

//			Capture.current = testgame;

			Debug.Log (me + "Saving capture.");

			SaveLoad.Save ();

			done = true;

			break;



		default:

			// Default is caught in 
			done = true;

			break;

		}

		return done;

	}

	void Update ()
	{


		
	}

}
