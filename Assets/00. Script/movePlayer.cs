using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movePlayer : MonoBehaviour
{
    Transform playerTransform;
    static bool moveFlag = false;
    static Vector3 targetPosition;

    void Start()
    {
        playerTransform = GetComponent<Transform>();
    }

    void Update()
    {
        if (moveFlag && !cameraMove.cameraTopViewMode)
        {
            StartCoroutine(move());
            if (playerTransform.position == targetPosition)
                moveFlag = false;
        }
        else
            StopCoroutine(move());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Mine"))
            Debug.Log("pushed mine");//마인 밟을때 처리
        else if(other.gameObject.CompareTag("Button"))
        {
            Debug.Log("pushed button");
            tileControl tile = other.gameObject.GetComponent<tileControl>();
            //tile.setPushed();
        }
    }

    IEnumerator move()
    {
        playerTransform.position = Vector3.Lerp(playerTransform.position, targetPosition, 0.1f);
        yield return null;
    }
    public static void moveControl(Vector3 target)
    {
        moveFlag = true;
        targetPosition = target;
    }

    public static void longDistance(Vector3 target)
    {

    }

    public void highlightPlayer()
    {
    }
}
