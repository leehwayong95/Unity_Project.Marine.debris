//-----------------------------------------------------------------------------
// WaveQuery
// Copyright (C) Xix Interactive, LLC  (Brian Ramage)
//-----------------------------------------------------------------------------
using UnityEngine;
//using System.Collections;

public class WaveQuery
{

    //-----------------------------------------------------------------------------
    // LerpUnclamped
    //-----------------------------------------------------------------------------
    public static float LerpUnclamped (float from, float to, float value)
    {
        return from + value*(to-from);
    }
    
    //-----------------------------------------------------------------------------
    // Hash
    //-----------------------------------------------------------------------------
    float Hash( Vector2 p, float repeat )
    {
        p.x = p.x - repeat * Mathf.Floor(p.x/repeat);  // modulous - same as mod() on glsl
        p.y = p.y - repeat * Mathf.Floor(p.y/repeat);  // modulous - same as mod() on glsl

        float h = Vector2.Dot( p, new Vector2(727.123f,659.827f) );

//        float t = 166 * h + 10139;
//        float t = 499 * h;
        float t = 101 * h;
        return t - Mathf.Floor( t );
    }

    //-----------------------------------------------------------------------------
    // Noise
    //-----------------------------------------------------------------------------
    float Noise( Vector2 p, float repeat )
    {
        Vector2 i = new Vector2( Mathf.Floor( p.x ), Mathf.Floor( p.y ) );
        Vector2 f = p - i;
        Vector2 u = Vector2.Scale( Vector2.Scale(f,f), ( new Vector2(3.0f,3.0f) - 2.0f * f ) );  // cubic-ish interpolation

        // interpolate between the 4 random numbers at each corner - perlin
        float hash0 = Hash( i + new Vector2(0.0f,0.0f), repeat );
        float hash1 = Hash( i + new Vector2(1.0f,0.0f), repeat );
        float hash2 = Hash( i + new Vector2(0.0f,1.0f), repeat );
        float hash3 = Hash( i + new Vector2(1.0f,1.0f), repeat );

        float lerp0 = LerpUnclamped( hash0, hash1, u.x );
        float lerp1 = LerpUnclamped( hash2, hash3, u.x );

        return -1.0f+2.0f * LerpUnclamped( lerp0, lerp1, u.y );
    }

    //-----------------------------------------------------------------------------
    // matMul2x2
    //-----------------------------------------------------------------------------
    Vector2 MatMul2x2( Vector4 mat2x2, Vector2 vec )
    {
        Vector2 ret;
        
        ret.x = vec.x * mat2x2.x + vec.y * mat2x2.y;
        ret.y = vec.x * mat2x2.z + vec.y * mat2x2.w;
        return ret;
    }

    //-------------------------------------
    // octave
    //-------------------------------------
    float Octave(Vector2 uv, float chop)
    {
        const float REPEAT = 16.0f;
        
        float noiseVal = Noise(uv, REPEAT);

       
        uv.x += noiseVal;
        uv.y += noiseVal;
    
        Vector2 uvFloor = new Vector2( Mathf.Floor(uv.x * 1.0f/REPEAT), Mathf.Floor(uv.y * 1.0f/REPEAT) );
        uv = uv - REPEAT * uvFloor;
    //    uv -= float2( 0.125, 0.125 );  // reduces seam artifacts, unknown why, commented out because virtually invisible anyway

        Vector2 wv;
        wv.x = 1.0f - Mathf.Abs( Mathf.Sin(uv.x) );
        wv.y = 1.0f - Mathf.Abs( Mathf.Sin(uv.y) );
        
        Vector2 swv;
        swv.x = Mathf.Abs( Mathf.Cos(uv.x) );
        swv.y = Mathf.Abs( Mathf.Cos(uv.y) );

        wv.x = Mathf.Lerp( wv.x, swv.x, wv.x );
        wv.y = Mathf.Lerp( wv.y, swv.y, wv.y );
        return Mathf.Pow( 1.0f - Mathf.Pow( wv.x * wv.y, 0.501f ), chop );
    }

    //-----------------------------------------------------------------------------
    // GetHeightAtPoint 
    //-----------------------------------------------------------------------------
    public float GetHeightAtPoint( Vector3 pnt, float freq, float amp, float chop, float speed,
                                   float smooth, int numOctaves, bool opposingWaves, Vector4 waveDirRotMat )
    {
        const float WAVE_SCALE  = 0.1f;
        const float WAVE2_SCALE = 0.7f;

        pnt *= WAVE_SCALE;

        // set up rotation matrix to rotate uv by 30 degrees
        float cos30 = Mathf.Cos(30.0f * Mathf.PI / 180.0f);
        float sin30 = Mathf.Sin(30.0f * Mathf.PI / 180.0f);
        Vector4 rotMat = new Vector4( cos30, -sin30, sin30, cos30 );


        Vector2 uv = new Vector2( pnt.x, pnt.z );

        uv = MatMul2x2( waveDirRotMat, uv );

        Vector2 tuv = MatMul2x2( rotMat, uv );
        
        rotMat *= smooth;

        Vector2 speedVec = new Vector2( speed, speed );
        float h = 0.0f;
      
        for( int i=0; i<numOctaves; ++i )
        {
            float d;
            if( opposingWaves )
            {
                d  = Octave( (uv + speedVec) * freq * 1, chop );  
                d += Octave( (tuv - speedVec) * freq * WAVE2_SCALE, chop );
            }
            else
            {
                d  = Octave( (uv + speedVec) * freq * 1, chop );  
                d += Octave( (tuv + speedVec * 0.5f) * freq * WAVE2_SCALE, chop ) * 1.5f;
            }

            h += d * amp;        
            
            uv = MatMul2x2( rotMat, uv );
            tuv = uv;
            freq *= 2.0f;
            amp *= 0.2f;
            chop = LerpUnclamped( chop, 1.0f, 0.2f );
        }

        return h;
    }


}
