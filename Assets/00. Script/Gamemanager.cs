using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

public class Gamemanager : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject Plane;
    public GameObject Cube;
    public GameObject Mine;
    public GameObject[] trash = new GameObject[5];
    public static int mineCount;
    void Start()
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

    void createMine()
    {
        for(int i = 0; i < (mineCount = Random.Range(1,8));  i++)
            Instantiate(Mine, new Vector3(Random.Range(0, 9), -2, Random.Range(0, 9)),Quaternion.identity);
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
    public void createTrashRand()
    {
        for (int i = 0; i < 100; i++)
            Instantiate(trash[Random.Range(0,4)], new Vector3(Random.Range(0f, 9f), 0.1f, Random.Range(0f, 9f)), Quaternion.identity);
    }
    public void selectPosition()
    {
        if(Input.GetMouseButtonDown(0))
        {
            //if()UI충돌이 아닐 때 조건 추가
            movePlayer player = GameObject.FindGameObjectWithTag("Player").GetComponent<movePlayer>();
            
            //레이저 쏘기 1000거리만큼
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit, 1000f);
                Debug.Log(hit.transform.position);
            Vector3 target = new Vector3(hit.transform.position.x, 0.25f, hit.transform.position.z);
            player.moveControl(target);
        }
    }

    public static void mineCounter()
    {
        Debug.Log("Mine Count : " + mineCount);
    }
}
