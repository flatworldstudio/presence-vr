using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using markerTracking2;
using UnityEngine.UI;

public class trackerTest : MonoBehaviour {

    public RawImage raw;
    public RawImage debugging;
    Tracker tracker;

	// Use this for initialization
	void Start () {

        //Debug.Log("tracking");

       tracker = new Tracker();

        //if (raw.texture != null)
        //{
         

        //    tracker.Set(raw.texture as Texture2D);


           
            //tracker.debugTexture = new Texture2D(1080, 720);

        //    debugging.texture = tracker.debugTexture;

        //    StartCoroutine(tracker.Detect());


        //}

	}
	
	// Update is called once per frame
	void Update () {

        Application.targetFrameRate=30;

        if (raw.texture != null)
        {

            //tracker.DetectMarker(raw.texture as WebCamTexture);


            tracker.TrackMarkers(raw.texture as WebCamTexture);

            //tracker.Detect(raw.texture as Texture2D);

            debugging.texture = tracker.debugTexture;

            foreach (Detection trackedMarker in tracker.markers){

                bool wasLeft = trackedMarker.lastAnchor.x <tracker.width/2;
                bool isLeft = trackedMarker.anchor.x<tracker.width/2;

                if (wasLeft !=isLeft){

                    Debug.Log("CALLIBRATING");
                }



            }


            //Debug.Log(raw.texture.width + " " + raw.texture.height);

        }

	}
}
