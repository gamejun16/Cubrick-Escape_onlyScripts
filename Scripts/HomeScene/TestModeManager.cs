using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestModeManager : MonoBehaviour
{

    public InputField levelInput, stageInput;


    public void LoadTargetStage_CHEAT()
    {
        int level = int.Parse(levelInput.text);
        int stage = int.Parse(stageInput.text);

        LogMonitor.instance.AddLog($"## CHEAT CALLED ##");
        LogMonitor.instance.AddLog($"lv : {level}, st : {stage}");

        StageLoader.LoadStage(level, stage);
        LoadingManager.instance.LoadScene("IngameScene");
    }
}
