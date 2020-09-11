//-----------------------------------------------------------------------------
// SkyCapture
// Copyright (C) Xix Interactive, LLC
//-----------------------------------------------------------------------------
using UnityEngine;
using System.Collections;

namespace Hydroform
{

[ExecuteInEditMode]

public class SkyCapture : MonoBehaviour 
{

    public float UpdateFreq = 60;

    private Cubemap mCubemap = null;
    private Camera mCam = null;  // capture cam
    private HydroformComponent OceanPrefab;
    private float mLastUpdateTime = -1e+25f;
    
    const string CAM_NAME = "SkyCapCam";

    //-----------------------------------------------------------------------------
    // Awake
    //-----------------------------------------------------------------------------
	void Awake()
	{
        // find hydroform, get material
        OceanPrefab = (HydroformComponent) FindObjectOfType( typeof(HydroformComponent) );

        if( OceanPrefab == null )
        {
            Debug.LogError( "Ocean prefab not found for SkyCapture: " + gameObject.name, gameObject );
            return;
        }

/*
        Material meshMat = Resources.Load("HydroformMat", typeof(Material)) as Material;
        Material brimMat = Resources.Load("BrimMat", typeof(Material)) as Material;
        meshMat.SetTexture( "_Cube", (Texture) reflectFX.skybox );
        brimMat.SetTexture( "_Cube", reflectFX.skybox );
*/

        // create camera if doesn't already exist


        foreach( Transform child in transform )
        {
            if( child.name == CAM_NAME )
            {
                mCam = child.GetComponent<Camera>();
                continue;
            }
        }

        if( mCam == null )
        {
            GameObject camObj = new GameObject( CAM_NAME, typeof(Camera) );
            mCam = camObj.GetComponent<Camera>();
            mCam.gameObject.SetActive( false );
            mCam.cullingMask = 0;
            mCam.clearFlags = CameraClearFlags.Skybox;
            mCam.transform.SetParent( transform, false );
        }

        if( !mCubemap )
        {
            mCubemap = new Cubemap( 256, TextureFormat.RGB24, false );
        }

//        mCam.RenderToCubemap( mCubemap );
//        AssetDatabase.CreateAsset( mCubemap, "Assets/NewCube.cubemap" );
//        mCam.targetTexture = null;


/*
        if( Camera.main )
        {
            Vector3 pos = Camera.main.transform.position;
            
            Camera.main.transform.position = transform.position;

            Cubemap cube = new Cubemap( 256, TextureFormat.RGB24, false );
            Camera.main.RenderToCubemap( cube );
            
            AssetDatabase.CreateAsset( cube, "Assets/NewCube.cubemap" );

            Camera.main.transform.position = pos;
        }
*/

	}

    //-----------------------------------------------------------------------------
    // OnDestroy
    //-----------------------------------------------------------------------------
	void OnDestroy()
    {
        DestroyImmediate( mCam );
        DestroyImmediate( mCubemap );
    }

	
    //-----------------------------------------------------------------------------
    // Capture
    //-----------------------------------------------------------------------------
    void Capture()
    {
        if( !mCam || !OceanPrefab || !mCubemap ) return;

        // check ocean to make sure skycapture turned on
        if( !OceanPrefab.reflectFX.skyboxCapture ) return;



        mCam.RenderToCubemap( mCubemap );

        OceanPrefab.mHydroMat.SetTexture( "_Cube", (Texture) mCubemap );
        OceanPrefab.mBrimMat.SetTexture( "_Cube", (Texture) mCubemap );

        mCam.targetTexture = null;


    }

    //-----------------------------------------------------------------------------
    // LateUpdate 
    //-----------------------------------------------------------------------------
	void LateUpdate ()
	{
        // capture at specified frequency
        float curTime = Time.timeSinceLevelLoad;
        float diff = curTime - mLastUpdateTime;
        if( diff > UpdateFreq )
        {
            Capture();
            mLastUpdateTime = curTime;
        }

	}
	
}

}  // namespace Hydroform
