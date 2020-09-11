//-----------------------------------------------------------------------------
// BuoyController
// Copyright (C) Xix Interactive, LLC  (Brian Ramage)
//-----------------------------------------------------------------------------
using UnityEngine;
//using System.Collections;


namespace Hydroform
{

public class BuoyController : MonoBehaviour
{

    HydroformComponent Water;

    //-----------------------------------------------------------------------------
    // Start
    //-----------------------------------------------------------------------------
	void Start ()
    {
	    HydroformComponent [] compList = FindObjectsOfType( typeof(HydroformComponent) ) as HydroformComponent[];
        if( compList[0] != null )
        {
            Water = compList[0];
        }
        
	}

    //-----------------------------------------------------------------------------
    // FixedUpdate
    //-----------------------------------------------------------------------------
    void FixedUpdate()
    {
        if( Water == null ) return;
        
        float height = Water.GetHeightAtPoint( transform.position );
        transform.position = new Vector3( transform.position.x, height, transform.position.z );
    }
}

} // namespace Hydroform
