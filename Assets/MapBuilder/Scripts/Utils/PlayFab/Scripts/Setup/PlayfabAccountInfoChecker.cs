using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;

namespace Koi.Playfab
{
    public class PlayfabAccountInfoChecker : MonoBehaviour
    {
        private const string CHECKED_KEY = "AccountInfoChecker_CHECKED_KEY";

        [SerializeField] PlayfabTypeNameUI typeNameUIPrefabs;
        PlayfabTypeNameUI typeNameUI;

        void Start()
        {
            PlayfabLogin.Instance.AddLoginSuccessListner(OnLogginSuccess);
        }

        void OnLogginSuccess()
        {
            if (PlayfabLogin.Instance.userAccountInfo != null)
            {
//                Debug.LogError("PlayfabLogin: userAccountInfo = " + LitJson.JsonMapper.ToJson(PlayfabLogin.Instance.userAccountInfo));
                var accountInfo = PlayfabLogin.Instance.userAccountInfo;
                if (!(accountInfo.TitleInfo != null && !string.IsNullOrEmpty(accountInfo.TitleInfo.DisplayName)))
                {
                    Debug.LogError("PlayfabLogin: have account info (maybe error) but not have user name -> request");
                    PlayfabLogin.Instance.RequestAccountInfo(OnReceivedAccountInfo);
                }
            }
            else
            {
                Debug.LogError("PlayfabLogin: not have user account info -> request");
                PlayfabLogin.Instance.RequestAccountInfo(OnReceivedAccountInfo);
            }
        }

        void OnReceivedAccountInfo(UserAccountInfo info)
        {
            if (!(info != null && info.TitleInfo != null && !string.IsNullOrEmpty(info.TitleInfo.DisplayName)))
            {
                Debug.LogError("PlayfabLogin: request and received account info but not have username -> open dialog to put");
//                OpenTypeNameUI();
                StartCoroutine(DoSetupName());
            }
        }

        IEnumerator DoSetupName()
        {
            yield return null;

            while (Social.Active == null
                   || Social.Active.localUser == null
                   || !Social.Active.localUser.authenticated)
            {
                yield return new WaitForSeconds(1.0f);
            }

            if (userAccountInfo == null || userAccountInfo.TitleInfo == null || string.IsNullOrEmpty(userAccountInfo.TitleInfo.DisplayName))
            {
                //post social name
                if (!string.IsNullOrEmpty(Social.localUser.userName))
                {
                    PostName(Social.localUser.userName);
                }
            }
        }

        void PostName(string username)
        {
            UpdateUserTitleDisplayNameRequest request = new UpdateUserTitleDisplayNameRequest();
            request.DisplayName = username;
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, (result) =>
                {
                    Debug.LogError("PlayfabLeaderboard:/PlayfabTypeName: Update result " + JsonUtility.ToJson(result));
                    // callback to PlayfabLogin update
                    PlayfabLogin.Instance.RequestAccountInfo();
                }, (error) =>
                {
                    Debug.LogError("PlayfabLeaderboard:/PlayfabTypeName: Update error " + JsonUtility.ToJson(error));
                });
        }

        void OpenTypeNameUI(bool force = false)
        {
            if (PlayerPrefs.GetInt(CHECKED_KEY, 0) == 0 || force)
            {
                PlayerPrefs.SetInt(CHECKED_KEY, 1);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogError("PlayfabLogin: not have user name but not open TypeNameUI because opened before");
                return;
            }

            Debug.LogError("PlayfabLogin: open type name UI");
            if (typeNameUI == null)
            {
                typeNameUI = Instantiate(typeNameUIPrefabs);
            }
            typeNameUI.OpenUI(false);
        }

        UserAccountInfo userAccountInfo
        {
            get
            {
                return PlayfabLogin.Instance.userAccountInfo;
            }
        }
    }
}
