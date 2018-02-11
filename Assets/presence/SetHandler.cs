using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHandler : MonoBehaviour
{

	public SetController setController;
	public GameObject cloud;

	public GameObject kinectManagerObject;

	//	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	//KinectManager kinectManager;

	//	#endif


	ushort[] depthMap;
	int width, height;

	string me = "Task handler: ";

	int interval = -1;
	int interval2 = 0;
	Quaternion q;
	Vector3 p;
	GameObject c, g;

	float frameStamp;

	void Start ()
	{

		setController.addTaskHandler (TaskHandler);

		ParticleCloud.init (GameObject.Find ("Cloud"));

		//	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

		//	kinectManager = kinectManagerObject.GetComponent<KinectManager> ();

		//#endif
	}


	public bool TaskHandler (StoryTask task)
	{
		
		bool done = false;

		switch (task.description) {





		case "kinectcloud":

			ushort[] newFrame;
			int sample;
			int dataSize;
			Vector3 point;
			int particleIndex;

			ParticleSystem.Particle[] allParticles;
			ParticleSystem.Particle particle;

			if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL) {

				#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			
				if (interval==-1){
					
					frameStamp = Time.time;
					interval++;
				}



				if (interval == 4) {
					
					interval = 0;

					float frameDuration = Time.time - frameStamp;

					ParticleCloud.setLifeTime (frameDuration+ 0.015f);

					frameStamp = Time.time;

					depthMap = PRESENCE.pKinect.kinectManager.GetRawDepthMap ();

					width = PRESENCE.pKinect.kinectManager.getUserDepthWidth ();
					height = PRESENCE.pKinect.kinectManager.getUserDepthHeight ();

					sample = 8;

					dataSize = (height / sample) * (width / sample);

					particleIndex=0;

					newFrame = new ushort [dataSize];
					allParticles = new ParticleSystem.Particle[dataSize];

				//	allParticles = ParticleCloud.ps.GetParticles();

						
					PRESENCE.frame++;

					int dataIndex = 0;

					for (int y = 0; y < height; y += sample) {

						for (int x = 0; x < width; x += sample) {

							int i = y * width + x;



							newFrame [dataIndex] = (ushort)(depthMap [i]);

							dataIndex++;


							ushort userMap = (ushort)(depthMap [i] & 7);
							ushort userDepth = (ushort)(depthMap [i] >> 3);


							if (userMap != 0) {

								point = depthToWorld (x, y, userDepth);
								point.x = -point.x;

								point.y = -point.y;
								point.z += 0.05f;

							point.y += PRESENCE.kinectHeight;
								//point.y += 1.5f;

								//particle = new ParticleSystem.Particle ();
					//			allParticles[pi].position = point;
					//			allParticles[pi].startSize = 0.5f;
					//			allParticles[pi].startLifetime = 0.5f;
							//	particle.startSize =0.1f;
						//		particle.startLifetime=0.1f;
					//			particle.remainingLifetime=0.1f;
					//			allParticles[pi]=particle;
								particleIndex++;

								ParticleCloud.Emit (point);



							}

						

						}



					}

			//		task.setStringValue( "debug","f: "+PRESENCE.frame+" p: "+pi);

			//		ParticleCloud.SetParticles(allParticles,pi);


					task.setUshortValue ("frameData", newFrame);
					task.setIntValue ("frame", PRESENCE.frame);
					task.setIntValue ("frameSampleSize", sample);
					task.setIntValue ("frameWidth", width);
					task.setIntValue ("frameHeight", height);

				}


				interval++;


				#endif

			}

			if (GENERAL.AUTHORITY == AUTHORITY.LOCAL) {

			

				int getFrame;

				if (task.getIntValue ("frame", out getFrame)) {

					if (getFrame > PRESENCE.frame) {

						// newer frame available

						if (interval==-1){

							frameStamp = Time.time;
							interval++;
						}

						float frameDuration = Time.time - frameStamp;

						ParticleCloud.setLifeTime (frameDuration+ 0.015f);

						frameStamp = Time.time;

						task.getUshortValue ("frameData", out newFrame);
						task.getIntValue ("frameSampleSize", out sample);
						task.getIntValue ("frameWidth", out width);
						task.getIntValue ("frameHeight", out height);

//						ParticleCloud.setLifeTime (0.1f);

						int mx = width / sample;
						int my = height / sample;
						int i;

						particleIndex=0;

						for (int y = 0; y < my; y++) {

							for (int x = 0; x < mx; x++) {

								i = y * mx + x;

								//	newFrame [framei] = (ushort)(depthMap [i]);

								ushort userMap = (ushort)(newFrame [i] & 7);
								ushort userDepth = (ushort)(newFrame [i] >> 3);

								if (userMap != 0) {

									point = depthToWorld (x * sample, y * sample, userDepth);
									point.x = -point.x;
									point.y = -point.y;
									point.y += PRESENCE.kinectHeight;
									point.z += 0.05f;

									ParticleCloud.Emit (point);
									particleIndex++;
								}

							}

						} // end of plotting loop
							
					
						PRESENCE.frame = getFrame;
						task.setStringValue( "debug","f: "+PRESENCE.frame+" p: "+	particleIndex);


					}

				}

			}



			break;
	


		case "setdebug":

			c = GameObject.Find ("Compass");
			g = DebugObject.getNullObject (0.5f, 0.5f, 2.5f);
			g.transform.SetParent (c.transform, false);

			c = GameObject.Find ("Kinect");
			g = DebugObject.getNullObject (0.25f, 0.25f, 0.5f);
			g.transform.SetParent (c.transform, false);



			done = true;

			break;

		case "viewerdebug":

			c = GameObject.Find ("viewerCamera");
			g = DebugObject.getNullObject (0.25f, 0.25f, 0.5f);
			g.transform.SetParent (c.transform, false);

			c = GameObject.Find ("HandLeft");
			g = DebugObject.getNullObject (0.25f);
			g.transform.SetParent (c.transform, false);

			c = GameObject.Find ("HandRight");
			g = DebugObject.getNullObject (0.25f);
			g.transform.SetParent (c.transform, false);

			done = true;

			break;

		case "handdebug":

			c = GameObject.Find ("HandLeft");
			g = DebugObject.getNullObject (0.1f);
			g.transform.SetParent (c.transform, false);

			c = GameObject.Find ("HandRight");
			g = DebugObject.getNullObject (0.1f);
			g.transform.SetParent (c.transform, false);

			done = true;

			break;



		case "placeset":

			GameObject s = GameObject.Find ("SetHandler");
			GameObject vi = GameObject.Find ("viewerInterest");
			GameObject k = GameObject.Find ("Kinect");

			c = GameObject.Find ("Compass");

			PRESENCE.north = -PRESENCE.kinectHeading;

			c.transform.rotation = Quaternion.Euler (0, PRESENCE.north, 0);
			s.transform.rotation = c.transform.rotation;

			p = PRESENCE.kinectHomeDistance * Vector3.forward;

			s.transform.position = p;
			c.transform.position = p;

			p.y = vi.transform.position.y;

			vi.transform.position = p;

			p = Vector3.zero;
			p.y = PRESENCE.kinectHeight;

			k.transform.position = p;



			PRESENCE.pKinect.centered = true;

			done = true;

			break;



		case "placekinect":


			k = GameObject.Find ("Kinect");

		//	Vector3 p;
		//	Quaternion q;
			q = Quaternion.Euler (0, PRESENCE.kinectHeading, 0);


			k.transform.localRotation = q;

			p = (-1f * PRESENCE.kinectHomeDistance) * (q * Vector3.forward);

			p.y = PRESENCE.kinectHeight;

			k.transform.position = p;


			PRESENCE.pKinect.kinectPosition = p;

			PRESENCE.pKinect.kinectRotation = q;

			PRESENCE.pKinect.centered = false;

			done = true;

			break;


		case "nextdepth":

			IO.depthIndex++;

			if (IO.depthIndex == IO.savedDepthCaptures.Count) {
				IO.depthIndex = 0;
			}

			Debug.Log (me + "next depth");

			done = true;

			break;

		case "showdepthdata":

			if (IO.savedDepthCaptures.Count > 0) {

				interval2++;

				if (interval2 == 100) {
					interval2 = 0;

					IO.depthIndex++;

					if (IO.depthIndex == IO.savedDepthCaptures.Count) {
						IO.depthIndex = 0;
					}



				}



			}

			if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL) {
				


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
				if (task.getIntValue ("index", out ind))
					IO.depthIndex = ind;

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

					sample = 8;

					//	Vector3 point;

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


	Vector3 depthToWorld (int x, int y, int depthValue)
	{
		
		Vector3 result = Vector3.zero;

		//double depth = depthLookUp [depthValue];
		//float depth = rawDepthToMeters(depthValue);

		float depth = depthValue / 1000f;

		result.x = (float)((x - cx_d) * depth * fx_d);
		result.y = (float)((y - cy_d) * depth * fy_d);
		result.z = (float)(depth);

		return result;

	}


	void Update ()
	{
		
	}

}
