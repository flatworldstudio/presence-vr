
using System;
using UnityEngine;
using System.Collections;



public enum STORYMODE
{
	NOCHANGE,
	OVERVIEW,
	VIEWER

}



public static class PRESENCE {

	// KINECT INFO
//	public static GameObject kinectObject;
	public static float north=0;
	public static float kinectHeight =  1.175f;
	public static float kinectHeading = 25;
	public static float kinectHomeDistance =2;

	public static PKinect pKinect;

	public static float mobileInitialHeading=-1;
	public static float mobileInitialHeading1=-1;


	public static float vrHeadOffset=0;

	public static bool isOverview = true;

	public static int frame;

	public static bool capturing;


}






