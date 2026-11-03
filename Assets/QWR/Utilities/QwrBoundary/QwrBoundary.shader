/************************************************************************************

Copyright   :   Copyright 2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

Qwr Developer Website: http://developer.qwr.asia

************************************************************************************/
Shader "Qwr/QwrBoundary"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AlphaFraction ("Alpha", Float) = 1.0
	}
	SubShader
	{
		Tags { "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			AlphaTest Off
			Cull Off
			Lighting Off
			ZWrite Off
			ZTest Always

			Fog{ Mode Off }

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul (UNITY_MATRIX_VP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float _AlphaFraction;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col.w *= _AlphaFraction;
				return col;
			}
			ENDCG
		}
	}
}
