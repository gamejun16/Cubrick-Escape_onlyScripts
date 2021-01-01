using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TouchPanelManager : MonoBehaviour
{
    public Transform twoHand;
    //public Animation[] twoHandBtnAnims;

    [Space]
    public Transform oneHand;
    public Animation[] oneHandBtnAnims; // 상하좌우 버튼 애니메이션 캐싱


    Playmode playmode; // 양 손 / 한 손 조작 모드

    private void Start()
    {
        DataContainer.instance.GetSettingInfo(out playmode);

        switch (playmode)
        {
            case Playmode.oneHandMode:
                twoHand.gameObject.SetActive(false);
                oneHand.gameObject.SetActive(true);
                break;

            case Playmode.twoHandMode:
                twoHand.gameObject.SetActive(true);
                oneHand.gameObject.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// 터치 패널 입력 정보를 플레이어에게 전달
    /// 재귀 형식으로 동작
    /// </summary>
    /// <param name="input"> 0: 전, 1: 후, 2: 좌, 3: 후</param>
    public void MovementByTouch(int input)
    {
        PlayerMover.instance.MovementByTouch(input);

        switch (playmode)
        {
            case Playmode.oneHandMode:
                oneHandBtnAnims[input].Play("MoveButtonAnim");
                break;
            case Playmode.twoHandMode:
                
                break;
        }
    }

}
