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

            if (Visualiser != null && !Visualiser.IsInitialised())
                Visualiser.Initialise(this.gameObject);

            if (DepthTransport != null && Visualiser != null && Visualiser.IsInitialised())
                Visualiser.Update(DepthTransport.ActiveFrame); // frame may or may not be different, it's up to the interface implementation to deal with that.
        }

        public void SetTranscoder(string name)
        {

            if (DepthTransport == null)
                DepthTransport = new DepthTransport();

            DepthTransport.SetTranscoder(name);
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
            int mode=0;
            if (DepthTransport!=null)
                mode =(int)DepthTransport.Mode;
                
            task.SetIntValue(prefix + "_depthmode",mode);

        }

        public void GetModeFromTask(StoryTask task, string prefix)
        {
            int mode;
            task.GetIntValue(prefix + "_depthmode", out mode);

            if (DepthTransport!=null)
                DepthTransport.Mode=(DEPTHMODE)mode;

        }

    public void GetSettingsFromTask (StoryTask task, string prefix){

        string visualiser, transcoder, buffer;
        Vector3 position;
        Quaternion rotation;

        task.GetStringValue(prefix + "_visualiser", out visualiser);
        task.GetStringValue(prefix + "_transcoder", out transcoder);
        task.GetStringValue(prefix + "_buffer", out buffer);
        task.GetVector3Value(prefix + "_position", out position);
        task.GetQuaternionValue(prefix + "_rotation", out rotation);

        SetVisualiser(visualiser);
        SetTranscoder(transcoder);
        DepthTransport.TransCoder.SetBufferFile(IO.GetFileBuffer(buffer));
        Visualiser.SetTransform(position, rotation);

        Debug.Log( GetVisualiser());
        Debug.Log( GetTranscoder());
        Debug.Log( GetPosition().ToString());
        Debug.Log( GetRotation().ToString());
        Debug.Log( GetBufferFileName());

    }








    }
}