using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;


[System.Serializable]
public class StageRow
{
    public int[] bricks;

    public StageRow(int count)
    {
        bricks = new int[count];
    }

    public StageRow(int[] _bricks)
    {
        bricks = _bricks;
    }
}

[System.Serializable]
public class StageInfo
{
    public int floor;

    public StageRow[] rows;


    public StageInfo(int _floor, int _count)
    {
        floor = _floor;

        rows = new StageRow[_count];
        for (int i = 0; i < rows.Length; i++)
        {
            rows[i] = new StageRow(_count);
        }
    }

    public StageInfo(int _floor, StageRow[] _rows)
    {
        floor = _floor;

        rows = _rows;
    }
}

public class StageLoader : MonoBehaviour
{
    static public StageLoader instance;

    static int loadLevel, loadStage;
    
    bool isGoToHomeScene;

    StageInfo []stageInfos;

    // Start is called before the first frame update
    void Start()
    {
        if(instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }


        // 현재 스테이지의 기존 최고 점수를 게임 플레이 환경에 전달
        // 플레이 종료 시점에 플레이어의 기록과 전달된 기존 기록을 비교해 최고 점수 여부를 판별
        List<int> stepScoreList = DataContainer.instance.GetGameRecordStagesStep(loadLevel);

        for (int i = 0; i < 9 - stepScoreList.Count; i++) {
            stepScoreList.Add(int.MaxValue);
        }
        
        BrickDataContainer.SetCurStageBestStep(stepScoreList[loadStage - 1]);
        
        stageInfos = new StageInfo[3];

        LoadStageInfoFromJson();
    }
    
    static public void LoadStage(int _level, int _stage)
    {
        loadLevel = _level;
        loadStage = _stage;
    }

    /// <summary>
    /// 로드 레벨 및 스테이지 정보 갱신
    /// </summary>
    /// <param name="l">loadLevel</param>
    /// <param name="s">loadStage</param>
    public void SetLevelAndStage(int l, int s)
    {
        loadLevel = l;
        loadStage = s;
    }

    /// <summary>
    /// 현재 레벨,스테이지 반환
    /// </summary>
    /// <returns>[0]: 레벨, [1]: 스테이지</returns>
    public int[] GetLevelAndStage()
    {
        int[] levelAndStage = new int[2];
        levelAndStage[0] = loadLevel;
        levelAndStage[1] = loadStage;

        return levelAndStage;
    }

    /// <summary>
    /// Stage를 load하는 함수
    /// 미리 설정된 level과 stage 값에 따라 stage가 load 된다.
    /// </summary>
    void LoadStageInfoFromJson()
    {
        try
        {
            for (int floor = 0; floor < 3; floor++)
            {
                string jsonData;
                string tmpPath, path;

#if UNITY_EDITOR
                // LogMonitor.instance.AddLog("Load stage on UNITY_EDITOR");

                tmpPath = "Stages/" + loadLevel + "/" + loadStage + "/stageInfo_" + floor + ".json";
                //path = Path.Combine(Application.dataPath, tmpPath);
                path = Path.Combine(Application.streamingAssetsPath, tmpPath);

                jsonData = File.ReadAllText(path);


#elif UNITY_ANDROID
                // LogMonitor.instance.AddLog("Load stage on UNITY_ANDROID");

                tmpPath = "Stages/" + loadLevel + "/" + loadStage + "/stageInfo_" + floor + ".json";
                path = Path.Combine(Application.streamingAssetsPath, tmpPath);

                WWW reader = new WWW(path);
                while (!reader.isDone) ;

                jsonData = reader.text;
#endif

                
                stageInfos[floor] = JsonUtility.FromJson<StageInfo>(jsonData);
            }

            LogMonitor.instance.AddLog("Load stage is done");
            print($"[DEV] LoadStageInfo done");

            // 브릭 생성 및 배치
            SpawnBricks();
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.Message);
            LogMonitor.instance.AddLog($"[ERROR] Error Msg : {e.Message}");
        }
    }

    /// <summary>
    /// json 파일에서 load한 정보를 토대로 브릭 생성 및 배치
    /// </summary>
    void SpawnBricks()
    {

        string bricksPath = "Prefabs/Bricks/";
        string targetBrickId = "";

        for (int floor = 0; floor < 3; floor++)
        {
            for (int r = 0; r < stageInfos[floor].rows.Length; r++)
            {
                for (int c = 0; c < stageInfos[floor].rows[r].bricks.Length; c++)
                {
                    if (stageInfos[floor].rows[r].bricks[c] == -1) continue;

                    targetBrickId = "[" + string.Format("{0:D3}", stageInfos[floor].rows[r].bricks[c]) + "]Brick";

                    var obj = Resources.Load<GameObject>(bricksPath + targetBrickId);
                    GameObject brick = Instantiate(obj);


                    brick.transform.position = new Vector3(c - 2, 8 - (8 * floor), 2 - r);
                }
            }
        }

        print($"[DEV] Spawn Bricks done");
    }

    /// <summary>
    /// 목표 씬 로드
    /// </summary>
    /// <param name="_targetScene">로드하고 하는 씬 이름</param>
    public void CallLoadScene(string _targetScene)
    {
        //LoadingManager.instance.LoadScene(_targetScene);
        LoadingManager.instance.LoadScene(_targetScene, loadLevel, loadStage);
    }
    
    /// <summary>
    /// 다음 스테이지 로드
    /// </summary>
    public void CallNextStage()
    {
        //// 금방 업데이트 됐다면
        //if (loadLevel == PlayerPrefs.GetInt("level") && loadStage == PlayerPrefs.GetInt("stage"))
        //{
        //    if(loadLevel == 5 && loadStage == 9)
        //        LoadingManager.instance.LoadScene("HomeScene");
            
        //    else if(loadStage == 1)
        //        LoadingManager.instance.LoadScene("HomeScene");

        //    else
        //    {
        //        LoadStage(loadLevel, loadStage);
        //        LoadingManager.instance.LoadScene("IngameScene", loadLevel, loadStage);
        //    }
        //}

        //// 이미 클리어 이력이 있는 스테이지를 클리어한 것 이라면
        //else
        //{
        //    // load, level 업데이트 및 진행 여부 확인

        //    if(loadStage == 9)
        //    {
        //        if(loadLevel == 5)
        //        {
        //            LoadingManager.instance.LoadScene("HomeScene");
        //            return;
        //        }
        //        else
        //        {
        //            loadLevel += 1;
        //            loadStage = 1;
        //        }
                
        //    }
        //    else
        //    {
        //        loadStage += 1;
        //    }

        //    LoadStage(loadLevel, loadStage);
        //    LoadingManager.instance.LoadScene("IngameScene", loadLevel, loadStage);
        //}

        if(loadStage == 9)
        {
            if(loadLevel == 5)
            {
                // 최종 레벨의 최종 스테이지가 클리어 된 상황
            }
            else
            {
                loadLevel += 1;
                loadStage = 1;
            }
        }
        else
        {
            loadStage += 1;
        }

        LoadingManager.instance.LoadScene("IngameScene");
    }

    /// <summary>
    /// 게임 클리어시 brick[001]에서 호출.
    /// 스코어 및 레벨-스테이지 정보를 업데이트한다.
    /// </summary>
    public void GameClear()
    {
        int level = PlayerPrefs.GetInt("level"), stage = PlayerPrefs.GetInt("stage");
        int score = BrickDataContainer.GetStep();

        // 스코어 갱신
        DataContainer.instance.UpdateScore(loadLevel, loadStage, score);

        // 가장 마지막 스테이지를 클리어했다면, 다음 스테이지 Unlock.
        if(level == loadLevel && stage == loadStage)
        {
            if (stage == 9)
            {
                if (level == 5)
                {
                    // 최종 레벨의 최종 스테이지가 클리어 된 상황
                }
                else
                {
                    level += 1;
                    stage = 1;
                }
            }
            else
            {
                stage += 1;
            }
        }
        

        PlayerPrefs.SetInt("level", level);
        PlayerPrefs.SetInt("stage", stage);
        
        // 변동 정보 로컬에 저장
        DataContainer.instance.SaveLocalData_GameRecord();
    }
}
