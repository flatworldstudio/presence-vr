
using UnityEngine;

public static class ParticleCloud  {
	
	static ParticleSystem ps;
	static ParticleSystem.EmitParams emitParams;
	static GameObject e1,e2;
	static float r1,r2;
	static int count=0;

	// Use this for initialization
	public static void init (GameObject particleSystem, GameObject emitter1,GameObject emitter2) {

		e1 = emitter1;
		e2 = emitter2;

		ps = particleSystem.GetComponent<ParticleSystem> ();
		emitParams = new ParticleSystem.EmitParams();
		r1 = 0;
		r2 = 0;
	}



	// Update is called once per frame
	public static void update () {


		if (count == 2) {
			
			DoEmit ();

			count = 0;
		}

		count++;

	}

//	static float b=0f;

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
