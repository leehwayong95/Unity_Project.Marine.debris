using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{

    public float speed;

    private Vector3 vector;

    // shift 누르면 빠르게 달릴 수 있게 변수 따로 지정
    public float runSpeed;
    private float applyRunSpeed; // 실제 적용값
    private bool applyRunFlag = false;

    public int walkCount;
    private int currentWalkCount;


    private bool canMove = true;

    private Animator animator;


    // speed = 2.4, walkCount = 20
    // 2.4 * 20 = 48 >> 한 번 방향키가 눌릴 때마다 48픽셀만큼 이동시키겠다라는 의미
    // While문 사용
    //currentWalkCount += 1 , 20이 될 경우 반복문 종료.


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    IEnumerator MoveCoroutine()
    {
        // 걷다가 서고 걷다가 서고 하는 부분이 부자연스럽기 때문에 반복문 추가!
        // 이렇게 하면, 코루틴은 한 번만 실행되고 이 코루틴 안에서 입력이 이루어지면 계속 이동이 이루어짐.
        while (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0) // 키입력이 이루어졌을 경우
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                applyRunSpeed = runSpeed;
                applyRunFlag = true;
            }
            else
            {
                applyRunSpeed = 0; // 적용 안 된 경우
                applyRunFlag = false;
            }


            vector.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), transform.position.z);
            // 어차피 z축 값은 바뀌지 않기 때문에, 이 스크립트에 적용되는 객체의 z축 값을 계속 적용시킴

            if (vector.x != 0) // 두 개 이상의 방향키를 눌렀을 때 무브먼트 변경을 위한 옵션 설정
                vector.y = 0;

            // vector.x = 1; 우쪽으로 이동
            // vector.y = 0;

            animator.SetFloat("DirX", vector.x);
            animator.SetFloat("DirZ", vector.z);
            // DirX로 전달받아서 좌표값 설정

            animator.SetBool("Walking", true); // 상태전이

            while (currentWalkCount < walkCount)
            {
                // 상하좌우 무브먼트 구현
                if (vector.x != 0)
                {
                    transform.Translate(vector.x * (speed + applyRunSpeed), 1, 0);
                    // transform.position = vector;
                }
                else if (vector.z != 0)
                {
                    transform.Translate(0, 1, vector.z * (speed + applyRunSpeed));
                }
                if (applyRunFlag)
                    currentWalkCount++;

                currentWalkCount++;
                yield return new WaitForSeconds(0.01f); // 대기
            }
            currentWalkCount = 0;
        }
        animator.SetBool("Walking", false); // 다시 서있는 모션으로 변경
        canMove = true; // 코루틴이 완료되면 다시 방향키를 누를 수 있게 true로 바꿔주기
    }

        
    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            // 상하좌우 방향키가 눌렸을 경우
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                // 방향키를 누른 순간 canMove가 false가 되고
                canMove = false; // 이 구간이 두 번 다시 실행 안되게 막아주기
                StartCoroutine(MoveCoroutine());
            }
        }
        
    }
}
