// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// MatCap Shader, (c) 2015-2017 Jean Moreno

Shader "CrossSection/MatCap/Vertex/Plain Additive"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MatCap ("MatCap (RGB)", 2D) = "white" {}
		_PlaneNormal("PlaneNormal",Vector) = (0,1,0,0)
		_PlanePosition("PlanePosition",Vector) = (0,0,0,1)
		_StencilMask("Stencil Mask", Range(0, 255)) = 255
	}
	
	Subshader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend One OneMinusSrcColor
		Cull Off
		Lighting Off
		ZWrite Off

		Stencil
		{
			Ref [_StencilMask]
			CompBack Always
			PassBack Replace

			CompFront Always
			PassFront Zero
		}

		Cull Back
		Pass
		{
			Tags { "LightMode" = "Always" }
			
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fog
				#include "UnityCG.cginc"
				
				struct v2f
				{
					float4 pos	: SV_POSITION;
					float2 cap	: TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float3 worldPos : TEXCOORD2;
				};
				
				v2f vert (appdata_base v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos (v.vertex);
					o.worldPos = mul (unity_ObjectToWorld, v.vertex);
					float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
					worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);

					o.cap.xy = worldNorm.xy * 0.5 + 0.5;
					
					UNITY_TRANSFER_FOG(o, o.pos);

					return o;
				}
				
				uniform float4 _Color;
				uniform sampler2D _MatCap;

				fixed3 _PlaneNormal;
		fixed3 _PlanePosition;
		bool checkVisability(fixed3 worldPos)
		{
			float dotProd1 = dot(worldPos - _PlanePosition, _PlaneNormal);
			return dotProd1 > 0  ;
		}
				
				float4 frag (v2f i) : COLOR
				{
				if (checkVisability(i.worldPos))discard;
					float4 mc = tex2D(_MatCap, i.cap) * _Color * 2.0;
					UNITY_APPLY_FOG_COLOR(i.fogCoord, mc, float4(0,0,0,0));
					return mc;
				}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}
