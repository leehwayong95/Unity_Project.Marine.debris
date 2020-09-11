//-----------------------------------------------------------------------------
// DayNight
// Copyright (C) Xix Interactive, LLC
//-----------------------------------------------------------------------------
using UnityEngine;
using System.Collections;

namespace Hydroform
{

 
public class DayNight : MonoBehaviour 
{
    public float DayCycleLifetime = 60;

    public Light SunDirLight;
//    float StartIntensity;
    
    private HydroformComponent OceanPrefab;


    //-----------------------------------------------------------------------------
    // Awake
    //-----------------------------------------------------------------------------
    void Awake()
    {
        // find hydroform
        OceanPrefab = (HydroformComponent) FindObjectOfType( typeof(HydroformComponent) );

        if( OceanPrefab == null )
        {
            Debug.LogError( "Ocean prefab not found for SkyCapture: " + gameObject.name, gameObject );
            return;
        }

//        StartIntensity = SunDirLight.intensity;
    }
    
    //-----------------------------------------------------------------------------
    // Update
    //-----------------------------------------------------------------------------
    void Update()
    {
        float curTime = Time.timeSinceLevelLoad % DayCycleLifetime / DayCycleLifetime;

        SunDirLight.transform.rotation = Quaternion.Euler( 0, 45, 45 ) * Quaternion.Euler( curTime * 360 - 30, 0, 0 );
 
        // Update specular light more quickly than PeriodUpdate()
        // Not necessary unless sun moving very quickly like the DayNight demo
        Shader.SetGlobalVector( "_LightDir", SunDirLight.transform.forward );

/*
        Color SSSColor = OceanPrefab.subsurfaceFX.subsurfaceColor;

        if( OceanPrefab.surfaceFX.sunLight )
        {
            float lightFactor = Vector3.Dot( Vector3.up, -OceanPrefab.surfaceFX.sunLight.transform.forward );
            lightFactor = Mathf.Max( lightFactor, 0 );
            Color waterColor = Color.Lerp( OceanPrefab.surfaceFX.nightWaterColor, OceanPrefab.surfaceFX.waterColor, lightFactor );


            if( lightFactor <= 0.1 )
            {
//                float test = Mathf.SmoothStep( 0.0f, 1.0f, lightFactor / 0.1f );
                SSSColor =  Color.Lerp( waterColor, SSSColor, lightFactor / 0.1f );
            }

        }
        Shader.SetGlobalColor( "_SSSColor", OceanPrefab.GetCorrectColor(SSSColor) );
*/



/*
        float intensity = StartIntensity;        

        if( curTime <= 0.23f || curTime >= 0.75f )
        {
            intensity = 0;
        }
        else if( curTime <= 0.25f )
        {
            intensity = Mathf.Clamp01((curTime - 0.23f) * (1 / 0.02f));
        }
        else if( curTime >= 0.73f )
        {
            intensity = Mathf.Clamp01(1 - ((curTime - 0.73f) * (1 / 0.02f)));
        }
 
        SunDirLight.intensity = StartIntensity * intensity;
*/ 

        //CurTime += (Time.deltaTime / DayCycleLifetime);
        //if( CurTime >= 1 ) CurTime = CurTime - 1;

    }
}

}  // namespace Hydroform
