//-----------------------------------------------------------------------------
// Cage Shader
// Copyright (C) Xix Interactive, LLC  (Brian Ramage)
//-----------------------------------------------------------------------------
Shader "Hydroform/Cage"
{
    Properties 
    {
//        _Cube ("Cubemap", CUBE) = "" {}
        _Color ("Color",Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "Queue" = "AlphaTest+1"  // draw after geometry, but before translucents. +100 to draw after water
               "RenderType" = "Opaque" 
               "ForceNoShadowCasting" = "true" }   // this saves it from being rendered twice


        Pass
        {

Cull Off

            CGPROGRAM

            #pragma vertex cageVert
            #pragma fragment cageFrag
            
            #pragma target 3.5
            

            #pragma multi_compile __ HYDRO_SHOREFX  
            #pragma multi_compile __ HYDRO_OPPOSING_WAVESETS
            #pragma multi_compile __ HYDRO_DEEP_FOAM


            #include "UnityCG.cginc"
            #include "WaterFunc.cginc"

            struct v2f2
            {
                float4 pos : SV_POSITION;
                float3 eyeRelativePos : TEXCOORD0;
                float bottom: TEXCOORD1;  // used to distinguish the bottom square from the top mesh
            };

            float4 _CageStartPos;
           
            //-------------------------------------
            // vertex shader
            //-------------------------------------
            v2f2 cageVert( float4 inPos:POSITION, float3 inTex:TEXCOORD0 )
            {
                v2f2 vData;
                
                float3 camPos = float3( _CamPosition.x, _CamPosition.y - _HydroWaterHeight, _CamPosition.z );

                
                float3 mainPnt = _CageStartPos.xyz;

                float bottom = 0;

                float3 pnt;
                
                pnt = mainPnt + float3( inTex.x, inTex.y * -10, inTex.z ); // 100 is draw down 100 meters


                if( pnt.y > -1 )
                {
                    pnt.y = getVertexHeight( pnt * WAVE_SCALE, getSpeed(), _OctavesVert ) - 0.0;

                    // rotate it to view space to get proper distance from eye
                    float3 viewPos = mul( UNITY_MATRIX_V, float4( pnt, 1.0 ) );
                    float distFromEyeAndDepth = -viewPos.z;

                    // reduce wave height in distance to match up with brim
                    float distFactor = exp( -distFromEyeAndDepth * _WaveFalloff );
                    distFactor = min( 1, distFactor );  // fix bug where at high altitudes if player looks up, the waves freak out
                    pnt.y *= distFactor;

                #ifdef HYDRO_SHOREFX
                    float depth = getDepth( pnt );
                    pnt.y *= min( (depth / _ShoreData.y) + _MinWaveAmp, 1.0 );  // fade wave height if shallow depth.  _ShoreData.y = waveDampDepth
                #else
                    float depth = camHeight;
                #endif
                }
                else
                {
                    pnt.y = min( -2, camPos.y - 10 );
                    bottom = 1;
                }



//                vData.distFromEyeAndDepth.y = depth;

                pnt.y += _HydroWaterHeight;
                
                vData.eyeRelativePos = pnt - _CamPosition;  // this is used instead of world position to prevent noise artifacts when geometry is far from center: https://www.enkisoftware.com/devlogpost-20150131-1-Normal-generation-in-the-pixel-shader
                vData.bottom = bottom;

                // project into screen space
                vData.pos = mul( UNITY_MATRIX_VP, float4( pnt, 1.0 ) );
               
                return vData;
            }


            //-------------------------------------
            // fragment shader
            //-------------------------------------
            fixed4 cageFrag( v2f2 inData ) : SV_Target
            {
                float3 dFdxPos = ddx( inData.eyeRelativePos );
                float3 dFdyPos = ddy( inData.eyeRelativePos );
                float3 facenormal = normalize( cross(dFdyPos,dFdxPos ));


                // draw topside of topmesh in different color to "hide" the cage when viewed from above over terrain
                if( facenormal.y > 0.1 && inData.bottom < 0.5 )
                {
                    return fixed4( 1, 0, 0, 1 );
                }

                return fixed4( 0, 1, 0, 1 );
            }


            ENDCG
        }
    }
}
