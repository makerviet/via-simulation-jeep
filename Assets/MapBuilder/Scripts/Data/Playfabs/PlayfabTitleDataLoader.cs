using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayfabTitleDataLoader : MonoBehaviour
{
    System.Action<Dictionary<string, string>> OnMultiLoadedListener;
    System.Action<string> OnLoadedListener;
    Action OnLoadFailedListener;

    [Header("Log / Debug")]
    [SerializeField] bool IsLoaded = false;
    [SerializeField] List<string> keys = new List<string>();

    int countLoadTitleData = 0;
    public void StartLoadTitleData(List<string> pKeys, System.Action<Dictionary<string, string>> callback, Action failedCallback)
    {
        this.keys = pKeys;
        
        OnLoadFailedListener = failedCallback;
        OnMultiLoadedListener = callback;
        OnLoadedListener = null;

        countLoadTitleData = 0;
        IsLoaded = false;
        LoadTitleData();
    }

    public void StartLoadTitleData(string pKey, Action<string> callback, Action failedCallback)
    {
        this.keys.Clear();
        keys.Add(pKey);

        OnLoadFailedListener = failedCallback;
        OnMultiLoadedListener = null;
        OnLoadedListener = callback;

        countLoadTitleData = 0;
        IsLoaded = false;
        LoadTitleData();
    }

    void LoadTitleData()
    {
        countLoadTitleData++;
        if (countLoadTitleData > 60)
        {
            OnLoadFailedListener?.Invoke();
            Debug.LogError("PlayfabTitleDataLoader: CANNOT load title data");
            return;
        }

        Debug.LogError("PlayfabTitleDataLoader: load level master data, key = " + keys[0]);
        GetTitleDataRequest request = new GetTitleDataRequest();
        request.Keys = new List<string>();
        foreach (var key in keys)
        {
            request.Keys.Add(key);
        }
        
        PlayFabClientAPI.GetTitleData(request, (result) =>
        {
            result.Request = null;
            try
            {
                var dict = result.Data;
                foreach (var key in keys)
                {
                    if (dict.ContainsKey(key))
                    {
                        string json = dict[key];
                        if (!IsLoaded && !string.IsNullOrEmpty(json))
                        {
                            IsLoaded = true;

                            if (OnMultiLoadedListener != null)
                            {
                                OnMultiLoadedListener.Invoke(dict);
                            }
                            else if (OnLoadedListener != null)
                            {
                                OnLoadedListener.Invoke(dict[key]);
                            }
                            Debug.LogError("PlayfabTitleDataLoader: level loaded by playfab");

                            OnDataLoaded();
                        }
                    }
                }
                
            }
            catch (System.Exception ex)
            {
                Debug.LogError("PlayfabTitleDataLoader: get data error: " + ex.Message);
            }
        }, (error) =>
        {
            float delayTime = Mathf.Min(30, countLoadTitleData * 2);
            Debug.LogError("PlayfabTitleDataLoader: load title data, ERROR, key = " + keys[0]);
            StartCoroutine(DelayToRerequest(LoadTitleData, delayTime));
        });
    }

    void OnDataLoaded()
    {

    }


    IEnumerator DelayToRerequest(System.Action request, float timeDelay = 1.0f)
    {
        Debug.LogError("PlayfabTitleDataLoader: Delay to re request: " + timeDelay);
        yield return new WaitForSeconds(timeDelay);
        request?.Invoke();
    }
}
