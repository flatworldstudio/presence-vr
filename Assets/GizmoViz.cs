using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoViz : MonoBehaviour {

    Vector3 AddEuler = new Vector3(0.25f,1f,0.5f);
    Vector2[] PerlinValue;
    Vector2[] PerlinAdd;
    //Vector2[] PerlinStart;
    float s=10;
    float ps=0.01f;

	// Use this for initialization
	void Start () {
		
        PerlinValue=new Vector2[3];
        PerlinAdd=new Vector2[3];
        //PerlinStart=new Vector2[3];

        for (int a=0;a<3;a++){
            PerlinValue[a] = new Vector2(Random.Range(-100,100),Random.Range(-100,100));
            //PerlinValue[a]=PerlinStart[a];
            PerlinAdd[a] = new Vector2(Random.Range(-ps,ps),Random.Range(-ps,ps));
        }


	}
	
	// Update is called once per frame
	void Update () {
		

        for (int a=0;a<3;a++){
            
            PerlinValue[a]+=PerlinAdd[a];
        }

        AddEuler = new Vector3(s*(-0.5f+Mathf.PerlinNoise(PerlinValue[0].x,PerlinValue[0].y)),s*(-0.5f+Mathf.PerlinNoise(PerlinValue[1].x,PerlinValue[1].y)),s*(-0.5f+Mathf.PerlinNoise(PerlinValue[2].x,PerlinValue[2].y)));

        Quaternion AddQuat =  Quaternion.Euler(AddEuler);


        Quaternion current = this.transform.localRotation;
        current = current* AddQuat;
        this.transform.localRotation=current;

        this.transform.localPosition=AddEuler/10f;


	}
}
