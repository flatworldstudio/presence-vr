


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using markerTracking2;
using StoryEngine;

public class CalibrateOnMarker : MonoBehaviour
{

    public RawImage previewCamera,previewDebug;
    Tracker tracker;

    [HideInInspector]
    public static WebCamTexture webCamTexture;

    public GameObject HeadSetReference;

    StoryTask taskReference;

    [HideInInspector]
    public bool callibrated;
    [HideInInspector]
    public float HeadsetYawOffset;

    //IEnumerator Start()
    //{

    //    yield return new WaitForSeconds(1.0f);

    //    webCamTexture = new WebCamTexture();

    //    GetComponent<RawImage>().texture = webCamTexture;

    //    webCamTexture.Play();
    //}

    public void StartCalibration (StoryTask task){


        taskReference=task;

        this.gameObject.SetActive(true);

        tracker = new Tracker();

        if (webCamTexture==null){
            webCamTexture = new WebCamTexture();
        }
        previewCamera.texture = webCamTexture;

        callibrated=false;
        webCamTexture.Play();

    }

    public void EndCalibration (){


        taskReference=null;

        if (webCamTexture != null)
                   webCamTexture.Stop();

        this.gameObject.SetActive(false);



    }

    void Start()
    {

        //Debug.LogWarning("STARTING WEBCAM script");  

        //tracker = new Tracker();

        //webCamTexture = new WebCamTexture();

        ////GetComponent<RawImage>().texture = webCamTexture;

        //previewCamera.texture = webCamTexture;

        //callibrated=false;
        //webCamTexture.Play();

    }


	//private void OnDisable()
	//{
 //       Debug.LogWarning("DISABLING WEBCAM");  
 //       if (webCamTexture != null)
 //           webCamTexture.Stop();
	//}


	//private void OnEnable()
	//{
 //       Debug.LogWarning("ENABLING WEBCAM");  
 //       if (webCamTexture!=null){
 //           callibrated=false;
 //           webCamTexture.Play();

 //       }

	//}


	void OnDestroy()
    {
        if (webCamTexture != null)
            webCamTexture.Stop();
    }


	private void Update()
	{
		
        if (!GENERAL.ALLTASKS.Contains(taskReference)){

            // Task was removed, abort.

            EndCalibration();

            return;
        }
            
        if (tracker!=null && webCamTexture!=null){
            
            tracker.TrackMarkers(webCamTexture);
            previewDebug.texture=tracker.debugTexture;

            foreach (Detection trackedMarker in tracker.markers){

                bool wasLeft = trackedMarker.lastAnchor.x <tracker.width/2;
                bool isLeft = trackedMarker.anchor.x<tracker.width/2;

                if (wasLeft !=isLeft){

                    Debug.Log("CALIBRATING");

                    callibrated=true;
                    HeadsetYawOffset = HeadSetReference.transform.localRotation.eulerAngles.y;

                }



            }



        }


	}
	
}


