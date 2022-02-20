using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;
using System;

namespace Koi.Playfab
{
    public class PlayfabTypeNameUI : MonoBehaviour
    {
        Action<string> OnUpdatedNameListener;

        [SerializeField] InputField inputField;
        [SerializeField] Button okButton;
        [SerializeField] Button okButton_2;
        [SerializeField] Button cancelButton;

        [SerializeField] Text errorText;

        void Start()
        {
            okButton.onClick.AddListener(OnOkPress);
            okButton_2.onClick.AddListener(OnOkPress);
            cancelButton.onClick.AddListener(OnCancelPress);
        }

        public void OpenUI(bool allowCancel, string currentName = null)
        {
            gameObject.SetActive(true);
            okButton.gameObject.SetActive(!allowCancel);
            okButton_2.gameObject.SetActive(allowCancel);
            cancelButton.gameObject.SetActive(allowCancel);
            if (!string.IsNullOrEmpty(currentName))
            {
                inputField.text = currentName;
            }
        }

        public void AddUpdatedNameListener(Action<string> pListener)
        {
            OnUpdatedNameListener -= pListener;
            OnUpdatedNameListener += pListener;
        }

        public void RemoveUpdatedNameListener(Action<string> pListener)
        {
            OnUpdatedNameListener -= pListener;
        }

        void OnOkPress()
        {
            string username = inputField.text;
            bool canUse = false;
            for (int i = 0; i < username.Length; i++)
            {
                if (username[i].CompareTo(' ') != 0 || username[i].CompareTo('\n') == 0)
                {
                    canUse = true;
                    break;
                }
            }

            if (canUse)
            {
                PostName(username);
            }
        }

        void OnCancelPress()
        {
            gameObject.SetActive(false);
        }


        void PostName(string username)
        {
            errorText.gameObject.SetActive(false);
            UpdateUserTitleDisplayNameRequest request = new UpdateUserTitleDisplayNameRequest();
            request.DisplayName = username;
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, (result) =>
                {
                    Debug.LogError("PlayfabLeaderboard:/PlayfabTypeName: Update result " + JsonUtility.ToJson(result));
                    // callback to PlayfabLogin update
                    PlayfabLogin.Instance.RequestAccountInfo();
                    gameObject.SetActive(false);
                    if (OnUpdatedNameListener != null)
                    {
                        OnUpdatedNameListener(result.DisplayName);
                    }
                }, (error) =>
                {
                    Debug.LogError("PlayfabLeaderboard:/PlayfabTypeName: Update error " + JsonUtility.ToJson(error));

                    errorText.gameObject.SetActive(true);
                    errorText.text = "Error: " + error.ErrorMessage;
//                    gameObject.SetActive(false);
                });
        }
    }
}

