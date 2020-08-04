﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movePlayer : MonoBehaviour
{
    Transform playerTransform;
    bool moveFlag = false;
    Vector3 target;

    void Start()
    {
        playerTransform = GetComponent<Transform>();
    }

    void Update()
    {
        if (moveFlag)
        {
            StartCoroutine(move());
            if (playerTransform.position == target)
                moveFlag = false;
        }
        else
            StopCoroutine(move());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Mine"))
            Debug.Log("pushed mine");
    }

    public void moveControl(Vector3 target)
    {
        moveFlag = true;
        this.target = target;
    }

    IEnumerator move()
    {
        playerTransform.position = Vector3.Lerp(playerTransform.position, this.target, 0.1f);
        yield return null;
    }    

}
