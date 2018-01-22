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

		GENERAL.ALLTASKS = new List<StoryTask> ();

	


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

			List<StoryTask> newTasks = new List<StoryTask> ();

			for (int p = 0; p < GENERAL.ALLPOINTERS.Count; p++) {

				StoryPointer pointer = GENERAL.ALLPOINTERS [p];

				if (pointer.hasChanged) {

					switch (pointer.scope) {

					case SCOPE.GLOBAL:

						// If pointer scope is global, we add a task if our own scope is global as well. (If our scope is local, we'll be receiving the task over the network)

						if (GENERAL.SCOPE == SCOPE.GLOBAL) {

							if (pointer.getStatus () == POINTERSTATUS.NEWTASK) {

								pointer.setStatus (POINTERSTATUS.PAUSED);

								StoryTask task = new StoryTask (pointer, SCOPE.GLOBAL);

								task.loadPersistantData (pointer);

								pointer.currentTask = task;
								newTasks.Add (task);

								Debug.Log (me + "Global scope, global pointer, new task: "+task.description);
							}

						}

						break;

					case SCOPE.LOCAL:
					default:

						// If pointer scope is local, always check if new tasks have to be generated.

						if (pointer.getStatus () == POINTERSTATUS.NEWTASK) {

							pointer.setStatus (POINTERSTATUS.PAUSED);

							StoryTask task = new StoryTask (pointer, SCOPE.LOCAL);

							task.loadPersistantData (pointer);

							pointer.currentTask = task;

							newTasks.Add (task);

							Debug.Log (me + "Local pointer, new task: "+task.description);

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

			StoryTask task = GENERAL.ALLTASKS [i];

			if (task.hasChanged) {

				if (task.getStatus () == TASKSTATUS.COMPLETE) {

					GENERAL.ALLTASKS.RemoveAt (i);

					Debug.Log (me + "task " + task.ID + ":" + task.description + " completed, removing from general.alltasks.");


//					foreach (StoryTask st in GENERAL.ALLTASKS) {
//
//						//			updateTaskDisplay (task);
//						Debug.Log ("alltasks "+ st.description);
//					}



				}

				switch (GENERAL.SCOPE) {

				case SCOPE.LOCAL:

					if (task.scope == SCOPE.GLOBAL) {

						Debug.Log (me + "task " + task.ID + ":" + task.description + " changed, sending update to server.");

						sendTaskUpdateToServer (task.getUpdateMessage ());

					}

					break;

				case SCOPE.GLOBAL:

					if (task.scope == SCOPE.GLOBAL) {

						Debug.Log (me + "task " + task.ID + ":" + task.description + " changed, sending update to clients.");


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

				debug += ("client connection null.");

			}

		}

		//		Debug.Log (debug);

	}

	void applyTaskUpdate (TaskUpdate taskUpdate)
	{

		StoryPointer updatePointer = GENERAL.getPointer (taskUpdate.pointerID);

		StoryTask updateTask = GENERAL.getTask (taskUpdate.taskID);

		if (updateTask == null) {
			
			Debug.Log (me + "Creating new task upon network update, applying global ID, setting global scope.");

			updateTask = new StoryTask (taskUpdate.description, updatePointer, taskUpdate.taskID);

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

	public List<string> updatedStringNames;
	public List<string> updatedStringValues;

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

		// Deserialise updated string values.

		updatedStringNames = new List<string> ();
		updatedStringValues = new List<string> ();

		int stringCount = reader.ReadInt32 ();

		debug += "/ updated strings: " + stringCount;

		for (int i = 0; i < stringCount; i++) {

			string stringName = reader.ReadString ();
			string stringValue = reader.ReadString ();

			updatedStringNames.Add (stringName);
			updatedStringValues.Add (stringValue);

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

		// Serialise updated string values.

		writer.Write (updatedStringNames.Count);

		debug += "/ updated strings: " + updatedStringNames.Count;

		for (int i = 0; i < updatedStringNames.Count; i++) {

			writer.Write (updatedStringNames [i]);
			writer.Write (updatedStringValues [i]);

		}

//		Debug.Log (debug);

	}

}

#endif




public class StoryTask
{
	string me = "Task: ";
	public string ID;
	int signoffs;
	public string description;

	public Dictionary<string,Int32> taskIntValues;
	public Dictionary<string,float> taskFloatValues;
	public Dictionary<string,Quaternion> taskQuaternionValues;
	public Dictionary<string,Vector3> taskVector3Values;
	public Dictionary<string,string> taskStringValues;


	public StoryPointer pointer;

	//	public TASKSTATUS status;

	public SCOPE scope;

//	public string callBackPoint;

	public float startTime, duration;
	public float d;

	#if NETWORKED

	public Dictionary<string,bool> taskValuesChangeMask;
	List<string> changedTaskValue;
	public bool hasChanged = false;

	#endif


	public void loadPersistantData (StoryPointer referencePointer){

		// we use the update message internally to transfer values from the carry over task

		if (referencePointer.scope == SCOPE.GLOBAL) {

			// mark changemask as true so these values get distributed over the network.
			
			ApplyUpdateMessage (referencePointer.persistantData.getUpdateMessage (),true);


		} else {
			
			ApplyUpdateMessage (referencePointer.persistantData.getUpdateMessage ());

		}

	}

	public StoryTask (){

		// a minimal version with just data. no references. used for data carry-over task. 

		pointer = new StoryPointer ();

		setDefaults ();

	}

	public StoryTask (string myDescription, StoryPointer fromStoryPointer, string setID)
	{

		// Task created from network, so global scope and with passed in ID.

		description = myDescription;
		pointer = fromStoryPointer;
		ID = setID;
		scope = SCOPE.GLOBAL;

		setDefaults ();
		GENERAL.ALLTASKS.Add (this);
	}

	public StoryTask (StoryPointer fromStoryPointer, SCOPE setScope)
	{

		// Create a task based on the current storypoint of the pointer.
		// Note that setting scope is explicit, but in effect the scope of the task is the same as the scope of the pointer.

		description = fromStoryPointer.currentPoint.task [0];

		pointer = fromStoryPointer;
		ID = UUID.getGlobalID ();
		scope = setScope;

		setDefaults ();
		GENERAL.ALLTASKS.Add (this);
	}

	void setDefaults ()
	{

		signoffs = 0;

//		GENERAL.ALLTASKS.Add (this);

		taskIntValues = new Dictionary<string,int> ();
		taskFloatValues = new Dictionary<string,float> ();
		taskQuaternionValues = new Dictionary<string,Quaternion> ();
		taskVector3Values = new Dictionary<string,Vector3> ();
		taskStringValues = new Dictionary<string,string> ();


//		taskIntValues ["status"] = (Int32) TASKSTATUS.ACTIVE;

		#if NETWORKED
		taskValuesChangeMask = new Dictionary<string,bool> ();
		#endif

//		setStatus (TASKSTATUS.ACTIVE);
		taskIntValues ["status"] = (Int32)TASKSTATUS.ACTIVE;

		taskValuesChangeMask ["status"] = false;

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

		msg.updatedStringNames = new List<string> ();
		msg.updatedStringValues = new List<string> ();


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

		string[] stringNames = taskStringValues.Keys.ToArray ();

		foreach (string stringName in stringNames) {

			if (taskValuesChangeMask [stringName]) {

				msg.updatedStringNames.Add (stringName);

				taskValuesChangeMask [stringName] = false;

				string stringValue;

				if (taskStringValues.TryGetValue (stringName, out stringValue))
					msg.updatedStringValues.Add (stringValue);

			}

		}



		return msg;

	}

	public void ApplyUpdateMessage (TaskUpdate update, bool changeMask = false)
	{
		Debug.Log (me + "Applying network task update.");


		if (update.updatedIntNames.Contains("status")) {
			Debug.Log (me + "incoming task status change, setting pointerstatus to taskupdated.");

			pointer.setStatus (POINTERSTATUS.TASKUPDATED);
		}


		// check task status change

//		if (update.updatedIntNames ["status"] ) {
//			Debug.Log (me + "incoming task status change.");
//			pointer.setStatus (POINTERSTATUS.TASKUPDATED);
//
//
//		}
		
		for (int i = 0; i < update.updatedIntNames.Count; i++) {
			taskIntValues [update.updatedIntNames [i]] = update.updatedIntValues [i];
			taskValuesChangeMask [update.updatedIntNames [i]] = changeMask;

		}

		for (int i = 0; i < update.updatedFloatNames.Count; i++) {
			taskFloatValues [update.updatedFloatNames [i]] = update.updatedFloatValues [i];
			taskValuesChangeMask [update.updatedFloatNames [i]] = changeMask;

		}

		for (int i = 0; i < update.updatedQuaternionNames.Count; i++) {
			taskQuaternionValues [update.updatedQuaternionNames [i]] = update.updatedQuaternionValues [i];
			taskValuesChangeMask [update.updatedQuaternionNames [i]] = changeMask;

		}

		for (int i = 0; i < update.updatedVector3Names.Count; i++) {
			taskVector3Values [update.updatedVector3Names [i]] = update.updatedVector3Values [i];
			taskValuesChangeMask [update.updatedVector3Names [i]] = changeMask;

		}

		for (int i = 0; i < update.updatedStringNames.Count; i++) {
			taskStringValues [update.updatedStringNames [i]] = update.updatedStringValues [i];
			taskValuesChangeMask [update.updatedStringNames [i]] = changeMask;

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

	public void setStringValue (string valueName, string value)
	{

		taskStringValues [valueName] = value;

		#if NETWORKED
		taskValuesChangeMask [valueName] = true;
		hasChanged = true;
		#endif

	}

	public bool getStringValue (string valueName, out string value)
	{

		if (!taskStringValues.TryGetValue (valueName, out value)) {
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

//	public void setStringValue (string valueName, string value)
//	{
//
//		taskStringValues [valueName] = value;
//
//		#if NETWORKED
//		taskValuesChangeMask [valueName] = true;
//		hasChanged = true;
//		#endif
//
//	}
//
//	public bool getStringValue (string valueName, out string value)
//	{
//
//		if (!taskStringValues.TryGetValue (valueName, out value)) {
//			return false;
//		}
//
//		return true;
//
//	}

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



	//	public void setIntValue (string valueName, Int32 value)
	//
	//	{
	//
	//		taskIntValues [valueName] = value;
	//
	//		#if NETWORKED
	//		taskValuesChangeMask [valueName] = true;
	//		hasChanged = true;
	//		#endif
	//
	//	}
	//
	//	public bool getIntValue (string valueName, out Int32 value)
	//
	//	{
	//
	//		if (!taskIntValues.TryGetValue (valueName, out value)) {
	//			return false;
	//		}
	//
	//		return true;
	//
	//	}


	void setPointerToUpdated (){

		switch (GENERAL.SCOPE) {

		case SCOPE.GLOBAL:
		case SCOPE.SOLO:
			// we're the global server or running solo so we can trigger the pointer. regardless of the task's scope.

			pointer.setStatus (POINTERSTATUS.TASKUPDATED);

			break;

		case SCOPE.LOCAL:

			// we're a local client. only if the task is local do we trigger the pointer.

			if (scope == SCOPE.LOCAL) {

				pointer.setStatus (POINTERSTATUS.TASKUPDATED);

			}

			break;

		default:


			break;



		}



	}

	public void setStatus (TASKSTATUS theStatus)
	{
		
//		status = theStatus;

		taskIntValues ["status"] = (Int32)theStatus;

		setPointerToUpdated ();

//		switch (GENERAL.SCOPE) {
//
//		case SCOPE.GLOBAL:
//		case SCOPE.SOLO:
//			// we're the global server or running solo so we can trigger the pointer. regardless of the task's scope.
//
//			pointer.setStatus (POINTERSTATUS.TASKUPDATED);
//
//			break;
//
//		case SCOPE.LOCAL:
//
//			// we're a local client. only if the task is local do we trigger the pointer.
//
//			if (scope == SCOPE.LOCAL) {
//				
//				pointer.setStatus (POINTERSTATUS.TASKUPDATED);
//
//			}
//
//			break;
//
//		default:
//
//
//			break;
//
//
//
//		}

//		pointer.setStatus (POINTERSTATUS.TASKUPDATED);




		#if NETWORKED

		taskValuesChangeMask ["status"] = true;
		hasChanged = true;

		#endif

	}

	public TASKSTATUS getStatus ()
	{

		Int32 value;

		if (!taskIntValues.TryGetValue ("status", out value)) {
					
			Debug.LogError (me + "status value for task not found");

			value = (Int32)TASKSTATUS.ACTIVE;
		} 

		return (TASKSTATUS)value;

	}


			
	void complete ()
	{

		if (getStatus () != TASKSTATUS.COMPLETE) {

			// make sure a task is only completed once.
			
			setStatus (TASKSTATUS.COMPLETE);

//			pointer.taskStatusChanged ();

//			pointer.setStatus (POINTERSTATUS.TASKUPDATED);


		} else {
			Debug.LogWarning (me + "A task was completed more than once.");

		}

	}

	public void setCallBack (string theCallBackPoint)
	{

//		Debug.Log ("performing callback: " + theCallBackPoint);

//		callBackPoint = theCallBackPoint;


		setStringValue ("callBackPoint", theCallBackPoint);


//		setStatus (TASKSTATUS.CALLBACK);

		setPointerToUpdated ();
	
//		pointer.setStatus (POINTERSTATUS.TASKUPDATED);

//		#if NETWORKED
//
//		taskValuesChangeMask ["callBackPoint"] = true;
//		hasChanged = true;
//
//		#endif

	}

	public void clearCallBack(){

		setStringValue ("callBackPoint", "");


	}


	public string getCallBack (){

		string value;

		if (getStringValue("callBackPoint",out value)){

			return value;

		} else {
			
			return ("");
		}

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
//	CALLBACK,
	COMPLETE,
//	CLEANUP

}

public class TaskArgs : EventArgs
{

	public List <StoryTask> theTasks;

	public TaskArgs (List <StoryTask> tasks) : base () // extend the constructor 
	{ 
		theTasks = tasks;
	}

	public TaskArgs (StoryTask task) : base () // extend the constructor 
	{ 
		theTasks = new List <StoryTask> ();
		theTasks.Add (task);
	}

}

