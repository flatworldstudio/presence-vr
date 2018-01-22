using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.IO;
using UnityEngine;


public class Script



{

	// COULD BE MADE STATIC.


	List <String> manuscript;
	public Boolean isReady;
	string me = "Script: ";

	public Script (string fileName)

	{

		GENERAL.storyPoints = new Dictionary <string,StoryPoint> ();

		GENERAL.storyPoints.Add ("end", new StoryPoint ("end")); // we use this storypoint for ending things, because it'll work over the network as well.



		isReady = false;

		if (Load (fileName)) {

			parse ();

		} else {
			
			Debug.LogWarning ("Script file didn't load.");

		}

		isReady = true;
			
	}


	void parse ()
	{
//
//		manuscript.Add ("\r");
//		manuscript.Add ("\r");


//		Debug.Log (me + "count: " + manuscript.Count);

		Dictionary <string,StoryPoint>  storyLines = new Dictionary <string,StoryPoint> ();

		StoryPoint point, previousPoint;
//		targetPoint;

		string currentStoryline="default";
		previousPoint = new StoryPoint ("");


		int l = 0;

		string storyLine, storyLabel, storyPoint;

		while (l < manuscript.Count) {
			
			storyLine=isStoryLine(l);
		
			if (storyLine != null)
				

			{
				l++;

				storyPoint=isStoryPoint(l);

				if (storyPoint != null) {
					
//					Debug.Log (me + "new storyline: " + storyLine);

					string[] task = getTask (manuscript[l]);
//					Debug.Log (me + "task: " + task[0]);

					point = new StoryPoint (storyLine, storyLine, task);

//					point.name = storyLine;
										
					storyLines.Add (storyLine, point);
					GENERAL.storyPoints.Add (storyLine, point);

					currentStoryline = storyLine;

					previousPoint = point;


				} else {

					Debug.LogWarning (me + "#storyline should be followed by storypoint");

				}

			} 

			storyLabel=isStoryLabel(l);

			if (storyLabel!=null)

			{
				l++;

				storyPoint=isStoryPoint(l);

				if (storyPoint != null) {

//					Debug.Log (me + "new storylabel: " + storyLabel);

					string[] task = getTask (manuscript[l]);
//					Debug.Log (me + "task: " + task[0]);

					point = new StoryPoint (storyLabel, currentStoryline, task);


//					point.name = storyLabel;
//					if (point.taskType != TASKTYPE.BASIC)
//						storyPoint = getUniqueName ();
					
//					storyLabels.Add (storyLabel, point);
					GENERAL.storyPoints.Add (storyLabel, point);

					previousPoint.setNextStoryPoint (point);
					previousPoint = point;

				} else {

					Debug.LogWarning (me + "@storylabel should be followed by storypoint");

				}

			} 

			storyPoint=isStoryPoint(l);

			if (storyLine==null && storyLabel==null && storyPoint != null) 

			{
//				Debug.Log (me + "storyPoint: "+l+" " + storyPoint);
				string[] task = getTask (manuscript[l]);
//				Debug.Log (me + "task: " + task[0]);

//				point = new StoryPoint (storyPoint, currentStoryline, task);
				point = new StoryPoint (UUID.getID (), currentStoryline, task);

//				if (point.taskType != TASKTYPE.BASIC) {
//					storyPoint = UUID.getId ();
//					point.ID = storyPoint;
//				}


//				storyPoints.Add (storyPoint, point);
				GENERAL.storyPoints.Add (point.ID, point);


				previousPoint.setNextStoryPoint (point);
				previousPoint = point;
			
			}




			l++;

		}
//		string[] keys = storyLines.Keys.Select(x => x.ToString()).ToArray();

//		Enumerable storyLineKeys = storyLines.Keys;


		string[] keys = storyLines.Keys.ToArray();


		for (int sl = 0; sl<keys.Count(); sl++) {

			string key = keys [sl];
//			Debug.Log ("Storyline key: " + key);


			StoryPoint thePoint;

			if (!storyLines.TryGetValue (key, out thePoint)) {
//				Debug.Log (me + "Can't find point");
			} else {

				while (thePoint.getNextStoryPoint () != null) {

//					Debug.Log (thePoint.ID + " | " + string.Join (" ", thePoint.task));
					thePoint = thePoint.getNextStoryPoint();
				}

//				Debug.Log (thePoint.ID + " | " + string.Join (" ", thePoint.task));




			}




//			storyPoint point = storyLines[keys[sl]];



//			getNextStoryPointName
		




		}





//		string s = string.Join(";", storyLines.Select(x => x.Key + " : " + x.Value.task[0]+ " -> " + x.Value.getNextStoryPointName()).ToArray());
//
//		Debug.Log (s);
//
//		s = string.Join(";", storyLabels.Select(x => x.Key + " : " + x.Value.task[0]+ " -> " + x.Value.getNextStoryPointName()).ToArray());
//
//		Debug.Log (s);
//
//		s = string.Join(";", storyPoints.Select(x => x.Key + " : " + x.Value.task[0]+ " -> " + x.Value.getNextStoryPointName()).ToArray());
//
//		Debug.Log (s);

	}



	string isStoryLine (int i) {

		string r = null;

		Char delimiter = '#';
		string[] s = manuscript [i].Split (delimiter);

		if (s.Length > 1) {
			r = s [1];
		}

		return r;


	}


	string isStoryLabel (int i) {

		string r = null;

		Char delimiter = '@';
		string[] s = manuscript [i].Split (delimiter);

		if (s.Length > 1) {
			r = s [1];
		}

		return r;


	}

	string isStoryPoint (int i) {

		string r = null;


		Char delimiter = ' ';
		string[] s = manuscript [i].Split (delimiter);
		r = s [0];

		if (r.Equals ("")) {
			r = null;
//			Debug.Log ("empty!!");
		}

		return r;


	}





	private string getElementFromLine (string line, int i)
	{
		string[] split = splitLine (line);
		if (split.Length > i) {
			return split [i];
		} else {
			//				Debug.Log ("Empty line");
			return "null";
		}
	}

	private string[] getTask (string line)
	{
		string[] s;

		s = splitLine (line);

		//			Debug.Log (s [1]);
//		if (s.Length > 1) {
//			r = new string[s.Length - 1];
//
//			Array.Copy (s, 1, r, 0, s.Length - 1);
//			//				Debug.Log (r [0]);
//			return r;
//		} else {
//			r = new string[0];
//			return r;
//		}

		return s;
	}





	private string[] splitLine (string line)
	{
		Char delimiter = ' ';
		string[] r = line.Split (delimiter);
		return r;
	}

	private bool Load (string fileName)
	{
		manuscript = new List<string> ();

		TextAsset mytxtData = (TextAsset)Resources.Load (fileName);

		if (mytxtData != null) {
			string txt = mytxtData.text;
			string[] lines = txt.Split (new string[] { "\r\n", "\n" }, StringSplitOptions.None);
			manuscript.AddRange (lines);
			return true;
		} else {

			return false;
		}

	}

}

public class StoryPoint
{
	public string ID;
	public string storyLineName;
	public string[] task;

	public TASKTYPE taskType;
	StoryPoint nextStoryPoint;

	public StoryPoint (string myName)
	{
		ID = myName;
		storyLineName = "...";

		if (ID.Equals ("end")) {
			
			task = new string[] {"end"};
			taskType=TASKTYPE.END;

		} else {
			
			task = new string[] {"none"};
			taskType = TASKTYPE.BASIC;
		}

	}

	public StoryPoint (string myName, string myStoryLine, string[] myTask)
	{
		ID = myName;
		storyLineName = myStoryLine;

		task = myTask;

		switch (task [0]) {

		case "start":
		case "stop":
		case "tell":
		case "goto":
//		case "end":
		case "hold":
			
			taskType = TASKTYPE.ROUTING;
			break;

		case "end":

			taskType = TASKTYPE.END;
			break;
		default:
			
			taskType = TASKTYPE.BASIC;
			break;

		}

	}

	public void setNextStoryPoint (StoryPoint myNextStoryPoint)
	{
	
		nextStoryPoint = myNextStoryPoint;

	}

	public StoryPoint getNextStoryPoint ()
	{

		return  nextStoryPoint;

	}

	public string getNextStoryPointName ()
	{
	
		if (nextStoryPoint != null) {

			return nextStoryPoint.ID;
		
		} else {
		
			return ("no next point");

		}

	}

}
