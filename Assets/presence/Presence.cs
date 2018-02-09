
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

	public static float mobileInitialHeading=-1;
	public static float mobileInitialHeading1=-1;

//	public static 
//	public static Vector3 kinectPosition;
	public static float kinectHeight =  1.175f;
	public static float kinectHeading = 25;
	public static float kinectHomeDistance =2;
	public static Vector3 kinectPosition;
	public static Quaternion kinectRotation;


	public static bool isOverview = true;





}






