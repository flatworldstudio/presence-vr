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
        public GameObject PresenceObjectPrefab, presences;
        public SetController setController;

        public AudioSource VoiceOver;

        public GameObject Set, KinectObject, ViewerRoot;


        public GameObject kinectManagerObject;

        ParticleCloud[] clouds;

        //  #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        //KinectManager kinectManager;

        //  #endif

        public GameObject MoodParticles;
        public LightControl MainStageLight, MoodLight01, MoodLight02;

        ushort[] depthMap;
        int width, height;

        public RawImage PreviewImage;

        string me = "Task handler: ";

        int interval = -1;
        int interval2 = 0;
        Quaternion q;
        Vector3 p;
        GameObject c, g;

        float serverFrameStamp, clientFrameStamp;
        float serverFrameRate, clientFrameRate;


        float targetFrameRate = 0.15f;
        float safeFrameRate = 0.1f;

        int droppedFrames = 0;


        void Start()
        {

            setController.addTaskHandler(TaskHandler);

            //  #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            //  kinectManager = kinectManagerObject.GetComponent<KinectManager> ();

            //#endif
        }

        int dataSize;
        ushort[] newFrame;
        int sample;
        Vector3 point;

        int particleIndex;
        int count;
        ParticleCloud cloud, mirror;
        int frame = -1;

        public bool TaskHandler(StoryTask task)
        {

            bool done = false;

            switch (task.description)
            {

                // ------------------------------------------------------------------------------------------------------------------------
                // Guide VO Playback
                // Note: make this work on remote only.

                case "GuideOpening":

                    if (PlayVoiceOver("GuideOpening", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideSitdown":

                    if (PlayVoiceOver("GuideSitdown", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideBegin":

                    if (PlayVoiceOver("GuideBegin", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideObservehands":

                    if (PlayVoiceOver("GuideObservehands", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideDrawing":

                    if (PlayVoiceOver("GuideDrawing", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideObservedrawing":

                    if (PlayVoiceOver("GuideObservedrawing", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideGetup":

                    if (PlayVoiceOver("GuideGetup", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideThirdpersonintro":

                    if (PlayVoiceOver("GuideThirdpersonintro", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideThirdperson":

                    if (PlayVoiceOver("GuideThirdperson", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideThirdpersonexit":

                    if (PlayVoiceOver("GuideThirdpersonexit", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideReturntocenter":

                    if (PlayVoiceOver("GuideReturntocenter", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideMirrorintro":

                    if (PlayVoiceOver("GuideMirrorintro", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideMirror":

                    if (PlayVoiceOver("GuideMirror", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideSlowdown":

                    if (PlayVoiceOver("GuideSlowdown", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideReverse":

                    if (PlayVoiceOver("GuideReverse", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideFreeze":

                    if (PlayVoiceOver("GuideFreeze", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideShatter":

                    if (PlayVoiceOver("GuideShatter", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideCircleintro":

                    if (PlayVoiceOver("GuideCircleintro", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideCircle":

                    if (PlayVoiceOver("GuideCircle", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideCircleclone":

                    if (PlayVoiceOver("GuideCircleclone", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideCircleobserve":

                    if (PlayVoiceOver("GuideCircleobserve", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideNotalone":

                    if (PlayVoiceOver("GuideNotalone", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuidePresences":

                    if (PlayVoiceOver("GuidePresences", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideEndpresences":

                    if (PlayVoiceOver("GuideEndpresences", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "GuideEndsession":

                    if (PlayVoiceOver("GuideEndsession", task))
                    {
                        if (VoiceOver.isPlaying)
                        {
                            task.SetStringValue("debug", "" + VoiceOver.time + VoiceOver.isPlaying);
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;


                    // ----------------------------------------------------------------------------------------------------
                    //  Visualisation effects.

                case "DrawingOn":


                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {

                        SETTINGS.user.SetVisualiser("ShowSkeleton",1);

                        task.SetIntValue("user_1_isdrawing", 1);
                        SETTINGS.user.PullVisualiserSettingsFromTask(task, "user");

                    }

                    done = true;

                    break;

                case "DrawingOff":


                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        task.SetIntValue("user_1_isdrawing", 0);
                        SETTINGS.user.PullVisualiserSettingsFromTask(task, "user");

                    }

                    done = true;

                    break;

                case "DrawingRemove":


                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {

                        SETTINGS.user.SetVisualiser("", 1);

                    }

                    done = true;

                    break;

              
                // ------------------------------------------------------------------------------------------------------------------------
                // Ambience scripts. 


                case "moodlight":

                    float MainPerlinStart, MainPerlin, MoodLight01PerlinStart, MoodLight02PerlinStart, MoodLight01Perlin, MoodLight02Perlin;

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER || (SETTINGS.deviceMode == DEVICEMODE.VRCLIENT && !GENERAL.wasConnected))
                    {

                        // We are either the server or an unconnected vrclient so we take the lead.

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


                    }
                    else
                    {

                        // We are a connected vr client so we'll take the lead from the server.

                        task.GetFloatValue("mainperlinstart", out MainPerlinStart);
                        task.GetFloatValue("mainperlin", out MainPerlin);
                        task.GetFloatValue("moodlight01perlinstart", out MoodLight01PerlinStart);
                        task.GetFloatValue("moodlight01perlin", out MoodLight01Perlin);
                        task.GetFloatValue("moodlight02perlinstart", out MoodLight02PerlinStart);
                        task.GetFloatValue("moodlight02perlin", out MoodLight02Perlin);


                    }

                    // And now we apply those values.

                    MainStageLight.Variation = 1f * Mathf.PerlinNoise(MainPerlinStart + MainPerlin, 0);
                    MoodLight01.Variation = 1f * Mathf.PerlinNoise(MoodLight01PerlinStart + MoodLight01Perlin, 0);
                    MoodLight02.Variation = 1f * Mathf.PerlinNoise(MoodLight02PerlinStart + MoodLight02Perlin, 0);

                    // We include confinement here.

                    if (SETTINGS.UserInConfinedArea)
                    {

                        MainStageLight.Master = 2f;

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


                case "addkinectnull":



                    c = GameObject.Find("Kinect");
                    g = DebugObject.getNullObject(2, 2, 1);

                    g.transform.SetParent(c.transform, false);

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

        bool PlayVoiceOver(string clipName, StoryTask taskRef)
        {

            string vostatus;

            if (!taskRef.GetStringValue("vo", out vostatus))
            {
                taskRef.SetStringValue("vo", clipName);
                AudioClip clip = (AudioClip)Resources.Load("audio/" + clipName);

                VoiceOver.clip = clip;
                VoiceOver.Play();

                return false;

            }
            else
            {
                return true;

            }

        }

        void Update()
        {

        }

    }
}