using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

// 조작 모드 설정
public enum Playmode { oneHandMode, twoHandMode, };

// 카메라 고정 지점 설정
public enum CamFixmode { stageCenterFix, playerCenterFix, };


[System.Serializable]
class Level
{
    public int[] stageStep; // 각 스테이지 스코어

    public int levelStep; // 본 level의 모든 스테이지 스코어 누계

    // constructor
    public Level()
    {
        stageStep = new int[9];

        for (int i = 0; i < stageStep.Length; i++)
        {
            stageStep[i] = int.MaxValue;
        }
        
        levelStep = 0;
    }
    
    /// <summary>
    /// 해당 레벨 모든 스테이지의 스코어(레벨 스코어) 누계 산출 및 저장
    /// </summary>
    public void UpdateLevelScore()
    {
        levelStep = 0;

        for(int i=0;i<stageStep.Length; i++)
        {
            if (stageStep[i] != int.MaxValue)
            {
                levelStep += stageStep[i];
            }
        }
    }


    public int GetLevelStep() => levelStep;
}

[System.Serializable]
class GameRecord
{
    public Level[] level;
    
    // unlock된 최상위 레벨 및 스테이지
    // ex) 2-4 까지 클리어 및 2-5 진행 가능한 경우, topLevel : 2,   topStage : 5.
    public int topLevel, topStage; 
    
    // constructor
    public GameRecord()
    {
        level = new Level[5];

        for (int i = 0; i < level.Length; i++)
        {
            level[i] = new Level();
        }

        // 1레벨 1스테이지 unlock
        topLevel = 1;
        topStage = 1;
    }

    
}

[System.Serializable]
class SettingInfo
{
    public Playmode playmode;

    public CamFixmode camFixmode;

    public float sound; // Clamp -20 ~ 20 (default: 0)
    public int fov; // Clamp 40 ~ 70 (default: 60)


    /// <summary>
    /// default setting 
    /// </summary>
    public SettingInfo()
    {
        playmode = Playmode.oneHandMode;
        camFixmode = CamFixmode.playerCenterFix;
        sound = 0f;
        fov = 50;
    }

    #region public methods

    public void SetPlaymode(Playmode mode)
    {
        playmode = mode;
    }
    public Playmode GetPlaymode() => playmode;

    public void SetCamFixmode(CamFixmode mode)
    {
        camFixmode = mode;
    }
    public CamFixmode GetCamFixmode() => camFixmode;

    public void SetSound(float s)
    {
        sound = Mathf.Clamp(s, -20, 20);

    }
    public float GetSound() => sound;

    public void SetFov(int f)
    {
        fov = Mathf.Clamp(f, 40, 70);
    }
    public int GetFov() => fov;

    #endregion
}

public class DataContainer : MonoBehaviour
{
    static public DataContainer instance;

    [SerializeField]
    SettingInfo settingInfo; // 설정 정보

    [SerializeField]
    GameRecord gameRecord; // 성적 정보

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        

        LoadLocalData_SettingInfo();
        LoadLocalData_GameRecord();
    }

    #region init methods

    /// <summary>
    /// 설정 정보 초기화
    /// </summary>
    void InitSettingInfo()
    {
        settingInfo = new SettingInfo();
    }

    /// <summary>
    /// 기록 초기화
    /// </summary>
    void InitGameRecord()
    {
        gameRecord = new GameRecord();
    }    
    
    /// <summary>
    /// SettingInfo 정보를 로컬 기기에서 Load
    /// </summary>
    void LoadLocalData_SettingInfo()
    {
        try
        {
            string jsonData, tmpPath, path;

            tmpPath = "LocalData/settingInfo.json";


#if UNITY_EDITOR
            path = Path.Combine(Application.streamingAssetsPath, tmpPath);

            if(!Directory.Exists(Application.streamingAssetsPath + "/LocalData"))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath + "/LocalData");
            }
#elif UNITY_ANDROID
            path = Path.Combine(Application.persistentDataPath, tmpPath);

            if(!Directory.Exists(Application.persistentDataPath + "/LocalData"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/LocalData");
            }
#endif
            
            // 로컬 데이터의 존재 여부 확인
            if (!File.Exists(path))
            {
                InitSettingInfo();

                jsonData = JsonUtility.ToJson(settingInfo, true);
                File.WriteAllText(path, jsonData);

                LogMonitor.instance.AddLog("There are no data !!");

                return;
            }

            LogMonitor.instance.AddLog("Find local data !!");


//#if UNITY_EDITOR

            jsonData = File.ReadAllText(path);

//#elif UNITY_ANDROID
        
//        WWW reader = new WWW(path);
//        while(!reader.isDone);

//        jsonData = reader.text;
        
//#endif

            settingInfo = JsonUtility.FromJson<SettingInfo>(jsonData);
        }
        catch(System.Exception e)
        {
            LogMonitor.instance.AddLog("Error occur on LoadLocalData_SettingInfo");
            LogMonitor.instance.AddLog(e.Message);
        }
    }

    /// <summary>
    /// GameRecord 정보를 로컬 기기에서 Load
    /// </summary>
    void LoadLocalData_GameRecord()
    {
        try
        {
            string jsonData, tmpPath, path;

            tmpPath = "LocalData/gameRecord.json";

#if UNITY_EDITOR
            path = Path.Combine(Application.streamingAssetsPath, tmpPath);

#elif UNITY_ANDROID
        path = Path.Combine(Application.persistentDataPath, tmpPath);
#endif

            // 로컬 데이터의 존재 여부 확인
            // 존재하지 않는다면 GameRecord 정보 및 로컬파일 생성
            if (!File.Exists(path))
            {
                InitGameRecord();

                jsonData = JsonUtility.ToJson(gameRecord, true);
                File.WriteAllText(path, jsonData);
                
                PlayerPrefs.SetInt("level", gameRecord.topLevel);
                PlayerPrefs.SetInt("stage", gameRecord.topStage);
                
                return;
            }
    
            jsonData = File.ReadAllText(path);
            
            gameRecord = JsonUtility.FromJson<GameRecord>(jsonData);

            if (gameRecord == null)
                LogMonitor.instance.AddLog($"gameRecord is null");
            
            PlayerPrefs.SetInt("level", gameRecord.topLevel);
            PlayerPrefs.SetInt("stage", gameRecord.topStage);

        }
        catch(System.Exception e)
        {
            LogMonitor.instance.AddLog("Error occur on LoadLocalData_GameRecord");
            LogMonitor.instance.AddLog(e.Message);
        }
    }

#endregion


#region public methods

    /// <summary>
    /// gameRecord 정보를 로컬에 일괄 저장합니다.
    /// </summary>
    public void SaveLocalData_GameRecord()
    {
        try
        {
            gameRecord.topLevel = PlayerPrefs.GetInt("level");
            gameRecord.topStage = PlayerPrefs.GetInt("stage");


            string jsonData, tmpPath, path;

            tmpPath = "LocalData/gameRecord.json";

#if UNITY_EDITOR
            path = Path.Combine(Application.streamingAssetsPath, tmpPath);

#elif UNITY_ANDROID
        path = Path.Combine(Application.persistentDataPath, tmpPath);
#endif


            jsonData = JsonUtility.ToJson(gameRecord, true);

            File.WriteAllText(path, jsonData);
        }
        catch(System.Exception e)
        {
            LogMonitor.instance.AddLog("Error occur on SaveLocalData_GameRecord");
            LogMonitor.instance.AddLog(e.Message);
        }
    }

    /// <summary>
    /// settingInfo 정보를 로컬에 일괄 저장합니다.
    /// </summary>
    public void SaveLocalData_SettingInfo()
    {
        try
        {
            string jsonData, tmpPath, path;

            tmpPath = "LocalData/settingInfo.json";

#if UNITY_EDITOR
            path = Path.Combine(Application.streamingAssetsPath, tmpPath);

#elif UNITY_ANDROID
        path = Path.Combine(Application.persistentDataPath, tmpPath);
#endif


            jsonData = JsonUtility.ToJson(settingInfo, true);

            File.WriteAllText(path, jsonData);
        }
        catch (System.Exception e)
        {
            LogMonitor.instance.AddLog("Error occur on SaveLocalData_SettingInfo");
            LogMonitor.instance.AddLog(e.Message);
        }
    }

    /// <summary>
    /// 해당 스테이지 스코어 갱신
    /// </summary>
    /// <param name="_level">레벨(1~5)</param>
    /// <param name="_stage">스테이지(1~9)</param>
    /// <param name="_step">점수(step)</param>
    public void UpdateScore(int _level, int _stage, int _step)
    {
        // 더 나은 점수(스탭)를 기록했다면 => 갱신
        if(gameRecord.level[_level - 1].stageStep[_stage - 1] > _step)
        {
            print($"스탭 스코어 갱신");
            gameRecord.level[_level - 1].stageStep[_stage - 1] = _step;
        }

        gameRecord.level[_level - 1].UpdateLevelScore();
    }
    
    /// <summary>
    /// gameRecord 중 특정 레벨의 스테이지별 step 기록을 리스트로 반환
    /// </summary>
    /// <param name="_level">기록을 얻고자 하는 레벨(1~5)</param>
    /// <returns>스테이지별 기록(List<int>)</returns>
    public List<int> GetGameRecordStagesStep(int _level)
    {
        _level -= 1; // value to idx

        List<int> res = new List<int>();

        for (int i = 0; i < 9; i++) {
            if(gameRecord.level[_level].stageStep[i] != int.MaxValue)
            {
                res.Add(gameRecord.level[_level].stageStep[i]);
            }
        }
        return res;
    }

    /// <summary>
    /// GameRecord 중 특정 레벨의 스테이지별 토탈 step 기록을 반환
    /// </summary>
    /// <param name="_level">기록을 얻고자 하는 레벨(1~5)</param>
    /// <returns>해당 레벨 스코어</returns>
    public int GetGameRecordStagesTotalStep(int _level) => gameRecord.level[_level - 1].GetLevelStep();
    public string GetGameRecordToJson() => JsonUtility.ToJson(gameRecord);
    public void SetGameRecordFromJson(string jsonData)
    {
        gameRecord = JsonUtility.FromJson<GameRecord>(jsonData);
        
        PlayerPrefs.SetInt("level", gameRecord.topLevel);
        PlayerPrefs.SetInt("stage", gameRecord.topStage);

        SaveLocalData_GameRecord();
    }


    public void SetSettingInfo(Playmode mode) => settingInfo.SetPlaymode(mode);
    public void SetSettingInfo(CamFixmode mode) => settingInfo.SetCamFixmode(mode);
    public void SetSettingInfo(float sound) => settingInfo.SetSound(sound);
    public void SetSettingInfo(int fov) => settingInfo.SetFov(fov);

    public void GetSettingInfo(out CamFixmode mode) => mode = settingInfo.GetCamFixmode();
    public void GetSettingInfo(out Playmode mode) => mode = settingInfo.GetPlaymode();
    public void GetSettingInfo(out float sound) => sound = settingInfo.GetSound();
    public void GetSettingInfo(out int fov) => fov = settingInfo.GetFov();
    

    #endregion

}
