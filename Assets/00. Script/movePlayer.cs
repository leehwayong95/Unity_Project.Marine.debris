using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movePlayer : MonoBehaviour
//Player를 움직이는 스크립트
{
    Transform playerTransform;
    //Player의 좌표를 이동시키 위한 변수생성
    static bool moveFlag = false;
    //타스크립트에서 컨트롤하기 위한 bool변수 생성
    static Vector3 targetPosition;
    //타 스크립트에서 타겟 포지션을 전달 받기 위한 정적 변수 생성

    void Start()
    //시작할때
    {
        playerTransform = GetComponent<Transform>();
        //Player의 위치를 담당하고 있는 Component와 객체를 연결한다.
    }

    void Update()
    {
        if (moveFlag && !cameraMove.cameraTopViewMode)
        //moveFlag변수가 활성화되어있고, 카메라의TopViewMode가 아니라면
        {
            StartCoroutine(move());
            //move 메서드를 여러프레임에 걸쳐 실행한다
            if (playerTransform.position == targetPosition)
            //전달받은 targetPositon과 Player의 위치가 같다면
                moveFlag = false;
                //moveFlag변수를 비활성화 시킨다.
        }
        else//if조건에 벗어난다면
            StopCoroutine(move());
            //move메서드 Coroutine을 중지 시킨다.
    }

    IEnumerator move()
    //Player 움직임을 위한 Coroutine 메서드
    {
        playerTransform.position = Vector3.Lerp(playerTransform.position, targetPosition, 0.1f);
        //Player의 position을 선형보간을 이용해 targetPosition으로 부드럽게 이동시킨다
        yield return null;
    }
    public static void moveControl(Vector3 target)
    //타 스크립트에서 플레이어를 움직이기 위한 정적 메서드
    {
        moveFlag = true;
        //moveFlag를 활성화시키고
        targetPosition = target;
        //매개변수로 전달받은 target좌표를 내부 targetPosition변수에 저장한다
    }

    public static void longDistance(Vector3 target)
    {
        //BFS로 이동하는 부분(미구현)
    }
}
