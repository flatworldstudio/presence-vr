﻿#if !UNITY_IOS && !UNITY_ANDROID

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StoryEngine;

public class GestureDetection : MonoBehaviour, KinectGestures.GestureListenerInterface
{

    StoryTask TaskRef;
    bool active = false;
    bool userPresent = false;
    uint User;
    KinectGestures.Gestures Gesture;

    public static GestureDetection Instance;

    public void BeginDetect(StoryTask task, KinectGestures.Gestures gesture)
    {
        TaskRef = task;
        Gesture = gesture;
       
        if (userPresent)
        {

            KinectManager.Instance.DetectGesture(User, Gesture);
            active = true;
            
        }
       
    }

    public void EndDetect()
    {
        KinectManager.Instance.ClearGestures(User);
        
        Debug.Log("Ending gesture detection.");

        TaskRef = null;
        active = false;

        Gesture = KinectGestures.Gestures.None;
    }

    void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown("d")){
            
            if (TaskRef != null)
                TaskRef.SetStringValue("status", "detected");
            
        }
          
    }
    void KinectGestures.GestureListenerInterface.UserDetected(uint userId, int userIndex)
    {
        //     throw new System.NotImplementedException();
    //    Debug.Log("Gestures: user is detected");

        if (TaskRef != null)
            TaskRef.SetStringValue("debug", "User detected...");

        User = userId;
        userPresent = true;

        if (!active && TaskRef != null){

            KinectManager.Instance.DetectGesture(User, Gesture);
            active = true;

       }
             
    }

    void KinectGestures.GestureListenerInterface.UserLost(uint userId, int userIndex)
    {
    //    Debug.Log("Gestures: user is lost");
        
        userPresent = false;

        if (TaskRef != null)
            TaskRef.SetStringValue("debug", "User lost...");
        //  throw new System.NotImplementedException();
    }

    void KinectGestures.GestureListenerInterface.GestureInProgress(uint userId, int userIndex, KinectGestures.Gestures gesture, float progress, KinectWrapper.NuiSkeletonPositionIndex joint, Vector3 screenPos)
    {
        // throw new System.NotImplementedException();
      //  Debug.Log("IN PROGRESS");
        if (TaskRef != null)
            TaskRef.SetStringValue("debug", "In progress...");
    }

    bool KinectGestures.GestureListenerInterface.GestureCompleted(uint userId, int userIndex, KinectGestures.Gestures gesture, KinectWrapper.NuiSkeletonPositionIndex joint, Vector3 screenPos)
    {
        //  throw new System.NotImplementedException();
     //   Debug.Log("DETECT");

        if (TaskRef != null)
        {
            TaskRef.SetStringValue("status", "detected");
            TaskRef.SetStringValue("debug", "Detected");
        }
           
        return true;
    }

    bool KinectGestures.GestureListenerInterface.GestureCancelled(uint userId, int userIndex, KinectGestures.Gestures gesture, KinectWrapper.NuiSkeletonPositionIndex joint)
    {
        //    throw new System.NotImplementedException();

        Debug.Log("Gestures: cancelled");

        return true;
    }
}
#endif
