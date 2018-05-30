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
        public GameObject PresenceObjectPrefab,presences;
        public SetController setController;


        //public GameObject cloud;

        public GameObject kinectManagerObject;

        ParticleCloud[] clouds;

        //	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        //KinectManager kinectManager;

        //	#endif

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

        //        Dictionary<string, GameObject> PresenceInstances;

        //    Presence Presences[0];

        void Start()
        {

            setController.addTaskHandler(TaskHandler);

            //SETTINGS.Presences = new Presence[2];


            //	ParticleCloud.init (GameObject.Find ("Cloud"));



            //	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            //	kinectManager = kinectManagerObject.GetComponent<KinectManager> ();

            //#endif
        }
        //	ParticleSystem.Particle[] allParticles;
        int dataSize;
        ushort[] newFrame;
        int sample;
        //	int dataSize;
        Vector3 point;

        int particleIndex;
        int count;
        ParticleCloud cloud, mirror;
        int frame = -1;

        //float MainStageLightPerlin;




        public bool TaskHandler(StoryTask task)
        {

            bool done = false;

            switch (task.description)
            {



                //case "playbackfile":

                    //// Play back the checked out file.

                    //Presence filePlayback;

                    //if (!SETTINGS.Presences.TryGetValue("playbackpresence", out filePlayback))
                    //{

                    //    // if intended presence doesn't exist (and it shouldn't), create it.

                    //    filePlayback = Presence.Create(this.gameObject);
                    //    SETTINGS.Presences.Add("playbackpresence", filePlayback);
                    //    filePlayback.SetVisualiser("ShowSkeleton");
                    //    filePlayback.SetTranscoder("SkeletonOnly");

                    //}

                    //done = true;
                    //break;




              

          
                //case "createclone":

                //int pi = 1;

                //if (SETTINGS.Presences[pi] == null)
                //    SETTINGS.Presences[pi] = Presence.Create(PresenceObjectPrefab, this.gameObject);

                //SETTINGS.Presences[pi].SetVisualiser("ShowSkeleton");
                //SETTINGS.Presences[pi].SetTranscoder("SkeletonOnly");
                //SETTINGS.Presences[pi].Initialise();

                //done = true;
                //break;


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

                    // We include confinement here


                    if (GENERAL.UserInConfinedArea)
                    {


                        MainStageLight.Master = GENERAL.UserCalibrated ? 2f : 0.5f;


                    }
                    else
                    {

                        MainStageLight.Master = 0.1f;

                    }


                    break;

                case "lostconnection":

                    GENERAL.UserCalibrated = false;

                    done = true;
                    break;

                case "moodon":

                    MoodParticles.SetActive(true);
                    //MainStageLight.Master = 3;

                    // blocking

                    //Debug.Log("moodon executed at "+Time.frameCount);

                    //string test;
                    //if (!task.getStringValue("test",out test))
                    //task.setStringValue("test","test");



                    done = true;
                    break;

                case "moodoff":

                    MoodParticles.SetActive(false);

                    //MainStageLight.Master = 3;

                    // blocking

                    //Debug.Log("moodon executed at "+Time.frameCount);

                    //string test;
                    //if (!task.getStringValue("test",out test))
                    //task.setStringValue("test","test");



                    done = true;
                    break;

                case "recorddepth":
                case "playdepth":

                    //    ComputeParticles(PRESENCE.MainDepthTransport.GetRawDepthMap(), PRESENCE.MainDepthTransport.DepthWidth, PRESENCE.MainDepthTransport.DepthHeight, 2, new Vector3(0, PRESENCE.kinectHeight, 0), clouds[0]);

                    break;

                case "depthcompress":

                    ushort[] depth;
                    if (task.GetUshortValue("depth", out depth))
                    {
                        //             ComputeParticles(depth, PRESENCE.MainDepthTransport.DepthWidth, PRESENCE.MainDepthTransport.DepthHeight, 2, new Vector3(0, PRESENCE.kinectHeight, 0), clouds[0]);
                    }
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



                /*
            case "createclouds":

                clouds = new ParticleCloud[2];

                clouds[0] = new ParticleCloud(20000);
                clouds[1] = new ParticleCloud(20000);

                SETTINGS.PointCloud = new Vector3[2500];

                cloud = clouds[0];
                mirror = clouds[1];


                done = true;

                break;

            case "resetclouds":

                foreach (ParticleCloud pc in clouds)
                {
                    pc.ApplyParticles(0);

                }

                done = true;

                break;

            case "createsinglecloud":

                clouds = new ParticleCloud[1];

                clouds[0] = new ParticleCloud(20000);

                SETTINGS.PointCloud = new Vector3[2500];


                done = true;

                break;
*/
                /*
            case "playsequence":

                if (Time.time - SETTINGS.TimeStamp > 0.04f)
                {

                    SETTINGS.TimeStamp = Time.time;

                    CloudFrame currentFrame = SETTINGS.capture.Frames[SETTINGS.CaptureFrame];

                    int pointCount = currentFrame.Points.Length;


                    for (int p = 0; p < pointCount; p++)
                    {

                        clouds[0].allParticles[p].position = currentFrame.Points[p];

                    }


                    clouds[0].ApplyParticles(pointCount);

                    SETTINGS.CaptureFrame++;

                    if (SETTINGS.CaptureFrame == SETTINGS.capture.Frames.Length)
                    {

                        SETTINGS.CaptureFrame = 0;
                    }

                    task.SetStringValue("debug", "" + SETTINGS.CaptureFrame);

                }

                break;

            case "capture":

                if (Time.time - SETTINGS.TimeStamp > 0.04f)
                {

                    SETTINGS.TimeStamp = Time.time;

                    CloudFrame newFrame = new CloudFrame(SETTINGS.FrameSize);

                    Array.Copy(SETTINGS.PointCloud, newFrame.Points, SETTINGS.FrameSize);

                    SETTINGS.capture.Frames[SETTINGS.CaptureFrame] = newFrame;

                    SETTINGS.CaptureFrame++;

                    //	Debug.Log (PRESENCE.CaptureFrame + " " + PRESENCE.capture.Frames.Length);

                    if (SETTINGS.CaptureFrame == SETTINGS.capture.Frames.Length)
                    {

                        done = true;

                    }

                    task.SetStringValue("debug", "" + SETTINGS.CaptureFrame);


                }


                break;
*/
                case "cloudstream2":


                    // Get the raw depth and plot it straight into a particle cloud.

                    //if (DepthTransport.IsLive())
                    //{

                    //         ComputeParticles(PRESENCE.MainDepthTransport.GetRawDepthMap(), PRESENCE.MainDepthTransport.DepthWidth, PRESENCE.MainDepthTransport.DepthHeight, 2, new Vector3(0, PRESENCE.kinectHeight, 0), clouds[0]);

                    //     Debug.Log("type " + PreviewImage.texture.GetType().ToString());

                    //      var texture = PreviewImage.texture;





                    if (PreviewImage.texture != null)
                    {



                        //StreamSDK.UpdateStreamRemote(video);


                        //  ComputeParticles(DepthTransport.TextureToRawDepth((Texture2D)PreviewImage.texture),DepthTransport.Width, DepthTransport.Height, 2, new Vector3(0, PRESENCE.kinectHeight, 0), clouds[1]);


                    }




                    //} else
                    //{

                    //    Debug.LogError("depth transport not live");
                    //}


                    // Develop streaming. Encode and decode locally.

                    if (StreamSDK.instance != null)
                    {
                        int getFrame;
                        byte[] getVideo;
                        int min, max;

                        if (task.GetIntValue("frame", out getFrame) && frame != getFrame && task.GetByteValue("video", out getVideo) && task.GetIntValue("min", out min) && task.GetIntValue("max", out max))
                        {

                            frame = getFrame;

                            StreamSDK.UpdateStreamRemote(getVideo);

                            //   DepthTransport.DepthMin = min;
                            //    DepthTransport.DepthMax = max;


                            int DepthWidth = StreamSDK.instance.width;
                            int DepthHeight = StreamSDK.instance.height;


                            //                ComputeParticles(PRESENCE.MainDepthTransport.TextureToRawDepth((Texture2D)PreviewImage.texture, min, max), DepthWidth, DepthHeight, 1, new Vector3(0, PRESENCE.kinectHeight, 0), clouds[1]);


                        }




                    }



                    break;

                //case "userstream":

                //    UncompressedFrame ShowFrame = SETTINGS.Presences[0].DepthTransport.ActiveUncompressedFrame;

                //    if (ShowFrame != null && ShowFrame.Joints != null)
                //    {
                //        viewerObject.transform.parent.transform.position = ShowFrame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head];

                //        //  handl.transform.position = ShowFrame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft];
                //        //    handr.transform.position = ShowFrame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight];

                //        //     body.transform.position = ShowFrame.Body;
                //    }
                //    else
                //    {
                //        Debug.LogWarning("Trying to display an empty frame.");
                //    }






                //    break;

                /*
            case "cloudstream":



                string mode = "live";

                GENERAL.GLOBALS.getStringValue("mode", out mode);





                // SERVER SIDE


                if (PRESENCE.deviceMode == DEVICEMODE.SERVER)
                {

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            string init;

            if (!task.getStringValue ("init", out init)) {
                task.setStringValue ("init", "done");


                serverFrameStamp = Time.time - targetFrameRate;
                clientFrameStamp = Time.time - targetFrameRate;

                sample = 8;

                width = DepthTransport.OwnsKinect.DepthWidth;
                height = DepthTransport.OwnsKinect.DepthHeight;
                dataSize = (height / sample) * (width / sample);



            }

            if (!task.getFloatValue ("clientFrameRate", out clientFrameRate))
                clientFrameRate	= targetFrameRate;

            serverFrameRate = Mathf.Lerp (serverFrameRate, Time.time - serverFrameStamp, 0.1f);

            serverFrameStamp = Time.time;

            depthMap = DepthTransport.OwnsKinect.GetRawDepthMap ();

            particleIndex = 0;

            newFrame = new ushort [dataSize];

            int dataIndex = 0;

            for (int y = 0; y < height; y += sample) {

                for (int x = 0; x < width; x += sample) {

                    int i = y * width + x;

                    newFrame [dataIndex] = (ushort)(depthMap [i]);

                    ushort userMap = (ushort)(depthMap [i] & 7);
                    ushort userDepth = (ushort)(depthMap [i] >> 3);

                    if (userMap != 0) {

                        point = depthToWorld (x, y, userDepth);
                        point.x = -point.x;

                        point.y = -point.y;
                        point.z += 0.05f;

                        point.y += PRESENCE.kinectHeight;

                        cloud.allParticles [particleIndex].position = point;
                        PRESENCE.PointCloud [particleIndex] = point;

                        if (mode == "mirror") {

                            point.z *= -1;
                            point.z += 2;

                            mirror.allParticles [particleIndex].position = point;
                        }

                        particleIndex++;

                    }

                    dataIndex++;

                }

            }

            PRESENCE.FrameSize = particleIndex;

            switch (mode) {


            case "serveronly":
            case "preview":
                cloud.ApplyParticles (particleIndex);
                mirror.ApplyParticles (0);

                break;

            case "live":
                cloud.ApplyParticles (particleIndex);
                mirror.ApplyParticles (0);
                PRESENCE.CaptureFrame++;

                break;

            case "mirror":
                cloud.ApplyParticles (particleIndex);

                mirror.ApplyParticles (particleIndex);
                PRESENCE.CaptureFrame++;

                break;


            case "echo":
                cloud.ApplyParticles (particleIndex);

                if (PRESENCE.CaptureFrame < PRESENCE.capture.Frames.Length) {

                    CloudFrame newFrame = new CloudFrame (PRESENCE.FrameSize);

                    Array.Copy (PRESENCE.PointCloud, newFrame.Points, PRESENCE.FrameSize);

                    PRESENCE.capture.Frames [PRESENCE.CaptureFrame] = newFrame;



                    //	Debug.Log (PRESENCE.CaptureFrame + " " + PRESENCE.capture.Frames.Length);



                }
                if (PRESENCE.CaptureFrame > PRESENCE.echoOffset && PRESENCE.CaptureFrame < PRESENCE.capture.Frames.Length + PRESENCE.echoOffset) {


                    CloudFrame currentFrame = PRESENCE.capture.Frames [PRESENCE.CaptureFrame - PRESENCE.echoOffset];

                    if (currentFrame != null) {


                        int pointCount = currentFrame.Points.Length;


                        for (int p = 0; p < pointCount; p++) {

                            mirror.allParticles [p].position = currentFrame.Points [p];

                        }


                        mirror.ApplyParticles (pointCount);
                    }


                }

                PRESENCE.CaptureFrame++;





                break;
            default:
                break;


            }







            //task.setStringValue ("debug",""+particleIndex );

            if (Time.time - clientFrameStamp >= targetFrameRate) {

                clientFrameStamp = Time.time;

                task.setUshortValue ("frameData", newFrame);
                task.setIntValue ("frame", PRESENCE.frame);

                task.setIntValue ("frameSampleSize", sample);
                task.setIntValue ("frameWidth", width);
                task.setIntValue ("frameHeight", height);

                PRESENCE.frame++;

            }

            // only send at the interval the client can more or less handle

            //	if (clientFrameRate>targetFrameRate)





            int droppedFrames = 0;

            task.getIntValue ("dropped", out droppedFrames);

            if (droppedFrames == 0) {

                targetFrameRate = Mathf.Lerp (targetFrameRate, safeFrameRate, 0.01f);

            } else {

                safeFrameRate += 0.01f;

                targetFrameRate += 0.01f;

            }


            task.setStringValue ("debug", "s: " + Mathf.Round (100f * serverFrameRate) + " c: " + Mathf.Round (100f * clientFrameRate) + " t: " + Mathf.Round (100f * targetFrameRate) + " sf: " + Mathf.Round (100f *	safeFrameRate) + " d: " + droppedFrames);






















#endif

                }




                // CLIENT SIDE


                if (PRESENCE.deviceMode == DEVICEMODE.VRCLIENT)
                {



                    int getFrame;

                    if (task.getIntValue("frame", out getFrame))
                    {


                        task.getFloatValue("targetFrameRate", out targetFrameRate);


                        if (getFrame > PRESENCE.frame)
                        {

                            // newer frame available

                            if (clientFrameStamp == 0)
                            {

                                clientFrameStamp = Time.time - targetFrameRate; // we start perfectly on rate.

                            }
                            //	clientFrameRate = Time.time - clientFrameStamp;
                            clientFrameRate = Mathf.Lerp(clientFrameRate, Time.time - clientFrameStamp, 0.1f);

                            clientFrameStamp = Time.time;

                            //						frameRateDeviation = Time.time - clientFrameStamp;


                            //						if (interval==-1){
                            //
                            //							clientFrameStamp = Time.time;
                            //							interval++;
                            //						}

                            //						clientFrameDuration = Time.time - clientFrameStamp;

                            cloud.setLifeTime(clientFrameRate + 0.015f);


                            task.getUshortValue("frameData", out newFrame);
                            task.getIntValue("frameSampleSize", out sample);
                            task.getIntValue("frameWidth", out width);
                            task.getIntValue("frameHeight", out height);

                            //						ParticleCloud.setLifeTime (0.1f);

                            int mx = width / sample;
                            int my = height / sample;
                            int i;

                            particleIndex = 0;

                            for (int y = 0; y < my; y++)
                            {

                                for (int x = 0; x < mx; x++)
                                {

                                    i = y * mx + x;

                                    //	newFrame [framei] = (ushort)(depthMap [i]);

                                    ushort userMap = (ushort)(newFrame[i] & 7);
                                    ushort userDepth = (ushort)(newFrame[i] >> 3);

                                    if (userMap != 0)
                                    {

                                        point = depthToWorld(x * sample, y * sample, userDepth);
                                        point.x = -point.x;
                                        point.y = -point.y;
                                        point.y += PRESENCE.kinectHeight;
                                        point.z += 0.05f;



                                        //		cloud.Emit (point);



                                        cloud.allParticles[particleIndex].position = point;
                                        PRESENCE.PointCloud[particleIndex] = point;

                                        if (mode == "mirror")
                                        {

                                            point.z *= -1;
                                            point.z += 2;

                                            mirror.allParticles[particleIndex].position = point;
                                        }





                                        particleIndex++;


                                    }

                                }

                            } // end of plotting loop



                            PRESENCE.FrameSize = particleIndex;

                            switch (mode)
                            {

                                case "serveronly":
                                    cloud.ApplyParticles(0);
                                    mirror.ApplyParticles(0);

                                    break;

                                case "preview":

                                    cloud.ApplyParticles(particleIndex);
                                    mirror.ApplyParticles(0);

                                    break;

                                case "live":

                                    cloud.ApplyParticles(particleIndex);
                                    mirror.ApplyParticles(0);
                                    PRESENCE.CaptureFrame++;
                                    break;

                                case "mirror":
                                    cloud.ApplyParticles(particleIndex);

                                    mirror.ApplyParticles(particleIndex);

                                    PRESENCE.CaptureFrame++;
                                    break;


                                case "echo":

                                    cloud.ApplyParticles(particleIndex);

                                    if (PRESENCE.capture != null)
                                    {

                                        if (PRESENCE.CaptureFrame < PRESENCE.capture.Frames.Length)
                                        {

                                            CloudFrame newFrame = new CloudFrame(PRESENCE.FrameSize);

                                            Array.Copy(PRESENCE.PointCloud, newFrame.Points, PRESENCE.FrameSize);

                                            PRESENCE.capture.Frames[PRESENCE.CaptureFrame] = newFrame;



                                            //	Debug.Log (PRESENCE.CaptureFrame + " " + PRESENCE.capture.Frames.Length);



                                        }
                                        if (PRESENCE.CaptureFrame > PRESENCE.echoOffset && PRESENCE.CaptureFrame < PRESENCE.capture.Frames.Length + PRESENCE.echoOffset)
                                        {

                                            CloudFrame currentFrame = PRESENCE.capture.Frames[PRESENCE.CaptureFrame - PRESENCE.echoOffset];

                                            int pointCount = currentFrame.Points.Length;


                                            for (int p = 0; p < pointCount; p++)
                                            {

                                                mirror.allParticles[p].position = currentFrame.Points[p];

                                            }


                                            mirror.ApplyParticles(pointCount);



                                        }
                                    }


                                    PRESENCE.CaptureFrame++;


                                    break;
                                default:
                                    break;


                            }







                            droppedFrames = getFrame - PRESENCE.frame - 1;

                            PRESENCE.frame = getFrame;

                            task.setIntValue("dropped", droppedFrames);

                            task.setFloatValue("clientFrameRate", clientFrameRate);

                            //			task.setStringValue( "debug","f: "+PRESENCE.frame+" p: "+	particleIndex);


                        }

                    }

                }



                break;

            */

                case "addkinectnull":

                    //	c = GameObject.Find ("Compass");
                    //	g = DebugObject.getNullObject (0.5f, 0.5f, 2.5f);
                    //	g.transform.SetParent (c.transform, false);

                    c = GameObject.Find("Kinect");
                    g = DebugObject.getNullObject(0.25f, 0.25f, 0.5f);
                    g.transform.SetParent(c.transform, false);

                    done = true;

                    break;

                //case "addviewernulls":

                //    c = GameObject.Find("viewerCamera");
                //    g = DebugObject.getNullObject(0.25f, 0.25f, 0.5f);
                //    g.transform.SetParent(c.transform, false);

                //    c = GameObject.Find("HandLeft");
                //    g = DebugObject.getNullObject(0.25f);
                //    g.transform.SetParent(c.transform, false);

                //    c = GameObject.Find("HandRight");
                //    g = DebugObject.getNullObject(0.25f);
                //    g.transform.SetParent(c.transform, false);

                //    c = GameObject.Find("Body");
                //    g = DebugObject.getNullObject(0.5f);
                //    g.transform.SetParent(c.transform, false);


                //    done = true;

                //    break;

                //case "addhandnulls":

                //    c = GameObject.Find("HandLeft");
                //    g = DebugObject.getNullObject(0.1f);
                //    g.transform.SetParent(c.transform, false);

                //    c = GameObject.Find("HandRight");
                //    g = DebugObject.getNullObject(0.1f);
                //    g.transform.SetParent(c.transform, false);

                //    done = true;

                //    break;



                case "alignset":

                    // Position set and user relative to kinect.

                    if (SETTINGS.kinectIsOrigin)
                    {

                        GameObject set = GameObject.Find("SetHandler");
                        GameObject viewer = GameObject.Find("viewerInterest");
                        GameObject kinectObject = GameObject.Find("Kinect");

                        //	c = GameObject.Find ("Compass");

                        SETTINGS.north = -SETTINGS.kinectHeading;

                        //	c.transform.rotation = Quaternion.Euler (0, PRESENCE.north, 0);

                        set.transform.rotation = Quaternion.Euler(0, SETTINGS.north, 0);

                        p = SETTINGS.kinectCentreDistance * Vector3.forward;

                        set.transform.position = p;
                        //	c.transform.position = p;

                        p.y = viewer.transform.position.y;

                        viewer.transform.position = p;

                        p = Vector3.zero;
                        p.y = SETTINGS.kinectHeight;

                        kinectObject.transform.position = p;

                    }
                    else
                    {

                        Debug.LogWarning("Kinect not at origin, not aligning.");

                    }

                    //DepthTransport.centered = true;

                    done = true;

                    break;


                /*
            case "placekinect":


                GameObject kinect = GameObject.Find("Kinect");

                //	Vector3 p;
                //	Quaternion q;
                q = Quaternion.Euler(0, PRESENCE.kinectHeading, 0);


                kinect.transform.localRotation = q;

                p = (-1f * PRESENCE.kinectCentreDistance) * (q * Vector3.forward);

                p.y = PRESENCE.kinectHeight;

                kinect.transform.position = p;


                DepthTransport.kinectPosition = p;

                DepthTransport.kinectRotation = q;

                DepthTransport.centered = false;

                done = true;

                break;
                */
                /*
            case "nextdepth":

                IO.depthIndex++;

                if (IO.depthIndex == IO.savedDepthCaptures.Count)
                {
                    IO.depthIndex = 0;
                }

                Debug.Log(me + "next depth");

                done = true;

                break;

            case "showdepthdata":

                cloud = clouds[0];

                if (IO.savedDepthCaptures.Count > 0)
                {

                    interval2++;

                    if (interval2 == 100)
                    {
                        interval2 = 0;

                        IO.depthIndex++;

                        if (IO.depthIndex == IO.savedDepthCaptures.Count)
                        {
                            IO.depthIndex = 0;
                        }



                    }



                }

                if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL)
                {



                    int ind;

                    if (task.GetIntValue("index", out ind))
                    {

                        if (ind != IO.depthIndex)
                        {

                            // changed

                            task.SetIntValue("index", IO.depthIndex);
                            task.SetStringValue("debug", "" + IO.depthIndex);


                        }

                    }
                    else
                    {

                        task.SetIntValue("index", IO.depthIndex);
                        task.SetStringValue("debug", "" + IO.depthIndex);

                    }





                }
                else
                {

                    int ind;
                    if (task.GetIntValue("index", out ind))
                        IO.depthIndex = ind;

                }


                if (IO.depthIndex >= 0)
                {

                    //				int index = 0;
                    //
                    //				if (!task.getIntValue ("index", out index)) {
                    //
                    //					task.setIntValue ("index", 0);
                    //				}

                    //				int index = IO.depthIndex;



                    //				string index;
                    //
                    //				if (task.getStringValue("debug" , out index)){
                    //
                    //					if (int.Parse(index) != IO.depthIndex) {
                    //						
                    //						task.setStringValue("debug",""+IO.depthIndex);
                    //
                    //					}
                    //
                    //
                    //				}


                    //				int ind;
                    //
                    //				if (task.getIntValue ("index", out ind)) {
                    //
                    //
                    //
                    //
                    //
                    //				} else {
                    //
                    //					task.setIntValue ("index", IO.depthIndex);
                    //				}


                    DepthCapture.current = IO.savedDepthCaptures[IO.depthIndex];

                    //				ParticleCloud.setLifeTime (0.1f);

                    if (interval == 4 && DepthCapture.current != null)
                    {

                        interval = 0;

                        depthMap = DepthCapture.current.GetRawDepthMap();

                        width = DepthCapture.current.getUserDepthWidth();
                        height = DepthCapture.current.getUserDepthHeight();

                        sample = 8;

                        //	Vector3 point;

                        for (int y = 0; y < height; y += sample)
                        {

                            for (int x = 0; x < width; x += sample)
                            {

                                int i = y * width + x;

                                ushort userMap = (ushort)(depthMap[i] & 7);
                                ushort userDepth = (ushort)(depthMap[i] >> 3);

                                if (userMap != 0)
                                {
                                    point = depthToWorld(x, y, userDepth);
                                    point.y = -point.y;
                                    point.y += 1.5f;

                                    cloud.Emit(point);
                                }

                            }

                        }

                    }

                    interval++;

                }
                else
                {


                    done = true;

                }








                break;


*/


                case "pointcloud":
                    cloud = clouds[0];
                    cloud.setLifeTime(0.5f);

                    cloud.update();



                    break;




                default:

                    done = true;

                    break;

            }

            return done;

        }

        // default callibration values. c being a correction presumably and f the fov


        double fx_d = 1.0d / 5.9421434211923247e+02;
        double fy_d = 1.0d / 5.9104053696870778e+02;
        double cx_d = 3.3930780975300314e+02;
        double cy_d = 2.4273913761751615e+02;


        Vector3 depthToWorld(int x, int y, int depthValue)
        {

            Vector3 result = Vector3.zero;

            //double depth = depthLookUp [depthValue];
            //float depth = rawDepthToMeters(depthValue);

            float depth = depthValue / 1000f;

            result.x = (float)((x - cx_d) * depth * fx_d);
            result.y = (float)((y - cy_d) * depth * fy_d);
            result.z = (float)(depth);

            return result;

        }


        public void ComputeParticles(ushort[] DepthMap, int Width, int Height, int Sample, Vector3 Transform, ParticleCloud Cloud)
        {

            // takes a kinect styled uint[] RawDepthMap, Width, Height.
            // and plots the points (at sample intervals) into a Particle Cloud with a given Transform.

            int ParticleIndex = 0;
            int Scale = 640 / Width;

            for (int y = 0; y < Height; y += Sample)
            {

                for (int x = 0; x < Width; x += Sample)
                {

                    int i = y * Width + x;

                    ushort userMap = (ushort)(DepthMap[i] & 7);
                    ushort userDepth = (ushort)(DepthMap[i] >> 3);

                    if (userMap != 0)
                    {

                        point = depthToWorld(x * Scale, y * Scale, userDepth);
                        point.x = -point.x;
                        point.y = -point.y;
                        point += Transform;

                        Cloud.allParticles[ParticleIndex].position = point;

                        ParticleIndex++;
                        if (ParticleIndex == Cloud.allParticles.Length)
                        {
                            // abort
                            x = Width;
                            y = Height;

                        }

                        // ParticleIndex = ParticleIndex % Cloud.allParticles.Length;

                    }

                }

            }

            Cloud.ApplyParticles(ParticleIndex);

        }



        void Update()
        {

        }

    }
}