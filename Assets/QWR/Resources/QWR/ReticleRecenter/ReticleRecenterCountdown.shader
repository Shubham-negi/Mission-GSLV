// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Qwr/ReticleRecenterCountdown"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ProgressTex("ProgressTexture",2D) = "white"{}
        _MaskTex("AlphaMask",2D) = "white"{}
        _countdownValue("Countdown Value", Range(0,1)) = 0.5
    }
    SubShader
    {
        // No culling or depth
        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Tags{ "Queue" = "Transparent"}
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _MaskTex;
            sampler2D _ProgressTex;
            float _countdownValue;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 mainTexCol = tex2D(_MainTex, i.uv);
                fixed4 progressCol = tex2D(_ProgressTex, i.uv);

                fixed4 maskCol = tex2D(_MaskTex, i.uv);
                float progressWeight = 1 - step(_countdownValue, maskCol.a);
                progressCol.a = progressCol.a*progressWeight;

                fixed4 result = mainTexCol + progressCol* progressCol.a;
                return result;
            }
            ENDCG
        }
    }
}
