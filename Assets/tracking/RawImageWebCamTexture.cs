


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using markerTracking2;


public class RawImageWebCamTexture : MonoBehaviour
{

    public RawImage previewCamera,previewDebug;
    Tracker tracker;

    [HideInInspector]
    public static WebCamTexture webCamTexture;

    public GameObject HeadSetReference;


    public bool callibrated;
    public float HeadsetYawOffset;

    //IEnumerator Start()
    //{

    //    yield return new WaitForSeconds(1.0f);

    //    webCamTexture = new WebCamTexture();

    //    GetComponent<RawImage>().texture = webCamTexture;

    //    webCamTexture.Play();
    //}

    void Start()
    {

        Debug.LogWarning("STARTING WEBCAM script");  

        tracker = new Tracker();

        webCamTexture = new WebCamTexture();

        //GetComponent<RawImage>().texture = webCamTexture;

        previewCamera.texture = webCamTexture;

        callibrated=false;
        webCamTexture.Play();

    }
	private void OnDisable()
	{
        Debug.LogWarning("DISABLING WEBCAM");  
        if (webCamTexture != null)
            webCamTexture.Stop();
	}
	private void OnEnable()
	{
        Debug.LogWarning("ENABLING WEBCAM");  
        if (webCamTexture!=null){
            callibrated=false;
            webCamTexture.Play();

        }

	}


	void OnDestroy()
    {
        if (webCamTexture != null)
            webCamTexture.Stop();
    }


	private void Update()
	{
		
        if (tracker!=null){
            
            tracker.TrackMarkers(webCamTexture);
            previewDebug.texture=tracker.debugTexture;

            foreach (Detection trackedMarker in tracker.markers){

                bool wasLeft = trackedMarker.lastAnchor.x <tracker.width/2;
                bool isLeft = trackedMarker.anchor.x<tracker.width/2;

                if (wasLeft !=isLeft){

                    Debug.Log("CALLIBRATING");

                    callibrated=true;
                    HeadsetYawOffset = HeadSetReference.transform.localRotation.eulerAngles.y;

                }



            }



        }


	}
	
}


