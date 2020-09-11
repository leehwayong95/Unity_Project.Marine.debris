//-----------------------------------------------------------------------------
// CamController
// Copyright (C) Xix Interactive, LLC  (Brian Ramage)
//-----------------------------------------------------------------------------
using UnityEngine;
using System.Collections;

public class CamController : MonoBehaviour
{
	public Vector3 RotateSpeed;

    public float horizontalSpeed = 0.1F;
    public float verticalSpeed = 0.1F;

    public float camSpeed = 0.2F;

    float mPitchAng = 0.0f;
    float mYawAng = 0.0f;

    //-----------------------------------------------------------------------------
    // Start
    //-----------------------------------------------------------------------------
	void Start ()
    {
	    // set initial pitch / yaw from existing camera orientation
        Vector3 camDir = transform.rotation * Vector3.forward;
        Vector3 camDirNoPitch = camDir;
        camDirNoPitch.y = 0.0f;
        camDirNoPitch.Normalize();
        mPitchAng = Quaternion.Angle( Quaternion.LookRotation( camDir ), Quaternion.LookRotation( camDirNoPitch ) );
        mYawAng = Mathf.Rad2Deg * Mathf.Atan2( camDirNoPitch.x, camDirNoPitch.z );
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
    }
	
    //-----------------------------------------------------------------------------
    // FixedUpdate
    //-----------------------------------------------------------------------------
    void FixedUpdate()
    {
        transform.position += System.Convert.ToSingle(Input.GetKey(KeyCode.W)) * camSpeed * transform.forward;
        transform.position -= System.Convert.ToSingle(Input.GetKey(KeyCode.S)) * camSpeed * transform.forward;

        transform.position += System.Convert.ToSingle(Input.GetKey(KeyCode.D)) * camSpeed * transform.right;
        transform.position -= System.Convert.ToSingle(Input.GetKey(KeyCode.A)) * camSpeed * transform.right;

        // constrain camera distance from the island
//        if( transform.position.y > 50.0f ) transform.position = new Vector3( transform.position.x, 50, transform.position.z );
        
//        Vector3 islandCenter = new Vector3( 236, 0, 173 );
//        Vector3 toCenter = transform.position - islandCenter;
//        if( toCenter.magnitude > 200 )
//        {
//            toCenter.Normalize();
//            transform.position = islandCenter + toCenter * 200;
//        }

    }

    //-----------------------------------------------------------------------------
    // LateUpdate
    //-----------------------------------------------------------------------------
	void LateUpdate ()
	{
        if( Input.GetMouseButton(1) )
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            float h = horizontalSpeed * Input.GetAxis("Mouse X");
            float v = verticalSpeed * Input.GetAxis("Mouse Y");

            mYawAng += h * Mathf.Rad2Deg;

            mPitchAng += -v * Mathf.Rad2Deg;
            mPitchAng = Mathf.Clamp( mPitchAng, -90, 90 );

            transform.rotation = Quaternion.Euler( mPitchAng, mYawAng, 0.0f );
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }



// Test all rotations on camera
//        Vector3 rotations = new Vector3( 0.0f, 10.0f, 0.25f );
//        rotations.x = Time.time * RotateSpeed.x;
//        rotations.y = Time.time * RotateSpeed.y;
//        rotations.z = Time.time * RotateSpeed.z;
////        rotations.z = Mathf.Sin( Time.time * RotateSpeed.z ) * 360.0f / 8.0f;
//        transform.eulerAngles = rotations;

    }
}
