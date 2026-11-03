Shader "Glass Reflective" {
	Properties {
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
		_BumpMap("NormalMap",2D)  = "white" {}
		_Cube ("Reflection Cubemap", Cube) = "black" { TexGen CubeReflect }
	}
	SubShader {
		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}
		LOD 300
		
		CGPROGRAM
			#pragma surface surf BlinnPhong decal:add nolightmap
			
			samplerCUBE _Cube;
			sampler2D _BumpMap;
			fixed4 _ReflectColor;
			half _Shininess;
			
			struct Input {
				float3 worldRefl;
				float2 uv_BumpMap;

			};
			
			void surf (Input IN, inout SurfaceOutput o) {
				o.Albedo = 0;
				o.Gloss = 1;
				o.Specular = _Shininess;
				
				fixed4 reflcol = texCUBE (_Cube, IN.worldRefl);
				o.Emission = reflcol.rgb * _ReflectColor.rgb;
				o.Alpha = reflcol.a * _ReflectColor.a;
				//o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));

			}
		ENDCG
	}
	FallBack "Transparent/VertexLit"
}