﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StoryEngine;
using UnityScript.Lang;
using PresenceEngine;
using System;
using UnityEngine.Serialization;

public class ConfineMent : MonoBehaviour
{
    public AudioSource signalSound;
    Collider[] colliders;
    public GameObject ColliderObject;
    public GameObject[] CornerPoints;
    ParticleCloud[] Cloud;
    bool[] CloudVisible;
    int[] Barrier;
    float[] Height;

    Gradient g;

    private void Start()
    {
       
    }
    private void StartOLD()
    {

        Color32 Color01 = new Color32(137, 223, 255, 255);
        Color32 Color02 = new Color32(137, 223, 161, 255);


        GradientColorKey[] gck;
        GradientAlphaKey[] gak;
        g = new Gradient();
        gck = new GradientColorKey[2];
        gck[0].color = Color01;
        gck[0].time = 0.0F;
        gck[1].color = Color02;
        gck[1].time = 1.0F;
        gak = new GradientAlphaKey[2];
        gak[0].alpha = 1.0F;
        gak[0].time = 0.0F;
        gak[1].alpha = 0.1F;
        gak[1].time = 1.0F;
        g.SetKeys(gck, gak);



        Cloud = new ParticleCloud[2];
        CloudVisible = new bool[2];
        Barrier = new int[2];
        Height = new float[2];

        colliders = ColliderObject.GetComponents<Collider>();

        for (int b = 0; b < 2; b++)
        {
            //Cloud[b] = new ParticleCloud(41*21, "Cloud", true);
            Cloud[b] = new ParticleCloud(41*41, "Cloud", true);

            var ex=   Cloud[b].ps.externalForces;
            ex.enabled = false;
            //ex.multiplier = 0.25f;

            var ns =  Cloud[b] .ps.noise;
            ns.enabled =false;

            Cloud[b].initialEmit(41*41);


            Cloud[b].ApplyParticles(0);
            Cloud[b].CloudObject.transform.SetParent(ColliderObject.transform, false);
            CloudVisible[b] = false;
            Barrier[b] = -1;
            Height[b] = 0;
        }


    }


    private void Update()
    {






    }

    //private void OnCollisionEnter()
    //{
    //       Debug.Log("ENTER");

    //       GENERAL.UserInConfinedArea=false;
    //}


    //private void OnCollisionExit()
    //{

    //    Debug.Log("EXIT");

    //    GENERAL.UserInConfinedArea=true;

    //}

    int AvailableCloudIndex()
    {

        for (int b = 0; b < Cloud.Length; b++)
        {

            if (!CloudVisible[b] )
                return b;

        }

        return -1;

    }

    int CloudIndexOf(int barrier)
    {

        for (int b = 0; b < Cloud.Length; b++)
        {

            if (Barrier[b] == barrier)
                return b;

        }

        return -1;

    }

    void BuildBarrier(int barrier, int barrierCloudIndex,float alpha=1f)
    {

        Barrier[barrierCloudIndex] = barrier;

        ParticleCloud cloud = Cloud[barrierCloudIndex];

        bool visible = CloudVisible[barrierCloudIndex];


        // -2 to 2

        /*
        var ex=  cloud.ps.externalForces;
        ex.enabled = true;
        ex.multiplier = 0.25f;

        var ns = cloud.ps.noise;
        ns.enabled =true;
       
        cloud.initialEmit(cloud.allParticles.Length);
*/


        // baseline for the barrier
        //Vector3 base0 = new Vector3(-2f, 0.05f, -2f);
        //Vector3 base1 = new Vector3(2f, 0.05f, -2f);

        Vector3 base0 = CornerPoints[barrier % CornerPoints.Length].transform.localPosition;
        Vector3 base1 = CornerPoints[(barrier + 1) % CornerPoints.Length].transform.localPosition;
        //Debug.Log(base0.ToString() + " " + base1.ToString());


        // number of points along baseline
        int across = 40;
        // number of point up (same distance between points as on baseline)

        float up = Height[barrierCloudIndex];

        //float up = 0;
        ////int div=0;
        //for (int b=0;b<Cloud.Length;b++){
        //    if (CloudVisible[b]){

        //        up=Mathf.Max(up,Height[b]);
        //        //div++;
        //    }


        //}

        //up = div>0?up/div:0;


        float distance = Vector3.Distance(base0, base1) / across;
        float step = 1f / across;
        Vector3 point;
        int p = 0;

        Color multiply = new Color32(255,255,255,(byte)(alpha*255));


        for (float y = 0; y < up; y += distance)
        {

            Color pointColor = g.Evaluate(1f - y / up);
            pointColor=pointColor*multiply;

            for (float t = step; t < 1; t += step)
            {

                point = Vector3.Lerp(base0, base1, t);
                point.y += y;

                cloud.allParticles[p].position = point;
                cloud.allParticles[p].startSize = 0.025f;
                //cloud.allParticles[p].startSize = 0.0075f;
                //barrier.allParticles[p].startColor = Color.blue;
                cloud.allParticles[p].startLifetime = 2f;

                cloud.allParticles[p].startColor = pointColor;
                //cloud.allParticles[p].externalForces
                //cloud.allParticles[p].gravit = pointColor;


                p++;



            }
        }


        p = Mathf.Min(p, cloud.allParticles.Length);// clamp number

        cloud.ApplyParticles(visible ? p : 0);

        //Debug.Log(" " + visible + " " + p);
    }




    void OnTriggerEnter(Collider other)
    {

        signalSound.Play();
        SETTINGS.UserInConfinedArea=false;
    }

    void OnTriggerExit(Collider other)
    {

        SETTINGS.UserInConfinedArea=true;

    }

    void OnTriggerEnterOLD(Collider other)
    {

        int barrier = System.Array.IndexOf(colliders, other);

        //Debug.LogWarning("ENTER " + barrier);
        //GENERAL.UserInConfinedArea = false;

        int index = AvailableCloudIndex();

        CloudVisible[index] = true;
        Height[index]=2f;

        var ex=   Cloud[index].ps.externalForces;
        ex.enabled = false;
        //ex.multiplier = 0.25f;

        var ns =  Cloud[index] .ps.noise;
        ns.enabled =false;

        Cloud[index].initialEmit(Cloud[index].allParticles.Length);

        BuildBarrier(barrier, index,0f);

    }


    private void OnTriggerStayOLD(Collider other)
    {

       
        int barrier = System.Array.IndexOf(colliders, other);

        int index = CloudIndexOf(barrier);

        if (index >= 0 && index < Cloud.Length)
        {
            Vector3 closestPoint = other.ClosestPoint(transform.position);
            float d = Vector3.Distance(closestPoint, transform.position);
            //Height[index] = (1f-Vector3.Distance(closestPoint, transform.position)) * 2f;// player collider radius is 1
            BuildBarrier(barrier, index,Mathf.Clamp(1f-d,0f,1f));//rebuild

        }



        //Debug.Log(Vector3.Distance(closestPoint,transform.position));





    }
    //Vector3 closestPoint;
    //public void OnDrawGizmos()
    //{
    //    if (closestPoint!=null)
    //    Gizmos.DrawSphere(closestPoint, 0.1f);
    //}

    void OnTriggerExitOLD(Collider other)
    {


        SETTINGS.UserInConfinedArea = true;


        int barrier = System.Array.IndexOf(colliders, other);

        //Debug.LogWarning("EXIT " + barrier);

        int index = CloudIndexOf(barrier);




        if (index >= 0 && index < Cloud.Length)
        {


            /*
            var ex=  Cloud[index].ps.externalForces;
            ex.enabled = true;
            ex.multiplier = 0.25f;

            var ns = Cloud[index].ps.noise;
            ns.enabled =true;

            Cloud[index].initialEmit(Cloud[index].allParticles.Length);

            BuildBarrier(barrier, index,0.1f);

*/

            CloudVisible[index] = false;
            //Height[index]=0;
            Cloud[index].ApplyParticles(0);

        }


    }



}
