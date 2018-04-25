using UnityEngine;

public class ParticleCloud
{

    public ParticleSystem ps;
    public ParticleSystem.EmitParams emitParams;
    public GameObject CloudObject;

    GameObject e1, e2;
    float r1, r2;
    int count = 0;

    public ParticleSystem.Particle[] allParticles;

    public ParticleCloud(int size)
    {

        // CLONE CLOUD
        CloudObject = Object.Instantiate(Resources.Load("Cloud", typeof(GameObject))) as GameObject;

        ps = CloudObject.GetComponent<ParticleSystem>();
        emitParams = new ParticleSystem.EmitParams();

        var main = ps.main;
        main.maxParticles = size;

        // Emit a batch of particles and stash them.
        initialEmit(size);
        allParticles = new ParticleSystem.Particle[size];
        ps.GetParticles(allParticles);

    }









    public void init(GameObject particleSystem)
    {

        ps = particleSystem.GetComponent<ParticleSystem>();

        emitParams = new ParticleSystem.EmitParams();

        var main = ps.main;
        main.maxParticles = 5000;

        e1 = GameObject.Find("e1");
        e2 = GameObject.Find("e2");
        r1 = 0;
        r2 = 0;

    }

    public void ApplyParticles(int number)
    {

        ps.SetParticles(allParticles, number);

    }

    public void SetParticles(ParticleSystem.Particle[] particles, int number)
    {



        ps.SetParticles(particles, number);



    }

    public void initialEmit(int number)
    {

        emitParams = new ParticleSystem.EmitParams();

        for (int p = 0; p < number; p++)
        {

            Vector3 pos = new Vector3((Random.value - 0.5f) * 5f, Random.value * 2.5f, (Random.value - 0.5f) * 5f);
         //   pos = Vector3.zero;

            emitParams.position = pos;
            ps.Emit(emitParams, 1);
        }





    }

    public void Emit(Vector3 pos)
    {


        emitParams.position = pos;
        ps.Emit(emitParams, 1);



    }
    public void setLifeTime(float value)
    {

        var main = ps.main;

        main.startLifetime = value;

    }

    public void update()
    {


        if (count == 2)
        {

            DoEmit();

            count = 0;
        }

        count++;

    }

    void DoEmit()
    {




        Quaternion rot = Quaternion.Euler(new Vector3(r2 - 90f, 0, 0));

        e1.transform.localRotation = rot;


        Vector3 dotPosition = Vector3.zero;

        //		emitParams.position = new Vector3(0.0f, 0.0f, 0.0f);

        for (int p = -15; p < 15; p++)
        {

            float a = Mathf.PI * 2 / 60 * p;

            dotPosition.x = Mathf.Sin(a) * 5;
            dotPosition.y = Mathf.Cos(a) * 5;

            e2.transform.localPosition = dotPosition;

            emitParams.position = e2.transform.position;

            ps.Emit(emitParams, 1);

        }


        r2 += 5f;
        r2 = r2 % 180f;


        //		r1+=0.
    }

}

