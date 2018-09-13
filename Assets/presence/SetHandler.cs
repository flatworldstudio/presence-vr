using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using Random = UnityEngine.Random;
using StoryEngine;


namespace PresenceEngine
{

    public class SetHandler : MonoBehaviour
    {

#if SERVER && (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)

        //      KinectManager kinectManager, kinectManagerObject;
#endif


        public GameObject PresenceObjectPrefab, presences;
        public SetController setController;
        public AudioSource VoiceOver;
        public GameObject Set, KinectObject, ViewerRoot, KinectGizmo;
        ParticleCloud[] clouds;
        public GameObject MoodParticles;
        public LightControl MainStageLight, MoodLight01, MoodLight02;
        //   public GameObject Circle;
        ushort[] depthMap;
        int width, height;

        public RawImage PreviewImage;
        string me = "Set handler: ";

        //int interval = -1;
        //int interval2 = 0;
        Quaternion q;
        Vector3 p;
        GameObject c, g;

        float serverFrameStamp, clientFrameStamp;
        float serverFrameRate, clientFrameRate;

        //float targetFrameRate = 0.15f;
        //float safeFrameRate = 0.1f;

        //int droppedFrames = 0;

        void Start()
        {

            setController.addTaskHandler(TaskHandler);

            /*
#if SERVER && (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)

              kinectManager = kinectManagerObject.GetComponent<KinectManager> ();

#endif
*/

        }

        int dataSize;
        ushort[] newFrame;
        int sample;
        Vector3 point;

        int particleIndex;
        int count;
        ParticleCloud cloud, mirror;
        //      int frame = -1;

        public bool TaskHandler(StoryTask task)
        {

            bool done = false;

            switch (task.description)
            {

                // ------------------------------------------------------------------------------------------------------------------------
                // Guide VO Playback
                // Note: make this work on remote only.
                case "ResetGuided":
#if VOICEOFF

                    VoiceOver.volume = 0f;
#endif
                    done = true;
                    break;


                case "G01_Opening":
                    
                    if (PlayVoiceOver("G01_Opening", task))
                    {
                        //  task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                    {
                        //    Debug.LogWarning("VO TASK KILLED");
                        done = true;
                    }


                    break;

                case "G02_Position":

                    if (PlayVoiceOver("G02_Position", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G03_Lifthands":

                    if (PlayVoiceOver("G03_Lifthands", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G04_Seehands":

                    if (PlayVoiceOver("G04_Seehands", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G05_Draw":

                    if (PlayVoiceOver("G05_Draw", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G06_Watchdrawing":

                    if (PlayVoiceOver("G06_Watchdrawing", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G07_Return":

                    if (PlayVoiceOver("G07_Return", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G08_ThirdpersonIntro":

                    if (PlayVoiceOver("G08_ThirdpersonIntro", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G09_Thirdperson":

                    if (PlayVoiceOver("G09_Thirdperson", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G10_Return":

                    if (PlayVoiceOver("G10_Return", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G11_Closeeyes":

                    if (PlayVoiceOver("G11_Closeeyes", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G12_Mirror":

                    if (PlayVoiceOver("G12_Mirror", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G13_Notlinear":

                    if (PlayVoiceOver("G13_Notlinear", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G14_Backward":

                    if (PlayVoiceOver("G14_Backward", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G15_Timestop":

                    if (PlayVoiceOver("G15_Timestop", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G16_Shatter":

                    if (PlayVoiceOver("G16_Shatter", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G17_Circle":

                    if (PlayVoiceOver("G17_Circle", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G18_Followcircle":

                    if (PlayVoiceOver("G18_Followcircle", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G19_Otherselves":

                    if (PlayVoiceOver("G19_Otherselves", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G20_Continue":

                    if (PlayVoiceOver("G20_Continue", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G21_Stepaway":

                    if (PlayVoiceOver("G21_Stepaway", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G22_Timespace":

                    if (PlayVoiceOver("G22_Timespace", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G23_Others":

                    if (PlayVoiceOver("G23_Others", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G24_Sitdown":

                    if (PlayVoiceOver("G24_Sitdown", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;

                    break;

                case "G25_Endsession":

                    if (PlayVoiceOver("G25_Endsession", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;



                    break;

                case "G26_Removeheadset":

                    if (PlayVoiceOver("G26_Removeheadset", task))
                    {
                        //    task.SetStringValue("debug", "" + VoiceOver.time);
                    }
                    else
                        done = true;



                    break;



                // ----------------------------------------------------------------------------------------------------
                //  Visualisation effects.

                case "ShowCircle":

                    Circle.Instance.StartDrawing();
                    done = true;

                    break;

                case "HideCircle":

                    //      Circle.SetActive(false);
                    Circle.Instance.StopDrawing();

                    done = true;
                    break;


#if SERVER
                case "DrawingOn":




                    SETTINGS.user.SetVisualiser("ShowSkeleton", 1);

                    task.SetIntValue("user_1_isdrawing", 1);
                    SETTINGS.user.PullVisualiserSettingsFromTask(task, "user");



                    done = true;

                    break;

                case "DrawingOff":



                    task.SetIntValue("user_1_isdrawing", 0);
                    SETTINGS.user.PullVisualiserSettingsFromTask(task, "user");



                    done = true;

                    break;

                case "DrawingRemove":




                    SETTINGS.user.SetVisualiser("", 1);



                    done = true;

                    break;
#endif


                // ------------------------------------------------------------------------------------------------------------------------
                // Ambience scripts. 


                case "moodlight":

                    float MainPerlinStart, MainPerlin, MoodLight01PerlinStart, MoodLight02PerlinStart, MoodLight01Perlin, MoodLight02Perlin;

#if SERVER || CLIENT

                    // Initiate values if not yet present. (When client connects to server, the server will force new values)

                    if (!task.GetFloatValue("mainperlinstart", out MainPerlinStart))
                        task.SetFloatValue("mainperlinstart", Random.Range(-100f, 100f));

                    task.GetFloatValue("mainperlin", out MainPerlin);
                    task.SetFloatValue("mainperlin", MainPerlin += 0.05f);

                    if (!task.GetFloatValue("moodlight01perlinstart", out MoodLight01PerlinStart))
                        task.SetFloatValue("moodlight01perlinstart", Random.Range(-100f, 100f));

                    task.GetFloatValue("moodlight01perlin", out MoodLight01Perlin);
                    task.SetFloatValue("moodlight01perlin", MoodLight01Perlin += 0.05f);

                    if (!task.GetFloatValue("moodlight02perlinstart", out MoodLight02PerlinStart))
                        task.SetFloatValue("moodlight02perlinstart", Random.Range(-100f, 100f));

                    task.GetFloatValue("moodlight02perlin", out MoodLight02Perlin);
                    task.SetFloatValue("moodlight02perlin", MoodLight02Perlin += 0.05f);


#endif

#if CLIENT

                    // We are a connected vr client so we'll load the values all the time, in case they change server side. (Ie on connection)

                    task.GetFloatValue("mainperlinstart", out MainPerlinStart);
                    task.GetFloatValue("mainperlin", out MainPerlin);
                    task.GetFloatValue("moodlight01perlinstart", out MoodLight01PerlinStart);
                    task.GetFloatValue("moodlight01perlin", out MoodLight01Perlin);
                    task.GetFloatValue("moodlight02perlinstart", out MoodLight02PerlinStart);
                    task.GetFloatValue("moodlight02perlin", out MoodLight02Perlin);

#endif

                    // And now we apply those values.

                    MainStageLight.Variation = 1f * Mathf.PerlinNoise(MainPerlinStart + MainPerlin, 0);
                    MoodLight01.Variation = 1f * Mathf.PerlinNoise(MoodLight01PerlinStart + MoodLight01Perlin, 0);
                    MoodLight02.Variation = 1f * Mathf.PerlinNoise(MoodLight02PerlinStart + MoodLight02Perlin, 0);

                    // Dim light if user leaves confined area.

                    if (SETTINGS.UserInConfinedArea)
                    {

                        MainStageLight.Master = 1.5f;

                    }
                    else
                    {

                        MainStageLight.Master = 0.1f;

                    }


                    break;

                case "moodon":

                    MoodParticles.SetActive(true);

                    done = true;
                    break;

                case "moodoff":

                    MoodParticles.SetActive(false);

                    done = true;
                    break;

#if SERVER
                case "displaycheck":

                    Debug.Log(me + "displays connected: " + Display.displays.Length);

                    // Display.displays[0] is the primary, default display and is always ON.
                    // Check if additional displays are available and activate each.

                    if (Display.displays.Length > 1)
                    {
                        Display.displays[1].Activate();

                    }

                    if (Display.displays.Length > 2)
                    {
                        Display.displays[2].Activate();

                    }

                    done = true;

                    break;
#endif

                case "addkinectnull":

                    g = DebugObject.getNullObject(0.5f, 0.5f, 0.5f);
                    g.transform.SetParent(KinectGizmo.transform, false);

                    done = true;

                    break;

                case "alignset":

                    // Position set and user relative to kinect.

                    if (SETTINGS.kinectIsOrigin)
                    {

                        // Rotate entire set.
                        //SETTINGS.north = -SETTINGS.kinectHeading;

                        Set.transform.rotation = Quaternion.Euler(0, -SETTINGS.kinectHeading, 0);

                        // Get center and offset the set. So that kinect is at origin.
                        p = SETTINGS.kinectCentreDistance * Vector3.forward;
                        Set.transform.position = p;


                        // Position user at center of field, keeping height. (a default value of 1.8)
                        p.y = ViewerRoot.transform.position.y;
                        ViewerRoot.transform.position = p;

                        // Place kinect object at origin at the appropriate height.
                        p = Vector3.zero;
                        p.y = SETTINGS.SensorY;
                        KinectObject.transform.position = p;

                    }
                    else
                    {

                        Debug.LogWarning("Kinect not at origin, not aligning.");

                    }

                    done = true;

                    break;




                default:

                    done = true;

                    break;

            }

            return done;

        }

        // default callibration values. c being a correction presumably and f the fov


        //double fx_d = 1.0d / 5.9421434211923247e+02;
        //double fy_d = 1.0d / 5.9104053696870778e+02;
        //double cx_d = 3.3930780975300314e+02;
        //double cy_d = 2.4273913761751615e+02;


        //Vector3 depthToWorld(int x, int y, int depthValue)
        //{

        //    Vector3 result = Vector3.zero;

        //    //double depth = depthLookUp [depthValue];
        //    //float depth = rawDepthToMeters(depthValue);

        //    float depth = depthValue / 1000f;

        //    result.x = (float)((x - cx_d) * depth * fx_d);
        //    result.y = (float)((y - cy_d) * depth * fy_d);
        //    result.z = (float)(depth);

        //    return result;

        //}


        //public void ComputeParticles(ushort[] DepthMap, int Width, int Height, int Sample, Vector3 Transform, ParticleCloud Cloud)
        //{

        //    // takes a kinect styled uint[] RawDepthMap, Width, Height.
        //    // and plots the points (at sample intervals) into a Particle Cloud with a given Transform.

        //    int ParticleIndex = 0;
        //    int Scale = 640 / Width;

        //    for (int y = 0; y < Height; y += Sample)
        //    {

        //        for (int x = 0; x < Width; x += Sample)
        //        {

        //            int i = y * Width + x;

        //            ushort userMap = (ushort)(DepthMap[i] & 7);
        //            ushort userDepth = (ushort)(DepthMap[i] >> 3);

        //            if (userMap != 0)
        //            {

        //                point = depthToWorld(x * Scale, y * Scale, userDepth);
        //                point.x = -point.x;
        //                point.y = -point.y;
        //                point += Transform;

        //                Cloud.allParticles[ParticleIndex].position = point;

        //                ParticleIndex++;
        //                if (ParticleIndex == Cloud.allParticles.Length)
        //                {
        //                    // abort
        //                    x = Width;
        //                    y = Height;

        //                }

        //                // ParticleIndex = ParticleIndex % Cloud.allParticles.Length;

        //            }

        //        }

        //    }

        //    Cloud.ApplyParticles(ParticleIndex);

        //}

        string CurrentClip = "";


        bool PlayVoiceOver(string clipName, StoryTask taskRef)
        {
            // returns true if the specified clip is playing
            // returns false if the specified clip is done or the clip was changed
#if SERVER
            string id = "server";
#endif
#if CLIENT

            string id = "client";
#endif

            string vostatus;

            if (!taskRef.GetStringValue(id, out vostatus))
                vostatus = "start";

            switch (vostatus)
            {
                case "start":

                    taskRef.SetStringValue(id, clipName);
                    AudioClip clip = (AudioClip)Resources.Load("audio/" + clipName);

                    VoiceOver.clip = clip;
                    VoiceOver.Play();
                    CurrentClip = clipName;

                    return true;
                //break;

                default:

                    if (VoiceOver.isPlaying && vostatus == CurrentClip)
                        return true;
                    else
                        return false;

            }



        }

        void Update()
        {

        }

    }
}