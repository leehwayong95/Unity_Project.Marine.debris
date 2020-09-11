//-----------------------------------------------------------------------------
// DepthWrite Shader
// Copyright (C) Xix Interactive, LLC
//-----------------------------------------------------------------------------

Shader "Hydroform/VolumeMaskInterior" 
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
                // this shader is used for the case where the camera is inside a volume mask
                // the volume mask in this case, should have interior geometry that this shader is
                // attached to that is separate from the outer geometry.  See the cube in the volumeMask
                // demo for reference.
                return 65536; // return a value likely much larger than could be in scene
            }


            ENDCG
        }
    }
}

