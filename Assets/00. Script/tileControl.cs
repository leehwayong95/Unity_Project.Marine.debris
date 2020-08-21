using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
//using System.IO.Ports; Android Build error
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
        //Transform Lookpostition = null;
        //Lookpostition.position = new Vector3(transform.position.x - 6, transform.position.y + 6, transform.position.z - 6);
        //Lookpostition.rotation = Quaternion.Euler(new Vector3(36, 45, 0));

        //canvas.transform.LookAt(Lookpostition);

        if(pushed)
        {
            //if(transform.position.y == -1.5f)
            if (transform.localScale.z == 0.002f)
                StopCoroutine(editButtonscale());
            else
                StartCoroutine(editButtonscale());
        }
    }

    IEnumerator editButtonscale()
    {
        /* Lerp Transform : flag가 일그러지는 현상 때문에 고려한 기능.
        Vector3 vector = new Vector3(0, -1.5f, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, vector, 0.1f);
        */
        Vector3 targetScale = new Vector3(0.3f, 0.3f, 0.002f);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 0.1f);

        yield return null;
    }

    public void setPushed()
    {
        pushed = true;
    }
}
