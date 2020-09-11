//-----------------------------------------------------------------------------
// Brim Shader
// Copyright (C) Xix Interactive, LLC  (Brian Ramage)
//-----------------------------------------------------------------------------
Shader "Hydroform/Brim"
{
    Properties 
    {
        _Cube ("Cubemap", CUBE) = "" {}
        _FoamTex ("Foam", 2D) = "" {}
    }
    SubShader
    {
        Tags { "Queue" = "AlphaTest+1"  // draw after geometry, but before translucents. +100 to draw after water
               "RenderType" = "Opaque" 
               "ForceNoShadowCasting" = "true" }   // this saves it from being rendered twice

        GrabPass {"_GrabPassTex"}

        Pass
        {

//Cull Off

            CGPROGRAM

            #pragma vertex brimVert
            #pragma fragment frag
            
            #pragma target 3.5
            
            #pragma multi_compile __ HYDRO_SPECULAR
            #pragma multi_compile __ HYDRO_SSS
            #pragma multi_compile __ HYDRO_SSS_VIEW_LIMIT

            #pragma multi_compile __ HYDRO_REFRACTION
            #pragma multi_compile __ HYDRO_SHOREFX
            #pragma multi_compile __ HYDRO_OPPOSING_WAVESETS
            #pragma multi_compile __ HYDRO_DEEP_FOAM
//            #pragma multi_compile __ HYDRO_SSR  // disable for brim
            #pragma multi_compile __ HYDRO_PLANAR_REFELCT
//            #pragma multi_compile __ HYDRO_UNDERWATER  // turn off underwater pixels for brim, keep in mind this won't work if the brim is eventually rendered underwater
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "WaterFunc.cginc"

            #define DEFAULT_DEPTH 20  // arbitrary deep number, this is mostly for shorelines so it shouldn't affect brim, although might want to test against far shorelines

           
            //-------------------------------------
            // vertex shader
            //-------------------------------------
            v2f brimVert( float4 inPos:POSITION, float2 inTex:TEXCOORD0 )
            {
                v2f vData;
                
                float3 camPos = float3( _CamPosition.x, _CamPosition.y - _HydroWaterHeight, _CamPosition.z );
                
                // rotate mesh to match camera roll
                float4 inPoint = float4( inTex.x * cos(_CamRollAngle) - inTex.y * sin(_CamRollAngle), 
                                         inTex.y * cos(_CamRollAngle) + inTex.x * sin(_CamRollAngle), 0.0, 1.0 );

                inPoint.y *= _ScreenAspect;  // spread points evenly
                inPoint.xy *= _MeshScale;  // scale the mesh up depending on camera roll

                // project ray into world
                float4 ray = -mul( _InvCamProj, inPoint );
                ray.w = 0.0;

                vData.interpRay = ray.zyx;

                ray = mul(-ray, UNITY_MATRIX_V);  // inverse cam view
//                ray = mul(ray, _CamView);  // inverse cam view


                if( ray.y >= -0.001 )
                {
                    ray.y = -0.001;  // collapse all verts above horizon to horizon

// commented out to test if removing it fixes issue where pixels are noisy at horizon
/*
                    ray.xyz = normalize( ray.xyz ) * 1e+25;

                    vData.worldPos = ray.xyz;
                    vData.worldPos.y = _HydroWaterHeight;
                    vData.distFromEyeAndDepth = float2( 1e+25, DEFAULT_DEPTH );
                    vData.pos = mul( UNITY_MATRIX_VP, float4( ray.xyz, 1.0 ) );
                    vData.screenPos = ComputeScreenPos( vData.pos );
                    vData.screenGrabPos = ComputeGrabScreenPos( vData.pos );

                    return vData;
*/                    
                }


                float4 farRay = ray;
                
                // do intersection with water plane, assume it is at z=0;
                ray *= abs( camPos.y / ray.y );

                // calc world position
                vData.worldPos = camPos + ray.xyz;
                vData.worldPos.y = _HydroWaterHeight -0.25;  // 0.25 adjustment fixes brim poking through the regular mesh in shallows

                
                // rotate it to view space to get proper distance from eye
                vData.distFromEyeAndDepth.x = -mul(UNITY_MATRIX_V, float4( vData.worldPos, 1.0 )).z;
//                vData.distFromEye = mul(_CamView, float4( vData.worldPos, 1.0 )).z;

                vData.distFromEyeAndDepth.y = getDepth( vData.worldPos );


                float3 outVecPos = vData.worldPos;

                // project back into screen space, now with proper z and w coordinates
                vData.pos = mul( UNITY_MATRIX_VP, float4( outVecPos, 1.0 ) );
                vData.screenPos = ComputeScreenPos( vData.pos );
                vData.screenGrabPos = ComputeGrabScreenPos( vData.pos );
               
                return vData;
            }


            //-------------------------------------
            // fragment shader
            //-------------------------------------
            fixed4 frag( v2f inData ) : SV_Target
            {
                return calcPixel( inData );
            }


            ENDCG
        }
    }
}
