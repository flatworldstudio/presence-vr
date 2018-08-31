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
        UxInterface overviewInterface, viewInterface, serverInterface;

        public GameObject uxCanvas;

        public GameObject viewerRoot, viewerOffset, viewerCamera;


        public GameObject overviewObject,   projectionObject, headSet, setObject, handl, handr, body, Kinect, SetHandler, startPosition;
        //	UxMapping overviewMap;
        float timer = 0;

        float heading, lastHeading, smoothOffset, northOffset;

        public AudioSource signalSound;

        public GameObject AutoCallibrateObject;

        public CalibrateOnMarker CalibrationScript;

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

            //SETTINGS.ViewerPositionOffset = Vector3.zero;
            //SETTINGS.ViewerOrientationOffset = Quaternion.Euler(0, 0, 0);


            fileBrowserConstraint.hardClamp = true;
            fileBrowserConstraint.hardClampMin = new Vector3(-width - 100, 0);
            fileBrowserConstraint.hardClampMax = new Vector3(width + 100, 0);

            fileBrowserConstraint.springs = true;
            fileBrowserConstraint.springPositions = new Vector2[3];
            fileBrowserConstraint.springPositions[0] = new Vector2(-width, 0); // files.
            fileBrowserConstraint.springPositions[1] = new Vector2(0, 0); // folders.
            fileBrowserConstraint.springPositions[2] = new Vector2(width, 0); // offscreen


       //     SETTINGS.ActiveCamera = overviewObject.GetComponentInChildren<Camera>();


            //MakeBrowserConstraint();

            //float width = Screen.width;
            //Display[0].widtj;
            //float width =  
            //Display.displays[0].

            //Display.displays[1].Activate();

            //float width  =  Display.main.renderingWidth;

            //fileBrowserConstraint.hardClamp = true;
            //fileBrowserConstraint.hardClampMin = new Vector3(-width - 100, 0);
            //fileBrowserConstraint.hardClampMax = new Vector3(width + 100, 0);

            //fileBrowserConstraint.springs = true;
            //fileBrowserConstraint.springPositions = new Vector2[3];
            //fileBrowserConstraint.springPositions[0] = new Vector2(-width, 0); // files.
            //fileBrowserConstraint.springPositions[1] = new Vector2(0, 0); // folders.
            //fileBrowserConstraint.springPositions[2] = new Vector2(width, 0); // offscreen




#if UNITY_IOS

            // Callibration: rotate headset so that north is always north.

            Input.compass.enabled = true;
            Input.compensateSensors = false;

            //		PRESENCE.mobileInitialHeading1 = Input.compass.magneticHeading;
            //		PRESENCE.mobileInitialHeading = Input.compass.magneticHeading;

            //		viewerObject.transform.parent.transform.localRotation = Quaternion.Euler (0, -1f* PRESENCE.mobileInitialHeading, 0);


#endif






            //		viewerInterface.camera.cameraReference.SetActive (false);

            /*
            overviewInterface.camera.cameraReference.SetActive (true);


            UiConstraint simpleConstraint = new UiConstraint ();

            simpleConstraint.edgeSprings = true;
            simpleConstraint.edgeSpringMin = new Vector3 (0, 0);
            simpleConstraint.edgeSpringMax = new Vector3 (0, 0);

            simpleConstraint.hardClamp = true;
            simpleConstraint.hardClampMin = new Vector3 (-100, -50);
            simpleConstraint.hardClampMax = new Vector3 (100, 50);

            GameObject menu = GameObject.Find ("menu");

            UiButton but = new UiButton ("record", menu, simpleConstraint);
            but.callback = "recorddepth";
            overviewInterface.uiButtons.Add ("record", but);
            viewerInterface.uiButtons.Add ("record", but);


            but = new UiButton ("stop", menu, simpleConstraint);
            but.callback = "stopdepth";
            overviewInterface.uiButtons.Add ("stop", but);
            viewerInterface.uiButtons.Add ("stop", but);


            but = new UiButton ("play", menu, simpleConstraint);
            but.callback = "playdepth";
            overviewInterface.uiButtons.Add ("play", but);
            viewerInterface.uiButtons.Add ("play", but);

    */


        }

        private static Quaternion GyroToUnity(Quaternion q)
        {

            return new Quaternion(q.x, q.y, -q.z, -q.w);
            //		return new Quaternion(q.y, -q.x, q.z, q.w);

        }

        //	static readonly Quaternion baseIdentity =  Quaternion.Euler(90, 0, 0);
        static readonly Quaternion landscapeLeft = Quaternion.Euler(0, 0, -90);
        static readonly Quaternion baseIdentity = Quaternion.Euler(90, 0, 0);

        public bool TaskHandler(StoryTask task)
        {

            bool done = false;

            switch (task.description)
            {
                //

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

#if DEV
                        if (Input.anyKeyDown)
                            task.SetStringValue("status", "detected");
#endif

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

#if DEV
                        if (Input.anyKeyDown)
                            task.SetStringValue("status", "detected");
#endif

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

#if DEV
                        if (Input.anyKeyDown)
                            task.SetStringValue("status", "detected");
#endif

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

#if DEV
                        if (Input.anyKeyDown)
                            task.SetStringValue("status", "detected");
#endif

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


                case "OffsetThirdperson":

                    // viewercamera is vr headset. interest is user for position only. object is user for corrections and manipulations.
                

                    viewerOffset.transform.localRotation = SETTINGS.HeadsetCorrection * Quaternion.Euler(-90f, 0f, 0f);
                    viewerOffset.transform.localPosition = new Vector3(0, 2, 0);

                    //SETTINGS.ViewerOrientationOffset = Quaternion.Euler(90f, 180f, 0f);
                    //SETTINGS.ViewerPositionOffset = new Vector3(0, 2, 0);


                    done = true;
                    break;

                case "OffsetReset":

                    viewerOffset.transform.localRotation = SETTINGS.HeadsetCorrection * Quaternion.identity;
                    viewerOffset.transform.localPosition = Vector3.zero;

               //     SETTINGS.ViewerOrientationOffset = Quaternion.identity;
                 //   SETTINGS.ViewerPositionOffset = Vector3.zero;


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

                // Flow messages



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
                case "sessiontimer":


                    if (SETTINGS.CaptureFrame == SETTINGS.SessionDuration)
                    {

                        done = true;
                    }


                    break;

                //case "materialiseon":

                //presenceSound.Play();

                //done=true;
                //break;

                case "playsignal":

                    signalSound.Play();

                    done = true;

                    break;


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




                /*
            case "detectgesture":




                if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                {
                    string status;

                    if (!task.GetStringValue("status", out status))
                    {

                        Davinci.BeginDetect(task);
                        task.SetStringValue("status", "detecting");

                    }

#if DEV
                    if (Input.anyKeyDown)
                        task.SetStringValue("status", "detected");
#endif

                    if (status == "detected")
                    {
                        task.SetStringValue("status", "detecting");
                        userMessager.ShowTextMessage("Pose detected", 1);
                    }


                }

                if (SETTINGS.deviceMode == DEVICEMODE.VRCLIENT)
                {


                    string status;

                    task.GetStringValue("status", out status);

                    if (status == "detected")
                    {
                        task.SetStringValue("status", "detecting");
                        signalSound.Play();
                    }

                }



                //    uint playerID2 = KinectManager.Instance != null ? KinectManager.Instance.GetPlayer1ID() : 0;

                //     if (playerID2 > 0)
                //      {
                //          KinectManager.Instance.DetectGesture(playerID2, KinectGestures.Gestures.RaiseLeftHand);

                //    done = true;
                //      }



                break;
                */

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

                        viewerRoot.transform.position = ShowFrame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head] ;
                        
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

             
                    //UncompressedFrame ShowFrame = SETTINGS.user.DepthTransport.ActiveFrame;

                    //if (ShowFrame != null && ShowFrame.Joints != null && ShowFrame.Tracked[(int)KinectWrapper.NuiSkeletonPositionIndex.Head])
                    //{
                    //    viewerObject.transform.parent.transform.position = ShowFrame.Joints[(int)KinectWrapper.NuiSkeletonPositionIndex.Head];

                    //}
                    //else
                    //{
                    //    //Log.Error("Not a valid frame, can't set user position.");
                    //}

                    break;

                /*
            case "userstream":

                if (PRESENCE.deviceMode == DEVICEMODE.SERVER)
                {

                    // get head and hands position.

                    if (DepthTransport.OwnsKinect != null && DepthTransport.OwnsKinect.Mode == DEPTHMODE.LIVE)
                    {


                        viewerObject.transform.parent.transform.position = DepthTransport.OwnsKinect.getJoint(3); // head

                        handl.transform.position = DepthTransport.OwnsKinect.getJoint(7);
                        handr.transform.position = DepthTransport.OwnsKinect.getJoint(11);

                        body.transform.position = DepthTransport.OwnsKinect.getPosition();

                        // hacking

                        ///    KinectManager.Instance.DetectGesture()






                    }
                    else
                    {
                        // Get some random values for debugging.

                        float hx = 6f * Mathf.PerlinNoise(0.1f * Time.time, 0);
                        float hz = 6f * Mathf.PerlinNoise(0, 0.1f * Time.time);

                        viewerObject.transform.parent.transform.position = new Vector3(hx, PRESENCE.kinectHeight, hz);

                        handl.transform.position = new Vector3(hx - 0.5f, PRESENCE.kinectHeight / 2, hz);
                        handr.transform.position = new Vector3(hx + 0.5f, PRESENCE.kinectHeight / 2, hz);
                        body.transform.position = new Vector3(hx, PRESENCE.kinectHeight / 2, hz);


                        //Debug.Log("kinect not live");

                    }

                    // retrieve head orientation

                    Quaternion q;

                    if (task.getQuaternionValue("headrotation", out q))
                    {

                        headSet.transform.rotation = q;

                    }

                    // push head and hand position

                    task.setVector3Value("head", viewerObject.transform.parent.transform.position);
                    task.setVector3Value("lefthand", handl.transform.position);
                    task.setVector3Value("righthand", handr.transform.position);
                    task.setVector3Value("body", body.transform.position);

                }

                if (PRESENCE.deviceMode == DEVICEMODE.VRCLIENT)
                {

                    // put head rotation

                    task.setQuaternionValue("headrotation", headSet.transform.rotation);

                    // retrieve head and hand position

                    Vector3 head, lefthand, righthand, bodypos;

                    if (task.getVector3Value("head", out head))
                        viewerObject.transform.parent.transform.position = head;


                    if (task.getVector3Value("lefthand", out lefthand))
                        handl.transform.localPosition = lefthand;

                    if (task.getVector3Value("righthand", out righthand))
                        handr.transform.localPosition = righthand;

                    if (task.getVector3Value("body", out bodypos))
                        body.transform.localPosition = bodypos;

                }

                break;
                */

                case "makefoldermenu":

                    if (serverInterface == null)
                    {
                        done = true;
                        break;
                    }

                    GameObject browser = GameObject.Find("FileBrowser");
                    GameObject FolderMenu = GameObject.Find("Folders");


                    //MakeBrowserConstraint();

                    //fileBrowserConstraint = new UiConstraint();

                    //float width  =  Display.main.renderingWidth;

                    //fileBrowserConstraint.hardClamp = true;
                    //fileBrowserConstraint.hardClampMin = new Vector3(-width - 100, 0);
                    //fileBrowserConstraint.hardClampMax = new Vector3(width + 100, 0);

                    //fileBrowserConstraint.springs = true;
                    //fileBrowserConstraint.springPositions = new Vector2[3];
                    //fileBrowserConstraint.springPositions[0] = new Vector2(-width, 0); // files.
                    //fileBrowserConstraint.springPositions[1] = new Vector2(0, 0); // folders.
                    //fileBrowserConstraint.springPositions[2] = new Vector2(width, 0); // offscreen


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


                //                case "setflow_mirror":

                //                    /*
                //                     b= serverInterface.GetButton("playbackstart");
                //                    b.callback="playsingle";

                //                    b= serverInterface.GetButton("playbackstop");
                //                    b.callback="stopplaysingle";

                //                    b= serverInterface.GetButton("recordstart");
                //                    b.callback="recordsingle";

                //                    b= serverInterface.GetButton("recordstop");
                //                    b.callback="stoprecordsingle";
                //*/

                //                    done = true;
                //                    break;

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
                                // PFolder[] allFolders = IO.GetLocalFolders();

                                task.SetStringValue("persistantData", "" + Index);


                            }
                        }
                        task.setCallBack(serverCallback.label);



                    }


                    filePath.text = IO.CheckedOutFile;

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

                //case "newfileconfirm":

                //    NewFile.transform.localScale = Vector3.zero;
                //    fileNameInput.onEndEdit.RemoveAllListeners();
                ////    fileNameInput.onEndEdit.AddListener(SetNewFile);


                //    //string name = fileNameInput.text != "" ? fileNameInput.text : "_default";

                //    IO.checkedOutFile = IO.CheckedOutFolder + "/" + (fileNameInput.text != "" ? fileNameInput.text : "_default");

                //    Debug.Log("new file " + IO.checkedOutFile);

                //    done = true;

                //    break;

                case "createviewvr":

                    //			PRESENCE.isOverview = false;

                    viewInterface = new UxInterface();

                    UxMapping uxMap = new UxMapping();


                    uxMap.ux_none += UxMethods.none;


                    uxMap.ux_tap_2d += UxMethods.none;
                    uxMap.ux_tap_3d += UxMethods.none;
                    //			uxMap.ux_tap_none += UxMethods.clearSelectedObjects;
                    uxMap.ux_tap_none += UxMethods.tapNone;

                    uxMap.ux_single_2d += UxMethods.none;
                    uxMap.ux_single_3d += UxMethods.none;
                    uxMap.ux_single_none += UxMethods.none;

                    uxMap.ux_double_2d += UxMethods.none;
                    uxMap.ux_double_3d += UxMethods.none;
                    uxMap.ux_double_none += UxMethods.none;

                    viewInterface.defaultUxMap = uxMap;

                    viewInterface.camera = new UxCamera(viewerOffset);

                    viewInterface.camera.control = CAMERACONTROL.VOID;

                    viewInterface.camera.constraint = new UiConstraint();

                    viewInterface.canvasObject = uxCanvas;

                    viewInterface.tapNoneCallback = "calibrate";

                    done = true;
                    break;


                case "createview":

                    SETTINGS.isOverview = false;


                    viewInterface = new UxInterface();

                    uxMap = new UxMapping();



#if UNITY_IOS

                    uxMap.ux_none += UxMethods.rotateCamera;

                    uxMap.ux_tap_2d += UxMethods.highlightButton2d;
                    uxMap.ux_tap_3d += UxMethods.select3dObject;
                    uxMap.ux_tap_none += UxMethods.clearSelectedObjects;
                    uxMap.ux_tap_none += UxMethods.stopControls;

                    uxMap.ux_single_2d += UxMethods.drag2d;
                    uxMap.ux_single_3d += UxMethods.panCamera;
                    uxMap.ux_single_none += UxMethods.panCamera;

                    uxMap.ux_double_2d += UxMethods.drag2d;
                    uxMap.ux_double_3d += UxMethods.zoomCamera;
                    uxMap.ux_double_none += UxMethods.zoomCamera;

                    viewInterface.defaultUxMap = uxMap;

                    viewInterface.camera = new UxCamera(viewerOffset);
                    viewInterface.camera.control = CAMERACONTROL.GYRO;
                    viewInterface.camera.constraint = new UiConstraint();


#else

                    uxMap.ux_none += UxMethods.none;


                    uxMap.ux_tap_2d += UxMethods.highlightButton2d;
                    uxMap.ux_tap_3d += UxMethods.select3dObject;
                    uxMap.ux_tap_none += UxMethods.clearSelectedObjects;
                    uxMap.ux_tap_none += UxMethods.stopControls;

                    uxMap.ux_single_2d += UxMethods.drag2d;
                    uxMap.ux_single_3d += UxMethods.rotateCamera;
                    uxMap.ux_single_none += UxMethods.rotateCamera;

                    uxMap.ux_double_2d += UxMethods.drag2d;
                    uxMap.ux_double_3d += UxMethods.panCamera;
                    uxMap.ux_double_3d += UxMethods.zoomCamera;
                    uxMap.ux_double_none += UxMethods.panCamera;
                    uxMap.ux_double_none += UxMethods.zoomCamera;

                    viewInterface.defaultUxMap = uxMap;

                    viewInterface.camera = new UxCamera(viewerOffset);
                    viewInterface.camera.control = CAMERACONTROL.TURN;
                    viewInterface.camera.constraint = new UiConstraint();

#endif

                    viewInterface.canvasObject = uxCanvas;




                    done = true;

                    break;









                case "createoverview":


                    SETTINGS.isOverview = true;


                    overviewInterface = new UxInterface();



                    UxMapping overviewMap = new UxMapping();

                    overviewMap.ux_none += UxMethods.none;

                    overviewMap.ux_tap_2d += UxMethods.highlightButton2d;
                    overviewMap.ux_tap_3d += UxMethods.none;
                    overviewMap.ux_tap_none += UxMethods.tapNone;

                    overviewMap.ux_single_2d += UxMethods.drag2d;
                    overviewMap.ux_single_3d += UxMethods.rotateCamera;
                    overviewMap.ux_single_none += UxMethods.rotateCamera;

                    overviewMap.ux_double_2d += UxMethods.drag2d;
                    overviewMap.ux_double_3d += UxMethods.panCamera;

                    overviewMap.ux_double_3d += UxMethods.zoomCamera;

                    overviewMap.ux_double_none += UxMethods.panCamera;
                    overviewMap.ux_double_none += UxMethods.zoomCamera;

                    overviewInterface.defaultUxMap = overviewMap;

                    overviewInterface.camera = new UxCamera(overviewObject);
                    overviewInterface.camera.control = CAMERACONTROL.ORBIT;
                    overviewInterface.camera.constraint = new UiConstraint();
                    overviewInterface.camera.constraint.pitchClamp = true;
                    overviewInterface.camera.constraint.pitchClampMin = 1f;
                    overviewInterface.camera.constraint.pitchClampMax = 89f;

                    overviewInterface.canvasObject = uxCanvas;
                    overviewInterface.tapNoneCallback = "screentap";

                    /*
                    UiConstraint asc = new UiConstraint ();

                    asc.hardClamp = true;
                    asc.hardClampMin = new Vector3 (0, 0);
                    asc.hardClampMax = new Vector3 (0, 0);



                    GameObject g = GameObject.Find ("AllScreen");

                    UiButton AllScreen = new UiButton ("AllScreen", g, asc);
                    AllScreen.callback = "screentap";
                    overviewInterface.uiButtons.Add ("AllScreen", AllScreen);
        */
                    done = true;

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

                        //if (viewCam.targetDisplay == 1)
                        //{
                        //    if (userCam.targetDisplay == 2)
                        //    {
                        //        // switch to viewcam on 2, projectioncam on 1
                        //        viewCam.targetDisplay = 2;
                        //        userCam.targetDisplay = 0;
                        //        projectionCam.targetDisplay = 1;

                        //    }
                        //    else
                        //    {
                        //        // switch to viewcam on 2, projectioncam on 1
                        //        viewCam.targetDisplay = 2;
                        //        userCam.targetDisplay = 0;
                        //        projectionCam.targetDisplay = 1;

                        //    }


                        //}




                        //    if (userCam.targetDisplay == 2)
                        //{
                        //    // user view on 2nd

                        //    if (viewCam.targetDisplay == 1)
                        //    {
                        //        // overview on 1st
                        //        viewCam.targetDisplay =2;
                        //        userCam.targetDisplay = 0;
                        //        projectionCam.targetDisplay = 1;

                        //    }
                        //    else
                        //    {

                        //    }
                        //    viewCam.targetDisplay == 2;



                        //}


                        //    if (projectionCam.targetDisplay == 1)
                        //{
                        //    //	cam.enabled = !cam.enabled;

                        //    // were in overview mode, so switch to pov mode. 

                        //    userCam.targetDisplay = 1;
                        //    //	userCam.enabled = true;

                        //    projectionCam.targetDisplay = 2;


                        //    //		viewCam.enabled = true;
                        //    //	viewCam.targetDisplay = 0;


                        //}
                        //else
                        //{

                        //    userCam.targetDisplay = 2;

                        //    projectionCam.targetDisplay = 1;



                        //}







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


                case "createcallibrationdebug":

                    newNullObject = DebugObject.getNullObject(1f, 1f, 1f);
                    newNullObject.name = "northnull";
                    newNullObject.transform.parent = GameObject.Find("SetHandler").transform;
                    newNullObject.transform.localPosition = Vector3.zero;
                    newNullObject.transform.localRotation = Quaternion.identity;


                    newNullObject = DebugObject.getNullObject(1f, 1f, 1f);
                    newNullObject.name = "kinectnull";
                    newNullObject.transform.parent = GameObject.Find("Kinect").transform;
                    newNullObject.transform.localPosition = Vector3.zero;
                    newNullObject.transform.localRotation = Quaternion.identity;

                    newNullObject = DebugObject.getNullObject(1f, 1f, 1f);
                    newNullObject.name = "compassoffsetnull";
                    newNullObject.transform.parent = GameObject.Find("SetHandler").transform;
                    newNullObject.transform.localPosition = Vector3.zero;
                    newNullObject.transform.localRotation = Quaternion.identity;

                    newNullObject = DebugObject.getNullObject(1f, 1f, 1f);
                    newNullObject.name = "viewernull";
                    newNullObject.transform.parent = viewerOffset.transform;
                    newNullObject.transform.localPosition = Vector3.zero;
                    newNullObject.transform.localRotation = Quaternion.identity;


                    done = true;

                    break;




                //		case "overview":
                //
                //			uxController.update (overviewInterface);
                //
                //
                //
                //			break;



                case "view":

                    uxController.update(viewInterface);


                    break;

                case "syncviewer":

#if SERVER

			Quaternion deviceRotation;
			float mobileInitialHeading;

			if (task.getQuaternionValue("deviceRotation", out deviceRotation)){
				headSet.transform.localRotation = deviceRotation;

			}

			if (task.getFloatValue("mobileInitialHeading", out mobileInitialHeading)){
				viewerObject.transform.localRotation = Quaternion.Euler (0, Mathf.Rad2Deg * mobileInitialHeading, 0);
//			task.setStringValue("debug",""+PRESENCE.mobileInitialHeading);


			}





#endif


                    Quaternion headRotation;



                    //			#if IOS && !UNITY_EDITOR
                    //
                    //			headRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);
                    //
                    //			#endif
                    //
                    //
                    //			#if IOS && UNITY_EDITOR
                    //
                    ////			headRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);
                    //
                    //			headRotation = headSet.transform.rotation;
                    //
                    //
                    //			#endif

#if CLIENT

			headRotation = headSet.transform.rotation;

			task.setQuaternionValue("headRotation",headRotation);

//			task.setFloatValue ("mobileInitialHeading",PRESENCE.mobileInitialHeading);

			Vector3 dp;

			if (task.getVector3Value("devicePosition",out dp))
			{
				viewerObject.transform.localPosition=dp;
			}
				


//			task.setStringValue("debug",""+PRESENCE.mobileInitialHeading);

#endif




                    break;

                case "gyro":

                    Input.gyro.enabled = true;



                    Quaternion attitude = baseIdentity * GyroToUnity(Input.gyro.attitude);

                    Vector3 z = attitude * Vector3.forward;



                    float angle = Mathf.Atan2(z.x, z.z);


                    //			float gyroYaw = gyroAttitude.eulerAngles.y;


                    string gyroOn = Input.gyro.enabled ? "on " : "off ";



                    task.SetStringValue("debug", gyroOn + " a: " + angle + "z: " + z.ToString());





                    break;


                case "callibrateheadset":

                    //			Input.compass.enabled = true;
                    //
                    //			float compassYaw = Input.compass.magneticHeading;
                    //			string compassOn = Input.compass.enabled ? "on " : "off ";
                    //
                    //			viewerObject.transform.localRotation = Quaternion.Euler (0, Mathf.Rad2Deg * compassYaw);
                    //			task.setFloatValue("initialHeading"

                    done = true;

                    break;

#if UNITY_IOS

                case "compass":


                    //		Input.compass.enabled = true;


                    if (Input.compass.rawVector.magnitude != 0)
                    {

                        // wait for a reading.

                        SETTINGS.mobileInitialHeading = Input.compass.magneticHeading;

                        viewerOffset.transform.parent.transform.localRotation = Quaternion.Euler(0, SETTINGS.mobileInitialHeading, 0);

                        done = true;

                    }

                    //			float compassYaw=Input.compass.magneticHeading;
                    //			string compassOn = Input.compass.enabled ? "on " : "off ";
                    //
                    //			task.setStringValue ("debug", compassOn + compassYaw);



                    break;

#endif






                case "autocalibrate":



                    if (SETTINGS.deviceMode == DEVICEMODE.VRCLIENT)
                    {

#if DEV

                        userMessager.ShowTextMessage("Calibrated", 3);

                        GENERAL.UserCalibrated = true;
                        task.SetStringValue("status", "calibrated");
                        //  task.setCallBack("clientcalibrated");

                        done = true;


#else





                        if (!AutoCallibrateObject.activeSelf)
                        {
                            GENERAL.UserCalibrated = false;

                            //AutoCallibrateObject.SetActive(true);

                            CalibrationScript.StartCalibration(task);


                            userMessager.ShowTextMessage("Calibrating", 3);
                        }



                        if (AutoCallibrateObject.GetComponent<CalibrateOnMarker>().callibrated)
                        {

                            // rotate the headset towards the kinect.

                            // kinect as at an angle of

                            Vector3 kinectPosition = Kinect.transform.position - headSet.transform.position;

                            float kinectAtAngle = Mathf.Atan2(kinectPosition.x, kinectPosition.z) * Mathf.Rad2Deg;

                            Debug.Log("kinect: " + kinectAtAngle);

                            // headset is (locally) rotated at an angle of

                            Vector3 euler = headSet.transform.localRotation.eulerAngles;

                            float headYaw = euler.y;

                            Debug.Log("headYaw: " + headYaw);

                            // which leaves a delta of

                            viewerOffset.transform.parent.transform.rotation = Quaternion.Euler(0, kinectAtAngle - headYaw, 0);

                            //AutoCallibrateObject.SetActive(true);
                            signalSound.Play();


                            //AutoCallibrateObject.SetActive(false);

                            CalibrationScript.EndCalibration();

                            //userMessager.TextMessageOff();
                            userMessager.ShowTextMessage("Calibrated", 3);

                            GENERAL.UserCalibrated = true;
                            task.SetStringValue("status", "calibrated");

                            done = true;

                            //   task.setCallBack("clientcalibrated");

                            //AutoCallibrateObject.GetComponent<RawImageWebCamTexture>().callibrated=false;//repeat


                        }
#endif

                    }


                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {

                        // on the server we hold to keep the task alive IF there are clients.

                        if (!GENERAL.wasConnected)
                        {
                            done = true;
                            Debug.Log("no connected clients, skipping calibration");
                            break;
                        }

                        string value;

                        if (task.GetStringValue("status", out value))
                        {
                            if (value == "calibrated")
                            {

                                done = true;

                            }
                        }

                    }




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

                        SETTINGS.HeadsetCorrection= Quaternion.Euler(0, 180 - headYaw, 0);


                    }



                    done = true;

                    break;

                case "checkforcalibration":

                    string callBackName = uxController.update(viewInterface);

                    if (!callBackName.Equals(""))
                    {

                        task.setCallBack(callBackName);

                    }



                    break;






                case "overviewinterface":

                    if (SETTINGS.isOverview)
                    {

                        UserCallBack callBack = uxController.updateUx(overviewInterface);

                        if (callBack.trigger)
                        {
                            task.setCallBack(callBack.label);
                        }

                        //				callBackName = uxController.update (overviewInterface);

                        //				if (!callBackName.Equals ("")) {
                        //
                        //					task.setCallBack (callBackName);
                        //
                        //				}

                    }

                    break;



                case "interfaceactive":

                    if (SETTINGS.isOverview)
                    {

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

                        if (DepthTransport.OwnsKinect != null && DepthTransport.OwnsKinect.Mode == DEPTHMODE.LIVE)
                        {



                            //	KinectManager manager = DepthTransport.kinectManager;

                            uint playerID = KinectManager.Instance != null ? KinectManager.Instance.GetPlayer1ID() : 0;

                            if (playerID >= 0)
                            {

                                //    viewerObject.transform.parent.transform.position = DepthTransport.getJoint(playerID, 3); // head

                                //    handl.transform.position = DepthTransport.getJoint(playerID, 7);
                                //    handr.transform.position = DepthTransport.getJoint(playerID, 11);

                            }



                        }
#endif

                        // get

                        float comp;
                        float head;

                        if (task.GetFloatValue("compass", out comp) && task.GetFloatValue("headyaw", out head))
                        {

                            // values from mobile

                            float vel = 0;

                            float newOffset = comp - head + SETTINGS.north;

                            if (newOffset < 0)
                                newOffset += 360f;

                            SETTINGS.vrHeadOffset = Mathf.SmoothDamp(SETTINGS.vrHeadOffset, newOffset, ref vel, 0.1f);

                            viewerOffset.transform.parent.transform.localRotation = Quaternion.Euler(0, SETTINGS.vrHeadOffset, 0);

                            task.SetFloatValue("viewerYawOffset", SETTINGS.vrHeadOffset);

                            //	task.setStringValue ("debug", "c: " + comp);

                        }

                        Quaternion q;

                        if (task.GetQuaternionValue("headrotation", out q))
                        {

                            headSet.transform.rotation = q;

                        }

                        // put


                        task.SetVector3Value("viewerPosition", viewerOffset.transform.parent.transform.position);
                        task.SetVector3Value("hlPosition", handl.transform.position);
                        task.SetVector3Value("hrPosition", handr.transform.position);

                        // send once

                        //      task.setVector3Value("compassPosition", compass.transform.position);
                        //         task.setQuaternionValue("compassRotation", compass.transform.rotation);

                        task.SetVector3Value("kinectPosition", Kinect.transform.position);
                        task.SetQuaternionValue("kinectRotation", Kinect.transform.rotation);

                        task.SetVector3Value("setPosition", SetHandler.transform.position);
                        task.SetQuaternionValue("setRotation", SetHandler.transform.rotation);


                        //	}

                        callBackName = uxController.update(overviewInterface);

                        if (!callBackName.Equals(""))
                        {

                            task.setCallBack(callBackName);

                        }


                    }

                    if (!SETTINGS.isOverview)
                    {

                        // skipping the compass because of inaccuracy...
                        // perhaps use markers.

                        /*

                        Vector3 euler = headSet.transform.localRotation.eulerAngles;

                        float yaw = euler.y;

                        float pitch = euler.x <= 180 ? euler.x : euler.x - 360f;
                        float roll = euler.z <= 180 ? euler.z : euler.z - 360f;

                        heading = Input.compass.magneticHeading;

                        #if UNITY_EDITOR

                        heading =yaw; // in de editor we start with point of device north, so heading and yaw are always the same. in effect this is facing east.

                        #endif

                        float relativeNorth = 270f - heading;

                        lastHeading = heading;

                        if (pitch < 40 && pitch > -40 && roll < 40 && roll > -40) {


                             northOffset = relativeNorth + yaw;
                            compass.transform.localRotation = Quaternion.Euler (0, northOffset, 0);



                        }


                        float vel=0;

                        smoothOffset = Mathf.SmoothDampAngle (smoothOffset, northOffset, ref vel, 0.5f);

        //				viewerObject.transform.parent.transform.rotation= Quaternion.Euler (0, -northOffset, 0);



                        task.setStringValue ("debug", "N: " + Mathf.Round(relativeNorth)+ " D: "
                            + Mathf.Round(heading-lastHeading) 
                            +" P "+Mathf.Round(pitch)
                            +" R "+Mathf.Round(roll)
                            +" Y "+Mathf.Round(yaw)

                        ) ;

        */


                        /*





                        float pitch = euler.x >= 0 ? euler.x : euler.x + 360f;

                        if (pitch > -15 && pitch < 15) {

                            task.setFloatValue ("headyaw", euler.y);

                            float compassHeading = euler.y;


                            #if UNITY_IOS

                            if (Input.compass.enabled) {

                            compassHeading = Input.compass.magneticHeading;

                            }

                            #endif

                            task.setFloatValue ("compass", compassHeading);

                            task.setStringValue ("debug", "c: " + compassHeading + " h: " + euler.y + " d: " + (euler.y - compassHeading));


                        }

        */

                        task.SetQuaternionValue("headrotation", headSet.transform.rotation);


                        // get

                        Vector3 kp, sp, cp;
                        Quaternion kq, sq, cq;

                        if (task.GetVector3Value("kinectPosition", out kp) && task.GetVector3Value("setPosition", out sp)
                            && task.GetQuaternionValue("kinectRotation", out kq) && task.GetQuaternionValue("setRotation", out sq)
                            && task.GetVector3Value("compassPosition", out cp) && task.GetQuaternionValue("compassRotation", out cq))
                        {

                            // set all the time

                            Kinect.transform.position = kp;
                            Kinect.transform.rotation = kq;

                            SetHandler.transform.position = sp;
                            SetHandler.transform.rotation = sq;

                            //       compass.transform.position = sp;
                            //          compass.transform.rotation = sq;

                        }


                        float viewerYawOffset;

                        //				if (task.getFloatValue ("viewerYawOffset", out viewerYawOffset)) {
                        //
                        //					// we're letting server tell us the offset all the time. could localise.
                        //
                        //					viewerObject.transform.parent.transform.localRotation = Quaternion.Euler (0, viewerYawOffset, 0);
                        //
                        //				}
                        //
                        Vector3 viewerPositionV;

                        if (task.GetVector3Value("viewerPosition", out viewerPositionV))
                        {

                            viewerOffset.transform.parent.transform.position = viewerPositionV;

                        }

                        Vector3 hl;

                        if (task.GetVector3Value("hlPosition", out hl))
                        {

                            handl.transform.localPosition = hl;

                        }

                        Vector3 hr;

                        if (task.GetVector3Value("hrPosition", out hr))
                        {

                            handr.transform.localPosition = hr;

                        }

                        //				string callBackName = uxController.update (viewInterface);
                        //
                        //				if (!callBackName.Equals ("")) {
                        //
                        //					task.setCallBack (callBackName);
                        //
                        //				}

                    }


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


                case "interfaceactive2":






                    if (!SETTINGS.isOverview)
                    {

                        // get testrotation



                        // get overviewer

                        /*

                        Quaternion viewrotation;
                        Vector3 viewinterest;
                        float viewzoom;

                        if (task.getQuaternionValue ("viewrotation", out viewrotation) && task.getVector3Value ("viewinterest", out viewinterest) && task.getFloatValue ("viewzoom", out viewzoom)) {

                            overviewInterface.camera.cameraInterest.transform.localRotation = viewrotation;
                            overviewInterface.camera.cameraInterest.transform.position = viewinterest;
                            Vector3 temp = overviewInterface.camera.cameraObject.transform.localPosition;
                            temp.z = viewzoom;
                            overviewInterface.camera.cameraObject.transform.localPosition = temp;

                        }


                        // send viewer

                        Quaternion userrotation = viewInterface.camera.cameraInterest.transform.localRotation;
                        Vector3 userinterest = viewInterface.camera.cameraInterest.transform.position;
                        float userzoom = viewInterface.camera.cameraObject.transform.localPosition.z;

                        task.setQuaternionValue ("userrotation", userrotation);
                        task.setVector3Value ("userinterest", userinterest);
                        task.setFloatValue ("userzoom", userzoom);


        */

                        callBackName = uxController.update(viewInterface);

                        if (!callBackName.Equals(""))
                        {

                            task.setCallBack(callBackName);

                            //				Debug.Log("calling callback " +callBack);

                        }


                    }


                    if (SETTINGS.isOverview)
                    {


                        // send testrotation


                        // get viewer

                        /*

                        Quaternion userrotation;
                        Vector3 userinterest;
                        float userzoom;

                        if (task.getQuaternionValue ("userrotation", out userrotation) && task.getVector3Value ("userinterest", out userinterest) && task.getFloatValue ("userzoom", out userzoom)) {

                            viewInterface.camera.cameraInterest.transform.localRotation = userrotation;
                            viewInterface.camera.cameraInterest.transform.position = userinterest;
                            Vector3 temp = viewInterface.camera.cameraObject.transform.localPosition;
                            temp.z = userzoom;
                            viewInterface.camera.cameraObject.transform.localPosition = temp;

                        }

                        // send overviewer

                        Quaternion viewrotation = overviewInterface.camera.cameraInterest.transform.localRotation;
                        Vector3 viewinterest = overviewInterface.camera.cameraInterest.transform.position;
                        float viewzoom = overviewInterface.camera.cameraObject.transform.localPosition.z;

                        task.setQuaternionValue ("viewrotation", viewrotation);
                        task.setVector3Value ("viewinterest", viewinterest);
                        task.setFloatValue ("viewzoom", viewzoom);

                        //
        */
                        callBackName = uxController.update(overviewInterface);

                        if (!callBackName.Equals(""))
                        {

                            task.setCallBack(callBackName);

                            //				Debug.Log("calling callback " +callBack);

                        }
                    }

                    break;


                default:

                    done = true;

                    break;

            }

            return done;

        }

        //void MakeBrowserConstraint(){

        //    //float width  =  Display.displays[0].renderingWidth;
        //    //Debug.Log("WIDTH: "+width);

        //    float width=1280;

        //    fileBrowserConstraint.hardClamp = true;
        //    fileBrowserConstraint.hardClampMin = new Vector3(-width - 100, 0);
        //    fileBrowserConstraint.hardClampMax = new Vector3(width + 100, 0);

        //    fileBrowserConstraint.springs = true;
        //    fileBrowserConstraint.springPositions = new Vector2[3];
        //    fileBrowserConstraint.springPositions[0] = new Vector2(-width, 0); // files.
        //    fileBrowserConstraint.springPositions[1] = new Vector2(0, 0); // folders.
        //    fileBrowserConstraint.springPositions[2] = new Vector2(width, 0); // offscreen

        //}

        void Update()
        {

        }

    }
}