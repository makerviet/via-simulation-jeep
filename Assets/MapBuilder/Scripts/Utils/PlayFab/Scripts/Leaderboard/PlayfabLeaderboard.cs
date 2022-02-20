using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlayFab.ClientModels;
using PlayFab;

namespace Koi.Playfab
{
    public enum LeaderboardType
    {
        Today = 0,
        ThisWeek = 1,
        AllTime = 2
    }

    public class PlayfabLeaderboard : MonoBehaviour
    {
        private const string SCORE_ALL_TIME_KEY = "score_monthly";
        private const string SCORE_DAILY_KEY = "score_daily";
        private const string SCORE_WEEKLY_KEY = "score_weekly";

        public static PlayfabLeaderboard Instance;

        Dictionary<LeaderboardType, string> statisticKey = new Dictionary<LeaderboardType, string>();
        Dictionary<LeaderboardType, int> statisticVersion = new Dictionary<LeaderboardType, int>();
        Dictionary<LeaderboardType, int> userScoreVersion = new Dictionary<LeaderboardType, int>();
        List<string> listStatisticKey = new List<string>();

        [SerializeField] PlayfabLeaderboardUI leaderboardUIPrefab;

        PlayfabLeaderboardUI leaderboardUI;

        bool onLoggedIn = false;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitData();
            }
            else
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
        }

        void Start()
        {
            PlayfabLogin.Instance.AddLoginSuccessListner(OnLoginSuccess);
        }

        void OnLoginSuccess()
        {
            // request to get Version data of leaderboard.
            RequestLeaderboard(LeaderboardType.Today, 0, 10, null, null);
            RequestLeaderboard(LeaderboardType.ThisWeek, 0, 10, null, null);
            RequestLeaderboard(LeaderboardType.AllTime, 0, 10, null, null);

            onLoggedIn = true;
        }

        void InitData()
        {
            statisticKey.Add(LeaderboardType.AllTime, SCORE_ALL_TIME_KEY);
            statisticKey.Add(LeaderboardType.Today, SCORE_DAILY_KEY);
            statisticKey.Add(LeaderboardType.ThisWeek, SCORE_WEEKLY_KEY);

            listStatisticKey.Add(SCORE_ALL_TIME_KEY);
            listStatisticKey.Add(SCORE_DAILY_KEY);
            listStatisticKey.Add(SCORE_WEEKLY_KEY);

            statisticVersion.Add(LeaderboardType.AllTime, 0);
            statisticVersion.Add(LeaderboardType.Today, 0);
            statisticVersion.Add(LeaderboardType.ThisWeek, 0);

            userScoreVersion.Add(LeaderboardType.AllTime, 0);
            userScoreVersion.Add(LeaderboardType.Today, 0);
            userScoreVersion.Add(LeaderboardType.ThisWeek, 0);
        }

        void SetupStatisticVersion(LeaderboardType type, int version)
        {
            if (statisticVersion.ContainsKey(type))
            {
                statisticVersion[type] = version;
            }
            else {
                statisticVersion.Add(type, version);
            }
        }

        [ContextMenu("Show Default Leaderboard UI")]
        public void ShowDefaultLeaderboardUI()
        {
            if (PlayfabLogin.Instance.isLoggedIn)
            {
                if (leaderboardUI == null)
                {
                    leaderboardUI = Instantiate(leaderboardUIPrefab);
                }
                leaderboardUI.gameObject.SetActive(true);
            }
        }

        public void HideDefaultLeaderboardUI()
        {
            if (leaderboardUI != null)
            {
                leaderboardUI.gameObject.SetActive(true);
            }
        }

        public static void PostScore(int score)
        {
            if (Instance != null)
            {
                Instance.DoPostScore(score);
            }
        }

        public void DoPostScore(int score)
        {
            if (!onLoggedIn)
            {
                return;
            }

            UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest();
            request.Statistics = new List<StatisticUpdate>();
            foreach (string key in listStatisticKey)
            {
                StatisticUpdate statisticUpdate = new StatisticUpdate();
                statisticUpdate.StatisticName = key;
                statisticUpdate.Value = score;
                request.Statistics.Add(statisticUpdate);
            }

            PlayFabClientAPI.UpdatePlayerStatistics(request, (result) =>
                {
                    Debug.LogError("PlayfabLeaderboard: pos score success " + JsonUtility.ToJson(result));
                }, (error) =>
                {
                    Debug.LogError("PlayfabLeaderboard: pos score error " + JsonUtility.ToJson(error));
                });
        }

        void PostScoreByStatistic(string statisticName, int score)
        {
            
        }

        public void RequestLeaderboard(LeaderboardType type, int startPosition, int maxResultCount, Action<List<PlayerLeaderboardEntry>> pLeaderboardResult, Action pError, Action<int, int> playerRankResult = null)
        {
            string statisticName = statisticKey[type];
            GetLeaderboardRequest request = new GetLeaderboardRequest();
            request.StatisticName = statisticName;
            request.StartPosition = startPosition;
            request.MaxResultsCount = maxResultCount;

            PlayFabClientAPI.GetLeaderboard(request, (result) =>
                {
//                    Debug.LogError("PlayfabLeaderboard: result = " + LitJson.JsonMapper.ToJson(result));
                    if (pLeaderboardResult != null)
                    {
                        pLeaderboardResult(result.Leaderboard);
                    }
                    SetupStatisticVersion(type, result.Version);
                }, (errorCallback) =>
                {
                    pError();
                });

            if (playerRankResult != null)
            {
                GetLeaderboardAroundPlayerRequest playerRankRequest = new GetLeaderboardAroundPlayerRequest();
                playerRankRequest.MaxResultsCount = 1;
                playerRankRequest.StatisticName = statisticName;
                playerRankRequest.Version = statisticVersion[type];
                PlayFabClientAPI.GetLeaderboardAroundPlayer(playerRankRequest, (result) =>
                    {
                        if (result.Leaderboard != null && result.Leaderboard.Count > 0
                            && result.Version == statisticVersion[type])
                        {
                            var playerEntry = result.Leaderboard[0];
                            playerRankResult(playerEntry.Position, playerEntry.StatValue);
                        }
                        Debug.LogError("Rank result = " + JsonUtility.ToJson(result));
                    }, (error) =>
                    {
                    });
            }
        }

        public void RequestPlayerScore(Action<List<int>> pResult, Action pError)
        {
            GetPlayerStatisticsRequest request = new GetPlayerStatisticsRequest();
            request.StatisticNames = new List<string>();
            int numberType = Enum.GetNames(typeof(LeaderboardType)).Length;
            for (int i = 0; i < numberType; i++)
            {
                request.StatisticNames.Add(statisticKey[(LeaderboardType)(i)]);
            }

            PlayFabClientAPI.GetPlayerStatistics(request, (statisticResult) =>
                {
                    Debug.LogError("PlayfabLeaderboard: playser stattistics = " + JsonUtility.ToJson(statisticResult));
                    if (pResult != null)
                    {
                        List<int> listScore = new List<int>();
                        for (int i = 0; i < numberType; i++)
                        {
                            LeaderboardType type = (LeaderboardType)(i);
                            string key = statisticKey[type];
                            bool exist = false;
                            foreach (StatisticValue pValue in statisticResult.Statistics)
                            {
                                if (pValue.StatisticName.CompareTo(key) == 0)
                                {
                                    if (pValue.Version >= statisticVersion[type])
                                    {
                                        userScoreVersion[type] = (int)pValue.Version;
                                        listScore.Add(pValue.Value);
                                    }
                                    else
                                    {
                                        listScore.Add(0);
                                    }
                                    exist = true;
                                    break;
                                }
                            }
                            if (!exist)
                            {
                                listScore.Add(0);
                            }
                        }
                        pResult(listScore);
                    }
                }, (error) =>
                {
                    if (pError != null)
                    {
                        pError();
                    }
                });
        }


        public static void PostPlay(int nMax)
        {
            if (IsServiceAvailable)
            {
                Instance.DoPostPlay(nMax);
            }
        }

        void DoPostPlay(int nMax)
        {
            PostStatistic(new List<string> { "n_game_play", "n_game_play_daily", "n_game_play_v2" }, new List<int> {1, 1, nMax});
        }

        public static void PostPlaySurvival()
        {
            if (IsServiceAvailable)
            {
                Instance.DoPostPlaySurvival();
            }
        }

        void DoPostPlaySurvival()
        {
            PostStatistic(new List<string> { "n_game_play_survival", "n_game_play_survival_daily" },
                new List<int> {1, 1});
        }

        public static void PostPlayWin(int nMax)
        {
            if (IsServiceAvailable)
            {
                Instance.DoPostPlayWin(nMax);
            }
        }

        void DoPostPlayWin(int nMax)
        {
            PostStatistic(new List<string> { "n_game_play_win", "n_game_play_daily_win", "n_game_play_win_v2" }, new List<int> { 1, 1, nMax});
        }

        public static void PostPlayLose(int nMax)
        {
            if (IsServiceAvailable)
            {
                Instance.DoPostPlayLose(nMax);
            }
        }

        void DoPostPlayLose(int nMax)
        {
            PostStatistic(new List<string> { "n_game_play_lose", "n_game_play_daily_lose", "n_game_play_lose_v2" }, new List<int> { 1, 1, nMax});
        }


        public static void PostUserLevel(int level)
        {
            if (IsServiceAvailable)
            {
                Instance.DoPostUserLevel(level);
            }
        }

        void DoPostUserLevel(int level)
        {
            PostStatistic("max_level", level);
        }


        public static void PostUserTrackingData(int maxLevel, int nDay, int nDailyGift, int maxDailyGiftInRow)
        {
            if (IsServiceAvailable)
            {
                Instance.DoPostUserTrackingData(maxLevel, nDay, nDailyGift, maxDailyGiftInRow);
            }
        }

        void DoPostUserTrackingData(int maxLevel, int nDay, int nDailyGift, int maxDailyGiftInRow)
        {
            PostStatistic(new List<string> {
                "max_level",
                "n_day",
                "n_daily_gift",
                "n_daily_gift_inrow"
            }, new List<int> {
                maxLevel, nDay, nDailyGift, maxDailyGiftInRow });
        }


        public static void PostShowRewardedVideo()
        {
            if (IsServiceAvailable)
            {
                Instance.DoPostShowRewardedVideo();
            }
        }

        void DoPostShowRewardedVideo()
        {
            PostStatistic(new List<string> { "n_ads_show", "n_ads_show_daily" }, new List<int> { 1, 1});
        }

        public static void PostShowInter()
        {
            if (IsServiceAvailable)
            {
                Instance.DoPostShowInter();
            }
        }

        void DoPostShowInter()
        {
            PostStatistic(new List<string> { "n_inter_show", "n_inter_show_daily" }, new List<int> { 1, 1 });
        }


        void PostStatistic(string statisticName, int value)
        {
            UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest();
            request.Statistics = new List<StatisticUpdate>();
            StatisticUpdate statisticUpdate = new StatisticUpdate();
            statisticUpdate.StatisticName = statisticName;
            statisticUpdate.Value = value;
            request.Statistics.Add(statisticUpdate);

            PlayFabClientAPI.UpdatePlayerStatistics(request, (result) =>
            {
            }, (error) =>
            {
            });
        }

        void PostStatistic(List<string> statisticNames, List<int> values)
        {
            if (!onLoggedIn)
            {
                return;
            }

            UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest();
            request.Statistics = new List<StatisticUpdate>();
            for (int i = 0; i < statisticNames.Count; i++)
            {
                StatisticUpdate statisticUpdate = new StatisticUpdate();
                statisticUpdate.StatisticName = statisticNames[i];
                statisticUpdate.Value = values[i];
                request.Statistics.Add(statisticUpdate);
            }

            PlayFabClientAPI.UpdatePlayerStatistics(request, (result) =>
            {
            }, (error) =>
            {
            });
        }

        public static bool IsServiceAvailable
        {
            get
            {
                return Instance != null && PlayfabLogin.Instance.isLoggedIn;
            }
        }
    }
}
