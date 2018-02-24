using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SetHandler : MonoBehaviour
{

	public SetController setController;

	//public GameObject cloud;

	public GameObject kinectManagerObject;

	ParticleCloud[] clouds;

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

	float serverFrameStamp, clientFrameStamp;
	float serverFrameRate, clientFrameRate;


	float targetFrameRate = 0.15f;
	float safeFrameRate = 0.1f;

	int droppedFrames = 0;




	void Start ()
	{

		setController.addTaskHandler (TaskHandler);


		//	ParticleCloud.init (GameObject.Find ("Cloud"));



		//	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

		//	kinectManager = kinectManagerObject.GetComponent<KinectManager> ();

		//#endif
	}
	//	ParticleSystem.Particle[] allParticles;
	int dataSize;
	ushort[] newFrame;
	int sample;
	//	int dataSize;
	Vector3 point;

	int particleIndex;
	int count;
	ParticleCloud cloud, mirror;


	public bool TaskHandler (StoryTask task)
	{
		
		bool done = false;

		switch (task.description) {


		case "createcloud":

			clouds = new ParticleCloud[2];

			clouds [0] = new ParticleCloud (2500);
			clouds [1] = new ParticleCloud (2500);

			PRESENCE.PointCloud = new Vector3[2500];


			done = true;

			break;

		case "createsinglecloud":

			clouds = new ParticleCloud[1];

			clouds [0] = new ParticleCloud (2500);

			PRESENCE.PointCloud = new Vector3[2500];


			done = true;

			break;


		case "playsequence":

			if (Time.time - PRESENCE.TimeStamp > 0.04f) {

				PRESENCE.TimeStamp = Time.time;

				CloudFrame currentFrame = PRESENCE.capture.Frames [PRESENCE.CaptureFrame];

				int pointCount = currentFrame.Points.Length;


				for (int p = 0; p < pointCount; p++) {

					clouds[0].allParticles [p].position = currentFrame.Points [p];

				}
	

				clouds [0].ApplyParticles (pointCount);

				PRESENCE.CaptureFrame++;

				if (PRESENCE.CaptureFrame == PRESENCE.capture.Frames.Length) {

					PRESENCE.CaptureFrame = 0;
				}

				task.setStringValue ("debug", "" + PRESENCE.CaptureFrame);

			}

			break;

		case "capture":

			if (Time.time - PRESENCE.TimeStamp > 0.04f) {
				
				PRESENCE.TimeStamp = Time.time;

				CloudFrame newFrame = new CloudFrame (PRESENCE.FrameSize);

				Array.Copy (PRESENCE.PointCloud, newFrame.Points, PRESENCE.FrameSize);

				PRESENCE.capture.Frames [PRESENCE.CaptureFrame] = newFrame;

				PRESENCE.CaptureFrame++;

			//	Debug.Log (PRESENCE.CaptureFrame + " " + PRESENCE.capture.Frames.Length);

				if (PRESENCE.CaptureFrame == PRESENCE.capture.Frames.Length) {

					done = true;

				}

				task.setStringValue ("debug", "" + PRESENCE.CaptureFrame);


			}


			break;


		case "kinectcloud":
			
			string init;

			if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL) {

				#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	

				if (!task.getStringValue ("init", out init)) {
					task.setStringValue ("init", "done");

					cloud = clouds [0];
					mirror = clouds [1];

					serverFrameStamp = Time.time - targetFrameRate;
					clientFrameStamp = Time.time - targetFrameRate;

					sample = 8;

					width = PRESENCE.pKinect.kinectManager.getUserDepthWidth ();
					height = PRESENCE.pKinect.kinectManager.getUserDepthHeight ();
					dataSize = (height / sample) * (width / sample);



				}

					if (!task.getFloatValue ("clientFrameRate", out clientFrameRate))
						clientFrameRate	= targetFrameRate;

					serverFrameRate = Mathf.Lerp (serverFrameRate, Time.time - serverFrameStamp, 0.1f);

					serverFrameStamp = Time.time;

					depthMap = PRESENCE.pKinect.kinectManager.GetRawDepthMap ();

					particleIndex = 0;

					newFrame = new ushort [dataSize];

					int dataIndex = 0;

					for (int y = 0; y < height; y += sample) {

						for (int x = 0; x < width; x += sample) {

							int i = y * width + x;

							newFrame [dataIndex] = (ushort)(depthMap [i]);

							ushort userMap = (ushort)(depthMap [i] & 7);
							ushort userDepth = (ushort)(depthMap [i] >> 3);

							if (userMap != 0) {

								point = depthToWorld (x, y, userDepth);
								point.x = -point.x;

								point.y = -point.y;
								point.z += 0.05f;

								point.y += PRESENCE.kinectHeight;

								cloud.allParticles [particleIndex].position = point;
							PRESENCE.PointCloud[particleIndex] = point;


								point.z *= -1;
								point.z += 2;

								mirror.allParticles [particleIndex].position = point;


								particleIndex++;

							}

							dataIndex++;

						}

					}


					cloud.ApplyParticles (particleIndex);
					mirror.ApplyParticles (particleIndex);
				PRESENCE.FrameSize=particleIndex;

			

					//task.setStringValue ("debug",""+particleIndex );

					if (Time.time - clientFrameStamp >= targetFrameRate) {

						clientFrameStamp = Time.time;

						task.setUshortValue ("frameData", newFrame);
						task.setIntValue ("frame", PRESENCE.frame);

						task.setIntValue ("frameSampleSize", sample);
						task.setIntValue ("frameWidth", width);
						task.setIntValue ("frameHeight", height);

						PRESENCE.frame++;

					}

					// only send at the interval the client can more or less handle

					//	if (clientFrameRate>targetFrameRate)





					int droppedFrames = 0;

					task.getIntValue ("dropped", out droppedFrames);

					if (droppedFrames == 0) {

						targetFrameRate = Mathf.Lerp (targetFrameRate, safeFrameRate, 0.01f);

					} else {

						safeFrameRate += 0.01f;

						targetFrameRate += 0.01f;

					}


					task.setStringValue ("debug", "s: " + Mathf.Round (100f * serverFrameRate) + " c: " + Mathf.Round (100f * clientFrameRate) + " t: " + Mathf.Round (100f * targetFrameRate) + " sf: " + Mathf.Round (100f *	safeFrameRate) + " d: " + droppedFrames);






















				#endif

			}



			


			//	if (interval == 1) {
					
			//		interval = 0;
			

					


			//	interval++;




			//}

			if (GENERAL.AUTHORITY == AUTHORITY.LOCAL) {

			

				int getFrame;

				if (task.getIntValue ("frame", out getFrame)) {
					

					task.getFloatValue ("targetFrameRate", out targetFrameRate);


					if (getFrame > PRESENCE.frame) {

						// newer frame available

						if (clientFrameStamp == 0) {
							
							clientFrameStamp = Time.time - targetFrameRate; // we start perfectly on rate.

						}
						//	clientFrameRate = Time.time - clientFrameStamp;
						clientFrameRate = Mathf.Lerp (clientFrameRate, Time.time - clientFrameStamp, 0.1f);

						clientFrameStamp = Time.time;

//						frameRateDeviation = Time.time - clientFrameStamp;


//						if (interval==-1){
//
//							clientFrameStamp = Time.time;
//							interval++;
//						}

//						clientFrameDuration = Time.time - clientFrameStamp;

						cloud.setLifeTime (clientFrameRate + 0.015f);


						task.getUshortValue ("frameData", out newFrame);
						task.getIntValue ("frameSampleSize", out sample);
						task.getIntValue ("frameWidth", out width);
						task.getIntValue ("frameHeight", out height);

//						ParticleCloud.setLifeTime (0.1f);

						int mx = width / sample;
						int my = height / sample;
						int i;

						particleIndex = 0;

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

									cloud.Emit (point);
									particleIndex++;
								}

							}

						} // end of plotting loop
							
					

						droppedFrames = getFrame - PRESENCE.frame - 1;

						PRESENCE.frame = getFrame;

						task.setIntValue ("dropped", droppedFrames);

						task.setFloatValue ("clientFrameRate", clientFrameRate);

						//			task.setStringValue( "debug","f: "+PRESENCE.frame+" p: "+	particleIndex);


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

			cloud = clouds [0];

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

								cloud.Emit (point);
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
			cloud = clouds [0];
			cloud.setLifeTime (0.5f);

			cloud.update ();



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
