using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Setting : MonoBehaviour
{//사운드 버튼 Toggle로 On/Off 구현
    public Toggle soundToggle;

    // Start is called before the first frame update
    void Start()
    {//soundToggle Toggle로 연결
        soundToggle = GetComponent<Toggle>();
    }

    // Update is called once per frame
    void Update()
    {//위에 선언한 soundToggle로 onoff_Sound()함수 실행
        onoff_Sound(soundToggle);
    }

    public void onoff_Sound(Toggle soundToggle)
    {//Toggle 내장함수 isOn = true 일 때
        if(soundToggle.isOn)
        {//AudioSource 볼륨 1 -> Sound On
            AudioListener.volume = 1.0f;
        }
     //Toggle 내장함수 isOn = false 일 때
        else
        {//AudioSource 볼륨 0 -> Sound Off
            AudioListener.volume = 0.0f;
        }
    }
}
