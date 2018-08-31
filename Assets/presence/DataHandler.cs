using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.XR.WSA.Input;
using System.Linq;

using StoryEngine;
using System;
//using UnityEditorInternal;

namespace PresenceEngine
{

    public class DataHandler : MonoBehaviour
    {
        public Camera CaptureCamera;

        public GameObject presences;
        Presence fileplayback;
        float startListening = 0f;
        bool listening = false;
        public AudioSource presenceSound;

        GameObject captureTarget;

        public GameObject BufferStatusIn, BufferStatusOut;

        int interval;
        ushort[] depthMap;
        float timeStamp;
        public RawImage kinectImage, previewImage;
        public Text FPS;


        Texture2D DepthTexture, PreviewTexture;
        int frame = 0;

        GameObject go;
        int testSize = 10000;
        public DataController dataController;
        public GameObject headSet;

        string me = "Data handler: ";



        void Awake()
        {

            // Engine modules.

            Log.SetModuleLevel("AssitantDirector", LOGLEVEL.ERRORS);
            Log.SetModuleLevel("Director", LOGLEVEL.ERRORS);


            // Custom modules.


        }


        void Start()
        {

    
            dataController.addTaskHandler(TaskHandler);

            IO.localStorageFolder = Application.persistentDataPath + "/data";

            SETTINGS.Presences = new Dictionary<string, Presence>();

           


        }

        float recordStart;
        ushort[] rawDepthPrev, rawDepthCur;

        int minPrev, minCur, maxPrev, maxCur;

        void Update()
        {

            SetNetworkIndicators();


        }

        public bool TaskHandler(StoryTask task)
        {



            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;

            bool done = false;

            switch (task.description)
            {

                // -----------------------------------------------------------------------
                // Main roles.


                case "amserver":

                    SETTINGS.deviceMode = DEVICEMODE.SERVER;

                    done = true;

                    break;

                case "amvrclient":

                    SETTINGS.deviceMode = DEVICEMODE.VRCLIENT;



                    done = true;

                    break;




                // ----------------------------------------------------------------------------------------------------
                // Presence data manipulations

                case "createuser":

                    string username = "user";

                    if (!SETTINGS.Presences.TryGetValue(username, out SETTINGS.user))
                    {
                        SETTINGS.user = Presence.Create(presences, username);

                    }

                    SETTINGS.user.SetTranscoder("SkeletonAndDepth");
                    SETTINGS.user.SetDepthSampling(4);

                    done = true;

                    break;

                case "userstream":


                    // This task takes care of the actual current user only.

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {

                        Application.targetFrameRate = 30;
                        QualitySettings.vSyncCount = 0;

                        DepthTransport UserDT = SETTINGS.user.DepthTransport;

                        // Retrieve new depth info.
                        UserDT.GetNewFrame();

                        // Get latest value for headrotation (from client, via task) and add it to the frame (for recording).
                        task.GetQuaternionValue("user_headrotation", out UserDT.ActiveFrame.HeadOrientation);
                        task.GetVector3Value("user_headposition", out UserDT.ActiveFrame.HeadPosition);

                        // Encode depth to task.

                        if (!UserDT.Encode(task, "user"))
                            Log.Warning("Encode failed");

                        task.SetStringValue("debug", "time: " + UserDT.CurrentTime);

                        // user may move in or out of detection.

                        //  task.SetIntValue("user_debugcontrol", UserDT.IsUserDetected() ? 1:0);
                        //    SETTINGS.user.Visualiser.SetMode(task, "user");

                        //task.SetIntValue("user_cloudcontrol", GENERAL.UserMaterialised ? 1 : 0);


                    }

                    if (SETTINGS.deviceMode == DEVICEMODE.VRCLIENT)
                    {
                        Application.targetFrameRate = 30;
                        QualitySettings.vSyncCount = 0;

                        DepthTransport UserDT = SETTINGS.user.DepthTransport;

                        // Decode depth from task

                        if (!UserDT.Decode(task, "user"))
                            Log.Warning("Decode failed");

                        // put head orientation, include calibartion
                        UserDT.ActiveFrame.HeadOrientation = SETTINGS.HeadsetCorrection * headSet.transform.localRotation;
                        task.SetQuaternionValue("user_headrotation", UserDT.ActiveFrame.HeadOrientation);
                        UserDT.ActiveFrame.HeadPosition = headSet.transform.localPosition;
                        task.SetVector3Value("user_headposition", UserDT.ActiveFrame.HeadPosition);

                        //    int usercalibrated;

                        //  task.GetIntValue("usercalibrated", out usercalibrated);
                        //      SETTINGS.user.Visualiser.SetMode(task, "user");
                        //   GENERAL.UserCalibrated = (usercalibrated == 1);
                    }


                    break;


                case "materialiseon":
                case "MaterialiseOn":

                    // Use visualiser's to and from task method to change setting.

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        SETTINGS.user.SetVisualiser("PointShaded", 0);
                        task.SetIntValue("user_0_cloudvisible", 1);

                        SETTINGS.user.PullVisualiserSettingsFromTask(task, "user");

                        presenceSound.Play();

                        done = true;
                    }


                    break;

                case "materialiseoff":

                    // Use visualiser's to and from task method to change setting.

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        task.SetIntValue("user_0_cloudvisible", 0);

                        SETTINGS.user.PullVisualiserSettingsFromTask(task, "user");

                        done = true;
                    }

                    if (SETTINGS.deviceMode == DEVICEMODE.VRCLIENT)
                    {
                        int v;
                        if (task.GetIntValue("user_0_cloudvisible", out v))
                        {

                            SETTINGS.user.PullVisualiserSettingsFromTask(task, "user");

                        }


                        done = true;
                    }

                    break;

                case "handlepresences":

                    // Take care of progressing, over the network, of all other instances (not the user).

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {

                        // Get current list from the task.

                        string[] presenceNames;

                        if (!task.GetStringArrayValue("presences", out presenceNames))
                        {
                            // push list of all presences
                            task.SetStringArrayValue("presences", SETTINGS.Presences.Keys.ToArray());

                        }

                        // Now go over them and push all data.

                        foreach (KeyValuePair<string, Presence> presence in SETTINGS.Presences)
                        {

                            presence.Value.PushAllSettingToTask(task, presence.Key);

                            if (presence.Value.DepthTransport.Mode == DEPTHMODE.COPY)
                            {
                                // Copy frame reference.

                                presence.Value.DepthTransport.ActiveFrame = presence.Value.DepthTransport.TargetPresence.DepthTransport.ActiveFrame;

                            }

                            if (presence.Value.DepthTransport.Mode == DEPTHMODE.PLAYBACK)
                            {
                                // Play back while in playback mode and playback successful else fall through.

                                int status = presence.Value.DepthTransport.LoadFrameFromBuffer(presence.Value.DepthTransport.CurrentTime);

                                switch (status)
                                {
                                    case -1:
                                    case 0:

                                        // we're before the start or in the buffer

                                        if (!SETTINGS.ManualPlayback)
                                        {
                                            presence.Value.DepthTransport.CurrentTime += Time.deltaTime;
                                        }

                                        //Debug.Log("playing");
                                        break;


                                    case 1:
                                        // wé're at the end
                                        presence.Value.DepthTransport.CurrentTime = presence.Value.DepthTransport.TransCoder.GetBufferFile().StartTime;
                                        //Debug.Log("loop");
                                        break;

                                    default:
                                        // Error.
                                        Debug.LogError("Buffer playback error.");
                                        break;


                                }

                                // Push time.

                                task.SetFloatValue(presence.Key + "_time", presence.Value.DepthTransport.CurrentTime);

                                if (status == 0 && !presence.Value.SoundPlayed)
                                {
                                    presence.Value.SoundPlayed = true;
                                    presenceSound.Play();
                                }

                                // Debug.

                                if (presence.Key == "playbackpresence")
                                {

                                    task.SetStringValue("debug", "" + presence.Value.DepthTransport.CurrentTime);

                                }

                            }

                        }

                    }


                    if (SETTINGS.deviceMode == DEVICEMODE.VRCLIENT)
                    {

                        // Retrieve list.

                        string[] taskPresenceNames;

                        if (task.GetStringArrayValue("presences", out taskPresenceNames))
                        {

                            foreach (string presenceName in taskPresenceNames)
                            {

                                Presence presence;

                                if (!SETTINGS.Presences.TryGetValue(presenceName, out presence))
                                {

                                    // Create instance and retrieve/apply settings.

                                    presence = Presence.Create(presences, presenceName);

                                    //    SETTINGS.Presences.Add(presenceName, presence);


                                    //presence.Visualiser.SettingsFromTask(task, presenceName);
                                }

                                presence.PullAllSettingsFromTask(task, presenceName);

                                // Update depthmode 

                                //  presence.PullModeFromTask(task, presenceName);

                                if (presence.DepthTransport.Mode == DEPTHMODE.COPY)
                                {
                                    presence.DepthTransport.ActiveFrame = presence.DepthTransport.TargetPresence.DepthTransport.ActiveFrame;

                                }

                                float getTime;

                                // We're displaying the point in time as indicated by the server.

                                if (presence.DepthTransport.Mode == DEPTHMODE.PLAYBACK && task.GetFloatValue(presenceName + "_time", out getTime))

                                {
                                    presence.DepthTransport.CurrentTime = getTime;
                                    int status = presence.DepthTransport.LoadFrameFromBuffer(getTime);


                                    //if (status == 0 && !presence.SoundPlayed)
                                    //{
                                    //    presence.SoundPlayed = true;
                                    //    presenceSound.Play();
                                    //}

                                }

                            }

                        }

                        // Reversed: go over presences and destroy if they're no longer listed.

                        string[] localPresenceNames = SETTINGS.Presences.Keys.ToArray();

                        for (int i = localPresenceNames.Length - 1; i >= 0; i--)
                        {

                            if (Array.IndexOf(taskPresenceNames, localPresenceNames[i]) == -1)

                            {


                                // A presence exists locally that has no reference in the task (does not exist on server) so we kill it.

                                Debug.Log("removing " + localPresenceNames[i]);

                                Presence presence = SETTINGS.Presences[localPresenceNames[i]];

                                Destroy(presence.gameObject);

                                SETTINGS.Presences.Remove(localPresenceNames[i]);

                            }

                        }

                    }

                    break;

                case "deletepresences":

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {

                        foreach (KeyValuePair<string, Presence> presence in SETTINGS.Presences)
                        {
                            if (presence.Key != "user")
                            {

                                Destroy(presence.Value.gameObject);

                            }
                        }

                        SETTINGS.Presences.Clear();
                        SETTINGS.Presences.Add("user", SETTINGS.user);

                    }

                    done = true;
                    break;



                // ----------------------------------------------------------------------------------------------------
                // Pauses

                case "pause3":

                    float TimeOut;

                    if (!task.GetFloatValue("timeout", out TimeOut))
                    {
                        TimeOut = Time.time + 3;
                        task.SetFloatValue("timeout", TimeOut);

                    }

                    if (Time.time > TimeOut)
                        done = true;

                    break;

                case "pause5":

                    if (!task.GetFloatValue("timeout", out TimeOut))
                    {
                        TimeOut = Time.time + 5;
                        task.SetFloatValue("timeout", TimeOut);

                    }

                    if (Time.time > TimeOut)
                        done = true;

                    break;



                    // ----------------------------------------------------------------------------------------------------
                    // User manipulations

                case "grabframe":


                    Debug.Log("grabbing frame");
                    GrabFrame();


                    done = true;
                    break;

                case "nextframe":

                    SETTINGS.ManualPlayback = true;

                    Presence pbp;

                    if (SETTINGS.Presences.TryGetValue("playbackpresence", out pbp))
                    {
                        pbp.DepthTransport.CurrentTime += 1 / 60f;

                    }

                    done = true;
                    break;

                case "previousframe":

                    SETTINGS.ManualPlayback = true;

                    if (SETTINGS.Presences.TryGetValue("playbackpresence", out pbp))
                    {
                        pbp.DepthTransport.CurrentTime -= 1 / 60f;

                    }

                    done = true;
                    break;

                case "togglemanualplayback":

                    SETTINGS.ManualPlayback = !SETTINGS.ManualPlayback;

                    done = true;
                    break;


                    // ----------------------------------------------------------------------------------------------------
                    // Data capture and playback.

               
                case "playbackfile":

                    // Play back the checked out file.

                    Debug.Log("Starting name " + IO.CheckedOutFile);

                    FileformatBase pbBuffer = IO.LoadFile(IO.CheckedOutFile);

                    Debug.Log("Starting name " + IO.CheckedOutFile);

                    if (pbBuffer != null)
                    {
                        
                        if (pbBuffer.EndTime > pbBuffer.StartTime)
                        {

                            Debug.Log("Start time: " + pbBuffer.StartTime + " End time: " + pbBuffer.EndTime);

                            // Not a placeholder file so proceed.

                            string pn = "playbackpresence";

                            if (!SETTINGS.Presences.TryGetValue(pn, out fileplayback))
                            {

                                // No presence in scene so create it.
                                fileplayback = Presence.Create(presences, pn);

                            }

                            fileplayback.SetVisualiser(SETTINGS.DefaultVisualiser, 0);
                            fileplayback.SetVisualiser("ShowSkeleton", 1);

                            fileplayback.SetTranscoder(pbBuffer.TransCoderName);

                            fileplayback.DepthTransport.TransCoder.SetBufferFile(pbBuffer);
                            fileplayback.DepthTransport.CurrentTime = fileplayback.DepthTransport.TransCoder.GetBufferFile().StartTime;
                            fileplayback.DepthTransport.Mode = DEPTHMODE.PLAYBACK;

                            fileplayback.SetVisualiseTransform(Vector3.zero, Vector3.one, Quaternion.identity);

                            // Roundabout way of setting setting on visualiser. This task will be removed.

                            task.SetFloatValue("playbackpresence_0_cloudvisible", 1);
                            //     task.SetFloatValue("playbackpresence_1_cloudvisible", 0);
                            fileplayback.PullVisualiserSettingsFromTask(task, "playbackpresence");

                            Debug.Log("started buffer " + pbBuffer.Name);

                        }

                    }

                    done = true;

                    break;

                case "stopplaybackfile":

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {

                        if (fileplayback != null)
                            fileplayback.DepthTransport.Mode = DEPTHMODE.OFF;

                    }
                    done = true;
                    break;


                case "stopallplayback":

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {

                        foreach (KeyValuePair<string, Presence> presence in SETTINGS.Presences)
                        {
                            if (presence.Key != "user")
                            {

                                presence.Value.DepthTransport.Mode = DEPTHMODE.OFF;

                            }

                        }


                    }
                    done = true;
                    break;



                case "playmirror":

                    // Creates a mirror presence of the user if recording OR of the playback file if playing.

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {

                        Debug.Log("Starting mirror of " + IO.CheckedOutFile);

                        FileformatBase pbcBuf = IO.LoadFile(IO.CheckedOutFile);

                        if (pbcBuf != null)
                        {

                            string mirrorPresence = "mirrorpresence";

                            if (!SETTINGS.Presences.TryGetValue(mirrorPresence, out fileplayback))
                            {
                                fileplayback = Presence.Create(presences, mirrorPresence);
                                //      SETTINGS.Presences.Add(mirrorPresence, fileplayback);
                            }

                            fileplayback.SetVisualiser(SETTINGS.DefaultVisualiser);
                            fileplayback.SetTranscoder(pbcBuf.TransCoderName);
                            fileplayback.DepthTransport.TransCoder.SetBufferFile(pbcBuf);
                            fileplayback.DepthTransport.CurrentTime = 0;
                            fileplayback.SetVisualiseTransform(new Vector3(0, 0, 1), new Vector3(1, 1, -1), Quaternion.identity);
                            task.SetFloatValue(mirrorPresence + "_cloudvisible", 1);
                            fileplayback.PullVisualiserSettingsFromTask(task, mirrorPresence);
                            fileplayback.DepthTransport.Mode = DEPTHMODE.COPY;

                            if (SETTINGS.user.DepthTransport.Mode == DEPTHMODE.RECORD)
                            {
                                fileplayback.DepthTransport.Target = "user";
                            }
                            else
                            {
                                fileplayback.DepthTransport.Target = "playbackpresence";
                            }

                            Debug.Log("started buffer " + pbcBuf.Name);

                        }

                    }

                    done = true;

                    break;


                case "playecho":

                    // Play back the previous n files if available.

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        int checkedOut = IO.CheckedOutFileIndex();

                        for (int c = 1; c < 3; c++)
                        {


                            string name = IO.GetFilePath(checkedOut + c);

                            Debug.Log("starting name " + c + " " + name);

                            FileformatBase pbcBuf = IO.LoadFile(IO.GetFilePath(checkedOut + c));

                            if (pbcBuf != null)
                            {



                                if (pbcBuf.EndTime > pbcBuf.StartTime)
                                {
                                    // Not a placeholder file so proceed.

                                    string pbn = "playbackpresence" + c;
                                    if (!SETTINGS.Presences.TryGetValue(pbn, out fileplayback))
                                    {

                                        // No presence in scene so create it.
                                        fileplayback = Presence.Create(presences, pbn);
                                        //  SETTINGS.Presences.Add("playbackpresence" + c, fileplayback);

                                    }

                                    fileplayback.SetVisualiser(SETTINGS.DefaultVisualiser);
                                    fileplayback.SetTranscoder(pbcBuf.TransCoderName);

                                    fileplayback.DepthTransport.TransCoder.SetBufferFile(pbcBuf);

                                    fileplayback.DepthTransport.CurrentTime = fileplayback.DepthTransport.TransCoder.GetBufferFile().StartTime - UnityEngine.Random.Range(0.5f, 3f);

                                    fileplayback.SetVisualiseTransform(Vector3.zero, Vector3.one, Quaternion.identity);

                                    fileplayback.DepthTransport.Mode = DEPTHMODE.PLAYBACK;

                                    task.SetFloatValue("playbackpresence" + c + "_cloudvisible", 1);
                                    fileplayback.PullVisualiserSettingsFromTask(task, "playbackpresence" + c);

                                    Debug.Log("started buffer " + c + " " + pbcBuf.Name);
                                }







                            }





                        }
                    }

                    done = true;

                    break;

                case "playdelay":

                    // Plays back additional copies of the user or playback presence.

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        //int checkedOut = IO.CheckedOutFileIndex();

                        for (int c = 1; c < 3; c++)
                        {

                            //string name = IO.GetFilePath(checkedOut + c);

                            //Debug.Log("Generating name " + c);

                            FileformatBase pbcBuf = IO.LoadFile(IO.CheckedOutFile);

                            if (pbcBuf != null)
                            {

                                //if (pbcBuf.EndTime > pbcBuf.StartTime)
                                //{
                                // Not a placeholder file so proceed.
                                string dpn = "playbackpresence" + c;
                                if (!SETTINGS.Presences.TryGetValue(dpn, out fileplayback))
                                {

                                    fileplayback = Presence.Create(presences, dpn);
                                    //   SETTINGS.Presences.Add("playbackpresence" + c, fileplayback);

                                }

                                fileplayback.SetVisualiser(SETTINGS.DefaultVisualiser);
                                fileplayback.SetTranscoder(pbcBuf.TransCoderName);
                                fileplayback.DepthTransport.TransCoder.SetBufferFile(pbcBuf);

                                fileplayback.DepthTransport.CurrentTime = -0.5f * c;

                                fileplayback.SetVisualiseTransform(new Vector3(0.5f * c, 0, 0), Vector3.one, Quaternion.identity);

                                fileplayback.DepthTransport.Mode = DEPTHMODE.PLAYBACK;

                                task.SetFloatValue("playbackpresence" + c + "_cloudvisible", 1);
                                fileplayback.PullVisualiserSettingsFromTask(task, "playbackpresence" + c);

                                Debug.Log("started buffer " + c + " " + pbcBuf.Name);


                                //}



                            }





                        }
                    }

                    done = true;

                    break;

               

                case "waitforallplaybacktoend":

                    // 

                    bool AllOff = true;

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {

                        foreach (KeyValuePair<string, Presence> presence in SETTINGS.Presences)
                        {
                            if (presence.Key != "user" && presence.Value.DepthTransport.Mode != DEPTHMODE.OFF)
                            {

                                AllOff = false;

                            }
                        }

                    }

                    if (AllOff)
                        done = true;


                    break;


              

                case "recordprepare":


                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        if (SETTINGS.user.DepthTransport != null && IO.CheckedOutFile != "")
                        {
                            Debug.Log("preparing record for " + IO.CheckedOutFile);

                            SETTINGS.user.DepthTransport.TransCoder.CreateBufferFile(IO.CheckedOutFile);

                            task.SetStringValue("user_file", IO.CheckedOutFile);

                            done = true;

                        }
                        else
                        {
                            Debug.LogWarning("Cannot prepare record");
                            done = true;
                        }

                    }

                    if (SETTINGS.deviceMode == DEVICEMODE.VRCLIENT)
                    {
                        if (SETTINGS.user.DepthTransport != null)
                        {
                            // Wait for filename then fall through
                            string file;
                            if (task.GetStringValue("user_file", out file))
                            {
                                IO.SelectFile(file);
                                SETTINGS.user.DepthTransport.TransCoder.CreateBufferFile(IO.CheckedOutFile);

                                done = true;

                            }


                        }
                        else
                        {
                            done = true;
                        }
                    }

                    break;

                case "recordstart":


                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        if (SETTINGS.user.DepthTransport != null && IO.CheckedOutFile != "")
                        {
                            if (!task.GetFloatValue("timeout", out TimeOut))
                            {
                                TimeOut = Time.time + SETTINGS.SessionDuration;
                                task.SetFloatValue("timeout", TimeOut);
                                SETTINGS.user.DepthTransport.Mode = DEPTHMODE.RECORD;

                            }

                            if (Time.time > TimeOut)
                            {
                                done = true;
                            }

                        }
                        else
                        {
                            done = true;
                        }

                    }

                    if (SETTINGS.deviceMode == DEVICEMODE.VRCLIENT)
                    {
                        if (SETTINGS.user.DepthTransport != null)
                        {

                            SETTINGS.user.DepthTransport.Mode = DEPTHMODE.RECORD;

                            done = true;



                        }
                    }

                    break;

                case "recordstop":

                    // Same for server and client.

                    if (SETTINGS.user.DepthTransport.Mode == DEPTHMODE.RECORD)
                    {
                        SETTINGS.user.DepthTransport.Mode = DEPTHMODE.LIVE;

                        Debug.Log("Stopped recording. Logged frames " + SETTINGS.user.DepthTransport.TransCoder.GetBufferFile().Frames.Count);


                        FileformatBase BufferFile = SETTINGS.user.DepthTransport.TransCoder.GetBufferFile();
                        IO.SaveFileToSelected(BufferFile);

                    }


                    done = true;
                    break;



                    // ------------------------------------------------------------------------------------------------------------------------------
              

                    // Depth data manipulations.
          

                case "receivelivedepth":
                    // For client.

                    SETTINGS.user.DepthTransport.Mode = DEPTHMODE.LIVE;

                    done = true;

                    break;

                case "depthlive":

                    // For server.
                    SETTINGS.user.DepthTransport.Mode = DEPTHMODE.LIVE;
                    done = true;

                    break;

                case "depthoff":
                    
                    SETTINGS.user.DepthTransport.Mode = DEPTHMODE.OFF;

                    done = true;

                    break;

                case "startspatialdata":

                    // attempts to start live kinect.
                    // can be simplified because depthtransport returns a value regardless of kinect status

                    string kinectstatus;

                    if (!task.GetStringValue("kinectstatus", out kinectstatus))
                    {
                        kinectstatus = "void";

                    }


                    switch (kinectstatus)
                    {


                        case "void":

                            // first run.

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN


                            DepthTransport.OwnsKinect.Mode = DEPTHMODE.LIVE;

                            kinectstatus = "initialising";

#else

                            Debug.LogWarning(me + "Non windows platform: dummy kinect.");
                            kinectstatus = "dummy";


#endif

                            timeStamp = Time.time;

                            break;

                                                   

                        case "live":

                            done = true;
                            break;



                        case "dummy":

                            done = true;

                            break;



                        default:

                            break;



                    }


                    task.SetStringValue("kinectstatus", kinectstatus);


                    break;



                    // ----------------------------------------------------------------------------------------------------
                // NETWORKING

                case "networkguion":

                    dataController.displayNetworkGUI(true);

                    done = true;

                    break;


                case "startdiscover":

                    dataController.startBroadcastClient();

                    startListening = Time.time;
                    listening = true;

                    done = true;

                    break;

                case "listenforserver":

                    if (listening)
                    {

                        if (dataController.foundServer())
                        {

                            Debug.Log(me + "Found broadcast server");

                            // Store network server address.

                            GENERAL.networkServer = GENERAL.broadcastServer;

                            dataController.stopBroadcast();

                            task.setCallBack("foundserver");

                            listening = false;

                            done = true;

                        }

                        /*
                        // Optional time out.

                        if (Time.time - startListening > 10f) {

                            dataController.networkBroadcastStop ();

                            done = true;

                        }
                        */

                    }

                    break;

                case "listenforclients":


                    // Server side.

                    int ConnectedClients = dataController.serverConnections();

                    //  led.SetActive(ConnectedClients > 0);
                    GENERAL.wasConnected = (ConnectedClients > 0);


                    task.SetStringValue("debug", "clients: " + ConnectedClients);

                    int newClient = GENERAL.GETNEWCONNECTION();

                    if (newClient != -1)
                    {

                        Debug.Log(me + "New client on connection " + newClient);

                        task.setCallBack("newclient");

                    }

                    break;

                case "monitorconnection":

                    if (!dataController.clientIsConnected())
                    {

                        if (GENERAL.wasConnected)
                        {

                            GENERAL.wasConnected = false;
                            task.SetStringValue("debug", "lost connection");

                            task.setCallBack("serverlost");


                            done = true; // we'll fall throught to the next line in the script, since there is no longer a connection to monitor.

                        }
                        else
                        {

                            task.SetStringValue("debug", "no connection yet");

                        }

                    }
                    else
                    {

                        GENERAL.wasConnected = true;

                        task.SetStringValue("debug", "connected");

                    }

                    //    led.SetActive(GENERAL.wasConnected);

                    break;

                case "startclient":

                    Debug.Log(me + "Starting network client.");

                    dataController.startNetworkClient(GENERAL.networkServer);

                    done = true;

                    break;

                case "startserver":

                    Debug.Log(me + "Starting network server");

                    dataController.startBroadcastServer();
                    dataController.startNetworkServer();

                    done = true;

                    break;


                case "isglobal":

                    if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL)
                    {

                        task.pointer.scope = SCOPE.GLOBAL;

                    }

                    done = true;

                    break;


                case "passglobal":

                    // go over all pointers 

                    foreach (StoryTask theTask in GENERAL.ALLTASKS)
                    {

                        if (theTask.scope == SCOPE.GLOBAL)
                        {

                            theTask.MarkAllAsModified();

                        }

                    }

                    done = true;

                    break;

                

                default:

                    // Default is caught in 
                    done = true;

                    break;

            }

            return done;

        }


        int GrabFileIndex = 0;

        void GrabFrame()
        {
            RenderTexture rt = new RenderTexture(4096, 4096 / 16 * 9, 24, RenderTextureFormat.ARGB32);

            CaptureCamera.targetTexture = rt;
            CaptureCamera.Render();

            // Set the supplied RenderTexture as the active one
            RenderTexture.active = rt;

            // Create a new Texture2D and read the RenderTexture image into it
            Texture2D tex = new Texture2D(rt.width, rt.height);
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

            byte[] bytes;
            bytes = tex.EncodeToPNG();

            System.IO.File.WriteAllBytes(Application.dataPath + "/grab" + GrabFileIndex + ".png", bytes);

            GrabFileIndex++;

            CaptureCamera.targetTexture = null;

        }


        void SetNetworkIndicators()
        {

            FPS.text = "" + (Mathf.Round(1f / Time.deltaTime));

            BufferStatusIn.SetActive(GENERAL.wasConnected);
            BufferStatusOut.SetActive(GENERAL.wasConnected);

            switch (AssitantDirector.BufferStatusIn)
            {
                case 0:
                    BufferStatusIn.GetComponent<Image>().color = Color.grey;
                    break;
                case 1:
                    BufferStatusIn.GetComponent<Image>().color = Color.green;

                    break;

                default:
                    BufferStatusIn.GetComponent<Image>().color = Color.blue;
                    break;



            }
            switch (AssitantDirector.BufferStatusOut)
            {
                case 0:
                    BufferStatusOut.GetComponent<Image>().color = Color.grey;
                    break;
                case 1:
                    BufferStatusOut.GetComponent<Image>().color = Color.green;

                    break;

                default:
                    BufferStatusOut.GetComponent<Image>().color = Color.blue;
                    break;



            }

        }



    }
}