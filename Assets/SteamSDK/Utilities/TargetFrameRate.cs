using UnityEngine;
using System.Collections;

public class TargetFrameRate : MonoBehaviour {

	public int frameRate = 30;

	// Use this for initialization
	void Start () {
		//Application.targetFrameRate = frameRate;
        //QualitySettings.vSyncCount = 2;
	}
}
