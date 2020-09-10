using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Worldmap : MonoBehaviour
{
    public GameObject toworldmap_img;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ontoworldmap_img()
    {//Worldmap_Panel Image 버튼 화 onClick으로 연결
     //toworldmap_img popup
        toworldmap_img.SetActive(true);
    }

    public void offtoworldmap_img()
    {//Worldmap_Panel 아니오 버튼 onClick으로 연결
     //toworldmap_img Close
        toworldmap_img.SetActive(false);
    }
}
