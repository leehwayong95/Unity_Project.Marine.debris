//-----------------------------------------------------------------------------
// Octave Shader
// Copyright (C) Xix Interactive, LLC  (Brian Ramage)
//-----------------------------------------------------------------------------
Shader "Hydroform/Octave" 
{
    Properties 
    {
    }
    SubShader
    {
        Tags { "HydroTag" = "true"
//               "Queue" = "Transparent"
               "ForceNoShadowCasting" = "true" }   // this saves it from being rendered twice


        Pass
        {

            Cull Off

            CGPROGRAM


            //-------------------------------------
            // data
            //-------------------------------------
            struct vert2frag
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };


            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile __ HYDRO_SPECULAR
            #pragma multi_compile __ HYDRO_SSS
            #pragma multi_compile __ HYDRO_SSS_VIEW_LIMIT
            #pragma multi_compile __ HYDRO_REFRACTION
            #pragma multi_compile __ HYDRO_SHOREFX
            #pragma multi_compile __ HYDRO_OPPOSING_WAVESETS
            #pragma multi_compile __ HYDRO_DEEP_FOAM
            #pragma multi_compile __ HYDRO_SSR
            

            #include "UnityCG.cginc"
            #include "WaterFunc.cginc"


            float _TexSize;
            

            //-------------------------------------
            // vertex shader
            //-------------------------------------
            vert2frag vert(float4 inPos:POSITION)
            {
                vert2frag vData;
                
                vData.pos = float4( inPos.xyz, 1.0 );
                vData.uv = inPos.xy * 0.5 + 0.5;

                // flip y depending on opengl vs d3d
                if (_ProjectionParams.x < 0)
                {
                    vData.uv.y = 1.0 - vData.uv.y;
                }

                return vData;
            }


            static const float2 poissonDisk[16] =
            {
                float2(0.2770745f, 0.6951455f),
                float2(0.1874257f, -0.02561589f),
                float2(-0.3381929f, 0.8713168f),
                float2(0.5867746f, 0.1087471f),
                float2(-0.3078699f, 0.188545f),
                float2(0.7993396f, 0.4595091f),
                float2(-0.09242552f, 0.5260149f),
                float2(0.3657553f, -0.5329605f),
                float2(-0.3829718f, -0.2476171f),
                float2(-0.01085108f, -0.6966301f),
                float2(0.8404155f, -0.3543923f),
                float2(-0.5186161f, -0.7624033f),
                float2(-0.8135794f, 0.2328489f),
                float2(-0.784665f, -0.2434929f),
                float2(0.9920505f, 0.0855163f),
                float2(-0.687256f, 0.6711345f)
            };



            //-------------------------------------
            // fragment shader
            //-------------------------------------
            float4 frag( vert2frag inData ) : SV_Target
            {
                float pixOffset = 1.0 / _TexSize;

                float2 pos = float2( inData.uv.x, inData.uv.y );
                
                // run crude low-pass filter on the height field so that sharp artifacts do not show in reflections
                float accum = 0.0;
                float accumDev = 0.0;
                for( int i=0; i<16; ++i )
                {
                    float dist = poissonDisk[i] * 5.0;
                    float dev = 1.0 / dist*dist;
                    accumDev += dev;
                    accum  += octave( (pos + poissonDisk[i]*pixOffset*5.0) * dev * REPEAT, 1.6 ) * 0.8;
                }
//return 0.5;
                return accum / accumDev;
            }


            ENDCG
        }
    }
    FallBack "Diffuse"
}
