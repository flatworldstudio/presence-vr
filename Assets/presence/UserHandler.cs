using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StoryEngine;
//using GoogleVR.Demos;
//using NUnit.Framework.Constraints;
//using Microsoft.Win32.SafeHandles;

namespace PresenceEngine
{

    public class UserHandler : MonoBehaviour
    {
        public UserController userController;

        UxInterface serverInterface;

        public GameObject uxCanvas;

        public GameObject viewerRoot, viewerOffset, viewerCamera;


        public GameObject overviewObject, projectionObject, headSet, setObject, handl, handr, body, Kinect, SetHandler, startPosition;
        //float timer = 0;

        //float heading, lastHeading, smoothOffset, northOffset;

        public AudioSource signalSound;

        //public GameObject AutoCallibrateObject;

        //public CalibrateOnMarker CalibrationScript;

        //   public GestureDetection GestureDetection;

        public GameObject NewFile;
        public UnityEngine.UI.InputField fileNameInput;

        //public DemoInputManager GoogleVRInputManager;
        public UserMessager userMessager;

        //	public 

        string me = "Task handler: ";

        UiConstraint fileBrowserConstraint;

        UxController uxController;

        public Text filePath;

        void Start()
        {


            uxCanvas.SetActive(true);

            userController.addTaskHandler(TaskHandler);

            uxController = new UxController();

            fileBrowserConstraint = new UiConstraint();

            float width = 1280;

            fileBrowserConstraint.hardClamp = true;
            fileBrowserConstraint.hardClampMin = new Vector3(-width - 100, 0);
            fileBrowserConstraint.hardClampMax = new Vector3(width + 100, 0);

            fileBrowserConstraint.springs = true;
            fileBrowserConstraint.springPositions = new Vector2[3];
            fileBrowserConstraint.springPositions[0] = new Vector2(-width, 0); // files.
            fileBrowserConstraint.springPositions[1] = new Vector2(0, 0); // folders.
            fileBrowserConstraint.springPositions[2] = new Vector2(width, 0); // offscreen



        }



        public bool TaskHandler(StoryTask task)
        {

            bool done = false;

            switch (task.description)
            {

                // -----------------------------------------------------------------------
                // Main roles.

                case "amvrclient":

                    SETTINGS.ActiveCamera = viewerOffset.GetComponentInChildren<Camera>();

                    done = true;
                    break;

                case "amserver":

                    SETTINGS.ActiveCamera = overviewObject.GetComponentInChildren<Camera>();

                    done = true;
                    break;

                // -----------------------------------------------------------------------
                // Gestures


                case "WaitforSeated":

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        string status;

                        if (!task.GetStringValue("status", out status))
                        {

                            GestureDetection.Instance.BeginDetect(task, KinectGestures.Gestures.Centerseat);
                            task.SetStringValue("status", "detecting");

                        }

                        if (status == "detected")
                        {
                            task.SetStringValue("status", "detected");
                            userMessager.ShowTextMessage("Pose detected", 1);

                            GestureDetection.Instance.EndDetect();
                            done = true;
                        }

                    }

                    break;

                case "WaitforGetup":

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        string status;

                        if (!task.GetStringValue("status", out status))
                        {

                            GestureDetection.Instance.BeginDetect(task, KinectGestures.Gestures.Getup);
                            task.SetStringValue("status", "detecting");

                        }

                        if (status == "detected")
                        {
                            task.SetStringValue("status", "detected");
                            userMessager.ShowTextMessage("Pose detected", 1);

                            GestureDetection.Instance.EndDetect();
                            done = true;
                        }

                    }

                    break;

                case "WaitforRaisedhands":

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        string status;

                        if (!task.GetStringValue("status", out status))
                        {

                            GestureDetection.Instance.BeginDetect(task, KinectGestures.Gestures.HandsFolded);
                            task.SetStringValue("status", "detecting");

                        }

                        if (status == "detected")
                        {
                            task.SetStringValue("status", "detected");
                            userMessager.ShowTextMessage("Pose detected", 1);

                            GestureDetection.Instance.EndDetect();
                            done = true;
                        }

                    }

                    break;

                case "waitforgesture":

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        string status;

                        if (!task.GetStringValue("status", out status))
                        {

                            GestureDetection.Instance.BeginDetect(task, KinectGestures.Gestures.Tpose);

                            //     GestureTpose gestureSeated = GestureObject.AddComponent<GestureTpose>();
                            //        gestureSeated.BeginDetect(task);
                            //                            Davinci.BeginDetect(task);
                            task.SetStringValue("status", "detecting");

                        }


                        if (status == "detected")
                        {
                            task.SetStringValue("status", "detecting");
                            userMessager.ShowTextMessage("Pose detected", 1);
                            //  GestureObject.GetComponent<GestureTpose>().EndDetect();
                            GestureDetection.Instance.EndDetect();
                            //     GestureObject.GetComponent<GestureTpose>().EndDetect();
                            //   Destroy(GestureObject.GetComponent<GestureTpose>());
                            //   Davinci.EndDetect();

                            done = true;
                        }


                    }

                    if (SETTINGS.deviceMode == DEVICEMODE.VRCLIENT)
                    {


                        string status;

                        task.GetStringValue("status", out status);

                        if (status == "detected")
                        {
                            // task.SetStringValue("status", "detecting");
                            if (!signalSound.isPlaying)
                                signalSound.Play();

                            done = true;
                        }

                    }




                    break;


                // -----------------------------------------------------------------------
                    // Manipulating pov

                case "OffsetThirdperson":

                    // viewercamera is vr headset. interest is user for position only. object is user for corrections and manipulations.

                    viewerOffset.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f) * SETTINGS.HeadsetCorrection;
                    viewerOffset.transform.localPosition = new Vector3(0, 2, 0);


                    done = true;
                    break;

                case "OffsetReset":

                    viewerOffset.transform.localRotation = SETTINGS.HeadsetCorrection * Quaternion.identity;
                    viewerOffset.transform.localPosition = Vector3.zero;


                    done = true;
                    break;

                // -----------------------------------------------------------------------

                // Connectivity messages.

                case "listenforserver":

                    userMessager.ShowTextMessage("Waiting for server", 1);

                    done = true;
                    break;


                case "lostconnection":

                    userMessager.ShowTextMessage("Lost server connection", 1);
                    signalSound.Play();

                    done = true;
                    break;

                case "startclient":

                    userMessager.ShowTextMessage("Connected to server", 1);
                    signalSound.Play();

                    done = true;
                    break;


                    // -----------------------------------------------------------------------
                    // Flow messages & alerts.


                case "playsignal":

                    signalSound.Play();

                    done = true;

                    break;


                case "depthlive":

                    userMessager.ShowTextMessage("Streaming depth", 1);
                    serverInterface.HideButton("startpresence");
                    serverInterface.ShowButton("stoppresence");

                    done = true;
                    break;

                case "depthoff":

                    userMessager.ShowTextMessage("Streaming depth off", 1);
                    serverInterface.HideButton("stoppresence");
                    serverInterface.ShowButton("startpresence");

                    done = true;
                    break;


                case "pressedrecordstart":

                    userMessager.ShowTextMessage("Begin recording", 0.5f);

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        serverInterface.HideButton("recordstart");
                        serverInterface.ShowButton("recordstop");
                    }
                    done = true;
                    break;

                case "pressedrecordstop":

                    userMessager.ShowTextMessage("Stop recording", 0.5f);
                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        serverInterface.HideButton("recordstop");
                        serverInterface.ShowButton("recordstart");
                    }

                    done = true;
                    break;

                case "pressedplay":

                    userMessager.ShowTextMessage("Begin playback", 0.5f);
                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        serverInterface.HideButton("playbackstart");
                        serverInterface.ShowButton("playbackstop");
                    }

                    done = true;
                    break;

                case "pressedstop":

                    userMessager.ShowTextMessage("Stop playback", 0.5f);
                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        serverInterface.HideButton("playbackstop");
                        serverInterface.ShowButton("playbackstart");
                    }

                    done = true;
                    break;


                /*
            case "waitforuser":

                // Detect user start position
                ///	GameObject.Find("startposition");


                //	Vector3 startPos3d = startPos.transform.position;
                Vector2 startPos = new Vector2(startPosition.transform.position.x, startPosition.transform.position.z);
                Vector2 userPos = new Vector2(viewerObject.transform.parent.transform.position.x, viewerObject.transform.parent.transform.position.z);

                float delta = Vector2.Distance(startPos, userPos);

                if (delta < 1f)
                {

                    timer += Time.deltaTime;

                    if (timer > 2f)
                    {

                        //	PRESENCE.capture = new CloudSequence (PRESENCE.captureLength);
                        //	PRESENCE.CaptureFrame = 0;
                        //	GENERAL.GLOBALS.setIntValue ("setcaptureframe", 0);

                        //		PRESENCE.TimeStamp = Time.time;

                        done = true;

                    }

                }
                else
                {

                    timer = 0f;

                }

                task.setStringValue("debug", "" + timer);

                break;
                */



#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

                case "waitforuser":


                    if (DepthTransport.OwnsKinect != null && DepthTransport.OwnsKinect.Mode == DEPTHMODE.LIVE)
                    {

                        if (DepthTransport.OwnsKinect.IsUserDetected())
                        {
                            userMessager.ShowTextMessage("User detected", 3);
                            done = true;

                        }


                    }
                    else
                    {

                        // we're probably simulating so just fall through
                        done = true;

                    }




                    break;
#endif




                    // -----------------------------------------------------------------------
                    // Live

                case "userstream":

                    // Applying values to user objects. Data transfer done in data handler.


                    if (SETTINGS.user == null)
                    {
                        Debug.LogWarning("No user object registered.");
                        done = true;
                        break;
                    }


                    UncompressedFrame ShowFrame = SETTINGS.user.DepthTransport.ActiveFrame;

                    if (ShowFrame != null && ShowFrame.Joints != null && ShowFrame.Tracked[(int)KinectWrapper.NuiSkeletonPositionIndex.Head])
                    {
                        // apply user head position to camera on both server and client
                        //    viewerObject.transform.parent.transform.position = ShowFrame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head] + SETTINGS.ViewerPositionOffset;

                        viewerRoot.transform.position = ShowFrame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head];

                    }

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {

                        // apply user head rotation and local position from device. this has calibration applied.

                        Quaternion ho;

                        if (task.GetQuaternionValue("user_headrotation", out ho))
                        {

                            viewerCamera.transform.localRotation = ho;


                        }

                        Vector3 hp;

                        if (task.GetVector3Value("user_headposition", out hp))
                        {

                            viewerCamera.transform.localPosition = hp;

                        }



                    }



                    break;

                    // -----------------------------------------------------------------------
                    // User interface scripts.

                case "servercontrol":

                    UserCallBack serverCallback = uxController.updateUx(serverInterface);

                    if (serverCallback.trigger)
                    {

                        if (serverCallback.sender != null)
                        {
                            char delimiter = '#';
                            string[] labelSplit = serverCallback.sender.name.Split(delimiter);


                            if (labelSplit.Length == 2)
                            {

                                int Index = int.Parse(labelSplit[1]);

                                task.SetStringValue("persistantData", "" + Index);

                            }
                        }
                        task.setCallBack(serverCallback.label);

                    }

                    filePath.text = IO.CheckedOutFile;

                    break;

                case "keyboardcontrol":

                    if (Input.GetKeyDown("m"))
                    {
                        task.setCallBack("togglemanualplayback");

                    }

                    if (Input.GetKeyDown("g"))
                    {
                        task.setCallBack("grabframe");

                    }


                    if (Input.GetKey("p"))
                    {
                        if (SETTINGS.ManualPlayback)
                            task.setCallBack("previousframe");

                    }


                    if (Input.GetKey("n"))
                    {
                        if (SETTINGS.ManualPlayback)
                            task.setCallBack("nextframe");

                    }



                    break;

                case "makeservercontrols":


                    serverInterface = new UxInterface();

                    UxMapping serverMapping = new UxMapping();

                    serverMapping.ux_none += UxMethods.none;

                    serverMapping.ux_tap_2d += UxMethods.highlightButton2d;
                    serverMapping.ux_tap_3d += UxMethods.none;
                    serverMapping.ux_tap_none += UxMethods.tapNone;

                    serverMapping.ux_single_2d += UxMethods.drag2d;
                    serverMapping.ux_single_3d += UxMethods.rotateCamera;
                    serverMapping.ux_single_none += UxMethods.rotateCamera;

                    serverMapping.ux_double_2d += UxMethods.drag2d;
                    serverMapping.ux_double_3d += UxMethods.panCamera;

                    serverMapping.ux_double_3d += UxMethods.zoomCamera;

                    serverMapping.ux_double_none += UxMethods.panCamera;
                    serverMapping.ux_double_none += UxMethods.zoomCamera;

                    serverInterface.defaultUxMap = serverMapping;

                    serverInterface.camera = new UxCamera(overviewObject);
                    serverInterface.camera.control = CAMERACONTROL.ORBIT;

                    serverInterface.camera.constraint = new UiConstraint();
                    serverInterface.camera.constraint.pitchClamp = true;
                    serverInterface.camera.constraint.pitchClampMin = 5f;
                    serverInterface.camera.constraint.pitchClampMax = 85f;

                    serverInterface.canvasObject = uxCanvas;
                    serverInterface.tapNoneCallback = "screentap";

                    UiConstraint constraint = new UiConstraint();

                    constraint.hardClamp = true;
                    constraint.hardClampMin = new Vector3(0, -250);
                    constraint.hardClampMax = new Vector3(0, -250);

                    GameObject menu = GameObject.Find("servermenu");

                    UiButton control = new UiButton("startpresence", menu, constraint);
                    control.callback = "startpresence";
                    serverInterface.addButton(control);

                    control = new UiButton("stoppresence", menu, constraint);
                    control.callback = "stoppresence";
                    serverInterface.addButton(control);

                    control = new UiButton("newfile", menu, constraint);
                    control.callback = "newfile";
                    serverInterface.addButton(control);

                    control = new UiButton("browser", menu, constraint);
                    control.callback = "togglebrowser";
                    serverInterface.addButton(control);

                    control = new UiButton("newfolder", menu, constraint);
                    control.callback = "newfolder";
                    serverInterface.addButton(control);

                    // Flows

                    control = new UiButton("flow01", menu, constraint);
                    control.callback = "flow_solo";
                    serverInterface.addButton(control);

                    control = new UiButton("flow02", menu, constraint);
                    control.callback = "flow_mirror";
                    serverInterface.addButton(control);

                    control = new UiButton("flow03", menu, constraint);
                    control.callback = "flow_delay";
                    serverInterface.addButton(control);

                    control = new UiButton("flow04", menu, constraint);
                    control.callback = "flow_echo";
                    serverInterface.addButton(control);

                    control = new UiButton("flow05", menu, constraint);
                    control.callback = "flow_guided";
                    serverInterface.addButton(control);

                    // Callbacks for play, record and stop are different for different flows. These are the defaults.

                    control = new UiButton("playbackstart", menu, constraint);
                    control.callback = "";
                    serverInterface.addButton(control);

                    control = new UiButton("playbackstop", menu, constraint);
                    control.callback = "";
                    serverInterface.addButton(control);

                    control = new UiButton("recordstart", menu, constraint);
                    control.callback = "";
                    serverInterface.addButton(control);

                    control = new UiButton("recordstop", menu, constraint);
                    control.callback = "";
                    serverInterface.addButton(control);

                    // Hide buttons.

                    serverInterface.HideButton("playbackstop");
                    serverInterface.HideButton("recordstop");
                    serverInterface.HideButton("stoppresence");

                    NewFile.transform.localScale = Vector3.zero;


                    done = true;

                    break;

                case "makefoldermenu":

                    if (serverInterface == null)
                    {
                        done = true;
                        break;
                    }

                    GameObject browser = GameObject.Find("FileBrowser");
                    GameObject FolderMenu = GameObject.Find("Folders");


                    PFolder[] folders = IO.GetLocalFolders();

                    //    if (folders.Length > 0)
                    //      IO.CheckedOutFolder = folders[0].LocalPath;

                    for (int i = 0; i < 18; i++)
                    {

                        GameObject icon = FolderMenu.transform.Find("folder#" + i).gameObject;


                        UiButton folderButton = new UiButton("folder#" + i, browser, fileBrowserConstraint);
                        folderButton.callback = "folder";
                        serverInterface.addButton(folderButton);

                        if (i < folders.Length)
                        {

                            icon.GetComponentInChildren<UnityEngine.UI.Text>().text = folders[i].Name;
                            icon.transform.localScale = Vector3.one;

                        }
                        else
                        {
                            icon.transform.localScale = Vector3.zero;
                        }

                    }

                    // Now hide it

                    UiButton target;
                    if (serverInterface.uiButtons.TryGetValue("folder#0", out target))
                    {
                        uxController.setSpringTarget(target, 2);
                    }

                    done = true;
                    break;

                case "setfiledefaults":


                    IO.SelectFile(SETTINGS.DEFAULTFILE);
                    IO.SelectFolder(SETTINGS.DEFAULTFOLDER);

                    //Debug.Log(IO.CheckedOutFile);
                    //Debug.Log(IO.BrowseFolder);

                    done = true;
                    break;

                case "makefilemenu":

                    if (SETTINGS.deviceMode != DEVICEMODE.SERVER || serverInterface == null)
                    {

                        done = true;
                        break;

                    }

                    //MakeBrowserConstraint();

                    browser = GameObject.Find("FileBrowser");
                    GameObject FileMenu = GameObject.Find("Files");

                    Vector3 position = FileMenu.transform.localPosition;
                    position.x = Screen.width;
                    FileMenu.transform.localPosition = position;

                    //List <PFile> files = IO.GetFileList(IO.SelectedFolder);


                    List<PFile> files = IO.FilesInSelectedFolder;


                    for (int i = 0; i < 18; i++)
                    {

                        GameObject icon = FileMenu.transform.Find("file#" + i).gameObject;


                        UiButton folderButton = new UiButton("file#" + i, browser, fileBrowserConstraint);
                        folderButton.callback = "file";
                        serverInterface.addButton(folderButton);

                        if (i < files.Count)
                        {

                            icon.GetComponentInChildren<UnityEngine.UI.Text>().text = files[i].Name;
                            icon.transform.localScale = Vector3.one;

                        }
                        else
                        {
                            icon.transform.localScale = Vector3.zero;
                        }

                    }


                    done = true;
                    break;

                

                case "setflow_solo":

                    userMessager.ShowTextMessage("Flow: Solo", 1);

                    UiButton b = serverInterface.GetButton("playbackstart");
                    b.callback = "playsolo";

                    b = serverInterface.GetButton("playbackstop");
                    b.callback = "stopplaysolo";

                    b = serverInterface.GetButton("recordstart");
                    b.callback = "recordsolo";

                    b = serverInterface.GetButton("recordstop");
                    b.callback = "stoprecordsolo";

                    done = true;
                    break;

                case "setflow_mirror":

                    userMessager.ShowTextMessage("Flow: Mirror", 1);

                    b = serverInterface.GetButton("playbackstart");
                    b.callback = "playmirror";

                    b = serverInterface.GetButton("playbackstop");
                    b.callback = "stopplaymirror";

                    b = serverInterface.GetButton("recordstart");
                    b.callback = "recordmirror";

                    b = serverInterface.GetButton("recordstop");
                    b.callback = "stoprecordmirror";

                    done = true;
                    break;


             
                case "setflow_delay":


                    userMessager.ShowTextMessage("Flow: Delay", 1);

                    b = serverInterface.GetButton("playbackstart");
                    b.callback = "playdelay";

                    b = serverInterface.GetButton("playbackstop");
                    b.callback = "stopplaydelay";

                    b = serverInterface.GetButton("recordstart");
                    b.callback = "recorddelay";

                    b = serverInterface.GetButton("recordstop");
                    b.callback = "stoprecorddelay";


                    done = true;
                    break;

                case "setflow_echo":

                    userMessager.ShowTextMessage("Flow: Echo", 1);

                    b = serverInterface.GetButton("playbackstart");
                    b.callback = "playecho";

                    b = serverInterface.GetButton("playbackstop");
                    b.callback = "stopplayecho";

                    b = serverInterface.GetButton("recordstart");
                    b.callback = "recordecho";

                    b = serverInterface.GetButton("recordstop");
                    b.callback = "stoprecordecho";


                    done = true;
                    break;

                case "setflow_guided":

                    userMessager.ShowTextMessage("Flow: Guided", 1);

                    b = serverInterface.GetButton("playbackstart");
                    b.callback = "playguided";

                    b = serverInterface.GetButton("playbackstop");
                    b.callback = "stopplayguided";

                    b = serverInterface.GetButton("recordstart");
                    b.callback = "recordguided";

                    b = serverInterface.GetButton("recordstop");
                    b.callback = "stoprecordguided";


                    done = true;
                    break;

              

                case "togglebrowser":

                    //    UiButton target;

                    if (serverInterface.uiButtons.TryGetValue("folder#0", out target))
                    {
                        //MakeBrowserConstraint();

                        uxController.setSpringTarget(target, 1);

                        //if (target.gameObject.GetComponent<RectTransform>().position.x > Screen.width-250-10)
                        //{

                        //    uxController.setSpringTarget(target, 1);

                        //}
                        //else
                        //{
                        //    uxController.setSpringTarget(target, 2);
                        //}

                    }


                    done = true;
                    break;


                case "setfolder":

                    // UiButton target;

                    if (serverInterface.uiButtons.TryGetValue("folder#0", out target))
                        uxController.setSpringTarget(target, 0);



                    string data;
                    task.GetStringValue("persistantData", out data);

                    //Debug.Log("pers " + data);

                    IO.SelectFolder(IO.GetLocalFolders()[int.Parse(data)].Path);

                    done = true;
                    break;

                case "setfile":

                    task.GetStringValue("persistantData", out data);

                    IO.SelectFile(IO.FilesInSelectedFolder[int.Parse(data)].Path);

                    done = true;

                    break;


                case "makenewfile":

                    string firstrun;

                    if (!task.GetStringValue("firstrun", out firstrun))
                    {

                        task.SetStringValue("firstrun", "done");

                        NewFile.transform.localScale = Vector3.one;
                        fileNameInput.onEndEdit.RemoveAllListeners();
                        fileNameInput.onEndEdit.AddListener((name) =>
                        {
                            IO.MakeNewFile(IO.SelectedFolder + "/" + name + ".prs");

                            NewFile.transform.localScale = Vector3.zero;
                            if (serverInterface.uiButtons.TryGetValue("folder#0", out target))
                                uxController.setSpringTarget(target, 0);
                            task.ForceComplete();
                        });

                        // We pass a callback function that will complete the task when called. So we keep the task open here.

                    }


                    break;

                case "makenewfolder":

                    //  NewFile.transform.localScale = Vector3.one;


                    if (!task.GetStringValue("firstrun", out firstrun))
                    {
                        task.SetStringValue("firstrun", "done");

                        NewFile.transform.localScale = Vector3.one;
                        fileNameInput.onEndEdit.RemoveAllListeners();
                        fileNameInput.onEndEdit.AddListener((name) =>
                        {
                            IO.MakeNewFolder("/" + name);
                            NewFile.transform.localScale = Vector3.zero;
                            if (serverInterface.uiButtons.TryGetValue("folder#0", out target))
                                uxController.setSpringTarget(target, 0);
                            task.ForceComplete();
                        });
                    }


                    //  done = true;

                    break;

             

                case "toggleview":

                    if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL)
                    {

                        // only execute on server. we'll cycle through.

                        Camera viewCam = overviewObject.GetComponentInChildren<Camera>();
                        Camera userCam = viewerOffset.GetComponentInChildren<Camera>();

                        int t = viewCam.targetDisplay;

                        viewCam.targetDisplay = userCam.targetDisplay;
                        userCam.targetDisplay = t;

                        //  SETTINGS.ActiveCamera = t==1? viewCam:userCam;

                        SETTINGS.ActiveCamera = t == 1 ? userCam : viewCam;


                    }

                    done = true;

                    break;

                case "createoverviewdebug":

                    GameObject newNullObject = DebugObject.getNullObject(0.1f, 0.1f, 0.2f);
                    newNullObject.transform.parent = GameObject.Find("overviewObject").transform;
                    newNullObject.transform.localPosition = Vector3.zero;
                    newNullObject.transform.localRotation = Quaternion.identity;

                    newNullObject = DebugObject.getNullObject(0.05f, 0.05f, 0.1f);
                    newNullObject.transform.parent = GameObject.Find("overviewInterest").transform;
                    newNullObject.transform.localPosition = Vector3.zero;
                    newNullObject.transform.localRotation = Quaternion.identity;



                    done = true;

                    break;


                case "calibrateheadset":

                    userMessager.ShowTextMessage("Calibrating", 1);

                    if (SETTINGS.deviceMode == DEVICEMODE.VRCLIENT)
                    {

                        // headset may have drifted. 
                        // we set the root object to neutralise whatever the headsets yaw is

                        // rotate the headset towards the kinect.


                        // headset is (locally) rotated at an angle of

                        Vector3 euler = headSet.transform.localRotation.eulerAngles;

                        float headYaw = euler.y;

                        Debug.Log("headYaw: " + headYaw);

                        // which leaves a delta of

                        viewerOffset.transform.rotation = Quaternion.Euler(0, 180 - headYaw, 0);

                        SETTINGS.HeadsetCorrection = Quaternion.Euler(0, 180 - headYaw, 0);



                    }

                    done = true;

                    break;




                default:

                    done = true;

                    break;

            }

            return done;

        }


        void Update()
        {

        }

    }
}