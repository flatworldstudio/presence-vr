using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHandler : MonoBehaviour
{

	public SetController setController;
	public GameObject cloud;

	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	public GameObject kinect;
	KinectManager kinectManager;

	#endif


	ushort[] depthMap;
	int width, height;

	string me = "Task handler: ";

	int interval = 0;


	void Start ()
	{

		setController.addTaskHandler (TaskHandler);

		ParticleCloud.init (GameObject.Find ("Cloud"));

		#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

		kinectManager = kinect.GetComponent<KinectManager> ();

		#endif
	}


	public bool TaskHandler (StoryTask task)
	{
		
		bool done = false;

		switch (task.description) {

		case "nextdepth":

			IO.depthIndex++;

			if (IO.depthIndex == IO.savedDepthCaptures.Count) {
				IO.depthIndex = 0;
			}

			Debug.Log (me + "next depth");

			done = true;

			break;

		case "showdepthdata":



			if (GENERAL.SCOPE == SCOPE.GLOBAL) {



				int ind;

				if (task.getIntValue ("index", out ind)) {
					
					if (ind != IO.depthIndex) {

						// changed

						task.setIntValue ("index", IO.depthIndex);
						task.setStringValue ("debug", "" + IO.depthIndex);


					}

				} else {

					task.setIntValue ("index", IO.depthIndex);
					task.setStringValue ("debug", "" + IO.depthIndex);

				}
					




			} else {

				int ind;
				if (task.getIntValue("index",out ind))
					IO.depthIndex=ind;

			}


			if (IO.depthIndex >= 0) {

				//				int index = 0;
				//
				//				if (!task.getIntValue ("index", out index)) {
				//
				//					task.setIntValue ("index", 0);
				//				}

				//				int index = IO.depthIndex;



//				string index;
//
//				if (task.getStringValue("debug" , out index)){
//
//					if (int.Parse(index) != IO.depthIndex) {
//						
//						task.setStringValue("debug",""+IO.depthIndex);
//
//					}
//
//
//				}


//				int ind;
//
//				if (task.getIntValue ("index", out ind)) {
//
//
//
//
//
//				} else {
//
//					task.setIntValue ("index", IO.depthIndex);
//				}


				DepthCapture.current = IO.savedDepthCaptures [IO.depthIndex];

//				ParticleCloud.setLifeTime (0.1f);

				if (interval == 4 && DepthCapture.current != null) {

					interval = 0;

					depthMap = DepthCapture.current.GetRawDepthMap ();

					width = DepthCapture.current.getUserDepthWidth ();
					height = DepthCapture.current.getUserDepthHeight ();

					int sample = 8;
					Vector3 point;

					for (int y = 0; y < height; y += sample) {

						for (int x = 0; x < width; x += sample) {

							int i = y * width + x;

							ushort userMap = (ushort)(depthMap [i] & 7);
							ushort userDepth = (ushort)(depthMap [i] >> 3);

							if (userMap != 0) {
								point = depthToWorld (x, y, userDepth);
								point.y = -point.y;
								point.y += 1.5f;

								ParticleCloud.Emit (point);
							}

						}

					}

				}

				interval++;

			} else {


				done = true;

			}








			break;



		#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

			// Code specific to Kinect, to be compiled and run on windows only.

		case "startkinect":


			kinect.SetActive (true);

			done = true;

			break;




		case "recordkinect":

			// we grab a frame and write it to disk...

			Debug.LogWarning (me + "writing depth to disk at " + Application.persistentDataPath);

			width = kinectManager.getUserDepthWidth ();
			height = kinectManager.getUserDepthHeight ();

			depthMap = kinectManager.GetRawDepthMap ();

			DepthCapture dc = new DepthCapture (width, height);
			DepthCapture.current = dc;

			dc.put (depthMap);

			IO.SaveDepthCaptures ();





			done = true;

			break;

		case "kinect":


			ParticleCloud.setLifeTime (0.1f);


			if (interval == 4) {
				interval = 0;

				depthMap = kinectManager.GetRawDepthMap ();

				width = kinectManager.getUserDepthWidth ();
				height = kinectManager.getUserDepthHeight ();



				int sample = 8;
				Vector3 point;

				for (int y = 0; y < height; y += sample) {

					for (int x = 0; x < width; x += sample) {

						int i = y * width + x;

						ushort userMap = (ushort)(depthMap [i] & 7);
						ushort userDepth = (ushort)(depthMap [i] >> 3);

						if (userMap != 0) {
							point = depthToWorld (x, y, userDepth);
							point.y = -point.y;
							point.y += 1.5f;

							ParticleCloud.Emit (point);
						}

					}



				}





			}




			interval++;


			//			int y = i / kinectManager.getUserDepthWidth;
			//			int x = i - y * usersMapHeight;



			// test plot into cloud at 1/4 resolution

			//			if (x % 8 == 0 && y % 8 == 0) {
			//
			//				point = depthToWorld (x, y, userDepth);
			//				point.y = -point.y;
			//				point.y += 1.5f;
			//
			//				ParticleCloud.Emit (point);
			//
			//
			//			}




			break;

			#endif
			
		case "pointcloud":

			ParticleCloud.setLifeTime (0.5f);

			ParticleCloud.update ();



			break;

		case "initialise":
			
			if (GENERAL.STORYMODE == STORYMODE.VIEWER) {
				
				task.setIntValue ("customvalue", 191);
				task.setIntValue ("othervalue", 55);

				Debug.Log (me + "Setting value customvalue.");
			} 

			int v, v2;

			if (task.getIntValue ("customvalue", out v)) {
				task.getIntValue ("othervalue", out v2);
				Debug.Log (me + "Value customvalue: " + v);
				Debug.Log (me + "Value othervalue: " + v2);

				done = true;
			}

			break;


		default:

			done = true;

			break;

		}

		return done;

	}

	// default callibration values. c being a correction presumably and f the fov


	double fx_d = 1.0d / 5.9421434211923247e+02;
	double fy_d = 1.0d / 5.9104053696870778e+02;
	double cx_d = 3.3930780975300314e+02;
	double cy_d = 2.4273913761751615e+02;


 Vector3 depthToWorld (int x, int y, int depthValue){
		Vector3 result = Vector3.zero;

		//double depth = depthLookUp [depthValue];
		//float depth = rawDepthToMeters(depthValue);
		float depth = depthValue/1000f;

		result.x = (float)((x - cx_d) * depth * fx_d);
		result.y = (float)((y - cy_d) * depth * fy_d);
		result.z = (float)(depth);
		return result;

	}


	void Update ()
	{
		
	}

}
