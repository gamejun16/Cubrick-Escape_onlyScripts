using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using System.Text;

public enum CanvasType
{
    Ingame, Clear,
}

public class UIContainer : MonoBehaviour
{
    static public UIContainer instance;

    [Header("Ingame-Setting-Clear-...")]
    [Tooltip("==UI==")]public Transform uiRoot;

    [Header("IngameCanvas")]
    public Animation anim;
    public Text curStep;
    public Text curStage;
    
    [Header("ClearCanvas")]
    public Text clearStep;
    public Text clearStage;
    public GameObject newRecord;
    
    GameObject[] canvas;
    
    IEnumerator coroutine;
    
    
    private void Start()
    {
        instance = this;
        
        canvas = new GameObject[uiRoot.childCount];
        for(int i=0;i<uiRoot.childCount; i++)
        {
            canvas[i] = uiRoot.GetChild(i).gameObject;
        }

        UpdateStage(StageLoader.instance.GetLevelAndStage());

    }

    #region Ingame canvas functions
    
    /// <summary>
    /// 인게임 및 클리어 UI에 표시되는 스텝 정보를 업데이트
    /// </summary>
    public void UpdateStep()
    {
        StringBuilder sb = new StringBuilder();

        //curStep.text = BrickDataContainer.GetStep() + " step";
        sb.Append(BrickDataContainer.GetStep()).Append(" step");
        curStep.text = sb.ToString();
        sb.Clear();

        clearStep.text = curStep.text;

        if (BrickDataContainer.GetStep() >= BrickDataContainer.GetCurStageBestStep())
            newRecord.SetActive(false);
    }


    /// <summary>
    /// 인게임 및 클리어 UI에 표시되는 현재 레벨 및 스테이지 정보를 업데이트
    /// </summary>
    /// <param name="levelAndStage"></param>
    public void UpdateStage(int[] levelAndStage)
    {
        curStage.text = levelAndStage[0] + " - " + levelAndStage[1];
        clearStage.text = levelAndStage[0] + " - " + levelAndStage[1] + " Stage Clear !!";
    }

    #endregion
    
    #region Canvas On/Off Eff

    /// <summary>
    /// 특정 Canvas를 On 할 때 호출.
    /// </summary>
    public void CanvasOn(CanvasType type, bool isEff = false)
    {
        if (isEff)
        {
            coroutine = CanvasOnEff(canvas[(int)type].transform);
            StartCoroutine(coroutine);
        }
        else
        {
            canvas[(int)type].SetActive(true);
        }
    }

    /// <summary>
    /// 특정 Canvas를 Off 할 때 호출.
    /// </summary>
    public void CanvasOff(CanvasType type, bool isEff = false)
    {
        if (isEff)
        {
            coroutine = CanvasOffEff(canvas[(int)type].transform);
            StartCoroutine(coroutine);
        }
        else
        {
            canvas[(int)type].SetActive(false);
        }
    }


    /// <summary>
    /// Canvas On 연출 코루틴
    /// 화면 중앙에서 뿅 하고 커지면서 튀어나오는 연출
    /// </summary>
    IEnumerator CanvasOnEff(Transform canvas)
    {
        float timer = 0f;

        Transform originParent = canvas.parent;
        GameObject tmpParent = new GameObject("tmpCanvasParent");
        
        canvas.parent = tmpParent.transform;

        canvas.gameObject.SetActive(true);
        tmpParent.transform.localScale = new Vector3(0, 0, 0);

        while(timer < 0.2f)
        {
            timer += Time.deltaTime;

            tmpParent.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(1, 1, 1), timer * 5);

            yield return null;
        }

        tmpParent.transform.localScale = new Vector3(1, 1, 1);
        canvas.parent = originParent;

        Destroy(tmpParent);

        coroutine = null;
    }

    /// <summary>
    /// Canvas Off 연출 코루틴
    /// 화면 중앙으로 뿅 하고 작아지면서 사라지는 연출
    /// </summary>
    IEnumerator CanvasOffEff(Transform canvas)
    {
        float timer = 0f;

        Transform originParent = canvas.parent;
        GameObject tmpParent = new GameObject("tmpCanvasParent");

        canvas.parent = tmpParent.transform;

        tmpParent.transform.localScale = new Vector3(1, 1, 1);

        while (timer < 0.2f)
        {
            timer += Time.deltaTime;

            tmpParent.transform.localScale = Vector3.Lerp(new Vector3(1, 1,1), Vector3.zero, timer * 5);
            yield return null;
        }

        tmpParent.transform.localScale = new Vector3(0, 0);
        canvas.parent = originParent;

        Destroy(tmpParent);

        canvas.gameObject.SetActive(false);

        coroutine = null;
    }


    /// <summary>
    /// 스테이지 클리어시 Ingame Canvas Off 연출 코루틴
    /// </summary>
    /// <returns></returns>
    public void StageClearIngameCanvasOffEff()
    {
        anim.Play("StageClearIngameCanvasOffAnim");
    }

    #endregion
}
