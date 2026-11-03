using UnityEngine;

namespace GslvSpace
{
    /// <summary>Turns an object steadily around one of its local axes while the scene is playing.</summary>
    public class SlowSpin : MonoBehaviour
    {
        [Tooltip("Degrees per second.")]
        public float speed = 3f;
        public Vector3 localAxis = Vector3.up;

        void Update()
        {
            transform.Rotate(localAxis, speed * Time.deltaTime, Space.Self);
        }
    }
}
