// Sky seen while climbing out of the atmosphere, drawn on the inside of a large sphere that follows the learner.
// A gradient from the horizon to the zenith, a thin bright limb along Earth's visible horizon, the sun, and stars
// that show through as the sky turns black. Drawn after the skybox (queue 2501) and before the flight Earth (2502),
// without depth, so it covers the skybox and anything far away but never the rocket.
Shader "GSLV/Sky Atmosphere"
{
    Properties
    {
        _ZenithColor ("Zenith", Color) = (0.16, 0.42, 0.85, 1)
        _HorizonColor ("Horizon", Color) = (0.66, 0.80, 0.95, 1)
        _LimbColor ("Limb glow", Color) = (0.42, 0.72, 1.0, 1)
        _LimbStrength ("Limb strength", Float) = 0
        _LimbWidth ("Limb width (radians)", Float) = 0.05
        _HorizonDip ("Horizon dip (radians)", Float) = 0
        _SunDir ("Direction to the sun", Vector) = (0, 1, 0, 0)
        _SunColor ("Sun", Color) = (1, 0.95, 0.85, 1)
        _SunGlow ("Sun glow", Float) = 0.3
        _StarTex ("Stars (equirectangular)", 2D) = "black" {}
        _StarStrength ("Stars", Float) = 0
        _Opacity ("Opacity", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent-499" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Skybox" }
        Cull Front
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _ZenithColor, _HorizonColor, _LimbColor, _SunColor;
            float _LimbStrength, _LimbWidth, _HorizonDip, _SunGlow, _StarStrength, _Opacity;
            float4 _SunDir;
            sampler2D _StarTex;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 d = normalize(i.dir);
                float elev = asin(clamp(d.y, -1.0, 1.0));
                float t = elev + _HorizonDip;                                   // angle above the visible horizon
                float up = saturate(t / 1.2);
                float3 sky = lerp(_HorizonColor.rgb, _ZenithColor.rgb, sqrt(up));
                sky += _LimbColor.rgb * (_LimbStrength * exp(-(t * t) / (_LimbWidth * _LimbWidth)));
                float s = saturate(dot(d, normalize(_SunDir.xyz)));
                sky += _SunColor.rgb * (pow(s, 900.0) * 6.0 + pow(s, 24.0) * _SunGlow);
                float2 uv = float2(atan2(d.x, d.z) / (2.0 * UNITY_PI) + 0.5, elev / UNITY_PI + 0.5);
                sky += tex2D(_StarTex, uv).rgb * (_StarStrength * saturate(t * 10.0));
                return fixed4(sky, _Opacity);
            }
            ENDCG
        }
    }
}
