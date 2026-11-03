// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.36 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.36;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3241,x:32719,y:32712,varname:node_3241,prsc:2|custl-3966-OUT,alpha-4923-OUT;n:type:ShaderForge.SFN_Fresnel,id:2631,x:31481,y:32917,varname:node_2631,prsc:2;n:type:ShaderForge.SFN_Tex2d,id:2351,x:31828,y:32434,ptovrint:False,ptlb:Main Tex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:True,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:4923,x:32038,y:32829,varname:node_4923,prsc:2|A-2351-A,B-4197-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2902,x:31481,y:33089,ptovrint:False,ptlb:Fresnel Power,ptin:_FresnelPower,varname:_FresnelPower,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:8102,x:31782,y:33033,varname:node_8102,prsc:2|A-2631-OUT,B-2902-OUT;n:type:ShaderForge.SFN_Slider,id:4197,x:31671,y:32703,ptovrint:False,ptlb:Opacity Power,ptin:_OpacityPower,varname:_OpacityPower,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Multiply,id:3966,x:32375,y:32540,varname:node_3966,prsc:2|A-2351-RGB,B-8372-OUT;n:type:ShaderForge.SFN_Add,id:8372,x:32240,y:32938,varname:node_8372,prsc:2|A-2351-A,B-8102-OUT;proporder:2351-2902-4197;pass:END;sub:END;*/

Shader "Unlit/Diffuse_Fresnel_Opacity" {
    Properties {
        [NoScaleOffset]_MainTex ("Main Tex", 2D) = "white" {}
        _FresnelPower ("Fresnel Power", Float ) = 1
        _OpacityPower ("Opacity Power", Range(0, 1)) = 1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 100
        Pass {
            Name "FORWARD"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#ifndef UNITY_PASS_FORWARDBASE
            #define UNITY_PASS_FORWARDBASE
			#endif
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            //#pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex;
            uniform float _FresnelPower;
            uniform float _OpacityPower;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
////// Lighting:
                float4 _MainTex_var = tex2D(_MainTex,i.uv0);
                float node_8102 = ((1.0-max(0,dot(normalDirection, viewDirection)))*_FresnelPower);
                float3 finalColor = (_MainTex_var.rgb*(_MainTex_var.a+node_8102));
                fixed4 finalRGBA = fixed4(finalColor,(_MainTex_var.a*_OpacityPower));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
