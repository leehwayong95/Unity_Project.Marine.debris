using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMove : MonoBehaviour
{
    public Transform playerTransform;
    public static bool cameraTopViewMode = false; //false : playerfollow, true : topView
    float middle_x = 4.5f;
    float middle_z = 4.5f;
    float topView_h = 35f;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(cameraTopViewMode)
        {
            StopAllCoroutines();
            StartCoroutine(topView());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(playerFollow());
        }
    }

    IEnumerator playerFollow()
    {
        while (true)
        {
            Vector3 playerPosition = new Vector3(playerTransform.position.x -6, playerTransform.position.y + 5.75f , playerTransform.position.z - 6);
            Quaternion targetRoation = Quaternion.Euler(new Vector3(36.741f, 45, 0));
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, playerPosition, 0.1f);
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, targetRoation, 0.1f);
            yield return null;
        }
    }

    IEnumerator topView()
    {
        while (true)
        {
            //Vector3 playerTopview = new Vector3(playerTransform.position.x, playerTransform.position.y + 30f, playerTransform.position.z);
            Vector3 playerTopview = new Vector3(middle_x, playerTransform.position.y + topView_h, middle_z);
            Quaternion targetRotation = Quaternion.Euler(new Vector3(90, 45, 0));
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, playerTopview, 0.1f);
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, targetRotation, 0.1f);
            yield return null;
        }
    }
    public void selectCameramode()
    {
        cameraTopViewMode = !cameraTopViewMode;
    }
}
