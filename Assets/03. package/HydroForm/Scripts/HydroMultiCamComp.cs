//-----------------------------------------------------------------------------
// HydroMultiCamComp
// Copyright (C) Xix Interactive, LLC  (Brian Ramage)
//-----------------------------------------------------------------------------
using UnityEngine;
using System.Collections;


namespace Hydroform
{

[AddComponentMenu("Hydroform/HydroMultiCamComp")]

public class HydroMultiCamComp : MonoBehaviour
{
    private HydroformComponent OceanPrefab;


    //-----------------------------------------------------------------------------
    // Awake
    //-----------------------------------------------------------------------------
    void Awake()
    {
        OceanPrefab = (HydroformComponent) FindObjectOfType( typeof(HydroformComponent) );

        if( OceanPrefab == null )
        {
            Debug.LogError( "Ocean prefab not found for HydroMultiCamComp on camera: " + gameObject.name, gameObject );
            return;
        }
    }

    //-----------------------------------------------------------------------------
    // OnEnable
    //-----------------------------------------------------------------------------
	void OnEnable()
    {
	}

    //-----------------------------------------------------------------------------
    // OnPreCull
    //-----------------------------------------------------------------------------
    void OnPreCull()
    {
        if( OceanPrefab == null ) return;

        if( Application.isPlaying )
        {
            OceanPrefab.UpdateCamData();
            OceanPrefab.UpdateReflection();

            if( gameObject.GetComponent<UnderwaterFilter>() != null )
            {
                OceanPrefab.UpdateUnderwaterCam();
            }

            OceanPrefab.RenderVolumeMasks();
            OceanPrefab.DrawMeshes( gameObject.GetComponent<Camera>() );
        }
    }

}

}  // namespace Hydroform