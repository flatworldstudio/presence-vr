using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightControl : MonoBehaviour {

    [HideInInspector]
    public float Master,Variation;
    //public float PerlinStart,PerlinValue;

    float vel;
    Light thisLight;
	void Start () {

        thisLight=GetComponent<Light>();
        Master=thisLight.intensity;
        Variation=1f;

        //value= Random.Range(-100f,100f);

	}
    //float value;
	// Update is called once per frame



	void Update () {

        thisLight.intensity=Mathf.SmoothDamp(thisLight.intensity,Master*Variation,ref vel,0.5f);

        //GetComponent<Light>().intensity =1.5f* Mathf.PerlinNoise(value,0);
        //value+=0.05f;


	}
}
