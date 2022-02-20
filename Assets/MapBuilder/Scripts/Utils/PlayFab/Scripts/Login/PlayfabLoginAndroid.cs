using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;

#if UNITY_ANDROID
//using GooglePlayGames;
#endif

namespace Koi.Playfab
{
    public class PlayfabLoginAndroid : PlayfabLoginBasePlatform
    {
        public enum LinkGoogleErrorCode
        {
            AccountAlreadyLinked = 1011,
            GoogleOAuthError = 1271,
            GoogleOAuthNoIdTokenIncludedInResponse = 1275,
            GoogleOAuthNotConfiguredForTitle = 1270,
            InvalidGoogleToken = 1026,
            LinkedAccountAlreadyClaimed = 1012
        }

        public override void LoginByDeviceId()
        {
            #if UNITY_ANDROID
            var request = new LoginWithAndroidDeviceIDRequest
                {
                    AndroidDeviceId = SystemInfo.deviceUniqueIdentifier,
                    CreateAccount = true
                };
            request.InfoRequestParameters = new GetPlayerCombinedInfoRequestParams();
            request.InfoRequestParameters.GetUserAccountInfo = true;

            PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnLoginSuccess, OnLoginFailure);
            #endif
        }

        public override void LinkWithAdid()
        {
            Debug.LogError("LinkWithAidi: Request aidi");
            Application.RequestAdvertisingIdentifierAsync((string advertisingId, bool trackingEnabled, string error) =>
            {
                Debug.LogError("LinkWithAidi: advertisingId " + advertisingId + " " + trackingEnabled + " " + error);
                if (!string.IsNullOrEmpty(advertisingId) && trackingEnabled)
                {
                    // post
                    Debug.LogError("LinkWithAidi: Do Link");
                    LinkCustomIDRequest request = new LinkCustomIDRequest();
                    request.CustomId = advertisingId;
                    request.ForceLink = true;
                    PlayFabClientAPI.LinkCustomID(request, (result) =>
                    {
                        Debug.LogError("LinkWithAidi: Link Success!");
                    }, (linkError) =>
                    {
                    });
                }
            });
        }

        public override void LoginByGameService()
        {
            #if UNITY_ANDROID
//            var request = new LoginWithGoogleAccountRequest();
//            request.CreateAccount = true;
//            request.TitleId = PlayFabSettings.TitleId;
//            request.ServerAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
//            request.InfoRequestParameters = new GetPlayerCombinedInfoRequestParams();
//
//            request.InfoRequestParameters.GetUserAccountInfo = true;
//
//            var serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
//            Debug.Log("PlayfabLogin: Server Auth Code: " + serverAuthCode + " _infoRequestParameters = " + JsonMapper.ToJson(request.InfoRequestParameters));
//            if (string.IsNullOrEmpty(serverAuthCode))
//            {
//                Debug.LogError("PlayfabLogin: Android, server auth code null empty. Id token= " + PlayGamesPlatform.Instance.GetIdToken());
//            }
//            else
//            {
//                Debug.LogError("PlayfabLogin: Android, server auth code:>_" + serverAuthCode + "_<");
//            }
//
//            PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
//                {
//                    TitleId = PlayFabSettings.TitleId,
//                    ServerAuthCode = serverAuthCode,
//                    CreateAccount = true
//                }, OnLoginSuccess, OnLoginFailure);
            #endif
        }

        public override void LinkToGameService()
        {
            #if UNITY_ANDROID
//            LinkGoogleAccountRequest linkRequest = new LinkGoogleAccountRequest();
//            linkRequest.ForceLink = false;
//            linkRequest.ServerAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
//            PlayFabClientAPI.LinkGoogleAccount(linkRequest, (result) =>
//                {
//                    Debug.LogError("PlayfabLogin: Link Result: " + JsonMapper.ToJson(result));
//                    OnLinkResult(LinkResultType.Success);
//                }, (error) =>
//                {
//                    Debug.LogError("PlayfabLogin: Link Error: " + JsonMapper.ToJson(error));
//                    LinkGoogleErrorCode code = (LinkGoogleErrorCode)error.HttpCode;
//                    switch (code)
//                    {
//                        case LinkGoogleErrorCode.AccountAlreadyLinked:
//                            OnLinkResult(LinkResultType.Success);
//                            break;
//
//                        case LinkGoogleErrorCode.LinkedAccountAlreadyClaimed:
//                            OnLinkResult(LinkResultType.AlreadyClaimed);
//                            break;
//
//                        default:
//                            OnLinkResult(LinkResultType.Failure);
//                            break;
//                    }
//                });
            #endif
        }

        public override void UnlinkGameService()
        {
            #if UNITY_ANDROID
            UnlinkGoogleAccountRequest unlinkRequest = new UnlinkGoogleAccountRequest();
            PlayFabClientAPI.UnlinkGoogleAccount(unlinkRequest, (result) =>
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
