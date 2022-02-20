using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Koi.Playfab
{
    public class PlayfabLoginIos : PlayfabLoginBasePlatform
    {
        public enum LinkGameCenterErrorCode
        {
            AccountAlreadyLinked = 1011,
            LinkedAccountAlreadyClaimed = 1012
        }

        private const string CUSTOM_ID_KEY = "CUSTOM_ID_KEY";

        string CustomID => PlayerPrefs.GetString(CUSTOM_ID_KEY, "");
        void SaveCustomId(string pCustomId)
        {
            PlayerPrefs.SetString(CUSTOM_ID_KEY, pCustomId);
            PlayerPrefs.Save();
        }

        public override void LoginByDeviceId()
        {
            #if UNITY_EDITOR
            var request = new LoginWithIOSDeviceIDRequest();
            request.DeviceId = SystemInfo.deviceUniqueIdentifier;
            request.CreateAccount = true;
            request.InfoRequestParameters = new GetPlayerCombinedInfoRequestParams();
            request.InfoRequestParameters.GetUserAccountInfo = true;
            PlayFabClientAPI.LoginWithIOSDeviceID(request, OnLoginSuccess, OniOSLoginFailure);
            #elif UNITY_IOS
            LoginWithIOSDevice();
            #endif
        }

        void LoginWithIOSDevice()
        {
#if UNITY_IOS
            string mCustomId = CustomID;
            bool customIdBlank = string.IsNullOrEmpty(mCustomId);
            if (customIdBlank && Device.advertisingTrackingEnabled
                && !string.IsNullOrEmpty(Device.advertisingIdentifier))
            {
                LoginByAdIdThenSyncToCustomId();
            }
            else
            {
                LoginByCustomId(mCustomId, customIdBlank);
            }
#endif
        }

        void LoginByCustomId(string pCustomId, bool needCreateNew)
        {
            if (needCreateNew)
            {
                pCustomId = GenCustomId();
            }
            var request = new LoginWithCustomIDRequest();
            request.CustomId = pCustomId;
            request.CreateAccount = true;
            request.InfoRequestParameters = new GetPlayerCombinedInfoRequestParams();
            request.InfoRequestParameters.GetUserAccountInfo = true;
            PlayFabClientAPI.LoginWithCustomID(request, (result) =>
            {
                if (needCreateNew)
                {
                    SaveCustomId(pCustomId);
                }
                OnLoginSuccess(result);
            }, OnLoginFailure);
        }

        string GenCustomId()
        {
            string deviceIdPart = "id";
            if (!string.IsNullOrEmpty(SystemInfo.deviceUniqueIdentifier))
            {
                deviceIdPart = SystemInfo.deviceUniqueIdentifier.Substring(0, 8);
            }

            var today = System.DateTime.Now;
            string day = string.Format("{0}{1}{2}", today.Year % 100, today.Month, today.Day);
            string time_rd = string.Format("{0}_{1}", today.TimeOfDay.Ticks, Random.Range(0, 1000));

            return string.Format("{0}_{1}_{2}", day, deviceIdPart, time_rd);
        }

        void LoginByAdIdThenSyncToCustomId()
        {
#if UNITY_IOS
            var request = new LoginWithIOSDeviceIDRequest();
            request.DeviceId = Device.advertisingIdentifier;
            request.CreateAccount = true;
            request.InfoRequestParameters = new GetPlayerCombinedInfoRequestParams();
            request.InfoRequestParameters.GetUserAccountInfo = true;
            PlayFabClientAPI.LoginWithIOSDeviceID(request, (result) =>
            {
                LinkCustomId(result);
                OnLoginSuccess(result);
            }, OnLoginFailure);
#endif
        }

        void LinkCustomId(LoginResult loginResult)
        {
            LinkCustomIDRequest request = new LinkCustomIDRequest();
            request.CustomId = loginResult.PlayFabId;
            request.ForceLink = true;
            PlayFabClientAPI.LinkCustomID(request, (result) =>
            {
                SaveCustomId(request.CustomId);
            }, (error) =>
            {
            });
        }

#region login result
        protected virtual void OniOSLoginFailure(PlayFabError error)
        {
            OnLoginFailure(error);
            if (error.Error == PlayFabErrorCode.ServiceUnavailable)
            {
            }
            else
            {
                //Device.advertisingTrackingEnabled
            }
            Debug.LogError("PlayfabLoginError: " + error.ErrorMessage +  JsonUtility.ToJson(error));
        }
#endregion


        public override void LoginByGameService()
        {
#if UNITY_IOS
            LoginWithGameCenterRequest request = new LoginWithGameCenterRequest();
            request.CreateAccount = true;
            request.PlayerId = Social.localUser.id;
            request.InfoRequestParameters = new GetPlayerCombinedInfoRequestParams();
            request.InfoRequestParameters.GetUserAccountInfo = true;
            PlayFabClientAPI.LoginWithGameCenter(request, OnLoginSuccess, OnLoginFailure);
#endif
        }

        public override void LinkToGameService()
        {
#if UNITY_IOS
            LinkGameCenterAccountRequest linkRequest = new LinkGameCenterAccountRequest();
            linkRequest.ForceLink = true;
            linkRequest.GameCenterId = Social.localUser.id;
            PlayFabClientAPI.LinkGameCenterAccount(linkRequest, (result) =>
            {
            Debug.LogError("TestPlayfabLogin: Link Result: " + JsonUtility.ToJson(result));
            OnLinkResult(LinkResultType.Success);
            }, (error) =>
            {
            Debug.LogError("TestPlayfabLogin: Link Error: " + JsonUtility.ToJson(error));
            LinkGameCenterErrorCode code = (LinkGameCenterErrorCode)error.HttpCode;
            switch (code)
            {
            case LinkGameCenterErrorCode.AccountAlreadyLinked:
            OnLinkResult(LinkResultType.Success);
            break;

            case LinkGameCenterErrorCode.LinkedAccountAlreadyClaimed:
            OnLinkResult(LinkResultType.AlreadyClaimed);
            break;

            default:
            OnLinkResult(LinkResultType.Failure);
            break;
            }
            });
#endif
        }

        public override void UnlinkGameService()
        {
#if UNITY_IOS
            UnlinkGameCenterAccountRequest unlinkRequest = new UnlinkGameCenterAccountRequest();
            PlayFabClientAPI.UnlinkGameCenterAccount(unlinkRequest, (result) =>
            {
            Debug.LogError("PlayfabLogin: UnLink Result: " + JsonUtility.ToJson(result));
            OnUnlinkResult(true);
            }, (error) =>
            {
            Debug.LogError("PlayfabLogin: UnLink Result: " + JsonUtility.ToJson(error));
            OnUnlinkResult(false);
            });
#endif
        }
    }
}
