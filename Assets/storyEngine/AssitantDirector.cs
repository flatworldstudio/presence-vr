//#define NETWORKED

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


#if NETWORKED
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

#endif

public delegate void NewTasksEvent (object sender, TaskArgs e);

public class AssitantDirector : MonoBehaviour

{
	
	public event NewTasksEvent newTasksEvent;

	string me = "Assistant director: ";

	Director theDirector;
	public string scriptName;
	public string launchOnStoryline;

	#if NETWORKED

	public ExtendedNetworkManager networkManager;

	const short stringCode = 1002;
	const short pointerCode = 1003;
	const short taskCode = 1004;

	#endif

	void Start ()
	{

		Debug.Log (me + "Starting ...");

		UUID.setIdentity ();

		Debug.Log (me + "Identity stamp " + UUID.identity);

		GENERAL.SCOPE = SCOPE.SOLO;

		theDirector = new Director ();

		GENERAL.ALLTASKS = new List<Task> ();

		#if NETWORKED

		// get the networkmanager to call network event methods on the assistant director.

		networkManager.onStartServerDelegate = onStartServer;
		networkManager.onStartClientDelegate = onStartClient;
		networkManager.onServerConnectDelegate = OnServerConnect;
		networkManager.onClientConnectDelegate = OnClientConnect;
		networkManager.onClientDisconnectDelegate = OnClientDisconnect;

		#endif

	}

	void Update ()

	{

		switch (theDirector.status) {

		case DIRECTORSTATUS.ACTIVE:

			theDirector.evaluatePointers ();

			List<Task> newTasks = new List<Task> ();

			for (int p = 0; p < GENERAL.ALLPOINTERS.Count; p++) {

				StoryPointer pointer = GENERAL.ALLPOINTERS [p];

				if (pointer.hasChanged) {

					switch (pointer.scope) {

					case SCOPE.GLOBAL:

						// If pointer scope is global, we add a task if our own scope is global as well. (If our scope is local, we'll be receiving the task over the network)

						if (GENERAL.SCOPE == SCOPE.GLOBAL) {

							if (pointer.getStatus () == POINTERSTATUS.NEWTASK) {

								pointer.setStatus (POINTERSTATUS.PAUSED);

								Task task = new Task (pointer,SCOPE.GLOBAL);

								pointer.currentTask = task;
								newTasks.Add (task);

								Debug.Log (me + "Global scope, global pointer, new task");
							}

						}

						break;

					case SCOPE.LOCAL:
					default:

						// If pointer scope is local, always check if new tasks have to be generated.

						if (pointer.getStatus () == POINTERSTATUS.NEWTASK) {

							pointer.setStatus (POINTERSTATUS.PAUSED);

							Task task = new Task (pointer,SCOPE.LOCAL);

							pointer.currentTask = task;

							newTasks.Add (task);

							Debug.Log (me + "Local pointer, new task");

						}

						break;

					}

				}

			}

			if (newTasks.Count > 0) {

				DistributeTasks (new TaskArgs (newTasks)); // if any new tasks call an event, passing on the list of tasks to any handlers listening
			}

			break;

		case DIRECTORSTATUS.READY:

			GENERAL.SIGNOFFS = eventHandlerCount ();


			if (GENERAL.SIGNOFFS == 0) {

				Debug.LogWarning (me + "No handlers registred. Pausing director.");
				theDirector.status = DIRECTORSTATUS.PAUSED;

			} else {

				Debug.Log (me + GENERAL.SIGNOFFS + " handlers registred.");
				Debug.Log (me + "Starting storyline " + launchOnStoryline);

				theDirector.beginStoryLine (launchOnStoryline);
				theDirector.status = DIRECTORSTATUS.ACTIVE;

			}

			break;

		case DIRECTORSTATUS.NOTREADY:

			theDirector.loadScript (scriptName);

			break;

		default:
			break;
		}

	}

	#if NETWORKED

	void LateUpdate ()
	{

		// see if any pointers were updated, if so send updates to the network.

		for (int p = 0; p < GENERAL.ALLPOINTERS.Count; p++) {

			StoryPointer pointer = GENERAL.ALLPOINTERS [p];

			if (pointer.hasChanged) {
				
				switch (GENERAL.SCOPE) {

				case SCOPE.LOCAL:

					if (pointer.scope == SCOPE.GLOBAL) {

						Debug.LogWarning (me + "A global pointer was changed locally... " + pointer.ID);

					}

					break;

				case SCOPE.GLOBAL:

					if (pointer.scope == SCOPE.GLOBAL) {

						Debug.Log (me + "Global pointer, global AD -> sending pointer update for " + pointer.ID);

						sendPointerUpdateToClients (pointer.getUpdateMessage ());

					}

					break;

				case SCOPE.SOLO:
				default:

					break;

				}

				pointer.hasChanged = false;

			}

		}

		// see if any tasks were updated, if so send updates to the network.

		for (int i = GENERAL.ALLTASKS.Count - 1; i >= 0; i--) {

			Task task = GENERAL.ALLTASKS [i];

			if (task.hasChanged) {

				if (task.status == TASKSTATUS.COMPLETE) {

					GENERAL.ALLTASKS.RemoveAt (i);

					Debug.Log (me + "task " + task.ID + " completed, REMOVING from alltasks.");

				}

				switch (GENERAL.SCOPE) {

				case SCOPE.LOCAL:

					if (task.scope == SCOPE.GLOBAL) {

						sendTaskUpdateToServer (task.getUpdateMessage ());

					}

					break;

				case SCOPE.GLOBAL:

					if (task.scope == SCOPE.GLOBAL) {

						sendTaskUpdateToClients (task.getUpdateMessage ());

					}

					break;

				case SCOPE.SOLO:
				default:

					break;

				}

				task.hasChanged = false;
			}

		}

	}

	#endif



	#if NETWORKED

	// Network connectivity handling.

	void onStartServer ()

	{

		GENERAL.SCOPE = SCOPE.GLOBAL;
		GENERAL.SETNEWCONNECTION (-1);

		Debug.Log (me + "start server delegate called, registring message handlers.");

		NetworkServer.RegisterHandler (stringCode, onServerStringMessage);
		NetworkServer.RegisterHandler (taskCode, onTaskUpdateFromClient);

	}

	void onStartClient (NetworkClient theClient)

	{

		Debug.Log (me + "start client delegate called, registring message handlers.");
		theClient.RegisterHandler (stringCode, onClientStringMessage);
		theClient.RegisterHandler (pointerCode, onPointerUpdateFromServer);
		theClient.RegisterHandler (taskCode, onTaskUpdateFromServer);

	}

	void OnServerConnect (NetworkConnection conn)

	{

		Debug.Log (me + "incoming server connection delegate called");

		GENERAL.SETNEWCONNECTION (conn.connectionId);

	}

	void OnClientConnect (NetworkConnection conn)

	{

		Debug.Log (me + "Client connection delegate called");

		GENERAL.SCOPE = SCOPE.LOCAL;
		GENERAL.LOSTCONNECTION = false;

	}

	void OnClientDisconnect (NetworkConnection conn)

	{

		Debug.Log (me + "Client disconnection delegate called");

		GENERAL.SCOPE = SCOPE.SOLO;
		GENERAL.LOSTCONNECTION = true;

	}

	// Handle basic string messages.

	void onServerStringMessage (NetworkMessage netMsg)
	{
		var message = netMsg.ReadMessage<StringMessage> ();

		Debug.Log (me + "string received from client: " + message.value);

	}

	void onClientStringMessage (NetworkMessage netMsg)
	{
		var message = netMsg.ReadMessage<StringMessage> ();

		Debug.Log (me + "string received from server: " + message.value);

	}

	// Handle pointer messages.

	void onPointerUpdateFromServer (NetworkMessage netMsg)
	{
		var message = netMsg.ReadMessage<PointerUpdate> ();

		Debug.Log (me + "Server update for pointer " + message.pointerUuid);

		applyPointerUpdate (message.pointerUuid, message.storyPoint, message.pointerStatus);
			
	}

	 void applyPointerUpdate (string pointerUuid, string pointName, int pointerStatus)
	{

		// get the story point

		StoryPoint point = GENERAL.getStoryPointByID (pointName);

		// see if the pointer exists, update or create new

		StoryPointer sp = GENERAL.getPointer (pointerUuid);

		if (sp == null) {

			sp = new StoryPointer (point, pointerUuid);

			Debug.Log (me + "Created a new (remotely owned) pointer with ID: " + sp.ID);

		} 

		sp.currentPoint = point;

		sp.setStatus ((POINTERSTATUS)pointerStatus);

		sp.setStatus (POINTERSTATUS.PAUSED); // overrule the status sent over the network, since global pointers aren't updated locally.

	}

	public void sendPointerUpdateToClients (PointerUpdate pointerMessage)

	{
	
		NetworkServer.SendToAll (pointerCode, pointerMessage);

		Debug.Log (me + "Sending pointer update to all clients: " + pointerMessage.pointerUuid + " " + pointerMessage.storyPoint);

	}

	// Handle task messages.

	void onTaskUpdateFromServer (NetworkMessage networkMessage)

	{

		var taskUpdate = networkMessage.ReadMessage<TaskUpdate> ();

		Debug.Log (me + "Incoming task update from server. ");

		applyTaskUpdate (taskUpdate);

	}

	void onTaskUpdateFromClient (NetworkMessage netMsg)

	{

		var taskUpdate = netMsg.ReadMessage<TaskUpdate> ();

		string debug = "";

		debug += "Incoming task update on connection ID " + netMsg.conn.connectionId;

		applyTaskUpdate (taskUpdate);

		List <NetworkConnection> connections = new List<NetworkConnection> (NetworkServer.connections);

		int c = 0;

		for (int ci = 0; ci < connections.Count; ci++) {

			NetworkConnection nc = connections [ci];

			if (nc != null) {

				if (nc.connectionId != netMsg.conn.connectionId) {

					debug += " sending update to connection ID " + nc.connectionId;

					NetworkServer.SendToClient (ci, taskCode, taskUpdate);
					c++;

				} else {

					debug += " skipping client connection ID " + nc.connectionId;

				}

			} else {

				debug +=  ("client connection null.");

			}

		}

		//		Debug.Log (debug);

	}

	void applyTaskUpdate (TaskUpdate taskUpdate)

	{

		StoryPointer updatePointer = GENERAL.getPointer (taskUpdate.pointerID);

		Task updateTask = GENERAL.getTask (taskUpdate.taskID);

		if (updateTask == null) {
			
			Debug.Log (me + "Creating new task upon network update, applying global ID, setting global scope.");

			updateTask = new Task (taskUpdate.description, updatePointer,taskUpdate.taskID);

			updateTask.ApplyUpdateMessage (taskUpdate);

			DistributeTasks (new TaskArgs (updateTask));

			if (updatePointer == null) {

				Debug.LogWarning (me + "update pointer not found: " + taskUpdate.pointerID);

			} else {

				updatePointer.currentTask = updateTask;

			}

		} else {

			updateTask.ApplyUpdateMessage (taskUpdate);
		}

	}

	void sendTaskUpdateToServer (TaskUpdate message)

	{
		
		networkManager.client.Send (taskCode, message);

//		Debug.Log (me + "Sending task update to server. ");
//		Debug.Log (message.toString());

	}

	void sendTaskUpdateToClients (TaskUpdate message)

	{

		NetworkServer.SendToAll (taskCode, message);

//		Debug.Log (me + "Sending task update to all clients. ");
//		Debug.Log (message.toString());

	}

	#endif

	public int eventHandlerCount ()
	{

		if (newTasksEvent != null) {
			
			return newTasksEvent.GetInvocationList ().Length;

		} else {
			
			return 0;
		}

	}

	// Invoke event;

	protected virtual void DistributeTasks (TaskArgs e)
	{

		if (newTasksEvent != null)
			newTasksEvent (this, e); // trigger the event, if there are any listeners

	}

}

public enum TASKTYPE
{
	//	ROOT,
	BASIC,
	ROUTING,
	END
	//	WAIT,
	//	SWITCH
}

#if NETWORKED


public class TaskUpdate : MessageBase
{
	
	public string pointerID, taskID;

	public string description;

	Dictionary<string,Int32> intValues;
	Dictionary<string,float> floatValues;

	public List<string> updatedIntNames;
	public List<Int32> updatedIntValues;

	public List<string> updatedFloatNames;
	public List<float> updatedFloatValues;

	public List<string> updatedQuaternionNames;
	public List<Quaternion> updatedQuaternionValues;

	public List<string> updatedVector3Names;
	public List<Vector3> updatedVector3Values;

	public string debug;

	public override void Deserialize (NetworkReader reader)
	{
		
		debug = "Deserialised: ";

		// Custom deserialisation.

//		string packetId = reader.ReadString ();
//		debug += "/ packetid: " + packetId;

		taskID = reader.ReadString ();
		pointerID = reader.ReadString ();
		description = reader.ReadString ();


		debug += "/ task: " + taskID;
		debug += "/ pointer: " + pointerID;
		debug += "/ description: " + description;


		// Deserialise updated int values.

		updatedIntNames = new List<string> ();
		updatedIntValues = new List<Int32> ();

		int intCount = reader.ReadInt32 ();

		debug += "/ updated ints: " + intCount;

		for (int i = 0; i < intCount; i++) {
			
			string intName = reader.ReadString ();
			Int32 intValue = reader.ReadInt32 ();

			updatedIntNames.Add (intName);
			updatedIntValues.Add (intValue);

		}

		// Deserialise updated float values.

		updatedFloatNames = new List<string> ();
		updatedFloatValues = new List<float> ();

		int floatCount = reader.ReadInt32 ();

		debug += "/ updated floats: " + floatCount;

		for (int i = 0; i < floatCount; i++) {

			string floatName = reader.ReadString ();
			float floatValue = reader.ReadSingle ();

			updatedFloatNames.Add (floatName);
			updatedFloatValues.Add (floatValue);

		}

		// Deserialise updated quaternion values.

		updatedQuaternionNames = new List<string> ();
		updatedQuaternionValues = new List<Quaternion> ();

		int quaternionCount = reader.ReadInt32 ();

		debug += "/ updated quaternions: " + quaternionCount;

		for (int i = 0; i < quaternionCount; i++) {

			string quaternionName = reader.ReadString ();
			Quaternion quaternionValue = reader.ReadQuaternion ();

			updatedQuaternionNames.Add (quaternionName);
			updatedQuaternionValues.Add (quaternionValue);

		}

		// Deserialise updated vector3 values.

		updatedVector3Names = new List<string> ();
		updatedVector3Values = new List<Vector3> ();

		int vector3Count = reader.ReadInt32 ();

		debug += "/ updated vector3s: " + vector3Count;

		for (int i = 0; i < vector3Count; i++) {

			string vector3Name = reader.ReadString ();
			Vector3 vector3Value = reader.ReadVector3 ();

			updatedVector3Names.Add (vector3Name);
			updatedVector3Values.Add (vector3Value);

		}

//		Debug.Log (debug);

	}

	public override  void Serialize (NetworkWriter writer)
	{

		debug = "Serialised: ";

		// Custom serialisation.

//		string packetId = UUID.getId ();
//		writer.Write (packetId);
//		debug += "/ packetid: " + packetId;

		// sender ip and storypointer uuid
				
		writer.Write (taskID);
		writer.Write (pointerID);
		writer.Write (description);

		debug += "/ task: " + taskID;
		debug += "/ pointer: " + pointerID;
		debug += "/ description: " + description;


		// Serialise updated int values.

		writer.Write (updatedIntNames.Count);

		debug += "/ updated ints: " + updatedIntNames.Count;

		for (int i = 0; i < updatedIntNames.Count; i++) {
			
			writer.Write (updatedIntNames [i]);
			writer.Write (updatedIntValues [i]);

		}

		// Serialise updated float values.

		writer.Write (updatedFloatNames.Count);

		debug += "/ updated floats: " + updatedFloatNames.Count;

		for (int i = 0; i < updatedFloatNames.Count; i++) {

			writer.Write (updatedFloatNames [i]);
			writer.Write (updatedFloatValues [i]);

		}

		// Serialise updated quaternion values.

		writer.Write (updatedQuaternionNames.Count);

		debug += "/ updated quaternions: " + updatedQuaternionNames.Count;

		for (int i = 0; i < updatedQuaternionNames.Count; i++) {

			writer.Write (updatedQuaternionNames [i]);
			writer.Write (updatedQuaternionValues [i]);

		}

		// Serialise updated vector3 values.

		writer.Write (updatedVector3Names.Count);

		debug += "/ updated vector3's: " + updatedVector3Names.Count;

		for (int i = 0; i < updatedVector3Names.Count; i++) {

			writer.Write (updatedVector3Names [i]);
			writer.Write (updatedVector3Values [i]);

		}

//		Debug.Log (debug);

	}

}

#endif

public class Task
{
	string me = "Task: ";
	public string ID;
	int signoffs;
	public string description;

	public Dictionary<string,Int32> taskIntValues;
	public Dictionary<string,float> taskFloatValues;
	public Dictionary<string,Quaternion> taskQuaternionValues;
	public Dictionary<string,Vector3> taskVector3Values;

	public StoryPointer pointer;
	public TASKSTATUS status;
	public SCOPE scope;

	public string callBackPoint;

	public float startTime, duration;
	public float d;

	#if NETWORKED

	public Dictionary<string,bool> taskValuesChangeMask;
	List<string> changedTaskValue;
	public bool hasChanged = false;

	#endif

	public Task (string myDescription, StoryPointer fromStoryPointer,string setID)
	{

		// Task created from network, so global scope and with passed in ID.

		description = myDescription;
		pointer = fromStoryPointer;
		ID = setID;
		scope = SCOPE.GLOBAL;

		setDefaults ();

	}
	public Task (StoryPointer fromStoryPointer,SCOPE setScope)

	{

		// Create a task based on the current storypoint of the pointer.
		// Note that setting scope is explicit, but in effect the scope of the task is the same as the scope of the pointer.

		description = fromStoryPointer.currentPoint.task [0];

		pointer = fromStoryPointer;
		ID = UUID.getGlobalID ();
		scope = setScope;

		setDefaults ();

	}

	void setDefaults (){

		signoffs = 0;
		status = TASKSTATUS.ACTIVE;
		GENERAL.ALLTASKS.Add (this);

		taskIntValues = new Dictionary<string,int> ();
		taskFloatValues = new Dictionary<string,float> ();
		taskQuaternionValues = new Dictionary<string,Quaternion> ();
		taskVector3Values = new Dictionary<string,Vector3> ();

		#if NETWORKED
		taskValuesChangeMask = new Dictionary<string,bool> ();
		#endif

	}

	#if NETWORKED

	public TaskUpdate getUpdateMessage ()
	{

		TaskUpdate msg = new TaskUpdate ();

		msg.pointerID = pointer.ID;
		msg.taskID = ID;
		msg.description = description;

		msg.updatedIntNames = new List<string> ();
		msg.updatedIntValues = new List<Int32> ();
		msg.updatedFloatNames = new List<string> ();
		msg.updatedFloatValues = new List<float> ();
		msg.updatedQuaternionNames = new List<string> ();
		msg.updatedQuaternionValues = new List<Quaternion> ();
		msg.updatedVector3Names = new List<string> ();
		msg.updatedVector3Values = new List<Vector3> ();

		string[] intNames = taskIntValues.Keys.ToArray ();

		foreach (string intName in intNames) {

			if (taskValuesChangeMask [intName]) {
				
				msg.updatedIntNames.Add (intName);

				taskValuesChangeMask [intName] = false;

				int intValue;

				if (taskIntValues.TryGetValue (intName, out intValue))
					msg.updatedIntValues.Add (intValue);

			}

		}

		string[] floatNames = taskFloatValues.Keys.ToArray ();

		foreach (string floatName in floatNames) {

			if (taskValuesChangeMask [floatName]) {

				msg.updatedFloatNames.Add (floatName);

				taskValuesChangeMask [floatName] = false;

				float floatValue;

				if (taskFloatValues.TryGetValue (floatName, out floatValue))
					msg.updatedFloatValues.Add (floatValue);

			}

		}

		string[] quaternionNames = taskQuaternionValues.Keys.ToArray ();

		foreach (string quaternionName in quaternionNames) {

			if (taskValuesChangeMask [quaternionName]) {

				msg.updatedQuaternionNames.Add (quaternionName);

				taskValuesChangeMask [quaternionName] = false;

				Quaternion quaternionValue;

				if (taskQuaternionValues.TryGetValue (quaternionName, out quaternionValue))
					msg.updatedQuaternionValues.Add (quaternionValue);

			}

		}

		string[] vector3Names = taskVector3Values.Keys.ToArray ();

		foreach (string vector3Name in vector3Names) {

			if (taskValuesChangeMask [vector3Name]) {

				msg.updatedVector3Names.Add (vector3Name);

				taskValuesChangeMask [vector3Name] = false;

				Vector3 vector3Value;

				if (taskVector3Values.TryGetValue (vector3Name, out vector3Value))
					msg.updatedVector3Values.Add (vector3Value);

			}

		}

		return msg;

	}

	public void ApplyUpdateMessage (TaskUpdate update)

	{
		
		for (int i = 0; i < update.updatedIntNames.Count; i++) {
			taskIntValues [update.updatedIntNames [i]] = update.updatedIntValues [i];
			taskValuesChangeMask [update.updatedIntNames [i]] = false;

		}

		for (int i = 0; i < update.updatedFloatNames.Count; i++) {
			taskFloatValues [update.updatedFloatNames [i]] = update.updatedFloatValues [i];
			taskValuesChangeMask [update.updatedFloatNames [i]] = false;

		}

		for (int i = 0; i < update.updatedQuaternionNames.Count; i++) {
			taskQuaternionValues [update.updatedQuaternionNames [i]] = update.updatedQuaternionValues [i];
			taskValuesChangeMask [update.updatedQuaternionNames [i]] = false;

		}

		for (int i = 0; i < update.updatedVector3Names.Count; i++) {
			taskVector3Values [update.updatedVector3Names [i]] = update.updatedVector3Values [i];
			taskValuesChangeMask [update.updatedVector3Names [i]] = false;

		}

	}

	#endif

	public void setIntValue (string valueName, Int32 value)

	{

		taskIntValues [valueName] = value;

		#if NETWORKED
		taskValuesChangeMask [valueName] = true;
		hasChanged = true;
		#endif

	}

	public bool getIntValue (string valueName, out Int32 value)

	{
		
		if (!taskIntValues.TryGetValue (valueName, out value)) {
			return false;
		}

		return true;

	}

	public void setFloatValue (string valueName, float value)

	{

		taskFloatValues [valueName] = value;

		#if NETWORKED
		taskValuesChangeMask [valueName] = true;
		hasChanged = true;
		#endif

	}

	public bool getFloatValue (string valueName, out float value)

	{
		
		if (!taskFloatValues.TryGetValue (valueName, out value)) {
			return false;
		}

		return true;

	}

	public void setVector3Value (string valueName, Vector3 value)

	{

		taskVector3Values [valueName] = value;

		#if NETWORKED
		taskValuesChangeMask [valueName] = true;
		hasChanged = true;
		#endif

	}

	public bool getVector3Value (string valueName, out Vector3 value)

	{
		
		if (!taskVector3Values.TryGetValue (valueName, out value)) {
			return false;
		}

		return true;

	}

	public void setQuaternionValue (string valueName, Quaternion value)

	{

		taskQuaternionValues [valueName] = value;

		#if NETWORKED
		taskValuesChangeMask [valueName] = true;
		hasChanged = true;
		#endif

	}

	public bool getQuaternionValue (string valueName, out Quaternion value)
	{
		
		if (!taskQuaternionValues.TryGetValue (valueName, out value)) {
			return false;
		}

		return true;

	}

	public void setStatus (TASKSTATUS theStatus)
	{
		
		status = theStatus;

		#if NETWORKED
		hasChanged = true;
		#endif

	}
			
	void complete ()
	{

		if (status != TASKSTATUS.COMPLETE) {

			// make sure a task is only completed once.
			
			setStatus (TASKSTATUS.COMPLETE);

			pointer.taskStatusChanged ();

		}

	}

	public void callBack (string theCallBackPoint)
	{

//		Debug.Log ("performing callback: " + theCallBackPoint);

		callBackPoint = theCallBackPoint;

		setStatus (TASKSTATUS.CALLBACK);
	
		pointer.setStatus (POINTERSTATUS.TASKUPDATED);

	}

	public void signOff (String fromMe)
	{

		if (GENERAL.SIGNOFFS == 0) {
			Debug.LogError ("Trying to signoff on a task with 0 required signoffs.");
		}

		signoffs++;

		//			Debug.Log ("SIGNOFFS "+fromMe + description + " signoffs: " + signoffs + " of " + signoffsRequired);

		if (signoffs == GENERAL.SIGNOFFS) {
				
			complete ();

		}

	}

}

public enum SCOPE
{
	SOLO,
	LOCAL,
	GLOBAL

}

public enum TASKSTATUS
{
	ACTIVE,
	CALLBACK,
	COMPLETE

}

public class TaskArgs : EventArgs

{

	public List <Task> theTasks;

	public TaskArgs (List <Task> tasks) : base () // extend the constructor 
	{ 
		theTasks = tasks;
	}

	public TaskArgs (Task task) : base () // extend the constructor 
	{ 
		theTasks = new List <Task> ();
		theTasks.Add (task);
	}

}

