using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate bool UserTaskHandler (StoryTask theTask);
	
public class UserController : MonoBehaviour
{
	
	GameObject StoryEngineObject;
	UserTaskHandler userTaskHandler;

	AssitantDirector ad;
	bool handlerWarning = false;

	public List <StoryTask> taskList;

	string me = "User controller: ";

	void Start ()
	{
		Debug.Log (me + "Starting...");

		taskList = new List <StoryTask> ();

		StoryEngineObject = GameObject.Find ("StoryEngineObject");

		if (StoryEngineObject == null) {

			Debug.LogWarning ("StoryEngineObject not found.");

		} else {
			
			ad = StoryEngineObject.GetComponent <AssitantDirector> ();
			ad.newTasksEvent += new NewTasksEvent (newTasksHandler); // registrer for task events

		}

	}

	public void addTaskHandler (UserTaskHandler theHandler)
	{
		userTaskHandler = theHandler;
		Debug.Log (me + "Handler added");
	}


	void Update ()
	{
		
		int t = 0;

		while (t < taskList.Count) {

			StoryTask task = taskList [t];

			if (task.pointer.getStatus () == POINTERSTATUS.KILLED && task.description != "end") {

				taskList.RemoveAt (t);

			} else {

				if (userTaskHandler != null) {

					if (userTaskHandler (task)) {

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

	public void addTasks (List<StoryTask> theTasks)
	{
		taskList.AddRange (theTasks);
	}

}




