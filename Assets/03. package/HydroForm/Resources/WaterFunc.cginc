//-----------------------------------------------------------------------------
// Water functions
// Copyright (C) Xix Interactive, LLC  (Brian Ramage)
//-----------------------------------------------------------------------------

//-------------------------------------
// data - moved this struct here to fix 
//        compile on Linux
//-------------------------------------
struct v2f
{
    float4 pos : SV_POSITION;
    float3 worldPos : TEXCOORD0;
    float2 distFromEyeAndDepth : TEXCOORD1;  // x = distfromeye, y = depth
    float4 screenPos : TEXCOORD2;
    float4 screenGrabPos : TEXCOORD3;
    float3 interpRay : TEXCOORD4;
};


#define CAMFAR _ProjectionParams.z

float3      _CamPosition;
float4x4    _InvCamProj;

float4x4    _CamView;

samplerCUBE _Cube;
sampler _FoamTex;

sampler _OctaveTex;
sampler _GrabPassTex;
sampler _HydroReflectTex;
sampler2D_float _CameraDepthTexture;
sampler _HydroVolumeMaskTex;
sampler _HydroHeightTex;

float _ScreenAspect;
float _CamRollAngle;
float _MeshScale;

float3 _PatchStartPos;
float  _PatchSize;

float  _HydroWaterHeight;
float  _OctavesPix;

float4  _WaveData;  // x = amplitude, y = frequency
float4  _WaveDirRotMat;
float4  _WaterColor;
float2  _FresnelData;  // x is power, y is blend

float3 _LightDir;

float4 _SpecularColor;
float _SpecularPower;

float4 _SSSColor;
float3 _SSSData;  // x is power, y is threshold, z = heightCutoff

float _InTime;
float _WaveSmoothness;

float4 _FoamColor;
float3 _FoamData;  // x = low freq, y = high freq, z = speed
float3 _DeepFoamData;  // x = low freq, y = high freq, z = speed
float3 _DeepFoamData2;  // x = threshold, y = low freq power, z = high freq power
float2 _FoamDepth;  // x = low freq foam, y = high freq foam

float3 _HeightFieldStartPos;
float _ReflectDistortion;
float3 _ShoreData; // x = water clarity, y = waveDampDepth
float4 _ShallowWaterColor;
float _HydroLinearLighting;  // 1 if linear, 0 otherwise

// Fog
float4 _HydroFogColor;
float _HydroHorizonSampleOffset;
float _HydroSampleHorizon;

// Vertex vars
float _WaveFalloff;
float _OctavesVert;
float _MinWaveAmp;

// Underwater
float4 _UnderwaterWaveColor;
float4 _UnderwaterFogTopColor;
float4 _UnderwaterOverlayColor;


// DEFINES
#define WAVE_SCALE 0.1
#define WAVE2_SCALE 0.7
#define REPEAT 16.0
#define REPEAT10 REPEAT * 10.0
#define INVREPEAT10 0.1/REPEAT

#define camHeight 100


//#define REFLECTION_DEBUG


//-------------------------------------
// getSpeed
//-------------------------------------
float getSpeed()
{
    return _InTime;
}

//-------------------------------------
// hash
//-------------------------------------
float hash( float2 p, float repeat )
{
    p = p - repeat * floor(p/repeat);  // modulous - same as mod() on glsl

    float h = dot(p,float2(727.123,659.827));
    float t = 101 * h;

    return frac(t);
}

//-------------------------------------
// tileable perlin noise
//-------------------------------------
float noise( float2 p, float repeat )
{
    float2 i = floor( p );
    float2 f = p - i;
    float2 u = f*f*(3.0-2.0*f);  // cubic-ish interpolation

    // interpolate between the 4 random numbers at each corner - perlin
    return -1.0+2.0*lerp( lerp( hash( i + float2(0.0,0.0), repeat ), 
                                hash( i + float2(1.0,0.0), repeat ), u.x),
                          lerp( hash( i + float2(0.0,1.0), repeat ), 
                                hash( i + float2(1.0,1.0), repeat ), u.x), u.y);
}

//-------------------------------------
// octave
//-------------------------------------
float octave(float2 uv, float chop)
{
    uv += noise(uv, REPEAT); 
    uv = uv - REPEAT * floor(uv * 1.0/REPEAT);

    float2 wv = 1.0-abs(sin(uv));
    float2 swv = abs(cos(uv));    

    wv = lerp(wv,swv,wv);
    return pow(1.0-pow(wv.x * wv.y,0.501),chop);
}


//-------------------------------------
// getVertexHeight
//-------------------------------------
float getVertexHeight( float3 p, float speed, int numOctaves )
{
    // set up rotation matrix to rotate uv by 30 degrees
    const float cos30 = cos(30.0 * UNITY_PI / 180.0);
    const float sin30 = sin(30.0 * UNITY_PI / 180.0);
    float2x2 rotMat = float2x2( cos30, -sin30, sin30, cos30 );

    float2x2 rotMat2 = float2x2( _WaveDirRotMat.x, _WaveDirRotMat.y, _WaveDirRotMat.z, _WaveDirRotMat.w );

    float freq = _WaveData.y;
    float amp = _WaveData.x;
    float choppy = _WaveData.w;

    float2 uv = mul( rotMat2, p.xz );
    float2 tuv = mul( rotMat, uv );

    rotMat *= _WaveSmoothness;

    float h = 0.0;

    for(int i = 0; i < numOctaves; i++)
    {
#ifdef HYDRO_OPPOSING_WAVESETS
        float d  = octave( (uv + speed) * freq, _WaveData.w );  // WaveData.w = chop
              d += octave( (tuv - speed) * freq * WAVE2_SCALE, _WaveData.w );
#else
        float d  = octave( (uv + speed) * freq, _WaveData.w );  // WaveData.w = chop
              d += octave( (tuv + speed * 0.5) * freq * WAVE2_SCALE, _WaveData.w ) * 1.5;
#endif
        h += d * amp;        

        uv = mul(rotMat,uv);   // change wave direction / speed
        tuv = uv;
        freq *= 2.0;
        amp *= 0.2;
        choppy = lerp(choppy,1.0,0.2);
    }

    return h;
}

//-------------------------------------
// getDepth
//-------------------------------------
float getDepth( float3 pnt )
{
    float2 heightTexCoord = (pnt.xz - _HeightFieldStartPos.xz) / 2048;
    float clip = step( abs(heightTexCoord.x - frac(heightTexCoord.x) ), 0.001 );
    clip = 1.0 - clip * step( abs(heightTexCoord.y - frac(heightTexCoord.y) ), 0.001 );
    clip *= 1000.0;

    float depth = tex2Dlod( _HydroHeightTex, float4(heightTexCoord, 0, 0) ) - camHeight + clip;
    depth = max( 0.0, depth );
    return depth;
}

//-------------------------------------
// vertex shader
//-------------------------------------
v2f calcVert( float4 inPos:POSITION, float2 inTex:TEXCOORD0 )
{
    v2f vData;

    float deltaX = _PatchSize;
    float deltaZ = _PatchSize;
    float3 mainPnt = _PatchStartPos;

    float3 pnt = mainPnt + float3( deltaX * inTex.x, 0.0, deltaZ * inTex.y );
    pnt.y = getVertexHeight( pnt * WAVE_SCALE, getSpeed(), _OctavesVert );

    // rotate it to view space to get proper distance from eye
    float3 viewPos = mul( UNITY_MATRIX_V, float4( pnt.xyz, 1.0 ) );
    vData.distFromEyeAndDepth.x = -viewPos.z;

    // reduce wave height in distance to match up with brim
    float distFactor = exp( -vData.distFromEyeAndDepth.x * _WaveFalloff );
    distFactor = min( 1, distFactor );  // fix bug where at high altitudes if player looks up, the waves freak out
    pnt.y *= distFactor;

#ifdef HYDRO_SHOREFX
    float depth = getDepth( pnt );
    pnt.y *= min( (depth / _ShoreData.y) + _MinWaveAmp, 1.0 );  // fade wave height if shallow depth.  _ShoreData.y = waveDampDepth
#else
    float depth = camHeight;
#endif

    vData.distFromEyeAndDepth.y = depth;

    pnt.y += _HydroWaterHeight;
    vData.worldPos = pnt;

    
    // recalculate depth with new y position - necessary for depth comparision on volume masks
    vData.distFromEyeAndDepth.x = -mul( UNITY_MATRIX_V, float4( pnt.xyz, 1.0 ) ).z;


    // project into screen space
    vData.pos = mul( UNITY_MATRIX_VP, float4( pnt, 1.0 ) );
    vData.screenPos = ComputeScreenPos( vData.pos );
    vData.screenGrabPos = ComputeGrabScreenPos( vData.pos );

    // interpRay used for more accurate depth-from-eye
    viewPos.xy /= viewPos.z;
    float3 ray = -mul( _InvCamProj, float4( viewPos.x, viewPos.y, viewPos.z, 1 ) );
    vData.interpRay = ray.xyz;


    return vData;
}



//-------------------------------------
// getHeightFromTex
//-------------------------------------
float getHeightFromTex( float3 p, float speed, int numOctaves )
{
    // set up rotation matrix to rotate uv by 30 degrees
    const float cos30 = cos(30.0 * UNITY_PI / 180.0);
    const float sin30 = sin(30.0 * UNITY_PI / 180.0);
    float2x2 rotMat = float2x2( cos30, -sin30, sin30, cos30 );

    float2x2 rotMat2 = float2x2( _WaveDirRotMat.x, _WaveDirRotMat.y, _WaveDirRotMat.z, _WaveDirRotMat.w );

    float freq = _WaveData.y;
    float amp = _WaveData.x;

    float2 uv = mul( rotMat2, p.xz );
    float2 tuv = mul( rotMat, uv );

    rotMat *= _WaveSmoothness;

    float h = 0.0;

    [unroll(4)]
    for(int i = 0; i < numOctaves; i++)
    {
#ifdef HYDRO_OPPOSING_WAVESETS
        float d  = tex2D( _OctaveTex, float2( (uv + speed*10.0) * INVREPEAT10 * freq) ).r;  // magic number here is to scale it precisely the same as the noise lookup in the vertex shader
              d += tex2D( _OctaveTex, float2( (tuv - speed*10.0) * INVREPEAT10 * freq * WAVE2_SCALE) ).r;
#else
        float d  = tex2D( _OctaveTex, float2( (uv + speed*10.0) * INVREPEAT10 * freq) ).r;  // magic number here is to scale it precisely the same as the noise lookup in the vertex shader
              d += tex2D( _OctaveTex, float2( (tuv + speed*5.0) * INVREPEAT10 * freq * WAVE2_SCALE) ).r * 1.5;
#endif

        h += d * amp;

        uv = mul(rotMat,uv);   // change wave direction and speed
        tuv = uv;
        freq *= 2.0;
        amp *= 0.2;
    }

    return h;
}

//-------------------------------------
// getNormalFromTex
//-------------------------------------
float3 getNormalFromTex( float3 pos, float speed, float delta, int numOctaves, float3 toEye, out float height )
{
    float3 mainPos = float3( pos.x, getHeightFromTex(pos,speed,numOctaves), pos.z );
    
    height = mainPos.y;  // return height
    
    float3 sliceVec = toEye * delta;
    float3 pos2 = mainPos + sliceVec;
    pos2.y = getHeightFromTex( pos2,speed,numOctaves);

    float3 tan2 = pos2 - mainPos;
    float3 cot = cross( tan2, float3(0.0,1.0,0.0) );

    return normalize( cross( cot, tan2 ) );
}

//-------------------------------------
// getColorFromNormal
//-------------------------------------
float4 getColorFromNormal( float3 normal, float3 toEye, float height, float4 refractColor, float4 reflectColor, float depth )
{
    // subsurface scatter
    float4 baseColor = _WaterColor;
    
#ifdef HYDRO_SSS
    float sssFactor = min( 1.0, pow( height / (_WaveData.x*_SSSData.y), _SSSData.x ) );  // SSSData.x = power, SSSData.y = threshold
    float camHeightOverWater =  _CamPosition.y - _HydroWaterHeight;
    sssFactor *= 1.0 - saturate( camHeightOverWater / _SSSData.z );  // _SSSData.z = SSSHeightCutoff
    
    

#ifdef HYDRO_SSS_VIEW_LIMIT
    sssFactor *= max( 0.0, dot( normalize(toEye.xz), normalize(_LightDir.xz) ) );
#endif

    baseColor = lerp( baseColor, _SSSColor, sssFactor );
#endif

#ifdef HYDRO_REFRACTION
    float4 midColor = _ShallowWaterColor;
    float interp = saturate( depth/_ShoreData.x );  // _ShoreData.x = water clarity
    interp = saturate( 1.0 - pow( 1.0-interp, 2.0 ) );  // exponential falloff on water clarity

    refractColor = refractColor * lerp( 1.0, midColor, interp );
    baseColor = lerp( refractColor, baseColor, interp );  
#endif

    float fresnel = 1.0 - max(0.0, dot(normal,toEye));
    fresnel = pow(fresnel,_FresnelData.x) * _FresnelData.y;


    return lerp( baseColor, reflectColor, fresnel );
}

//-------------------------------------
// getFoamColor
//-------------------------------------
float4 getFoamColor( float3 pos, float height )
{
    height = max( 0.0, height );  // infuriating that height can be negative coming in given that it's always positive in vertex shader.  Driver bug?
    float intensity = max( 0.0, (_FoamDepth.x - height) / _FoamDepth.x );  // fade in low freq foam
    float intensity2 = max( 0.0, (_FoamDepth.y - height) / _FoamDepth.y );  // fade in high freq foam
    float intensity3 = 1.0 - pow( max( 0.0, (1.0 - height) / 1.0), 8.0 );  // fade out all foam in very shallow water


    float freq = _FoamData.x;
    float2 uv = pos.xz;


    float speed = getSpeed() * _FoamData.z;


    // mix low freq foam
    float h  = tex2D( _FoamTex, float2( (uv + speed) * freq) ).r;
          h += tex2D( _FoamTex, float2( (uv - speed) * freq * WAVE2_SCALE) ).g;
    h *= 0.5;

    // add high freq foam
    h += tex2D( _FoamTex, float2( (uv+speed*0.5) * _FoamData.y ) ).r * intensity2 * 0.5;

//if( height > 10  ) return float4( 1, 0, 0, 1 );


    return h * intensity * intensity3 * _FoamColor;
}

//-------------------------------------
// getDeepFoamColor
//-------------------------------------
float4 getDeepFoamColor( float3 pos, float height )
{
    // could take data in from vertex shader?
    
    float heightFactor = min( 1.0, pow( height / _DeepFoamData2.x, _DeepFoamData2.y ) );
    float heightFactor2 = min( 1.0, pow( height / _DeepFoamData2.x, _DeepFoamData2.z ) );


    float freq = 0.03;
    
    float2 uv = pos.xz;


    float speed = getSpeed() * _DeepFoamData.z;


    // mix low freq foam - move it opposite speed as main waves
    float h  = tex2D( _FoamTex, float2( (uv - speed) * _DeepFoamData.x) ).r;
          h += tex2D( _FoamTex, float2( (uv - speed*2) * _DeepFoamData.x * WAVE2_SCALE) ).g;

    h *= heightFactor;

    // add high freq foam
    h += tex2D( _FoamTex, float2( (uv-speed*1.2) * _DeepFoamData.y ) ).r * heightFactor2 * 2.0;


    return h * _FoamColor;
}


static const float reflectDists[10] =
{
    float(1.0f),
    float(2.0f),
    float(4.0f),
    float(6.0f),
    float(8.0f),

    float(16.0f),
    float(32.0f),
    float(64.0f),
    float(128.0f),
    float(256.0f)
};


//-------------------------------------
// getScreenPos
//-------------------------------------
float4 getScreenPos( float3 pos, float alignBuffs )
{
    float4 screenPos = mul( UNITY_MATRIX_VP, float4( pos, 1.0 ) );
    return ComputeGrabScreenPos( screenPos * float4( 1, alignBuffs, 1, 1 ) );
}

//-------------------------------------
// reflectCubemap
//-------------------------------------
float4 reflectCubemap( float3 reflectDir )
{
//    float3 reflectEye = float3( reflectDir.x, reflectDir.y, reflectDir.z );  // swizzle for cubemap so it matches the skybox

    float4 reflectColor = texCUBE( _Cube, reflectDir );
    reflectColor = lerp( reflectColor, pow(reflectColor,2.2), _HydroLinearLighting );
return reflectColor;
//    return texCUBE( _Cube, reflectDir );
}


//-------------------------------------
// calcReflect - screenspace reflection
//-------------------------------------
float4 calcReflect( float3 startPos, float3 reflectDir )
{
    // align up direction of grabPass and the depth buffer so they can be indexed the same
    //------------
    float test1 = _ProjectionParams.x;

#if UNITY_UV_STARTS_AT_TOP
    float test2 = 1;
#else
    float test2 = -1;
#endif
    
    float alignBuffs = -( abs(test1 + test2) - 1 );
    //------------


    float4 startScreenPos = getScreenPos( startPos, alignBuffs );
    float3 endPos = startPos + reflectDir * 30.0f;
    float4 endScreenPos = getScreenPos( endPos, alignBuffs );
    
    float4 itDir = normalize(endScreenPos - startScreenPos);

    float4 lastClearPos = startScreenPos;

    [unroll(10)]

    for( int i=0; i<10; ++i )
    {
        float4 itPos = startScreenPos + itDir * reflectDists[i];

        float2 screenPos = itPos.xy / itPos.w;

        // clip 
        if( screenPos.y > 1.0 || screenPos.x > 1.0 || screenPos.x < 0.0 )
        {
            return reflectCubemap( reflectDir );
        }



        float screenDepth = tex2D(_CameraDepthTexture, screenPos );
        screenDepth = LinearEyeDepth(screenDepth);

        if( itPos.w > screenDepth )
        {
            float stepDist;

            if( i==0 )
            {
                stepDist = reflectDists[0] * 0.5;
            }
            else
            {
                stepDist = (reflectDists[i] - reflectDists[i-1]) * 0.5;
            }
            
            for( int j=0; j<5; ++j )
            {
                itPos = lastClearPos + itDir * stepDist;

                // these 2 lines result in less 'hits' and more accurately sample the reflection, but are less 'full' looking and slower due to the extra sampling
//                screenDepth = tex2D(_CameraDepthTexture, itPos.xy/itPos.w );
//                screenDepth = LinearEyeDepth(screenDepth);

                if( itPos.w < screenDepth )
                {
                    lastClearPos = itPos;
                }

                stepDist *= 0.5;
                
            }


            // tried moving this check inside binary search loop, and result looked better but performance was very choppy
            if( abs(itPos.w - screenDepth) < 1 )
            {
                float2 uv = itPos.xy / itPos.w;
                if( alignBuffs < 0 ) uv.y = 1.0 - uv.y;
                return tex2D( _GrabPassTex, uv );
            }
            else
            {
                return reflectCubemap( reflectDir );
            }
            
            
        }
        
        lastClearPos = itPos;
    }


    return reflectCubemap( reflectDir );
}

//-------------------------------------
// getRefractColor
//-------------------------------------
float4 getRefractColor( float3 toEye, float3 normal, v2f inData, out float depth )
{
    float distortion = 0.9;  // 1.0 = no distortion
    float4 refractDir = float4( refract( -toEye, normal, distortion ), 0 );
    refractDir.xyz -= -toEye;


    // fade out distortion at edge of screen
    float4 screenPos = UNITY_PROJ_COORD( inData.screenPos ) / inData.screenPos.w;
    screenPos.xy = screenPos.xy * 2 - 1;
    refractDir.xy = refractDir.xz;  // swizzle to get distortion on y screen axis
    refractDir.xy *= float2( 1, 1 ) - abs(screenPos.xy);


    depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD( inData.screenPos + refractDir ) );
    depth = LinearEyeDepth(depth);


    if( depth < inData.screenPos.w )  // if( depth is closer to cam than water surface, something is in front of water and the depth buffer needs to be re-sampled with no refraction distortion
    {
        depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD( inData.screenPos ) );
        depth = LinearEyeDepth(depth);
        refractDir = float4( 0,0,0,0 );
    }

    depth -= inData.screenPos.w;
//    depth *= length(inData.interpRay);  // change distance based on angle from pixel to camera - commented out because it causes artifacts when camera very close to water


    return tex2Dproj( _GrabPassTex, UNITY_PROJ_COORD( inData.screenGrabPos + refractDir ) );
}


//-------------------------------------
// calcUnderwaterPixel
//-------------------------------------
fixed4 calcUnderwaterPixel( float3 toEye, float3 normal, float dist, float depth, float4 refractColor, float height )
{
    // the grab pass hasn't grabbed the sky at this point because it draws last.  sky pixels are undefined, so let's fill them with the reflection cubemap
    if( depth >= 200 )
    {
        refractColor = reflectCubemap( -toEye );
    }


    float4 baseColor = _WaterColor;

    float fresnel = 1.0 - max(0.0, dot(-normal,toEye));
    float power = 2;
    float blend = 2;
    fresnel = min( 1.0, pow( fresnel, power ) * blend );


    height /= _WaveData.x * 3;
    height += 0.7;
    float4 reflectColor = _UnderwaterWaveColor * height + refractColor * 0.1;


    float4 color =  lerp( refractColor, reflectColor, fresnel );


    // fog out the waves
    float fogFactor = (dist*dist*250) / (_ProjectionParams.z *_ProjectionParams.z);
    fogFactor = saturate( fogFactor );
    color =  lerp( color, _UnderwaterFogTopColor, fogFactor );
    
    
    // match the underwater filter so the fog is seamless between the waves and the image effect
    color *= _UnderwaterOverlayColor;
    
    
    color.a = 0.05;  // hack, identifies this pixel as water
    return color;
}

//-------------------------------------
// calcPixel
//-------------------------------------
fixed4 calcPixel( v2f inData ) : SV_Target
{
    float speed = getSpeed();

    float3 toEye = _CamPosition - inData.worldPos;  // this is approximate, maybe need more accurate?
    float dist = length( toEye );  // need to calculate this here or the brim will not match up with the patch - the brim vertices are too far apart for it to match
    toEye = normalize( toEye );


    float volumeSample = tex2Dproj( _HydroVolumeMaskTex, UNITY_PROJ_COORD(inData.screenPos) ).r;

    if( volumeSample > 0.0 && volumeSample < inData.distFromEyeAndDepth.x  || volumeSample >= 65535 )
    {
        discard;
    }


    float height = 0.0;

#ifdef HYDRO_UNDERWATER
    float3 dFdxPos = ddx( inData.worldPos );
    float3 dFdyPos = ddy( inData.worldPos );
    float3 facenormal = normalize( cross(dFdyPos,dFdxPos ));

    if( facenormal.y < -0.0 )
    {
        float3 normal = getNormalFromTex( inData.worldPos, speed, 0.05, 3, toEye, height );

        float depth;
        float4 refractColor = getRefractColor( toEye, normal, inData, depth );

        return calcUnderwaterPixel( toEye, normal, dist, depth, refractColor, height );
    }
#endif

    float3 normal = getNormalFromTex( inData.worldPos, speed, 0.1, _OctavesPix, toEye, height );  // magic number here can affect how noisy waves appear in distance

//return fixed4( height, height, height, height ) / 2;

    

// REFLECTION
#ifdef REFLECTION_DEBUG
    float3 reflectNormal = lerp( normal, float3( 0, 1, 0 ), 1 );
    float4 reflectColor = calcReflect( inData.worldPos, reflect( -toEye, reflectNormal ) );
    return reflectColor;
#else

    float3 reflectNormal = lerp( float3( 0, 1, 0 ), normal, _ReflectDistortion );


    #ifdef HYDRO_PLANAR_REFELCT
        float4 screenPos = inData.screenPos;
        screenPos.xy += reflectNormal.xz * 10;
        float4 reflectColor = tex2Dproj( _HydroReflectTex, UNITY_PROJ_COORD(screenPos) );
    #else
        #ifdef HYDRO_SSR
            float4 reflectColor = calcReflect( inData.worldPos, reflect( -toEye, reflectNormal ) );
        #else
            float4 reflectColor = reflectCubemap( reflect( -toEye, reflectNormal ) );
        #endif

    #endif

#endif



// REFRACTION
#ifdef HYDRO_REFRACTION

    float depth;
    float4 refractColor = getRefractColor( toEye, normal, inData, depth );

    float4 waterColor = getColorFromNormal( normal, toEye, height, refractColor, reflectColor, depth );
#else
    float4 waterColor = getColorFromNormal( normal, toEye, height, float4(0.0,0.0,0.0,0.0), reflectColor, 0.0 );
#endif  



// FOAM
    float4 foamColor = float4( 0.0, 0.0, 0.0, 0.0 );

#ifdef HYDRO_SHOREFX
    waterColor += getFoamColor( inData.worldPos, inData.distFromEyeAndDepth.y ) * lerp(1,3,_HydroLinearLighting);
#endif

#ifdef HYDRO_DEEP_FOAM
    waterColor += getDeepFoamColor( inData.worldPos, height );
#endif


// FOG

// copied this from UNITY_CALC_FOG_FACTOR_RAW in UnityCG.cginc:
#if defined(FOG_LINEAR)
    // factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
    #define HYDRO_CALC_FOG_FACTOR_RAW(coord) float unityFogFactor = (coord) * unity_FogParams.z + unity_FogParams.w
#elif defined(FOG_EXP)
    // factor = exp(-density*z)
    #define HYDRO_CALC_FOG_FACTOR_RAW(coord) float unityFogFactor = unity_FogParams.y * (coord); unityFogFactor = exp2(-unityFogFactor)
#elif defined(FOG_EXP2)
    // factor = exp(-(density*z)^2)
    #define HYDRO_CALC_FOG_FACTOR_RAW(coord) float unityFogFactor = unity_FogParams.x * (coord); unityFogFactor = exp2(-unityFogFactor*unityFogFactor)
#else
    #define HYDRO_CALC_FOG_FACTOR_RAW(coord) float unityFogFactor = 1.0
#endif

    HYDRO_CALC_FOG_FACTOR_RAW(dist);

    // grab fog color from skybox at horizon
    float3 horizonVec = -toEye;
    horizonVec.y = _HydroHorizonSampleOffset;
    horizonVec = normalize( horizonVec );
    
    float4 horizonColor = texCUBE( _Cube, horizonVec );
    horizonColor = lerp( horizonColor, pow(horizonColor,2.2), _HydroLinearLighting );

    float4 fogColor = lerp( horizonColor, _HydroFogColor, smoothstep( 0.0, 0.5, toEye.y ) + _HydroSampleHorizon );  // have to do this to plug a fog "hole" when looking straight down
    waterColor = lerp( fogColor, waterColor, saturate(unityFogFactor) );





    // specular
#ifdef HYDRO_SPECULAR
    float3 modNorm = normal;
    modNorm.xz = -modNorm.xz;

    float3 lightReflect = reflect( _LightDir, modNorm );
    lightReflect.y = pow( lightReflect.y, 4 );  // push specular to wave edges

    float specular = max( 0.0, dot( toEye, lightReflect ) );
    float specPower = _SpecularPower + dist * 0.04f;  // pinch off specular at horizon so it doesn't get really wide
    specular = pow( specular, specPower );

    float lightAngle = dot( float3(0,-1,0), _LightDir );
    specular *= step( lightAngle, 0.7071 );  // cut off specular if the light angle is above 45 degrees
    waterColor += specular * _SpecularColor * lerp( 0.0, 2, smoothstep(-0.2, 0.7071, lightAngle) ); // this is set up to smoothly fade out specular when sun is a little below the horizon

#endif

    waterColor.a = 0.05;  // indicates to the underwaterfilter that this is a water pixel

    return saturate( waterColor );  // saturate fixes issues with Linear/HDR turned on
}