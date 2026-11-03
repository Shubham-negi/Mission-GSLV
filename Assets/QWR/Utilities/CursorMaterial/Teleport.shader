/************************************************************************************

Copyright   :   Copyright 2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

Qwr Developer Website: http://developer.qwr.asia

************************************************************************************/

Shader "Qwr/Teleport"
{
  Properties
  {
    _MainTex ("Texture (A)", 2D) = "" {}
    _Color ("Color Overlay", Color) = (1,1,1,1)
    _HighlightColor ("Highlight Tint", Color) = (0.63,0.52,0.38,0.66)
    _ShadowColor ("Shadow Tint", Color) = (0.96,1,1,0.85)
  }

  SubShader
  {
    Tags { "Queue"="Geometry" "RenderType"="Geometry"}

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 2.0

      #include "UnityCG.cginc"
	  #include "QwrUnityCompatibility.cginc"

      struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;

      v2f vert (appdata v) {
        v2f o;
		o.vertex = QwrUnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        return o;
      }

      fixed4 _HighlightColor;
      fixed4 _ShadowColor;
      fixed4 _Color;

	  fixed4 frag(v2f i) : SV_Target{
        fixed alpha =  tex2D(_MainTex, i.uv).a;
        fixed3 highlight = max(fixed3(0,0,0),(alpha*alpha)*_HighlightColor.rgb - (1 - _HighlightColor.a));
		fixed3 shadow = max(fixed3(0, 0, 0), (alpha*_ShadowColor.rgb - Luminance(highlight)*_ShadowColor.a));
        fixed4 col = fixed4(highlight + shadow,0)*_Color;
        return col;
      }
      ENDCG
    }
  }
}
