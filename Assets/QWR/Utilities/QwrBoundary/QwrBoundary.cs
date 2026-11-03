/************************************************************************************

Copyright   :   Copyright 2018 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/
using UnityEngine;
using System.Collections;

namespace qwr
{
    [RequireComponent(typeof(Renderer))]
    /// <summary>
    /// Draws a circular cylinder around camera to avoid go out.
    /// </summary>
    public class QwrBoundary : MonoBehaviour
    {
        public float radius = 2.0f;
        public float height = 1.5f;
        /// <summary>
        /// Number of segments making the circle.
        /// </summary>
        public int reticleSegments = 40;

        private Renderer render;

        // Use this for initialization
        void Start()
        {
            radius = NoloController.NoloGetData() != null ? NoloController.NoloGetData()[4] : 2.0f;
            height = NoloController.NoloGetData() != null ? NoloController.NoloGetData()[0] : 1.5f;
            CreateReticleVertices();
            render = GetComponent<Renderer>();
            render.material.renderQueue = 4000 + 100;
        }

        private void CreateReticleVertices()
        {
            Mesh mesh = new Mesh();
            gameObject.AddComponent<MeshFilter>();
            GetComponent<MeshFilter>().mesh = mesh;

            int segments_count = reticleSegments;
            int vertex_count = (segments_count + 1) * 2;

            #region Vertices

            Vector3[] vertices = new Vector3[vertex_count];
            Vector2[] uv = new Vector2[vertex_count];

            const float kTwoPi = Mathf.PI * 2.0f;
            int vi = 0;
            for (int si = 0; si <= segments_count; ++si)
            {
                // Add two vertices for every circle segment: one at the beginning of the
                // prism, and one at the end of the prism.
                float angle = (float)si / (float)(segments_count) * kTwoPi;

                float x = radius * Mathf.Sin(angle);
                float z = radius * Mathf.Cos(angle);

                uv[vi] = new Vector2((float)si / (float)(segments_count), 0.0f);
                vertices[vi++] = new Vector3(x, 0.0f - height, z); // lower vertex.
                uv[vi] = new Vector2((float)si / (float)(segments_count), 1.0f);
                vertices[vi++] = new Vector3(x, 2.5f - height, z); // upper vertex.
            }
            #endregion

            #region Triangles
            int indices_count = (segments_count + 1) * 3 * 2;
            int[] indices = new int[indices_count];

            int vert = 0;
            int idx = 0;
            for (int si = 0; si < segments_count; ++si)
            {
                indices[idx++] = vert + 1;
                indices[idx++] = vert;
                indices[idx++] = vert + 2;

                indices[idx++] = vert + 1;
                indices[idx++] = vert + 2;
                indices[idx++] = vert + 3;

                vert += 2;
            }
            #endregion

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = indices;
            mesh.RecalculateBounds();
            ;
        }

        // Update is called once per frame
        void Update()
        {
            float alpha;
#if false
            if (QwrManager.peripheral == QWRPeripheral.Nolo)
            {
                float[] temp_alpha = new float[3];
                if (0 != NoloController._instance[(int)NoloController.NoloDevice.Nolo_Hmd].QwrpGetDeviceCurrentStatus().device_status)
                {
                    Vector3 pos_hmd = NoloController._instance[(int)NoloController.NoloDevice.Nolo_Hmd].GetPosition();
                    temp_alpha[0] = (new Vector2(pos_hmd.x, pos_hmd.z).magnitude < radius - 0.4f) ? 0.0f : 1.0f;
                }
                else
                {
                    temp_alpha[0] = 0;
                }
                if (0 != NoloController._instance[(int)NoloController.NoloDevice.Nolo_Left_Controller].QwrpGetDeviceCurrentStatus().device_status)
                {
                    Vector3 pos_left_controller = NoloController._instance[(int)NoloController.NoloDevice.Nolo_Left_Controller].GetPosition();
                    temp_alpha[1] = (new Vector2(pos_left_controller.x, pos_left_controller.z).magnitude < 0.9 * radius - 0.4f) ? 0.0f : 1.0f;
                }
                else
                {
                    temp_alpha[1] = 0;
                }
                if (0 != NoloController._instance[(int)NoloController.NoloDevice.Nolo_Right_Controller].QwrpGetDeviceCurrentStatus().device_status)
                {
                    Vector3 pos_right_controller = NoloController._instance[(int)NoloController.NoloDevice.Nolo_Right_Controller].GetPosition();
                    temp_alpha[2] = (new Vector2(pos_right_controller.x, pos_right_controller.z).magnitude < 0.9 * radius - 0.4f) ? 0.0f : 1.0f;
                }
                else
                {
                    temp_alpha[1] = 0;
                }
                alpha = Mathf.Max(temp_alpha);
            }
            else
#endif
            {
                Vector3 pos = QwrCameraRig._instance.GetPosition();
                alpha = (new Vector2(pos.x, pos.z).magnitude < radius - 0.4f) ? 0.0f : 1.0f;
            }
            render.material.SetFloat("_AlphaFraction", alpha);
        }
    }
}