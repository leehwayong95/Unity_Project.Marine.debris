Shader "Hydroform/UnderwaterFilter"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    uniform sampler2D_float _CameraDepthTexture;
    sampler _HydroUnderwaterMaskTex;
    sampler _HydroVolumeMaskTex;

    uniform float4 _MainTex_TexelSize;
    uniform float4x4 _FrustumCornersWS;
    uniform float4 _CameraWS;

    uniform float4 _HydroUnderwaterColor;
    uniform float4 _HydroUnderwaterFogColor;
    uniform float _HydroWaveHeightAtCam;

    uniform float4 _HydroUnderwaterData1;  // x is clarity, y is fog density

    uniform float _HydroUnderwaterClarity;

    uniform float4 _HeightParams;  // for height fog

    float4 _UnderwaterFogTopColor;
    float4 _UnderwaterFogBottomColor;
    float4 _UnderwaterOverlayColor;
    float4 _UnderwaterLipColor;
    float4 _UnderwaterData;    // x = fogDensity


    //-------------------------------------
    // data
    //-------------------------------------
    struct v2f {
        float4 pos : SV_POSITION;
        float4 uv : TEXCOORD0;
        float2 uv_depth : TEXCOORD1;
        float4 interpolatedRay : TEXCOORD2;
    };

    struct v2f2 {
        float4 pos : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    //-------------------------------------
    // vert
    //-------------------------------------
    v2f vert( appdata_img v )
    {
        v2f o;
        v.vertex.z = 0.1;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv.xy = v.texcoord.xy;
        o.uv.zw = v.texcoord.xy;  // put same tex coords in 2 places because only the maintex needs to be flipped upside down if the UV starts at top
        o.uv_depth = v.texcoord.xy;

        #if UNITY_UV_STARTS_AT_TOP
        if( _MainTex_TexelSize.y < 0 )
            o.uv.y = 1-o.uv.y;
        #endif              

        int frustumIndex = v.texcoord.x + (2 * o.uv.y);
        o.interpolatedRay = _FrustumCornersWS[frustumIndex];
        o.interpolatedRay.w = frustumIndex;  // is this necessary?

        return o;
    }


    // Linear half-space fog, from https://www.terathon.com/lengyel/Lengyel-UnifiedFog.pdf
    float ComputeHalfSpace (float3 wsDir, float4 _HeightParams )
    {
        float3 wpos = _CameraWS + wsDir;
        float FH = _HeightParams.x;
        float3 C = _CameraWS;
        float3 V = wsDir;
        float3 P = wpos;
        float3 aV = _HeightParams.w * V;
        float FdotC = _HeightParams.y;
        float k = _HeightParams.z;
        float FdotP = P.y-FH;
        float FdotV = wsDir.y;
        float c1 = k * (FdotP + FdotC);
        float c2 = (1-2*k) * FdotP;
        float g = min(c2, 0.0);
        g = -length(aV) * (c1 - g * g / abs(FdotV+1.0e-5f));
        return g;
    }

    //-------------------------------------
    // frag
    //-------------------------------------
    half4 frag( v2f inData ) : SV_Target
    {
        half4 sceneColor = tex2D(_MainTex, UnityStereoTransformScreenSpaceTex(inData.uv.xy));
        half4 underwaterMask = tex2D(_HydroUnderwaterMaskTex, UnityStereoTransformScreenSpaceTex(inData.uv.zw));

        //return underwaterMask;
        //return sceneColor;

        // Reconstruct world space position & direction
        // towards this screen pixel.
        float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(inData.uv_depth));

        float dpth = Linear01Depth(rawDepth);

        float4 wsDir = dpth * inData.interpolatedRay;  // keep in mind interpolatedRay.w is just and index so w is bunk at this point
        float4 wsPos = _CameraWS + wsDir;

        float dist = ComputeHalfSpace( wsDir, _HeightParams );
        dist -= _ProjectionParams.y;


        float4 outColor = sceneColor;



        float pixSize = 1 / _ScreenParams.y;  // THIS IS ASSUMING VIEPORT AND MASK TEX SIZE THE SAME


        // volume mask check to remove underwater fx if within a volume mask
        float volumeMask = tex2D(_HydroVolumeMaskTex, UnityStereoTransformScreenSpaceTex(inData.uv.xy)).r;


        if( underwaterMask.g>0.9  )
        {

        if( volumeMask >= 65535 ) return sceneColor;

            float4 underwaterColor = _UnderwaterOverlayColor;
            
            // render water "lip" / thickness where it intersects cam near plane
            for( uint i=1; i<7; ++i )
            {
                float4 texCoord = float4( inData.uv.zw + float2(0,i*pixSize), 0, 0 );
                float4 testPix = tex2Dlod( _HydroUnderwaterMaskTex, UnityStereoTransformScreenSpaceTex(texCoord) );
                if( testPix.g < 0.05 )
                {
                    return _UnderwaterLipColor;
//                    return lerp( outColor * 1.3, outColor, (i-1)/8.0 );
                }
            }
            
            
            if( sceneColor.a < 0.06 && sceneColor.a > 0.04 ) return outColor;
            

            float fogDensity = _UnderwaterData.x;

            // water gets a little darker as it goes deeper
            float rayAngle = dot( float3( 0,-1,0), normalize(wsDir.xyz) );
            rayAngle = max( 0, rayAngle );
            rayAngle *= 1000;
            rayAngle = min( 1, (rayAngle*rayAngle * 0.1) / 4000  );
            float4 underwaterFogColor = lerp( _UnderwaterFogTopColor, _UnderwaterFogBottomColor, rayAngle );

            // fog
            float fogFactor = (dist * dist * fogDensity * 0.00001 );  // cheap expo fog
            fogFactor = saturate( fogFactor );            
            outColor = lerp( outColor, underwaterFogColor, fogFactor );


            outColor *= underwaterColor;
        }
        
        return outColor;
    }

    //-------------------------------------
    // passthroughVert
    //-------------------------------------
    v2f2 passthroughVert( appdata_img v )
    {
        v2f2 o;

        half index = v.vertex.z;
        v.vertex.z = 0.1;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord.xy;

        return o;
    }

    //-------------------------------------
    // passthroughFrag
    //-------------------------------------
    half4 passthroughfrag( v2f_img inData ) : SV_Target
    {
        return tex2D( _MainTex, UnityStereoTransformScreenSpaceTex(inData.uv) );
    }

	ENDCG

	SubShader
	{
		ZTest Always 
        Cull Off 
        ZWrite Off
		Fog { Mode off }

		// 0
		Pass
		{
			CGPROGRAM

                #pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest 

			ENDCG
		}

        // 1
        Pass
        {
            CGPROGRAM

                #pragma vertex passthroughVert
                #pragma fragment passthroughfrag
                #pragma fragmentoption ARB_precision_hint_fastest 

            ENDCG
        }

	}

	FallBack off
}
