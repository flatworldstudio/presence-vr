
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PresenceEngine
{

    public enum DEVICEMODE
    {
        NONE,
        SERVER,
        VRCLIENT

    }



    public static class SETTINGS
    {

        public static string SelectedFolder, SelectedFile;
        public static bool UserInConfinedArea = true;

        public static int SessionDuration = 10*60; // this is a time out, not fixed length or buffer size.

   //     public static string DEFAULTFILE = "/default/default.prs";
  //      public static string DEFAULTFOLDER = "/default";

        public static Dictionary<string, Presence> Presences;
        public static Presence user; // this is a convience shortcut, set to the "user" entry in the dictionary.

        public static Camera ActiveCamera;

        public static string DefaultVisualiser = "PointShaded";
        public static bool ManualPlayback;

    //s    public static DEVICEMODE deviceMode;


        public static Quaternion HeadsetCorrection = Quaternion.identity;




        public static float SensorY = 1.25f;

        public static float kinectHeading = 45f;
        public static float kinectCentreDistance = Mathf.Sqrt(2f) * 2f;
        public static bool kinectIsOrigin = true;


    }






}