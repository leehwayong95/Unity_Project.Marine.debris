namespace Hydroform
{
    using UnityEngine;

    [AddComponentMenu("Hydroform/UnderwaterFilter")]


    //*****************************************************************************
    // UnderwaterFilter
    //*****************************************************************************
    public class UnderwaterFilter : MonoBehaviour
    {
        private Material mMaterial;
        public Shader Shader;
        public bool m_Enable = true;
        private HydroformComponent mHydroform;
        
        //-------------------------------------------------------------------------
        // Start
        //-------------------------------------------------------------------------
        void Start()
        {
            if( mMaterial == null )
            {
                mMaterial = new Material( Shader );
                mMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            mHydroform = (HydroformComponent) FindObjectOfType( typeof(HydroformComponent) );
        }

        //-------------------------------------------------------------------------
        // OnDisable
        //-------------------------------------------------------------------------
        void OnDisable()
        {
            if( mMaterial != null )
            {
                DestroyImmediate( mMaterial );
            }
        }

        //-------------------------------------------------------------------------
        // Update
        //-------------------------------------------------------------------------
        void Update()
        {
            if( mHydroform )
            {
                if( mHydroform.underwater.enableUnderwater ) enabled = true;
                else enabled = false;
            }
        }

        //-------------------------------------------------------------------------
        // OnRenderImage
        //-------------------------------------------------------------------------
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if( mMaterial == null ) return;


			Camera cam = GetComponent<Camera>();

			Transform camtr = cam.transform;
			float camNear = cam.nearClipPlane;
			float camFar = cam.farClipPlane;
			float camFov = cam.fieldOfView;
			float camAspect = cam.aspect;

            Matrix4x4 frustumCorners = Matrix4x4.identity;
			float fovWHalf = camFov * 0.5f;

			Vector3 toRight = camtr.right * camNear * Mathf.Tan (fovWHalf * Mathf.Deg2Rad) * camAspect;
			Vector3 toTop = camtr.up * camNear * Mathf.Tan (fovWHalf * Mathf.Deg2Rad);

			Vector3 topLeft = (camtr.forward * camNear - toRight + toTop);
			float camScale = topLeft.magnitude * camFar/camNear;

            topLeft.Normalize();
			topLeft *= camScale;

			Vector3 topRight = (camtr.forward * camNear + toRight + toTop);
            topRight.Normalize();
			topRight *= camScale;

			Vector3 bottomRight = (camtr.forward * camNear + toRight - toTop);
            bottomRight.Normalize();
			bottomRight *= camScale;

			Vector3 bottomLeft = (camtr.forward * camNear - toRight - toTop);
            bottomLeft.Normalize();
			bottomLeft *= camScale;

            frustumCorners.SetRow( 0, bottomLeft );
            frustumCorners.SetRow( 1, bottomRight );
            frustumCorners.SetRow( 2, topLeft );
            frustumCorners.SetRow( 3, topRight );


            //			var camPos= camtr.position;
            mMaterial.SetMatrix ("_FrustumCornersWS", frustumCorners);
            mMaterial.SetVector( "_CameraWS", cam.transform.position );

            if( mHydroform )
            {
                var camPos = camtr.position;
                float waterHeight = mHydroform.waveSettings.waterHeight + 1;
                float FdotC = camPos.y - waterHeight;
                float paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);
                float heightDensity = mHydroform.underwater.fogHeightDensity;
                mMaterial.SetVector( "_HeightParams", new Vector4( waterHeight, FdotC, paramK, heightDensity) );


                mMaterial.SetColor( "_UnderwaterFogTopColor", mHydroform.GetCorrectColor(mHydroform.underwater.fogTop) );
                mMaterial.SetColor( "_UnderwaterFogBottomColor", mHydroform.GetCorrectColor(mHydroform.underwater.fogBottom) );
                mMaterial.SetColor( "_UnderwaterOverlayColor", mHydroform.GetCorrectColor(mHydroform.underwater.overlayColor) );
                mMaterial.SetColor( "_UnderwaterLipColor", mHydroform.GetCorrectColor(mHydroform.underwater.lipColor) );

                Vector4 fogData = new Vector4( mHydroform.underwater.fogDensity, 0, 0, 0 );
                mMaterial.SetVector( "_UnderwaterData", fogData );
            }



            Graphics.Blit(source, destination, mMaterial, m_Enable ? 0 : 1 );
        }

    }







}
