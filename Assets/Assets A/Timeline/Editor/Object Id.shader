// Editor-only (lives in an Editor folder, so never in a build): DeviceDepthCheck draws the scenery with this.
// Colour: x = the object's id, y = the triangle's number, z = the distance from the camera.
// Depth: what an OpenGL ES 24-bit depth buffer would hold, rounded to _GslvQuant steps of 2^-24 (0 = not rounded), so a
// render with rounding and one without differ exactly where the headset cannot tell two surfaces apart.
Shader "Hidden/GSLV/Object Id"
{
    Properties
    {
        _MainTex ("", 2D) = "white" {}
        _Color ("", Color) = (1,1,1,1)
        _Cutoff ("", Float) = 0.5
        _OffsetFactor ("", Float) = 0
        _OffsetUnits ("", Float) = 0
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    float4 _GslvId;
    float _GslvQuant;
    sampler2D _MainTex;
    float4 _MainTex_ST;
    fixed4 _Color;
    fixed _Cutoff;
    float _OffsetFactor, _OffsetUnits;

    struct v2f
    {
        float4 pos : SV_POSITION;
        float2 uv : TEXCOORD0;
        float3 world : TEXCOORD1;
    };

    struct fout
    {
        float4 color : SV_Target;
        float depth : SV_Depth;
    };

    v2f vert(appdata_base v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
        o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
        return o;
    }

    fout shade(v2f i, uint prim)
    {
        float n = _ProjectionParams.y, f = _ProjectionParams.z;
        float z = -mul(UNITY_MATRIX_V, float4(i.world, 1)).z;
        // 1 - the window depth of a standard OpenGL projection, f(z-n)/((f-n)z): 1 at the near plane, 0 at the far one
        float u = n * (f / z - 1) / (f - n);
        // the material's own polygon offset (the road's depth bias), as the GPU applies it: slope and smallest step
        u -= _OffsetFactor * max(abs(ddx(u)), abs(ddy(u))) + _OffsetUnits * 5.9604645e-8;
        if (_GslvQuant > 0) u = round(u * 16777216.0 / _GslvQuant) * _GslvQuant / 16777216.0;
        fout o;
        o.color = float4(_GslvId.x, prim, distance(i.world, _WorldSpaceCameraPos), 1);
    #if defined(UNITY_REVERSED_Z)
        o.depth = u;
    #else
        o.depth = 1 - u;
    #endif
        return o;
    }

    fout fragOpaque(v2f i, uint prim : SV_PrimitiveID)
    {
        return shade(i, prim);
    }

    fout fragCutout(v2f i, uint prim : SV_PrimitiveID)
    {
        clip(tex2D(_MainTex, i.uv).a * _Color.a - _Cutoff);
        return shade(i, prim);
    }
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma target 4.0
            #pragma vertex vert
            #pragma fragment fragOpaque
            ENDCG
        }
    }

    SubShader
    {
        Tags { "RenderType"="TransparentCutout" }
        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma target 4.0
            #pragma vertex vert
            #pragma fragment fragCutout
            ENDCG
        }
    }
}
