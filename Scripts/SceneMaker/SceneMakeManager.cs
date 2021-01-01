using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using UnityEngine.UI;

public class SceneMakeManager : MonoBehaviour
{
    // 3 floor
    public StageInfo[] stageInfos;

    [Header("Elements")]
    public Transform bricksRoot;
    [Tooltip("top-mid-bot")] public Transform[] brickDetectors;
    public Text inputLevelText, inputStageText;

    private void Start()
    {
        // 3 floor
        stageInfos = new StageInfo[3];
        stageInfos[0] = new StageInfo(0, 5);
        stageInfos[1] = new StageInfo(0, 5);
        stageInfos[2] = new StageInfo(0, 5);
        
    }

    public void SaveStageInfoToJson()
    {
        // 필수 brick 배치 여부 확인
        if (!SceneMakeUIManager.instance.IsSavePossible())
            return;
       

        int level = int.Parse(inputLevelText.text), stage = int.Parse(inputStageText.text);

        // === search bricks ===
        for (int floor = 0; floor < 3; floor++)
        {
            BrickSensor[] sensors = brickDetectors[floor]. GetComponentsInChildren<BrickSensor>();

            // map size
            for (int i = 0; i < 5 * 5; i++)
            {
                int r = i / 5, c = i % 5;
                if (sensors[i].target == null)
                {
                    stageInfos[floor].rows[r].bricks[c] = -1;
                }
                else
                {
                    stageInfos[floor].rows[r].bricks[c] = int.Parse(sensors[i].target.GetComponent<BrickActionController>().brickId);
                }
            }
        }


        // === save stageInfo to json ===
        for (int floor = 0; floor < 3; floor++)
        {
            string jsonData = JsonUtility.ToJson(stageInfos[floor], true);
            string tmpPath = "Stages/" + level + "/" + stage + "/stageInfo_" + floor + ".json";

            //string path = Path.Combine(Application.dataPath, tmpPath);
            string path = Path.Combine(Application.streamingAssetsPath, tmpPath);
            File.WriteAllText(path, jsonData);
        }

        StageMakerNotice.instance.AsyncNoticer("스테이지를 저장합니다.", Color.green);
    }
    
    public void LoadStageInfoFromJson()
    {
        try
        {
            int level = int.Parse(inputLevelText.text), stage = int.Parse(inputStageText.text);

            for (int floor = 0; floor < 3; floor++)
            {

                string jsonData;
                string tmpPath = "Stages/" + level + "/" + stage + "/stageInfo_" + floor + ".json";

                //string path = Path.Combine(Application.dataPath, tmpPath);
                string path = Path.Combine(Application.streamingAssetsPath, tmpPath);
                jsonData = File.ReadAllText(path);
                stageInfos[floor] = JsonUtility.FromJson<StageInfo>(jsonData);
            }
        } catch(System.Exception e)
        {
            StageMakerNotice.instance.AsyncNoticer("스테이지를 불러올 수 없습니다.", Color.red);
            return;
        }

        SpawnBricks();

        StageMakerNotice.instance.AsyncNoticer("스테이지를 불러옵니다.", Color.green);
    }

    public void ClearStage()
    {
        // destroy all bricks
        int childCount = bricksRoot.childCount;
        for (int i = bricksRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(bricksRoot.GetChild(i).gameObject);
        }
        SceneMakeUIManager.instance.BrickCountReset();

        StageMakerNotice.instance.AsyncNoticer("스테이지를 초기화합니다.", Color.green);
    }

    void SpawnBricks()
    {
        // destroy all bricks
        int childCount = bricksRoot.childCount;
        for (int i=bricksRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(bricksRoot.GetChild(i).gameObject);
        }
        SceneMakeUIManager.instance.BrickCountReset();
        
        string bricksPath = "Prefabs/Bricks/";
        string targetBrickId = "";
        
        int instantiateCount = 0;
        for (int floor = 0; floor < 3; floor++)
        {
            for (int r = 0; r < stageInfos[floor].rows.Length; r++)
            {
                for (int c = 0; c < stageInfos[floor].rows[r].bricks.Length; c++)
                {
                    if (stageInfos[floor].rows[r].bricks[c] == -1) continue;

                    targetBrickId = "[" + string.Format("{0:D3}", stageInfos[floor].rows[r].bricks[c]) + "]Brick";

                    var obj = Resources.Load<GameObject>(bricksPath + targetBrickId);
                    GameObject brick = Instantiate(obj, bricksRoot);
                    instantiateCount++;

                    SceneMakeUIManager.instance.AddBrick(stageInfos[floor].rows[r].bricks[c]);

                    brick.transform.position = new Vector3(c - 2, 8 - 8 * floor, 2 - r);
                }
            }
        }

    }

    public void PlayView()
    {
        if (Camera.main.transform.eulerAngles.x < 46 && Camera.main.transform.eulerAngles.x > 44)
            return;

        Camera.main.transform.eulerAngles = new Vector3(0, 45, 0);
        Camera.main.transform.Translate(Vector3.forward * -5 + Vector3.up * 2);
        Camera.main.transform.eulerAngles = new Vector3(45, 45, 0);
    }

    public void EditorView()
    {
        if (Camera.main.transform.eulerAngles.x < 61 && Camera.main.transform.eulerAngles.x > 59)
            return;

        Camera.main.transform.eulerAngles = new Vector3(0, 45, 0);
        Camera.main.transform.Translate(Vector3.forward * 5 + Vector3.down * 2);
        Camera.main.transform.eulerAngles = new Vector3(60, 45, 0);
    }

    public void ElevateUp()
    {
        if(Camera.main.transform.position.y < 10)
        {
            Camera.main.transform.Translate(new Vector3(0, 8, 0), Space.World);
        }
    }
    public void ElevateDown()
    {
        if (Camera.main.transform.position.y > -0)
        {
            Camera.main.transform.Translate(new Vector3(0, -8, 0), Space.World);
        }
    }
}
