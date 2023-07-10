using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using static via.match.NetworkManager;

namespace via.match
{
    public class OnlineBattleHost : MonoBehaviour
    {
        public enum MatchState
        {
            S0_Prepare = 0,
            S1_Lobby = 1,
            S2_BattleLoading = 2,
            S3_OnBattle = 3,
            S4_OnResult = 4
        }

        const string HOST_USERNAME = "via_host";

        static OnlineBattleHost Instance;

        // menu
        static Action<PlayerData> OnPlayerJoinLobbyListener;
        static Action<PlayerData> OnPlayerOutLobbyListener;
        static Action<PlayerData> OnPlayerRunningDisconectListener;

        static Action<string, PlayerPoseMsg> OnPlayerPosMsgUpdateListener;

        [Header("Debug")]
        public List<PlayerData> onlinePlayers = new List<PlayerData>();
        public List<PlayerData> offlinePlayers = new List<PlayerData>();

        [SerializeField] MatchState m_state = MatchState.S0_Prepare;
        [SerializeField] float m_timeStartBattle;

        public static float TimeStartBattle => IsExist ? Instance.m_timeStartBattle : 0;
        public static List<PlayerData> JoinedPlayers => IsExist ? Instance.onlinePlayers : new List<PlayerData>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                GameObject.DontDestroyOnLoad(gameObject);
            }
            else
            {
                GameObject.DestroyImmediate(gameObject);
            }
        }



        private void Start()
        {
            InitListener();
            StartHost();
        }

        public void StartHost()
        {
            m_state = MatchState.S1_Lobby;
            if (NetworkManager.CanConnectNow && !NetworkManager.IsConnected)
            {
                NetworkManager.Connect(HOST_USERNAME);
            }
        }

        void InitListener()
        {
            NetworkManager.AddReceiveMsgListener(OnReceiveMsg);
        }

        public static void AddPlayerJoinLobbyListener(Action<PlayerData> pListener)
        {
            OnPlayerJoinLobbyListener -= pListener;
            OnPlayerJoinLobbyListener += pListener;
        }

        public static void AddPlayerOutLobbyListener(Action<PlayerData> pListener)
        {
            OnPlayerOutLobbyListener -= pListener;
            OnPlayerOutLobbyListener += pListener;
        }

        public static void AddPlayerPosUpdateListener(Action<string, PlayerPoseMsg> pListener)
        {
            OnPlayerPosMsgUpdateListener -= pListener;
            OnPlayerPosMsgUpdateListener += pListener;
        }


        public static void StartBattle()
        {
            if (Instance != null)
            {
                Instance.DoStartBattle();
            }
        }

        void DoStartBattle()
        {
            m_state = MatchState.S2_BattleLoading;
            // wait all player ready, or timeout
            NetworkManager.SendMessage(HOST_USERNAME, MsgDefine.M10_StartBattle, "");
            m_state = MatchState.S3_OnBattle;
            m_timeStartBattle = Time.realtimeSinceStartup;
        }


        public static void KickTeam(string teamId)
        {
            if (Instance != null)
            {
                Instance.DoKickTeam(teamId);
            }
        }

        void DoKickTeam(string teamId)
        {
            PlayerData playerData = PlayerOnlineOf(teamId);
            if (playerData != null)
            {
                onlinePlayers.Remove(playerData);
            }

            //string jsonData = JsonUtility.ToJson(playerData);
            //NetworkManager.SendMessage(HOST_USERNAME, MsgDefine.M01_KickOut, jsonData);
            OnUpdateTeamList();
        }


        void OnReceiveMsg(BattleMsgData msgData)
        {
            switch (m_state)
            {
                case MatchState.S1_Lobby:
                    ProcessLobbyMsg(msgData);
                    break;

                case MatchState.S3_OnBattle:
                    ProcessBattleMsg(msgData);
                    break;
            }
        }


        void ProcessLobbyMsg(BattleMsgData msgData)
        {
            Debug.LogError("Progress Lobby msg");
            switch (msgData.msg_define)
            {
                case MsgDefine.M00_JoinIn:
                    Debug.LogError("Progress Lobby msg join");
                    ProcessLobbyPlayerJoin(msgData.user_id, msgData.data);
                    break;

                case MsgDefine.M02_Disconnect:
                    ProcessLobbyPlayerDisconnect(msgData.user_id);
                    break;
            }
        }

        void ProcessLobbyPlayerJoin(string user_id, string jsonData)
        {
            if (!IsPlayerJoined(user_id))
            {
                PlayerMsg msg = JsonUtility.FromJson<PlayerMsg>(jsonData);
                PlayerData playerData = new PlayerData()
                {
                    name = msg.name,
                    player_id = user_id
                };
                onlinePlayers.Add(playerData);
                UnityThread.executeInUpdate(() =>
                {
                    OnPlayerJoinLobbyListener?.Invoke(playerData);
                });

                OnUpdateTeamList();
            }
        }

        void ProcessLobbyPlayerDisconnect(string user_id)
        {
            PlayerData playerData = PlayerOnlineOf(user_id);
            if (playerData != null)
            {
                UnityThread.executeInUpdate(() =>
                {
                    OnPlayerOutLobbyListener?.Invoke(playerData);
                });
                onlinePlayers.Remove(playerData);
                OnUpdateTeamList();
            }
        }


        void OnUpdateTeamList()
        {
            TeamListMsg teamListMsg = new TeamListMsg() {
                playerDatas = onlinePlayers
            };
            string json = JsonUtility.ToJson(teamListMsg);
            NetworkManager.SendMessage(HOST_USERNAME, MsgDefine.M03_UpdateTeamList, json);
        }



        void ProcessBattleMsg(BattleMsgData msgData)
        {
            switch (msgData.msg_define)
            {
                case MsgDefine.M11_UpdatePos:
                    UpdatePlayerPos(msgData.user_id, msgData.data);
                    break;

                case MsgDefine.M12_PlayerFinished:
                    UpdatePlayerFinished(msgData.user_id, msgData.data);
                    break;
            }
        }

        void UpdatePlayerPos(string userId, string json)
        {
            Debug.LogError("Msg update player Host");
            PlayerData playerData = PlayerOnlineOf(userId);
            if (playerData != null)
            {
                PlayerPoseMsg poseMsg = JsonUtility.FromJson<PlayerPoseMsg>(json);
                playerData.UpdateAddPoseData(poseMsg.pos);

                UnityThread.executeInUpdate(() =>
                {
                    OnPlayerPosMsgUpdateListener?.Invoke(userId, poseMsg);
                });
            }
        }

        void UpdatePlayerFinished(string userId, string json)
        {
            PlayerData playerData = PlayerOnlineOf(userId);
            if (playerData != null)
            {
                PlayerPoseMsg poseMsg = JsonUtility.FromJson<PlayerPoseMsg>(json);
                playerData.UpdateAddPoseData(poseMsg.pos);

                UnityThread.executeInUpdate(() =>
                {
                    OnPlayerPosMsgUpdateListener?.Invoke(userId, poseMsg);
                });
            }
        }










        PlayerData PlayerOnlineOf(string userId)
        {
            foreach (PlayerData playerData in onlinePlayers)
            {
                if (playerData.player_id.CompareTo(userId) == 0)
                {
                    return playerData;
                }
            }
            return null;
        }

        bool IsPlayerJoined(string userId)
        {
            return PlayerOnlineOf(userId) != null;
        }

        static bool IsExist => Instance != null;
    }
}