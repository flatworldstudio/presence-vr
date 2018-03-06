
using System;
using UnityEngine;
using System.Collections;



public enum DEVICEMODE
{
	NONE,
	SERVER,
	VRCLIENT

}



public static class PRESENCE {

	// KINECT INFO
//	public static GameObject kinectObject;
	public static DEVICEMODE deviceMode;


	public static float north=0;

	public static float kinectHeight =  1.175f;

	public static float kinectHeading = 45f;

	public static float kinectCentreDistance = Mathf.Sqrt(2f) *2f;
	public static bool kinectIsOrigin =true;


//	public static SpatialData pKinect;

	public static float mobileInitialHeading=-1;
	public static float mobileInitialHeading1=-1;


	public static float vrHeadOffset=0;

	public static bool isOverview = true;

	public static int frame;

	public static bool capturing;

	public static CloudSequence capture;
	public static int CaptureFrame;
	public static int sessionLength=200;
	public static int captureLength = 150;
	public static int echoOffset = 25;


	public static int FrameSize;
	public static Vector3[] PointCloud;
	public static float TimeStamp;
}






