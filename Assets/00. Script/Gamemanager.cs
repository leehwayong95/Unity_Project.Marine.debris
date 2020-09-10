using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Gamemanager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    public GameObject Mine;

    public GameObject Plane;
    public GameObject Cube;
    public GameObject[] trash = new GameObject[5];
    public Material[] hintMaterial = new Material[9];
    public static int mineCount = 0;
    static bool pauseFlag = false;

    public static int[,] mine_arr = new int [10,10];//mine배열
    public static int[,] openTile_arr = new int[10,10];//클릭한 배열

    void Awake()
    {
        createPlane();
        createMine();
        createTrashRand();
        //testing
        openTile_arr[3, 3] = 1;
        openTile_arr[3, 4] = 1;
        openTile_arr[3, 5] = 1;
        
        openTile_arr[4, 3] = 1;
        openTile_arr[4, 4] = 1;
        openTile_arr[4, 5] = 1;
        
        openTile_arr[5, 3] = 1;
        openTile_arr[5, 4] = 1;
        openTile_arr[5, 5] = 1;
        //testing
        /*
        for (int i = 0; i < 10; i++)
            for (int j = 0; j < 10; j++)
                openTile_arr[i, j] = 1;
        */
    }

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0) && !cameraMove.cameraTopViewMode && !pauseFlag)
            {
                openTilemapping(selectPosition(pointGameobject()));
                //pointGameobject : raycast 이용함수. 타깃 좌표 반환
                //selectPosition : 반환받은 것을 getpushedMapping으로 검사
                //openTilemapping : getpushedMapping으로 검사하고, 안열은거면 pushed_arr에 반환
            }
        }
    }

    void createMine() //GM이 가지고있는 mine_arr에 마인 구성
    {
        /* 10%확률 마인 생성기 코드
        for (int i = 0; i< 10; i++)
        {
            for(int j = 0; j<10; j++)
            {
                //플레이어 초기 Tile을 위해 Mine생성 초기화
                if ((i == 3 || i == 4 || i == 5) && (j == 3 || j == 4 || j == 5)) 
                    continue;
                int minepercent = Random.Range(0, 100);
                if (minepercent < 10)
                {
                    mine_arr[i, j] = 9;
                    mineCount++;//mineCount : 마인 갯수 세기
                }
                else
                    mine_arr[i, j] = 0;
            }
        }
        */
        //testing
        while (mineCount < 10)
        {
            int x, z;
            x = Random.Range(0, 9);
            z = Random.Range(0, 9);
            //X,Z &&연결 부분 OR로 바꾸면 그 행,열 다 빔
            if ((x >= 3 && x <= 5) && (z >= 3 && z <= 5) || (mine_arr[x, z] == 9)) 
                continue;//리스폰지역, 이미 마인인 부분 다시 랜덤찍기
            else//필터 안피해갔으면 마인 지정
            {
                mine_arr[x, z] = 9;
                mineCount++;
            }
        }
    }

    void createPlane()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Instantiate(Plane,new Vector3( i, 0, j), Quaternion.identity);
            }
        }
    }

    void createTrashRand()
    {
        for (int i = 0; i < 100; i++)
            Instantiate(trash[Random.Range(0,4)], new Vector3(Random.Range(0f, 9f), 0.1f, Random.Range(0f, 9f)), Quaternion.identity);
    }

    Vector3 pointGameobject() //Raycast 함수화
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 target;

        Physics.Raycast(ray, out hit, 1000f);
        Debug.Log(hit.transform.position);
        target = new Vector3(hit.transform.position.x, 0.25f, hit.transform.position.z);
        //y축 0.25f는 cube가 띄워져있는 높이
        return target;
    }

    void openTilemapping(Vector3 clickitem)
    {
        if(getpushedMapping((int)clickitem.x, (int)clickitem.z) == 1)
            openTile_arr[(int)clickitem.x, (int)clickitem.z] = 1;
    }

    Vector3 selectPosition(Vector3 target)
    {
        //Raycast를 통한 좌표 전달받아 moveControl에 전달
        if (getpushedMapping((int)target.x, (int)target.z) == 0)//getpushedMapping 0반환시
            movePlayer.moveControl(target);//이미 누른 타일이라서 이동
        else if(getpushedMapping((int)target.x, (int)target.z) == 1)//안누른타일..
            Debug.Log("이부분은 거리1이내 closetile 열때 조건 부분");//이부분에 BFS..
        else
            Debug.Log("이부분은 거리2이상 closetile 개방요청 처리부분");//2 : 움직이기불가
        return target;
    }

    public int getpushedMapping(int x, int z)
    {
        //플레이어의 무빙 검사 메서드
        //0 : closetile의 개방
        //1 : opentile의 이동
        //2 : 거리2이상의 closetile 개방 (금지 처리)
        if (openTile_arr[x, z] == 1)//arr가 이미 눌려있으면, 0반환
            return 0;
        else if(openTile_arr[x,z] == 0)//안눌려있으면
        {
            for(int i=x-1 ; i<=x+1 ; i++)//누른 타일 기준으로 주변 검사
            {
                if (i < 0 || i > 9)//overflow, underflow 방지
                    continue;
                for (int j=z-1; j<=z+1; j++)
                {
                    if (j < 0 || j > 9)//overflow, underflow 방지
                        continue;
                    else if (openTile_arr[i, j] == 1)//주변에 1이 있으면 1반환
                        return 1;
                }
            }
            return 2;//해당 반복문에 필터가 되지 않았으면 움직이기 불가이기 때문에 2반환
        }
        else//이 해당 조건이 모두 아니면, 아무것도아니기 때문에 2 반환
            return 2;
    }

    public void mineCounter() //마인 갯수 호출가능한지 테스트 static method
    {
        Debug.Log("Mine Count : " + Gamemanager.mineCount);
    }

    public static void setPause(bool flag)
    {
        pauseFlag = flag;
    }

    public Material getHintMaterial(int hint)
    {
        return hintMaterial[hint];
    }
}
