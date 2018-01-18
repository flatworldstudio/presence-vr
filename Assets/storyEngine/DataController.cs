using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if NETWORKED
using UnityEngine.Networking;
#endif

public delegate bool DataTaskHandler (Task theTask);
	
public class DataController : MonoBehaviour
{
	
	GameObject StoryEngineObject;
	DataTaskHandler dataTaskHandler;

	#if NETWORKED
	GameObject NetworkObject;
	NetworkBroadcast networkBroadcast;
	ExtendedNetworkManager networkManager;
	#endif

	AssitantDirector ad;

	bool handlerWarning = false;

	public List <Task> taskList;

	string me = "Data controller: ";

	void Start ()
	{
		Debug.Log (me + "Starting...");

		taskList = new List <Task> ();

		#if NETWORKED

		NetworkObject = GameObject.Find ("NetworkObject");

		if (NetworkObject == null) {

			Debug.LogWarning (me + "NetworkObject not found.");

		} else {

			networkBroadcast = NetworkObject.GetComponent<NetworkBroadcast> ();
			networkManager = NetworkObject.GetComponent<ExtendedNetworkManager> ();

		}

		#endif

		StoryEngineObject = GameObject.Find ("StoryEngineObject");

		if (StoryEngineObject == null) {

			Debug.LogWarning (me + "StoryEngineObject not found.");

		} else {
			
			ad = StoryEngineObject.GetComponent <AssitantDirector> ();
			ad.newTasksEvent += new NewTasksEvent (newTasksHandler); // registrer for task events

		}

	}
		
	#if NETWORKED

	// These are networking methods to be called from datahandler to establish connections. 
	// Once connected, handling is done internally by the assistant directors.

	public void networkBroadcastInit ()
	{
		Debug.Log (me + "Initialising network broadcast.");

		networkBroadcast.Initialize ();
		networkBroadcast.serverMessage = "";

	}

	public void networkBroadcastStartAsClient ()
	{
		Debug.Log (me + "Starting network broadcast as client.");

		networkBroadcast.StartAsClient ();

	}

	public void networkBroadcastStop ()
	{
		Debug.Log (me + "Stopping network broadcast.");

		networkBroadcast.StopBroadcast ();

	}

	public void networkBroadcastStartAsServer ()
	{
		Debug.Log (me + "Starting network broadcast as server.");

		networkBroadcast.StartAsServer ();

	}

	public bool foundServer ()
	{
		
		if (networkBroadcast.serverMessage != "") {
			
			return true;

		} else {
			
			return false;
		}

	}

	public void networkStartServer ()
	{
		
		NetworkManager.singleton.StartServer ();

	}

	#endif

	public void addTaskHandler (DataTaskHandler theHandler)
	{
		dataTaskHandler = theHandler;
		Debug.Log (me + "Handler added");
	}

	void Update ()
	{
		
		int t = 0;

		while (t < taskList.Count) {

			Task task = taskList [t];

			if (task.pointer.getStatus () == POINTERSTATUS.KILLED && task.description != "end") {

				taskList.RemoveAt (t);

			} else {

				if (dataTaskHandler != null) {

					if (dataTaskHandler (task)) {

						task.signOff (me);
						taskList.RemoveAt (t);

					} else {
						
						t++;

					}

				} else {
					
					if (!handlerWarning) {
						
						Debug.LogWarning (me + "No handler available, blocking task while waiting.");
						handlerWarning = true;
						t++;

					} 

				}

			}

		}

	}

	void newTasksHandler (object sender, TaskArgs e)
	{
	
		addTasks (e.theTasks);

	}

	public void addTasks (List<Task> theTasks)
	{
		
		taskList.AddRange (theTasks);

	}

}




