Shader "Qwr/hiddenmesh"
{
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite On ZTest Always
		//Tags  {"Queue"="Background"}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.0
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex.xy = v.vertex.xy * 2 - 1;
				o.vertex.zw = v.vertex.zw;
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				return float4(0, 0, 0, 1);
			}
			ENDCG
		}
	}
}
