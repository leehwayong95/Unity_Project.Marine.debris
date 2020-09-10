using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager uimanager;
    public GameObject Pause_Canvas;
    public GameObject Inventory_Panel;
    public GameObject WorldMap_Panel;
    public GameObject Setting_Panel;

    // Start is called before the first frame update
    void Start()
    {//script UImanager uimanager로 호출
        uimanager = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void openPauseCanvas()
    {//Pause 버튼에 onClick으로 연결
        Pause_Canvas.SetActive(true);
    //PauseCanvas popup 시 게임 시간 멈춤
    //Script Gamemanager에 setPause()참조
        Gamemanager.setPause(true);
    }

    public void closePauseCanvas()
    {//X 버튼에 onClick으로 연결
        Pause_Canvas.SetActive(false);
     //X 버튼 클릭 시 게임 시간 흐름
     //Script Gamemanager에 setPause()참조
        Gamemanager.setPause(false);
    }

    public void onInventory_Panel()
    {//Inventory 버튼에 onClick으로 연결
     //Inventory_Panel만 popup
        WorldMap_Panel.SetActive(false);
        Setting_Panel.SetActive(false);
        Inventory_Panel.SetActive(true);
    }

    public void onWorldMap_Panel()
    {//World 버튼에 onClick으로 연결
     //World_Panel만 popup
        Setting_Panel.SetActive(false);
        Inventory_Panel.SetActive(false);
        WorldMap_Panel.SetActive(true);
    }

    public void onSetting_Panel()
    {//Setting 버튼에 onClick으로 연결
     //Setting_Panel만 popup
        Inventory_Panel.SetActive(false);
        WorldMap_Panel.SetActive(false);
        Setting_Panel.SetActive(true);
    }
}
