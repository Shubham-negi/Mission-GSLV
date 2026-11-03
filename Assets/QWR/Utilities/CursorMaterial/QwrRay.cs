/************************************************************************************

Copyright   :   Copyright 2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/
using UnityEngine;
using System.Collections;

namespace qwr
{
    public class QwrRay : QwrBaseRay
    {
        // Use this for initialization
        void Start()
        {
            CreateRayVertices();

            transform.localPosition = new Vector3(0,0,0);

            Material material = gameObject.GetComponent<Renderer>().material;
            material.renderQueue = 4000;
        }

        private void CreateRayVertices()
        {
            Mesh mesh = new Mesh();
            gameObject.AddComponent<MeshFilter>();
            GetComponent<MeshFilter>().mesh = mesh;

            Vector3[] vertices =
            {
                new Vector3(0.5f, 0.866025f, 1.0f),
                new Vector3(1,0, 1.0f),
                new Vector3(0.5f, -0.866025f, 1.0f),

                new Vector3(-0.5f, -0.866025f, 1.0f),
                new Vector3(-1.0f, 0.0f, 1.0f),
                new Vector3(-0.5f, 0.866025f, 1.0f),

                new Vector3(0.5f, 0.866025f, 0.0f),
                new Vector3(1, 0, 0.0f),
                new Vector3(0.5f, -0.866025f, 0.0f),

                new Vector3(-0.5f, -0.866025f, 0.0f),
                new Vector3(-1.0f, 0.0f, 0.0f),
                new Vector3(-0.5f, 0.866025f, 0.0f),
            };

            Vector3 scale = new Vector3(0.002f, 0.002f, 1.0f);
            for (int i = 0; i < vertices.Length; ++i)
            {
                Vector3 vertex = vertices[i];
                vertices[i] = new Vector3(vertex.x * scale.x, vertex.y * scale.y, vertex.z * scale.z);
            }

            int[] indices =
            {
                0, 6, 7, 0, 7, 1,
                1, 7, 8, 1, 8, 2,
                2, 8, 9, 2, 9, 3,
                3, 9,10, 3,10, 4,
                4,10,11, 4,11, 5,
                5,11, 6, 5, 6, 0
            };

            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.RecalculateBounds();
            ;
        }

        public override void Set(Vector3 start, Vector3 end)
        {
            base.Set(start, end);
            float scale = (end - start).magnitude;
            transform.localScale = new Vector3(1, 1, scale);
            
        }
    }
}
