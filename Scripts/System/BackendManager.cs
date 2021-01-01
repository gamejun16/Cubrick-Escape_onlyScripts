using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

using BackEnd;
using LitJson;

/**
 * 
 * 뒤끝과 연결되어 게임 내에서 백앤드와 관련된 처리들을 총괄하는 스크립트
 * 
 * ex) 로그인, 랭킹 조회, ...
 * 
 * */

public class BackendManager : MonoBehaviour
{
    static public BackendManager instance;

    [Header("TitleScene")]
    public TitleSceneManager tcm;
    

    // 재접속 최대 시도 카운트
    // 초과시 진행 불가, 재접속 유도
    int backendRetryCount = 5;

    #region Log variables

    float logTimer = 0f;
    IEnumerator logCoroutine;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(instance.gameObject);

#if UNITY_EDITOR

        // 준비 완료
        LogMonitor.instance.AddLog("Playform : UNITY_EDITOR");
        tcm.ReadyToStart();

#elif UNITY_ANDROID

        LogMonitor.instance.AddLog("Playform : UNITY_ANDROID");

        GPGSInit();
        BackendInit();
#endif

    }

    void GPGSInit()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration
    .Builder()
    .RequestServerAuthCode(false)
    .RequestIdToken()
    .Build();

        PlayGamesPlatform.InitializeInstance(config);

        PlayGamesPlatform.DebugLogEnabled = true;
        //GPGS 시작.
        PlayGamesPlatform.Activate();
    }

    void BackendInit()
    {
        // backend init
        Backend.Initialize(() =>
        {
            // 초기화 성공한 경우 실행
            if (Backend.IsInitialized)
            {
                // example
                // 버전 체크 -> 업데이트
                
                LogMonitor.instance.AddLog("Game server is initialized now... (1/3)");
                
                GPGSLogin();
            }

            // 초기화 실패한 경우 실행
            else
            {
                if (FailToConnectBackend("Backend Init is fail !!"))
                {
                    Invoke("BackendInit", 1f);
                }
            }
        });

    }
    
    public void GPGSLogin()
    {
        LogMonitor.instance.AddLog("GPGSLogin called()");

        // 이미 로그인 된 경우
        if (Social.localUser.authenticated == true)
        {
            LogMonitor.instance.AddLog("gpgs login info is exist (2/3)");
            Backend.BMember.AuthorizeFederation(GetTokens(), FederationType.Google, "gpgs", (callback) =>
            {
                // gpgs 로그인 성공 -> 뒤끝 로그인 성공
                if (callback.IsSuccess())
                {
                    LogMonitor.instance.AddLog("뒤끝 login is success (3/3)");


                    // 준비 완료
                    tcm.ReadyToStart();
                }

                // gpgs 로그인 성공 -> 뒤끝 로그인 실패
                else
                {
                    if (FailToConnectBackend("뒤끝 login is fail"
                    + $"\n[ERROR] Error : {callback}"
                    ))
                    {
                        // 재접속 시도
                        Invoke("GPGSLogin", 1);
                    }
                }
            });
        }
        else
        {
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    LogMonitor.instance.AddLog("gpgs login is success (2/3)");

                    // 로그인 성공 -> 뒤끝 서버에 획득한 구글 토큰으로 가입요청
                    Backend.BMember.AuthorizeFederation(GetTokens(), FederationType.Google, "gpgs", (callback)=>
                    {
                        // gpgs 로그인 성공 -> 뒤끝 로그인 성공
                        if (callback.IsSuccess())
                        {
                            LogMonitor.instance.AddLog("뒤끝 login is success (3/3)");
                            
                            // 준비 완료
                            tcm.ReadyToStart();
                        }
                        // gpgs 로그인 성공 -> 뒤끝 로그인 실패
                        else
                        {
                            if (FailToConnectBackend("뒤끝 login is fail"
                                + $"\n[ERROR] Error : {callback}"
                                ))
                            {
                                // 재접속 시도
                                Invoke("GPGSLogin", 1);
                            }
                        }

                    });
                }
                else
                {
                    // gpgs 로그인 실패
                    if (FailToConnectBackend("gpgs login is fail"))
                    {
                        // 재접속 시도
                        Invoke("GPGSLogin", 1);
                    }
                }
            });
        }
    }
    
    public void GPGSLogout()
    {
        ((PlayGamesPlatform)Social.Active).SignOut();
        LogMonitor.instance.AddLog("## GPGS Logout ##");
    }

    /// <summary>
    /// 구글 토큰 받아오기
    /// </summary>
    /// <returns>토큰을 문자열로 반환</returns>
    public string GetTokens()
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            // 유저 토큰 받기 첫번째 방법
            string _IDtoken = PlayGamesPlatform.Instance.GetIdToken();
            // 두번째 방법
            // string _IDtoken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();
            

            return _IDtoken;

        }
        else
        {
            Debug.Log("접속되어있지 않습니다. PlayGamesPlatform.Instance.localUser.authenticated :  fail");
            
            return null;
        }
    }

    /// <summary>
    /// 네트워크 접속 실패시 호출
    /// </summary>
    /// <returns>true: 재접속 진행, false: 재접속 불가능, 강제종료 유도</returns>
    bool FailToConnectBackend(string log)
    {
        LogMonitor.instance.AddLog(log);

        // 계정 삭제 버튼 활성화
        tcm.DeleteBtnOpen();

        backendRetryCount -= 1;
        if (backendRetryCount > 0)
        {
            LogMonitor.instance.AddLog($"재연결 시도중입니다.. 남은 횟수 : {backendRetryCount}");
            return true;
        }
        else
        {
            LogMonitor.instance.AddLog($"연결 실패. 재접속하세요.");
            return false;
        }
    }

    /// <summary>
    /// 서버로부터 스테이지 정보를 Load
    /// # 1. 서버에 정보가 있는 경우 -> inDate 추출, 저장 및 사용
    /// </summary>
    public void LoadGameRecordDataInit()
    {
        LogMonitor.instance.AddLog("LoadGameRecordData() is called ...");
        
        Backend.GameInfo.GetPrivateContents("gameRecord", (callback) =>
        {
            if (callback.IsSuccess())
            {
                LoadGameRecordData(callback);
            }
            else
            {
                LogMonitor.instance.AddLog("Fail to load stage data");
            }
        });
    }

    /// <summary>
    /// InitStageInfo() 비동기 접속 성공시 호출
    /// </summary>
    void LoadGameRecordData(BackendReturnObject bro)
    {
        var rows = bro.Rows();

        // # 1.
        if (rows.Count > 0)
        {
            LogMonitor.instance.AddLog("There are data");

            string jsonData = rows[0]["jsonData"]["S"].ToString();
            DataContainer.instance.SetGameRecordFromJson(jsonData);
            
            LogMonitor.instance.AddLog("data load done");
        }

        // # 2.
        else
        {
            LogMonitor.instance.AddLog("There are no data");

        }
    }


    /// <summary>
    /// GameRecord 정보를 뒤끝 db로 업로드
    /// </summary>
    public void SaveGameRecordData()
    {
        LogMonitor.instance.AddLog("SaveGameRecordData() is called ...");

        Param param = new Param();
        
        string jsonData = DataContainer.instance.GetGameRecordToJson();
        
        param.Add("jsonData", jsonData);

        //// lv1 ~ lv5 column 채우기.
        //for (int i = 1; i < PlayerPrefs.GetInt("level"); i++)
        //{
        //    // 각 레벨 column에 레벨별 기록한 시간을 저장
        //    float totalTime = DataContainer.instance.GetGameRecordStagesTotalTime(i);
        //    param.Add("lv" + i, (int)Mathf.Round(totalTime * 100));
        //}
        //// 아직 레벨스코어가 없다면(다 깨지 않아서) infinity로 초기화
        //for (int i = PlayerPrefs.GetInt("level"); i <= 5; i++)
        //{
        //    param.Add("lv" + (i).ToString(), 123456789);
        //}

        BackendReturnObject bro = Backend.GameInfo.GetPrivateContents("gameRecord");
        
        var rows = bro.Rows();
        
        // 데이터가 있는 경우. -> Update
        if (rows.Count > 0)
        {
            string indate = rows[0]["inDate"]["S"].ToString();
            
            Backend.GameInfo.Update("gameRecord", indate, param, (callback) =>
            {
                if (callback.IsSuccess())
                    LogMonitor.instance.AddLog("SaveGameRecordData() -> Update is success");
                else
                {
                    LogMonitor.instance.AddLog("SaveGameRecordData() -> Update is fail");
                    LogMonitor.instance.AddLog($"error : {callback}");
                }
            });
        }

        // 데이터가 없는 경우. -> Insert
        else
        {
            Backend.GameInfo.Insert("gameRecord", param, (callback) => {
                if (callback.IsSuccess())
                {
                    LogMonitor.instance.AddLog("SaveGameRecordData() -> Insert is success");
                    LogMonitor.instance.AddLog($"indate : {callback.GetInDate()}");
                }
                else
                {
                    LogMonitor.instance.AddLog("SaveGameRecordData() -> Insert is fail");
                    LogMonitor.instance.AddLog($"error : {callback}");
                }
            });
           
        }
    }


    /// <summary>
    /// 로컬에 저장된 게스트 계정 정보를 삭제
    /// </summary>
    public void DeleteGuestUserInfo()
    {
        Backend.BMember.DeleteGuestInfo();
        LogMonitor.instance.AddLog("## 계정 정보가 삭제되었습니다 !! ##");
        Application.Quit();
    }

#region unuse

    /// <summary>
    /// 뒤끝 비동기 게스트 로그인 함수
    /// </summary>
    //void AGuestLogin()
    //{
    //    //Debug.Log("-------------A GuestLogin-------------");

    //    // login
    //    Backend.BMember.GuestLogin((callback) =>
    //    {

    //        //Debug.Log(callback);

    //        // 로그인 성공
    //        if (callback.IsSuccess())
    //        {
    //            LogMonitor.instance.AddLog("Login is success. Wait for load stage info... (2/2)");

    //            // 준비 완료
    //            tcm.ReadyToStart();

    //        }
    //        else
    //        {
    //            if (FailToConnectBackend("Backend Init is fail !!"
    //                + $"\n[ERROR] Error : {callback}"
    //                + $"\n[ERROR] Error Code : {callback.GetErrorCode()}"
    //                + $"\n[ERROR] Message : {callback.GetMessage()}"
    //                ))
    //            {
    //                // 재접속 시도
    //                Invoke("AGuestLogin", 1);
    //            }

    //            //print($"[DEV] 로그인 실패");
    //            //LogMonitor.instance.AddLog("[DEV] Login is fail !!");
    //            //LogMonitor.instance.AddLog($"[ERROR] Error : {callback}");
    //            //LogMonitor.instance.AddLog($"[ERROR] Error Code : {callback.GetErrorCode()}");
    //            //LogMonitor.instance.AddLog($"[ERROR] Message : {callback.GetMessage()}");
    //            //LogMonitor.instance.AddLog($"***** Try Login again *****");
    //        }

    //    });
    //}


    /// <summary>
    /// 
    ///         tmp
    ///         구글 해시 키 얻기 위한 함수
    ///         
    /// </summary>
    //public void GetGoogleHash()
    //{
    //    inputField.text = Backend.Utils.GetGoogleHash();
    //}

    //[ContextMenu("Data save")]
    //void StageSave()
    //{
    //    if(inDate != null)
    //    {

    //    }
    //    else if (inDate == null)
    //    {

    //        int curLevel = Random.Range(0, 10);
    //        int curStage = Random.Range(100, 110);

    //        Param param = new Param();

    //        param.Add("level", curLevel);
    //        param.Add("stage", curStage);

    //        BackendReturnObject bro = Backend.GameInfo.Insert("stage", param);

    //        if (bro.IsSuccess())
    //        {
    //            print($"[DEV] 데이터 저장 성공");
    //        }
    //        else
    //        {
    //            print($"[DEV] 데이터 저장 실패");
    //        }
    //    }
    //}


    //[ContextMenu("Stage load")]
    //void StageLoad()
    //{
    //    //print($"[DEV] level : {Backend.GameInfo.GetPrivateContents("stage", "level")}");
    //    BackendReturnObject bro = Backend.GameInfo.GetPrivateContents("stage");



    //    var rows = bro.Rows();

    //    JsonData data = rows[0];
    //    if (data.Keys.Contains("level"))
    //    {
    //        //var output = data["level"]["N"];
    //        print($"[DEV] level : {data["level"]["N"]}");
    //        print($"[DEV] stage : {data["stage"]["N"]}");

    //        print($"[DEV] indate : {data["inDate"]["S"]}");
    //        inDate = data["inDate"]["S"].ToString();
    //    }
    //}

#endregion
}