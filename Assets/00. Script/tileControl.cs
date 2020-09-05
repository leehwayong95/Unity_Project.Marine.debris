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
    }

    void Update()
    {
        if (Gamemanager.openTile_arr[(int)transform.position.x,(int)transform.position.z] == 1)
        {
            if (transform.localScale.z == 0.002f)
                StopCoroutine(editButtonscale());
            else
                StartCoroutine(editButtonscale());
            showHint(hint);
        }
    }

    IEnumerator editButtonscale()
    {
        Vector3 targetScale = new Vector3(0.3f, 0.3f, 0.002f);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 0.1f);

        yield return null;
    }

    int getHint()
    {
        int nearbyMine = 0;
        //mine일때 hint에 9리턴
        if (Gamemanager.mine_arr[(int)transform.position.x, (int)transform.position.z] == 9)
            return 9;
        else//아니면 주변 타일 검사
        {
            for (int i = ((int)transform.position.x) - 1; i <= ((int)transform.position.x) + 1; i++)
            {
                if (i < 0 || i > 9)
                    continue;
                for (int j = ((int)transform.position.z - 1); j <= ((int)transform.position.z) + 1; j++)
                {
                    if (j < 0 || j > 9)
                        continue;
                    else if (Gamemanager.mine_arr[i, j] == 9)
                        nearbyMine++;
                }
            }
            return nearbyMine;
        }   
    }

    void showHint(int num)
    {
        Gamemanager gm = GameObject.FindGameObjectWithTag("GM").GetComponent<Gamemanager>();
        MeshRenderer palnematerial = transform.GetChild(0).GetComponent<MeshRenderer>();

        if(num != 0)
            palnematerial.material = gm.hintMaterial[num - 1];
    }

    public void setPushed()
    {
        pushed = true;
    }
}
