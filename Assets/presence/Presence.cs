using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PresenceEngine
{
    public class Presence : MonoBehaviour
    {
       public iVisualiser Visualiser;
        public DepthTransport DepthTransport;
    

        static public Presence Create(GameObject prefab, GameObject parent)
        {
            GameObject PresenceObject = Instantiate(prefab);
            PresenceObject.transform.parent = parent.transform;
            
            Presence p = PresenceObject.GetComponent<Presence>();


            return p;

        }

        public void Initialise()
        {
            if (Visualiser!=null)
            Visualiser.Initialise(this.gameObject);

        }
       
        // Use this for initialization
        void Start()
        {
         
        }

        // Update is called once per frame
        void Update()
        {
            if (DepthTransport !=null && Visualiser != null && Visualiser.IsInitialised())
                        Visualiser.Update(DepthTransport.ActiveFrame); // frame may or may not be different, it's up to the interface implementation to deal with that.
        }

        public void SetTranscoder (string name)
        {

            if (DepthTransport == null)
                DepthTransport = new DepthTransport();

            DepthTransport.SetTranscoder(name);
        }
      
        public void SetVisualiser(string name)
        {


            switch (name)
            {
                case "ShowSkeleton":
                    Visualiser = new ShowSkeleton();
                    break;
                default:
                    Debug.LogError("Trying to set unkown visualiser.");
                    break;
            }

        }


    }
}