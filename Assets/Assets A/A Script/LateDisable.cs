using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LateDisable : MonoBehaviour
{
    public float _delayTime;
    public List<GameObject> _disableObjects;

    void OnEnable()
    {
        Invoke("DeactiveObject", _delayTime);
    }

    void DeactiveObject()
    {
        foreach (var activeObject in _disableObjects)
        {
            activeObject.SetActive(false);
        }
    }
}
