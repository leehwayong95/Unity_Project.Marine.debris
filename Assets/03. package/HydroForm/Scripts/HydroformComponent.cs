//-----------------------------------------------------------------------------
// Hydroform Water
// Copyright (C) Xix Interactive, LLC  (Brian Ramage)
//-----------------------------------------------------------------------------


using UnityEngine;
using System;
//using System.Collections;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.VR;

#if UNITY_2020_1_OR_NEWER
using UnityEngine.XR;
#endif

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

namespace Hydroform
{

[ExecuteInEditMode]


//[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class HydroformComponent : MonoBehaviour 
{
    private int PatchSize = 700;
    private float VertDensity = 0.5f;
    private int TexSize = 1024;   
    private int HeightFieldSize = 2048;
    private Camera mHeightCam = null;
    private Camera mReflectCam = null;
    private Camera mMaskCam = null;
    private RenderTexture mHeightTex;
    private RenderTexture mReflectTex;
    private RenderTexture mUnderwaterMaskTex;
    private RenderTexture mVolumeMaskTex = null;
    private Cubemap mInternalSkybox = null;

    int mPeriodUpdateInterval = 60;
    private int mPeriodUpdateCount = 9999999;

    const int MAX_VERTS = 65536;
    float CAGE_SIZE = 6;

    RenderTexture mOctaveTex = null;
    private float MaxDist = 600.0f;
    float mFalloffFactor = 0.005f;
    WaveQuery mWaveQuery = new WaveQuery();
    bool mInWaterRender = false;
    bool mInVolumeMaskRender = false;
    Vector3 [] mFrustVerts = new Vector3[4];  // done here to prevent leak?
    Camera mCurCam = null;

    private Mesh mBrimMesh = null;
    private Mesh mCageMesh = null;

    [HideInInspector]
    public Material mBrimMat = null;
    [HideInInspector]
    public Material mHydroMat = null;

    private Material mCageMat = null;

    private List<Mesh> mMeshList = new List<Mesh>(30);

    bool mInternalSSR = false;
    bool mInternalPlanarReflect = false;


    [Serializable]
    public struct WaveConstruct
    {
        [Tooltip( "Size of wave-shape texture" )]
        public int texSize;

        [Tooltip( "Number of verts per unity spatial unit. Higher densities look smoother, but can impact performance." )]
        public float vertDensity;

        [Tooltip( "Size of the patch of wave geometry. Larger numbers can impact performance, but allow waves to be seen further out from the camera." )]
        public int patchSize;

        [InspectorButton("ResetData")]
        public bool apply;
    }


    [Serializable]
    public struct WaveSettings
    {
        [Tooltip( "Height of the ocean - essentially sea level." )]
        public float waterHeight;

        [Tooltip( "Amplitude of the waves" )]
        public float amplitude;

        [Tooltip( "Frequency of the waves" )]
        public float frequency;

        [Tooltip( "Speed of the waves" )]
        public float speed;

        [Tooltip( "How pointy the waves are" )]
        public float chop;

        [Tooltip( "Smoothness of the ocean surface" )]
        [Range(1.0f, 2.0f)]
        public float smooth;

        [Tooltip( "Detail of the waves on the vertices (geometry)" )]
        [Range(1, 2)]
        public int vertComplexity;

        [Tooltip( "Detail of the wave surface in the pixel shader" )]
        [Range(1, 4)]
        public int pixComplexity;

        [Tooltip( "Direction of wave movement" )]
        [Range(0,359)]
        public float direction;

        [Tooltip( "Opposing waves give appearance of calm, directionless water" )]
        public bool opposingWaves;
    }

    [Serializable]
    public struct SurfaceFX
    {
        public Color waterColor;
        public Color nightWaterColor;

        public bool enableSpecular;

        [Tooltip( "Light object used to control specular direction, day/night water color, etc" )]
        public Light sunLight;

        [Range(0.0f, 1.0f)]
        public float specularBrightness;

        [Tooltip( "How concentrated the specular highlights are" )]
        [Range(2.0f, 150.0f)]
        public float specularPower;

        public Color specularColor;

        [Tooltip( "How concentrated the reflections are" )]
        [Range(1.0f, 10.0f)]
        public float reflectPower;

        [Tooltip( "How bright the reflections are" )]
        [Range(0.0f, 2.0f)]
        public float reflectBlend;
    }

    [Serializable]
    public struct SSSFX
    {
        [Tooltip( "Enable subsurface scatter" )]
        public bool enableSSS;

        [Tooltip( "Subsurface scatter color" )]
        public Color subsurfaceColor;

        [Tooltip( "Limit SSS to occer when wave is between viewer and light" )]
        public bool enableSSSViewLimit;

        [Tooltip( "SSS falloff curve" )]
        public float SSSPower;

        [Tooltip( "Amplitude at which SSS cuts off" )]
        public float SSSThreshold;

        [Tooltip( "Camera height above water at which point SSS is no longer visible" )]
        public float SSSHeightCutoff;
    }

    [Serializable]
    public struct ShoreFoam
    {
        [Tooltip( "Scale for low frequency foam layer" )]
        public float lowFreq;

        [Tooltip( "Scale for high frequency foam layer" )]
        public float highFreq;

        [Tooltip( "Speed of foam animation" )]
        public float speed;

        [Tooltip( "Max depth for low frequency foam layer to appear" )]
        public float depthLowFreq;

        [Tooltip( "Max depth for high frequency foam layer to appear" )]
        public float depthHighFreq;
    }

    [Serializable]
    public struct DeepFoam
    {
        [Tooltip( "Enable deep foam for stormy ocean" )]
        public bool enableDeepFoam;

        [Tooltip( "Scale for low frequency foam layer" )]
        public float lowFreq;

        [Tooltip( "Scale for high frequency foam layer" )]
        public float highFreq;

        [Tooltip( "Speed of foam animation" )]
        public float speed;

        [Tooltip( "Amplitude at which foam begins" )]
        public float threshold;

        [Tooltip( "Power falloff curve for low frequency foam layer" )]
        public float lowFreqPower;

        [Tooltip( "Power falloff curve for high frequency foam layer" )]
        public float highFreqPower;
    }

    [Serializable]
    public struct ShoreFX
    {
        public bool enableShoreFX;
        public bool enableRefraction;

        public Color foamColor;

        public Color shallowColor;

        [Tooltip( "ShallowColorMultiplier - allows way to brighten the shallow color" )]
        public float shallowColorMultiplier;
        
        [Tooltip( "Minimum wave amplidute - how high waves are coming up on shore. 0 will taper them to completely flat." )]
        public float minWaveAmp;

        public ShoreFoam shoreFoam;
        public DeepFoam deepFoam;

        [Tooltip( "Clarity - higher values are clearer, allowing high view distance" )]
        public float waterClarity;

        [Tooltip( "Starting depth for when wave height begins to dampen going towards shore" )]
        public float waveDampDepth;
    }

    [Serializable]
    public struct ReflectFX
    {
        [Tooltip( "Static cubemap for sky reflections" )]
        public Cubemap skybox;

        [Tooltip( "Periodically capture skybox - useful for dynamic skies  **OVERRIDES skybox cubemap parameter above**" )]
        public bool skyboxCapture;

        [Tooltip( "Enable screenspace reflections" )]
        public bool enableSSR;

        [Tooltip( "Enable planar reflections (slower, better looking than SSR)" )]
        public bool enablePlanarReflect;

        [Tooltip( "Size of planar reflection texture" )]
        public int reflectTexSize;

        [Tooltip( "Distortion applied to reflections" )]
        [Range(0.0f, 1.0f)]
        public float reflectionDistortion;
    }

    [Serializable]
    public struct Underwater
    {
        [Tooltip( "Turn underwater rendering on/off - underwater rendering only visible in game mode" )]
        public bool enableUnderwater;

        [Tooltip( "Top color for underwater fog gradient" )]
        public Color fogTop;

        [Tooltip( "Bottom color for underwater fog gradient" )]
        public Color fogBottom;

        [Tooltip( "Color that is overlaid on the underwater pixels" )]
        public Color overlayColor;

        [Tooltip( "Color for the water 'thickness' when the water intersects the camera near plane" )]
        public Color lipColor;

        [Tooltip( "Color on the waves for light reflecting downwards" )]
        public Color waveColor;

        [Tooltip( "Distance fog density" )]
        public float fogDensity;

        [Tooltip( "Height fog density" )]
        public float fogHeightDensity;
    }

    [Serializable]
    public struct FogSettings
    {
        [Tooltip( "Sample the skybox at the horizon for fog color - allows smooth transition from ocean to sky" )]
        public bool sampleSkyboxAtHorizon;

        [Tooltip( "Offset where to grab horizon sample from.  Positive values are above horizon, negative values below" )]
        public float horizonSampleOffset;

        [Tooltip( "Turn the fog override settings on/off" )]
        public bool overrideUnityFogSettings;

        [Tooltip( "Fog color - overrides unity fog color if override box is checked above" )]
        public Color overrideFogColor;
    }


    public WaveConstruct waveConstruction = new WaveConstruct
    {
        texSize = 1024,
        vertDensity = 1,
        patchSize = 700
    };

    public WaveSettings waveSettings = new WaveSettings
    {
        waterHeight = 0.0f,
        amplitude = 1.6f,
        frequency = 0.7f,
        speed = 0.4f,
        chop = 3.0f,
        smooth = 1.5f,
        vertComplexity = 2,
        pixComplexity = 4,
        direction = 45,
        opposingWaves = true
    };

    public SurfaceFX surfaceFX = new SurfaceFX
    {
        waterColor = new Color( 0.05f, 0.1f, 0.15f, 1.0f ),
        nightWaterColor = new Color( 0.05f, 0.1f, 0.15f, 1.0f ) * 0.5f,
        sunLight = null,
        enableSpecular = true,
        specularBrightness = 0.75f,
        specularPower = 50.0f,
        specularColor = new Color( 0.9f, 0.9f, 0.9f, 0.9f ),
        reflectPower = 3.5f,
        reflectBlend = 0.70f,
    };

    public SSSFX subsurfaceFX = new SSSFX
    {
        enableSSS = true,
        subsurfaceColor = new Color( 0.05f, 0.1f, 0.15f, 1.0f ),
        enableSSSViewLimit = false,
        SSSPower = 5.5f,
        SSSThreshold = 1.8f,
        SSSHeightCutoff = 100,
    };

    public ShoreFX shoreFX = new ShoreFX
    {
        enableShoreFX = true,
        enableRefraction = true,
        foamColor = new Color( 0.71f, 0.914f, 0.95f, 1.0f ),
        minWaveAmp = 0.4f,

        shoreFoam = new ShoreFoam
        {
            lowFreq = 0.01875f,
            highFreq = 0.2f,
            speed = 0.4f,
            depthLowFreq = 8f,
            depthHighFreq = 6f,
        },

        deepFoam = new DeepFoam
        {
            enableDeepFoam = false,
            lowFreq = 0.03f,
            highFreq = 0.4f,
            speed = 1.0f,
            threshold = 2.5f,
            lowFreqPower = 4,
            highFreqPower = 12,
        },
        waterClarity = 10,
        waveDampDepth = 20,

        shallowColor = new Color( 0.5f, 1.0f, 0.85f, 1.0f ),
        shallowColorMultiplier = 2,
    };

    public ReflectFX reflectFX = new ReflectFX
    {
        skybox = null,
        skyboxCapture = true,
        enableSSR = true,
        enablePlanarReflect = false,
        reflectTexSize = 512,
        reflectionDistortion = 0.5f,
    };

    public Underwater underwater = new Underwater
    {
        enableUnderwater = true,
        fogTop = new Color( 0.25f, 0.45f, 0.425f, 1.0f ) * 1.2f,
        fogBottom = new Color( 0.25f, 0.45f, 0.425f, 1.0f ),
        overlayColor = new Color( 0.659f, 1.0f, 1.0f, 1.0f ),
        lipColor = new Color( 0.375f, 0.675f, 0.639f, 1.0f ),
        waveColor = new Color( 0.25f, 0.5f, 0.5f, 1.0f ),
        fogDensity = 0.2f,
        fogHeightDensity = 0.4f,
    };

    public FogSettings fogSettings = new FogSettings
    {
        sampleSkyboxAtHorizon = true,
        horizonSampleOffset = 0.005f,
        overrideUnityFogSettings = false,
        overrideFogColor = new Color( .725f, .804f, .824f, 1.0f ),
    };

    //-----------------------------------------------------------------------------
    // Awake
    //-----------------------------------------------------------------------------
    void Awake()
    {
        ResetData();
        if( Application.isPlaying ) setUpdateInterval( 60 );
        else setUpdateInterval( 2 );


        if( mHydroMat )
        {
            if( QualitySettings.activeColorSpace == ColorSpace.Linear )
            {
                Shader.SetGlobalFloat( "_HydroLinearLighting", 1 );
            }
            else
            {
                Shader.SetGlobalFloat( "_HydroLinearLighting", 0 );
            }
        }

#if UNITY_EDITOR
        transform.hideFlags = HideFlags.HideInInspector;
#endif
    }

    //-----------------------------------------------------------------------------
    // ResetData
    //-----------------------------------------------------------------------------
    public void ResetData()
    {
        DestroyImmediate( mOctaveTex );

        GenerateGeometry();
        CreateBrimMesh();
        CreateCageMesh();
        CreateTextures();
        CreateCameras();

        mUpCount = 999999999;
        mPeriodUpdateCount = 9999999;

        mInternalSSR = reflectFX.enableSSR;
        mInternalPlanarReflect = reflectFX.enablePlanarReflect;
    }

    //-----------------------------------------------------------------------------
    // OnDestroy
    //-----------------------------------------------------------------------------
    void OnDestroy()
    {
        if( mHeightCam )
        {
            mHeightCam.targetTexture = null;
        }

        if( mMaskCam )
        {
            mMaskCam.targetTexture = null;
        }

        if( mReflectCam )
        {
            mReflectCam.targetTexture = null;
        }

        TryDestroyResource( mHeightTex );
        TryDestroyResource( mUnderwaterMaskTex );
        TryDestroyResource( mOctaveTex );
        TryDestroyResource( mReflectTex );
        TryDestroyResource( mVolumeMaskTex );
    }

    //-----------------------------------------------------------------------------
    // TryDestroyResource
    //-----------------------------------------------------------------------------
    void TryDestroyResource( RenderTexture texture )
    {
        if( texture == null )
        {
            return;
        }

        texture.Release();

        if( Application.isPlaying )
        {
            Destroy( texture );
        }
        else
        {
            DestroyImmediate( texture );
        }
    }

    //-----------------------------------------------------------------------------
    // CreateTextures
    //-----------------------------------------------------------------------------
    void CreateTextures()
    {
        if( mHeightCam && mHeightCam.targetTexture == mHeightTex ) mHeightCam.targetTexture = null;  // make sure camera isn't pointing to the existing texture before destroying it

        // Heightfield texture
        DestroyImmediate( mHeightTex );

//        mHeightTex = new RenderTexture( HeightFieldSize, HeightFieldSize, 16 );
        mHeightTex = new RenderTexture( HeightFieldSize, HeightFieldSize, 16, RenderTextureFormat.RFloat );
        mHeightTex.name = "Heightfield Texture";
        mHeightTex.isPowerOfTwo = true;
        mHeightTex.hideFlags = HideFlags.DontSave;
        mHeightTex.filterMode = FilterMode.Bilinear;
//        mHeightTex.wrapMode = TextureWrapMode.Repeat;
        mHeightTex.wrapMode = TextureWrapMode.Clamp;
        Shader.SetGlobalTexture( "_HydroHeightTex", mHeightTex );

        if( mMaskCam && mMaskCam.targetTexture == mUnderwaterMaskTex ) mMaskCam.targetTexture = null;  // make sure camera isn't pointing to the existing texture before destroying it
        DestroyImmediate( mUnderwaterMaskTex );

        mUnderwaterMaskTex = null;
        if( Screen.width > 0 && Screen.height > 0 )
        {
            mUnderwaterMaskTex = new RenderTexture( Screen.width, Screen.height, 16 );  // Do I need a depth buffer?
            mUnderwaterMaskTex.name = "Underwater Mask Texture";
            mUnderwaterMaskTex.isPowerOfTwo = false;
            mUnderwaterMaskTex.hideFlags = HideFlags.DontSave;
            mUnderwaterMaskTex.filterMode = FilterMode.Point;
            mUnderwaterMaskTex.wrapMode = TextureWrapMode.Clamp;
            Shader.SetGlobalTexture( "_HydroUnderwaterMaskTex", mUnderwaterMaskTex );
        }

        // Reflect texture
        if( mReflectCam && mReflectCam.targetTexture == mReflectTex ) mReflectCam.targetTexture = null;  // make sure camera isn't pointing to the existing texture before destroying it
        DestroyImmediate( mReflectTex );

        mReflectTex = new RenderTexture( reflectFX.reflectTexSize, reflectFX.reflectTexSize, 16 );
        mReflectTex.name = "Reflect Texture";
        mReflectTex.isPowerOfTwo = true;
        mReflectTex.hideFlags = HideFlags.DontSave;
        mReflectTex.filterMode = FilterMode.Bilinear;
        mReflectTex.wrapMode = TextureWrapMode.Clamp;

#if UNITY_5_5_OR_NEWER
        mReflectTex.autoGenerateMips = false;
#else
        mReflectTex.generateMips = false;
#endif
        Shader.SetGlobalTexture( "_HydroReflectTex", mReflectTex );


        // Volume Mask texture
        if( mMaskCam && mMaskCam.targetTexture == mVolumeMaskTex ) mMaskCam.targetTexture = null;  // make sure camera isn't pointing to the existing texture before destroying it
        if( Screen.width > 0 && Screen.height > 0 )
        {
            DestroyImmediate( mVolumeMaskTex );
            mVolumeMaskTex = null;

            mVolumeMaskTex = new RenderTexture( Screen.width, Screen.height, 32, RenderTextureFormat.RFloat );
            mVolumeMaskTex.name = "Volume Mask Texture";
            mVolumeMaskTex.isPowerOfTwo = false;
            mVolumeMaskTex.hideFlags = HideFlags.DontSave;
            mVolumeMaskTex.filterMode = FilterMode.Point;
            mVolumeMaskTex.wrapMode = TextureWrapMode.Clamp;
            Shader.SetGlobalTexture( "_HydroVolumeMaskTex", mVolumeMaskTex );
        }
    }

    //-----------------------------------------------------------------------------
    // VRIsPresent
    //-----------------------------------------------------------------------------
    bool VRIsPresent()
    {
#if UNITY_2020_1_OR_NEWER
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
        foreach (var xrDisplay in xrDisplaySubsystems)
        {
            if (xrDisplay.running)
            {
                return true;
            }
        }
#endif
        return false;
    }

    //-----------------------------------------------------------------------------
    // CopyCamData - borrowed directly from Standard Assets Water    
    //-----------------------------------------------------------------------------
    void CopyCamData(Camera src, Camera dest)
    {
        if (dest == null)
        {
            return;
        }

        // set water camera to clear the same way as current camera
        dest.clearFlags = src.clearFlags;
        dest.backgroundColor = src.backgroundColor;
        if (src.clearFlags == CameraClearFlags.Skybox)
        {
            Skybox sky = src.GetComponent<Skybox>();
            Skybox mysky = dest.GetComponent<Skybox>();
            if (!sky || !sky.material)
            {
                if( mysky != null )
                {
                    mysky.enabled = false;
                }
            }
            else
            {
                if( mysky != null )
                {
                    mysky.enabled = true;
                    mysky.material = sky.material;
                }
            }
        }
        // update other values to match current camera.
        // even if we are supplying custom camera&projection matrices,
        // some of values are used elsewhere (e.g. skybox uses far plane)
        dest.farClipPlane = src.farClipPlane;
        dest.nearClipPlane = src.nearClipPlane;
        dest.orthographic = src.orthographic;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;

#if UNITY_2020_1_OR_NEWER
        if( !VRIsPresent() )
#else
        if( !UnityEngine.XR.XRDevice.isPresent )
#endif
        {
           dest.fieldOfView = src.fieldOfView;
        }


    }

    //-----------------------------------------------------------------------------
    // CreateCameras - create cameras for reflection / refraction
    //-----------------------------------------------------------------------------
    void CreateCameras()
    {
        foreach( Transform child in transform )
        {
            if( child.name == "Height Cam" )
            {
                mHeightCam = child.GetComponent<Camera>();
                continue;
            }
            if( child.name == "Reflect Cam" )
            {
                mReflectCam = child.GetComponent<Camera>();
                continue;
            }
            if( child.name == "Mask Cam" )
            {
                mMaskCam = child.GetComponent<Camera>();
                continue;
            }
        }

        if( shoreFX.enableShoreFX && mHeightCam == null )
        {
            GameObject camObj = new GameObject( "Height Cam", typeof(Camera) );
            mHeightCam = camObj.GetComponent<Camera>();
            mHeightCam.gameObject.SetActive( false );

            mHeightCam.orthographic = true;
            mHeightCam.orthographicSize = HeightFieldSize * 0.5f;  // ortho is the half size on the vertical
            mHeightCam.renderingPath = RenderingPath.Forward;

            mHeightCam.nearClipPlane = 1.0f;
            mHeightCam.farClipPlane = 500.0f;
            mHeightCam.clearFlags = CameraClearFlags.SolidColor;
            mHeightCam.backgroundColor = new Color( 1000.0f, 1000.0f, 1000.0f, 1000.0f );
            mHeightCam.cullingMask = -1;

            mHeightCam.transform.SetParent( transform, false );
        }

        if( mHeightCam )
        {
            mHeightCam.targetTexture = mHeightTex;
            Shader depthShader = Resources.Load("DepthWrite", typeof(Shader)) as Shader;
            mHeightCam.SetReplacementShader( depthShader, "" );
        }

        if( reflectFX.enablePlanarReflect && mReflectCam == null )
        {
            GameObject camObj = new GameObject( "Reflect Cam", typeof(Camera) );
            mReflectCam = camObj.GetComponent<Camera>();
            mReflectCam.gameObject.SetActive( false );

            Camera cam = Camera.current;
            if( cam )
            {
                CopyCamData( cam, mReflectCam );
            }

            mReflectCam.transform.SetParent( transform, false );
        }

        if( mReflectCam )
        {
            mReflectCam.targetTexture = mReflectTex;
        }

        if( mMaskCam == null )
        {
            GameObject camObj = new GameObject( "Mask Cam", typeof(Camera) );
            mMaskCam = camObj.GetComponent<Camera>();
            mMaskCam.gameObject.SetActive( false );
            mMaskCam.cullingMask = ( 1<<LayerMask.NameToLayer("Water") );

            Camera cam = Camera.current;
            if( cam )
            {
                CopyCamData( cam, mMaskCam );
            }

            mMaskCam.renderingPath = RenderingPath.Forward;
            mMaskCam.transform.SetParent( transform, false );
        }
        if( mMaskCam )
        {
            mMaskCam.targetTexture = mUnderwaterMaskTex;
        }

        // Create skycapture object if not present
        SkyCapture skyCap = (SkyCapture) FindObjectOfType( typeof(SkyCapture) );
        if( !skyCap )
        {
            GameObject skycapObj = new GameObject( "CapSkybox", typeof(SkyCapture) );
            skycapObj.transform.SetParent( transform, false );
        }



    }

    //-----------------------------------------------------------------------------
    // CreateMesh
    //-----------------------------------------------------------------------------
    Mesh CreateMesh( int numVertsX, int numVertsY, float startOffset, int totalRows )
    {
        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.name = "Water Grid";
        

        // create verts
        Vector3 [] verts = new Vector3[ numVertsX*numVertsY ];
        Vector2 [] uvCoords = new Vector2[ numVertsX*numVertsY ];

        float xInc = 1.0f / (float)(numVertsX-1);
        float yInc = 1.0f / (float)(totalRows-1);

        for( int j=0; j<numVertsY; ++j )
        {
            for( int i=0; i<numVertsX; ++i )
            {
                Vector2 temp = new Vector2( xInc * i, startOffset + yInc*j );
                uvCoords[j*numVertsX+i] = temp;  // store the actual positions in the uv channel
                verts[j*numVertsX+i] = Vector3.zero;  // set verts to all be 0,0,0, that way when it won't be drawn in other passes, all triangles will be degenerate
//                print( System.String.Format( "X:{0:F2},  Y:{1:F2},  Z:{2:F2} ", temp.x, temp.y, temp.z ) );
            }
        }

        mesh.vertices = verts;
        mesh.uv = uvCoords;


        // create vert indices
        int [] indices = new int[ (numVertsX-1) * (numVertsY-1) * 6 ];
        int indexPtr = 0;


        for( int i=0, j=0; j<numVertsY-1; ++j, ++i )
        {
            for( int a=0; a<numVertsX-1; ++a, ++i )
            {
                // do a quad at a time
                indices[indexPtr++] = i;
                indices[indexPtr++] = i+numVertsX;
                indices[indexPtr++] = i+1;

                indices[indexPtr++] = i+1;
                indices[indexPtr++] = i+numVertsX;
                indices[indexPtr++] = i+numVertsX+1;
            }
        }

//print( System.String.Format( "Index count:{0}", indexPtr ) );
//
//for( int i=0; i<indexPtr; ++i )
//{
//    print( System.String.Format( "INDEX:{0}", indices[i] ) );
//}


        mesh.triangles = indices;
        mesh.bounds = new Bounds( Vector3.zero, new Vector3( 100, 100, 100 ) );
        return mesh;        
    }

    //-----------------------------------------------------------------------------
    // CreateCageMesh
    //-----------------------------------------------------------------------------
    void CreateCageMesh()
    {
        DestroyImmediate( mCageMesh );
        
        mCageMat = Resources.Load("CageMat", typeof(Material)) as Material;

        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.name = "Cage Mesh";

        float gridDensity = 1.0f / waveConstruction.vertDensity;

        CAGE_SIZE = gridDensity * 6;

        int numVertsX = (int) (waveConstruction.vertDensity * CAGE_SIZE) + 1;
        int numVertsY = 2;
        int totalRows = 2;

        int vertCount = numVertsX * 2 * 4;  // 2 because there are lots of verts on top and bottom edges, 4 for 4 walls
        vertCount += 4;  // bottom square
        vertCount += numVertsX * numVertsX;  // top mesh

        Vector3 [] verts = new Vector3[vertCount];
        List<Vector3> uvCoords = new List<Vector3>(vertCount);

        float xInc = gridDensity;
        float yInc = 1.0f / (float)(totalRows-1);

        int index = 0;

        // side wall
        for( int j=0; j<numVertsY; ++j )
        {
            for( int i=0; i<numVertsX; ++i )
            {
                Vector3 temp = new Vector3( xInc * i - CAGE_SIZE/2, yInc*j, -CAGE_SIZE/2 );
                uvCoords.Add(temp);  // store the actual positions in the uv channel
                verts[index] = Vector3.zero;  // set verts to all be 0,0,0, that way when it won't be drawn in other passes, all triangles will be degenerate
                ++index;
            }
        }

        // side wall
        for( int j=0; j<numVertsY; ++j )
        {
            for( int i=0; i<numVertsX; ++i )
            {
                Vector3 temp = new Vector3( xInc * i - CAGE_SIZE/2, yInc*j, CAGE_SIZE/2 );
                uvCoords.Add(temp);  // store the actual positions in the uv channel
                verts[index] = Vector3.zero;  // set verts to all be 0,0,0, that way when it won't be drawn in other passes, all triangles will be degenerate
                ++index;
            }
        }

        // side wall
        for( int j=0; j<numVertsY; ++j )
        {
            for( int i=0; i<numVertsX; ++i )
            {
                Vector3 temp = new Vector3( -CAGE_SIZE/2, yInc*j, xInc * i - CAGE_SIZE/2 );
                uvCoords.Add(temp);  // store the actual positions in the uv channel
                verts[index] = Vector3.zero;  // set verts to all be 0,0,0, that way when it won't be drawn in other passes, all triangles will be degenerate
                ++index;
            }
        }

        // side wall
        for( int j=0; j<numVertsY; ++j )
        {
            for( int i=0; i<numVertsX; ++i )
            {
                Vector3 temp = new Vector3( CAGE_SIZE/2, yInc*j, xInc * i - CAGE_SIZE/2 );
                uvCoords.Add(temp);  // store the actual positions in the uv channel
                verts[index] = Vector3.zero;  // set verts to all be 0,0,0, that way when it won't be drawn in other passes, all triangles will be degenerate
                ++index;
            }
        }

        // bottom square
        for( int j=0; j<2; ++j )
        {
            for( int i=0; i<2; ++i )
            {
                Vector3 temp = new Vector3( (i*2-1) * CAGE_SIZE/2, 1, (j*2-1) * CAGE_SIZE/2 );
                uvCoords.Add(temp);  // store the actual positions in the uv channel
                verts[index] = Vector3.zero;  // set verts to all be 0,0,0, that way when it won't be drawn in other passes, all triangles will be degenerate
                ++index;
            }
        }

        // top mesh
        for( int j=0; j<numVertsX; ++j )
        {
            for( int i=0; i<numVertsX; ++i )
            {
                float xOffset = xInc * i - CAGE_SIZE / 2;
                float zOffset = xInc * j - CAGE_SIZE / 2;
                Vector3 temp = new Vector3( xOffset, 0, zOffset );
                uvCoords.Add(temp);  // store the actual positions in the uv channel
                verts[index] = Vector3.zero;  // set verts to all be 0,0,0, that way when it won't be drawn in other passes, all triangles will be degenerate
                ++index;
            }
        }


        mesh.vertices = verts;
        mesh.SetUVs( 0, uvCoords );

        // create vert indices
        int numIndices = (numVertsX-1)*(numVertsX-1) * 6;  // top mesh
        numIndices += ((numVertsX-1) * 6 * 4);  // side walls
        numIndices += 6; // bottom 
        int [] indices = new int[ numIndices ];
        int indexPtr = 0;
        int k=0;

        for( int i=0; i<4; ++i )  // one for each wall of cage
        {
            for( int j=0; j<numVertsY-1; ++j, ++k )
            {
                for( int a=0; a<numVertsX-1; ++a, ++k )
                {
                    // do a quad at a time
                    indices[indexPtr++] = k;
                    indices[indexPtr++] = k+numVertsX;
                    indices[indexPtr++] = k+1;

                    indices[indexPtr++] = k+1;
                    indices[indexPtr++] = k+numVertsX;
                    indices[indexPtr++] = k+numVertsX+1;
                }
            }
            k += numVertsX; // disconnect from previous mesh wall
        }

        // bottom square
        indices[indexPtr++] = k;
        indices[indexPtr++] = k + 2;
        indices[indexPtr++] = k + 1;

        indices[indexPtr++] = k + 1;
        indices[indexPtr++] = k + 2;
        indices[indexPtr++] = k + 3;

        k +=4;

        // top mesh
        for( int j=0; j<numVertsX-1; ++j, ++k )
        {
            for( int a=0; a<numVertsX-1; ++a, ++k )
            {
                // do a quad at a time
                indices[indexPtr++] = k;
                indices[indexPtr++] = k+numVertsX;
                indices[indexPtr++] = k+1;

                indices[indexPtr++] = k+1;
                indices[indexPtr++] = k+numVertsX;
                indices[indexPtr++] = k+numVertsX+1;
            }
        }



        mesh.triangles = indices;
        mesh.bounds = new Bounds( Vector3.zero, new Vector3( 200, 200, 200 ) );
        mCageMesh = mesh;
    }


    //-----------------------------------------------------------------------------
    // CreateBrimMesh
    //-----------------------------------------------------------------------------
    void CreateBrimMesh()
    {
        DestroyImmediate( mBrimMesh );
        
        mBrimMat = Resources.Load("BrimMat", typeof(Material)) as Material;

        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.name = "Brim Mesh";

        int numVertsX = 100;
        int numVertsY = 100;

        // create verts
        Vector3 [] verts = new Vector3[ numVertsX*numVertsY ];
        Vector2 [] uvCoords = new Vector2[ numVertsX*numVertsY ];

        float xInc = 2.0f / (float)(numVertsX-1);
        float yInc = 2.0f / (float)(numVertsY-1);

        for( int j=0; j<numVertsY; ++j )
        {
            for( int i=0; i<numVertsX; ++i )
            {
                Vector2 temp = new Vector2( -1.0f + xInc*i, -1.0f + yInc*j );
                verts[j*numVertsX+i] = Vector3.zero;
                uvCoords[j*numVertsX+i] = temp;
            }
        }

        mesh.vertices = verts;
        mesh.uv = uvCoords;


        // create vert indices
        int [] indices = new int[ (numVertsX-1) * (numVertsY-1) * 6 ];
        int indexPtr = 0;


        for( int i=0, j=0; j<numVertsY-1; ++j, ++i )
        {
            for( int a=0; a<numVertsX-1; ++a, ++i )
            {
                // do a quad at a time
                indices[indexPtr++] = i;
                indices[indexPtr++] = i+numVertsX;
                indices[indexPtr++] = i+1;

                indices[indexPtr++] = i+1;
                indices[indexPtr++] = i+numVertsX;
                indices[indexPtr++] = i+numVertsX+1;
            }
        }


        mesh.triangles = indices;
        mesh.bounds = new Bounds( Vector3.zero, new Vector3( 100, 100, 100 ) );
        mBrimMesh = mesh;
    }


    //-----------------------------------------------------------------------------
    // GenerateGeometry
    //-----------------------------------------------------------------------------
    void GenerateGeometry()
    {
        // remove existing meshes
        for( int i=0; i<mMeshList.Count; ++i )
        {
            DestroyImmediate( mMeshList[i] );
        }
        mMeshList.Clear();


        mHydroMat = Resources.Load("HydroformMat", typeof(Material)) as Material;


        // update variables from the editor
        VertDensity = waveConstruction.vertDensity;
        PatchSize = waveConstruction.patchSize;
        MaxDist = PatchSize - 100.0f;


        mFalloffFactor = 5.0f / PatchSize;  // want edge of patch to be e^3.5 for nice falloff
        Shader.SetGlobalFloat( "_WaveFalloff", mFalloffFactor );



        int numVertsX = (int)(PatchSize * VertDensity) + 1;
        int numVertsY = (int)(PatchSize * VertDensity) + 1;
        int totalVerts = numVertsX * numVertsY;
        int vertsLeft = totalVerts;
        int rowInc = 0;
        
        while( vertsLeft > 0 )
        {
            int rows = MAX_VERTS;
            
            if( vertsLeft < MAX_VERTS )
            {
                rows = vertsLeft + numVertsX;
            }

            rows /= numVertsX;

//            print( System.String.Format( "ROWS:{0}", rows ) );

            Mesh mesh = CreateMesh( numVertsX, rows, Mathf.Max(rowInc)/(float)(numVertsY-1), numVertsY );
            mMeshList.Add( mesh );
            
            vertsLeft -= numVertsX * (rows-1);
            rowInc += rows-1;
        }

        UpdateCamData();

    }

    //-----------------------------------------------------------------------------
    // DrawMeshes
    //-----------------------------------------------------------------------------
    public void DrawMeshes( Camera cam )
    {
        if( cam == null )
        {
            cam = GetCamera();
            if( cam == null ) return;
        }


        Vector3 camPos = cam.transform.position;

        // don't draw the brim if the camera is very close to the water
        if( mBrimMesh && camPos.y > GetMaxWaveHeight() )
        {
            // Draw Brim

            // Note the layer mask is now off so that it's NOT drawn in the underwater pass.  This shouldn't be a problem for the reflection
            // pass as it will be backfaced out.  It should theoretically be a problem for the height cam, but it's not for some reason
#if UNITY_5_4_OR_NEWER
            if( Application.isPlaying )
                Graphics.DrawMesh( mBrimMesh, camPos, Quaternion.identity, mBrimMat, 0 /*LayerMask.NameToLayer("Water")*/, cam, 0, null, false, false, false );
            else
                Graphics.DrawMesh( mBrimMesh, camPos, Quaternion.identity, mBrimMat, 0 /*LayerMask.NameToLayer("Water")*/, null, 0, null, false, false, false );
#else
            Graphics.DrawMesh( mBrimMesh, camPos, Quaternion.identity, mBrimMat, 0 /*LayerMask.NameToLayer("Water")*/, null, 0, null, false, false );
#endif
        }

        if( mHydroMat )
        {
            // Draw main mesh
            for( int i=0; i<mMeshList.Count; ++i )
            {
                if( mMeshList[i] )
                {

#if UNITY_5_4_OR_NEWER
                if( Application.isPlaying )
                        Graphics.DrawMesh( mMeshList[i], camPos, Quaternion.identity, mHydroMat, LayerMask.NameToLayer("Water"), cam, 0, null, false, false, false );
                else
                        Graphics.DrawMesh( mMeshList[i], camPos, Quaternion.identity, mHydroMat, LayerMask.NameToLayer("Water"), null, 0, null, false, false, false );
#else
                    Graphics.DrawMesh( mMeshList[i], camPos, Quaternion.identity, mHydroMat, LayerMask.NameToLayer("Water"), null, 0, null, false, false );
#endif
                }
            }
        }
        
    }

    //-----------------------------------------------------------------------------
    // GetCorrectColor
    //-----------------------------------------------------------------------------
    public Color GetCorrectColor( Color inColor)
    {
        if( QualitySettings.activeColorSpace == ColorSpace.Linear )
        {
            return inColor.linear;
        }
        return inColor;
    }

    //-----------------------------------------------------------------------------
    // FrameUpdate - update these values every frame - for things that need faster updates than PeriodUpdate
    //-----------------------------------------------------------------------------
    public void FrameUpdate()
    {
        Color SSSColor = subsurfaceFX.subsurfaceColor;
        Color waterColor = surfaceFX.waterColor;

        if( surfaceFX.sunLight )
        {

            float lightFactor = Vector3.Dot( Vector3.up, -surfaceFX.sunLight.transform.forward );
            lightFactor = Mathf.Max( lightFactor, 0 );
            waterColor = Color.Lerp( surfaceFX.nightWaterColor, surfaceFX.waterColor, lightFactor );
            if( lightFactor <= 0.1 )
            {
                SSSColor =  Color.Lerp( waterColor, SSSColor, lightFactor / 0.1f );
            }
        }

        Shader.SetGlobalColor( "_WaterColor", GetCorrectColor(waterColor) );
        Shader.SetGlobalColor( "_SSSColor", GetCorrectColor(SSSColor) );
    }


    //-----------------------------------------------------------------------------
    // LateUpdate
    //-----------------------------------------------------------------------------
    void LateUpdate()
    {
        PeriodUpdate();
        FrameUpdate();

        UpdateHeightCam();


        if( !Application.isPlaying )
        {
            DrawMeshes(null);
        }
    }

    //-----------------------------------------------------------------------------
    // OnRenderObject
    //-----------------------------------------------------------------------------
    void OnRenderObject()
    {
        if( mOctaveTex == null )  
        {
            CreateWaveTex();  // do this here in render area because Unity throws error in console if you do render-to-texture in an Update() call
        }
    }

    //-----------------------------------------------------------------------------
    // OnDrawGizmos
    //-----------------------------------------------------------------------------
    void OnDrawGizmos()
    {
        if( Application.isPlaying ) return;

        if( mOctaveTex == null )  
        {
            CreateWaveTex();  // do this here in render area because Unity throws error in console if you do render-to-texture in an Update() call
        }


        PeriodUpdate();

        // Do these updates here in editor mode, otherwise artifacts appear when a camera is selected in editor
        UpdateCamData();
        UpdateReflection();
    }

    //-----------------------------------------------------------------------------
    // Mul vec4 by mat4x4
    //-----------------------------------------------------------------------------
    Vector4 MulVec( Matrix4x4 mat, Vector4 vec )
    {
        Vector4 outVec;
        
        Vector4 row = mat.GetRow(0);
        outVec.x = vec.x * row.x + vec.y * row.y + vec.z * row.z + vec.w * row.w;

        row = mat.GetRow(1);
        outVec.y = vec.x * row.x + vec.y * row.y + vec.z * row.z + vec.w * row.w;
        
        row = mat.GetRow(2);
        outVec.z = vec.x * row.x + vec.y * row.y + vec.z * row.z + vec.w * row.w;

        row = mat.GetRow(3);
        outVec.w = vec.x * row.x + vec.y * row.y + vec.z * row.z + vec.w * row.w;
        
        return outVec;
    }

    //-----------------------------------------------------------------------------
    // ProjPntToWorld
    //-----------------------------------------------------------------------------
    Vector3 ProjPntToWorld( Vector3 pnt, Camera cam, Matrix4x4 invProj, Matrix4x4 invView )
    {
        Vector3 camPos = cam.transform.position;
        camPos.y -= waveSettings.waterHeight + waveSettings.amplitude;

        // project ray into world
//        Vector3 ray = invProj.MultiplyPoint( pnt );
//        ray = invView.MultiplyPoint( ray );

        Vector4 test = new Vector4( pnt.x, pnt.y, pnt.z, 1.0f );

        test = -MulVec( invProj, test );
        test.x = -test.x;

        test.w = 0.0f;
        test = MulVec( invView, test );
        
        
//        print( System.String.Format( "Test : X:{0:F3},  Y:{1:F3},  Z:{2:F3},  W:{3:F3}:", test.x, test.y, test.z, test.w ) );

        Vector3 ray = new Vector3( test.x, test.y, test.z );

        // Check if ray above or below horizon and make adjustment depending on whether underwater
        if( (camPos.y > 0 && ray.y >= -0.001) ||
            (camPos.y <= 0 && ray.y < -0.001) )
        {
            // ray above horizon, find ray angle at horizon line
            Vector3 f1 = cam.transform.forward;
            float ang = Mathf.Atan2( f1.x, f1.z );


            Vector3 eulerAngs = new Vector3( 0.0f, ang * Mathf.Rad2Deg, 0.0f );
            Vector3 backupPos = gameObject.transform.position;
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.Rotate( eulerAngs );
            Matrix4x4 newInvView = gameObject.transform.worldToLocalMatrix.inverse;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.position = backupPos;

            // re-project at horizon line
            test.Set( pnt.x, 0.0f, pnt.z, 1.0f );
            test = -MulVec( invProj, test );
            test.x = -test.x;
            test.w = 0.0f;
            test = MulVec( newInvView, test );

            ray.Set( test.x, test.y, test.z );
            ray.Normalize();
            
            Vector3 worldPos2 = camPos + ray * MaxDist;
            worldPos2.y = 0.0f;  // y value ignored

            return worldPos2;
            

            //--------------------------------------
            // Alternate horizon projection
            //--------------------------------------

//            // ray above horizon, find ray angle at horizon line
//            ray.Normalize();
//            
//            Vector3 horizon = new Vector3( ray.x, 0.0f, ray.z );
//            horizon.Normalize();
//            float horizonAng = Mathf.Acos( Vector3.Dot( horizon, ray ) );
//            
//
//            // bottom ray
//            test.Set( pnt.x, 1.0f, pnt.z, 1.0f );
//            test = -MulVec( invProj, test );
//            test.x = -test.x;
//            test.w = 0.0f;
//            test = MulVec( invView, test );
//            
//            Vector3 bottomRay = new Vector3( test.x, test.y, test.z );
//            bottomRay.Normalize();
//
//            float totalAng = Mathf.Acos( Vector3.Dot( ray, bottomRay ) );
//
//            horizonAng = (2.0f * horizonAng / totalAng) - 1.0f;
//
//            // re-project at horizon line
//            test.Set( pnt.x, horizonAng, pnt.z, 1.0f );
//            test = -MulVec( invProj, test );
//            test.x = -test.x;
//            test.w = 0.0f;
//            test = MulVec( invView, test );
//
//            ray.Set( test.x, test.y, test.z );
//            ray.Normalize();
//            Vector3 worldPos2 = camPos + ray * MaxDist;
//            worldPos2.y = 0.0f;
//
//            return worldPos2;
        }

        ray *= Mathf.Abs( camPos.y / ray.y );  // do intersection with water plane, assume it is at z=0;

        if( ray.magnitude > MaxDist )
        {
            // ray is too long, clamp dist
            ray.Normalize();
            Vector3 worldPos = camPos + ray * MaxDist;
            worldPos.y = 0.0f;  // y value ignored
            return worldPos;
        }


        // calc world position
        return camPos + ray;
    }

    //-----------------------------------------------------------------------------
    // getGridSnapPos
    //-----------------------------------------------------------------------------
    float getGridSnapPos( float pos, float gridDensity )
    {
        return pos - (pos - gridDensity * Mathf.Floor(pos/gridDensity));
    }

    //-----------------------------------------------------------------------------
    // CalcGridParams
    //-----------------------------------------------------------------------------
    void CalcGridParams( Camera cam, Matrix4x4 invProj, Matrix4x4 invView )
    {
        mFrustVerts[0] = new Vector3( -1.15f, -10.0f, 0.0f );
        mFrustVerts[1] = new Vector3(  1.15f, -10.0f, 0.0f );
        mFrustVerts[2] = new Vector3(  1.15f,  10.0f, 0.0f );  // 10 to extend downwards/upwards and compensate for wave height
        mFrustVerts[3] = new Vector3( -1.15f,  10.0f, 0.0f );  // 10 to extend downwards/upwards and compensate for wave height


        // figure out how much roll the camera has
        Vector3 rightCenter = cam.transform.right;
        rightCenter.y = 0.0f;
        rightCenter.Normalize();
        
        float rollAng = Mathf.Acos( Vector3.Dot( cam.transform.right, rightCenter ) );
        if( cam.transform.right.y < 0.0 ) rollAng = -rollAng;   // need to do this becuase using dot product for angle will always be positive


        Matrix4x4 invViewMat = invView;
        
        if( Mathf.Abs( rollAng ) > 0.01 )
        {
            // if there is roll, create new invView matrix with roll component removed
            Vector3 eulerAngs = new Vector3( 0.0f, 0.0f, rollAng * Mathf.Rad2Deg );
            Vector3 backupPos = gameObject.transform.position;
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.Rotate( eulerAngs );
            invViewMat = invView * gameObject.transform.worldToLocalMatrix;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.position = backupPos;
        }
        
        // project the frustum corners onto water plane
        for( int i=0; i<4; ++i )
        {
            mFrustVerts[i] = ProjPntToWorld( mFrustVerts[i], cam, invProj, invViewMat );
        }
        


        Vector3 boundsUL = new Vector3( 1e+25f, 0.0f, 1e+25f );
        Vector3 boundsLR = new Vector3( 1e-25f, 0.0f, 1e-25f );

        for( int i=0; i<4; ++i )
        {
            if( mFrustVerts[i].x < boundsUL.x ) boundsUL.x = mFrustVerts[i].x;
            if( mFrustVerts[i].z < boundsUL.z ) boundsUL.z = mFrustVerts[i].z;

            if( mFrustVerts[i].x > boundsLR.x ) boundsLR.x = mFrustVerts[i].x;
            if( mFrustVerts[i].z > boundsLR.z ) boundsLR.z = mFrustVerts[i].z;
        }

        float gridDensity = 1.0f / VertDensity;
        Vector3 camPos = cam.transform.position;

        // check if camera poiting mostly up or down, likely with no horizon in view.  In this case, move the patch start point back a bit to insure full
        // coverage over the camera
        Vector2 frustSize = new Vector2( (boundsLR.x - boundsUL.x), (boundsLR.z - boundsUL.z) );
        if( (frustSize.x * frustSize.y < PatchSize * PatchSize * 0.25f) ||
            Mathf.Abs(camPos.y - (waveSettings.waterHeight + waveSettings.amplitude) ) < 3 )
        {
            boundsUL.x -= gridDensity * 2;
            boundsUL.z -= gridDensity * 2;
        }

        // special case, check if camera pointing straight up (or down?) and projection returns invalid values for some reason
        int z = 1;
        for( ; z<4; ++z )
        {
            if( Mathf.Abs(mFrustVerts[z].z - mFrustVerts[0].z) > 0.01f )
            {
                break;
            }
        }
        if( z == 4 ) // all the same value, bad camera transform, camera pointed directly up or down
        {
            // just center the patch over the camera
            boundsUL.x = camPos.x - PatchSize * 0.5f;
            boundsUL.z = camPos.z - PatchSize * 0.5f;
        }

        //        print( System.String.Format( "UL BOUND : X:{0:F3},  Y:{1:F3},  Z:{2:F3} ", boundsUL.x, boundsUL.y, boundsUL.z ) );
        //        print( System.String.Format( "LR BOUND : X:{0:F3},  Y:{1:F3},  Z:{2:F3} ", boundsLR.x, boundsLR.y, boundsLR.z ) );


        Vector3 startPos = new Vector3( boundsUL.x, 0.0f, boundsUL.z );
        startPos.x = getGridSnapPos( startPos.x, gridDensity );
        startPos.z = getGridSnapPos( startPos.z, gridDensity );

//        print( System.String.Format( "Start Pos : X:{0:F3},  Y:{1:F3},  Z:{2:F3} ", startPos.x, startPos.y, startPos.z ) );


        Shader.SetGlobalVector( "_PatchStartPos", startPos );
        Shader.SetGlobalFloat( "_PatchSize", PatchSize );


        // set underwater cage pos
        Vector3 lookDir = cam.transform.rotation * Vector3.forward;

        if( lookDir.z >= 0 )
        {
            startPos.z = getGridSnapPos( camPos.z, gridDensity ) + gridDensity * 2; // *2 because if it's just 1, could be behind camera or so close it clips the near plane
        }
        else
        {
            startPos.z = getGridSnapPos( camPos.z, gridDensity ) - gridDensity;
        }

        if( lookDir.x >= 0 )
        {
            startPos.x = getGridSnapPos( camPos.x, gridDensity ) + gridDensity * 2; // *2 because if it's just 1, could be behind camera or so close it clips the near plane
        }
        else
        {
            startPos.x = getGridSnapPos( camPos.x, gridDensity ) - gridDensity;
        }

        Vector4 cageData = startPos;
        cageData.w = CAGE_SIZE;
        Shader.SetGlobalVector( "_CageStartPos", cageData );  // stick the cage size in the w coordinate



//        print( System.String.Format( "Cam Pos : X:{0:F3},  Y:{1:F3},  Z:{2:F3} ", camPos.x, camPos.y, camPos.z ) );
//        print( System.String.Format( "UpLeft : X:{0:F3},  Y:{1:F3},  Z:{2:F3} ", ul.x, ul.y, ul.z ) );


    }

    //-----------------------------------------------------------------------------
    // Updates periodically  - for data that doesn't need to update once a frame
    //-----------------------------------------------------------------------------
    void PeriodUpdate()
    {
        if( ++mPeriodUpdateCount < mPeriodUpdateInterval ) return;
        mPeriodUpdateCount = 0;

        // check if rendertextures lost (happens on screensize changes, going to fullscreen, etc)
        if( mOctaveTex == null || mOctaveTex.IsCreated() != true || mVolumeMaskTex == null )
        {
            ResetData();
            return;
        }

        if( mInternalSkybox != reflectFX.skybox )
        {
            if( reflectFX.skyboxCapture == false )
            {
                // set the cubemap
                mHydroMat.SetTexture( "_Cube", (Texture) reflectFX.skybox );
                mBrimMat.SetTexture( "_Cube", reflectFX.skybox );
            }
        }

        Shader.SetGlobalFloat( "_HydroWaterHeight", waveSettings.waterHeight );

        Vector4 waveData = new Vector4( waveSettings.amplitude, waveSettings.frequency, waveSettings.speed, waveSettings.chop );  // Speed unused in shader at the moment
        Shader.SetGlobalVector( "_WaveData", waveData );
        Shader.SetGlobalFloat( "_WaveSmoothness", waveSettings.smooth );
        Shader.SetGlobalFloat( "_MinWaveAmp", shoreFX.minWaveAmp );

        float waveDirCos = Mathf.Cos(waveSettings.direction*Mathf.Deg2Rad);
        float waveDirSin = Mathf.Sin(waveSettings.direction*Mathf.Deg2Rad);
        Vector4 waveDirRotMat = new Vector4( waveDirCos, -waveDirSin, waveDirSin, waveDirCos );
        Shader.SetGlobalVector( "_WaveDirRotMat", waveDirRotMat );

        Shader.SetGlobalColor( "_FoamColor", GetCorrectColor(shoreFX.foamColor) );
        Shader.SetGlobalVector( "_FoamData", new Vector3( shoreFX.shoreFoam.lowFreq, shoreFX.shoreFoam.highFreq, shoreFX.shoreFoam.speed ) );
        Shader.SetGlobalVector( "_FoamDepth", new Vector2( shoreFX.shoreFoam.depthLowFreq, shoreFX.shoreFoam.depthHighFreq ) );
        Shader.SetGlobalVector( "_DeepFoamData", new Vector3( shoreFX.deepFoam.lowFreq, shoreFX.deepFoam.highFreq, shoreFX.deepFoam.speed ) );
        Shader.SetGlobalVector( "_DeepFoamData2", new Vector3( shoreFX.deepFoam.threshold, shoreFX.deepFoam.lowFreqPower, shoreFX.deepFoam.highFreqPower ) );

        Shader.SetGlobalVector( "_ShoreData", new Vector3( shoreFX.waterClarity, shoreFX.waveDampDepth, 0 ) );

        Vector2 fresnelData = new Vector2( surfaceFX.reflectPower, surfaceFX.reflectBlend );
        Shader.SetGlobalVector( "_FresnelData", fresnelData );

        Shader.SetGlobalFloat( "_OctavesPix", waveSettings.pixComplexity );
        Shader.SetGlobalFloat( "_OctavesVert", waveSettings.vertComplexity );

        Vector4 underwaterData = new Vector4( Application.isPlaying ? -1 : 1, 0, 0, 0 );
        Shader.SetGlobalVector( "_HydroUnderwaterData", underwaterData );

        // Set up reflections so that only one (or neither) can be selected at a time
        if( mInternalSSR != reflectFX.enableSSR )
        {
            mInternalSSR = reflectFX.enableSSR;

            if( mInternalSSR && mInternalPlanarReflect )
            {
                mInternalPlanarReflect = reflectFX.enablePlanarReflect = false;
            }
        }
        if( mInternalPlanarReflect != reflectFX.enablePlanarReflect )
        {
            mInternalPlanarReflect = reflectFX.enablePlanarReflect;

            if( mInternalSSR && mInternalPlanarReflect )
            {
                mInternalSSR = reflectFX.enableSSR = false;
            }
        }


        if( shoreFX.enableRefraction ) Shader.EnableKeyword("HYDRO_REFRACTION");
        else                   Shader.DisableKeyword("HYDRO_REFRACTION");

        if( mHeightCam != null && shoreFX.enableShoreFX )     Shader.EnableKeyword("HYDRO_SHOREFX");
        else                                                  Shader.DisableKeyword("HYDRO_SHOREFX");


        if( surfaceFX.sunLight != null && surfaceFX.enableSpecular )
        {
            Shader.EnableKeyword("HYDRO_SPECULAR");
            Vector3 lightDir = surfaceFX.sunLight.transform.forward;
            Shader.SetGlobalVector( "_LightDir", lightDir );

            Shader.SetGlobalColor( "_SpecularColor", GetCorrectColor(surfaceFX.specularColor * surfaceFX.specularBrightness) );
            Shader.SetGlobalFloat( "_SpecularPower", surfaceFX.specularPower );

        }
        else
        {
            Shader.DisableKeyword("HYDRO_SPECULAR");
        }

        if( subsurfaceFX.enableSSS ) Shader.EnableKeyword("HYDRO_SSS");
        else Shader.DisableKeyword("HYDRO_SSS");

        if( subsurfaceFX.enableSSSViewLimit ) Shader.EnableKeyword("HYDRO_SSS_VIEW_LIMIT");
        else Shader.DisableKeyword("HYDRO_SSS_VIEW_LIMIT");

        if( shoreFX.deepFoam.enableDeepFoam ) Shader.EnableKeyword("HYDRO_DEEP_FOAM");
        else Shader.DisableKeyword("HYDRO_DEEP_FOAM");

        if( waveSettings.opposingWaves ) Shader.EnableKeyword("HYDRO_OPPOSING_WAVESETS");
        else Shader.DisableKeyword("HYDRO_OPPOSING_WAVESETS");

        if( reflectFX.enableSSR ) Shader.EnableKeyword("HYDRO_SSR");
        else Shader.DisableKeyword("HYDRO_SSR");

        if( reflectFX.enablePlanarReflect ) Shader.EnableKeyword("HYDRO_PLANAR_REFELCT");
        else Shader.DisableKeyword("HYDRO_PLANAR_REFELCT");

        Vector3 SSSData = new Vector3( subsurfaceFX.SSSPower, subsurfaceFX.SSSThreshold, subsurfaceFX.SSSHeightCutoff );
        Shader.SetGlobalVector( "_SSSData", SSSData );
        
        Shader.SetGlobalFloat( "_ReflectDistortion", reflectFX.reflectionDistortion );

        Color tempColor = shoreFX.shallowColor * shoreFX.shallowColorMultiplier;
        Shader.SetGlobalColor( "_ShallowWaterColor", GetCorrectColor(tempColor) );

        mHydroMat.SetColor( "_UnderwaterFogTopColor", GetCorrectColor(underwater.fogTop) );
        mHydroMat.SetColor( "_UnderwaterOverlayColor", GetCorrectColor(underwater.overlayColor) );
        mHydroMat.SetColor( "_UnderwaterWaveColor", GetCorrectColor(underwater.waveColor) );

        if( underwater.enableUnderwater && Application.isPlaying )   Shader.EnableKeyword("HYDRO_UNDERWATER");
        else                                Shader.DisableKeyword("HYDRO_UNDERWATER");

        Shader.SetGlobalColor( "_HydroFogColor", fogSettings.overrideUnityFogSettings ? fogSettings.overrideFogColor : RenderSettings.fogColor );
        Shader.SetGlobalFloat( "_HydroSampleHorizon", fogSettings.sampleSkyboxAtHorizon ? 0 : 1);
        Shader.SetGlobalFloat( "_HydroHorizonSampleOffset", fogSettings.horizonSampleOffset);


    }

    //-----------------------------------------------------------------------------
    // UpdateCamData - send camera data to the shader
    //-----------------------------------------------------------------------------
    public void UpdateCamData()
    {
        Camera cam = Camera.current;
        if( cam == null ) return;

        if( cam != mHeightCam && cam != mReflectCam && cam != mMaskCam  )
        {
            if( shoreFX.enableRefraction )
            {
                cam.depthTextureMode = DepthTextureMode.Depth;
            }
            // This causes problems in deferred mode
            //else
            //{
            //    cam.depthTextureMode = DepthTextureMode.None;
            //}
        }
        
        Vector3 camPos = cam.transform.position;
        Matrix4x4 camProjection = GL.GetGPUProjectionMatrix( cam.projectionMatrix, false );
        Matrix4x4 camView = cam.transform.worldToLocalMatrix;

        CalcGridParams( cam, camProjection.inverse, camView.inverse );

        // set shader data

        Shader.SetGlobalFloat( "_ScreenAspect", cam.aspect );
        Shader.SetGlobalVector( "_CamPosition", camPos );
        Shader.SetGlobalMatrix( "_InvCamProj", camProjection.inverse );
        Shader.SetGlobalMatrix( "_CamView", camView );

        Shader.SetGlobalFloat( "_InTime", GetTime() );  // repeat because if seconds get really big, artifacts appear in shader

        float rollAng = cam.transform.eulerAngles.z;
        Shader.SetGlobalFloat( "_CamRollAngle", -rollAng * Mathf.Deg2Rad );

        rollAng = rollAng - 90.0f * Mathf.Floor( rollAng/90.0f );  // modulus 90.0
            
        float meshScale = 1.0f;
            
        if( rollAng <= 45.0f )
        {
            meshScale = Mathf.Lerp( 1.0f, 1.5f, rollAng / 45.0f );
        }
        else
        {
            meshScale = Mathf.Lerp( 1.5f, 1.0f, (rollAng-45.0f) / 45.0f );
        }

        Shader.SetGlobalFloat( "_MeshScale", 1.1f + meshScale );


        // move water object so that it's always in front of the camera and therefore
        // always gets drawn
//        transform.position = cam.transform.position + cam.transform.forward * 5.0f;
    }

    //-----------------------------------------------------------------------------
    // CreateWaveTex
    //-----------------------------------------------------------------------------
    public void CreateWaveTex()
    {
        TexSize = waveConstruction.texSize;

        // Create camera
        GameObject camObj = new GameObject( "_TempCam" );
        Camera cam = camObj.AddComponent<Camera>();;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.white;
        cam.renderingPath = RenderingPath.Forward;
        
        // Create rendertarget (keep around) (note, definitely want mipmaps, and may want to specifically render each one out)
        DestroyImmediate( mOctaveTex );

#if UNITY_IOS || UNITY_ANDROID
        mOctaveTex = new RenderTexture( TexSize, TexSize, 0, RenderTextureFormat.R8 );
#else
        mOctaveTex = new RenderTexture( TexSize, TexSize, 0, RenderTextureFormat.RFloat );
#endif

        Shader.SetGlobalTexture( "_OctaveTex", mOctaveTex );
        mOctaveTex.anisoLevel = 2;
        mOctaveTex.filterMode = FilterMode.Bilinear;
        mOctaveTex.useMipMap = true;
        mOctaveTex.wrapMode = TextureWrapMode.Repeat;
        mOctaveTex.hideFlags = HideFlags.HideAndDontSave;


        // Create mesh
        GameObject quadObj = new GameObject( "RTQuad" );
        quadObj.AddComponent<MeshRenderer>();
        quadObj.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();
        mesh.name = "QuadMesh";
        
        Vector3 [] verts = new Vector3[4] { new Vector3(-1.0f, -1.0f, 0.0f),
                                            new Vector3( 1.0f, -1.0f, 0.0f),
                                            new Vector3(-1.0f,  1.0f, 0.0f),
                                            new Vector3( 1.0f,  1.0f, 0.0f) };
        
        int [] indices = new int[6] { 0, 2, 1, 1, 2, 3 };

        mesh.vertices = verts;
        mesh.triangles = indices;

        quadObj.GetComponent<MeshFilter>().mesh = mesh;


        // Render to the target(s)
        
        // need to make sure the quadObj is in front of the camera or it won't render
        quadObj.transform.position = new Vector3( 0.0f, 0.0f, 10.0f );

        Material octMat = Resources.Load("OctaveMat", typeof(Material)) as Material;
        quadObj.GetComponent<Renderer>().material = octMat;
        
        Shader octShader = Resources.Load("Octave", typeof(Shader)) as Shader;
        Shader.SetGlobalFloat( "_TexSize", (float)TexSize );

        
        cam.SetReplacementShader( octShader, "HydroTag" );
        cam.targetTexture = mOctaveTex;
        cam.Render();
        
        cam.targetTexture = null;  // must do this or the rt will get cleared when camera is destroyed
        
        // Clean up camera and mesh (?)
        if( Application.isPlaying )
        {
            Destroy( quadObj );
            Destroy( camObj );
            Destroy( mesh );
        }
        else
        {
            DestroyImmediate( quadObj );
            DestroyImmediate( camObj );
            DestroyImmediate( mesh );
        }

    }

    //-----------------------------------------------------------------------------
    // GetMaxWaveHeight
    //-----------------------------------------------------------------------------
    float GetMaxWaveHeight()
    {
        return waveSettings.waterHeight + waveSettings.amplitude * 2.0f + waveSettings.amplitude * 0.25f;
    }

    //-----------------------------------------------------------------------------
    // GetCamera
    //-----------------------------------------------------------------------------
    Camera GetCamera()
    {
        if( Camera.current != null && mCurCam != Camera.current )
        {
            if( Camera.current.GetComponent<Hydroform.HydroMultiCamComp>() || !Application.isPlaying )
            {
                mCurCam = Camera.current;
            }
        }

        return mCurCam;
    }

    //-----------------------------------------------------------------------------
    // GetTime - for consistency across scripts / shaders
    //-----------------------------------------------------------------------------
    float GetTime()
    {
        float seconds = Time.realtimeSinceStartup;
        if( Application.isPlaying ) seconds = Time.time;
        
        return Mathf.Repeat( seconds * waveSettings.speed, 5000.0f );
    }

    //-----------------------------------------------------------------------------
    // GetHeightAtPoint 
    //-----------------------------------------------------------------------------
    public float GetHeightAtPoint( Vector3 pnt )
    {
        float waveDirCos = Mathf.Cos(waveSettings.direction*Mathf.Deg2Rad);
        float waveDirSin = Mathf.Sin(waveSettings.direction*Mathf.Deg2Rad);
        Vector4 waveDirRotMat = new Vector4( waveDirCos, -waveDirSin, waveDirSin, waveDirCos );

        float h =  mWaveQuery.GetHeightAtPoint( pnt, waveSettings.frequency, waveSettings.amplitude, waveSettings.chop, 
                                                GetTime(), waveSettings.smooth, waveSettings.vertComplexity, waveSettings.opposingWaves, waveDirRotMat );
                                                
        // reduce wave height in distance to match up with brim
        Camera cam = Camera.main;
        if( cam != null )
        {
            float camDist = Vector3.Distance( cam.transform.position, pnt );
            float distFactor = Mathf.Exp( -camDist * mFalloffFactor );
            h *= distFactor;
        }

        h += waveSettings.waterHeight;
                                                
        return h;                                                
    }

    //-----------------------------------------------------------------------------
    // Calculates reflection matrix around the given plane
    // - borrowed directly from Standard Assets Water
    //-----------------------------------------------------------------------------
    static void CalculateReflectionMatrix( ref Matrix4x4 reflectionMat, Vector4 plane )
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (- 2F * plane[0] * plane[1]);
        reflectionMat.m02 = (- 2F * plane[0] * plane[2]);
        reflectionMat.m03 = (- 2F * plane[3] * plane[0]);

        reflectionMat.m10 = (- 2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (- 2F * plane[1] * plane[2]);
        reflectionMat.m13 = (- 2F * plane[3] * plane[1]);

        reflectionMat.m20 = (- 2F * plane[2] * plane[0]);
        reflectionMat.m21 = (- 2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (- 2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }

    //-----------------------------------------------------------------------------
    // Given position/normal of the plane, calculates plane in camera space.
    // - borrowed directly from Standard Assets Water
    //-----------------------------------------------------------------------------
    Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign, float clipPlaneOffset )
    {
        Vector3 offsetPos = pos + normal * clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    //-----------------------------------------------------------------------------
    // RenderVolumeMasks
    //-----------------------------------------------------------------------------
    public void RenderVolumeMasks()
    {
        if( mInVolumeMaskRender ) return;
        mInVolumeMaskRender = true;

        Camera cam = GetCamera();
        if( cam == null ) return;

        if( !mMaskCam ) return;

        CopyCamData( cam, mMaskCam );


        mMaskCam.transform.position = cam.transform.position;
        mMaskCam.transform.rotation = cam.transform.rotation;
        mMaskCam.clearFlags = CameraClearFlags.SolidColor;
        mMaskCam.backgroundColor = Color.black;
        mMaskCam.targetTexture = mVolumeMaskTex;
        mMaskCam.cullingMask = ( 1<<LayerMask.NameToLayer("VolumeMask") );

        mMaskCam.Render();
        mMaskCam.targetTexture = null;

        mInVolumeMaskRender = false;
    }
    
    //-----------------------------------------------------------------------------
    // UpdateUnderwaterCam
    //-----------------------------------------------------------------------------
    public void UpdateUnderwaterCam()
    {
        Camera cam = GetCamera();
        if( cam == null ) return;
        Vector3 camPos = cam.transform.position;

        if( !mCageMat || !mMaskCam ) return;

        if( camPos.y > GetMaxWaveHeight() )
        {
            mUnderwaterMaskTex.DiscardContents();
        }

        CopyCamData( cam, mMaskCam );

        mMaskCam.transform.position = cam.transform.position;
        mMaskCam.transform.rotation = cam.transform.rotation;
        mMaskCam.clearFlags = CameraClearFlags.SolidColor;
        mMaskCam.backgroundColor = Color.black;
        mMaskCam.targetTexture = mUnderwaterMaskTex;
        mMaskCam.cullingMask = ( 1<<LayerMask.NameToLayer("Water") );

        if( mCageMesh )
        {
            Graphics.DrawMesh( mCageMesh, camPos, Quaternion.identity, mCageMat, LayerMask.NameToLayer( "Water" ), mMaskCam, 0, null, false, false, false );
        }

        mMaskCam.Render();
        mMaskCam.targetTexture = null;
    }

    //-----------------------------------------------------------------------------
    // UpdateReflection
    //-----------------------------------------------------------------------------
    public void UpdateReflection()
    {
        Camera cam = Camera.current;
        if( cam == null ) return;

        if( mInWaterRender ) return;  // prevent recursive render
        mInWaterRender = true;

        // update reflect cam
        if( mReflectCam != null && reflectFX.enablePlanarReflect )
        {
            CopyCamData( cam, mReflectCam );

            const float clipPlaneOffset = 2.07f;
            Vector3 pos = new Vector3( 0, waveSettings.waterHeight, 0 );
            Vector3 normal = new Vector3( 0, 1, 0 );

            // Reflect camera around reflection plane
            float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflection, reflectionPlane);
            Vector3 oldpos = cam.transform.position;
            Vector3 newpos = reflection.MultiplyPoint(oldpos);
            mReflectCam.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            Vector4 clipPlane = CameraSpacePlane(mReflectCam, pos, normal, 1.0f, clipPlaneOffset );
            mReflectCam.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);

            mReflectCam.cullingMask &= ~(1 << LayerMask.NameToLayer("Water")); // never render water layer;
            GL.invertCulling = true;
            mReflectCam.transform.position = newpos;
            Vector3 euler = cam.transform.eulerAngles;
            mReflectCam.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
            mReflectCam.Render();
            mReflectCam.transform.position = oldpos;
            GL.invertCulling = false;
        }

        mInWaterRender = false;
    }
    
    //-----------------------------------------------------------------------------
    // UpdateHeightCam
    //-----------------------------------------------------------------------------
    int mUpCount = 999999999;  // update count - used to count frames between height cam updates

    public void UpdateHeightCam()
    {
        Camera cam = GetCamera();
        if( cam == null ) return;

        if( mInWaterRender ) return;  // prevent recursive render
        mInWaterRender = true;

        // update heightfield cam
        if( mHeightCam != null && shoreFX.enableShoreFX )
        {
            ++mUpCount;
            if( mUpCount > 1000 )
            {
                Vector3 lookVec = new Vector3( 0.0f, -1.0f, 0.0f );
                Vector3 upVec = new Vector3( 0.0f, 0.0f, 1.0f );
                mHeightCam.transform.rotation = Quaternion.LookRotation( lookVec, upVec );
                Vector3 pos = new Vector3( Mathf.Floor(cam.transform.position.x), 100.0f + waveSettings.waterHeight, Mathf.Floor(cam.transform.position.z) );

                mHeightCam.transform.position = pos;
                mHeightCam.cullingMask &= ~(1 << LayerMask.NameToLayer("Water")); // never render water layer;

                Vector3 heightStartPos = new Vector3(pos.x-HeightFieldSize*0.5f, pos.y, pos.z-HeightFieldSize*0.5f);
                Shader.SetGlobalVector( "_HeightFieldStartPos", heightStartPos );

                mHeightCam.Render();
                mUpCount = 0;
            }
        }

        
        mInWaterRender = false;
    }

    //-----------------------------------------------------------------------------
    // setUpdateInterval
    //-----------------------------------------------------------------------------
    public void setUpdateInterval( int interval )
    {
        mPeriodUpdateInterval = interval;
    }
}

} //namespace Hydroform
