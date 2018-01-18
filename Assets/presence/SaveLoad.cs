using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;

[System.Serializable]

public class Capture
{

	public static Capture current;
	public static bool capturing=false;
	public static bool playing=false;

//	public Character knight;
//	public Character rogue;
//	public Character wizard;

//	public Vector3[] position;
//	public Quaternion[] orientation;
	public Frame[] frames;
	int size =250;

	int i;

	public Capture ()
	{
//		i = 0;

		frames = new Frame[size];

//		orientation = new Quaternion [1000];

//		knight = new Character ();
//		rogue = new Character ();
//		wizard = new Character ();
	}
	public void capture ()
	{
		Capture.capturing = true;
		i = 0;

	}
	public void play (){
		Capture.playing = true;
		i = 0;

	}

	public bool read (out Frame f) {

		if (i == size) {
			f = new Frame ();
			return false;

		} else {

			 f =  frames [i];
			i++;
			return true;
		}


	}

	public bool log (Vector3 pos, Quaternion orient){

		if (i == size) {
			return false;
		} else {
			Frame f = new Frame (pos,orient);
//			f.position = pos;
//			f.orientation = orient;

			frames [i] = f;
			i++;
			return true;
		}

	}

}


[System.Serializable] 
public class Frame
{
	public float[] position;
	public float[] orientation;

//	public string name;

	public Frame (Vector3 pos, Quaternion orient)
	{
		position = new float[3];

		position[0]=pos.x;
		position[1]=pos.y;
		position[2]=pos.z;

		orientation = new float[4];

		orientation [0] = orient [0];
		orientation [1] = orient [1];
		orientation [2] = orient [2];
		orientation [3] = orient [3];


	}

	public Vector3 getPosition() {
		return new Vector3 (position [0], position [1], position [2]);

	}

	public Quaternion getRotation (){
		return new Quaternion (orientation[0],orientation[1],orientation[2],orientation[3]);

	}

	public Frame ()
	{
				
	}
}


public static class SaveLoad {

	public static List<Capture> savedCaptures = new List<Capture>();

	public static void Save() {

		savedCaptures.Add(Capture.current);

		BinaryFormatter bf = new BinaryFormatter();

		FileStream file = File.Create (Application.persistentDataPath + "/savedGames.gd");

		bf.Serialize(file, SaveLoad.savedCaptures);

		file.Close();

	}


	public static void Load() {
		if(File.Exists(Application.persistentDataPath + "/savedGames.gd")) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
			SaveLoad.savedCaptures = (List<Capture>)bf.Deserialize(file);
			file.Close();
		}
	}


}