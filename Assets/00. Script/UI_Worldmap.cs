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
    {
        toworldmap_img.SetActive(true);
    }

    public void offtoworldmap_img()
    {
        toworldmap_img.SetActive(false);
    }
}
