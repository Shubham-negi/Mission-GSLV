
using UnityEngine;

namespace qwr
{
    public class BatteryPower : MonoBehaviour
    {

        const int textureNum = 10;
        int textureIndex = textureNum - 1;

        public Texture[] textures = new Texture[textureNum];

        Material material = null;

        void Awake()
        {
            Renderer renderer = GetComponent<Renderer>();
            if(renderer != null)
                material = renderer.material;
        }

        void Update()
        {
            if (material != null)
            {
                int power = QwrDaydreamController.BatteryPower;
                if (power < 0 || power > 100)
                    return;

                int index = Mathf.Clamp((QwrDaydreamController.BatteryPower -1) / textureNum, 0, textureNum - 1);
                if (textureIndex == index)
                    return;

                textureIndex = index;

                material.SetTexture("_MainTex", textures[textureIndex]);
            }


        }

    }
}
