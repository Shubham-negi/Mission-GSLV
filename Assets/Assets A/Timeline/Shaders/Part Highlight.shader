// A highlighted part: lit like the Standard shader it replaces, with a glow from its own texture and a Fresnel rim that
// follows the edges of the shape as seen from the eye. The rim replaces the surface rather than adding to it, so an edge
// on a white part stays the rim's colour instead of burning out to white.
// PartHighlight sets _Color, _EmissionColor, _RimColor and _RimPower for each renderer while the part is highlighted.
Shader "GSLV/Part Highlight"
{
    Properties
    {
        _Color ("Colour", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
        _MetallicGlossMap ("Metallic (R) Smoothness (A)", 2D) = "white" {}
        _GlossMapScale ("Smoothness Scale", Range(0,1)) = 1
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0
        [HDR] _EmissionColor ("Glow (times Albedo)", Color) = (1,1,1,1)
        [HDR] _RimColor ("Fresnel Colour", Color) = (1,0.95,0.68,1)
        _RimPower ("Fresnel Power (higher = thinner rim)", Range(0.5,8)) = 1.4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard vertex:vert fullforwardshadows nometa exclude_path:deferred exclude_path:prepass
        #pragma target 3.0
        #pragma shader_feature _METALLICGLOSSMAP

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _MetallicGlossMap;
        fixed4 _Color;
        half _GlossMapScale;
        half _Glossiness;
        half _Metallic;
        half4 _EmissionColor;
        half4 _RimColor;
        half _RimPower;

        struct Input
        {
            float2 uv_MainTex;
            float facing;   // 1 where the surface faces the eye, 0 at the edge of the shape
        };

        // per vertex: one sum for each corner instead of each pixel of each eye, and it needs no tangents
        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.facing = dot(UnityObjectToWorldNormal(v.normal), normalize(UnityWorldSpaceViewDir(worldPos)));
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed3 tex = tex2D(_MainTex, IN.uv_MainTex).rgb;
            half rim = pow(1.0 - saturate(IN.facing), _RimPower);

            o.Albedo = tex * _Color.rgb * (1.0 - rim);
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
        #ifdef _METALLICGLOSSMAP
            fixed4 mg = tex2D(_MetallicGlossMap, IN.uv_MainTex);
            o.Metallic = mg.r;
            o.Smoothness = mg.a * _GlossMapScale;
        #else
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        #endif
            o.Emission = lerp(tex * _EmissionColor.rgb, _RimColor.rgb, rim);
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
