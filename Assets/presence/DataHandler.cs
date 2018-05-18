using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.XR.WSA.Input;

using StoryEngine;

namespace PresenceEngine
{

    public class DataHandler : MonoBehaviour
    {

        float startListening = 0f;
        bool listening = false;

        Capture capture;
        GameObject captureTarget;
        GameObject led;
        int interval;
        //int width, height;
        ushort[] depthMap;
        float timeStamp;
        public RawImage kinectImage, previewImage;
        public Text FPS;


        Texture2D DepthTexture, PreviewTexture;
        //   iTransCoder MainTranscoder;
        int frame = 0;

        GameObject go;
        int testSize = 10000;
        public DataController dataController;
        public GameObject headSet;

        string me = "Data handler: ";

        void SetNetworkIndicators (){
            
            FPS.text=""+(Mathf.Round(1f/Time.deltaTime));


            if (AssitantDirector.BufferStatusOk){
                led.GetComponent<Image>().color=Color.green;
            }else{
                led.GetComponent<Image>().color=Color.red;
                }
            }


        FileformatBase FindBufferFileInScene(string fileName)
        {

            Debug.Log("Trying to find buffer " + fileName);


            foreach (Presence p in SETTINGS.Presences)
            {
                if (p!=null && p.DepthTransport != null && p.DepthTransport.TransCoder != null)
                {
                    FileformatBase bufferFile = p.DepthTransport.TransCoder.GetBufferFile();
                    if (bufferFile != null && bufferFile.Name == fileName)
                    {
                        Debug.Log("found buffer file");
                        return bufferFile;
                    }


                }

            }
            Debug.Log("Didn't find buffer file");
            return null;
        }


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

            IO.localStorageFolder = Application.persistentDataPath + "/data";

            //    PRESENCE.MainDepthTransport = new DepthTransport();


        }

        float recordStart;
        ushort[] rawDepthPrev, rawDepthCur;

        int minPrev, minCur, maxPrev, maxCur;



        public bool TaskHandler(StoryTask task)
        {

            SetNetworkIndicators ();

            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;

            bool done = false;

            switch (task.description)
            {

                case "testtask":


                    //Application.targetFrameRate = 30;
                    //QualitySettings.vSyncCount = 0;

                    //FPS.text=""+1f/Time.deltaTime;

                           
                    if (GENERAL.AUTHORITY==AUTHORITY.GLOBAL){
                    task.SetIntValue("frame", Time.frameCount);

                    }
                    if (GENERAL.AUTHORITY==AUTHORITY.LOCAL){

                        task.SetStringValue("debug", "load "+AssitantDirector.loadBalance);
                    }
                    //if (AssitantDirector.BufferStatusOk){
                    //    led.GetComponent<Image>().color=Color.green;
                    //}else{
                    //    led.GetComponent<Image>().color=Color.red;

                    //}


                    break;

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


                case "playbackbuffer":

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        int frame;
                        if (!task.GetIntValue("frame", out frame))
                        {
                           if (IO.CheckedOutFile != "")
                            {
                                task.SetStringValue("file", IO.CheckedOutFile);
                                FileformatBase Buffered = FindBufferFileInScene(IO.CheckedOutFile);

                                if (Buffered == null)
                                {
                                    // try loading it from disk
                                    Debug.Log("loading from disk");

                                    Buffered = IO.LoadFromCheckedOutFileAsync();

                                    if (Buffered == null)
                                    {
                                        Log.Error("loading file failed");
                                        done = true;
                                        break;
                                    }
                                    }

                                SETTINGS.Presences[1].DepthTransport.Mode = DEPTHMODE.PLAYBACK;
                                SETTINGS.Presences[1].DepthTransport.TransCoder.SetBufferFile(Buffered);
                                SETTINGS.Presences[1].DepthTransport.FrameNumber = SETTINGS.Presences[1].DepthTransport.TransCoder.GetBufferFile().FirstFrame;
                                task.SetIntValue("frame", SETTINGS.Presences[1].DepthTransport.FrameNumber);

                            }
                            else
                            {
                                done = true;
                                }
                           }

                        if (SETTINGS.Presences[1].DepthTransport.Mode == DEPTHMODE.PLAYBACK )
                        {
                            // Play back while in playback mode and playback successful else fall through.


                            if ( SETTINGS.Presences[1].DepthTransport.LoadFrameFromBuffer())
                            {
                                SETTINGS.Presences[1].DepthTransport.FrameNumber++;



                            }
                            else
                            {
                                SETTINGS.Presences[1].DepthTransport.FrameNumber = SETTINGS.Presences[1].DepthTransport.TransCoder.GetBufferFile().FirstFrame;


                            }


                            task.SetIntValue("frame", SETTINGS.Presences[1].DepthTransport.FrameNumber);
                            task.SetStringValue("debug", "" + SETTINGS.Presences[1].DepthTransport.FrameNumber);
                        }
                        else
                        {

                            done = true;
                        }

                    }

                    if (SETTINGS.deviceMode == DEVICEMODE.VRCLIENT)
                    {

                        int frame;
                        if (task.GetIntValue("frame",out frame)){

                            string file;

                            if (task.GetStringValue("file",out file) && file!=""){
                                
                                IO.SetCheckedOutFile(file);
                                task.SetStringValue("file", "");

                                FileformatBase Buffered = FindBufferFileInScene(IO.CheckedOutFile);

                                if (Buffered == null)
                                {
                                    // try loading it from disk
                                    Debug.Log("loading from disk");

                                    Buffered = IO.LoadFromCheckedOutFileAsync();

                                    if (Buffered == null)
                                    {
                                        Log.Error("loading file failed");
                                        // File failed
                                        done = true;
                                        break;
                                    }

                                }

                                SETTINGS.Presences[1].DepthTransport.Mode = DEPTHMODE.PLAYBACK;
                                SETTINGS.Presences[1].DepthTransport.TransCoder.SetBufferFile(Buffered);

                            }

                            SETTINGS.Presences[1].DepthTransport.FrameNumber=frame;

                        }



                        if (SETTINGS.Presences[1].DepthTransport.Mode == DEPTHMODE.PLAYBACK )
                        {
                            if (!SETTINGS.Presences[1].DepthTransport.LoadFrameFromBuffer())
                                Debug.Log("Skipping playback frame?");
                        }
                        else
                        {

                            done = true;
                        }




                    }

                        //if (SETTINGS.Presences[0].DepthTransport.TransCoder.GetBufferFile() != null)
                        //{

                        //    // Check if the file is in buffer






                        //    // set to playback, assign bufferfile and set framenumber
                        //    SETTINGS.Presences[0].DepthTransport.Mode = DEPTHMODE.PLAYBACK;
                        //    SETTINGS.Presences[0].DepthTransport.TransCoder.SetBufferFile(SETTINGS.Presences[0].DepthTransport.TransCoder.GetBufferFile());
                        //    SETTINGS.Presences[0].DepthTransport.FrameNumber = SETTINGS.Presences[0].DepthTransport.TransCoder.GetBufferFile().FirstFrame;
                        //    task.setIntValue("frame", SETTINGS.Presences[0].DepthTransport.FrameNumber);
                        //}
                        //else
                        //{

                        //    done = true;
                        //}

                   

                    //if (SETTINGS.Presences[1].DepthTransport.Mode == DEPTHMODE.PLAYBACK && SETTINGS.Presences[1].DepthTransport.LoadFrameFromBuffer())
                    //{
                    //    // Play back while in playback mode and playback successful else fall through.

                    //    SETTINGS.Presences[1].DepthTransport.FrameNumber++;


                    //    task.setIntValue("frame", SETTINGS.Presences[1].DepthTransport.FrameNumber);

                    //    task.setStringValue("debug", "" + SETTINGS.Presences[1].DepthTransport.FrameNumber);
                    //}
                    //else
                    //{

                    //    done = true;
                    //}
                    break;

                case "stopplaybackbuffer":


                    SETTINGS.Presences[1].DepthTransport.Mode = DEPTHMODE.OFF;

                    done = true;
                    break;

                case "recordstart":


                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {
                        if (SETTINGS.Presences[0].DepthTransport != null && IO.CheckedOutFile != "")
                        {
                            if (!task.GetFloatValue("timeout", out TimeOut))
                            {
                                TimeOut = Time.time + SETTINGS.SessionDuration;
                                task.SetFloatValue("timeout", TimeOut);
                                SETTINGS.Presences[0].DepthTransport.Mode = DEPTHMODE.RECORD;

                                SETTINGS.Presences[0].DepthTransport.TransCoder.CreateBufferFile(IO.CheckedOutFile);
                                task.SetStringValue("file", IO.CheckedOutFile);

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
                        if (SETTINGS.Presences[0].DepthTransport != null)
                        {
                            // Wait for filename then fall through
                            string file;
                            if (task.GetStringValue("file", out file))
                            {
                                IO.SetCheckedOutFile(file);
                                SETTINGS.Presences[0].DepthTransport.Mode = DEPTHMODE.RECORD;
                                SETTINGS.Presences[0].DepthTransport.TransCoder.CreateBufferFile(IO.CheckedOutFile);

                                done = true;
                            }


                        }
                    }

                    break;

                case "recordstop":

                    if (SETTINGS.Presences[0].DepthTransport.Mode == DEPTHMODE.RECORD)
                    {
                        SETTINGS.Presences[0].DepthTransport.Mode = DEPTHMODE.LIVE;

                        Debug.Log("Stopped recording. Logged frames " + SETTINGS.Presences[0].DepthTransport.TransCoder.GetBufferFile().Frames.Count);


                        FileformatBase BufferFile = SETTINGS.Presences[0].DepthTransport.TransCoder.GetBufferFile();
                        IO.SaveToCheckedOutFile(BufferFile);

                    }


                    done = true;
                    break;




                case "userstream":



                 //   FPS.text = ("FPS: " + 1f / Time.smoothDeltaTime);

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {

                        Application.targetFrameRate = 30;
                        QualitySettings.vSyncCount = 0;

                        // Write the transcoder to the task.

                        //string TransCoder;
                        //if (!task.getStringValue("transcoder", out TransCoder))
                        //    task.setStringValue("transcoder", SETTINGS.Presences[0].DepthTransport.TransCoder.Name());

                        DepthTransport MainDT = SETTINGS.Presences[0].DepthTransport;

                        // Retrieve depth info.
                        MainDT.GetNewFrame();

                        // Get latest value for headrotation (from client, via task) and add it to the frame (for recording).
                        task.GetQuaternionValue("headrotation", out MainDT.ActiveFrame.HeadOrientation);

                        if (!MainDT.Encode(task))
                            Log.Warning("Encode failed");

                    }

                    if (SETTINGS.deviceMode == DEVICEMODE.VRCLIENT)
                    {
                        Application.targetFrameRate = 30;
                        QualitySettings.vSyncCount = 0;
                        // Check if we need to set transcoder
                        DepthTransport MainDT = SETTINGS.Presences[0].DepthTransport;

                        //DepthTransport MainDT = SETTINGS.Presences[0].DepthTransport;

                        //if (MainDT != null && MainDT.TransCoder == null)
                        //{
                        //    // No transcoder set, try to get it from task.
                        //    string TransCoder;
                        //    if (task.getStringValue("transcoder", out TransCoder))
                        //        SETTINGS.Presences[0].DepthTransport.SetTranscoder(TransCoder);
                        //}
                        int test;
                        if (task.GetIntValue("frame", out test))
                        {

                            //      Debug.Log(test);

                        }
                        else
                        {
                            Debug.Log("no frame value");
                        }


                        if (!MainDT.Decode(task))
                            Log.Warning("Decode failed");

                        // put head orientation 

                        task.SetQuaternionValue("headrotation", headSet.transform.rotation);
                        MainDT.ActiveFrame.HeadOrientation = headSet.transform.rotation;

                    }

                    break;


                /*
            case "depthcompress":
                Application.targetFrameRate = 30;
                QualitySettings.vSyncCount = 0;
                FPS.text = ("FPS: " + 1f / Time.smoothDeltaTime);

                ushort[] getRawDepth = DepthTransport.OwnsKinect.GetRawDepthMap();
                rawDepthPrev = rawDepthCur;
                rawDepthCur = getRawDepth;




                if (rawDepthPrev == null)
                    rawDepthPrev = new ushort[640 * 480];

                ushort[] rawDepth = rawDepthCur;

                if (kinectImage.texture == null)
                {

                    DepthTexture = new Texture2D(DepthTransport.OwnsKinect.DepthWidth, DepthTransport.OwnsKinect.DepthHeight);
                    kinectImage.texture = DepthTexture;

                }

                //if (previewImage.texture == null)
                //{

                //    PreviewTexture = new Texture2D(DepthTransport.Width, DepthTransport.Height);
                //    previewImage.texture = PreviewTexture;

                //}

                DepthTransport.OwnsKinect.RawDepthToTexture(rawDepth, DepthTexture);

                minPrev = minCur;
                maxPrev = maxCur;


                minCur = DepthTransport.OwnsKinect.Min;
                maxCur = DepthTransport.OwnsKinect.Max;



                byte[] data = StreamSDK.GetVideo();



                if (data != null)
                {

                    //task.setByteValue("data", data);
                    //task.setStringValue("debug", "datasize: " + data.Length);

                    //FPS.text = ("FPS: " + 1f / Time.smoothDeltaTime);

                    StreamSDK.UpdateStreamRemote(data);

                    DepthTransport.OwnsKinect.ApplyEdge(rawDepthPrev, (Texture2D)previewImage.texture, minPrev, maxPrev);


                }

                //previewImage.texture = PreviewTexture;


                //Texture2D testTexture = new Texture2D(DepthTransport.Width, DepthTransport.Height);


                //DepthTransport.RawDepthToTexture(rawDepth, DepthTexture);
                //previewImage.texture = DepthPreviewTextureTexture;

                //ushort[] decodeDepth = DepthTransport.TextureToRawDepth(DepthTexture, min, max);


                ushort[] decodeDepth = DepthTransport.OwnsKinect.TextureToRawDepth((Texture2D)previewImage.texture, minPrev, maxPrev);

                task.setUshortValue("depth", decodeDepth);



                //ComputeParticles(DepthTransport.GetRawDepthMap(), DepthTransport.Width, DepthTransport.Height, 2, new Vector3(0, PRESENCE.kinectHeight, 0), clouds[0]);





                break;
*/
                case "detectgesture":







                    break;

                case "receivelivedepth":
                    // For client.
                    SETTINGS.Presences[0].DepthTransport.Mode = DEPTHMODE.LIVE;
                    done = true;

                    break;

                case "depthlive":

                    // For server.
                    SETTINGS.Presences[0].DepthTransport.Mode = DEPTHMODE.LIVE;
                    done = true;

                    break;

                case "depthoff":


                    SETTINGS.Presences[0].DepthTransport.Mode = DEPTHMODE.OFF;

                    done = true;

                    break;

                case "depthrecord":


                    SETTINGS.Presences[0].DepthTransport.Mode = DEPTHMODE.RECORD;
                    recordStart = Time.time;
                    done = true;

                    break;

                case "depthplayback":

                    SETTINGS.Presences[0].DepthTransport.Mode = DEPTHMODE.PLAYBACK;
                    done = true;

                    break;

                case "togglepresence":

                    if (SETTINGS.Presences[0].DepthTransport == null || SETTINGS.Presences[0].DepthTransport.Mode == DEPTHMODE.OFF)
                    {

                        // switch on

                        task.setCallBack("startpresence");

                    }
                    else
                    {
                        // switch off

                        task.setCallBack("stoppresence");
                    }



                    break;

                /*
            case "recorddepth":
                Application.targetFrameRate = 30;
                QualitySettings.vSyncCount = 0;
                FPS.text = ("FPS: " + 1f / Time.smoothDeltaTime);

                if (Time.time - recordStart > 5)
                {

                    DepthTransport.OwnsKinect.Mode = DEPTHMODE.OFF;
                    DepthTransport.OwnsKinect.depthSequence.SaveAs("depthCapture");
                    done = true;
                }




                break;
                */
                case "playdepth":
                    Application.targetFrameRate = 30;
                    QualitySettings.vSyncCount = 0;
                    FPS.text = ("FPS: " + 1f / Time.smoothDeltaTime);

                    break;

                case "depthwritetest":


                    RawDepthSequence sequence = new RawDepthSequence();

                    ushort[] faux = new ushort[640 * 480];

                    sequence.Put(faux);
                    sequence.SaveAs("testsequence");
                    done = true;



                    break;


                case "depthloadtest":


                    RawDepthSequence load = new RawDepthSequence();

                    //ushort[] faux = new ushort[640 * 480];

                    //sequence.Add(faux);
                    load.LoadFrom("testsequence");

                    ushort[] loaddata = load.Get(0);


                    done = true;



                    break;
                /*
            case "transfertest":

                if (PRESENCE.deviceMode == DEVICEMODE.SERVER)

                {

                    if (previewImage.texture == null)
                    {

                        PreviewTexture = new Texture2D(DepthTransport.OwnsKinect.DepthWidth, DepthTransport.OwnsKinect.DepthHeight);
                        previewImage.texture = PreviewTexture;

                    }



                    if (Input.GetKeyUp("="))
                    {

                        //   testSize += 250;
                        StreamSDK.instance.quality += 5;
                        if (StreamSDK.instance.quality < 0)
                        {
                            StreamSDK.instance.quality = 0;
                        }
                    }


                    if (Input.GetKeyUp("-"))
                    {
                        StreamSDK.instance.quality -= 5;
                        if (StreamSDK.instance.quality > 100)
                        {
                            StreamSDK.instance.quality = 100;
                        }
                        // testSize += 250;

                    }

                    Application.targetFrameRate = 35;
                    QualitySettings.vSyncCount = 0;


                    //byte[] data = new byte[testSize];


                    byte[] vidData = StreamSDK.GetVideo();

                    if (vidData != null)
                    {

                        task.setByteValue("data", vidData);
                        task.setStringValue("debug", "datasize: " + vidData.Length);

                        FPS.text = ("FPS: " + 1f / Time.smoothDeltaTime);

                        StreamSDK.UpdateStreamRemote(vidData);


                        //  outline detection.
                        // re-apply outline to streamed img.

                        //DepthTransport.Edge(ushort[] depthMap, Texture2D texture);


                    }

                }

                if (PRESENCE.deviceMode == DEVICEMODE.VRCLIENT)
                {

                    byte[] data2;

                    if (task.getByteValue("data", out data2))
                    {

                        //  previ
                        Application.targetFrameRate = 30;
                        QualitySettings.vSyncCount = 0;
                        //task.setStringValue("debug", "datasize: " + data.Length);
                        FPS.text = ("FPS: " + 1 / Time.smoothDeltaTime + "UPD " + task.LastUpdatesPerFrame);
                        StreamSDK.UpdateStreamRemote(data2);

                    }



                }



                break;

            */


                case "initstream":
                    {
                        if (StreamSDK.instance != null)
                        {
                            StreamSDK.instance.width = 640;
                            StreamSDK.instance.height = 480;
                            StreamSDK.instance.quality = 65;

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

                /*
            case "cloudstream2":

                {

                    if (PRESENCE.deviceMode == DEVICEMODE.SERVER)
                    {

                        if (StreamSDK.instance == null)
                        {
                            Debug.LogError("Streamsdk null.");

                        }

                        if (DepthTexture == null)
                        {
                            DepthTexture = new Texture2D(DepthTransport.OwnsKinect.DepthWidth, DepthTransport.OwnsKinect.DepthHeight);

                        }

                        ushort[] DepthMap = DepthTransport.OwnsKinect.GetRawDepthMap();
                        kinectImage.texture = DepthTransport.OwnsKinect.RawDepthToTexture(DepthMap, DepthTexture);

                        byte[] video = StreamSDK.GetVideo();

                        if (video != null)

                        {
                            frame++;

                            task.setIntValue("frame", frame);

                            task.setByteValue("video", video);

                            task.setIntValue("min", DepthTransport.OwnsKinect.DepthMin.Int);
                            task.setIntValue("max", DepthTransport.OwnsKinect.DepthMax.Int);

                            task.setStringValue("debug", "frame: " + frame + "data: " + video.Length);

                            StreamSDK.UpdateStreamRemote(video);


                            //DepthTransport.ApplyEdge(DepthMap, PreviewTexture);



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
                */
                case "amserver":

                    SETTINGS.deviceMode = DEVICEMODE.SERVER;
                    done = true;

                    break;

                case "amvrclient":

                    SETTINGS.deviceMode = DEVICEMODE.VRCLIENT;
                    done = true;

                    break;

                case "setmodetopreview":

                    GENERAL.GLOBALS.SetStringValue("mode", "preview");

                    done = true;

                    break;

                case "setmodetolive":

                    GENERAL.GLOBALS.SetStringValue("mode", "live");

                    done = true;

                    break;

                case "setmodetomirror":

                    GENERAL.GLOBALS.SetStringValue("mode", "mirror");

                    done = true;

                    break;

                case "setmodetoecho":

                    GENERAL.GLOBALS.SetStringValue("mode", "echo");

                    done = true;

                    break;

                case "setmodetoserveronly":

                    GENERAL.GLOBALS.SetStringValue("mode", "serveronly");

                    done = true;

                    break;

                case "captureframezero":

                    SETTINGS.CaptureFrame = 0;

                    done = true;
                    break;

                case "setsession":

                    if (SETTINGS.deviceMode == DEVICEMODE.SERVER)
                    {

                        SETTINGS.capture = new CloudSequence(SETTINGS.captureLength);

                        // pass values to client

                        GENERAL.GLOBALS.SetIntValue("echooffset", SETTINGS.echoOffset);
                        GENERAL.GLOBALS.SetIntValue("capturelength", SETTINGS.captureLength);


                        done = true;
                    }

                    if (SETTINGS.deviceMode == DEVICEMODE.VRCLIENT)
                    {

                        int captureLength, echooffset;


                        // get values from globals

                        if (GENERAL.GLOBALS.GetIntValue("capturelength", out captureLength))
                        {

                            SETTINGS.captureLength = captureLength;
                            SETTINGS.capture = new CloudSequence(SETTINGS.captureLength);

                            if (GENERAL.GLOBALS.GetIntValue("echooffset", out echooffset))
                            {

                                SETTINGS.echoOffset = echooffset;

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

                    //DepthTransport.SetActive(false);
                    //DepthTransport.SetMode(DEPTHMODE.OFF);

                    //         DepthTransport.OwnsKinect.Mode = DEPTHMODE.OFF;


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

                            //GameObject kinectObject = GameObject.Find ("Kinect");
                            //	DepthTransport = new DepthTransport ();
                            //    DepthTransport.SetActive(true);
                            DepthTransport.OwnsKinect.Mode = DEPTHMODE.LIVE;


                            kinectstatus = "initialising";

#else

                            Debug.LogWarning(me + "Non windows platform: dummy kinect.");
                            kinectstatus = "dummy";



#endif

                            timeStamp = Time.time;

                            break;


                        //case "initialising":

                        ////Debug.Log( me+ "kinect awake? "+ DepthTransport.KinectManager.am

                        //if (DepthTransport.OwnsKinect.Mode == DEPTHMODE.LIVE)
                        //{

                        //    kinectstatus = "live";

                        //    Debug.Log(me + "Kinect live in : " + (Time.time - timeStamp));

                        //    kinectstatus = "live";

                        //}
                        //else
                        //{

                        //    // Time out

                        //    if ((Time.time - timeStamp) > 1f)
                        //    {

                        //        Debug.LogWarning(me + "Kinect timed out.");

                        //        kinectstatus = "dummy";

                        //    }


                        //}
                        //break;

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


                /*
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        case "recordkinect":

            // we grab a frame and write it to disk...

            Debug.LogWarning(me + "writing depth to disk at " + Application.persistentDataPath);

            int width = DepthTransport.OwnsKinect.DepthWidth;
            int height = DepthTransport.OwnsKinect.DepthHeight;

            depthMap = DepthTransport.OwnsKinect.GetRawDepthMap();

            DepthCapture dc = new DepthCapture(width, height);
            DepthCapture.current = dc;

            dc.put(depthMap);

            IO.SaveDepthCaptures();





            done = true;

            break;

#endif
*/




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

                    int ConnectedClients = dataController.serverConnections();

                    led.SetActive(ConnectedClients>0);

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

                /*
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




                IO.SaveCloudSequence(SETTINGS.capture);

                done = true;

                break;

            case "loadsequence":

                SETTINGS.CaptureFrame = 0;
                SETTINGS.TimeStamp = Time.time;

                SETTINGS.capture = IO.LoadCloudSequence();

                if (SETTINGS.capture != null)
                {
                    done = true;
                }



                break;

            case "loadsequenceresource":

                SETTINGS.CaptureFrame = 0;
                SETTINGS.TimeStamp = Time.time;

                SETTINGS.capture = IO.LoadCloudSequenceFromResources();

                if (SETTINGS.capture != null)
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
*/

                // MISC / WIP

                /*
            case "goglobal":

                task.pointer.scope = SCOPE.GLOBAL;
                task.pointer.modified = true;

                done = true;

                break;

*/
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

                        GENERAL.GLOBALS.SetStringValue("test", "hello world");

                        //				task.setStringValue ("debug", "" + Random.value);

                    }

                    done = true;

                    break;

                case "sometask2":

                    if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL)
                    {

                        string test;

                        if (GENERAL.GLOBALS.GetStringValue("test", out test))
                        {

                            task.SetStringValue("debug", test);

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
}