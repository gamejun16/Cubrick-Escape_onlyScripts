using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 * 게임 플레이 중 저장 및 관리가 필요한 데이터들을 보관하는 스크립트
 * 
 * */

static public class BrickDataContainer
{
    /// <summary>
    /// 데이터 초기화
    /// </summary>
    static public void ResetAllIngameData()
    {
        playerStep = 0;
        
        curLeftSwitch = 0;
    }

    #region step

    // 플레이어의 모든 스텝
    // 벽에 막혀 이동하지 못한 경우도 카운트한다.
    // 랭킹 기록에 이용된다.
    static int playerStep;

    // 현재 시작된 스테이지의 최고 점수
    static int curStageBestStep;

    /// <summary>
    /// 매 이동마다 호출 및 스탭 기록
    /// </summary>
    static public void Step()
    {
        playerStep += 1;
    }
    static public int GetStep() => playerStep;
    
    static public void SetCurStageBestStep(int bestStep) => curStageBestStep = bestStep;
    static public int GetCurStageBestStep() => curStageBestStep;

    #endregion

    #region brick[007] switch brick

    // 스테이지에 남아 있는 스위치의 개수
    // 0이 되어야 Finish brick이 활성화된다.
    static int curLeftSwitch;

    /// <summary>
    /// start()에서 각 switch brick들이 호출. 스위치 개수 초기화
    /// </summary>
    /// <returns> true : Finish brick 비활성화. </returns>
    static public bool SwitchAdd()
    {
        curLeftSwitch++;
        Debug.Log("curLeftSwitch : " + curLeftSwitch);

        if (curLeftSwitch == 1)
            return true;

        else
            return false;
    }

    /// <summary>
    /// switch brick 활성화시 호출. 0이 되면 모든 스위치가 On인 것 -> Finish brick 활성화
    /// </summary>
    /// <returns> true : Finish brick 활성화.</returns>
    static public bool SwitchOn()
    {
        curLeftSwitch--;
        Debug.Log("curLeftSwitch : " + curLeftSwitch);

        if (curLeftSwitch == 0)
            return true;

        else
            return false;
    }


    /// <summary>
    /// switch brick이 다시 비활성화 되었을 때 호출. 모두 활성화 해야 Finish brick이 활성화된다.
    /// </summary>
    /// <returns> true : Finish brick 비활성화 </returns>
    static public bool SwitchOff()
    {
        curLeftSwitch++;
        Debug.Log("curLeftSwitch : " + curLeftSwitch);

        if (curLeftSwitch == 1)
            return true;

        else
            return false;
    }

    #endregion

    
}

