using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMover : MonoBehaviour
{
    static public PlayerMover instance; // singleton

    public delegate void Step();
    public event Step StepEvent;

    
    [Header("Components")]
    public Animator anim;
    public AudioSource audioSource;

    public bool isMovePossible;

    IEnumerator coroutine;

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        StepEvent += BrickDataContainer.Step;
    }
    

    // Update is called once per frame
    void Update()
    {
        if (!isMovePossible) return;
        KeyInputCheck();
    }

    /// <summary>
    /// 키 입력 및 이동 방향 확인
    /// </summary>
    void KeyInputCheck()
    {
        if (coroutine == null && Input.anyKeyDown)
        {
            Vector3 dir = Vector3.zero; ;
            
            // 전
            if (Input.GetKeyDown(KeyCode.W))
                dir = new Vector3(0, 0, 1);
            
            // 후
            else if (Input.GetKeyDown(KeyCode.S))
                dir = new Vector3(0, 0, -1);
            
            // 좌
            else if (Input.GetKeyDown(KeyCode.A))
                dir = new Vector3(-1, 0, 0);
            
            // 우
            else if (Input.GetKeyDown(KeyCode.D))
                dir = new Vector3(1, 0, 0);


            if (dir != Vector3.zero) Movement(dir);
        }
    }

    /// <summary>
    /// 이동
    /// </summary>
    /// <param name="dir">방향</param>
    void Movement(Vector3 dir)
    {
        StepEvent();
        UIContainer.instance.UpdateStep();

        RaycastHit hit;
        Ray ray = new Ray(transform.position, dir);
        
        if (Physics.Raycast(ray, out hit, 1f))
        {
            //print($"[TEST] {hit.collider.name}에 막혀서 갈 수 없습니다. ");
            bool isPassPossible = hit.collider.GetComponent<BrickActionController>().CallBrickAction();
            
            if (isPassPossible)
            {
                coroutine = MoveSpin(dir);
                StartCoroutine(coroutine);
            }
            else
            {
                anim.SetTrigger("isBlocked");
            }
        }
        else
        {
            // audio play
            audioSource.Play();

            coroutine = MoveSpin(dir);
            StartCoroutine(coroutine);
        }
    }


    /// <summary>
    /// 터치에 의해 호출되는 함수
    /// </summary>
    /// <param name="input"></param>
    public void MovementByTouch(int input)
    {
        if (!isMovePossible || coroutine != null) return;

        Vector3 dir = Vector3.zero; ;

        switch (input)
        {
            case 0: // 전
                dir = new Vector3(0, 0, 1);
                break;
            case 1: // 후
                dir = new Vector3(0, 0, -1);

                break;
            case 2: // 좌
                dir = new Vector3(-1, 0, 0);

                break;
            case 3: // 후
                dir = new Vector3(1, 0, 0);

                break;
        }

        if (dir != Vector3.zero) Movement(dir);
    }

    IEnumerator MoveSpin(Vector3 dir)
    {

        float timer = 0f;
        float moveTimeLimit = 0.15f; // n초에 맞춰서 이동

        // position
        Vector3 originPos = transform.position;
        Vector3 targetPos = transform.position + dir;

        
        // rotation
        // dir 정보를 토대로 전후좌우 방향 정보 추출 및 목표 회전 방향 기록
        Vector3 originRot = transform.eulerAngles;
        Vector3 targetRot = dir.z != 0 ? (dir.z == 1 ? transform.eulerAngles + new Vector3(90, 0, 0) : transform.eulerAngles + new Vector3(-90, 0, 0)) : (dir.x == -1 ? transform.eulerAngles + new Vector3(0, 0, 90) : transform.eulerAngles + new Vector3(0, 0, -90));

        while (timer < moveTimeLimit )
        {
            timer += Time.deltaTime;

            transform.position = Vector3.Lerp(originPos, targetPos, timer * (1 / moveTimeLimit));

            transform.eulerAngles = Vector3.Lerp(originRot, targetRot, timer * (1 / moveTimeLimit));



            yield return null;
        }

        transform.position = targetPos;
        transform.eulerAngles = new Vector3(0, 0, 0);
    
        coroutine = null;   
    }
    

    public void CallAnimTrigger(string trigger)
    {
        isMovePossible = false;
        anim.SetTrigger(trigger);
    }
}
