// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Qwr/shdCube"
{
    Properties
    {
        _CubeMin("CubeMin", Range(-10,10)) = -4
        _CubeMax("CubeMax", Range(-10,10)) = 4
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };
            float _CubeMin;
            float _CubeMax;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                float4 color = (mul(unity_ObjectToWorld, v.vertex) - _CubeMin) / (_CubeMax - _CubeMin);
                o.color = float4(color.rgb, 1);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                // sample the texture
                float3 lightDirection = float3(0.2, 0.507, -0.707);
                float3 lightColor = float3(0.5, 0.5, 0.5);
                fixed3 col = i.color.rgb;
                col *= max(0.2, dot(i.normal, lightDirection));

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return fixed4(col, 0.7);
            }
            ENDCG
        }
    }
}
