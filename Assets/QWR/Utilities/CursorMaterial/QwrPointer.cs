/************************************************************************************

Copyright   :   Copyright 2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace qwr
{
	[RequireComponent(typeof(Renderer))]
    public class QwrPointer : QwrBasePointer
	{
        /// <summary>
        /// Number of segments making the reticle circle.
        /// </summary>
        public int reticleSegments = 20;

        /// <summary>
        /// Growth speed multiplier for the reticle/
        /// </summary>
        public float reticleGrowthSpeed = 8.0f;

        public bool autoScale = false;

        /// <summary>
        /// Private members
        /// </summary>
        private Material materialComp;

        /// <summary>
        /// Current inner angle of the reticle (in degrees).
        /// </summary>
        private float reticleInnerAngle = 0.0f;
        /// <summary>
        /// Current outer angle of the reticle (in degrees).
        /// </summary>
        private float reticleOuterAngle = 0.5f;

        /// <summary>
        /// Minimum inner angle of the reticle (in degrees).
        /// </summary>
        private const float kReticleMinInnerAngle = 0.0f;
        /// <summary>
        /// Minimum outer angle of the reticle (in degrees).
        /// </summary>
        private const float kReticleMinOuterAngle = 0.5f;
        /// <summary>
        /// Angle at which to expand the reticle when intersecting with an object
		/// (in degrees).
        /// </summary>
        public float kReticleGrowthAngle = 0.5f;

        /// <summary>
        /// Minimum distance of the reticle (in meters).
        /// </summary>
        private const float kReticleDistanceMin = 0.45f;

        /// <summary>
        /// Current inner and outer diameters of the reticle,
		/// before distance multiplication.
        /// </summary>
        private float reticleInnerDiameter = 0.0f;
		private float reticleOuterDiameter = 0.0f;

		void Awake()
		{
            gameObject.layer = 31;
            // Graphics.DrawMesh();
		}

		protected void Start()
		{
			CreateReticleVertices();

			materialComp = gameObject.GetComponent<Renderer>().material;
        }

		void Update()
		{
            UpdateDiameters();
		}

        /// <summary>
        /// Called when the user is pointing at valid GameObject. This can be a 3D
		/// or UI element.
		///
		/// The targetObject is the object the user is pointing at.
		/// The intersectionPosition is where the ray intersected with the targetObject.
		/// The intersectionRay is the ray that was cast to determine the intersection.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        public override void OnEnterObject(GameObject targetObject)
		{
            if (targetObject == null)
                return;

            SetPointerTarget(targetObject);
        }

        public override void OnHoverObject(GameObject targetObject)
		{
			SetPointerTarget(targetObject);
		}

        public override void OnExitObject(GameObject targetObject)
		{
            reticleInnerAngle = kReticleMinInnerAngle;
            reticleOuterAngle = kReticleMinOuterAngle;
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

			const float kTwoPi = Mathf.PI * 2.0f;
			int vi = 0;
			for (int si = 0; si <= segments_count; ++si)
			{
				// Add two vertices for every circle segment: one at the beginning of the
				// prism, and one at the end of the prism.
				float angle = (float)si / (float)(segments_count) * kTwoPi;

				float x = Mathf.Sin(angle) * 0.5f;
				float y = Mathf.Cos(angle) * 0.5f;

				vertices[vi++] = new Vector3(x, y, 0.0f); // Outer vertex.
				vertices[vi++] = new Vector3(x, y, 1.0f); // Inner vertex.
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
			mesh.triangles = indices;
			mesh.RecalculateBounds();
			;

            
		}

		

		private void UpdateDiameters()
		{
			
				if (reticleInnerAngle < kReticleMinInnerAngle)
				{
					reticleInnerAngle = kReticleMinInnerAngle;
				}

				if (reticleOuterAngle < kReticleMinOuterAngle)
				{
					reticleOuterAngle = kReticleMinOuterAngle;
				}

				float inner_half_angle_radians = Mathf.Deg2Rad * reticleInnerAngle * 0.5f;
				float outer_half_angle_radians = Mathf.Deg2Rad * reticleOuterAngle * 0.5f;

				float inner_diameter = 2.0f * Mathf.Tan(inner_half_angle_radians);
				float outer_diameter = 2.0f * Mathf.Tan(outer_half_angle_radians);

				if (isHovering )
				{
					reticleInnerDiameter = inner_diameter;
					reticleOuterDiameter = outer_diameter;
				}

				else
				{

					reticleInnerDiameter =
						Mathf.Lerp(reticleInnerDiameter, inner_diameter, Time.deltaTime * reticleGrowthSpeed);
					reticleOuterDiameter =
						Mathf.Lerp(reticleOuterDiameter, outer_diameter, Time.deltaTime * reticleGrowthSpeed);
				}

				materialComp.SetFloat("_InnerDiameter", reticleInnerDiameter * transform.localPosition.magnitude);
				materialComp.SetFloat("_OuterDiameter", reticleOuterDiameter * transform.localPosition.magnitude);
		

		}


		bool isHovering;
		private void SetPointerTarget(GameObject targetObject)
		{
            bool isInteractive = (ExecuteEvents.GetEventHandler<IPointerEnterHandler>(targetObject) != null);

            if (isInteractive)
            {
				isHovering = true;
                reticleInnerAngle = kReticleMinInnerAngle + kReticleGrowthAngle;
                reticleOuterAngle = kReticleMinOuterAngle + kReticleGrowthAngle;

            }
            else
            {
				isHovering = false;
                reticleInnerAngle = kReticleMinInnerAngle;
                reticleOuterAngle = kReticleMinOuterAngle;
            }
        }
    }
}
