using UnityEngine;
using System.Collections;


namespace Presence
{

    static public class DebugObject

    {
        // Static class to create,add to a target and return a reference of basic visualisation gameobjects suchs as nulls or basic boids.

        static Material[] mat;



        static public GameObject getDiamond(float scaling)
        {
            GameObject Diamond = new GameObject("PointDiamond");
            //		visualDebugPoint.transform.parent = target.transform;
            //		visualDebugPoint.transform.position = point;

            Diamond.AddComponent<MeshFilter>();
            Diamond.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            Diamond.GetComponent<MeshFilter>().mesh = mesh;

            Vector3[] vertices = new Vector3[5];

            vertices[0] = new Vector3(0.0f, 0.0f, 10.0f);
            vertices[1] = new Vector3(7.0f, 0.0f, -7.0f);
            vertices[2] = new Vector3(-7.0f, 0.0f, -7.0f);
            vertices[3] = new Vector3(0.0f, 7.0f, 0.0f);
            vertices[4] = new Vector3(0.0f, -7.0f, 0.0f);

            int[] triangles = new int[3 * 6];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 3;

            triangles[3] = 1;
            triangles[4] = 0;
            triangles[5] = 4;

            triangles[6] = 1;
            triangles[7] = 2;
            triangles[8] = 3;

            triangles[9] = 2;
            triangles[10] = 1;
            triangles[11] = 4;

            triangles[12] = 2;
            triangles[13] = 0;
            triangles[14] = 3;

            triangles[15] = 0;
            triangles[16] = 2;
            triangles[17] = 4;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            Diamond.transform.localScale = new Vector3(scaling, scaling, scaling);

            Diamond.GetComponent<Renderer>().material = Resources.Load("DarkGrey") as Material;

            return (Diamond);

        }




        static public GameObject getNullObject(float scale)
        {
            return getNullObject(new Vector3(scale, scale, scale));

        }


        static public GameObject getNullObject(float scalex, float scaley, float scalez)
        {
            return getNullObject(new Vector3(scalex, scaley, scalez));

        }

        static public GameObject getNullObject(Vector3 scale)

        {
            GameObject NullObject = new GameObject("NullObject");

            //		visualDebugPoint.transform.parent = target.transform;
            //		visualDebugPoint.transform.position = point;

            NullObject.AddComponent<MeshFilter>();
            NullObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            NullObject.GetComponent<MeshFilter>().mesh = mesh;

            Vector3[] vertices = new Vector3[5];

            vertices[0] = new Vector3(0.0f, 0.0f, 0.0f);
            vertices[1] = new Vector3(10.0f, 0.0f, 0.0f);
            vertices[2] = new Vector3(0.0f, 10.0f, 0.0f);
            vertices[3] = new Vector3(0.0f, 0.0f, 10.0f);
            vertices[4] = new Vector3(0.25f, 0.25f, 0.25f);

            int[] trianglesRed = new int[2 * 3];
            // x
            trianglesRed[0] = 0;
            trianglesRed[1] = 1;
            trianglesRed[2] = 4;

            trianglesRed[3] = 0;
            trianglesRed[4] = 4;
            trianglesRed[5] = 1;

            int[] trianglesGreen = new int[2 * 3];
            //y
            trianglesGreen[0] = 0;
            trianglesGreen[1] = 4;
            trianglesGreen[2] = 2;

            trianglesGreen[3] = 0;
            trianglesGreen[4] = 2;
            trianglesGreen[5] = 4;

            int[] trianglesBlue = new int[2 * 3];
            //z
            trianglesBlue[0] = 0;
            trianglesBlue[1] = 3;
            trianglesBlue[2] = 4;

            trianglesBlue[3] = 0;
            trianglesBlue[4] = 4;
            trianglesBlue[5] = 3;

            mesh.vertices = vertices;
            mesh.subMeshCount = 3;

            mesh.SetTriangles(trianglesRed, 0);
            mesh.SetTriangles(trianglesGreen, 1);
            mesh.SetTriangles(trianglesBlue, 2);

            NullObject.transform.localScale = 0.1f * scale;

            mat = new Material[3];
            mat[0] = Resources.Load("Red") as Material;
            mat[1] = Resources.Load("Green") as Material;
            mat[2] = Resources.Load("Blue") as Material;

            NullObject.GetComponent<Renderer>().materials = mat;

            return (NullObject); // return a reference to the created gameobject
        }
    }
}