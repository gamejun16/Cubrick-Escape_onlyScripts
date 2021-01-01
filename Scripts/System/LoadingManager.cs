using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;


/**
 * 
 * DontDestroyOnLoad 로 선언되며 로딩시 화면을 가리기 위한 캔버스를 포함
 * 
 * process)
 *  # 1. LoadScene 호출
 *  # 2. 타겟 씬 및 스테이지를 Async로 load 및 실행 억제
 *  # 3. 기존 씬에서 화면을 가리는 애니메이션 진행
 *  # 4. 다 가려지면, load 대기 및 load가 완료되면 씬 교체
 *      >> 화면은 그대로 가려져 있어야 함
 *  # 5. 화면이 밝아지고 Load된 씬이 드러나며 게임 진행
 *      >> [000]StartBrick이 플레이어를 소환하는 시점 조율 필요
 *      >> 화면 밝아진 다음 진행되도록
 * 
 * */

public class LoadingManager : MonoBehaviour
{

    static public LoadingManager instance;

    [Header("Components")]
    public Animator anim;


    [Header("Tip")]
    public Text desc;

    string targetScene;
    IEnumerator coroutine;
    bool isLoadingAnimDone;
    float tipWaitTime;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;
    }
    

    /// <summary>
    /// 씬 전환시 호출
    /// </summary>
    public void LoadScene(string _targetScene)
    {
        // 이미 동작중이면 중복 진행 X
        if (coroutine != null) return;

        // 버튼 사운드
        UIAudioManager.instance.AudioPlay(0);

        // 플레이 데이터 초기화
        BrickDataContainer.ResetAllIngameData();

        // 로딩화면에서 표시되는 팁의 내용을 제어
        SetTip(); // 랜덤

        targetScene = _targetScene;
        coroutine = LoadSceneActivation();
        
        StartCoroutine(coroutine);
    }

    /// <summary>
    /// 씬 전환시 호출. 레벨 및 스테이지에 따라 표시되는 팁을 제어
    /// </summary>
    /// <param name="curLevel">현재 레벨 전달</param>
    public void LoadScene(string _targetScene, int curLevel, int curStage)
    {
        // 이미 동작중이면 중복 진행 X
        if (coroutine != null) return;

        // 버튼 사운드
        UIAudioManager.instance.AudioPlay(0);

        // 플레이 데이터 초기화
        BrickDataContainer.ResetAllIngameData();

        // 로딩화면에서 출력되는 팁의 내용을 제어
        int tipOrder = -1;
        if (curLevel == 1)
        {
            if (curStage == 1) tipOrder = 0;
            else if (curStage == 2) tipOrder = 1;
        }
        SetTip(tipOrder);

        targetScene = _targetScene;
        coroutine = LoadSceneActivation();
        
        StartCoroutine(coroutine);
    }
    
    IEnumerator LoadSceneActivation()
    {
        print($"[DEV] Scene Loading now ...");
        
        // # 2
        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        // # 3
        anim.SetTrigger("isLoadingBegin");

        while (!isLoadingAnimDone)
        {
            yield return null;
        }

        // # 4
        // 씬 로딩 대기 및 tipWaitTime 초 동안 대기
        float timer = 0f;
        
        while(op.progress < 0.9f || timer < tipWaitTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        op.allowSceneActivation = true;

        // # 5
        anim.SetTrigger("isLoadingEnd");
        isLoadingAnimDone = false; // reset

        // # done

        coroutine = null;
    }

    public void LoadingDoneEventTrigger() => isLoadingAnimDone = true;


    /// <summary>
    /// 로딩 화면에서 출력되는 팁을 제어한다.
    /// </summary>
    /// <param name="order">특정 팁을 출력한다. 없다면 랜덤하게 출력된다.</param>
    public void SetTip(int order = -1)
    {
        // default tip wait time
        // 팁의 길이가 긴 경우 본 변수에 수치를 + 해서 대기 시간을 늘릴 수 있음
        tipWaitTime = 1;

        switch (order)
        {
            case 0:
                desc.text = "목표 지점에 올라타 이 곳을 탈출하세요.";
                break;

            case 1:
                desc.text = "금이 간 Brick은 부술 수 있을지도 모릅니다.";
                break;
            case 2:

            default:
                int idx = Random.Range(0, 4);

                switch (idx)
                {
                    case 0:
                        desc.text = "설정에서 『한 손』, 『양 손』 모드를 전환할 수 있습니다.";
                        break;
                    case 1:
                        desc.text = "어플리케이션을 재설치하면\n데이터가 <color=\"red\">삭제</color>될 수 있습니다.";
                        break;
                    case 2:
                        desc.text = "최소한의 이동으로 스테이지를 클리어 하세요.";
                        break;
                    case 3:
                        desc.text = "스테이지 선택 창 하단의 \"Show Record\"를 눌러 스테이지별 기록을 볼 수 있습니다.";
                        tipWaitTime += 1f;
                        break;
                    case 4:
                        desc.text = "화면 상단의 Retry를 통해 다시 탈출을 시도하세요.";
                        break;
                }
            break;
        }
    }
    
}
