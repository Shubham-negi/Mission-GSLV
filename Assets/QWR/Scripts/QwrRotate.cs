using UnityEngine;
using System.Collections;

public class QwrRotate : MonoBehaviour
{

    Quaternion rotated = Quaternion.identity;
    Vector3 moved = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            transform.rotation *= Quaternion.Inverse(rotated);
            rotated = Quaternion.identity;
            transform.position += -moved;
            moved = Vector3.zero;
            return;
        }
        Quaternion value = Quaternion.identity;
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            value *= Quaternion.Euler(new Vector3(-10.0f / 60.0f, 0, 0));
        }
        if (Input.GetKey(KeyCode.S))
        {
            value *= Quaternion.Euler(new Vector3(10.0f / 60.0f, 0, 0));
        }
        if (Input.GetKey(KeyCode.A))
        {
            value *= Quaternion.Euler(new Vector3(0, -10.0f / 60.0f, 0));
        }
        if (Input.GetKey(KeyCode.D))
        {
            value *= Quaternion.Euler(new Vector3(0, 10.0f / 60.0f, 0));
        }
        if (Input.GetKey(KeyCode.Q))
        {
            value *= Quaternion.Euler(new Vector3(0, 0, 10.0f / 60.0f));
        }
        if (Input.GetKey(KeyCode.E))
        {
            value *= Quaternion.Euler(new Vector3(0, 0, -10.0f / 60.0f));
        }

        if (Input.GetKey(KeyCode.I))
        {
            move += new Vector3(0, 0, 10.0f / 600.0f);
        }
        if (Input.GetKey(KeyCode.K))
        {
            move += new Vector3(0, 0, -10.0f / 600.0f);
        }
        if (Input.GetKey(KeyCode.J))
        {
            move += new Vector3(-10.0f / 600.0f, 0, 0);
        }
        if (Input.GetKey(KeyCode.L))
        {
            move += new Vector3(10.0f / 600.0f, 0, 0);
        }
        if (Input.GetKey(KeyCode.U))
        {
            move += new Vector3(0, 10.0f / 600.0f, 0);
        }
        if (Input.GetKey(KeyCode.O))
        {
            move += new Vector3(0, -10.0f / 600.0f, 0);
        }

        rotated *= value;
        transform.rotation *= value;
        moved += move;
        transform.position += move;
    }
}
