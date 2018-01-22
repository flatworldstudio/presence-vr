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

		case "startkinect":

			#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

			kinect.SetActive (true);

			#else

			Debug.LogError(me + "Kinect only available on windows platform.");

			#endif

			done = true;

			break;


		case "kinect":

			#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

			ParticleCloud.setLifeTime (0.1f);

			ushort[] depthMap;

			if (interval == 4) {
				interval = 0;

				depthMap = kinectManager.GetRawDepthMap ();

				int width = kinectManager.getUserDepthWidth ();
				int height = kinectManager.getUserDepthHeight ();



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


			#else

			Debug.LogError(me + "Kinect only available on windows platform.");

			#endif


			break;

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
