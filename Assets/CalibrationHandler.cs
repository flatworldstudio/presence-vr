using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StoryEngine;

public class CalibrationHandler : MonoBehaviour
{
   public GameObject SerialController;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown("c"))
            Triggered();

    }

    public void OnConnectionEvent(bool value)
    {
        if (value)
        {
            Debug.Log("Arduino Connected");
        }
        else
        {
            Debug.LogWarning("Arduino Disconnected, Aborting Serial Controller");
            SerialController.SetActive(false);
        }

    }
    public void OnMessageArrived(string value)
    {
        Debug.Log("Arduino message: " + value);

        if (value == "CALIBRATE")
            Triggered();

    }

    void Triggered()
    {
        Director.Instance.beginStoryLine("calibratenow");
    }

}
