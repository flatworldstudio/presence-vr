using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StoryEngine;

public class Gestures : MonoBehaviour, KinectGestures.GestureListenerInterface
{

    StoryTask TaskRef;
    bool active = false;
    bool userPresent = false;
    uint User;

    public void BeginDetect(StoryTask task)
    {
        TaskRef = task;
        // this.gameObject.SetActive(true);
        if (userPresent)
        {
           
            KinectManager.Instance.DetectGesture(User, KinectGestures.Gestures.Tpose);
            active = true;
            
        }
       
    }
   

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        
		
	}

    void KinectGestures.GestureListenerInterface.UserDetected(uint userId, int userIndex)
    {
        //     throw new System.NotImplementedException();
        Debug.Log("user is detected");

        User = userId;
        userPresent = true;

        if (!active && TaskRef != null){
            KinectManager.Instance.DetectGesture(User, KinectGestures.Gestures.Tpose);
            active = true;

       }

     
      //  KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.Tpose);
    }

    void KinectGestures.GestureListenerInterface.UserLost(uint userId, int userIndex)
    {
      //  Debug.LogError("Lost user");

       
        userPresent = false;

        //  throw new System.NotImplementedException();
    }

    void KinectGestures.GestureListenerInterface.GestureInProgress(uint userId, int userIndex, KinectGestures.Gestures gesture, float progress, KinectWrapper.NuiSkeletonPositionIndex joint, Vector3 screenPos)
    {
        // throw new System.NotImplementedException();
     //   Debug.Log("RAISED");

       

    }

    bool KinectGestures.GestureListenerInterface.GestureCompleted(uint userId, int userIndex, KinectGestures.Gestures gesture, KinectWrapper.NuiSkeletonPositionIndex joint, Vector3 screenPos)
    {
        //  throw new System.NotImplementedException();
        Debug.LogError("DETECT");
        if (TaskRef != null)
            TaskRef.setStringValue("status", "detected");
        return true;
    }

    bool KinectGestures.GestureListenerInterface.GestureCancelled(uint userId, int userIndex, KinectGestures.Gestures gesture, KinectWrapper.NuiSkeletonPositionIndex joint)
    {
      // Debug.Log("......");
        //    throw new System.NotImplementedException();
        return true;
    }
}


/*
public interface GestureListenerInterface
{
    // Invoked when a new user is detected and tracking starts
    // Here you can start gesture detection with KinectManager.DetectGesture()
    void UserDetected(uint userId, int userIndex);

    // Invoked when a user is lost
    // Gestures for this user are cleared automatically, but you can free the used resources
    void UserLost(uint userId, int userIndex);

    // Invoked when a gesture is in progress 
    void GestureInProgress(uint userId, int userIndex, Gestures gesture, float progress,
                           KinectWrapper.NuiSkeletonPositionIndex joint, Vector3 screenPos);

    // Invoked if a gesture is completed.
    // Returns true, if the gesture detection must be restarted, false otherwise
    bool GestureCompleted(uint userId, int userIndex, Gestures gesture,
                          KinectWrapper.NuiSkeletonPositionIndex joint, Vector3 screenPos);

    // Invoked if a gesture is cancelled.
    // Returns true, if the gesture detection must be retarted, false otherwise
    bool GestureCancelled(uint userId, int userIndex, Gestures gesture,
                          KinectWrapper.NuiSkeletonPositionIndex joint);
}
*/