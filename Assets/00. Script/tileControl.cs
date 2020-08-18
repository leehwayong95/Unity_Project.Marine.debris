using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using UnityEngine;

public class tileControl : MonoBehaviour
{
    public bool pushed = false;

    void Start()
    {
        Transform transform = GetComponent<Transform>();
    }

    void Update()
    {
        if(pushed)
        {
            if (transform.localScale.z == 0.002f)
                StopCoroutine(editButtonscale());
            else
                StartCoroutine(editButtonscale());
        }
    }

    IEnumerator editButtonscale()
    {
        Vector3 targetScale = new Vector3(0.3f, 0.3f, 0.002f);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 0.1f);
        yield return null;
    }

    public void setPushed()
    {
        pushed = true;
    }
}
