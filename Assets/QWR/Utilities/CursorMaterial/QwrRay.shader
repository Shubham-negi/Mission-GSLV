// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/************************************************************************************

Copyright   :   Copyright 2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

Qwr Developer Website: http://developer.qwr.asia

************************************************************************************/
Shader "Qwr/ControllerRay"
{
	Properties
	{
		_Color("Color", Color) = (0, 0, 0, 0.2)
	}
	SubShader
	{
		Tags{ "Queue" = "Overlay" }

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			AlphaTest Off
			Cull Off
			Lighting Off
			ZWrite Off
			ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			uniform float4 _Color;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float alpha : COLOR4;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.alpha = - v.vertex.z * 0.1;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 ret = _Color;
				ret.a = 1.0;
				return ret;
			}
			ENDCG
		}
	}
}
