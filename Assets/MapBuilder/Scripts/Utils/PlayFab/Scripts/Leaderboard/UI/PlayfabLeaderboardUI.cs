using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using PlayFab.ClientModels;
using Koi.DeviceInput;

namespace Koi.Playfab
{
    public class PlayfabLeaderboardUI : MonoBehaviour
    {
        public static PlayfabLeaderboardUI Instance;

        [SerializeField] int maxResultCountEachRequest = 24;
        [SerializeField] Dropdown dropDown;
        [SerializeField] PlayfabLeaderboardScroller scroller;
        [SerializeField] PlayfabLeaderboardUICell playerCell;
        [SerializeField] Button closeButton;
        [SerializeField] Button editNameButton;

        [SerializeField] PlayfabTypeNameUI typeNameUIPrefabs;
        PlayfabTypeNameUI typeNameUI;

        LeaderboardType curLeaderboardType = LeaderboardType.Today;

        List<PlayerLeaderboardEntry> allTimeData = new List<PlayerLeaderboardEntry>();
        List<PlayerLeaderboardEntry> todayData = new List<PlayerLeaderboardEntry>();
        List<PlayerLeaderboardEntry> thisweekData = new List<PlayerLeaderboardEntry>();

        List<int> playerScore = new List<int>();
        List<int> playerPosition = new List<int>();

        public float testLoadTime = 0.25f;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                GameObject.DontDestroyOnLoad(gameObject);
                return;
            }
            else
            {
                GameObject.DestroyImmediate(gameObject);
            }
        }

        void Start()
        {
            InitListener();
        }

        void OnEnable()
        {
            ClearData();
            ChangeScrollerData();
            ChangePlayerData();

            CheckUserAccountInfo();

            BackPressEvent.AddSwallowBackPressListener(OnCloseLeaderboardClick);
        }

        void OnDisable()
        {
            BackPressEvent.RemoveSwallowBackPressListener(OnCloseLeaderboardClick);
        }

        void CheckUserAccountInfo()
        {
            UserAccountInfo info = userAccountInfo;
            if (info == null || info.TitleInfo == null || string.IsNullOrEmpty(info.TitleInfo.DisplayName))
            {
                OnEditNameClick();
            }
        }

        void InitListener()
        {
            dropDown.onValueChanged.AddListener(OnLeaderboardChanged);
            scroller.AddReachBottomListener(OnScrollToBottom);
            closeButton.onClick.AddListener(OnCloseLeaderboardClick);
            editNameButton.onClick.AddListener(OnEditNameClick);
        }

        void OnDestroy()
        {
            if (typeNameUI != null)
            {
                typeNameUI.RemoveUpdatedNameListener(OnUpdatedName);
            }
        }

//        void Update()
//        {
//            if (Input.GetKeyDown(KeyCode.Escape))
//            {
//                OnCloseLeaderboardClick();
//            }
//        }

        public void OnCloseLeaderboardClick()
        {
            gameObject.SetActive(false);
        }

        public void OnEditNameClick()
        {
            if (typeNameUI == null)
            {
                typeNameUI = Instantiate(typeNameUIPrefabs);
                typeNameUI.AddUpdatedNameListener(OnUpdatedName);
            }
            string currentName = null;
            if (PlayfabLogin.Instance != null)
            {
                UserAccountInfo info = userAccountInfo;
                if (info != null && info.TitleInfo != null && !string.IsNullOrEmpty(info.TitleInfo.DisplayName))
                {
                    currentName = info.TitleInfo.DisplayName;
                }
            }

            typeNameUI.OpenUI(true, currentName);
        }

        void OnUpdatedName(string pName)
        {
            if (!string.IsNullOrEmpty(pName))
            {
                // update for scroll
                Debug.LogError("Setup new name " + pName);
                string mPlayfabId = myPlayfabId;
                UpdateUserNameData(mPlayfabId, allTimeData, pName);
                UpdateUserNameData(mPlayfabId, todayData, pName);
                UpdateUserNameData(mPlayfabId, thisweekData, pName);

                var curData = GetCurData();
                scroller.SetupData(curData);
            }
        }

        void UpdateUserNameData(string playfabId, List<PlayerLeaderboardEntry> listData, string pUserName)
        {
            foreach (PlayerLeaderboardEntry entry in listData)
            {
                if (entry.PlayFabId.CompareTo(playfabId) == 0)
                {
                    entry.DisplayName = pUserName;
                    break;
                }
            }
        }

        void OnScrollToBottom()
        {
            LoadMoreData();
        }

        void LoadMoreData()
        {
            LeaderboardType curType = curLeaderboardType;
            List<PlayerLeaderboardEntry> curData = GetCurData();

            if (curData.Count > 0)
            {
                PlayfabLeaderboard.Instance.RequestLeaderboard(curType, curData.Count, maxResultCountEachRequest, (list) =>
                    {
                        UpdateLoadedData(curType, curData, list);
                    }, () =>
                    {
                        OnRequestDataError(curType);
                    });
            }
            else
            {
                PlayfabLeaderboard.Instance.RequestLeaderboard(curType, curData.Count, maxResultCountEachRequest, (list) =>
                    {
                        UpdateLoadedData(curType, curData, list);
                    }, () =>
                    {
                        OnRequestDataError(curType);
                    }, (pos, score) =>
                    {
                        if (score > 0)
                        {
                            playerScore[(int)(curType)] = score;
                            playerPosition[(int)(curType)] = pos;
                            playerCell.SetupScore(playerScore[(int)curLeaderboardType], playerPosition[(int)curLeaderboardType]);
                        }
                    });
            }


            // test load
//            StartCoroutine(DOTestLoadData());
        }

        void OnRequestDataError(LeaderboardType targetType)
        {
            if (targetType == curLeaderboardType)
            {
                scroller.OnRequestDataError();
            }
        }


        void UpdateLoadedData(LeaderboardType targetType, List<PlayerLeaderboardEntry> targetData, List<PlayerLeaderboardEntry> data)
        {
            string mPlayfabId = myPlayfabId;
            List<PlayerLeaderboardEntry> listNewEntry = new List<PlayerLeaderboardEntry>();
            foreach (PlayerLeaderboardEntry entry in data)
            {
                if (entry.PlayFabId.CompareTo(mPlayfabId) == 0)
                {
                    entry.DisplayName = "Me: " + entry.DisplayName;
                }

                bool isUpdateData = false;
                for (int i = 0; i < targetData.Count; i++)
                {
                    if (targetData[i].PlayFabId == entry.PlayFabId)
                    {
                        targetData[i] = entry;
                        isUpdateData = true;
                        break;
                    }
                }
                if (!isUpdateData)
                {
                    listNewEntry.Add(entry);
                }
            }
            foreach (PlayerLeaderboardEntry entry in listNewEntry)
            {
                targetData.Add(entry);
            }

            if (targetType == curLeaderboardType)
            {
                scroller.SetupData(targetData);
            }
        }


        IEnumerator DOTestLoadData()
        {
            Debug.LogError("Do load data for type " + curLeaderboardType.ToString());
            LeaderboardType curLoadType = curLeaderboardType;
            List<PlayerLeaderboardEntry> curLoadData = GetCurData();

            yield return new WaitForSeconds(testLoadTime);

            int startId = curLoadData.Count;
            List<PlayerLeaderboardEntry> loadedData = new List<PlayerLeaderboardEntry>();
            for (int i = 0; i < 24; i++)
            {
                PlayerLeaderboardEntry tempEntry = new PlayerLeaderboardEntry();
                tempEntry.DisplayName = curLoadType.ToString() + (startId + i);
                tempEntry.Position = startId + i;
                tempEntry.StatValue = startId + i;
                tempEntry.PlayFabId = "" + (startId + i);

                loadedData.Add(tempEntry);
            }

            for (int i = 0; i < loadedData.Count; i++)
            {
                curLoadData.Add(loadedData[i]);
            }
            if (curLoadType == curLeaderboardType)
            {
                scroller.SetupData(curLoadData);
            }
        }


        void OnLeaderboardChanged(int leaderboardId)
        {
            curLeaderboardType = (LeaderboardType)(leaderboardId);
            ChangeScrollerData();
            ChangePlayerData();
        }

        void ChangeScrollerData()
        {
            var curData = GetCurData();
            scroller.SetupData(curData);

            if (curData.Count == 0)
            {
                LoadMoreData();
            }
        }

        void ChangePlayerData()
        {
            playerCell.SetupScore(playerScore[(int)curLeaderboardType], playerPosition[(int)curLeaderboardType]);
//            PlayfabLeaderboard.Instance.RequestPlayerScore((listScore) =>
//                {
//                    if (listScore.Count > 0)
//                    {
//                        playerScore = listScore;
//                        if ((int)curLeaderboardType < playerScore.Count)
//                        {
//                            playerCell.SetupScore(playerScore[(int)curLeaderboardType]);
//                        }
//                        else
//                        {
//                            playerCell.SetupScore(0);
//                        }
//                    }
//                }, null);
        }


        List<PlayerLeaderboardEntry> GetCurData()
        {
            return (curLeaderboardType == LeaderboardType.AllTime)? allTimeData
                    : ((curLeaderboardType == LeaderboardType.ThisWeek)? thisweekData : todayData);
        }

        void ClearData()
        {
            allTimeData.Clear();
            thisweekData.Clear();
            todayData.Clear();

            ClearPlayerData();
        }

        void ClearPlayerData()
        {
            int nType = System.Enum.GetNames(typeof(LeaderboardType)).Length;
            playerScore.Clear();
            playerPosition.Clear();
            for (int i = 0; i < nType; i++)
            {
                playerScore.Add(0);
                playerPosition.Add(-1);
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
