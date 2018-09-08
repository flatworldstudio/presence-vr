using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.XR.WSA.Input;
using System.Linq;

using StoryEngine;
using System;
//using UnityEditorInternal;

using Logger = StoryEngine.Logger;

namespace PresenceEngine
{
    public delegate void TimeHandler(StoryTask task, string name, Presence presence, float time);



    public class DataHandler : MonoBehaviour
    {
        //  public IO IO;
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

        string PresenceName;
        FileformatBase FileBuffer;

        string ID = "Data handler";

        public TimeHandler TimeHandler;

        //string SelectedFile;
        //string SelectedFolder;

        //  string dpn;

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.

        void Log(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.NORMAL);
        }
        void Warning(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.WARNINGS);
        }
        void Error(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.ERRORS);
        }
        void Verbose(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.VERBOSE);
        }

        void Awake()
        {

            // Engine modules.

            Logger.SetLogLevel("AD", LOGLEVEL.WARNINGS);
            Logger.SetLogLevel("Director", LOGLEVEL.WARNINGS);
            Logger.SetLogLevel("DataController", LOGLEVEL.WARNINGS);
            Logger.SetLogLevel("DeusController", LOGLEVEL.WARNINGS);
            Logger.SetLogLevel("UserController", LOGLEVEL.WARNINGS);
            Logger.SetLogLevel("SetController", LOGLEVEL.WARNINGS);

            // Custom modules.

            Logger.SetLogLevel("IO", LOGLEVEL.NORMAL);
            Logger.SetLogLevel("Data handler", LOGLEVEL.VERBOSE);

        }


        void Start()
        {
            dataController.addTaskHandler(TaskHandler);

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
                 
                 //       IO.Instance.SaveFile(BufferFile, SETTINGS.SelectedFolder + "/" + SETTINGS.SelectedFile);

                case "storefileasync":

                    string savingState;

                    if (!task.GetStringValue("savingstate", out savingState))
                        task.SetStringValue("savingstate", "starting");


                    switch (savingState)
                    {
                        case "starting":
                            task.SetStringValue("savingstate", "inprogress");

                            //FileformatBase fi = new FileformatBase();
                            //fi.Name = "Hello world";
                            //fi.TransCoderName = "Skeleton";
                            //fi.SetTimeStamp("hello", 100);

                            //SkeletonOnlyFrame frame = new SkeletonOnlyFrame();
                            //frame.Points[0] = new Point(Vector3.one);

                            //fi.Frames.Add(frame);

                            //fi.Frames.Add(frame);

                            //fi.Frames.Add(frame);
                            FileformatBase BufferFile = SETTINGS.user.DepthTransport.TransCoder.GetBufferFile();

                            IO.Instance.SaveManual(BufferFile, SETTINGS.SelectedFolder + "/" + SETTINGS.SelectedFile, task);

                            break;
                        case "done":
                            done = true;
                            break;
                        default:
                            break;

                    }



                    break;


                case "loadselectedfile":

                    string loadingState;

                    if (!task.GetStringValue("loadingState", out loadingState))
                        task.SetStringValue("loadingState", "starting");

                    switch (loadingState)
                    {
                        case "starting":
                            task.SetStringValue("loadingState", "inprogress");
                            IO.Instance.LoadManual(SETTINGS.SelectedFolder + "/" + SETTINGS.SelectedFile, task);

                            break;

                        case "done":
                            done = true;
                            Log("loading completed");
                          //  FileformatBase loaded = IO.Instance.fileref;
                          
                            
                            break;
                        default:
                            break;

                    }


                 

                   

                    break;

                // -----------------------------------------------------------------------
                // Main roles.

#if SERVER
                case "amserver":

                    //         SETTINGS.deviceMode = DEVICEMODE.SERVER;
                    // Switched to using #if compiling.
                    done = true;

                    break;
#endif

#if CLIENT
                case "amvrclient":

            //        SETTINGS.deviceMode = DEVICEMODE.VRCLIENT;

                    // Switched to using #if compiling.

                    done = true;

                    break;
#endif



                // ----------------------------------------------------------------------------------------------------
                // User data manipulations

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

#if SERVER


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
                        Warning("Encode failed");

                    // task.SetStringValue("debug", "time: " + UserDT.CurrentTime);

                    // user may move in or out of detection.

                    //  task.SetIntValue("user_debugcontrol", UserDT.IsUserDetected() ? 1:0);
                    //    SETTINGS.user.Visualiser.SetMode(task, "user");

                    //task.SetIntValue("user_cloudcontrol", GENERAL.UserMaterialised ? 1 : 0);



#endif

#if CLIENT

                   
                        Application.targetFrameRate = 30;
                        QualitySettings.vSyncCount = 0;

                        DepthTransport UserDT = SETTINGS.user.DepthTransport;

                        // Decode depth from task

                        if (!UserDT.Decode(task, "user"))
						Warning("Decode failed");


                        // put head orientation, include calibartion

                        UserDT.ActiveFrame.HeadOrientation = SETTINGS.HeadsetCorrection * headSet.transform.localRotation;
                        task.SetQuaternionValue("user_headrotation", UserDT.ActiveFrame.HeadOrientation);
                        UserDT.ActiveFrame.HeadPosition = headSet.transform.localPosition;
                        task.SetVector3Value("user_headposition", UserDT.ActiveFrame.HeadPosition);


                   
               
                        //    int usercalibrated;

                        //  task.GetIntValue("usercalibrated", out usercalibrated);
                        //      SETTINGS.user.Visualiser.SetMode(task, "user");
                        //   GENERAL.UserCalibrated = (usercalibrated == 1);
                    
#endif

                    break;


#if SERVER
                case "materialiseon":
                case "MaterialiseOn":

                    // Use visualiser's to and from task method to change setting.


                    SETTINGS.user.SetVisualiser("PointShaded", 0);
                    task.SetIntValue("user_0_cloudvisible", 1);

                    SETTINGS.user.PullVisualiserSettingsFromTask(task, "user");

                    presenceSound.Play();

                    done = true;



                    break;
#endif

                case "materialiseoff":
                case "MaterialiseOff":

                    // Use visualiser's to and from task method to change setting.
#if SERVER

                    task.SetIntValue("user_0_cloudvisible", 0);

                    SETTINGS.user.PullVisualiserSettingsFromTask(task, "user");

                    done = true;

#endif

#if CLIENT

                    // presencehandler should pick up on server change...

                  //      task.SetIntValue("user_0_cloudvisible", 0);

                //    SETTINGS.user.PullVisualiserSettingsFromTask(task, "user");
                   
                        //int v;
                        //if (task.GetIntValue("user_0_cloudvisible", out v))
                        //{

                        //    SETTINGS.user.PullVisualiserSettingsFromTask(task, "user");

                        //}


                       done = true;
                    
#endif
                    break;



                // ----------------------------------------------------------------------------------------------------
                // Presence data manipulations



                case "handlepresences":

                    // Take care of progressing, over the network, of all other instances (not the user).
#if SERVER


                    //// Get current list from the task.

                    //string[] presenceNames;

                    //if (!task.GetStringArrayValue("presences", out presenceNames))
                    //{
                    //    // push list of all presences
                    //    task.SetStringArrayValue("presences", SETTINGS.Presences.Keys.ToArray());

                    //}

                    //// Now go over them and push all data. Create a copy so we can manipulate the actual dictionary along the way.

                    //string[] keys = new string[SETTINGS.Presences.Keys.Count];
                    //SETTINGS.Presences.Keys.CopyTo(keys, 0);

                    // push list of all presences

                    string[] presenceKeys = SETTINGS.Presences.Keys.ToArray();
                    task.SetStringArrayValue("presences", presenceKeys);

                    // Now go over them and push all data. 

                    foreach (string key in presenceKeys)
                    {
                        //  KeyValuePair<string, Presence> presence = new KeyValuePair<string, Presence>();
                        Presence presence;
                        SETTINGS.Presences.TryGetValue(key, out presence);

                        presence.PushAllSettingToTask(task, key);

                        if (presence.DepthTransport.Mode == DEPTHMODE.COPY)
                        {
                            // Copy frame reference.
                            presence.DepthTransport.ActiveFrame = presence.DepthTransport.TargetPresence.DepthTransport.ActiveFrame;

                        }

                        if (presence.DepthTransport.Mode == DEPTHMODE.PLAYBACK)
                        {
                            // Play back while in playback mode and playback successful else fall through.

                            int status = presence.DepthTransport.LoadFrameFromBuffer(presence.DepthTransport.CurrentTime);

                            // Try to retrieve a playback speed.

                            float speed;
                            if (task.GetFloatValue(key + "_speed", out speed))
                                //    {
                                //        Log("speed " + speed);
                                //    }

                                switch (status)
                                {
                                    case -1:
                                    case 0:

                                        // we're before the start or in the buffer

                                        if (!SETTINGS.ManualPlayback)
                                        {
                                            presence.DepthTransport.CurrentTime += speed * Time.deltaTime;
                                        }

                                        //Log("playing");
                                        break;


                                    case 1:
                                        // wé're at the end
                                        //presence.Value.DepthTransport.CurrentTime = presence.Value.DepthTransport.TransCoder.GetBufferFile().StartTime;
                                        if (!SETTINGS.ManualPlayback)
                                        {
                                            presence.DepthTransport.CurrentTime += speed * Time.deltaTime;
                                        }
                                        //   Log("loop");

                                        break;

                                    default:
                                        // Error.
                                        Error("Buffer playback error.");
                                        break;


                                }

                            // Call a delegate to handle any specific timings like looping.

                            if (TimeHandler != null)
                                TimeHandler(task, key, presence, presence.DepthTransport.CurrentTime);


                            // Push time.

                            task.SetFloatValue(key + "_time", presence.DepthTransport.CurrentTime);

                            if (status == 0 && !presence.SoundPlayed)
                            {
                                presence.SoundPlayed = true;
                                presenceSound.Play();
                            }

                            // Debug.

                            task.SetStringValue("debug", "Presences: " + SETTINGS.Presences.Count);



                        }

                    }


#endif

#if CLIENT
                    

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
                                Log("Created an instance of " +presenceName);

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

                                if (presence.DepthTransport.Mode == DEPTHMODE.PLAYBACK && task.GetFloatValue(presenceName+  "_time", out getTime))

                                {


                              


                                    presence.DepthTransport.CurrentTime = getTime;
                                    int status = presence.DepthTransport.LoadFrameFromBuffer(getTime);

                                //if (Input.GetKey("d")){

                                //    Log("presenceName: "+ getTi  " " +status);


                                //    Log("buffer frames: "+presence.DepthTransport.TransCoder.GetBufferFile().Frames.Count);

                                //    Log("buffer end: "+ presence.DepthTransport.TransCoder.GetBufferFile().EndTime);

                                //}


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

                                Log("Removing instance of " + localPresenceNames[i]);

                                Presence presence = SETTINGS.Presences[localPresenceNames[i]];

                                Destroy(presence.gameObject);

                                SETTINGS.Presences.Remove(localPresenceNames[i]);

                            }

                        }

                    
#endif

                    break;

#if SERVER
                case "deletepresences":



                    foreach (KeyValuePair<string, Presence> presence in SETTINGS.Presences)
                    {
                        if (presence.Key != "user")
                        {

                            Destroy(presence.Value.gameObject);

                        }
                    }

                    SETTINGS.Presences.Clear();
                    SETTINGS.Presences.Add("user", SETTINGS.user);



                    done = true;
                    break;

                case "endcirclescene":

                    TimeHandler = null;

                    foreach (KeyValuePair<string, Presence> presence in SETTINGS.Presences)
                    {
                        if (presence.Key != "user")
                        {

                            Destroy(presence.Value.gameObject);

                        }
                    }

                    SETTINGS.Presences.Clear();
                    SETTINGS.Presences.Add("user", SETTINGS.user);

                    done = true;
                    break;
#endif


                // ----------------------------------------------------------------------------------------------------
                // Pauses, only run on server
#if SERVER
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


                case "pause15":

                    if (!task.GetFloatValue("timeout", out TimeOut))
                    {
                        TimeOut = Time.time + 15;
                        task.SetFloatValue("timeout", TimeOut);

                    }

                    if (Time.time > TimeOut)
                        done = true;

                    break;

#endif
                // ----------------------------------------------------------------------------------------------------
                // Deus user manipulations
#if SERVER
                case "grabframe":


                    Log("grabbing frame");
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

#endif
                // ----------------------------------------------------------------------------------------------------
                // Data capture and playback.

#if SERVER

                //case "loadselectedfile":

                //    //    IO.Instance.LoadFile(SETTINGS.SelectedFolder + "/" + SETTINGS.SelectedFile);
                //    Log("Loading file name " + SETTINGS.SelectedFile);

                //    IO.Instance.LoadFileAsync(SETTINGS.SelectedFolder + "/" + SETTINGS.SelectedFile, task);

                //    // string loadingState;

                //    if (task.GetStringValue("loadingstate", out loadingState))
                //    {
                //        if (loadingState == "done")
                //            Log("loading completed");
                //        else
                //            Log("loading failed");
                //        done = true;
                //    }




                //    break;

                case "playbackfile":

                    // Play back the checked out file. SHOULDN"t THIS BE SERVER ONNLY??

                    Log("Starting name " + SETTINGS.SelectedFile);

                    FileformatBase pbBuffer = IO.Instance.LoadFile(SETTINGS.SelectedFolder + "/" + SETTINGS.SelectedFile);

                    //      Log("Starting name " + IO.CheckedOutFile);

                    TimeHandler = PlaybackStop;

                    if (pbBuffer != null)
                    {
                        pbBuffer.DumpTimeStamps();

                        if (pbBuffer.EndTime > pbBuffer.StartTime)
                        {

                            Log("Start time: " + pbBuffer.StartTime + " End time: " + pbBuffer.EndTime);

                            float OutPoint = pbBuffer.EndTime;

                            // Not a placeholder file so proceed.

                            string pn = "playbackpresence";

                            if (!SETTINGS.Presences.TryGetValue(pn, out fileplayback))
                            {

                                // No presence in scene so create it.
                                fileplayback = Presence.Create(presences, pn);

                            }

                            fileplayback.SetVisualiser(SETTINGS.DefaultVisualiser, 0);
                        //    fileplayback.SetVisualiser("ShowSkeleton", 1);

                            fileplayback.SetTranscoder(pbBuffer.TransCoderName);

                            fileplayback.DepthTransport.TransCoder.SetBufferFile(pbBuffer);
                            fileplayback.DepthTransport.CurrentTime = pbBuffer.StartTime;
                            fileplayback.DepthTransport.Mode = DEPTHMODE.PLAYBACK;

                            fileplayback.SetVisualiseTransform(Vector3.zero, Vector3.one, Quaternion.identity);

                            // Roundabout way of setting setting on visualiser. This task will be removed.
                            //  task.SetIntValue("user_0_cloudvisible", 1);
                            task.SetIntValue("playbackpresence_0_cloudvisible", 1);
                            task.SetIntValue("playbackpresence_1_cloudvisible", 1);
                      //      task.SetIntValue("playbackpresence_1_isdrawing", 1);

                            fileplayback.PullVisualiserSettingsFromTask(task, "playbackpresence");

                            StoryTask handler = AssitantDirector.FindTaskByByLabel("handler");
                            handler.SetFloatValue(pn + "_speed", 1f);
                            handler.SetFloatValue(pn + "_outpoint", OutPoint);

                            Log("started buffer " + pbBuffer.Name);

                        }

                    }

                    done = true;

                    break;

                case "stopplaybackfile":



                    if (fileplayback != null)
                        fileplayback.DepthTransport.Mode = DEPTHMODE.OFF;


                    done = true;
                    break;


                case "stopallplayback":



                    foreach (KeyValuePair<string, Presence> presence in SETTINGS.Presences)
                    {
                        if (presence.Key != "user")
                        {

                            presence.Value.DepthTransport.Mode = DEPTHMODE.OFF;

                        }

                    }



                    done = true;
                    break;



                case "playmirror":

                    // Creates a mirror presence of the user if recording OR of the playback file if playing.



                    Log("Starting mirror of " + SETTINGS.SelectedFile);

                    FileBuffer = IO.Instance.LoadFile(SETTINGS.SelectedFolder + "/" + SETTINGS.SelectedFile);

                    if (FileBuffer != null)
                    {

                        string mirrorPresence = "mirrorpresence";

                        if (!SETTINGS.Presences.TryGetValue(mirrorPresence, out fileplayback))
                        {
                            fileplayback = Presence.Create(presences, mirrorPresence);
                            //      SETTINGS.Presences.Add(mirrorPresence, fileplayback);
                        }

                        fileplayback.SetVisualiser(SETTINGS.DefaultVisualiser);
                        fileplayback.SetTranscoder(FileBuffer.TransCoderName);
                        fileplayback.DepthTransport.TransCoder.SetBufferFile(FileBuffer);
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

                        Log("started buffer " + FileBuffer.Name);

                    }



                    done = true;

                    break;


                case "MirrorOn":

                    // Creates a mirrored duplicate of the User Presence.

                    Log("Starting mirror");

                    string mp = "mirrorpresence";
                    Presence target = SETTINGS.user;

                    if (!SETTINGS.Presences.TryGetValue(mp, out fileplayback))
                        fileplayback = Presence.Create(presences, mp);

                    fileplayback.SetTranscoder(target.GetTranscoder());
                    fileplayback.DepthTransport.Mode = DEPTHMODE.COPY;
                    fileplayback.DepthTransport.Target = "user";

                    fileplayback.SetVisualiser(SETTINGS.DefaultVisualiser);

                    // We'll place it relative to the current user position.

                    Vector3 place = SETTINGS.user.DepthTransport.ActiveFrame.Joints[(int)NuiSkeletonPositionIndex.Head];
                    place.y = 0;
                    place.x = 0;
                    place.z = place.z * 2;// When we project, we get the distance from the kinect as well. May want to somehow incorporate this.
                    place += new Vector3(0, 0, -1);
                    fileplayback.SetVisualiseTransform(place, new Vector3(1, 1, -1), Quaternion.identity);

                    // Show visualiser.

                    task.SetIntValue(mp + "_0_cloudvisible", 1);
                    fileplayback.PullVisualiserSettingsFromTask(task, mp);

                    done = true;
                    break;

                case "playecho":

                    // LEGACY

                    // Play back the previous n files if available.

                    string[] Files = FileList(SETTINGS.SelectedFolder);

                    int Selected = Array.IndexOf(Files, SETTINGS.SelectedFile);

                    for (int c = 1; c < 3; c++)
                    {


                        string name = (Selected + c) < Files.Length ? Files[Selected + c] : "";

                        Log("starting name " + c + " " + name);

                        FileBuffer = IO.Instance.LoadFile(SETTINGS.SelectedFolder + "/" + name);


                        if (FileBuffer != null)
                        {



                            if (FileBuffer.EndTime > FileBuffer.StartTime)
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
                                fileplayback.SetTranscoder(FileBuffer.TransCoderName);

                                fileplayback.DepthTransport.TransCoder.SetBufferFile(FileBuffer);

                                fileplayback.DepthTransport.CurrentTime = fileplayback.DepthTransport.TransCoder.GetBufferFile().StartTime - UnityEngine.Random.Range(0.5f, 3f);

                                fileplayback.SetVisualiseTransform(Vector3.zero, Vector3.one, Quaternion.identity);

                                fileplayback.DepthTransport.Mode = DEPTHMODE.PLAYBACK;

                                task.SetFloatValue("playbackpresence" + c + "_cloudvisible", 1);
                                fileplayback.PullVisualiserSettingsFromTask(task, "playbackpresence" + c);

                                Log("started buffer " + c + " " + FileBuffer.Name);
                            }







                        }





                    }


                    done = true;

                    break;




                case "AddCircleClone":

                    // Plays back additional copies of the user presence.

                    //FileBuffer = IO.Instance.LoadFile(IO.Instance.CheckedOutFile);

                    FileBuffer = SETTINGS.user.DepthTransport.TransCoder.GetBufferFile();


                    if (FileBuffer != null)
                    {
                        // We have a buffer file to play from.

                        // Set buffer ref and a delegate to loop these clones. Remember to remove it afterwards.

                        //   buffer = pbcBuf;
                        TimeHandler = CircleLoop;

                        // Find out what number of clones we're at.

                        int clone = 0;
                        Presence clonepresence;
                        while (SETTINGS.Presences.TryGetValue("circlepresence" + clone, out clonepresence))
                        {
                            clone++;
                        }

                        PresenceName = "circlepresence" + clone;

                        if (!SETTINGS.Presences.TryGetValue(PresenceName, out fileplayback))
                            fileplayback = Presence.Create(presences, PresenceName);

                        fileplayback.SetTranscoder(FileBuffer.TransCoderName);
                        fileplayback.DepthTransport.TransCoder.SetBufferFile(FileBuffer);
                        fileplayback.DepthTransport.Mode = DEPTHMODE.PLAYBACK;

                        float time = FileBuffer.GetTimeStamp("TimeStamp_BeginCircle");

                        fileplayback.DepthTransport.CurrentTime = time;
                        Log("setting current time to " + time);

                        fileplayback.SetVisualiser(SETTINGS.DefaultVisualiser);

                        fileplayback.SetVisualiseTransform(Vector3.zero, Vector3.one, Quaternion.identity);

                        // Show visualiser.

                        task.SetIntValue(PresenceName + "_0_cloudvisible", 1);
                        fileplayback.PullVisualiserSettingsFromTask(task, PresenceName);

                        StoryTask handler = AssitantDirector.FindTaskByByLabel("handler");
                        handler.SetFloatValue(PresenceName + "_speed", 1f);

                        Log("started circle presence for " + FileBuffer.Name);

                    }

                    done = true;

                    break;


                case "SpawnPresences":

                    // Generate bits of playback from previous sessions.

                    if (SETTINGS.Presences.Count > 3)
                        break;


                    if (UnityEngine.Random.value > 1f / 60f)
                        break;

                    int Index;

                    if (!task.GetIntValue("index", out Index))
                    {
                        Index = 1;
                        task.SetIntValue("index", Index);
                        TimeHandler = PresenceClone;
                    }



                    Files = FileList(SETTINGS.SelectedFolder);

                    int Current = Array.IndexOf(Files, SETTINGS.SelectedFile);




                    //  int Current = IO.Instance.CheckedOutFileIndex();
                    int Retrieve = Current + Index;

                    if (Retrieve >= Files.Length)
                    {
                        // Loop through sessions.
                        Index = 1;
                        Retrieve = Current + Index;
                    }

                    string FileName = Files[Retrieve];


                    FileBuffer = IO.Instance.LoadFile(FileName);

                    Log("attempting clone presence for " + FileName);

                    if (FileBuffer != null)
                    {
                        // We have a buffer file to play from.
                        FileBuffer.DumpTimeStamps();

                        float begin = FileBuffer.GetTimeStamp("StartTime");
                        float end = FileBuffer.GetTimeStamp("EndTime");
                        float length = end - begin;

                        if (begin != -1 && end != -1 && length > 15)
                        {
                            // It has a length we can work with.

                            float inpoint = UnityEngine.Random.Range(0, end - 20.5f);
                            float duration = UnityEngine.Random.Range(10, 20);
                            float OutPoint = inpoint + duration;

                            // Find out what number of clones we're at.

                            int clone = 0;
                            Presence clonepresence;
                            while (SETTINGS.Presences.TryGetValue("presenceclone" + clone, out clonepresence))
                            {
                                clone++;
                            }

                            PresenceName = "presenceclone" + clone;

                            if (!SETTINGS.Presences.TryGetValue(PresenceName, out fileplayback))
                                fileplayback = Presence.Create(presences, PresenceName);

                            fileplayback.SetTranscoder(FileBuffer.TransCoderName);
                            fileplayback.DepthTransport.TransCoder.SetBufferFile(FileBuffer);
                            fileplayback.DepthTransport.Mode = DEPTHMODE.PLAYBACK;
                            fileplayback.DepthTransport.CurrentTime = inpoint;
                            fileplayback.SetVisualiser(SETTINGS.DefaultVisualiser);

                            // random location. taking into account the fact that we are positioning the projector, not the presence.

                            Vector2 center2d = new Vector2(0, SETTINGS.kinectCentreDistance);
                            float a = UnityEngine.Random.Range(0, Mathf.PI * 2);
                            Vector2 v = new Vector2(Mathf.Sin(a), Mathf.Cos(a));
                            float offset = -SETTINGS.kinectCentreDistance + UnityEngine.Random.Range(-2f, 2f);// first compensate the projection distance to center. then add random. so projecting towards the center from different angles, with some offset.
                            Vector2 placement = center2d + v * offset;
                            Quaternion rotation = Quaternion.Euler(0, a * Mathf.Rad2Deg, 0);

                            // all presences should now 'face' the user.

                            fileplayback.SetVisualiseTransform(new Vector3(placement.x, 0, placement.y), Vector3.one, rotation);

                            // Show visualiser.

                            task.SetIntValue(PresenceName + "_0_cloudvisible", 1);
                            fileplayback.PullVisualiserSettingsFromTask(task, PresenceName);

                            StoryTask handler = AssitantDirector.FindTaskByByLabel("handler");
                            handler.SetFloatValue(PresenceName + "_speed", 1f);
                            handler.SetFloatValue(PresenceName + "_outpoint", OutPoint);

                            Log("started clone presence for " + FileBuffer.Name);
                        }
                    }

                    Index++;
                    task.SetIntValue("index", Index);

                    //  done = true;

                    break;

                case "ShatterUser":

                    // Generate bits of playback from current session

                    if (SETTINGS.Presences.Count > 4)
                        break;


                    if (UnityEngine.Random.value > 1f / 60f)
                        break;

                    //FileBuffer = IO.Instance.LoadFile(IO.Instance.CheckedOutFile);
                    FileBuffer = SETTINGS.user.DepthTransport.TransCoder.GetBufferFile();

                    TimeHandler = ShatterHandler;


                    if (FileBuffer != null)
                    {
                        // We have a buffer file to play from.

                        FileBuffer.DumpTimeStamps();

                        float begin = FileBuffer.GetTimeStamp("StartTime");
                        float end = SETTINGS.user.DepthTransport.CurrentTime;
                        float length = end - begin;

                        if (begin != -1 && length > 15)
                        {
                            // It has a length we can work with.

                            // Set random speed, and random in and out (taking speed sign into account)

                            float speed = UnityEngine.Random.Range(1f, 3f);
                            speed = UnityEngine.Random.value > 0.5f ? speed : -speed;
                            float inpoint, duration, OutPoint;

                            if (speed > 0)
                            {

                                inpoint = UnityEngine.Random.Range(0, end - 10.5f);
                                duration = UnityEngine.Random.Range(5, 10);
                                OutPoint = inpoint + duration;
                            }
                            else
                            {
                                inpoint = UnityEngine.Random.Range(10.5f, end);
                                duration = UnityEngine.Random.Range(5, 10);
                                OutPoint = inpoint - duration;

                            }

                            // Find out what number of clones we're at.

                            int clone = 0;
                            Presence clonepresence;
                            while (SETTINGS.Presences.TryGetValue("shatterclone" + clone, out clonepresence))
                            {
                                clone++;
                            }

                            PresenceName = "shatterclone" + clone;

                            if (!SETTINGS.Presences.TryGetValue(PresenceName, out fileplayback))
                                fileplayback = Presence.Create(presences, PresenceName);

                            fileplayback.SetTranscoder(FileBuffer.TransCoderName);
                            fileplayback.DepthTransport.TransCoder.SetBufferFile(FileBuffer);
                            fileplayback.DepthTransport.Mode = DEPTHMODE.PLAYBACK;

                            fileplayback.DepthTransport.CurrentTime = inpoint;

                            fileplayback.SetVisualiser(SETTINGS.DefaultVisualiser);

                            // random location. taking into account the fact that we are positioning the projector, not the presence.

                            Vector2 center2d = new Vector2(0, SETTINGS.kinectCentreDistance);

                            float a = UnityEngine.Random.Range(0, Mathf.PI * 2);
                            Vector2 v = new Vector2(Mathf.Sin(a), Mathf.Cos(a));
                            float offset = -SETTINGS.kinectCentreDistance + UnityEngine.Random.Range(-2f, 2f);// first compensate the projection distance to center. then add random. so projecting towards the center from different angles, with some offset.

                            Vector2 placement = center2d + v * offset;
                            Quaternion rotation = Quaternion.Euler(0, a * Mathf.Rad2Deg, 0);

                            fileplayback.SetVisualiseTransform(new Vector3(placement.x, 0, placement.y), Vector3.one, rotation);

                            // Show visualiser.

                            task.SetIntValue(PresenceName + "_0_cloudvisible", 1);
                            fileplayback.PullVisualiserSettingsFromTask(task, PresenceName);

                            StoryTask handler = AssitantDirector.FindTaskByByLabel("handler");
                            handler.SetFloatValue(PresenceName + "_speed", speed);
                            handler.SetFloatValue(PresenceName + "_outpoint", OutPoint);

                            Log("started shatter presence for " + FileBuffer.Name);
                        }
                    }

                    //  done = true;

                    break;

                //case "RemoveCircleClones":


                //    foreach (KeyValuePair<string,Presence> presence in SETTINGS.Presences)
                //    {
                //        if (presence.Key.IndexOf("circlepresence") != -1)
                //        {
                //            SETTINGS.Presences.Remove(presence.Key);
                //            Destroy(presence.Value.gameObject);

                //        }



                //    }


                //    if (name.IndexOf("circlepresence") == -1)

                //        done = true;
                //    break;


                case "DelayOn":

                    // Plays back additional copies of the user presence.

                    //FileBuffer = IO.Instance.LoadFile(IO.Instance.CheckedOutFile);
                    FileBuffer = SETTINGS.user.DepthTransport.TransCoder.GetBufferFile();

                    if (FileBuffer != null)
                    {

                        // We have a buffer file to play from.

                        PresenceName = "delaypresence";

                        if (!SETTINGS.Presences.TryGetValue(PresenceName, out fileplayback))
                        {

                            fileplayback = Presence.Create(presences, PresenceName);
                            //   SETTINGS.Presences.Add("playbackpresence" + c, fileplayback);

                        }



                        fileplayback.SetTranscoder(FileBuffer.TransCoderName);
                        fileplayback.DepthTransport.TransCoder.SetBufferFile(FileBuffer);
                        fileplayback.DepthTransport.Mode = DEPTHMODE.PLAYBACK;

                        fileplayback.DepthTransport.CurrentTime = SETTINGS.user.DepthTransport.CurrentTime; // we may be retrieve before stored.
                        Log("setting current time to " + SETTINGS.user.DepthTransport.CurrentTime);


                        fileplayback.SetVisualiser(SETTINGS.DefaultVisualiser);

                        // We'll place it relative to the current user position.

                        place = SETTINGS.user.DepthTransport.ActiveFrame.Joints[(int)NuiSkeletonPositionIndex.Head];
                        place.y = 0;
                        place.x = 0;
                        place.z = place.z * 2;// When we project, we get the distance from the kinect as well. May want to somehow incorporate this.
                        place += new Vector3(0, 0, -1);
                        fileplayback.SetVisualiseTransform(place, new Vector3(1, 1, -1), Quaternion.identity);

                        // Show visualiser.

                        task.SetIntValue(PresenceName + "_0_cloudvisible", 1);
                        fileplayback.PullVisualiserSettingsFromTask(task, PresenceName);


                        StoryTask handler = AssitantDirector.FindTaskByByLabel("handler");
                        handler.SetFloatValue(PresenceName + "_speed", 1f);


                        Log("started delay " + FileBuffer.Name);


                        //}



                    }








                    done = true;

                    break;



                case "DelaySlow":

                    PresenceName = "delaypresence";

                    Presence delayp;

                    if (SETTINGS.Presences.TryGetValue(PresenceName, out delayp))
                    {

                        StoryTask handler = AssitantDirector.FindTaskByByLabel("handler");
                        handler.SetFloatValue(PresenceName + "_speed", 0.5f);


                    }


                    done = true;


                    break;

                case "DelayStop":

                    PresenceName = "delaypresence";


                    if (SETTINGS.Presences.TryGetValue(PresenceName, out delayp))
                    {

                        StoryTask handler = AssitantDirector.FindTaskByByLabel("handler");
                        handler.SetFloatValue(PresenceName + "_speed", 0f);


                    }


                    done = true;


                    break;

                case "DelayReverse":

                    PresenceName = "delaypresence";

                    if (SETTINGS.Presences.TryGetValue(PresenceName, out delayp))
                    {

                        StoryTask handler = AssitantDirector.FindTaskByByLabel("handler");
                        handler.SetFloatValue(PresenceName + "_speed", -2f);


                    }


                    done = true;


                    break;



                case "playdelay":

                    // Plays back additional copies of the user or playback presence.


                    //int checkedOut = IO.Instance.CheckedOutFileIndex();

                    for (int c = 1; c < 3; c++)
                    {

                        //string name = IO.Instance.GetFilePath(checkedOut + c);

                        //Log("Generating name " + c);

                        //    FileBuffer = IO.Instance.LoadFile(IO.Instance.CheckedOutFile);
                        FileBuffer = SETTINGS.user.DepthTransport.TransCoder.GetBufferFile();


                        if (FileBuffer != null)
                        {

                            //if (pbcBuf.EndTime > pbcBuf.StartTime)
                            //{
                            // Not a placeholder file so proceed.
                            PresenceName = "playbackpresence" + c;
                            if (!SETTINGS.Presences.TryGetValue(PresenceName, out fileplayback))
                            {

                                fileplayback = Presence.Create(presences, PresenceName);
                                //   SETTINGS.Presences.Add("playbackpresence" + c, fileplayback);

                            }

                            fileplayback.SetVisualiser(SETTINGS.DefaultVisualiser);
                            fileplayback.SetTranscoder(FileBuffer.TransCoderName);
                            fileplayback.DepthTransport.TransCoder.SetBufferFile(FileBuffer);

                            fileplayback.DepthTransport.CurrentTime = -0.5f * c;

                            fileplayback.SetVisualiseTransform(new Vector3(0.5f * c, 0, 0), Vector3.one, Quaternion.identity);

                            fileplayback.DepthTransport.Mode = DEPTHMODE.PLAYBACK;

                            task.SetFloatValue("playbackpresence" + c + "_cloudvisible", 1);
                            fileplayback.PullVisualiserSettingsFromTask(task, "playbackpresence" + c);

                            Log("started buffer " + c + " " + FileBuffer.Name);


                            //}



                        }





                    }


                    done = true;

                    break;

                case "nonewdrawings":

                    // We're changing the task settings from here.
                    Log("nonewdrawings  ");

                    StoryTask drawtask = AssitantDirector.FindTaskByByLabel("PlayDrawings");// find task by label, should return the playbackdrawings task

                    if (drawtask != null && drawtask.description == "playbackdrawings")
                    {
                        drawtask.SetIntValue("index", 1000); // no new instances, because file index will be out of range.

                        for (int dp = 0; dp < 3; dp++)
                        {
                            PresenceName = "drawingpresence" + dp;
                            int state = 0;

                            if (drawtask.GetIntValue(PresenceName + "_state", out state) && state == 2)
                            {
                                if (SETTINGS.Presences.TryGetValue(PresenceName, out fileplayback))
                                {

                                    // stop drawing.

                                    drawtask.SetIntValue(PresenceName + "_0_isdrawing", 0);
                                    fileplayback.PullVisualiserSettingsFromTask(drawtask, PresenceName);
                                    drawtask.SetFloatValue(PresenceName + "_timeout", Time.time);
                                    drawtask.SetIntValue(PresenceName + "_state", 3);
                                    Log("Stopping drawing " + PresenceName);

                                }
                            }

                        }
                    }
                    else
                    {
                        Log("task not found correctly");
                    }

                    done = true;

                    break;

                case "playbackdrawings":

                    // Plays back the drawing section of a session, using the drawing visualiser.

                    float BeginDraw, EndDraw;
                    //  Index;

                    if (!task.GetIntValue("index", out Index))
                    {
                        Index = 1;
                        task.SetIntValue("index", Index);
                    }

                    Vector3[] Positions = new Vector3[]
                       {
                            new Vector3(0, 0,-1f),
                            new Vector3(1f, 0,-0.25f),
                            new Vector3(-2f, 0,0f)
                       };

                    // this can be simplified using the timing delegate to clean up on completion. so we can just generate presences and let them roam ?

                    for (int dp = 0; dp < 3; dp++)
                    {

                        PresenceName = "drawingpresence" + dp;



                        int state;

                        if (!task.GetIntValue(PresenceName + "_state", out state))
                            task.SetIntValue(PresenceName + "_state", state); // 0


                        switch (state)
                        {
                            case 0:

                                // Empty.

                                if (UnityEngine.Random.value < (1f / 60f))
                                {
                                    state = 1;

                                }

                                break;

                            case 1:
                                // New.

                                if (SETTINGS.Presences.TryGetValue(PresenceName, out fileplayback))
                                {
                                    Warning("Presence already exits: " + PresenceName);
                                    state = -1;
                                    break;
                                }

                                // Create presence.
                                Files = FileList(SETTINGS.SelectedFolder);

                                Current = Array.IndexOf(Files, SETTINGS.SelectedFile);

                                Retrieve = Current + Index;

                                if (Retrieve >= Files.Length)
                                {
                                    // Loop through sessions.
                                    Index = 1;
                                    break;
                                }



                                //   Current = IO.Instance.CheckedOutFileIndex();

                                FileName = Files[Retrieve];

                                FileBuffer = IO.Instance.LoadFile(SETTINGS.SelectedFolder + "/" + FileName);

                                if (FileBuffer == null)
                                {
                                    // No file, just revert to empty.
                                    state = 0;
                                    Log("no buffer found for " + FileName);
                                    break;
                                }

                                Log("Starting drawing presence " + FileName);

                                FileBuffer.DumpTimeStamps();

                                BeginDraw = FileBuffer.GetTimeStamp("TimeStamp_BeginDraw");

                                if (BeginDraw == -1)
                                {
                                    Warning("No drawing timestamp for: " + FileBuffer.Name);
                                    state = -1;
                                    Index++;
                                    break;

                                }
                                // Proceed if there is a value for begindraw

                                fileplayback = Presence.Create(presences, PresenceName);

                                fileplayback.SetVisualiser("ShowSkeleton");
                                fileplayback.SetTranscoder(FileBuffer.TransCoderName);
                                fileplayback.DepthTransport.TransCoder.SetBufferFile(FileBuffer);

                                fileplayback.DepthTransport.CurrentTime = BeginDraw;

                                // Create relative placement

                                Vector3 UserPosition = SETTINGS.user.DepthTransport.ActiveFrame.Joints[(int)NuiSkeletonPositionIndex.Head];
                                UserPosition.y = 0;

                                float randomy = UnityEngine.Random.Range(-180f, 180f);

                                fileplayback.SetVisualiseTransform(UserPosition + Positions[dp], Vector3.one, Quaternion.Euler(0, randomy, 0));
                                fileplayback.DepthTransport.Mode = DEPTHMODE.PLAYBACK;

                                task.SetIntValue(PresenceName + "_0_cloudvisible", 1);
                                task.SetIntValue(PresenceName + "_0_isdrawing", 1);
                                fileplayback.PullVisualiserSettingsFromTask(task, PresenceName);

                                StoryTask handler = AssitantDirector.FindTaskByByLabel("handler");
                                handler.SetFloatValue(PresenceName + "_speed", 1f);

                                Log("started drawing " + FileBuffer.Name);

                                Index++;

                                state = 2;


                                break;

                            case 2:

                                // Active
                                // Check if presence is at end of drawing

                                if (!SETTINGS.Presences.TryGetValue(PresenceName, out fileplayback))
                                {
                                    // Error, abort.
                                    state = -1;
                                    break;
                                }

                                BeginDraw = fileplayback.DepthTransport.TransCoder.GetBufferFile().GetTimeStamp("TimeStamp_BeginDraw");
                                EndDraw = fileplayback.DepthTransport.TransCoder.GetBufferFile().GetTimeStamp("TimeStamp_EndDraw");

                                if (fileplayback.DepthTransport.CurrentTime > EndDraw || fileplayback.DepthTransport.CurrentTime < BeginDraw)
                                {
                                    // Head outside of drawing timespan.
                                    task.SetIntValue(PresenceName + "_0_cloudvisible", 1);
                                    task.SetIntValue(PresenceName + "_0_isdrawing", 0);
                                    fileplayback.PullVisualiserSettingsFromTask(task, PresenceName);
                                    Log("Stopping drawing " + PresenceName);
                                    task.SetFloatValue(PresenceName + "_timeout", Time.time);

                                    Log("Stopping drawing " + fileplayback.DepthTransport.TransCoder.GetBufferFile().Name);

                                    state = 3;
                                }

                                break;

                            case 3:
                                // We've stopped drawing. Wait for animation to play out before cleaning up.

                                float to;
                                task.GetFloatValue(PresenceName + "_timeout", out to);

                                if (Time.time > to + 10f)
                                {
                                    state = -1;
                                }

                                break;



                            case -1:
                            default:

                                // Something wrong or done, try to remove presence.
                                if (SETTINGS.Presences.TryGetValue(PresenceName, out fileplayback))
                                {
                                    Destroy(fileplayback.gameObject);
                                    SETTINGS.Presences.Remove(PresenceName);
                                    Log("Removed presence " + PresenceName);
                                }
                                state = 0;
                                break;


                        }

                        task.SetIntValue(PresenceName + "_state", state);
                        task.SetIntValue("index", Index);
                    }

                    break;







                case "waitforallplaybacktoend":

                    // 

                    bool AllOff = true;



                    foreach (KeyValuePair<string, Presence> presence in SETTINGS.Presences)
                    {
                        if (presence.Key != "user" && presence.Value.DepthTransport.Mode != DEPTHMODE.OFF)
                        {

                            AllOff = false;

                        }
                    }



                    if (AllOff)
                        done = true;


                    break;
#endif



                case "recordprepare":

#if SERVER

                    if (SETTINGS.user.DepthTransport != null && SETTINGS.SelectedFolder != "" && SETTINGS.SelectedFile != "")
                    {
                        string path = SETTINGS.SelectedFolder + "/" + SETTINGS.SelectedFile;
                        Log("preparing record for " + path);


                        SETTINGS.user.DepthTransport.TransCoder.CreateBufferFile(path);

                        task.SetStringValue("user_file", path);

                        done = true;

                    }
                    else
                    {
                        Error("Cannot prepare record");
                        done = true;
                    }


#endif

#if CLIENT
                    Log("preparing buffer" );
                         
                        if (SETTINGS.user.DepthTransport != null)
                        {
                            // Wait for filename then fall through
                            string file;
                            if (task.GetStringValue("user_file", out file))
                            {
                                IO.Instance.SelectFile(file);

                                SETTINGS.user.DepthTransport.TransCoder.CreateBufferFile(IO.Instance.CheckedOutFile);

                            Log("created buffer" );
                                done = true;

                            }


                        }
                        else
                        {
                            done = true;
                        }
                    
#endif
                    break;

                case "TimeStampTest":

                    float CurrentTime = SETTINGS.user.DepthTransport.CurrentTime;
                    SETTINGS.user.DepthTransport.TransCoder.GetBufferFile().SetTimeStamp("test", CurrentTime);

                    done = true;
                    break;

                //case "TimeStamp_Begin":

                //    CurrentTime = SETTINGS.user.DepthTransport.CurrentTime;
                //    SETTINGS.user.DepthTransport.TransCoder.GetBufferFile().SetTimeStamp("TimeStamp_Begin", CurrentTime);

                //    done = true;
                //    break;

                case "TimeStamp_BeginDraw":

                    CurrentTime = SETTINGS.user.DepthTransport.CurrentTime;
                    SETTINGS.user.DepthTransport.TransCoder.GetBufferFile().SetTimeStamp("TimeStamp_BeginDraw", CurrentTime);

                    done = true;
                    break;

                case "TimeStamp_EndDraw":

                    CurrentTime = SETTINGS.user.DepthTransport.CurrentTime;
                    SETTINGS.user.DepthTransport.TransCoder.GetBufferFile().SetTimeStamp("TimeStamp_EndDraw", CurrentTime);

                    done = true;
                    break;

                case "TimeStamp_BeginCircle":

                    CurrentTime = SETTINGS.user.DepthTransport.CurrentTime;
                    SETTINGS.user.DepthTransport.TransCoder.GetBufferFile().SetTimeStamp("TimeStamp_BeginCircle", CurrentTime);

                    done = true;
                    break;

                case "TimeStamp_EndCircle":

                    CurrentTime = SETTINGS.user.DepthTransport.CurrentTime;
                    SETTINGS.user.DepthTransport.TransCoder.GetBufferFile().SetTimeStamp("TimeStamp_EndCircle", CurrentTime);

                    done = true;
                    break;





                case "recordstart":
                case "IsRecording":

#if SERVER

                    //     if (SETTINGS.user.DepthTransport != null && IO.Instance.CheckedOutFile != "")
                    if (SETTINGS.user.DepthTransport != null && SETTINGS.SelectedFolder != "" && SETTINGS.SelectedFile != "")
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


                        //          task.SetStringValue("debug", "" + (TimeOut - Time.time));


                    }
                    else
                    {
                        done = true;
                    }


#endif

#if CLIENT
                  
                        if (SETTINGS.user.DepthTransport != null)
                        {

                            SETTINGS.user.DepthTransport.Mode = DEPTHMODE.RECORD;

                           done = true;



                        }

                  

#endif
                    break;

                case "recordstop":

                    // Same for server and client.

                    if (SETTINGS.user.DepthTransport.Mode == DEPTHMODE.RECORD)
                    {
                        SETTINGS.user.DepthTransport.Mode = DEPTHMODE.LIVE;

                        Log("Stopped recording. Logged frames " + SETTINGS.user.DepthTransport.TransCoder.GetBufferFile().Frames.Count);


                        FileformatBase BufferFile = SETTINGS.user.DepthTransport.TransCoder.GetBufferFile();
                        IO.Instance.SaveFile(BufferFile, SETTINGS.SelectedFolder + "/" + SETTINGS.SelectedFile);

                    }


                    done = true;
                    break;

                case "recordoff":

                    // Same for server and client.

                    if (SETTINGS.user.DepthTransport.Mode == DEPTHMODE.RECORD)
                    {
                        SETTINGS.user.DepthTransport.Mode = DEPTHMODE.LIVE;

                        Log("Stopped recording. Logged frames " + SETTINGS.user.DepthTransport.TransCoder.GetBufferFile().Frames.Count);


                     //   FileformatBase BufferFile = SETTINGS.user.DepthTransport.TransCoder.GetBufferFile();
                 //       IO.Instance.SaveFile(BufferFile, SETTINGS.SelectedFolder + "/" + SETTINGS.SelectedFile);

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


                // ----------------------------------------------------------------------------------------------------
                // NETWORKING

                case "networkguion":

                    dataController.displayNetworkGUI(true);

                    done = true;

                    break;

#if CLIENT
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

                            Log(  "Found broadcast server");

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
#endif
#if SERVER
                case "listenforclients":


                    // Server side.

                    int ConnectedClients = dataController.serverConnections();

                    //  led.SetActive(ConnectedClients > 0);
                    GENERAL.wasConnected = (ConnectedClients > 0);


                    task.SetStringValue("debug", "clients: " + ConnectedClients);

                    int newClient = GENERAL.GETNEWCONNECTION();

                    if (newClient != -1)
                    {

                        Log("New client on connection " + newClient);

                        task.setCallBack("newclient");

                    }

                    break;
#endif

#if CLIENT
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

                    Log(  "Starting network client.");

                    dataController.startNetworkClient(GENERAL.networkServer);

                    done = true;

                    break;
#endif
#if SERVER
                case "startserver":

                    Log("Starting network server");

                    dataController.startBroadcastServer();
                    dataController.startNetworkServer();

                    done = true;

                    break;
#endif

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
                case "makenewfolder":

                case "makenewfile":
                case "setfiledefaults":
                    {
                        ClearFileList();

                        done = true;
                        break;
                    }

                case "void":


                    Warning("Launching on void.");

                    done = true;
                    break;


                default:

                    // Default is caught in 
                    done = true;

                    break;

            }

            return done;

        }

        //

        string[] _list;
        string _folder;

        // Don't want to query the disk every frame, so we keep a cache. 
        // Need to make sure the cache gets flushed when needed, which is in effect when a file is added. Changing folders it notices.

        string[] FileList(string folder)
        {
            if (folder != _folder)
            {
                _folder = folder;
                _list = IO.Instance.GetFiles(folder);
            }
            return _list;
        }

        void ClearFileList()
        {
            _list = new string[0];
            _folder = "";
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

        //   FileformatBase buffer;

        void CircleLoop(StoryTask task, string name, Presence presence, float time)
        {
            // See if any of the clones has reached the end of the circle.
            // buffer to be set manually for this delegate to work.

            if (name.IndexOf("circlepresence") == -1)
                return;

            FileformatBase buffer = presence.DepthTransport.TransCoder.GetBufferFile();

            if (buffer != null)
            {
                float EndTime = buffer.GetTimeStamp("TimeStamp_EndCircle");//-1 while not yet set.

                if (EndTime != -1 && time >= EndTime)
                {
                    float Begin = buffer.GetTimeStamp("TimeStamp_BeginCircle");
                    presence.DepthTransport.CurrentTime = Begin;
                    Log("LOOPING CIRCLE CLONE " + name);
                }


            }


        }

        void PlaybackStop(StoryTask task, string name, Presence presence, float time)
        {
            // See if any of the clones has reached their out point.

            if (name.IndexOf("playbackpresence") == -1)
                return;

            FileformatBase buffer = presence.DepthTransport.TransCoder.GetBufferFile();

            if (buffer != null)
            {
                float EndTime = -1;
                task.GetFloatValue(name + "_outpoint", out EndTime);

                if (EndTime != -1 && time >= EndTime)
                {

                    Log("Killing playback" + name);
                    Destroy(presence.gameObject);
                    SETTINGS.Presences.Remove(name);

                }


            }


        }

        void PresenceClone(StoryTask task, string name, Presence presence, float time)
        {
            // See if any of the clones has reached their out point.

            if (name.IndexOf("presenceclone") == -1)
                return;

            FileformatBase buffer = presence.DepthTransport.TransCoder.GetBufferFile();

            if (buffer != null)
            {
                float EndTime = -1;
                task.GetFloatValue(name + "_outpoint", out EndTime);

                if (EndTime != -1 && time >= EndTime)
                {

                    Log("Killing clone" + name);
                    Destroy(presence.gameObject);
                    SETTINGS.Presences.Remove(name);

                }


            }


        }

        void ShatterHandler(StoryTask task, string name, Presence presence, float time)
        {
            // See if any of the clones has reached their out point.

            if (name.IndexOf("shatterclone") == -1)
                return;

            FileformatBase buffer = presence.DepthTransport.TransCoder.GetBufferFile();

            if (buffer != null)
            {

                float speed;
                task.GetFloatValue(name + "_speed", out speed);
                float EndTime;
                task.GetFloatValue(name + "_outpoint", out EndTime);


                if (speed > 0 ? time >= EndTime : time <= EndTime)
                {
                    Log("Killing clone" + name);
                    Destroy(presence.gameObject);
                    SETTINGS.Presences.Remove(name);

                }



            }


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