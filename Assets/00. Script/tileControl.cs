using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
//using System.IO.Ports; Android Build error
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;

public class tileControl : MonoBehaviour
{
    public bool pushed = false;
    public int hint; //1~8 : hint, 9 : mine
    Canvas canvas;


    void Start()
    {
        canvas = GetComponentInChildren<Canvas>();
        //자신이 마인인지 아닌지 받아오는 부분
        hint = getHint();
        if (hint == 9)
            Debug.Log("i'm Mine");
        else
            Debug.Log(hint);
    }

    void Update()
    {
        //canvas.transform.LookAt(Camera.main.transform);//캔버스가 커브드모니터처럼 휘어버림.. 한번더 생각해볼것
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

    int getHint() //HINT 검사부분 고쳐야합니다. 예외처리가 안됩니다.
    {
        int nearbyMine = 0;
        //mine일때 hint에 9리턴
        if (Gamemanager.mine_arr[(int)transform.position.x, (int)transform.position.z] == 9)
            return 9;
        else//아니면 주변 타일 검사
        {
            for (int i = ((int)transform.position.x) - 1; i <= ((int)transform.position.x) + 1; i++)
            {
                for (int j = ((int)transform.position.z - 1); i <= ((int)transform.position.z) + 1; j++)
                {
                    if (Gamemanager.mine_arr[i, j] == 9)
                        nearbyMine++;
                }
            }
            return nearbyMine;
        }   
    }

    void showHint()//마인 디버깅용 메서드 무시하셔도 됩니다.
    {
        Gamemanager gm = GameObject.FindGameObjectWithTag("GM").GetComponent<Gamemanager>();
        Material plane = canvas.GetComponentInChildren<Material>();

        plane = gm.getHintMaterial(hint-1);

    }

    public void setPushed()
    {
        pushed = true;
    }
}
