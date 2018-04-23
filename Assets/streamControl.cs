using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class streamControl : MonoBehaviour {
    bool streamInit = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (StreamSDK.instance != null )
        {

            if (!streamInit)
            {
                Debug.Log("sdk live");
                StreamSDK.instance.width = 320;
                StreamSDK.instance.height = 240;
                StreamSDK.instance.quality = 20;

                StreamSDK.instance.framerate = 30;

                StreamSDK.instance.InitVideo();

                streamInit = true;


            }
            else {

                byte[] video = StreamSDK.GetVideo();
                if (video != null)
                {
                    StreamSDK.UpdateStreamRemote(video);


                }



            }
          

        }




	}
}
