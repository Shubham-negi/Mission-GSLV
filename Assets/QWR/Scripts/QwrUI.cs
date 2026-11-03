/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;

namespace qwr
{
	[RequireComponent(typeof(Camera))]
	[System.Obsolete]
	public class QwrUI : MonoBehaviour
	{
		public enum UITYPE
		{
			GUI = 1,
			CURSOR = 2,
		}
		public UITYPE UIType = UITYPE.GUI;

		[SerializeField]
		private Rect viewport = new Rect(0, 0, 1, 1);
		public float depth = 0.0f;
		public Camera _canvas_camera { get; private set; }
		private RenderTexture[] _canvas_texture = new RenderTexture[Common.NUM_BUFFER];
		private IntPtr[] _canvas_ptr = new IntPtr[Common.NUM_BUFFER];
		private int index = 0;

		public void QwrSetViewport (Rect view)
		{
			viewport = view;
			Reshape();
		}
		public Rect QwrGetViewport ()
		{
			return viewport;
		}

		// Use this for initialization
		void Start()
		{
			_canvas_camera = GetComponent<Camera>();
			Reshape();
			#if UNITY_ANDROID && !UNITY_EDITOR
			#else
			if (0.0f == depth)
			{
				Composer.QwruSetTextureEx((int)UIType, _canvas_ptr[index], 0, (int)qwrcTwType.DISTORTION, new qwrRect(viewport));
				Composer.QwruSetTextureEx((int)UIType, _canvas_ptr[index], 1, (int)qwrcTwType.DISTORTION, new qwrRect(viewport));
			}
			else
			{
				Rect viewport_left = new Rect(viewport.x + QwrManager.DeviceInfo.ipd / (4 * depth * (float)Math.Tan((Math.PI / 360) * QwrManager.DeviceInfo.fov_x)), viewport.y, viewport.width, viewport.height);
				Rect viewport_right = new Rect(viewport.x - QwrManager.DeviceInfo.ipd / (4 * depth * (float)Math.Tan((Math.PI / 360) * QwrManager.DeviceInfo.fov_x)), viewport.y, viewport.width, viewport.height);
				Composer.QwruSetTextureEx((int)UIType, _canvas_ptr[index], 0, (int)qwrcTwType.DISTORTION, new qwrRect(viewport_left));
				Composer.QwruSetTextureEx((int)UIType, _canvas_ptr[index], 1, (int)qwrcTwType.DISTORTION, new qwrRect(viewport_right));
			}
            #endif

#if UNITY_ANDROID && (UNITY_5_5_0 || UNITY_5_4_3)
            // In Unity 5.5.0 and Unity 5.4.3,
            // surface is deleted and rebuilt after the first frame is completed by Unity, the second frame will be black screen.
            // So, Skip the rendering of the first frame to avoid flickering.
            StartCoroutine(Coroutine_EnableCamera());
#endif

#if UNITY_EDITOR
            InitEditorMultiLayer();
#endif
        }

#if UNITY_EDITOR
        Material qwrLayerMaterial;

        void InitEditorMultiLayer()
        {
            CommandBuffer commandbuff = new CommandBuffer();

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

            qwrLayerMaterial = Resources.Load<Material>("QWR/QwrUILayer");

            commandbuff.DrawMesh(mesh, Matrix4x4.identity, qwrLayerMaterial);

            QwrCameraRig._instance._center_eye.AddCommandBuffer(CameraEvent.AfterEverything, commandbuff);
        }
#endif

		void Reshape()
		{
			for (int i = 0; i < Common.NUM_BUFFER; i++)
			{
				_canvas_texture[i] = new RenderTexture
					((int)(QwrManager.DeviceInfo.resolution_x * viewport.width), (int)(QwrManager.DeviceInfo.resolution_y * viewport.height)
					 , 24, RenderTextureFormat.Default);
				_canvas_texture[i].antiAliasing
					= 2;
				_canvas_texture[i].Create();
				_canvas_ptr[i] = _canvas_texture[i].GetNativeTexturePtr();
			}
		}

		void OnDestroy ()
		{
			Composer.QwruSetTextureEx((int)UIType, IntPtr.Zero, 2, (int)qwrcTwType.NONE, new qwrRect(viewport));
			for (int i = 0; i < Common.NUM_BUFFER; i++)
			{
				if (_canvas_texture[i].IsCreated())
				{
					_canvas_texture[i].Release();
				}
			}
		}

		// Update is called once per frame
		void LateUpdate()
		{
			index = (index + 1) % Common.NUM_BUFFER;
			_canvas_camera.targetTexture = _canvas_texture[index];
#if UNITY_EDITOR
			qwrLayerMaterial.SetTexture("albedo", _canvas_camera.targetTexture);
#endif
		}

		void OnPostRender()
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			qwrRect view = new qwrRect(new Rect(0, 0, _canvas_texture[index].width, _canvas_texture[index].height));
			IntPtr tempPtr = Marshal.AllocHGlobal(Marshal.SizeOf(view));
			Marshal.StructureToPtr(view, tempPtr, false);
			Composer.PostRender(RENDER_EVENT.Posttransparent, (int)tempPtr);
			if (0.0f == depth)
			{
				Composer.QwruSetTextureEx((int)UIType, _canvas_ptr[index], 2, (int)qwrcTwType.DISTORTION, new qwrRect(viewport));
			}
			else
			{
				Rect viewport_left = new Rect(viewport.x + QwrManager.DeviceInfo.ipd / (4 * depth * (float)Math.Tan((Math.PI / 360) * QwrManager.DeviceInfo.fov_x)), viewport.y, viewport.width, viewport.height);
				Rect viewport_right = new Rect(viewport.x - QwrManager.DeviceInfo.ipd / (4 * depth * (float)Math.Tan((Math.PI / 360) * QwrManager.DeviceInfo.fov_x)), viewport.y, viewport.width, viewport.height);
				Composer.QwruSetTextureEx((int)UIType, _canvas_ptr[index], 0, (int)qwrcTwType.DISTORTION, new qwrRect(viewport_left));
				Composer.QwruSetTextureEx((int)UIType, _canvas_ptr[index], 1, (int)qwrcTwType.DISTORTION, new qwrRect(viewport_right));
			}
#endif
        }

#if UNITY_ANDROID && (UNITY_5_5_0 || UNITY_5_4_3)
        IEnumerator Coroutine_EnableCamera()
        {
            if(_canvas_camera)
                _canvas_camera.enabled = false;

            yield return new WaitForEndOfFrame();

            if (_canvas_camera)
                _canvas_camera.enabled = true;
        }
#endif
        private void OnEnable()
        {
            if (QwrManager.IsScriptRenderingPipeline)
            {
#if UNITY_2019_1_OR_NEWER
                RenderPipelineManager.endCameraRendering += EndCameraRendering;
#endif
            }

        }

        private void OnDisable()
        {
            if (QwrManager.IsScriptRenderingPipeline)
            {
#if UNITY_2019_1_OR_NEWER
                RenderPipelineManager.endCameraRendering -= EndCameraRendering;
#endif
            }
        }

#if UNITY_2019_1_OR_NEWER
        void EndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if(camera == this._canvas_camera)
            {
                OnPostRender();
            }
        }
#endif
    }
}
