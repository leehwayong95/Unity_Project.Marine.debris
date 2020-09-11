//-----------------------------------------------------------------------------
// Hydroform Demo
// Copyright (C) Xix Interactive, LLC  (Brian Ramage)
//-----------------------------------------------------------------------------
using UnityEngine;
//using System.Collections;
using UnityEngine.UI;

namespace Hydroform
{


public class HydroformDemo : MonoBehaviour
{

    HydroformComponent mWater;
    Slider mAmpSlider;
    Slider mFreqSlider;
    Slider mSpeedSlider;
    Slider mSmoothSlider;
    Slider mComplexSlider;
    Slider mReflectBlendSlider;
    Slider mReflectPowerSlider;
    Toggle mRefractToggle;
    Toggle mShoreFXToggle;
    Toggle mSSSToggle;
    Toggle mSSRToggle;
    Slider mReflectDistortSlider;
    Slider mVertDensitySlider;
    Slider mFoamDepthSlider;
    Slider mColorSlider;
    
    float mLastVertDensityUpdate = 0;
    float mNewDensitySliderVal = 0;

    //-----------------------------------------------------------------------------
    // Start
    //-----------------------------------------------------------------------------
	void Start ()
    {
	    HydroformComponent [] compList = FindObjectsOfType( typeof(HydroformComponent) ) as HydroformComponent[];
        if( compList[0] != null )
        {
            mWater = compList[0];
            mWater.setUpdateInterval(2);
        }

        GameObject temp = GameObject.Find("AmpSlider");
        if( temp != null )
        {
            mAmpSlider = temp.GetComponent<Slider>();
            if( mAmpSlider != null )
            {
                mAmpSlider.value = mWater.waveSettings.amplitude;
                mAmpSlider.onValueChanged.AddListener( delegate {OnChangeValue();} );
            }
        }
        temp = GameObject.Find("FreqSlider");
        if( temp != null )
        {
            mFreqSlider = temp.GetComponent<Slider>();
            if( mFreqSlider != null )
            {
                mFreqSlider.value = mWater.waveSettings.frequency;
                mFreqSlider.onValueChanged.AddListener( delegate {OnChangeValue();} );
            }
        }
        temp = GameObject.Find("SpeedSlider");
        if( temp != null )
        {
            mSpeedSlider = temp.GetComponent<Slider>();
            if( mSpeedSlider != null )
            {
                mSpeedSlider.value = mWater.waveSettings.speed;
                mSpeedSlider.onValueChanged.AddListener( delegate {OnChangeValue();} );
            }
        }
        temp = GameObject.Find("SmoothSlider");
        if( temp != null )
        {
            mSmoothSlider = temp.GetComponent<Slider>();
            if( mSmoothSlider != null )
            {
                mSmoothSlider.value = mWater.waveSettings.smooth;
                mSmoothSlider.onValueChanged.AddListener( delegate {OnChangeValue();} );
            }
        }
        temp = GameObject.Find("ComplexSlider");
        if( temp != null )
        {
            mComplexSlider = temp.GetComponent<Slider>();
            if( mComplexSlider != null )
            {
                mComplexSlider.value = mWater.waveSettings.pixComplexity;
                mComplexSlider.onValueChanged.AddListener( delegate {OnChangeValue();} );
            }
        }
        temp = GameObject.Find("ReflectBlendSlider");
        if( temp != null )
        {
            mReflectBlendSlider = temp.GetComponent<Slider>();
            if( mReflectBlendSlider != null )
            {
                mReflectBlendSlider.value = mWater.surfaceFX.reflectBlend;
                mReflectBlendSlider.onValueChanged.AddListener( delegate {OnChangeValue();} );
            }
        }
        temp = GameObject.Find("ReflectPowerSlider");
        if( temp != null )
        {
            mReflectPowerSlider = temp.GetComponent<Slider>();
            if( mReflectPowerSlider != null )
            {
                mReflectPowerSlider.value = mWater.surfaceFX.reflectPower;
                mReflectPowerSlider.onValueChanged.AddListener( delegate {OnChangeValue();} );
            }
        }
        temp = GameObject.Find("RefractToggle");
        if( temp != null )
        {
            mRefractToggle = temp.GetComponent<Toggle>();
            if( mRefractToggle != null )
            {
                mRefractToggle.isOn = mWater.shoreFX.enableRefraction;
                mRefractToggle.onValueChanged.AddListener( delegate {OnChangeValue();} );
            }
        }
        temp = GameObject.Find("ShoreFXToggle");
        if( temp != null )
        {
            mShoreFXToggle = temp.GetComponent<Toggle>();
            if( mShoreFXToggle != null )
            {
                mShoreFXToggle.isOn = mWater.shoreFX.enableShoreFX;
                mShoreFXToggle.onValueChanged.AddListener( delegate {OnChangeValue();} );
            }
        }
        temp = GameObject.Find("SSSToggle");
        if( temp != null )
        {
            mSSSToggle = temp.GetComponent<Toggle>();
            if( mSSSToggle != null )
            {
                mSSSToggle.isOn = mWater.subsurfaceFX.enableSSS;
                mSSSToggle.onValueChanged.AddListener( delegate {OnChangeValue();} );
            }
        }
        temp = GameObject.Find("SSRToggle");
        if( temp != null )
        {
            mSSRToggle = temp.GetComponent<Toggle>();
            if( mSSRToggle != null )
            {
                mSSRToggle.isOn = mWater.reflectFX.enableSSR;
                mSSRToggle.onValueChanged.AddListener( delegate {OnChangeValue();} );
            }
        }
        temp = GameObject.Find("ReflectDistortSlider");
        if( temp != null )
        {
            mReflectDistortSlider = temp.GetComponent<Slider>();
            if( mReflectDistortSlider != null )
            {
                mReflectDistortSlider.value = mWater.reflectFX.reflectionDistortion;
                mReflectDistortSlider.onValueChanged.AddListener( delegate {OnChangeValue();} );
            }
        }
        temp = GameObject.Find( "VertexDensitySlider" );
        if( temp != null )
        {
            mVertDensitySlider = temp.GetComponent<Slider>();
            if( mVertDensitySlider != null )
            {
                mNewDensitySliderVal = mVertDensitySlider.value;
                mWater.waveConstruction.vertDensity = mNewDensitySliderVal / 4.0f;
            }
        }
        temp = GameObject.Find("FoamDepthSlider");
        if( temp != null )
        {
            mFoamDepthSlider = temp.GetComponent<Slider>();
            if( mFoamDepthSlider != null )
            {
                mFoamDepthSlider.value = mWater.shoreFX.shoreFoam.depthLowFreq;
                mFoamDepthSlider.onValueChanged.AddListener( delegate {OnChangeValue();} );
            }
        }
        temp = GameObject.Find("ColorSlider");
        if( temp != null )
        {
            mColorSlider = temp.GetComponent<Slider>();
            if( mColorSlider != null )
            {
                mColorSlider.onValueChanged.AddListener( delegate {OnChangeValue();} );
            }
        }
	}

    //-----------------------------------------------------------------------------
    // CalcColor
    //-----------------------------------------------------------------------------
    Vector4 CalcColor( float delta )
    {
        Vector4 color0 = new Vector4( 24/255.0f, 89/255.0f, 122/255.0f, 1 );
        Vector4 color1 = new Vector4( 18/255.0f, 34/255.0f, 61/255.0f, 1 );
        
        return Vector4.Lerp( color0, color1, delta );
    }

    //-----------------------------------------------------------------------------
    // CalcSSSColor
    //-----------------------------------------------------------------------------
    Vector4 CalcSSSColor( float delta )
    {
        Vector4 color0 = new Vector4( 28/255.0f, 98/255.0f, 114/255.0f, 1 );
        Vector4 color1 = new Vector4( 35/255.0f, 60/255.0f, 78/255.0f, 1 );
        
        return Vector4.Lerp( color0, color1, delta );
    }

    //-----------------------------------------------------------------------------
    // OnChangeValue
    //-----------------------------------------------------------------------------
    void OnChangeValue()
    {
        mWater.waveSettings.amplitude = mAmpSlider.value;
        mWater.waveSettings.frequency = mFreqSlider.value;
        mWater.waveSettings.speed = mSpeedSlider.value;
        mWater.waveSettings.smooth = mSmoothSlider.value;
        mWater.waveSettings.pixComplexity = (int) mComplexSlider.value;
        mWater.surfaceFX.reflectBlend = mReflectBlendSlider.value;
        mWater.surfaceFX.reflectPower = mReflectPowerSlider.value;
        mWater.shoreFX.enableRefraction = mRefractToggle.isOn;
        mWater.shoreFX.enableShoreFX = mShoreFXToggle.isOn;
        mWater.subsurfaceFX.enableSSS = mSSSToggle.isOn;
        mWater.reflectFX.enableSSR = mSSRToggle.isOn;
        mWater.reflectFX.reflectionDistortion = mReflectDistortSlider.value;
        mWater.shoreFX.shoreFoam.depthLowFreq = mFoamDepthSlider.value;
        
        mWater.surfaceFX.waterColor = CalcColor( mColorSlider.value );
        mWater.subsurfaceFX.subsurfaceColor = CalcSSSColor( mColorSlider.value );
    }


    //-----------------------------------------------------------------------------
    // Update
    //-----------------------------------------------------------------------------
    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }   



        if( (mWater.waveConstruction.vertDensity != mVertDensitySlider.value / 4.0f) && 
            mNewDensitySliderVal != mVertDensitySlider.value )
        {
            mLastVertDensityUpdate = Time.time;
            mNewDensitySliderVal = mVertDensitySlider.value;
        }
        
        if( Time.time - mLastVertDensityUpdate > 1 &&
            mWater.waveConstruction.vertDensity != mNewDensitySliderVal / 4.0f)
        {
            mWater.waveConstruction.vertDensity = mNewDensitySliderVal / 4.0f;
            mWater.ResetData();
        }

    }
	
    //-----------------------------------------------------------------------------
    // FixedUpdate
    //-----------------------------------------------------------------------------
    void FixedUpdate()
    {
        if( mWater == null ) return;
        
    }

    //-----------------------------------------------------------------------------
    // LateUpdate
    //-----------------------------------------------------------------------------
	void LateUpdate ()
	{
    }
}

} // namespace Hydroform

