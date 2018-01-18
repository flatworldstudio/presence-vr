using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;




public class NetworkBroadcast : NetworkDiscovery {


	public string serverAddress, serverMessage;


	public override void OnReceivedBroadcast (string fromAddress, string data){

		Debug.Log ("Received broadcast: " + data );

		serverMessage = data;
		serverAddress = fromAddress;

		NetworkManager.singleton.networkAddress = fromAddress;

		NetworkManager.singleton.StartClient ();

	}


}
