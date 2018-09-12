using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PresenceEngine
{
    public class Circle : MonoBehaviour
    {
        ParticleCloud Cloud;
       public Vector3 center;
        public float radius;

        float a1, a2;
     public   bool IsDrawing = false;
        Vector3 e1 = Vector3.zero;
        Vector3 e2 = Vector3.zero;

        public static Circle Instance;
        // Use this for initialization
        void Start()
        {
            Instance = this;
            a1 = a2 = 0;

            Cloud = new ParticleCloud(2500, "CloudDyn", false);
            Cloud.CloudObject.transform.SetParent(this.transform, false);
        }

        // Update is called once per frame
        void Update()
        {
            if (IsDrawing)
            {
                //   Vector3 center = Vector3.zero;


                Vector3 e1n = center + new Vector3(Mathf.Sin(a1) * radius, 0, Mathf.Cos(a1) * radius);
                Vector3 e2n = center + new Vector3(Mathf.Sin(a2) * radius, 0, Mathf.Cos(a2) * radius);

                if (e1!=Vector3.zero && e2 != Vector3.zero)
                {
                    
                    for (float i = 0; i < 1; i += 0.25f)
                    {
                        Cloud.Emit(Vector3.Lerp(e1, e1n, i));
                        Cloud.Emit(Vector3.Lerp(e2, e2n, i));
                    }
                }

                e1 = e1n;
                e2 = e2n;

                a1 += 0.05f;
                a2 -= 0.05f;
            }



        }

        public void StartDrawing()
        {
            IsDrawing = true;
        }

        public void StopDrawing()
        {
            IsDrawing = false;
        }



    }
}