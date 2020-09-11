// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//-----------------------------------------------------------------------------
// DepthWrite Shader
// Copyright (C) Xix Interactive, LLC
//-----------------------------------------------------------------------------

Shader "Hydroform/DepthWrite" 
{
    Properties 
    {
    }
    SubShader
    {
        Pass
        {
            Tags { "RenderType" = "Opaque" }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"


            struct v2f
            {
                float4 pos : SV_POSITION;
                float dist : TEXCOORD0;    // need to send this separately, otherwise Unity's glsl conversion fails
            };

            //-------------------------------------
            // Vertex shader
            //-------------------------------------
            v2f vert(float4 v:POSITION)
            {
                v2f vData;
                vData.pos = UnityObjectToClipPos (v);
                vData.dist = -UnityObjectToViewPos(v).z;
                return vData;
            }

            //-------------------------------------
            // Pixel shader
            //-------------------------------------
            float4 frag( v2f inData ) : SV_Target
            {
                return inData.dist;
            }


            ENDCG
        }
    }
}

