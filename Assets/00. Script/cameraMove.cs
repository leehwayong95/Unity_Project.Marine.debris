using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMove : MonoBehaviour
{
    public Transform playerTransform;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(playerFollow());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator playerFollow()
    {
        while (true)
        {
            Vector3 playerPosition = new Vector3(playerTransform.position.x -6, playerTransform.position.y + 5.75f , playerTransform.position.z - 6);
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, playerPosition, 0.1f);
            yield return null;
        }
    }
}
