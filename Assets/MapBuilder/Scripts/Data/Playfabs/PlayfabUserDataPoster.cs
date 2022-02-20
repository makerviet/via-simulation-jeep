using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayfabUserDataPoster : MonoBehaviour
{
    Action OnPostedListener;
    Action OnPostFailedListener;

    [Header("Log")]
    [SerializeField] bool IsPosted = false;
    [SerializeField] string key;
    [SerializeField] string jsonData;

    public void StartPostUserData(string pKey, string pJsonData, Action callback = null, Action failedCallback = null)
    {
        OnPostedListener = callback;
        OnPostFailedListener = failedCallback;

        this.key = pKey;
        this.jsonData = pJsonData;

        countPostUserData = 0;
        IsPosted = false;

        PostUserData();
    }

    public int countPostUserData;
    void PostUserData()
    {
        countPostUserData++;
        if (countPostUserData > 30)
        {
            OnPostFailedListener?.Invoke();
            return;
        }

        UpdateUserDataRequest request = new UpdateUserDataRequest();
        request.Data = new Dictionary<string, string>();
        request.Data.Add(key, jsonData);
        PlayFabClientAPI.UpdateUserData(request, (result) =>
        {
            if (!IsPosted)
            {
                IsPosted = true;
                OnPostedListener?.Invoke();
            }
        }, (error) =>
        {
            StartCoroutine(DelayToPostUserData(PostUserData));
        });
    }


    IEnumerator DelayToPostUserData(Action action, float timeDelay = 1.0f)
    {
        yield return new WaitForSeconds(timeDelay);
        if (action != null)
        {
            action();
        }
    }
}
