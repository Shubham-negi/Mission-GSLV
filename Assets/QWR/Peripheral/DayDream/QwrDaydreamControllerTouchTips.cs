

using UnityEngine;

namespace qwr
{
    public class QwrDaydreamControllerTouchTips : MonoBehaviour
    {
        Transform knob = null;

        void Start()
        {
            knob = transform.Find("Knob");

            if (knob != null)
                knob.gameObject.SetActive(false);
        }

        void Update()
        {
            if (knob != null)
            {
                if (QwrDaydreamController.TouchDown)
                {
                    knob.gameObject.SetActive(true);
                }
                else if (QwrDaydreamController.TouchUp)
                {
                    knob.gameObject.SetActive(false);
                }
                if (QwrDaydreamController.IsTouching)
                {
                    Vector2 position = QwrDaydreamController.TouchPos;
                    switch (QwrManager.touchPosOrig)
                    {
                        case TouchPosOrig.CENTER:
                            break;
                        case TouchPosOrig.TOP_LEFT:
                            position = position * 2.0f - Vector2.one;
                            break;
                    }

                    knob.localPosition = new Vector3(position.x, position.y, 0.0f);
                }
            }

        }
    }

}