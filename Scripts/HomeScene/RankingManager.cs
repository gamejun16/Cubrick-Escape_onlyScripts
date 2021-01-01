using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;

using UnityEngine.UI;
using System.Text;
using LitJson;

public class RankingManager : MonoBehaviour
{
    public GameObject enrollTab, rankingTab;

    [Header("EnrollTab")]
    public Text[] levelTxts;
    public Text[] stepTxts;
    public Text[] timeTxts;
    public Image[] upperFrame, lowerFrame;

    [Header("RankingTab")]
    public Transform[] levelRankings;
    

    #region enroll tab

    public void OpenEnrollTab()
    {
        enrollTab.SetActive(true);

        MobileDeviceSupporter.instance.AddToUICloser(CloseEnrollTab);

        // 스코어 정보 받아오기
        // 각 레벨의 스테이지가 모두 클리어 되어야 랭킹에 등록할 수 있다.

        // Unlock된 최고 레벨
        // 2라면 1레벨은 전부 클리어 된 것.
        int highLevel = PlayerPrefs.GetInt("level");

        StringBuilder sb = new StringBuilder();

        for (int lv=1;lv<highLevel; lv++)
        {
            // 테두리 밝게
            upperFrame[lv - 1].color = Color.yellow;
            lowerFrame[lv - 1].color = Color.yellow;

            // 글자 밝게
            levelTxts[lv - 1].color = Color.white;
            stepTxts[lv - 1].color = Color.white;
            timeTxts[lv - 1].color = Color.white;

            // 토탈 step, time 가져온 후 출력
            int totalStep = DataContainer.instance.GetGameRecordStagesTotalStep(lv);
            float totalTime = DataContainer.instance.GetGameRecordStagesTotalTime(lv);
            
            sb.Append(totalStep.ToString()).Append(" step");
            stepTxts[lv - 1].text = sb.ToString();
            sb.Clear();

            sb.AppendFormat("{0:F2}", totalTime.ToString()).Append(" sec");
            timeTxts[lv - 1].text = sb.ToString();
            sb.Clear();
        }

        // 사운드
        UIAudioManager.instance.AudioPlay(1);
    }

    public void CloseEnrollTab()
    {
        enrollTab.SetActive(false);

        MobileDeviceSupporter.instance.RemoveFromUICloser(CloseEnrollTab);

        // 사운드
        UIAudioManager.instance.AudioPlay(1);
    }

    /// <summary>
    /// 로컬 데이터를 서버와 동기화하는 함수
    /// 동기화한 후 Enroll tab을 close하고 Ranking tab을 open한다
    /// </summary>
    public void EnrollDataToServer()
    {
        BackendReturnObject bro;
                
        bro = Backend.GameSchemaInfo.Get("timeRanking", new Where(), 1);
        JsonData rows;

        // 데이터가 없다면 init 데이터 생성 및 진행
        if (!bro.IsSuccess())
        {
            Backend.GameSchemaInfo.Insert("timeRanking");
            bro = Backend.GameSchemaInfo.Get("timeRanking", new Where(), 1);
        }
        
        rows = bro.Rows();
        
        string rankTableIndate = rows[0]["inDate"]["S"].ToString(); // its different with user-indate
        List<int> myScores = new List<int>();

        // 스테이지를 다 클리어 한 레벨에 대해서만 갱신 진행.
        for (int i=1;i<PlayerPrefs.GetInt("level"); i++)
        {   
            // total time 자료형은 float.
            // UpdateRTRankTable 에서는 int 자료형만 Update 되므로 float->int 형변환 과정을 거침
            float totalTime = DataContainer.instance.GetGameRecordStagesTotalTime(i);
            int totalTimeFloatToInt = (int)Mathf.Round(totalTime * 100);
            myScores.Add(totalTimeFloatToInt);

            Backend.GameSchemaInfo.UpdateRTRankTable("timeRanking", "lv" + i, totalTimeFloatToInt, rankTableIndate);
        }
        
        // load rank data
        // Level1RankData uuid, etc.
        string[] uuid =
        {
            "8f752f60-09e3-11eb-bd5a-2d114656860e", // lv1 
            "23c21ad0-0a10-11eb-ad47-3f256a7ec4da", // lv2
            "2e28ea80-0a10-11eb-bd5a-2d114656860e", // lv3
            "34981a30-0a10-11eb-ad47-3f256a7ec4da", // lv4
            "3b0af360-0a10-11eb-bd5a-2d114656860e" // lv5
        };
        
        //for (int i = 0; i < uuid.Length; i++)
        for (int i = 0; i < 5; i++)
        {
            bro = Backend.RTRank.GetRTRankByUuid(uuid[i]);

            JsonData otherRows = null, myRows = null;

            if (bro.IsSuccess())
            {
                LogMonitor.instance.AddLog($"Get Rank success");

                otherRows = bro.Rows();

                LogMonitor.instance.AddLog($"[lv{i+1}]. [{otherRows.Count}]명 랭킹 데이터 존재");
            }
            else
            {
                LogMonitor.instance.AddLog($"Get lv{i+1} Rank fail");
            }


            if(i >= myScores.Count)
            {
                InitRankingTabDatas(i, otherRows, myRows);
                continue;
            }
            
            bro = Backend.RTRank.GetRTRankByScore(uuid[i], myScores[i]);

            if (bro.IsSuccess())
            {
                LogMonitor.instance.AddLog($"Get My Rank Success, rows count : {bro.Rows().Count}");

                myRows = bro.Rows();
            }
            else
            {
                LogMonitor.instance.AddLog($"Get lv{i + 1} My Rank fail : {bro}");
            }
            

            InitRankingTabDatas(i, otherRows, myRows);

        }

        CloseEnrollTab();
        OpenRankingTab();
    }

    /// <summary>
    /// EnrollTab에서 읽어들인 데이터들을 RankingTab에 표시하는 함수
    /// </summary>
    void InitRankingTabDatas(int idx, JsonData otherRows, JsonData myRows)
    {
        StringBuilder sb = new StringBuilder();

        Transform otherScore = levelRankings[idx].GetChild(1);
        Transform myScore = levelRankings[idx].GetChild(2);

        // otherScore
        List<Transform> scores = new List<Transform>();

        // 1~10위까지 표시.
        // 기록 표시 Text 추출 및 기록 갱신
        for (int j = 0; j < otherRows.Count; j++)
        {
            Text txt = otherScore.GetChild(j).GetChild(1).GetChild(2).GetComponent<Text>();
            var v = otherRows[j]["score"]["N"];
            float time = int.Parse(v.ToString()) * 0.01f;

            sb.Clear();
            sb.Append(time.ToString()).Append("<size=30>sec</size>");
            txt.text = sb.ToString();
        }


        // myScore
        Text myRankTxt = myScore.GetChild(0).GetChild(2).GetComponent<Text>();
        Text myScoreTxt = myScore.GetChild(1).GetChild(2).GetComponent<Text>();


        // my ranking 데이터가 없는 경우.
        if (myRows == null) return;

        sb.Clear();
        sb.Append(myRows[0]["rank"]["N"].ToString());
        myRankTxt.text = sb.ToString();

        sb.Clear();        
        sb.Append((int.Parse(myRows[0]["score"]["N"].ToString()) * 0.01f).ToString()).Append("<size=30>sec</size>");
        myScoreTxt.text = sb.ToString();

    }
    
    #endregion
    
    #region ranking tab

    public void OpenRankingTab()
    {
        rankingTab.SetActive(true);

        MobileDeviceSupporter.instance.AddToUICloser(CloseRankingTab);

        // 사운드
        UIAudioManager.instance.AudioPlay(1);
    }
    
    public void CloseRankingTab()
    {
        rankingTab.SetActive(false);

        MobileDeviceSupporter.instance.RemoveFromUICloser(CloseRankingTab);

        // 사운드
        UIAudioManager.instance.AudioPlay(1);
    }

    #endregion

}
