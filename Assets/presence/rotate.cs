using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour {
    float r = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Quaternion rot = Quaternion.Euler(r * 2, 0, r);
        this.transform.localRotation = rot;
        r+= 0.5f;
	}
}
