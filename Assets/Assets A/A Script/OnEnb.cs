using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnb : MonoBehaviour
{
    public float _delayTime;
    public GameObject _activeObject;

    void OnEnable()
    {
        Invoke("DeactiveObject", _delayTime);
    }

    void DeactiveObject()
    {
        _activeObject.SetActive(true);
    }
}