using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlayFab.ClientModels;
using PlayFab;

namespace Koi.Playfab
{
    public class PlayfabLoginBasePlatform : MonoBehaviour
    {
        public enum LinkResultType
        {
            Success = 0,
            Failure = 1,
            AlreadyClaimed = 2
        }

        Action<LoginResult> OnLoginSuccessLitener;
        Action<PlayFabError> OnLoginFailureListener;
        Action<LinkResultType> OnLinkResultListener;
        Action<bool> OnUnlinkResultListener;

        #region login method
        public virtual void LoginByDeviceId()
        {

        }

        public virtual void LoginByGameService()
        {

        }

        public virtual void LinkToGameService()
        {

        }

        public virtual void UnlinkGameService()
        {
        }

        public virtual void LinkWithAdid()
        {
        }
        #endregion


        #region add/remove listener
        public virtual void AddOnLoginSuccessListener(Action<LoginResult> pListener)
        {
            OnLoginSuccessLitener -= pListener;
            OnLoginSuccessLitener += pListener;
        }

        public virtual void RemoveOnLogedInListener(Action<LoginResult> pListener)
        {
            OnLoginSuccessLitener -= pListener;
        }

        public virtual void AddOnLoginFailureListener(Action<PlayFabError> pListener)
        {
            OnLoginFailureListener -= pListener;
            OnLoginFailureListener += pListener;
        }

        public virtual void RemoveOnLoginFailureListener(Action<PlayFabError> pListener)
        {
            OnLoginFailureListener -= pListener;
        }

        public virtual void AddLinkResultListener(Action<LinkResultType> pListener)
        {
            OnLinkResultListener -= pListener;
            OnLinkResultListener += pListener;
        }

        public virtual void RemoveLinkResultListener(Action<LinkResultType> pListener)
        {
            OnLinkResultListener -= pListener;
        }

        public virtual void AddUnlinkResultListener(Action<bool> pListener)
        {
            OnUnlinkResultListener -= pListener;
            OnUnlinkResultListener += pListener;
        }

        public virtual void RemoveUnlinkResultListener(Action<bool> pListener)
        {
            OnUnlinkResultListener -= pListener;
        }
        #endregion


        #region login result
        protected virtual void OnLoginSuccess(LoginResult result)
        {
            if (OnLoginSuccessLitener != null)
            {
                OnLoginSuccessLitener(result);
            }
        }

        protected virtual void OnLoginFailure(PlayFabError error)
        {
            if (OnLoginFailureListener != null)
            {
                OnLoginFailureListener(error);
            }
        }
        #endregion


        #region link/unlink result
        protected virtual void OnLinkResult(LinkResultType resultType)
        {
            if (OnLinkResultListener != null)
            {
                OnLinkResultListener(resultType);
            }
        }

        protected virtual void OnUnlinkResult(bool success)
        {
            if (OnUnlinkResultListener != null)
            {
                OnUnlinkResultListener(success);
            }
        }
        #endregion
    }
}
