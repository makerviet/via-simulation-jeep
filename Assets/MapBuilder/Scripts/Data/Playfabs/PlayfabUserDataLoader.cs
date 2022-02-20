using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayfabUserDataLoader : MonoBehaviour
{
    System.Action<string> OnLoadedListener;
    Action OnLoadFailedListener;

    [Header("Log")]
    [SerializeField] bool IsLoaded = false;
    [SerializeField] string key;


    public void StartLoadUserData(string pKey, Action<string> callback, Action failedCallback)
    {
        this.key = pKey;

        OnLoadFailedListener = failedCallback;
        OnLoadedListener = callback;

        countLoadUserData = 0;
        IsLoaded = false;
        LoadUserBehaviorData();
    }

    int countLoadUserData;
    void LoadUserBehaviorData()
    {
        countLoadUserData++;
        if (countLoadUserData > 60)
        {
            OnLoadFailedListener?.Invoke();
            Debug.LogError("PlayfabUserDataLoader: CANNOT load user data");
            return;
        }


        GetUserDataRequest request = new GetUserDataRequest();
        request.Keys = new List<string>();
        request.Keys.Add(key);

        PlayFabClientAPI.GetUserData(request, (result) =>
        {
            var dict = result.Data;

            if (dict.ContainsKey(key))
            {
                string json = dict[key].Value;
                if (!IsLoaded && !string.IsNullOrEmpty(json))
                {
                    IsLoaded = true;
                    if (OnLoadedListener != null)
                    {
                        OnLoadedListener.Invoke(json);
                    }
                    Debug.LogError("PlayfabTitleDataLoader: level loaded by playfab");

                    OnDataLoaded();
                }
            }
        }, (error) =>
        {
            float delayTime = Mathf.Min(30, countLoadUserData * 2);
            StartCoroutine(DelayToRerequest(LoadUserBehaviorData, delayTime));
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
