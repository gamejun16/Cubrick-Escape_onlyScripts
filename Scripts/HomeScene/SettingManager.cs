using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public GameObject settingTab;

    [Header("PlayMode Tab")]
    public Transform PlayMode;
    [Header("Sound Tab")]
    public Transform Sound;
    public Slider soundSlider;
    [Header("Fov Tab")]
    public Transform Fov;
    public Text fovValue;
    [Header("PlayMode Tab")]
    public Transform CamFixMode;

    //[Header("SaveAndLoad Tab")]
    //public Transform textsRoot;
    //public Transform saveWarningText;
    //public Transform loadWarningText;
    //public Text moreClickCountText;
    //int settingDataBtnCount; // save / load를 수행하기 위해서는 3 번의 버튼 입력이 필요.

    [Header("PlayerInfo Tab")]
    public Text playerDesc;


    public void OpenSettingTab()
    {
        InitSettingTab();

        MobileDeviceSupporter.instance.AddToUICloser(CloseSettingTab);

        //saveWarningText.gameObject.SetActive(false);
        //loadWarningText.gameObject.SetActive(false);
        //textsRoot.gameObject.SetActive(false);
        //settingDataBtnCount = 3;

        settingTab.SetActive(true);
        
        // 사운드
        UIAudioManager.instance.AudioPlay(1);
    }


    /// <summary>
    /// 변동 사항 저장 및 탭 닫기
    /// </summary>
    public void CloseSettingTab()
    {
        // 변동 사항을 로컬에 저장
        DataContainer.instance.SaveLocalData_SettingInfo();

        MobileDeviceSupporter.instance.RemoveFromUICloser(CloseSettingTab);

        settingTab.SetActive(false);

        // 사운드
        UIAudioManager.instance.AudioPlay(1);
    }

    /// <summary>
    /// 셋팅 탭 활성화시 각 버튼 및 슬라이드 등이 기존 저장된 값을 가리키도록 초기화
    /// </summary>
    public void InitSettingTab()
    {
        Playmode pm;
        CamFixmode cfm;
        float sound;
        int fov;

        DataContainer.instance.GetSettingInfo(out pm);
        DataContainer.instance.GetSettingInfo(out cfm);
        DataContainer.instance.GetSettingInfo(out sound);
        DataContainer.instance.GetSettingInfo(out fov);

        Transform both = PlayMode.GetChild(3);
        int idx = (int)pm;
        if (idx == 0)
            both.GetChild(1).GetChild(0).gameObject.SetActive(false);
        else
            both.GetChild(0).GetChild(0).gameObject.SetActive(false);
        both.GetChild(idx).GetChild(0).gameObject.SetActive(true);

        
        both = CamFixMode.GetChild(3);
        idx = (int)cfm;
        if (idx == 0)
            both.GetChild(1).GetChild(0).gameObject.SetActive(false);
        else
            both.GetChild(0).GetChild(0).gameObject.SetActive(false);
        both.GetChild(idx).GetChild(0).gameObject.SetActive(true);

        soundSlider.value = sound;

        fovValue.text = fov.ToString();

    }


    #region PlayMode

    /// <summary>
    /// 플레이모드 각 버튼 선택시 호출되는 함수
    /// </summary>
    /// <param name="idx">0: One Hand Mode, 1: Two Hands Mode</param>
    public void ClickPlayMode(int idx)
    {
        print($"[DEV] ClickPlayMode({idx}) is called");

        Transform both = PlayMode.GetChild(3);

        // 반대쪽 버튼 하이라이팅 비활성화
        if (idx == 0)
            both.GetChild(1).GetChild(0).gameObject.SetActive(false);
        else
            both.GetChild(0).GetChild(0).gameObject.SetActive(false);

        // 누른 쪽 버튼 하이라이팅 활성화
        both.GetChild(idx).GetChild(0).gameObject.SetActive(true);

        // 변동 사항 적용
        DataContainer.instance.SetSettingInfo((Playmode)idx);
    }

    #endregion

    #region Sound

    /// <summary>
    /// 사운드 각 버튼 선택시 호출되는 함수(bgm, effect)
    /// 각 탭의 현재 볼륨에 맞게 슬라이딩 바가 변경된다.
    /// </summary>
    /// <param name="idx">0: bgm, 1: effect </param>
    public void ClickVolumeType(int idx)
    {
        Transform both = Sound.GetChild(3).GetChild(0);

        // 반대쪽 버튼 하이라이팅 비활성화
        if(idx == 0)
            both.GetChild(1).GetChild(0).gameObject.SetActive(false);
        else
            both.GetChild(0).GetChild(0).gameObject.SetActive(false);

        both.GetChild(idx).GetChild(0).gameObject.SetActive(true);
    }
    
    public void GetSliderValue()
    {
        print($"slider value : {soundSlider.value}");

        //UIAudioManager.instance.audioMixer.SetFloat("EFF", soundSlider.value);
        UIAudioManager.instance.SetMixerVolume("EFF", soundSlider.value);

        // 변동 사항 적용
        DataContainer.instance.SetSettingInfo(soundSlider.value);
    }

    

    #endregion

    #region FoV

    /// <summary>
    /// FoV 5씩 증가
    /// Clamp(40, 70)
    /// </summary>
    public void IncreaseFov()
    {
        int i = int.Parse(fovValue.text);

        i = Mathf.Clamp(i + 5, 40, 70);
        fovValue.text = i.ToString();

        // 변동 사항 적용
        DataContainer.instance.SetSettingInfo(i);
    }

    /// <summary>
    /// FoV 5씩 감소
    /// Clamp(40, 70)
    /// </summary>
    public void DecreaseFov()
    {
        int i = int.Parse(fovValue.text);

        i = Mathf.Clamp(i - 5, 40, 70);
        fovValue.text = i.ToString();

        // 변동 사항 적용
        DataContainer.instance.SetSettingInfo(i);
    }

    #endregion

    #region CamFixMode

    /// <summary>
    /// 카메라 고정 위치 설정을 위한 버튼 클릭시 호출
    /// </summary>
    /// <param name="idx">0: Center Fix, 1: Player Fix </param>
    public void ClickCamFixMode(int idx)
    {
        Transform both = CamFixMode.GetChild(3);

        // 반대쪽 버튼 하이라이팅 비활성화
        if (idx == 0)
            both.GetChild(1).GetChild(0).gameObject.SetActive(false);
        else
            both.GetChild(0).GetChild(0).gameObject.SetActive(false);

        // 누른 쪽 버튼 하이라이팅 활성화
        both.GetChild(idx).GetChild(0).gameObject.SetActive(true);

        // 변동 사항 적용
        DataContainer.instance.SetSettingInfo((CamFixmode)idx);
    }

    #endregion

    #region Data
    
    //public void SaveGameRecordDataToServer()
    //{
    //    if(settingDataBtnCount > 1)
    //    {
    //        settingDataBtnCount -= 1;
    //        textsRoot.gameObject.SetActive(true);
    //        saveWarningText.gameObject.SetActive(true);
    //        loadWarningText.gameObject.SetActive(false);

    //        moreClickCountText.text = settingDataBtnCount.ToString();
    //        return;
    //    }

    //    CloseSettingTab();
    //}

    //public void LoadGameRecordDataFromServer()
    //{
    //    if (settingDataBtnCount > 1)
    //    {
    //        settingDataBtnCount -= 1;
    //        textsRoot.gameObject.SetActive(true);
    //        saveWarningText.gameObject.SetActive(false);
    //        loadWarningText.gameObject.SetActive(true);

    //        moreClickCountText.text = settingDataBtnCount.ToString();
    //        return;
    //    }

    //    CloseSettingTab();
    //}

    #endregion

    #region PlayerInfo

    public void DescPlayerInfo(string id)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("user id : " + id + '\n');

        playerDesc.text = sb.ToString();
    }

    #endregion

    #region DevInfo

    // ?

    #endregion


}
