Shader "Qwr/QwrUILayer"
{
	Properties
	{
		albedo("Texture", 2D) = "black" {}
	}

	SubShader
	{
		// No culling or depth
		Cull Off ZWrite On ZTest Always
		//Tags  {"Queue"="Background"}
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 position : POSITION;
				float2 texcoord : TEXCOORD;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 texcoord : TEXCOORD;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.position.xy = v.position.xy *2 - 1;
				o.position.zw = float2(0, 1);
				o.texcoord = v.texcoord;
				return o;
			}

			sampler2D albedo;

			float4 frag (v2f i) : SV_Target
			{
				return tex2D(albedo, i.texcoord);
			}
			ENDCG
		}
	}
}
