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

		#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN


		case "startkinect":


			kinect.SetActive (true);

			done = true;

			break;

		case "nextdepth":

			IO.depthIndex++;

			if (IO.depthIndex==IO.savedDepthCaptures.Count){
				IO.depthIndex=0;
			}

			done = true;
			break;

		case "showdepthdata":
			
			if (IO.depthIndex >= 0) {
				
//				int index = 0;
//
//				if (!task.getIntValue ("index", out index)) {
//
//					task.setIntValue ("index", 0);
//				}

//				int index = IO.depthIndex;
				task.setStringValue("debug",""+IO.depthIndex);

				DepthCapture.current = IO.savedDepthCaptures [IO.depthIndex];

				ParticleCloud.setLifeTime (0.1f);

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
								point = kinectManager.depthToWorld (x, y, userDepth);
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
							point = kinectManager.depthToWorld (x, y, userDepth);
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

	void Update ()
	{
		
	}

}
