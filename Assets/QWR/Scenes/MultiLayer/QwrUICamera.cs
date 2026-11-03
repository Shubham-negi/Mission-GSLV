/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using System.Threading;

namespace qwr
{
    /// <summary>
    /// Main entry point for the QwrUICamera API.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class QwrUICamera : MonoBehaviour
    {
        public enum UITYPE
        {
            GUI = 1,
            CURSOR = 2,
        }
        public UITYPE UIType = UITYPE.GUI;

        private Rect viewport = new Rect(0, 0, 1, 1);
        public float depth = 0.0f;
#if UNITY_EDITOR
        const int cameraNum = 3;
#else
        const int cameraNum = 2;
#endif
        public Camera leftCamera;
        public Camera rightCamera;
        public Camera centerCamera;

        private RenderTexture[] _canvas_texture = new RenderTexture[Common.NUM_BUFFER * cameraNum];
        private IntPtr[] _canvas_ptr = new IntPtr[Common.NUM_BUFFER * cameraNum];
        private int index = 0;

        static public QwrUICamera instance = null;

        public void QwrSetViewport(Rect view)
        {
            viewport = view;
            Reshape();
        }
        public Rect QwrGetViewport()
        {
            return viewport;
        }

        bool inited = false;

        private void Awake()
        {
            instance = this;
            Reshape();
        }
        // Use this for initialization
        void Start()
        {
#if (!UNITY_EDITOR) && UNITY_ANDROID && (UNITY_2018 || UNITY_2019)
            StartCoroutine(DelayStart());
#else
            StartImpl();
#endif
        }

        void Reshape()
        {
#if UNITY_EDITOR
            int width = (int)(Screen.width * viewport.width);
            int height = (int)(Screen.height * viewport.height);
#else
            int width = (int)(QwrManager.DeviceInfo.resolution_x * viewport.width);
            int height = (int)(QwrManager.DeviceInfo.resolution_y * viewport.height);
#endif
            for (int i = 0; i < Common.NUM_BUFFER; i++)
            {
                for (int j = 0; j < cameraNum; ++j)
                {
                    int index = i * cameraNum + j;
                    _canvas_texture[index] = new RenderTexture
                    (width,height
                     , 24, RenderTextureFormat.Default);
                    _canvas_texture[index].antiAliasing
                        = 4;
                    _canvas_texture[index].Create();
                    _canvas_ptr[index] = _canvas_texture[index].GetNativeTexturePtr();
                }

            }
        }

        void OnDestroy()
        {
            if (leftCamera != null)
                leftCamera.targetTexture = null;
            if (rightCamera != null)
                rightCamera.targetTexture = null;
            if (centerCamera != null)
                centerCamera.targetTexture = null;

            Composer.QwruSetTextureEx((int)UIType, IntPtr.Zero, 0, (int)qwrcTwType.NONE, new qwrRect(viewport));
            Composer.QwruSetTextureEx((int)UIType, IntPtr.Zero, 1, (int)qwrcTwType.NONE, new qwrRect(viewport));
            //Composer.QwruSetTextureEx((int)UIType, IntPtr.Zero, 2, (int)qwrcTwType.NONE, new qwrRect(viewport));
            for (int i = 0; i < Common.NUM_BUFFER; i++)
            {
                for (int j = 0; j < cameraNum; ++j)
                {
                    int index = i * cameraNum + j;
                    if (_canvas_texture[index].IsCreated())
                    {
                        _canvas_texture[index].Release();
                    }
                }

            }
        }

        void OnCameraPreRender(Camera camera)
        {
            if (camera == leftCamera)
            {
                leftCamera.targetTexture = _canvas_texture[index];
            }
            else if (camera == rightCamera)
            {
                rightCamera.targetTexture = _canvas_texture[index + 1];
            }
#if UNITY_EDITOR
            else if (camera == centerCamera)
            {
                centerCamera.targetTexture = _canvas_texture[index + 2];
                qwrLayerMaterial.SetTexture("albedo", centerCamera.targetTexture);
            }
#endif
        }

        //Update is called once per frame
        void LateUpdate()
        {
            if (!inited)
                return;

            index = (index + cameraNum) % (Common.NUM_BUFFER * cameraNum);
        }

        void OnCameraPostRender(Camera camera)
        {
#if UNITY_EDITOR
            if (camera == QwrCameraRig.Instance._center_eye)
            {
                var texture = _canvas_texture[index + 2];
                Camera currCamera = Camera.current;
                Camera.SetupCurrent(camera);
                Graphics.ExecuteCommandBuffer(commandbuff);
                Camera.SetupCurrent(currCamera);
            }
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
            if(camera != rightCamera)
                return;
            RenderTexture rt = _canvas_texture[index];
            if (rt == null)
                return;

            qwrRect view = new qwrRect(new Rect(0, 0, rt.width, rt.height));
            IntPtr tempPtr = Marshal.AllocHGlobal(Marshal.SizeOf(view));
            Marshal.StructureToPtr(view, tempPtr, false);
            Composer.PostRender(RENDER_EVENT.Posttransparent, (int)tempPtr);

            {
                // Rect viewport_left = new Rect(viewport.x + QwrManager.DeviceInfo.ipd / (4 * depth * (float)Math.Tan((Math.PI / 360) * QwrManager.DeviceInfo.fov_x)), viewport.y, viewport.width, viewport.height);
                // Rect viewport_right = new Rect(viewport.x - QwrManager.DeviceInfo.ipd / (4 * depth * (float)Math.Tan((Math.PI / 360) * QwrManager.DeviceInfo.fov_x)), viewport.y, viewport.width, viewport.height);
                Composer.QwruSetTextureEx((int)UIType, _canvas_ptr[index], 0, (int)qwrcTwType.DISTORTION, new qwrRect(viewport));
                Composer.QwruSetTextureEx((int)UIType, _canvas_ptr[index + 1], 1, (int)qwrcTwType.DISTORTION, new qwrRect(viewport));
            }
#endif
        }

#if UNITY_ANDROID && (UNITY_5_5_0 || UNITY_5_4_3)
        IEnumerator Coroutine_EnableCamera()
        {
            if(leftCamera)
                leftCamera.enabled = false;
            if(rightCamera)
                rightCamera.enabled = false;

            yield return new WaitForEndOfFrame();

            if(leftCamera)
                leftCamera.enabled = true;
            if(rightCamera)
                rightCamera.enabled = true;
        }
#endif
        private void OnDisable()
        {
            if(QwrManager.IsScriptRenderingPipeline)
            {
#if UNITY_2019_1_OR_NEWER
                RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
                RenderPipelineManager.endCameraRendering -= EndCameraRendering;
#endif
            }
            else
            {
                Camera.onPostRender -= OnCameraPostRender;
                Camera.onPreRender -= OnCameraPreRender;
            }
            
            QwrCameraRig.onPeripheralChanged -= OnPeripheralChanged;
        }

        private void OnEnable()
        {

            if (QwrManager.IsScriptRenderingPipeline)
            {
#if UNITY_2019_1_OR_NEWER
                RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
                RenderPipelineManager.endCameraRendering += EndCameraRendering;
#endif
            }
            else
            {
                Camera.onPostRender += OnCameraPostRender;
                Camera.onPreRender += OnCameraPreRender;
            }


            QwrCameraRig.onPeripheralChanged += OnPeripheralChanged;
        }

        bool comandEnabled = false;
        CommandBuffer commandbuff;

        static int blendSrcFactor = 0;

        void SetSrcBlendFactor(int value)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!QwrDevice.bVR9)
            {
                if(blendSrcFactor == value)
                    return;
                blendSrcFactor = value;

                IntPtr tempPtr = Marshal.StringToHGlobalAnsi("blend_srcfactor");
                Composer.QwruSetIntValue(tempPtr, value);
            }
#endif
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (leftCamera)
                    leftCamera.enabled = false;
                if (rightCamera)
                    rightCamera.enabled = false;

            }
            else
            {
                StartCoroutine(EnableCameras_Delay());
            }
        }

        IEnumerator EnableCameras_Delay()
        {
            yield return new WaitForEndOfFrame();
            if (leftCamera)
                leftCamera.enabled = true;
            if (rightCamera)
                rightCamera.enabled = true;
        }
        [SerializeField]
        Material qwrLayerMaterial;

#if UNITY_EDITOR


        void InitEditorMultiLayer()
        {
            commandbuff = new CommandBuffer();

            Mesh mesh = new Mesh();

            Vector3[] vertices =
            {
                new Vector3(viewport.x, viewport.y),
                new Vector3(viewport.x + viewport.width, viewport.y),
                new Vector3(viewport.x + viewport.width, viewport.y + viewport.height),
                new Vector3(viewport.x, viewport.y + viewport.height),
            };
            int[] indices =
            {
                0,1,2,
                0,2,3,
            };

            Vector2[] texcoords =
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
            };

            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.uv = texcoords;

            commandbuff.DrawMesh(mesh, Matrix4x4.identity, qwrLayerMaterial);
        }
#endif

        IEnumerator DelayStart()
        {
            leftCamera.enabled = false;
            rightCamera.enabled = false;
            centerCamera.enabled = false;

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            StartImpl();
        }

        void StartImpl()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
#else
            if (0.0f == depth)
            {
                Composer.QwruSetTextureEx((int)UIType, _canvas_ptr[index], 0, (int)qwrcTwType.DISTORTION, new qwrRect(viewport));
                Composer.QwruSetTextureEx((int)UIType, _canvas_ptr[index + 1], 1, (int)qwrcTwType.DISTORTION, new qwrRect(viewport));
            }
            else
            {
                // Rect viewport_left = new Rect(viewport.x + QwrManager.DeviceInfo.ipd / (4 * depth * (float)Math.Tan((Math.PI / 360) * QwrManager.DeviceInfo.fov_x)), viewport.y, viewport.width, viewport.height);
                // Rect viewport_right = new Rect(viewport.x - QwrManager.DeviceInfo.ipd / (4 * depth * (float)Math.Tan((Math.PI / 360) * QwrManager.DeviceInfo.fov_x)), viewport.y, viewport.width, viewport.height);
                Composer.QwruSetTextureEx((int)UIType, _canvas_ptr[index], 0, (int)qwrcTwType.DISTORTION, new qwrRect(viewport));
                Composer.QwruSetTextureEx((int)UIType, _canvas_ptr[index + 1], 1, (int)qwrcTwType.DISTORTION, new qwrRect(viewport));
            }
#endif

#if UNITY_ANDROID && (UNITY_5_5_0 || UNITY_5_4_3)
            // In Unity 5.5.0 and Unity 5.4.3,
            // surface is deleted and rebuilt after the first frame is completed by Unity, the second frame will be black screen.
            // So, Skip the rendering of the first frame to avoid flickering.
            StartCoroutine(Coroutine_EnableCamera());
#endif

            SetSrcBlendFactor(1);

#if UNITY_EDITOR
            InitEditorMultiLayer();
#endif
            leftCamera.depth = depth;
            leftCamera.transform.localPosition = new Vector3(QwrManager.DeviceInfo.ipd * -0.5f, 0, 0);
            leftCamera.fieldOfView = QwrManager.DeviceInfo.fov_y;

            rightCamera.depth = depth + 0.1f;
            rightCamera.transform.localPosition = new Vector3(QwrManager.DeviceInfo.ipd * 0.5f, 0, 0);
            rightCamera.fieldOfView = QwrManager.DeviceInfo.fov_y;

            centerCamera.depth = depth + 0.2f;
            centerCamera.transform.localPosition = new Vector3(0, 0, 0);
            centerCamera.fieldOfView = QwrManager.DeviceInfo.fov_y;

#if !UNITY_EDITOR
              centerCamera.enabled = false;
#endif


            inited = true;
        }
#if UNITY_2019_1_OR_NEWER
        private void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (ContainCamera(camera))
            {
                OnCameraPreRender(camera);
            }
        }

        void EndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (ContainCamera(camera))
            {
                OnCameraPostRender(camera);
            }
#if UNITY_EDITOR
            else if (QwrCameraRig.Instance._center_eye == camera)
            {
                OnCameraPostRender(camera);
            }
#endif
        }
#endif
        bool ContainCamera(Camera camera)
        {
            return camera == leftCamera || camera == rightCamera || camera == centerCamera;
        }

        IEnumerator OnPeripheralChanged_EndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            QwrBasePointer pointer = QwrPointerManager.Instance.pointer;
            if (pointer != null)
            {
                if (QwrCameraRig._instance.CurrentPeripheral.PeripheralType == QWRPeripheral.None)
                {
                    pointer.gameObject.layer = LayerMask.NameToLayer("UI");
                }
                else
                {
                    pointer.gameObject.layer = 0;
                }
            }
        }

        void OnPeripheralChanged(QwrBasePeripheral currPeripheral)
        {
            StartCoroutine(OnPeripheralChanged_EndOfFrame());
        }
    }
}
