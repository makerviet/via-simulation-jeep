using System;
using System.Collections;
using System.Collections.Generic;
using Koi.Playfab;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using static MapDataLoader;

public class PlayfabDataLoader : MonoBehaviour
{
    public enum LoadDataResult
    {
        R0_NoInternet = 0,
        R1_LoadFailed = 1,
        R2_Success = 2
    }

    private const string DEFAULT_MAP_MASTER_DATA = "default_map_master_data";
    private const string USER_MAP_DATA = "user_map_data";

    static PlayfabDataLoader Instance;


    [SerializeField] PlayfabTitleDataLoader defaultMapDataLoader;
    [SerializeField] PlayfabUserDataLoader userMapDataLoader;
    [SerializeField] PlayfabUserDataPoster userMapDataPoster;

    [Header("Log / Debug")]
    [SerializeField] MapAllData _defaultMapDatas;
    [SerializeField] MapAllData _userMapDatas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }


    void Start()
    {
        InitListener();
    }

    void InitListener()
    {
        PlayfabLogin.Instance.AddLoginSuccessListner(OnLoginSuccess);
    }

    void OnLoginSuccess()
    {
        DoLoadLevelMasterData();
        
    }

    void DoLoadLevelMasterData()
    {
        defaultMapDataLoader.StartLoadTitleData(DEFAULT_MAP_MASTER_DATA, (json) =>
        {
            Debug.LogError("Load Master Data success! " + json);
            _defaultMapDatas = JsonUtility.FromJson<MapAllData>(json);
            MapDataLoader.Instance.OnServerDefaultMapLoaded(_defaultMapDatas);
        }, () =>
        {
            Debug.LogError("Load Master Data Error");
            StartCoroutine(Delay(3.0f, DoLoadLevelMasterData));
        });
    }


    void DoLoadUserData()
    {
        userMapDataLoader.StartLoadUserData(USER_MAP_DATA, (json) =>
        {
            Debug.LogError("Load User map data success! " + json);
            _userMapDatas = JsonUtility.FromJson<MapAllData>(json);
            MapDataLoader.Instance.OnUserMapLoaded(_userMapDatas);
        }, () =>
        {
            Debug.LogError("Load User map Data Error");
            StartCoroutine(Delay(3.0f, DoLoadUserData));
        });
    }


    public static void PostUserMapData(MapAllData pData)
    {
        Instance.DoPostUserMapData(pData);
    }

    void DoPostUserMapData(MapAllData pData)
    {
        string json = JsonUtility.ToJson(pData);
        userMapDataPoster.StartPostUserData(USER_MAP_DATA, json, () => { }, () =>
        {
            //StartCoroutine(Delay(3.0f, () =>
            //{
            //    DoPostUserMapData(pData);
            //}));
        });
    }



    IEnumerator Delay(float duration, Action callback)
    {
        yield return new WaitForSeconds(duration);
        callback?.Invoke();
    }



    









    [ContextMenu("Log Challenge Master Data")]
    void LogChallengeMasterData()
    {
    }
}
