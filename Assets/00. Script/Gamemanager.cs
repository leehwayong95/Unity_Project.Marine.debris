using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Gamemanager : MonoBehaviour
{
    [SerializeField]
    //디버깅을 위한 Inspector 표출을 위한 SerializeField
    public GameObject Plane;
    //Tile Prefab을 미리 만들어두고, 이를 생성하기 위한 변수
    public GameObject Cube;
    //Player 객체를 연결하기 위한 변수
    public GameObject[] trash = new GameObject[5];
    //테스트 기능 : 타일좌표에 5개의 쓰레기 객체를 보관하기위한 배열
    public Material[] hintMaterial = new Material[9];
    //힌트 표출을 위한 힌트 Material 보관 배열 변수
    public static int mineCount = 0;
    //World에 총 Mine개수를 보관하는 정적변수
    static bool pauseFlag = false;
    //일시정지를 위한 게임 일시 정지 활성화 정적 변수

    public static int[,] mine_arr = new int [10,10];
    //mine배열로 2차원 배열을 생성 World와 같은 크기로 생성
    public static int[,] openTile_arr = new int[10,10];
    //Player의 Click을 보관하기 위한 배열

    void Awake()
    {
        createPlane();
        //시작시 tile을 생성하는 메서드 실행
        createMine();
        //지뢰를 지정하는 메서드 실행
        createTrashRand();
        //테스트 기능 : 쓰레기를 랜덤으로 만드는 기능 실행
        
        openTile_arr[3, 3] = 1;
        openTile_arr[3, 4] = 1;
        openTile_arr[3, 5] = 1;
        
        openTile_arr[4, 3] = 1;
        openTile_arr[4, 4] = 1;
        openTile_arr[4, 5] = 1;
        
        openTile_arr[5, 3] = 1;
        openTile_arr[5, 4] = 1;
        openTile_arr[5, 5] = 1;
        //Player가 생성되는 타일 9개를 이미 눌린상태로 설정
        /**********************************************
        for (int i = 0; i < 10; i++)
            for (int j = 0; j < 10; j++)
                openTile_arr[i, j] = 1;
        //지뢰, 힌트가 재대로 배치되었는지 디버깅을 위한 부분
        ************************************************/
    }

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        //User가 UI 위에 마우스를 두지 않았고
        {
            if (Input.GetMouseButtonDown(0) && !cameraMove.cameraTopViewMode && !pauseFlag)
            //마우스를 누르고, 카메라가 TopView가 아닌상태와 pauseFlag가 비활성화일때
            {
                openTilemapping(selectPosition(pointGameobject()));
                //어떤 게임 을 선택했는지 반환해주는 메서드를 실행하고,
                //반환 받은 값을 기반으로 이미 눌렀던 타일인지 검사해주는 메서드를 실행
                //검사하고 반환받은 값에 따라 타일을 여는 메서드 실행

                //pointGameobject : raycast 이용함수. 타깃 좌표 반환
                //selectPosition : 반환받은 것을 getpushedMapping으로 검사
                //openTilemapping : getpushedMapping으로 검사하고, 안열은거면 pushed_arr에 반환
            }
        }
    }

    void createMine() 
    //GM이 가지고있는 mine_arr에 마인을 랜덤으로 배치하는 메서드
    {
        /* ***********************************************************************
         * 초기 기획으로 10%확률 마인 생성기 코드임
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
        *****************************************************************************/
        while (mineCount < 10)
        //마인이 총 10개가 안되었을 때 계속 반복
        {
            int x, z;
            //생성할 x좌표와 z좌표를 보관할 변수를 만든다
            x = Random.Range(0, 9);
            z = Random.Range(0, 9);
            //x와 z좌표를 0와 9사이를 정수로 지정
            if ((x >= 3 && x <= 5) && (z >= 3 && z <= 5) || (mine_arr[x, z] == 9))
            //Player 리스폰 지역이라면
                continue;//다시 좌표 생성
            else//리스폰지역이아닌 좌표라면
            {
                mine_arr[x, z] = 9;//mine_arr 배열에 마인임을 지정
                mineCount++;//총마인 갯수 증가
            }
        }
    }

    void createPlane()
    //World 생성을 위한 메서드
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Instantiate(Plane,new Vector3( i, 0, j), Quaternion.identity);
                //0~10좌표로 Plane에 연결한 Prefab을 해당 좌표에 생성한다
            }
        }
    }

    void createTrashRand()//테스트기능 : 쓰레기 100개를 랜덤한 좌표에 생성한다.
    {
        for (int i = 0; i < 100; i++)
            Instantiate(trash[Random.Range(0,4)], new Vector3(Random.Range(0f, 9f), 0.1f, Random.Range(0f, 9f)), Quaternion.identity);
    }

    Vector3 pointGameobject() 
    //Raycast 함수화 : User가 어떤것을 선택했는지 반환해주는 메서드
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //카메라 상을 기준으로하고
        RaycastHit hit;
        Vector3 target;
        Physics.Raycast(ray, out hit, 1000f);
        //카메라 상에서 마우스가 클릭한 기준으로 직선거리에 있는 객체 반환
        target = new Vector3(hit.transform.position.x, 0.25f, hit.transform.position.z);
        //객체의 좌표를 x값과 z값을 빼서 좌표를 생성한다.
        //y축은 0.25인데, 이는 Player Cube가 띄워져있는 높이를 말한다
        return target;
        //만든 좌표를 반환한다.
    }

    void openTilemapping(Vector3 clickitem)
    //openTile_arr에 반환해주는 메서드
    {
        if(getpushedMapping((int)clickitem.x, (int)clickitem.z) == 1)
        //열수 있는 조건인지 검사하는 메서드 실행 후 반환되는값이 1이라면
            openTile_arr[(int)clickitem.x, (int)clickitem.z] = 1;
            //열수있는 조건이기때문에 타일을 연다
    }

    Vector3 selectPosition(Vector3 target)
    //Raycast를 통한 좌표 전달받아 moveControl에 반환하는 메서드
    {
        if (getpushedMapping((int)target.x, (int)target.z) == 0)
        //이동할 수 있는지 검사해주는 메서드를 실행하여 0이 반환된다면
            movePlayer.moveControl(target);
            //이미 누른 타일이라서 이동
        else if(getpushedMapping((int)target.x, (int)target.z) == 1)
        //검사한 메서드가 1이 반환한다면
            Debug.Log("이부분은 거리1이내 closetile 열때 조건 부분");
            //열수 있는 부분 거리1이내 타일 개방요청으로 아무것도안한다
        else
            Debug.Log("이부분은 거리2이상 closetile 개방요청 처리부분");
            //거리 2이상 안연 타일 개방요청으로 BFS로 이동하여 근처 타일로 이동한다(미구현)
        return target;
    }

    public int getpushedMapping(int x, int z)
    //플레이어가 움직일 수 있는지 검사하는 메서드
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
    //타 스크립트에서 puase 활성화를 위한 정적 메서드
    {
        pauseFlag = flag;
    }

    public Material getHintMaterial(int hint)
    //TileControl에서 hintMaterial을 전달받기 위한 메서드
    {
        return hintMaterial[hint];
        //매개변수로 전달받은 힌트 정보로 해당 Material을 반환
    }
}
