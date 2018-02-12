using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class DataHandler : MonoBehaviour
{

	float startListening = 0f;
	bool listening = false;

	Capture capture;
	GameObject captureTarget;
	GameObject led;
	int interval;
	int width, height;
	ushort[] depthMap;
	float timeStamp;

	GameObject go;

	public DataController dataController;

	string me = "Data handler: ";

	void Awake (){

		// Engine modules.

		Log.SetModuleLevel ("Assistant director", LOGLEVEL.WARNINGS);
		Log.SetModuleLevel ("Director", LOGLEVEL.WARNINGS);
		Log.SetModuleLevel ("Deus controller", LOGLEVEL.WARNINGS);
		Log.SetModuleLevel ("User controller", LOGLEVEL.WARNINGS);
		Log.SetModuleLevel ("Set controller", LOGLEVEL.WARNINGS);
		Log.SetModuleLevel ("Network manager", LOGLEVEL.WARNINGS);
		Log.SetModuleLevel ("Networkbroadcast", LOGLEVEL.WARNINGS);
		Log.SetModuleLevel ("Script", LOGLEVEL.WARNINGS);
		Log.SetModuleLevel ("Storypointer", LOGLEVEL.WARNINGS);
		Log.SetModuleLevel ("Storytask", LOGLEVEL.WARNINGS);
		Log.SetModuleLevel ("Taskupdate", LOGLEVEL.WARNINGS);
		Log.SetModuleLevel ("Uxcontroller", LOGLEVEL.WARNINGS);



		// Custom modules.


	}


	void Start ()
	{

		led = GameObject.Find ("led");
		led.SetActive (false);

		dataController.addTaskHandler (TaskHandler);





	}

	public bool TaskHandler (StoryTask task)
	{
		
		bool done = false;

		switch (task.description) {

		// KINECT

		case "dummykinect":

			go = GameObject.Find ("Kinect");

			PRESENCE.pKinect = new PKinect ();

			PRESENCE.pKinect.InitDummy (go);




			done = true;

			break;


		// Code specific to Kinect, to be compiled and run on windows only.


		case "startkinect":

			// attempts to start live kinect.


			string kinectstatus;

			if (!task.getStringValue ("kinectstatus", out kinectstatus)) {
				kinectstatus = "void";

			}


			switch (kinectstatus) {


			case "void":

				// first run.

		//		PRESENCE.kinectObject = GameObject.Find ("Kinect");

				#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

				GameObject kinectObject = GameObject.Find ("Kinect");

				PRESENCE.pKinect = new PKinect ();

				PRESENCE.pKinect.InitLive (kinectObject);

				kinectstatus = "initialising";

				#else

				Debug.LogWarning (me + "Non windows platform: dummy kinect.");
				kinectstatus = "dummy";



				#endif

				timeStamp = Time.time;

				break;


			case "initialising":

				//Debug.Log( me+ "kinect awake? "+ PRESENCE.pKinect.KinectManager.am

				if (PRESENCE.pKinect.IsLive ()) {

					kinectstatus = "live";

					Debug.Log (me + "Kinect live in : " + (Time.time - timeStamp));

					kinectstatus = "live";

				} else {

					// Time out

					if ((Time.time - timeStamp) > 1f) {
						
						Debug.LogWarning (me + "Kinect timed out.");

						kinectstatus = "dummy";

					}


				}
				break;

			case "live":
				
				done = true;
				break;



			case "dummy":

				done = true;

				break;



			default:

				break;



			}


			task.setStringValue ("kinectstatus", kinectstatus);


			break;



			#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		case "recordkinect":

			// we grab a frame and write it to disk...

			Debug.LogWarning (me + "writing depth to disk at " + Application.persistentDataPath);

			width = PRESENCE.pKinect.kinectManager.getUserDepthWidth ();
			height = PRESENCE.pKinect.kinectManager.getUserDepthHeight ();

			depthMap = PRESENCE.pKinect.kinectManager.GetRawDepthMap ();

			DepthCapture dc = new DepthCapture (width, height);
			DepthCapture.current = dc;

			dc.put (depthMap);

			IO.SaveDepthCaptures ();





			done = true;

			break;

		#endif

	



		// NETWORKING

		case "networkguion":

			dataController.displayNetworkGUI (true);

			done = true;

			break;

		case "startdiscover":

			dataController.startBroadcastClient ();

			startListening = Time.time;
			listening = true;

			done = true;

			break;

		case "listenforserver":

			if (listening) {

				if (dataController.foundServer ()) {

					Debug.Log (me + "Found broadcast server");

					// Store network server address.

					GENERAL.networkServer = GENERAL.broadcastServer;

					dataController.stopBroadcast ();

					task.setCallBack ("foundserver");

					listening = false;

					done = true;

				}

				/*
				// Optional time out.

				if (Time.time - startListening > 10f) {

					dataController.networkBroadcastStop ();

					done = true;

				}
				*/

			}

			break;
	
		case "listenforclients":

			task.setStringValue ("debug", "" + dataController.serverConnections ());

			int newClient = GENERAL.GETNEWCONNECTION ();

			if (newClient != -1) {

				Debug.Log (me + "New client on connection " + newClient);

				task.setCallBack ("newclient");

			}

			break;

		case "monitorconnection":

			if (!dataController.clientIsConnected ()) {

				if (GENERAL.wasConnected) {

					GENERAL.wasConnected = false;
					task.setStringValue ("debug", "lost connection");

					task.setCallBack ("serversearch");


					done = true; // we'll fall throught to the next line in the script, since there is no longer a connection to monitor.

				} else {

					task.setStringValue ("debug", "no connection yet");

				}

			} else {

				GENERAL.wasConnected = true;

				task.setStringValue ("debug", "connected");

			}

			led.SetActive (GENERAL.wasConnected);

			break;

		case "startclient":

			Debug.Log (me + "Starting network client.");

			dataController.startNetworkClient (GENERAL.networkServer);

			done = true;

			break;

		case "startserver":

			Debug.Log (me + "Starting network server");

			dataController.startBroadcastServer ();
			dataController.startNetworkServer ();

			done = true;

			break;





		// IO


		case "loaddepthdata":

			// load depth data from resources

			IO.LoadDepthCapturesResource ();

			if (IO.savedDepthCaptures.Count > 0) {
				IO.depthIndex = 0;
			}

			done = true;

			break;

		case "playback":

			if (Capture.playing) {

				Frame f;

				if (!capture.read (out f)) {


					done = true;


				} else {
					captureTarget.transform.position = f.getPosition ();
					captureTarget.transform.rotation = f.getRotation ();

				}


			} else {

				capture = Capture.current;

				capture.play ();

				captureTarget = GameObject.Find ("camB_object");


			}




			break;

		case "capture":

			if (Capture.capturing) {

				Vector3 pos = captureTarget.transform.position;
				Quaternion orient = captureTarget.transform.rotation;

				if (!capture.log (pos, orient)) {

					// if log filled, end task

					done = true;

				}


			} else {

				capture = new Capture ();
				Capture.current = capture;
				capture.capture ();

				captureTarget = GameObject.Find ("camB_object");


				Debug.Log (me + "Starting new capture.");



			}


			break;


		case "load":

			IO.LoadUserCaptures ();


			Capture.current = IO.savedCaptures [0];


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

			IO.SaveUserCaptures ();

			done = true;

			break;


		// MISC / WIP

		case "goglobal":

			task.pointer.scope = SCOPE.GLOBAL;
			task.pointer.modified = true;

			done = true;

			break;


		case "sync":
			StoryPointer targetPointer;

			if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL) {

				// this should be the case, since we're only calling this on the server...

				targetPointer = GENERAL.getPointerOnStoryline ("userinterface");

				if (targetPointer != null) {
					targetPointer.scope = SCOPE.GLOBAL;
					targetPointer.modified = true;

					Debug.Log (me + "sync pointer: " + targetPointer.ID);

					targetPointer.currentTask.scope = SCOPE.GLOBAL;

					targetPointer.currentTask.markAllAsModified ();

					Debug.Log (me + "sync task: " + targetPointer.currentTask.ID);
				}





				targetPointer = GENERAL.getPointerOnStoryline ("cloud");

				if (targetPointer != null) {
					targetPointer.scope = SCOPE.GLOBAL;
					targetPointer.modified = true;

					Debug.Log (me + "sync pointer: " + targetPointer.ID);

					targetPointer.currentTask.scope = SCOPE.GLOBAL;

					targetPointer.currentTask.markAllAsModified ();

					Debug.Log (me + "sync task: " + targetPointer.currentTask.ID);

				}






			} else {

				Debug.LogWarning (me + "Syncing but no global authority.");
			}


		

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

		// HACK
		string inputString = Input.inputString;
		if (inputString.Length > 0) {

			if (inputString == "d") {

				Debug.Log (me + "Simulating disconnect/pause ...");

				dataController.StopNetworkClient ();

			}

		}
		
	}

}
