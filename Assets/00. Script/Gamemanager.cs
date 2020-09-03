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
    public static int mineCount;
    static bool pauseFlag = false;

    public static int[,] mine_arr = new int [10,10];//mine배열


    void Awake()
    {

        createPlane();
        createMine();
        createTrashRand();
    }

    // Update is called once per frame
    void Update()
    {
        selectPosition();
    }

    void createMine() //GM이 가지고있는 mine_arr에 마인 구성
    {
        //test
        //for(int i = 0; i < (mineCount = Random.Range(1,8));  i++)
        //Instantiate(Mine, new Vector3(Random.Range(0, 9), -2, Random.Range(0, 9)),Quaternion.identity);
        for (int i = 0; i< 10; i++)
        {
            for(int j = 0; j<10; j++)
            {
                int minepercent = Random.Range(0, 100);
                if (minepercent < 10)
                {
                    mine_arr[i, j] = 9;
                    mineCount++;
                }
                else
                    mine_arr[i, j] = 0;
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
    public void selectPosition()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0) && !cameraMove.cameraTopViewMode && !pauseFlag)
            {
                //if()UI충돌이 아닐 때 조건 추가, player find 부분 삭제 고민중..
                //movePlayer player = GameObject.FindGameObjectWithTag("Player").GetComponent<movePlayer>();
                //Raycast를 통한 좌표 구해 moveControl에 전달
                movePlayer.moveControl(pointGameobject());
            }
            else if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("Click Right Button");
            }
        }   
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
