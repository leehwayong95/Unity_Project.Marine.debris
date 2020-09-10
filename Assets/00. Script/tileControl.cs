using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
//using System.IO.Ports; Android Build error
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;

public class tileControl : MonoBehaviour
//타일의 여러가지 기능을 담당하는 스크립트
{
    public bool pushed = false;
    //타일을 눌렀음을 확인하는 변수
    public int hint; 
    //1~8 : hint, 9 : mine
    //이 변수는 이 스크립트를 가지고있는 3D객체가 마인인지
    //아닌지 결정되는 변수. 1~8까지 주변 마인 갯수를 뜻하고,
    //9는 자기자신이 마인임을 뜻한다.

    void Start()
    {
        hint = getHint();
        //자신이 마인인지, 주변에 몇개가 있는지 검사하는 메서드 실행
        //시작시에 실행하는 부분.(아래 구현)
    }

    void Update()
    {
        if (Gamemanager.openTile_arr[(int)transform.position.x,(int)transform.position.z] == 1)
        //GM이 들고있는 openTile_arr 배열을 검사해서, 자신과 해당되는 배열이 1이 된다면
        {
            if (transform.localScale.z == 0.002f)//자신이 안눌려있다면,
                StopCoroutine(editButtonscale());//눌려지는 Coroutine실행
            else
                StartCoroutine(editButtonscale());//자신이 눌려져있다면 Coroutine실행
            showHint(hint);//그리고 힌트를 표출한다.
        }
    }

    IEnumerator editButtonscale()
    //타일 눌림 효과를 여러프레임에 걸쳐 주기 위한 Coroutine
    {
        Vector3 targetScale = new Vector3(0.3f, 0.3f, 0.002f);
        //얼마나 누를것인지 정해두는 변수 tile객체가 (x:0.3,y:0.3,z0.002)로 줄어든다.
        //Z축이 줄어드는 이유는 tile객체를 90도 돌려놨기 때문이다.
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 0.1f);
        //유니티 기본내장 선형보간을 통해 부드럽게 객체의 Scale을 줄인다.
        yield return null;//Coroutine을 위한 yield 반환
    }

    int getHint()
    //힌트를 가져오는 지역함수
    {
        int nearbyMine = 0;
        //자신의 주변에 지뢰가 몇개 있는지 검사하기 위한 변수
        if (Gamemanager.mine_arr[(int)transform.position.x, (int)transform.position.z] == 9)
        //자기자신이 마인이라면
            return 9;
            //그대로 9를 리턴해준다
        else//아니면 주변 타일 검사를 실시한다
        {
            for (int i = ((int)transform.position.x) - 1; i <= ((int)transform.position.x) + 1; i++)
            //자기 주변 X축 -1~+1 범위를 검사한다
            {
                if (i < 0 || i > 9)
                //타일 배열 행이 0~9까지인데 범위를 벗어나게 된다면
                    continue;
                    //반복문 건너뜀으로 예외 처리
                for (int j = ((int)transform.position.z - 1); j <= ((int)transform.position.z) + 1; j++)
                //자기 주변 Y축 -1~+1범위를 검사한다
                {
                    if (j < 0 || j > 9)
                    //타일 배열 열이 0~9까지인데 범위를 벗어나게 된다면
                        continue;
                        //반복문 건너뜀으로 예외처리
                    else if (Gamemanager.mine_arr[i, j] == 9)
                    //그게 아니라면, GM이 들고있는 mine_arr로 검사하는데 그게 지뢰라면
                        nearbyMine++;
                        //주변 마인 개수 변수를 1증가한다.
                }
            }
            return nearbyMine;
            //반복문이 끝나면 검사한 변수를 반환한다
        }   
    }

    void showHint(int num)
    //타일이 눌렸을때 힌트를 보여주기 위한 메서드
    {
        Gamemanager gm = GameObject.FindGameObjectWithTag("GM").GetComponent<Gamemanager>();
        //GM이 들고있는 Material을 참조하기 위해 GM의 소스코드와 객체를 연결한다
        MeshRenderer palnematerial = transform.GetChild(0).GetComponent<MeshRenderer>();
        //이 스크립트를 가지고있는 객체에 상속되어 있는 3D객체의 MeshRenederer의 Component와 연결한다

        if(num != 0)//매개변수로 들어온 수가 0이 아니라면
            palnematerial.material = gm.hintMaterial[num - 1];
            //상속되어있는 3D 객체의 MeshRenederer에 GM에 있는 힌트 배열에서
            //힌트에 해당하는 Material을 적용한다(보여준다)
    }

    public void setPushed()
    //타 스크립트에서 tile을 컨트롤하기 위한 메서드
    {
        pushed = true;//눌림 변수를 활성화시켜 Coroutine을 실행
    }
}
