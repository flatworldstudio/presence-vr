using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StreamSDKQualityManager : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {
		
		yield return new WaitUntil( ()=>StreamSDK.instance != null );

        Debug.Log("starting quality manager");
		//Default Video Options changed by Options Dialog in demo scene
		if( PlayerPrefs.GetInt( "StreamSDKOptions" ) != 1 || !PlayerPrefs.HasKey( "StreamSDKOptions" ) ) {
			StreamSDK.instance.width = 320;
			StreamSDK.instance.height = 240;
			StreamSDK.instance.quality = 20;

			StreamSDK.instance.framerate = 30;

			StreamSDK.instance.InitVideo();	

		} else {
			StreamSDK.instance.width = PlayerPrefs.GetInt( "StreamSDKVideoWidth" );
			StreamSDK.instance.height = PlayerPrefs.GetInt( "StreamSDKVideoHeight" );
			StreamSDK.instance.quality   = PlayerPrefs.GetInt( "StreamSDKVideoQuality" );

			StreamSDK.instance.framerate = PlayerPrefs.GetInt( "StreamSDKFramerate" );

			StreamSDK.instance.InitVideo();	
		}
        StreamSDK.instance.compression = Compression.high;
		StreamSDK.instance.averageFramesSpan = 60;

		//Default Audio Options changed by Calibrate Mic Dialog in demo scene
		if( PlayerPrefs.GetInt( "StreamSDKCalibrateMic" ) != 1  || !PlayerPrefs.HasKey( "StreamSDKCalibrateMic" ) ) {
			StreamSDK.instance.SetAudioDecay( 0.025f );
			StreamSDK.instance.SetAudioThreshold( 0.1f );
		} else {
			StreamSDK.instance.SetAudioDecay( PlayerPrefs.GetFloat( "StreamSDKAudioDecay" ) );
			StreamSDK.instance.SetAudioThreshold( PlayerPrefs.GetFloat( "StreamSDKAudioThreshold" ) );
		}
		StreamSDK.instance.audioFrequency = 8000;
		StreamSDK.instance.echoCancellation = EchoCancellation.off;

		StreamSDK.instance.InitMic();
	}
	
	// Update is called once per frame
	void Update () {
		if( StreamSDK.instance != null ) {
			//StreamSDK's video quality can be adjusted on the fly
			/*
			if( Input.GetKeyDown( KeyCode.Space ) ) {
				StreamSDK.instance.width = 160;
				StreamSDK.instance.height = 90;
				StreamSDK.instance.framerate = 30;
				StreamSDK.instance.quality = 100;
				StreamSDK.instance.averageFramesSpan = 30;
				StreamSDK.instance.InitVideo();	
			}
			*/

			//StreamSDK's framerate can be adjusted on the fly to maintain the stream, or increase overall performance
			/*
			if( StreamSDK.instance.framerates.Count >= StreamSDK.instance.averageFramesSpan && StreamSDK.instance.averageFramesSpan > 0 ) {
				if( StreamSDK.instance.framerates.Average() < StreamSDK.instance.framerate ) {
					//Cut the target framerate in half if the average framerate is too low and the number of measured frames is sufficient to get a dependable average
					StreamSDK.instance.framerate /= 2;
					StreamSDK.instance.framerates.Clear();
                    Debug.Log("frame rate slashed " + StreamSDK.instance.framerate);
				}
			}
			*/
			//Write your own algorithm to the results you desire based on your specific needs
			//YOUR CODE HERE
		}
	}
}
