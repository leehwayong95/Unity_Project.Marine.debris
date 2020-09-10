using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMove : MonoBehaviour
//카메라의 위치와 회전을 조절하는 스크립트
{
    public Transform playerTransform;
    //Player를 따라다니기 위해 Player의 위치를 저장하는변수
    public static bool cameraTopViewMode = false; 
    //false : playerfollow, true : topView
    //카메라 탑뷰를 활성화 하는 bool변수
    float middle_x = 4.5f;
    float middle_z = 4.5f;
    float topView_h = 35f;
    //TopView 좌표를 지정하기 위한 변수

    void Update()
    {
        if(cameraTopViewMode)
        //카메라 모드가 탑뷰이면,
        {
            StopAllCoroutines();//이 스크립트에서 실행하는 Coroutine을 모두 중지하고
            StartCoroutine(topView());//topView 메서드를 Coroutine 실행
        }
        else
        //카메라 모드가 탑뷰가 아니라면
        {
            StopAllCoroutines();//이 스크립트에서 실행하는 Coroutine을 모두 중지하고
            StartCoroutine(playerFollow());//playerFollow 메서드 Coroutine 실행
        }
    }

    IEnumerator playerFollow()
    //카메라가 Player객체를 따라다니는 Coroutine 메서드
    {
        while (true)//아래 내용을 계속 실행한다
        {
            Vector3 playerPosition = new Vector3(playerTransform.position.x -6, playerTransform.position.y + 5.75f , playerTransform.position.z - 6);
            //player Position에서 (-6,+5.75, -6)만큼 떨어져있는 좌표 객체 생성
            Quaternion targetRoation = Quaternion.Euler(new Vector3(36.741f, 45, 0));
            //QuarterView를 위한 회전 Vector값을 변환하여 Quaternion 객체 생성
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, playerPosition, 0.1f);
            //카메라의 위치를 선형보간으로 부드럽게 생성한 좌표로 이동한다.
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, targetRoation, 0.1f);
            //카메라의 회전값을 선형보간으로 부드럽게 생성한 Quaternion값으로 회전시킨다.
            yield return null;//Coroutine을 위한 yield return
        }
    }

    IEnumerator topView()
    //카메라가 topView를 위한 Coroutine 메서드
    {
        while (true)//아래 내용을 계속 실행한다
        {
            Vector3 playerTopview = new Vector3(middle_x, playerTransform.position.y + topView_h, middle_z);
            //미리 지정한 좌표를 기점으로 좌표 객체를 생성한다
            Quaternion targetRotation = Quaternion.Euler(new Vector3(90, 45, 0));
            //Rotation도 테스트를 한 좌표로 객체를 만든다
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, playerTopview, 0.1f);
            //카메라의 위치를 부드럽게 선형보간으로 이동시킨다.
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, targetRotation, 0.1f);
            //카메라의 회전을 부드럽게 선형보간으로 돌린다.
            yield return null;//Coroutine을 위한 yield 반환
        }
    }
    public void selectCameramode()
    //UI에서 연결하여 사용할 메서드
    {
        cameraTopViewMode = !cameraTopViewMode;
        //이게 실행 된다면, 카메라 모드를 토글형식으로 반전시켜 변수에 넣어준다.
    }
}
