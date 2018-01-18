using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate bool SetTaskHandler (Task theTask);
	
public class SetController : MonoBehaviour
{
	
	GameObject StoryEngineObject;
	SetTaskHandler setTaskHandler;

	AssitantDirector ad;
	bool handlerWarning = false;

	public List <Task> taskList;

	string me = "Set controller: ";

	void Start ()
	{
		Debug.Log (me + "Starting...");

		taskList = new List <Task> ();

		StoryEngineObject = GameObject.Find ("StoryEngineObject");

		if (StoryEngineObject == null) {

			Debug.LogWarning ("StoryEngineObject not found.");

		} else {
			
			ad = StoryEngineObject.GetComponent <AssitantDirector> ();
			ad.newTasksEvent += new NewTasksEvent (newTasksHandler); // registrer for task events

		}

	}

	public void addTaskHandler (SetTaskHandler theHandler)
	{
		setTaskHandler = theHandler;
		Debug.Log (me + "Handler added");
	}


	void Update ()
	{
		
		int t = 0;

		while (t < taskList.Count) {

			Task task = taskList [t];

			if (task.pointer.getStatus () == POINTERSTATUS.KILLED && task.description != "end") {

				taskList.RemoveAt (t);

			} else {

				if (setTaskHandler != null) {

					if (setTaskHandler (task)) {

						task.signOff (me);
						taskList.RemoveAt (t);

					} else {
						t++;

					}

				} else {

					if (!handlerWarning) {
						Debug.LogWarning (me + "No handler available, blocking task while waiting.");
						handlerWarning = true;
						t++;
					} 

				}

			}

		}

	}

	void newTasksHandler (object sender, TaskArgs e)
	{
		addTasks (e.theTasks);

	}

	public void addTasks (List<Task> theTasks)
	{
		taskList.AddRange (theTasks);
	}

}




