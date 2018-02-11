using UnityEngine;

public static class ParticleCloud  {

	public static ParticleSystem ps;
	public static ParticleSystem.EmitParams emitParams;

	static GameObject e1,e2;
	static float r1,r2;
	static int count=0;


	public static void init (GameObject particleSystem) {
		
		ps = particleSystem.GetComponent<ParticleSystem> ();

		emitParams = new ParticleSystem.EmitParams();

		var main = ps.main;
		main.maxParticles = 5000;

		e1 = GameObject.Find("e1");
		e2 =  GameObject.Find("e2");
		r1 = 0;
		r2 = 0;

	}

	public static void SetParticles (ParticleSystem.Particle [] particles, int number){



		ps.SetParticles (particles, number);



	}


	public static void Emit (Vector3 pos){


		emitParams.position =pos;
		ps.Emit(emitParams, 1);



	}
	public static void setLifeTime (float value){

		var main = ps.main;

	main.startLifetime=value;

	}

	public static void update () {


		if (count == 2) {

			DoEmit ();

			count = 0;
		}

		count++;

	}

	static void DoEmit()
	{




		Quaternion rot = Quaternion.Euler (new Vector3 (r2-90f, 0, 0));

		e1.transform.localRotation = rot;


		Vector3 dotPosition =  Vector3.zero;

		//		emitParams.position = new Vector3(0.0f, 0.0f, 0.0f);

		for (int p = -15; p < 15; p++) {

			float a = Mathf.PI * 2 /60 *p;

			dotPosition.x = Mathf.Sin (a) * 5;
			dotPosition.y = Mathf.Cos (a) * 5;

			e2.transform.localPosition = dotPosition;

			emitParams.position = e2.transform.position;

			ps.Emit(emitParams, 1);

		}


		r2 += 5f;
		r2 = r2 % 180f;


		//		r1+=0.
	}

}

