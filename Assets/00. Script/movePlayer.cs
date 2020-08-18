using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movePlayer : MonoBehaviour
{
    Transform playerTransform;
    bool moveFlag = false;
    Vector3 targetPosition;

    void Start()
    {
        playerTransform = GetComponent<Transform>();
    }

    void Update()
    {
        if (moveFlag)
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
            tile.setPushed();
        }
    }

    IEnumerator move()
    {
        playerTransform.position = Vector3.Lerp(playerTransform.position, this.targetPosition, 0.1f);
        yield return null;
    }
    public void moveControl(Vector3 target)
    {
        moveFlag = true;
        this.targetPosition = target;
    }

    public void highlightPlayer()
    {

    }
}
