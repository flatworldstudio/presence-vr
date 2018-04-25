using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class DataHandler : MonoBehaviour
{

    float startListening = 0f;
    bool listening = false;

    Capture capture;
    GameObject captureTarget;
    GameObject led;
    int interval;
    int width, height;
    ushort[] depthMap;
    float timeStamp;
    public RawImage kinectImage;

    int frame = 0;

    GameObject go;

    public DataController dataController;

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

        led = GameObject.Find("led");
        led.SetActive(false);

        dataController.addTaskHandler(TaskHandler);





    }

    public bool TaskHandler(StoryTask task)
    {

        bool done = false;

        switch (task.description)
        {
            case "initstream":
                {
                    if (StreamSDK.instance != null)
                    {
                        StreamSDK.instance.width = 320;
                        StreamSDK.instance.height = 240;
                        StreamSDK.instance.quality = 90;

                        StreamSDK.instance.framerate = 30;

                        StreamSDK.instance.InitVideo();
                    }
                    else
                    {

                        Log.Error("stream sdk not initialised");
                    }

                    done = true;
                    break;
                }
            case "cloudstream2":

                {

                    if (PRESENCE.deviceMode == DEVICEMODE.SERVER)
                    {

                        if (KinectManager.Instance != null && StreamSDK.instance != null)
                        {

                            kinectImage.texture = DepthTransport.GetDepthTexture();
                  

                            byte[] video = StreamSDK.GetVideo();


                            if (video != null)

                            {
                                frame++;

                                task.setIntValue("frame", frame);

                                task.setByteValue("video", video);

                                task.setIntValue("min", DepthTransport.DepthMin.Int);
                                task.setIntValue("max", DepthTransport.DepthMax.Int);



                                task.setStringValue("debug", "frame: " + frame + "data: " + video.Length);

                            }

                        }

                    }

                    if (PRESENCE.deviceMode == DEVICEMODE.VRCLIENT)
                    {
                        if (StreamSDK.instance != null)
                        {
                            int getFrame;
                            byte[] getVideo;




                            if (task.getIntValue("frame", out getFrame) && frame != getFrame && task.getByteValue("video", out getVideo))
                            {

                                frame = getFrame;
                                //    Debug.Log("data " + getVideo.Length);

                                //   Debug.Log("frame " + frame);

                                StreamSDK.UpdateStreamRemote(getVideo);

                                //    StreamSDK.UpdateStreamRemote(getVideo);





                            }




                        }



                    }




                    break;


                }

            case "amserver":

                PRESENCE.deviceMode = DEVICEMODE.SERVER;
                done = true;

                break;

            case "amvrclient":

                PRESENCE.deviceMode = DEVICEMODE.VRCLIENT;
                done = true;

                break;

            case "setmodetopreview":

                GENERAL.GLOBALS.setStringValue("mode", "preview");

                done = true;

                break;

            case "setmodetolive":

                GENERAL.GLOBALS.setStringValue("mode", "live");

                done = true;

                break;

            case "setmodetomirror":

                GENERAL.GLOBALS.setStringValue("mode", "mirror");

                done = true;

                break;

            case "setmodetoecho":

                GENERAL.GLOBALS.setStringValue("mode", "echo");

                done = true;

                break;

            case "setmodetoserveronly":

                GENERAL.GLOBALS.setStringValue("mode", "serveronly");

                done = true;

                break;

            case "captureframezero":

                PRESENCE.CaptureFrame = 0;

                done = true;
                break;

            case "setsession":

                if (PRESENCE.deviceMode == DEVICEMODE.SERVER)
                {

                    PRESENCE.capture = new CloudSequence(PRESENCE.captureLength);

                    // pass values to client

                    GENERAL.GLOBALS.setIntValue("echooffset", PRESENCE.echoOffset);
                    GENERAL.GLOBALS.setIntValue("capturelength", PRESENCE.captureLength);


                    done = true;
                }

                if (PRESENCE.deviceMode == DEVICEMODE.VRCLIENT)
                {

                    int captureLength, echooffset;


                    // get values from globals

                    if (GENERAL.GLOBALS.getIntValue("capturelength", out captureLength))
                    {

                        PRESENCE.captureLength = captureLength;
                        PRESENCE.capture = new CloudSequence(PRESENCE.captureLength);

                        if (GENERAL.GLOBALS.getIntValue("echooffset", out echooffset))
                        {

                            PRESENCE.echoOffset = echooffset;

                            done = true;

                        }


                    }

                }

                break;


            // KINECT

            case "dummykinect":

                go = GameObject.Find("Kinect");

                //	DepthTransport = new DepthTransport ();

                //		DepthTransport.InitDummy (go);




                done = true;

                break;


            // Code specific to Kinect, to be compiled and run on windows only.
            case "stopspatialdata":

                DepthTransport.SetActive(false);


                done = true;

                break;

            case "startspatialdata":

                // attempts to start live kinect.


                string kinectstatus;

                if (!task.getStringValue("kinectstatus", out kinectstatus))
                {
                    kinectstatus = "void";

                }


                switch (kinectstatus)
                {


                    case "void":

                        // first run.

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

                        //GameObject kinectObject = GameObject.Find ("Kinect");
                        //	DepthTransport = new DepthTransport ();
                        DepthTransport.SetActive(true);

                        kinectstatus = "initialising";

#else

				Debug.LogWarning (me + "Non windows platform: dummy kinect.");
				kinectstatus = "dummy";



#endif

                        timeStamp = Time.time;

                        break;


                    case "initialising":

                        //Debug.Log( me+ "kinect awake? "+ DepthTransport.KinectManager.am

                        if (DepthTransport.IsLive())
                        {

                            kinectstatus = "live";

                            Debug.Log(me + "Kinect live in : " + (Time.time - timeStamp));

                            kinectstatus = "live";

                        }
                        else
                        {

                            // Time out

                            if ((Time.time - timeStamp) > 1f)
                            {

                                Debug.LogWarning(me + "Kinect timed out.");

                                kinectstatus = "dummy";

                            }


                        }
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


                task.setStringValue("kinectstatus", kinectstatus);


                break;



#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            case "recordkinect":

                // we grab a frame and write it to disk...

                Debug.LogWarning(me + "writing depth to disk at " + Application.persistentDataPath);

                width = DepthTransport.Width;
                height = DepthTransport.Height;

                depthMap = DepthTransport.GetRawDepthMap();

                DepthCapture dc = new DepthCapture(width, height);
                DepthCapture.current = dc;

                dc.put(depthMap);

                IO.SaveDepthCaptures();





                done = true;

                break;

#endif





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

                task.setStringValue("debug", "" + dataController.serverConnections());

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
                        task.setStringValue("debug", "lost connection");

                        task.setCallBack("serversearch");


                        done = true; // we'll fall throught to the next line in the script, since there is no longer a connection to monitor.

                    }
                    else
                    {

                        task.setStringValue("debug", "no connection yet");

                    }

                }
                else
                {

                    GENERAL.wasConnected = true;

                    task.setStringValue("debug", "connected");

                }

                led.SetActive(GENERAL.wasConnected);

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





            // IO


            case "loaddepthdata":

                // load depth data from resources

                IO.LoadDepthCapturesResource();

                if (IO.savedDepthCaptures.Count > 0)
                {
                    IO.depthIndex = 0;
                }

                done = true;

                break;

            case "playback":

                if (Capture.playing)
                {

                    Frame f;

                    if (!capture.read(out f))
                    {


                        done = true;


                    }
                    else
                    {
                        captureTarget.transform.position = f.getPosition();
                        captureTarget.transform.rotation = f.getRotation();

                    }


                }
                else
                {

                    capture = Capture.current;

                    capture.play();

                    captureTarget = GameObject.Find("camB_object");


                }




                break;

            case "captureLEGACY":

                if (Capture.capturing)
                {

                    Vector3 pos = captureTarget.transform.position;
                    Quaternion orient = captureTarget.transform.rotation;

                    if (!capture.log(pos, orient))
                    {

                        // if log filled, end task

                        done = true;

                    }


                }
                else
                {

                    capture = new Capture();
                    Capture.current = capture;
                    capture.capture();

                    captureTarget = GameObject.Find("camB_object");


                    Debug.Log(me + "Starting new capture.");



                }


                break;


            case "load":

                IO.LoadUserCaptures();


                Capture.current = IO.savedCaptures[0];


                //			Debug.Log (me + "loaded with name " + Capture.current.knight.name);

                //					Game.current = testgame;

                //					SaveLoad.Save ();

                done = true;

                break;

            case "savecapture":




                IO.SaveCloudSequence(PRESENCE.capture);

                done = true;

                break;

            case "loadsequence":

                PRESENCE.CaptureFrame = 0;
                PRESENCE.TimeStamp = Time.time;

                PRESENCE.capture = IO.LoadCloudSequence();

                if (PRESENCE.capture != null)
                {
                    done = true;
                }



                break;

            case "loadsequenceresource":

                PRESENCE.CaptureFrame = 0;
                PRESENCE.TimeStamp = Time.time;

                PRESENCE.capture = IO.LoadCloudSequenceFromResources();

                if (PRESENCE.capture != null)
                {
                    done = true;
                }



                break;

            case "save":


                //			Capture testgame = new Capture ();

                //			testgame.knight.name = "My uuid: " + UUID.getID ();

                //			Debug.Log (me + "saving with name " + testgame.knight.name);

                //			Capture.current = testgame;

                Debug.Log(me + "Saving capture.");

                IO.SaveUserCaptures();

                done = true;

                break;


            // MISC / WIP

            case "goglobal":

                task.pointer.scope = SCOPE.GLOBAL;
                task.pointer.modified = true;

                done = true;

                break;


            case "isglobal":

                if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL)
                {

                    task.pointer.scope = SCOPE.GLOBAL;

                }

                done = true;

                break;

            case "globaltask":

                //			if (task.scope == SCOPE.GLOBAL) {
                //
                //				task.setStringValue ("debug", "global");
                //
                //
                //			} else {
                //
                //				task.setStringValue ("debug", "local");
                //			}


                break;

            case "sometask1":

                if (GENERAL.AUTHORITY == AUTHORITY.LOCAL)
                {

                    GENERAL.GLOBALS.setStringValue("test", "hello world");

                    //				task.setStringValue ("debug", "" + Random.value);

                }

                done = true;

                break;

            case "sometask2":

                if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL)
                {

                    string test;

                    if (GENERAL.GLOBALS.getStringValue("test", out test))
                    {

                        task.setStringValue("debug", test);

                    }

                    //				task.setStringValue ("debug", "" + Random.value);

                }

                break;


            case "passglobal":

                // go over all pointers 

                foreach (StoryTask theTask in GENERAL.ALLTASKS)
                {

                    if (theTask.scope == SCOPE.GLOBAL)
                    {

                        //					task.modified = true; // force send network update
                        theTask.MarkAllAsModified();

                    }

                }

                done = true;

                break;

            /*
		case "sync":
			
			StoryPointer targetPointer;

			if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL) {

				// this should be the case, since we're only calling this on the server...

				targetPointer = GENERAL.getPointerForStoryline ("userinterface");

				if (targetPointer != null) {
					targetPointer.scope = SCOPE.GLOBAL;
					targetPointer.modified = true;

//					Debug.Log (me + "sync pointer: " + targetPointer.ID);

					targetPointer.currentTask.scope = SCOPE.GLOBAL;

					targetPointer.currentTask.markAllAsModified ();

//					Debug.Log (me + "sync task: " + targetPointer.currentTask.ID);
				}





				targetPointer = GENERAL.getPointerForStoryline ("cloud");

				if (targetPointer != null) {
					targetPointer.scope = SCOPE.GLOBAL;
					targetPointer.modified = true;

//					Debug.Log (me + "sync pointer: " + targetPointer.ID);

					targetPointer.currentTask.scope = SCOPE.GLOBAL;

					targetPointer.currentTask.markAllAsModified ();

//					Debug.Log (me + "sync task: " + targetPointer.currentTask.ID);

				}






			} else {

				Debug.LogWarning (me + "Syncing but no global authority.");
			}


		

			done = true;

			break;


		*/


            default:

                // Default is caught in 
                done = true;

                break;

        }

        return done;

    }

    void Update()
    {

        // HACK
        string inputString = Input.inputString;
        if (inputString.Length > 0)
        {

            if (inputString == "d")
            {

                Debug.Log(me + "Simulating disconnect/pause ...");

                dataController.StopNetworkClient();

            }

        }

    }

}
