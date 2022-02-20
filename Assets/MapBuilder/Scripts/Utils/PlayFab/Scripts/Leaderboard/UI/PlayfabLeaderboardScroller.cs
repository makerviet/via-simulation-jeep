using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using PlayFab.ClientModels;

namespace Koi.Playfab
{
    public class PlayfabLeaderboardScroller : MonoBehaviour
    {
        public enum ScrollerState
        {
            Normal,
            WaitingData
        }

        Action OnReachBottomListener;

        [Header("Setup")]
        [SerializeField] GameObject loadingText;
        [SerializeField] RectTransform settingBar;
        [SerializeField] PlayfabLeaderboardUICell cellPrefab;
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] PlayfabLeaderboardUICell[] topThreeCells;

        [Tooltip("Content of ScrollView")]
        [SerializeField] RectTransform contentRoot;

        [Tooltip("white body for all LeaderboardCell, contain top 3 cells and all normal cells")]
        [SerializeField] Image bodyBackground;
        [SerializeField] Image bodyBgOptimize;
        [SerializeField] float offsetBogyBgSize = 1580;

        [Tooltip("Root of other normal cells")]
        [SerializeField] RectTransform normalCellRoot;

        [SerializeField] List<PlayfabLeaderboardUICell> listCell = new List<PlayfabLeaderboardUICell>();

        [Header("Config size")]
        [SerializeField] Vector2 normalCellStartPos = new Vector2(20, 0);
        [SerializeField] float normalCellHeight = 96;
        [SerializeField] float marginUI = 32;

        [Tooltip("padding of view when compare with screen size")]
        [SerializeField] int paddingView = 96;


        Vector2 lastScrollValue = Vector2.zero;
        Vector2 lastContentPos = Vector2.zero;
        List<PlayerLeaderboardEntry> mData = new List<PlayerLeaderboardEntry>();
        int inview_minId = 0;
        int inview_maxId = 0;

        ScrollerState m_State = ScrollerState.WaitingData;
        ScrollerState curState
        {
            get { return m_State; }
            set
            {
                if (m_State != value)
                {
                    m_State = value;
                    UpdateState();
                }
            }
        }

        void Start()
        {
            scrollRect.onValueChanged.AddListener(OnScrollChanged);
        }

        private void Update()
        {
            if (bodyBgOptimize.gameObject.activeInHierarchy)
            {
                // update position to match vs bodyBg
                float offsetY = bodyBackgroundLocalY;
                float contentRootY = contentRoot.anchoredPosition.y;
                float bgOptY = Mathf.Min(0, offsetY + contentRootY);
                var localPos = bodyBgOptimize.rectTransform.localPosition;
                localPos.y = bgOptY;
                bodyBgOptimize.rectTransform.localPosition = localPos;
            }
        }

        #region add/remove listener

        public void AddReachBottomListener(Action pListener)
        {
            OnReachBottomListener -= pListener;
            OnReachBottomListener += pListener;
        }

        public void RemoveReachBottomListener(Action pListener)
        {
            OnReachBottomListener -= pListener;
        }

        #endregion


        public void SetupData(List<PlayerLeaderboardEntry> pListPlayerEntry)
        {
            mData = pListPlayerEntry;
            curState = ScrollerState.Normal;
            UpdateData();
        }

        public void OnRequestDataError()
        {
            if (curState == ScrollerState.WaitingData)
            {
                curState = ScrollerState.Normal;
            }
        }

        void UpdateState()
        {
            loadingText.SetActive(isWaitingData);
        }


        void UpdateData()
        {
            for (int i = 0; i < 3; i++)
            {
                if (i < mData.Count)
                {
                    topThreeCells[i].gameObject.SetActive(true);
                    topThreeCells[i].SetupData(mData[i], mData[i].PlayFabId == myPlayfabId);
                }
                else
                {
                    topThreeCells[i].gameObject.SetActive(false);
                }
            }

            foreach (PlayfabLeaderboardUICell cell in listCell)
            {
                if (cell.data != null)
                {
                    PlayerLeaderboardEntry cellData = CellDataOfPos(cell.data.Position);
                    if (cellData != null)
                    {
                        cell.gameObject.SetActive(true);
                        cell.SetupData(cellData, cellData.PlayFabId == myPlayfabId);
                    }
                    else
                    {
                        cell.gameObject.SetActive(false);
                    }
                }
                else
                {
                    cell.gameObject.SetActive(false);
                }
            }

            UpdateScrollSize();
            UpdateCellList();
        }

        void UpdateScrollSize()
        {
            // bodyBackground
            float bodyHeight = Math.Abs(normalCellParentLocalY) + marginUI;

            int numberNormalCell = Mathf.Max(0, mData.Count - 3);
            bodyHeight += numberNormalCell * normalCellHeight + normalCellStartPos.y;

            SetRectTransformHeight(bodyBackground.rectTransform, bodyHeight);
            if (bodyHeight > offsetBogyBgSize)
            {
                bodyBgOptimize.gameObject.SetActive(true);
                bodyBackground.enabled = false;
            }
            else
            {
                bodyBgOptimize.gameObject.SetActive(false);
                bodyBackground.enabled = true;
            }

            // content
            float contentHeight = Math.Abs(bodyBackgroundLocalY) + bodyHeight;
            SetRectTransformHeight(contentRoot, contentHeight);
        }

        void UpdateCellList()
        {
            float firstNormalCellY = normalCellParentLocalY + bodyBackgroundLocalY + contentRoot.anchoredPosition.y;

            float viewHeight = scrollRect.viewport.rect.height;

            float paddingTopY = paddingView;
            float paddingBotY = -viewHeight - paddingView;

            // each normal cell (with Id = cellId) will have bound by:
            //      top = firstNormalCellY + (3 - cellId + 0.5f) * normalCellHeight
            //      bottom = firstNormalCellY + (3 - cellId - 0.5f) * normalCellHeight


            // find first cell in the view (smallest cellId which cell's botY < Padding_Top_Y)
            int minId = Mathf.CeilToInt((firstNormalCellY + (3 - 0.5f) * normalCellHeight - paddingTopY) / normalCellHeight);
            minId = Mathf.Max(3, minId);

            // find last cell in the view (biggest cellId which cell's topY > Padding_Bot_Y)
            int maxId = (int)((firstNormalCellY + (3 + 0.5f) * normalCellHeight - paddingBotY) / normalCellHeight);
            maxId = Mathf.Min(maxId, mData.Count-1);

            if (minId <= maxId)
            {
                UpdateCellVisible(firstNormalCellY, minId, maxId);
            }
        }

        void UpdateCellVisible(float firstNormalCellY, int minId, int maxId)
        {
            if (inview_minId == minId && inview_maxId == maxId)
            {
                // don't need update
                return;
            }

            List<int> listId = new List<int>();
            List<int> listPosition = new List<int>();
            for (int i = minId; i <= maxId; i++)
            {
                if (i < mData.Count)
                {
                    listId.Add(i);
                    listPosition.Add(mData[i].Position);
                }
            }

            // hide other cell 
            foreach (PlayfabLeaderboardUICell cell in listCell)
            {
                if (cell.data == null)
                {
                    cell.gameObject.SetActive(false);
                }
                else
                {
                    int idInListIfExist = listPosition.IndexOf(cell.data.Position);
                    if (idInListIfExist < 0)
                    {
                        cell.gameObject.SetActive(false);
                    }
                    else
                    {
                        cell.gameObject.SetActive(true);
                        listId.RemoveAt(idInListIfExist);
                        listPosition.RemoveAt(idInListIfExist);
                    }
                }
            }

            // find cell for position
            foreach (int idData in listId)
            {
                PlayfabLeaderboardUICell unuseCell = GetUnuseCell();
                unuseCell.SetupData(mData[idData], mData[idData].PlayFabId == myPlayfabId);
                var pos = unuseCell.rectTransform.anchoredPosition;
                pos.y = normalCellStartPos.y - (idData - 3) * normalCellHeight;
                unuseCell.rectTransform.anchoredPosition = pos;
            }

            inview_maxId = maxId;
            inview_minId = minId;
        }

        void OnScrollChanged(Vector2 posValue)
        {
            if (lastScrollValue.y > 0 && posValue.y <= 0
                || lastScrollValue.y >= 0 && posValue.y < 0)
            {
                if (OnReachBottomListener != null && !isWaitingData)
                {
                    OnReachBottomListener();
                    curState = ScrollerState.WaitingData;
                }
            }
            lastScrollValue = posValue;
            UpdateCellList();

            UpdateSettingBar();
        }

        void UpdateSettingBar()
        {
            var delta = contentRoot.anchoredPosition - lastContentPos;
            var pos = settingBar.anchoredPosition;
            pos.y += delta.y;
            pos.y = Mathf.Min(pos.y, contentRoot.anchoredPosition.y);
            pos.y = Mathf.Clamp(pos.y, 0, 100);
            settingBar.anchoredPosition = pos;
            lastContentPos = contentRoot.anchoredPosition;
        }


        float normalCellParentLocalY { get { return normalCellRoot.anchoredPosition.y; } }

        float bodyBackgroundLocalY { get { return bodyBackground.rectTransform.anchoredPosition.y; } }


        void SetRectTransformHeight(RectTransform pRect, float pHeight)
        {
            var size = pRect.sizeDelta;
            size.y = pHeight;
            pRect.sizeDelta = size;
        }

        bool IsOutOfValue(int pPos, int min, int max)
        {
            return pPos < min || pPos > max;
        }

        PlayfabLeaderboardUICell GetUnuseCell()
        {
            foreach (PlayfabLeaderboardUICell cell in listCell)
            {
                if (!cell.gameObject.activeInHierarchy)
                {
                    cell.gameObject.SetActive(true);
                    return cell;
                }
            }

            PlayfabLeaderboardUICell newCell = Instantiate(cellPrefab, normalCellRoot);
            //newCell.transform.SetParent(normalCellRoot);
            newCell.transform.localRotation = Quaternion.identity;
            newCell.transform.localScale = Vector3.one;
            //newCell.rectTransform.anchoredPosition = normalCellStartPos;
            var pos = cellPrefab.rectTransform.anchoredPosition;
            pos.y = normalCellStartPos.y;
            newCell.rectTransform.anchoredPosition = pos;
            listCell.Add(newCell);
            return newCell;
        }

        PlayerLeaderboardEntry CellDataOfPos(int position)
        {
            foreach (PlayerLeaderboardEntry data in mData)
            {
                if (data.Position == position)
                {
                    return data;
                }
            }
            return null;
        }

        bool isWaitingData
        {
            get 
            {
                return (curState == ScrollerState.WaitingData);
            }
        }

        UserAccountInfo userAccountInfo
        {
            get
            {
                return PlayfabLogin.Instance.userAccountInfo;
            }
        }

        string myPlayfabId
        {
            get
            {
                if (userAccountInfo != null)
                {
                    return userAccountInfo.PlayFabId;
                }
                return "";
            }
        }
    }
}
