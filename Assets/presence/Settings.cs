﻿
using System;
using UnityEngine;
using System.Collections;

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
        public static int SessionDuration = 3;
        public static  string  DEFAULTFILE = "/_default/_default";

        public static  string  DEFAULTFOLDER = "/_default";


        public static Presence[] Presences;
   //     public static Presence MainPresence;
        // KINECT INFO
        //	public static GameObject kinectObject;
        public static DEVICEMODE deviceMode;

        public static DepthTransport MainDepthTransport;



        public static float north = 0;

        public static float kinectHeight = 1.35f;

        public static float kinectHeading = 45f;

        public static float kinectCentreDistance = Mathf.Sqrt(2f) * 2f;
        public static bool kinectIsOrigin = true;


        //	public static DepthTransport pKinect;

        public static float mobileInitialHeading = -1;
        public static float mobileInitialHeading1 = -1;


        public static float vrHeadOffset = 0;

        public static bool isOverview = true;

        public static int frame;

        public static bool capturing;

        public static CloudSequence capture;
        public static int CaptureFrame;
   
        public static int captureLength = 2500;
        public static int echoOffset = 25;


        public static int FrameSize;
        public static Vector3[] PointCloud;
        public static float TimeStamp;
    }






}