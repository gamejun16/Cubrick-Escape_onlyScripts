using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class StageSelector : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Levels-Contents")] public RectTransform levelContents;
    public List<Text> levelProgress;

    [Space]
    public GameObject StageSelectCanvas;
    public Text curLevelText, curLevelScoreText;
    public List<Button> stages; // = new List<Button>();
    public List<Animator> stageAnims;
    public List<Text> stageStepRecordTexts;
    public List<Text> stageTimeRecordTexts;

    [Header("Environments")]
    public Animator environmentAnim;
    

    IEnumerator coroutine;
    List<Button> levels = new List<Button>();
    
    int curSelectLevel; // Stage 선택 직전 선택한 Level을 저장

    int showRecordIdx = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        InitLevelBtns();
        
        levelContents.anchoredPosition = new Vector3(4000, 0, 0);
    }

    /// <summary>
    /// 진행 정도에 따라 버튼들을 활성화하는 함수
    /// </summary>
    void InitLevelBtns()
    {
        // get button list
        for (int i = 0; i < levelContents.childCount; i++)
        {
            levels.Add(levelContents.GetChild(i).GetComponent<Button>());
            levels[i].interactable = false;
            levels[i].transform.GetChild(4).gameObject.SetActive(true);
        }
        
        // open unlocked level btns
        int level = PlayerPrefs.GetInt("level");
        int stage = PlayerPrefs.GetInt("stage");

        // 진행 가능한 가장 높은 레벨 버튼이 화면 중앙에 위치하도록
        coroutine = LevelMoveEff(levelContents.anchoredPosition + new Vector2(-2000 * (level - 1), 0));
        StartCoroutine(coroutine);

        for (int i = 0; i < level; i++)
        {
            levels[i].interactable = true;
            levels[i].transform.GetChild(4).gameObject.SetActive(false);

            // set progress
            if (i == level - 1)
            {
                levelProgress[i].text = $"Progress : {(stage - 1) * 100 / 9}%";
            }
            else
                levelProgress[i].text = "Progress : 100%";
        }

        print($"[DEV] {level} 레벨이 Open 되었습니다.");
    }
    
    public void LeftArrow()
    {
        if (coroutine == null && levelContents.anchoredPosition.x < 3000)
        {
            // audio play
            UIAudioManager.instance.AudioPlay(2);

            coroutine = LevelMoveEff(levelContents.anchoredPosition + new Vector2(2000, 0));
            StartCoroutine(coroutine);
        }
    }

    public void RightArrow()
    {
        if (coroutine == null && levelContents.anchoredPosition.x > -3000)
        {
            // audio play
            UIAudioManager.instance.AudioPlay(2);

            coroutine = LevelMoveEff(levelContents.anchoredPosition + new Vector2(-2000, 0));
            StartCoroutine(coroutine);
        }
    }

    /// <summary>
    /// 레벨 버튼들에 움직이는 이펙트를 적용하는 코루틴
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    IEnumerator LevelMoveEff(Vector2 targetPos)
    {
        float timer = 0f;
        float speed = 15f;
        
        do
        {
            timer += Time.deltaTime;

            levelContents.anchoredPosition = Vector2.Lerp(levelContents.anchoredPosition, targetPos, Time.deltaTime * speed);

            yield return null;
        } while (Vector2.Distance(levelContents.anchoredPosition, targetPos) > 1f);

        levelContents.anchoredPosition = targetPos;

        coroutine = null;
    }
    

    /// <summary>
    /// 레벨 선택 후 스테이지 선택 탭을 호출하는 함수
    /// </summary>
    /// <param name="level">선택한 레벨. (1~n)</param>
    public void OpenStageBtnsTab(int selectLevel)
    {
        // init
        MobileDeviceSupporter.instance.AddToUICloser(CloseStageBtnTab);

        curSelectLevel = selectLevel;
        curLevelText.text = "LEVEL " + curSelectLevel;
        //curLevelScoreText.text = "Total time : " + DataContainer.instance.GetGameRecordStagesTotalTime(curSelectLevel);
        curLevelScoreText.text = "Total step : " + DataContainer.instance.GetGameRecordStagesTotalStep(curSelectLevel);

        for (int i=0;i<stages.Count; i++)
        {
            stageStepRecordTexts[i].text = 0.ToString();
            stages[i].interactable = false;
            stages[i].transform.GetChild(3).gameObject.SetActive(true); // lock img
        }
        
        //open unlock stage btns
        int countIdx;
        if(PlayerPrefs.GetInt("level") == curSelectLevel)
            countIdx = PlayerPrefs.GetInt("stage");
        else
            countIdx = stages.Count;

        List<int> stageRecords = DataContainer.instance.GetGameRecordStagesStep(curSelectLevel);

        for(int i=0;i<countIdx; i++)
        {
            if (i < stageRecords.Count)
            {
                stageStepRecordTexts[i].text = stageRecords[i].ToString();
            }

            stages[i].interactable = true;

            stages[i].transform.GetChild(3).gameObject.SetActive(false); // lock img
        }

        // play audio
        UIAudioManager.instance.AudioPlay(1);

        StageSelectCanvas.SetActive(true);
        
    }

    /// <summary>
    /// 스테이지 선택 탭을 닫는 함수
    /// </summary>
    public void CloseStageBtnTab()
    {
        showRecordIdx = 0;


        MobileDeviceSupporter.instance.RemoveFromUICloser(CloseStageBtnTab);
        StageSelectCanvas.SetActive(false);
    }
    
    /// <summary>
    /// 스테이지 선택 함수
    /// </summary>
    /// <param name="stage">선택 스테이지.</param>
    public void SelectStage(int stage)
    {
        environmentAnim.SetTrigger("isEnter");

        // 로딩시 출력 팁 선택
        int tipOrder = -1;
        if (curSelectLevel == 1)
        {
            if (stage == 1) tipOrder = 0;
            else if (stage == 2) tipOrder = 1;
        }
        

        LoadingManager.instance.SetTip(tipOrder);
        StageLoader.LoadStage(curSelectLevel, stage);
        LoadingManager.instance.LoadScene("IngameScene", curSelectLevel, stage);
    }

    /// <summary>
    /// 스테이지별 기록 조회 제어 함수
    /// </summary>
    public void ShowRecordLoopController()
    {
        switch (showRecordIdx)
        {
            case 0:
                StageStepRecordOn();
                showRecordIdx += 1;
                break;
            case 1:
                StageRecordOff();
                showRecordIdx = 0;
                break;
        }
    }

    /// <summary>
    /// 스테이지별 스탭 기록을 보여주는 함수
    /// </summary>
     void StageStepRecordOn()
    {
        int countIdx;
        if (PlayerPrefs.GetInt("level") == curSelectLevel)
            countIdx = PlayerPrefs.GetInt("stage");
        else
            countIdx = stages.Count;

        for (int i = 0; i < countIdx; i++)
            stageAnims[i].SetTrigger("stepRecordOn");

        UIAudioManager.instance.AudioPlay(2);
    }

    ///// <summary>
    ///// 스테이지별 시간 기록을 보여주는 함수
    ///// </summary>
    // void StageTimeRecordOn()
    //{
    //    int countIdx;
    //    if (PlayerPrefs.GetInt("level") == curSelectLevel)
    //        countIdx = PlayerPrefs.GetInt("stage");
    //    else
    //        countIdx = stages.Count;

    //    for (int i = 0; i < countIdx; i++)
    //        stageAnims[i].SetTrigger("timeRecordOn");

    //    UIAudioManager.instance.AudioPlay(2);
    //}


    /// <summary>
    /// 스테이지별 기록을 끄는 함수
    /// </summary>
     void StageRecordOff()
    {

        int countIdx;
        if (PlayerPrefs.GetInt("level") == curSelectLevel)
            countIdx = PlayerPrefs.GetInt("stage");
        else
            countIdx = stages.Count;

        for (int i = 0; i < countIdx; i++)
            stageAnims[i].SetTrigger("recordOff");

        UIAudioManager.instance.AudioPlay(2);
    }
    
    
}
