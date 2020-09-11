//-----------------------------------------------------------------------------
// Water Shader
// Copyright (C) Xix Interactive, LLC  (Brian Ramage)
//-----------------------------------------------------------------------------
Shader "Hydroform/Water" 
{
    Properties 
    {
        _Cube ("Cubemap", CUBE) = "" {}
        _FoamTex ("Foam", 2D) = "" {}
    }


    SubShader
    {
        Tags { "Queue" = "AlphaTest"  // draw after geometry, but before translucents
               "RenderType" = "Opaque" 
               "ForceNoShadowCasting" = "true" }   // this saves it from being rendered twice

//        Lod 110

        GrabPass {"_GrabPassTex"}

        Pass
        {
            Cull Off

            CGPROGRAM

            #pragma target 3.5
            #pragma multi_compile __ HYDRO_SPECULAR
            #pragma multi_compile __ HYDRO_SSS
            #pragma multi_compile __ HYDRO_SSS_VIEW_LIMIT
            #pragma multi_compile __ HYDRO_REFRACTION
            #pragma multi_compile __ HYDRO_SHOREFX
            #pragma multi_compile __ HYDRO_OPPOSING_WAVESETS
            #pragma multi_compile __ HYDRO_DEEP_FOAM
            #pragma multi_compile __ HYDRO_SSR
            #pragma multi_compile __ HYDRO_PLANAR_REFELCT
            #pragma multi_compile __ HYDRO_UNDERWATER
            #pragma multi_compile_fog
            
            #pragma vertex calcVert
            #pragma fragment calcPixel
            
            #include "UnityCG.cginc"
            #include "WaterFunc.cginc"

            ENDCG
        }
    }
    
    
    
    
//    FallBack "Diffuse"  // don't do fallback, it'll just render a square
}
