using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager uimanager;
    public GameObject Pause_Canvas;
    public GameObject Inventory_Panel;
    public GameObject WorldMap_Panel;
    public GameObject Setting_Panel;

    // Start is called before the first frame update
    void Start()
    {
        uimanager = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void openPauseCanvas()
    {
        Pause_Canvas.SetActive(true);
        Time.timeScale = 0.0f;
    }

    public void closePauseCanvas()
    {
        Pause_Canvas.SetActive(false);
        Time.timeScale = 1.0f;
    }

    public void onInventory_Panel()
    {
        WorldMap_Panel.SetActive(false);
        Setting_Panel.SetActive(false);
        Inventory_Panel.SetActive(true);
    }

    public void onWorldMap_Panel()
    {
        Setting_Panel.SetActive(false);
        Inventory_Panel.SetActive(false);
        WorldMap_Panel.SetActive(true);
    }

    public void onSetting_Panel()
    {
        Inventory_Panel.SetActive(false);
        WorldMap_Panel.SetActive(false);
        Setting_Panel.SetActive(true);
    }
}
