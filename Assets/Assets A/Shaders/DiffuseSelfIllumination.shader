//Author: Sumit Tyagi
Shader "CrossSection/Diffuse Self-Illumination"{
	Properties{
		_Diffuse		("Diffuse Texture (RGB)", 2D)		= "white"{}
		_Color			("Fresnel Color (RGB)", Color)		= (1.0, 1.0, 1.0, 1.0)
		_Factor			("Fresnel Factor", float)			= 0.5
		_Illumination	("Illumination (RGBA)", 2D)			= "gray" {}
		_Emission		("Emission", Range(0.0, 1.0))		= 1.0
		_PlaneNormal("PlaneNormal",Vector) = (0,1,0,0)
		_PlanePosition("PlanePosition",Vector) = (0,0,0,1)
		_StencilMask("Stencil Mask", Range(0, 255)) = 255
		_CrossColor("Cross Section Color", Color) = (1,1,1,1)
	}
	
	SubShader{
		Tags{ "RenderType" = "Opaque" }

		Stencil
		{
			Ref [_StencilMask]
			CompBack Always
			PassBack Replace

			CompFront Always
			PassFront Zero
		}
		Cull Back
		CGPROGRAM
		#pragma surface surf MobileFresnel noambient exclude_path:prepass noforwardadd
		#pragma target 2.0
		
		struct Input{
			half2 uv_Diffuse, uv_Illumination;
			float3 worldPos;
		};
		
		sampler2D	_Diffuse, _Illumination;
		half4		_Color;
		half		_Factor;
		fixed		_Wrapping, _Emission;

	  fixed3 _PlaneNormal;
		fixed3 _PlanePosition;
		bool checkVisability(fixed3 worldPos)
		{
			float dotProd1 = dot(worldPos - _PlanePosition, _PlaneNormal);
			return dotProd1 > 0  ;
		}
		
		fixed4 LightingMobileFresnel(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten){


			
			fixed nlvDot = dot(s.Normal, normalize(lightDir + viewDir));
			fixed wrap = atten * nlvDot * nlvDot;
			fixed fres = _Factor * (1.0 - dot(viewDir, s.Normal));
			
			fixed4 c;
			c.rgb = fres + (wrap * s.Albedo * _LightColor0.rgb);
			c.a = s.Alpha;
			return c;
		}

	
		
		void surf(Input IN, inout SurfaceOutput o){

		if (checkVisability(IN.worldPos))discard;
			fixed4 illum = tex2D(_Illumination, IN.uv_Illumination);
			if (checkVisability(IN.worldPos))
			o.Albedo = tex2D(_Diffuse, IN.uv_Diffuse).rgb;
			o.Emission = illum.a * illum.rgb * _Emission;
			if (checkVisability(IN.worldPos))discard;
		}
		
		ENDCG



		Cull Front
			CGPROGRAM
#pragma surface surf NoLighting  noambient

		struct Input {
			half2 uv_MainTex;
			float3 worldPos;

		};
		sampler2D _MainTex;
		fixed4 _Color;
		fixed4 _CrossColor;
		fixed3 _PlaneNormal;
		fixed3 _PlanePosition;
		bool checkVisability(fixed3 worldPos)
		{
			float dotProd1 = dot(worldPos - _PlanePosition, _PlaneNormal);
			return dotProd1 >0 ;
		}
		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			if (checkVisability(IN.worldPos))discard;
			o.Albedo = _CrossColor;

		}
			ENDCG
		


	}





	FallBack "Mobile/Diffuse"
}