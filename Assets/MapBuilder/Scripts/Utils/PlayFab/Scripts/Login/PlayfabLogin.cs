using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlayFab;
using PlayFab.ClientModels;
//using LitJson;

namespace Koi.Playfab
{
    public class PlayfabLogin : MonoBehaviour
    {
        #region define/const
        public enum AuthType
        {
            None = 0,
            DeviceId = 1,
            Service = 2
        }

        private const string LAST_AUTH_TYPE_KEY = "PlayfabLogin_LAST_AUTH_TYPE_KEY";
        private const string CUSTOM_ID_KEY = "PlayfabLogin_CUSTOM_ID_KEY";

        #endregion

        public static PlayfabLogin Instance;

        Action OnLoginSuccessListener;
        Action OnLoginFailureListener;

        #region setup
        [SerializeField] string playfabId;
        [SerializeField] bool AutoLogin;
        [SerializeField] PlayfabLoginAndroid platformAndroid;
        [SerializeField] PlayfabLoginIos platformIos;

        PlayfabLoginBasePlatform platform
        {
            get
            {
                #if UNITY_ANDROID
                return platformAndroid;
                #endif
                return platformIos;
            }
        }
        #endregion


        #region var/properties/state

        bool m_IsLoggedIn = false;
        public bool isLoggedIn { get { return m_IsLoggedIn; } }

        AuthType curAuthType = AuthType.None;

        public UserAccountInfo userAccountInfo = null;
        public int timeDelayToLogin = 0;

        #endregion

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                GameObject.DontDestroyOnLoad(gameObject);
            }
            else
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
        }

        void Start()
        {
            InitListener();
        }

        

        void InitListener()
        {
            platform.AddOnLoginSuccessListener(OnLoginSuccess);
            platform.AddOnLoginFailureListener(OnLoginFailure);
        }

        public void OnPlayfabSyncDataLoaded(string loadedPlayfabId)
        {
            // get id from remote config
            if (!string.IsNullOrEmpty(loadedPlayfabId))
            {
                playfabId = loadedPlayfabId;
            }
            PlayFabSettings.TitleId = playfabId;
            AuthByDeviceId();
        }

        #region decide and auth

        /// <summary>
        /// Now: unuse.
        /// </summary>
        void DecideAuthType()
        {
            AuthType lastAuthState = LastAuthType();

            switch (lastAuthState)
            {
                case AuthType.None:
                    // wait social auth
                    curAuthType = AuthType.None;
                    break;

                case AuthType.DeviceId:
                    curAuthType = AuthType.DeviceId;
                    AuthByDeviceId();
                    break;

                case AuthType.Service:
                    curAuthType = AuthType.Service;
                    if (IsExistCustomId())
                    {
                        AuthByCustomId();
                    }
                    break;
            }
        }


        void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && !isLoggedIn)
            {
                Debug.LogError("Auth by deviceId when resume app and have internet");
                AuthByDeviceId();
            }
        }


        void AuthByDeviceId()
        {
            if (!string.IsNullOrEmpty(playfabId))
            {
                platform.LoginByDeviceId();
            }
        }


        void AuthByCustomId()
        {
            LoginWithCustomIDRequest request = new LoginWithCustomIDRequest();
            request.CreateAccount = true;
            request.CustomId = CustomId();
            request.TitleId = PlayFabSettings.TitleId;
            request.InfoRequestParameters = new GetPlayerCombinedInfoRequestParams();
            request.InfoRequestParameters.GetUserAccountInfo = true;
            PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
        }


        public static void AuthByCustomId(string pCustomId)
        {
            if (Instance != null)
            {
                Instance.DoAuthByCustomId(pCustomId);
            }
        }

        void DoAuthByCustomId(string pCustomId)
        {
            LoginWithCustomIDRequest request = new LoginWithCustomIDRequest();
            request.CreateAccount = true;
            request.CustomId = pCustomId;
            request.TitleId = PlayFabSettings.TitleId;
            request.InfoRequestParameters = new GetPlayerCombinedInfoRequestParams();
            request.InfoRequestParameters.GetUserAccountInfo = true;
            PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
        }


        void AuthByPlatformService()
        {
            platform.LoginByGameService();
        }
        #endregion


        #region add/remove listener
        public void AddLoginSuccessListner(Action pListener)
        {
            OnLoginSuccessListener -= pListener;
            OnLoginSuccessListener += pListener;
        }

        public void RemoveLoginSuccessLitener(Action pListener)
        {
            OnLoginSuccessListener -= pListener;
        }

        public void AddLoginFailureListener(Action pListener)
        {
            OnLoginFailureListener -= pListener;
            OnLoginFailureListener += pListener;
        }

        public void RemoveLoginFailureListener(Action pListener)
        {
            OnLoginFailureListener -= pListener;
        }
        #endregion


        #region login listener
        void OnLoginSuccess(LoginResult result)
        {
            if (m_IsLoggedIn)
            {
                return;
            }

            Debug.LogError("PlayFab: on login success");

            m_IsLoggedIn = true;
            SaveAuthType(curAuthType);
            if (result.InfoResultPayload != null)
            {
                userAccountInfo = result.InfoResultPayload.AccountInfo;
            }
//            result.Request = null;
//            Debug.LogError("PlayfabLogin: Login result = " + JsonMapper.ToJson(result));

            if (OnLoginSuccessListener != null)
            {
                OnLoginSuccessListener();
            }

#if UNITY_ANDROID
            platform.LinkWithAdid();
#endif
        }

        void OnLoginFailure(PlayFabError error)
        {
            if (curAuthType == AuthType.Service && LastAuthType() == AuthType.None)
            {
                // first time login, service failure -> can use deviceId
                curAuthType = AuthType.DeviceId;
                AuthByDeviceId();
            }
            else
            {
                //if (OnLoginFailureListener != null)
                //{
                //    OnLoginFailureListener();
                //}
                timeDelayToLogin++;
                if (timeDelayToLogin > 60)
                {
                    return;
                }
                float timeDelay = Mathf.Min(30, timeDelayToLogin * 2);
                StartCoroutine(DelayToLogin(timeDelay));
            }
        }
        #endregion

        IEnumerator DelayToLogin(float delayTime)
        {
            Debug.LogError("PlayFab: delay to re Login " + delayTime);
            yield return new WaitForSecondsRealtime(delayTime);
            AuthByDeviceId();
        }


        void OnSocialFirstAuthResult(bool success)
        {
            Debug.LogError("PlayfabLogin: on social first auth result, success = " + success);
            if (curAuthType == AuthType.None)
            {
                // first time login
                curAuthType = success ? AuthType.Service : AuthType.DeviceId;
                if (success)
                {
                    AuthByPlatformService();
                }
                else
                {
                    AuthByDeviceId();
                }
            }
            else if (curAuthType == AuthType.Service && !IsExistCustomId())
            {
                // logged in by playgame service at last time, but not have customId
                if (success)
                {
                    AuthByPlatformService();
                }
                else
                {
                    if (OnLoginFailureListener != null)
                    {
                        OnLoginFailureListener();
                    }
                }
            }
        }


        #region request data
        public void RequestAccountInfo(Action<UserAccountInfo> callback = null)
        {
            GetAccountInfoRequest request = new GetAccountInfoRequest();
            PlayFabClientAPI.GetAccountInfo(request, (result) =>
                {
                    userAccountInfo = result.AccountInfo;
                    Debug.LogError("PlayfabLogin: Get account info: " + JsonUtility.ToJson(result));
                    if (callback != null)
                    {
                        callback(result.AccountInfo);
                    }
                }, (error) =>
                {
                    Debug.LogError("PlayfabLogin: Get account info error: " + JsonUtility.ToJson(error));
                });
        }
        #endregion


        #region load data
        AuthType LastAuthType()
        {
            return (AuthType)PlayerPrefs.GetInt(LAST_AUTH_TYPE_KEY, (int)AuthType.None);
        }

        void SaveAuthType(AuthType pType)
        {
            PlayerPrefs.SetInt(LAST_AUTH_TYPE_KEY, (int)pType);
            PlayerPrefs.Save();
        }


        string CustomId()
        {
            return PlayerPrefs.GetString(CUSTOM_ID_KEY, "");
        }

        public void SaveCustomId(string pCustomId)
        {
            PlayerPrefs.SetString(CUSTOM_ID_KEY, pCustomId);
            PlayerPrefs.Save();
        }

        bool IsExistCustomId()
        {
            return !string.IsNullOrEmpty(CustomId());
        }
        #endregion

        public static bool IsAvailable => Instance != null && Instance.isLoggedIn;
    }
}
