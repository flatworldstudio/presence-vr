using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;


public delegate void OnClientMessageHandler (NetworkMessage netMessage);
public delegate void messageHandler (NetworkMessage netMessage);

public delegate void OnStartServerDelegate ();
public delegate void OnStartClientDelegate (NetworkClient theClient);

public delegate void OnServerConnectDelegate (NetworkConnection connection);
public delegate void OnClientConnectDelegate (NetworkConnection connection);
public delegate void OnClientDisconnectDelegate (NetworkConnection connection);

public class ExtendedNetworkManager : NetworkManager
{
	string me = "Network Manager: ";

	const short testCode = 1001;

	public OnStartServerDelegate onStartServerDelegate;
	public OnStartClientDelegate onStartClientDelegate;
	public OnServerConnectDelegate onServerConnectDelegate;
	public OnClientConnectDelegate onClientConnectDelegate;
	public OnClientDisconnectDelegate onClientDisconnectDelegate;

	public override void OnClientDisconnect (NetworkConnection connection)
	{

		Debug.LogWarning (me+"Disconnected");

		if (onClientDisconnectDelegate != null)
			onClientDisconnectDelegate (connection);

	}

	public override void OnStartServer ()

	{
		Debug.Log (me + "Server started.");

		if (onStartServerDelegate != null)
			onStartServerDelegate ();
		
		NetworkServer.RegisterHandler (testCode, onServerTestMessage);
	
	}

	public override void OnStartClient (NetworkClient theClient)

	{
		Debug.Log (me + "Client started.");

		if (onStartClientDelegate != null)
			onStartClientDelegate (theClient);

		client.RegisterHandler (testCode, onClientTestMessage);

	}

	public override void OnServerConnect (NetworkConnection connection)

	{
		Debug.Log (me + "Incoming client connection added.");

		if (onServerConnectDelegate != null)
			onServerConnectDelegate (connection);
		
	}

	public override void OnClientConnect (NetworkConnection conn)

	{
		Debug.Log (me + "Connected as client.");

		ClientScene.Ready (conn);

		if (onClientConnectDelegate != null)
			onClientConnectDelegate (conn);

		testMessageToServer ("Hello server.");
	}

	public void testMessageToServer (string value)
	{
		var msg = new StringMessage (value);
		client.Send (testCode, msg);
		Debug.Log (me + "Sending message to server: " + value);
	}

	public void testMessageToClients (string value)
	{
		var msg = new StringMessage (value);
		NetworkServer.SendToAll (testCode, msg);
		Debug.Log (me + "Sending message to all clients: " + value);
	}

	void onClientTestMessage (NetworkMessage netMsg)
	{
		var message = netMsg.ReadMessage<StringMessage> ();
		Debug.Log (me + "Test message from server: " + message.value);
	}

	void onServerTestMessage (NetworkMessage netMsg)
	{
		var message = netMsg.ReadMessage<StringMessage> ();
		Debug.Log (me + "Test message from client: " + message.value);

		if (message.value.Equals ("Hello server."))
			testMessageToClients ("Hello new client.");

	}

}
