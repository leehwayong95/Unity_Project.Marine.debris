﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

public class Gamemanager : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject Plane;
    public GameObject Cube;
    public GameObject Mine;
    void Start()
    {
        createPlane();
    }

    // Update is called once per frame
    void Update()
    {
        selectPosition();
    }

    void createPlane()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Vector3 point = new Vector3(i, 0, j);
                Instantiate(Plane, point, Quaternion.identity);
            }
        }
        
        Instantiate(Cube, new Vector3(3, 0.5f, 3), Quaternion.identity);

        for(int i = 0; i < Random.Range(1,8);  i++)
            Instantiate(Mine, new Vector3(Random.Range(0, 9), -2, Random.Range(0, 9)),Quaternion.identity);
    }

    public void selectPosition()
    {
        if(Input.GetMouseButtonDown(0))
        {
            movePlayer player = GameObject.FindGameObjectWithTag("Player").GetComponent<movePlayer>();
            
            //레이저 쏘기 1000거리만큼
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit, 1000f);
                Debug.Log(hit.transform.position);
            Vector3 target = new Vector3(hit.transform.position.x, 0.5f, hit.transform.position.z);
            player.moveControl(target);
        }
    }
}
