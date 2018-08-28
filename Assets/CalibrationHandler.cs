using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StoryEngine;

public class CalibrationHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnConnectionEvent (bool value)
    {
        if (value)
        {
            Debug.Log("Arduino Connected");
        }
        else
        {
            Debug.Log("Arduino Disconnected");
        }
      
    }
    public void OnMessageArrived (string value)
    {
        Debug.Log("Arduino message: " + value);
        Director.Instance.beginStoryLine("calibratenow");
    }
}
