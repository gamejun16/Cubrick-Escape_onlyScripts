using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

/**
 * 
 * Json 동작 여부 확인용 테스트 스크립트
 * 
 * */

    
public class StageInfoContainer : MonoBehaviour
{
    public StageInfo stageInfo = new StageInfo(999,
        new StageRow[]
        {
            new StageRow(new int[]{-11, -11, -11 }),
            new StageRow(new int[]{22, 22, 22 })
        });
    
    [ContextMenu("SaveStageInfo")]
    void SaveStageInfoToJson()
    {
        string jsonData = JsonUtility.ToJson(stageInfo, true);
        string path = Path.Combine(Application.dataPath, "stageInfo.json");
        File.WriteAllText(path, jsonData);

        print($"[DEV] SaveStageInfo done");
    }

    [ContextMenu("LoadStageInfo")]
    void LoadStageInfoFromJson()
    {
        string jsonData;
        string path = Path.Combine(Application.dataPath, "MyJson.json");
        jsonData = File.ReadAllText(path);
        stageInfo = JsonUtility.FromJson<StageInfo>(jsonData);

        print($"[DEV] LoadStageInfo done");
    }

    
}
