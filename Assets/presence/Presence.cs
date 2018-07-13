using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StoryEngine;

namespace PresenceEngine
{
    public class Presence : MonoBehaviour
    {
        public iVisualiser Visualiser;
        public DepthTransport DepthTransport;
        public bool SoundPlayed=false;

        static public Presence Create(GameObject parent)
        {


            GameObject PresenceObject = new GameObject();
            Presence p = PresenceObject.AddComponent<Presence>();


            //PresenceObject.transform.parent = parent.transform;


            PresenceObject.transform.SetParent(parent.transform, false);


            return p;

        }


        void Update()
        {


            if (DepthTransport != null && DepthTransport.Mode != DEPTHMODE.OFF)
            {
                if (Visualiser != null && !Visualiser.IsInitialised())
                    Visualiser.Initialise(this.gameObject);

                if (DepthTransport != null && Visualiser != null && Visualiser.IsInitialised())
                    Visualiser.Update(DepthTransport.ActiveFrame); // frame may or may not be different, it's up to the interface implementation to deal with that.
            }
            else
            {
                if (Visualiser != null)
                    Visualiser.Deinitialise();
            }


           

           
        }

        public void SetTranscoder(string name)
        {

            if (DepthTransport == null)
                DepthTransport = new DepthTransport();

            DepthTransport.SetTranscoder(name);
        }

        public void SetDepthSampling(int sampling)
        {

            if (DepthTransport != null)
                DepthTransport.DepthSampling = sampling;

        }

        public string GetTranscoder()
        {

            return ((DepthTransport != null && DepthTransport.TransCoder != null) ? DepthTransport.TransCoder.GetName() : "");

        }

        public void SetVisualiser(string name)
        {


            switch (name)
            {
                case "ShowSkeleton":
                    Visualiser = new ShowSkeleton();
                    Visualiser.Initialise(this.gameObject);
                    break;
                case "PointCloud":
                    Visualiser = new PointCloud();
                    Visualiser.Initialise(this.gameObject);
                    break;
                case "ShowMesh":
                    Visualiser = new ShowMesh();
                    Visualiser.Initialise(this.gameObject);
                    break;
                case "PointShaded":
                    Visualiser = new PointShaded();
                    Visualiser.Initialise(this.gameObject);
                    break;



                default:
                    Debug.LogError("Trying to set unkown visualiser.");
                    break;
            }

        }

        public string GetVisualiser()
        {
            return (Visualiser != null ? Visualiser.GetName() : "");

        }

        public Vector3 GetPosition()
        {
            return Visualiser != null ? Visualiser.GetPosition() : Vector3.zero;
        }

        public Vector3 GetScale()
        {
            return Visualiser != null ? Visualiser.GetScale() : Vector3.one;
        }

        public Quaternion GetRotation()
        {
            return Visualiser != null ? Visualiser.GetRotation() : Quaternion.identity;

        }

        public string GetBufferFileName()
        {
        return (DepthTransport != null && DepthTransport.TransCoder != null && DepthTransport.TransCoder.GetBufferFile()!=null) ? DepthTransport.TransCoder.GetBufferFile().Name : "";

        }
        public void AddSettingsToTask(StoryTask task, string prefix)
        {

            task.SetStringValue(prefix + "_visualiser", GetVisualiser());
            task.SetStringValue(prefix + "_transcoder", GetTranscoder());
            task.SetVector3Value(prefix + "_position", GetPosition());
            task.SetVector3Value(prefix + "_scale", GetScale());

            task.SetQuaternionValue(prefix + "_rotation", GetRotation());
            task.SetStringValue(prefix + "_buffer", GetBufferFileName());

        //Debug.Log( GetVisualiser());
        //Debug.Log( GetTranscoder());
        //Debug.Log( GetPosition().ToString());
        //Debug.Log( GetRotation().ToString());
        //Debug.Log( GetBufferFileName());

        }
        public void AddModeToTask(StoryTask task, string prefix)
        {
            int mode=(int)DEPTHMODE.OFF;
            string target = "";
            if (DepthTransport!=null){
                
                mode =(int)DepthTransport.Mode;
                target = DepthTransport.Target;

            }
                
            task.SetIntValue(prefix + "_depthmode",mode);
            task.SetStringValue(prefix + "_target",target);


        }

        public void GetModeFromTask(StoryTask task, string prefix)
        {
            int mode;
            task.GetIntValue(prefix + "_depthmode", out mode);

            string target;
            task.GetStringValue(prefix + "_target", out target);


            if (DepthTransport!=null){
                DepthTransport.Mode=(DEPTHMODE)mode;
                DepthTransport.Target=target;
            }

        }

    public void GetSettingsFromTask (StoryTask task, string prefix){

        string visualiser, transcoder, buffer;
        Vector3 position,scale;
        Quaternion rotation;

        task.GetStringValue(prefix + "_visualiser", out visualiser);
        task.GetStringValue(prefix + "_transcoder", out transcoder);
        task.GetStringValue(prefix + "_buffer", out buffer);
        task.GetVector3Value(prefix + "_position", out position);
            task.GetVector3Value(prefix + "_scale", out scale);

        task.GetQuaternionValue(prefix + "_rotation", out rotation);

        SetVisualiser(visualiser);
        SetTranscoder(transcoder);
        DepthTransport.TransCoder.SetBufferFile(IO.LoadFile(buffer));
        Visualiser.SetTransform(position,scale, rotation);

        Debug.Log( GetVisualiser());
        Debug.Log( GetTranscoder());
        Debug.Log( GetPosition().ToString());
        Debug.Log( GetRotation().ToString());
        Debug.Log( GetBufferFileName());

    }








    }
}