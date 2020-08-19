using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using UnityEngine;

public class tileControl : MonoBehaviour
{
    public bool pushed = false;
    public int hint; //-1 : mine , 0 : load , 1~ :hint
    Canvas canvas;


    void Start()
    {
        canvas = GetComponentInChildren<Canvas>();
    }

    void Update()
    {
        canvas.transform.LookAt(Camera.main.transform);//캔버스가 커브드모니터처럼 휘어버림.. 한번더 생각해볼것
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
