using UnityEngine;
using System.Collections;
public class LineRendererScript : MonoBehaviour{
	public static LineRendererScript instance;
	public Material LightBeam;
	internal LineRenderer lr;
	public Transform firstPoint,secondPoint;
	public bool isWorldSpace=true;
	public float LineWH =0.05f;

	void Start ()
	{
		instance = this;

		if (!gameObject.GetComponent<LineRenderer> ()) {
			lr = gameObject.AddComponent<LineRenderer> () as LineRenderer;
 		} else {
			lr = gameObject.GetComponent<LineRenderer> ();
		}

		SetLrProperties ();
		ResetRenderedLine ();
		InvokeRepeating ("DrawLine", 0.1f, 0.001f);
	}  	 
	void SetLrProperties ()
	{
		lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		lr.receiveShadows = false;
 		lr.material = LightBeam;
		lr.SetWidth (LineWH, LineWH);
		lr.SetVertexCount (0);
		lr.useWorldSpace = isWorldSpace; 
	} 
	void DrawLine ()
	{
 		lr.SetVertexCount (2);
		lr.SetPosition (0, firstPoint.position);
		lr.SetPosition (1, secondPoint.position);
	}
	public void ResetRenderedLine ()
	{
		lr.SetVertexCount (0);
	}
}