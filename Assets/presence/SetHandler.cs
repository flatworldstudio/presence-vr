using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHandler : MonoBehaviour {

	public SetController setController;


	string me = "Task handler: ";

	void Start () {

		setController.addTaskHandler (TaskHandler);

		ParticleCloud.init(GameObject.Find("Cloud"),GameObject.Find("e1"),GameObject.Find("e2"));

	}

	public bool TaskHandler (StoryTask task){
		
		bool done = false;

		switch (task.description) {


		case "pointcloud":


			ParticleCloud.update ();



			break;

		case "initialise":
			
			if (GENERAL.STORYMODE==STORYMODE.VIEWER) {
				
				task.setIntValue ("customvalue", 191);
				task.setIntValue ("othervalue", 55);

				Debug.Log (me + "Setting value customvalue.");
			} 

			int v,v2;

			if (task.getIntValue ("customvalue",out v)) {
				task.getIntValue ("othervalue", out v2);
				Debug.Log (me+"Value customvalue: " + v);
				Debug.Log (me+"Value othervalue: " + v2);

				done = true;
			}

			break;


		default:

			done = true;

			break;

		}

		return done;

	}
	
	void Update () {
		
	}

}
