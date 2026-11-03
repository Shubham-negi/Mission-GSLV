using UnityEngine;
using System.Collections;
using UnityEngine.VR;

public class ZoomInOut : MonoBehaviour {
	public bool AllowZoom = true;
	public Vector2 MinMaxZ;
    public float _dPos;
    public Vector2 TouchAxis;
    float speed = 0.01f;

    public GameObject _mainObject;

    void Start () {
	}

	// Update is called once per frame
	void Update () {
       // TA = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

#if UNITY_EDITOR
        TouchAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        #else
               // TouchAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
        #endif

        _dPos = gameObject.transform.localPosition.y;
        if (AllowZoom) _mainObject.transform.localScale=new Vector3(_dPos, _dPos, _dPos);
        if (AllowZoom) gameObject.transform.Translate(new Vector3(0, speed * TouchAxis.y, 0));
        if (_dPos > MinMaxZ.y)
        {
            AllowZoom = false;
            _dPos = MinMaxZ.y;
            gameObject.transform.position = new Vector3(0, _dPos, 0);
        }
        else if (_dPos < MinMaxZ.x)
        {
            _dPos = MinMaxZ.x;
            AllowZoom = false;
            gameObject.transform.position = new Vector3(0, _dPos, 0);
        }

        if(TouchAxis.y == 0) AllowZoom = true;
    }
}