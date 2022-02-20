using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
//using GooglePlayGames;
using UnityEngine.UI;
using Koi.Playfab;

public class TestPlayfabLogin : MonoBehaviour
{
    [SerializeField] InputField nameInput;
    [SerializeField] InputField transAccInput;

    [SerializeField] Text logText;

    [SerializeField] Button postNameBtn;
    [SerializeField] Button tranAccBtn;
    [SerializeField] Button loginByDeviceBtn;
    [SerializeField] Button loginByGoogleBtn;
    [SerializeField] Button linkByGoogleBtn;
    [SerializeField] Button getTransAccBtn;
    [SerializeField] Button getLeaderBoardBtn;

    public string playfabId;

    public void Start()
    {
        PlayFabSettings.TitleId = "A09E"; // Please change this value to your own titleId from PlayFab Game Manager
        InitListener();
    }

    void InitListener()
    {
        postNameBtn.onClick.AddListener(PostName);
        tranAccBtn.onClick.AddListener(TransAcc);
        loginByDeviceBtn.onClick.AddListener(LoginByDevice);
        loginByGoogleBtn.onClick.AddListener(LoginByGoogle);
        linkByGoogleBtn.onClick.AddListener(LinkGoogle);
        getTransAccBtn.onClick.AddListener(GetTransAcc);

        getLeaderBoardBtn.onClick.AddListener(GetLeaderboard);
    }

    void PostName()
    {
        UpdateUserTitleDisplayNameRequest request = new UpdateUserTitleDisplayNameRequest();
        request.DisplayName = nameInput.text;
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, (result)=>{
            Debug.LogError("TestPlayfabLogin: Update result " + JsonUtility.ToJson(result));
        }, (error)=>{
            Debug.LogError("TestPlayfabLogin: Update error " + JsonUtility.ToJson(error));
        });
    }

    void TransAcc()
    {
        // link with custom ID
        LinkCustomIDRequest request = new LinkCustomIDRequest();
//        System.DateTime dateTime = System.DateTime.Now;
        request.CustomId = "A_" + playfabId;
        request.ForceLink = true;
        PlayFabClientAPI.LinkCustomID(request, (result) =>
            {
                Debug.LogError("TestPlayfabLogin: link customID result " + JsonUtility.ToJson(result));
            }, (error) =>
            {
                Debug.LogError("TestPlayfabLogin: link customID error " + JsonUtility.ToJson(error));
            });
    }

    void GetTransAcc()
    {
        // login with customID
        string customId = transAccInput.text;
        LoginWithCustomIDRequest request = new LoginWithCustomIDRequest();
        request.CreateAccount = true;
        request.CustomId = customId;
        request.TitleId = PlayFabSettings.TitleId;
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    void GetLeaderboard()
    {
//        GetLeaderboardRequest request = new GetLeaderboardRequest();
//        request.StatisticName = "score";
//        request.MaxResultsCount = 20;
//        request.StartPosition = 0;
//        request.Version = 1;
////        request.ProfileConstraints = new PlayerProfileViewConstraints();
////        request.ProfileConstraints.
//        PlayFabClientAPI.GetLeaderboard(request, (result)=>{
//            Debug.LogError("LeaderboardResult: " + JsonMapper.ToJson(result));
//        }, (error)=>{
//            Debug.LogError("LeaderboardResult: error " + JsonMapper.ToJson(error));
//        });
        PlayfabLeaderboard.Instance.ShowDefaultLeaderboardUI();
    }

    void LoginByDevice()
    {
        //Note: Setting title Id here can be skipped if you have set the value in Editor Extensions already.
        //        var request = new LoginWithCustomIDRequest { CustomId = "GettingStartedGuide", CreateAccount = true };
        //        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
//        var request = new LoginWithAndroidDeviceIDRequest {
//            AndroidDeviceId = SystemInfo.deviceUniqueIdentifier,
//            CreateAccount = true
//        };
//
//        PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnLoginSuccess, OnLoginFailure);
    }


    void LoginByGoogle()
    {
        #if UNITY_ANDROID
//        var request = new LoginWithGoogleAccountRequest();
//        request.CreateAccount = true;
//        request.TitleId = PlayFabSettings.TitleId;
//        request.ServerAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
//
//        var serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
//        Debug.Log("Server Auth Code: " + serverAuthCode + "_ isAuthentcating = ");
//        if (string.IsNullOrEmpty(serverAuthCode))
//        {
//            Debug.LogError("TestPlayfabLogin: server auth code null empty. Id token= " + PlayGamesPlatform.Instance.GetIdToken());
//        }
//        else {
//            Debug.LogError("TestPlayfabLogin: server auth code:>_" + serverAuthCode + "_<");
//        }
//            
//        PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
//            {
//                TitleId = PlayFabSettings.TitleId,
//                ServerAuthCode = serverAuthCode,
//                CreateAccount = true
//            }, OnLoginSuccess, OnLoginFailure);
        #endif
    }

    void LinkGoogle()
    {
        #if UNITY_ANDROID
        // check exist
//        LinkGoogleAccountRequest linkRequest = new LinkGoogleAccountRequest();
//        linkRequest.ForceLink = false;
//        linkRequest.ServerAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
//        PlayFabClientAPI.LinkGoogleAccount(linkRequest, (result)=>{
//            logText.text = "Link Result: " + JsonMapper.ToJson(result);
//            Debug.LogError("TestPlayfabLogin: Link Result: " + JsonMapper.ToJson(result));
//        }, (error)=>{
//            logText.text = "Link Error: " + JsonMapper.ToJson(error);
//            Debug.LogError("TestPlayfabLogin: Link Error: " + JsonMapper.ToJson(error));
//
//            //           Error exist user: LinkedAccountAlreadyClaimed 1012
//        });
        UnlinkGoogleAccountRequest unlinkRequest = new UnlinkGoogleAccountRequest();
        PlayFabClientAPI.UnlinkGoogleAccount(unlinkRequest, (result)=>{
            logText.text = "Link Result: " + JsonUtility.ToJson(result);
            Debug.LogError("TestPlayfabLogin: UnLink Result: " + JsonUtility.ToJson(result));
        }, (error)=>{
            logText.text = "Link Result: " + JsonUtility.ToJson(error);
            Debug.LogError("TestPlayfabLogin: UnLink Result: " + JsonUtility.ToJson(error));
        });
        #endif
    }

    void DoLinkGoogle()
    {
        #if UNITY_ANDROID
//        LinkGoogleAccountRequest linkRequest = new LinkGoogleAccountRequest();
//        linkRequest.ForceLink = true;
//        linkRequest.ServerAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
//        PlayFabClientAPI.LinkGoogleAccount(linkRequest, (result)=>{
//            logText.text = "Link Result: " + JsonMapper.ToJson(result);
//            Debug.LogError("TestPlayfabLogin: Link Result: " + JsonMapper.ToJson(result));
//        }, (error)=>{
//            logText.text = "Link Error: " + JsonMapper.ToJson(error);
//            Debug.LogError("TestPlayfabLogin: Link Error: " + JsonMapper.ToJson(error));
//        });
        #endif
    }


    void OnLoginSuccess(LoginResult result)
    {
        playfabId = result.PlayFabId;
        Debug.LogError("TestPlayfabLogin: Congratulations, you made your first successful API call!");
        Debug.LogError("TestPlayfabLogin: Result: " + JsonUtility.ToJson(result));
        Debug.LogError("Session Ticket = " + result.SessionTicket);
//        GetUserData(result);
        GetAccountInfo();
    }

    void GetAccountInfo()
    {
        GetAccountInfoRequest request = new GetAccountInfoRequest();
        PlayFabClientAPI.GetAccountInfo(request, (result)=>{
            nameInput.text = result.AccountInfo.TitleInfo.DisplayName;
            Debug.LogError("TestPlayfabLogin: Get account info: " + JsonUtility.ToJson(result));
            logText.text = "playfabId = " + result.AccountInfo.PlayFabId + "\n" + JsonUtility.ToJson(result);
        }, (error)=>{
            Debug.LogError("TestPlayfabLogin: Get account info error: " + JsonUtility.ToJson(error));
        });
    }

    void GetUserData(LoginResult loginResult)
    {
        GetUserDataRequest request = new GetUserDataRequest();
        request.PlayFabId = loginResult.PlayFabId;
//        request.Keys = new List<string>();
//        request.Keys.Add("nick_name");
//        request.Keys.Add("player_rank");
//        request.Keys.Add("player_heart");
//        request.Keys.Add("read_only_test_1");


        PlayFabClientAPI.GetUserData(request, (userdata)=>{
            Debug.LogError("TestPlayfabLogin: Get userData: " + JsonUtility.ToJson(userdata));
            logText.text = "playfabId = " + loginResult.PlayFabId + "\n" + JsonUtility.ToJson(userdata);
        }, (error)=>{
            Debug.LogError("TestPlayfabLogin: Get userData error: " + JsonUtility.ToJson(error));
        });

    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("TestPlayfabLogin: Something went wrong with your first API call.  :(");
        Debug.LogError("TestPlayfabLogin: Here's some debug information:");
        Debug.LogError("TestPlayfabLogin: error: " + error.GenerateErrorReport());
//        logText.text = "Login Error " +  PlayGamesPlatform.Instance.GetServerAuthCode() + "\n" + JsonMapper.ToJson(error);
    }
}
