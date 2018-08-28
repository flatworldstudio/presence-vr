using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StoryEngine;

namespace PresenceEngine
{
    public class Presence : MonoBehaviour
    {
        //  iVisualiser Visualiser;
        iVisualiser[] Visualisers;

        //public iVisualiser Visualiser;

        public DepthTransport DepthTransport;
        public bool SoundPlayed = false;

        public string Name;

        static public Presence Create(GameObject parent, string name)
        {

            GameObject PresenceObject = new GameObject();
            PresenceObject.name = name;
            Presence p = PresenceObject.AddComponent<Presence>();
            p.name = name;

            PresenceObject.transform.SetParent(parent.transform, false);
            p.Visualisers = new iVisualiser[2];

            SETTINGS.Presences.Add(name, p);


            return p;

        }

        void Start()
        {

        }

        void Update()
        {

            if (DepthTransport != null && DepthTransport.Mode != DEPTHMODE.OFF)
            {

                for (int i = 0; i < Visualisers.Length; i++)
                {

                    if (Visualisers[i] != null && !Visualisers[i].IsInitialised())
                        Visualisers[i].Initialise(this.gameObject);

                    if (DepthTransport != null && Visualisers[i] != null && Visualisers[i].IsInitialised())
                        Visualisers[i].Update(DepthTransport.ActiveFrame); // frame may or may not be different, it's up to the interface implementation to deal with that.

                }


            }
            else
            {
                // If no depth data work from, remove all the visualisers.. (?)

                for (int i = 0; i < Visualisers.Length; i++)
                {
                    if (Visualisers[i] != null)
                        Visualisers[i].Deinitialise();

                }

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

        // Visualiser manipulation. Old methods just take the first visualiser. New methods allow for multiple.


        private iVisualiser Visualiser
        {
            get
            {
                return Visualisers[0];
            }
            set
            {
                Visualisers[0] = value;
            }

        }
        public void SetVisualiser(string name)
        {

            SetVisualiser(name, 0);

        }



        public void SetVisualiser(string vis, int i)
        {

            if (Visualisers[i] != null)
            {
                // A visualiser is already set.

                if (Visualisers[i].GetName() == vis)
                {
                    // No change, abort.
                    return;

                }
                else
                {
                    Debug.Log("Deinitialising visualiser " + Visualisers[i].GetName());
                    // Change, so deinitialise the current one.
                    Visualisers[i].Deinitialise();
                    Visualisers[i] = null;

                }

            }

            switch (vis)
            {
                case "ShowSkeleton":
                    Visualisers[i] = new ShowSkeleton();
                    Visualisers[i].Initialise(this.gameObject);
                    break;
                case "PointCloud":
                    Visualisers[i] = new PointCloud();
                    Visualisers[i].Initialise(this.gameObject);
                    break;
                case "ShowMesh":
                    Visualisers[i] = new ShowMesh();
                    Visualisers[i].Initialise(this.gameObject);
                    break;
                case "PointShaded":
                    Visualisers[i] = new PointShaded();
                    Visualisers[i].Initialise(this.gameObject);
                    break;

                case "":
                    // Setting none, which is equivalent to just deinitialising the one that was there. We've already done that.

                    break;

                default:
                    Debug.LogError("Trying to set unkown visualiser.");
                    break;
            }




        }

        public string GetVisualiser(int i)
        {
            return (Visualisers[i] != null ? Visualisers[i].GetName() : "");

        }


        public string GetVisualiser()
        {
            return (Visualiser != null ? Visualiser.GetName() : "");

        }

        // These just get values from the first visualiser - values assumed to be identical for all visualisers.

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
            return (DepthTransport != null && DepthTransport.TransCoder != null && DepthTransport.TransCoder.GetBufferFile() != null) ? DepthTransport.TransCoder.GetBufferFile().Name : "";

        }
        public void PushAllSettingToTask(StoryTask task, string prefix)
        {

            task.SetStringValue(prefix + "_visualiser_0", GetVisualiser(0));
            task.SetStringValue(prefix + "_visualiser_1", GetVisualiser(1));
            task.SetStringValue(prefix + "_transcoder", GetTranscoder());
            task.SetVector3Value(prefix + "_position", GetPosition());
            task.SetVector3Value(prefix + "_scale", GetScale());

            task.SetQuaternionValue(prefix + "_rotation", GetRotation());
            task.SetStringValue(prefix + "_buffer", GetBufferFileName());

            PushModeToTask(task, prefix);
            PushVisualiserSettingsToTask(task, prefix);

        }

        public void PullAllSettingsFromTask(StoryTask task, string prefix)
        {

            string visualiser_0, visualiser_1, transcoder, buffer;
            Vector3 position, scale;
            Quaternion rotation;

            task.GetStringValue(prefix + "_visualiser_0", out visualiser_0);
            task.GetStringValue(prefix + "_visualiser_1", out visualiser_1);
            task.GetStringValue(prefix + "_transcoder", out transcoder);
            task.GetStringValue(prefix + "_buffer", out buffer);
            task.GetVector3Value(prefix + "_position", out position);
            task.GetVector3Value(prefix + "_scale", out scale);
            task.GetQuaternionValue(prefix + "_rotation", out rotation);

            SetVisualiser(visualiser_0, 0);
            SetVisualiser(visualiser_1, 1);

            SetTranscoder(transcoder);

            if (buffer != "")
            {
                DepthTransport.TransCoder.SetBufferFile(IO.LoadFile(buffer));
            }

            SetVisualiseTransform(position, scale, rotation);
            //     Visualiser.SetTransform(position, scale, rotation);

            //   Visualiser.SettingsFromTask(task, prefix);

            PullModeFromTask(task, prefix);
            PullVisualiserSettingsFromTask(task, prefix);


            //Debug.Log(GetVisualiser());
            //Debug.Log(GetTranscoder());
            //Debug.Log(GetPosition().ToString());
            //Debug.Log(GetRotation().ToString());
            //Debug.Log(GetBufferFileName());

        }


        void PushModeToTask(StoryTask task, string prefix)
        {
            int mode = (int)DEPTHMODE.OFF;
            string target = "";
            if (DepthTransport != null)
            {

                mode = (int)DepthTransport.Mode;
                target = DepthTransport.Target;

            }

            task.SetIntValue(prefix + "_depthmode", mode);
            task.SetStringValue(prefix + "_target", target);


        }

        void PullModeFromTask(StoryTask task, string prefix)
        {
            int mode;
            task.GetIntValue(prefix + "_depthmode", out mode);

            string target;
            task.GetStringValue(prefix + "_target", out target);


            if (DepthTransport != null)
            {
                DepthTransport.Mode = (DEPTHMODE)mode;
                DepthTransport.Target = target;
            }

        }

        public void SetVisualiseTransform(Vector3 pos, Vector3 scale, Quaternion rot)
        {
            // All visualisers same transform.

            for (int i = 0; i < Visualisers.Length; i++)
            {
                if (Visualisers[i] != null)
                    Visualiser.SetTransform(pos, scale, rot);
            }

        }


        public void PullVisualiserSettingsFromTask(StoryTask task, string prefix)
        {
            for (int i = 0; i < Visualisers.Length; i++)
            {
                if (Visualisers[i] != null)
                    Visualisers[i].SettingsFromTask(task, prefix + "_" + i);
            }
        }

        public void PushVisualiserSettingsToTask(StoryTask task, string prefix)
        {
            for (int i = 0; i < Visualisers.Length; i++)
            {
                if (Visualisers[i] != null)
                    Visualisers[i].SettingsToTask(task, prefix + "_" + i);
            }
        }





    }
}