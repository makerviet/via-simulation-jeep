using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static via.match.NetworkManager;
using static via.match.OnlineBattleHost;

namespace via.match
{
    public class OnlineBattleClient : MonoBehaviour
    {
        //public enum MatchState
        //{
        //    S0_Prepare = 0,
        //    S1_Lobby = 1,
        //    S2_BattleLoading = 2,
        //    S3_OnBattle = 3,
        //    S4_OnResult = 4
        //}

        static OnlineBattleClient Instance;

        static Action<TeamListMsg> OnTeamListUpdateListener;
        static Action OnKickoutListener;
        static Action OnStartBattleListener;

        static Action<string, PlayerPoseMsg> OnPlayerPosMsgUpdateListener;

        public List<PlayerData> onlinePlayers = new List<PlayerData>();

        [SerializeField] MatchState m_state = MatchState.S0_Prepare;
        [SerializeField] string m_user_id;
        [SerializeField] string m_user_name;

        [SerializeField] float m_timeStartBattle;

        public static float TimeStartBattle => IsExist ? Instance.m_timeStartBattle : 0;
        public static List<PlayerData> JoinedPlayers => IsExist ? Instance.onlinePlayers : new List<PlayerData>();
        public static string UserId => IsExist ? Instance.m_user_id : "";

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
        }

        void InitListener()
        {
            NetworkManager.AddReceiveMsgListener(OnReceiveMsg);
        }

        public static void AddTeamListUpdateListener(Action<TeamListMsg> pListener)
        {
            OnTeamListUpdateListener -= pListener;
            OnTeamListUpdateListener += pListener;
        }

        public static void AddKickoutListener(Action pListener)
        {
            OnKickoutListener -= pListener;
            OnKickoutListener += pListener;
        }

        public static void AddStartBattleListener(Action pListener)
        {
            OnStartBattleListener -= pListener;
            OnStartBattleListener += pListener;
        }

        public static void AddPlayerPosUpdateListener(Action<string, PlayerPoseMsg> pListener)
        {
            OnPlayerPosMsgUpdateListener -= pListener;
            OnPlayerPosMsgUpdateListener += pListener;
        }

        public static void StartConnect(string user_id, string user_name)
        {
            if (IsExist)
            {
                Instance.DoStartConnect(user_id, user_name);
            }
        }

        void DoStartConnect(string user_id, string user_name)
        {
            m_user_id = user_id;
            m_user_name = user_name;
            m_state = MatchState.S1_Lobby;
            if (NetworkManager.CanConnectNow && !NetworkManager.IsConnected)
            {
                NetworkManager.Connect(user_id);
            }
        }


        public static void JoinLobby()
        {
            if (IsExist)
            {
                Instance.DoJoinLobby();
            }
        }

        void DoJoinLobby()
        {
            PlayerMsg msg = new PlayerMsg()
            {
                name = m_user_name
            };
            string jsonData = JsonUtility.ToJson(msg);
            NetworkManager.SendMessage(m_user_id, MsgDefine.M00_JoinIn, jsonData);
        }


        public static void OutLobby()
        {
            if (IsExist)
            {
                Instance.DoOutLobby();
            }
        }

        void DoOutLobby()
        {
            NetworkManager.SendMessage(m_user_id, MsgDefine.M02_Disconnect, "");
        }


        public static void UpdatePos(Vector3 pPosition)
        {
            UpdatePos(Time.realtimeSinceStartup - TimeStartBattle, pPosition);
        }


        public static void UpdatePos(float pTime, Vector3 pPosition)
        {
            if (IsExist)
            {
                Instance.DoUpdatePos(pTime, pPosition);
            }
        }

        void DoUpdatePos(float pTime, Vector3 pPosition)
        {
            PlayerPoseMsg msg = new PlayerPoseMsg()
            {
                pos = new PositionData()
                {
                    time = pTime,
                    pos = pPosition
                }
            };
            string jsonData = JsonUtility.ToJson(msg);
            NetworkManager.SendMessage(m_user_id, MsgDefine.M11_UpdatePos, jsonData);
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
            switch (msgData.msg_define)
            {
                case MsgDefine.M03_UpdateTeamList:
                    OnUpdateTeamList(msgData.data);
                    break;

                case MsgDefine.M10_StartBattle:
                    OnStartBattle();
                    break;
            }
        }

        void OnUpdateTeamList(string json)
        {
            TeamListMsg teamListMsg = JsonUtility.FromJson<TeamListMsg>(json);
            onlinePlayers = teamListMsg.playerDatas;

            bool exist = false;
            foreach (var player in onlinePlayers)
            {
                if (player.player_id.CompareTo(m_user_id) == 0)
                {
                    exist = true;
                    break;
                }
            }

            UnityThread.executeInUpdate(() =>
            {
                if (exist)
                {
                    OnTeamListUpdateListener?.Invoke(teamListMsg);
                }
                else
                {
                    onlinePlayers = new List<PlayerData>();
                    OnKickoutListener?.Invoke();
                }
            });
        }

        void OnStartBattle()
        {
            UnityThread.executeInUpdate(() =>
            {
                m_timeStartBattle = Time.realtimeSinceStartup;
                m_state = MatchState.S3_OnBattle;
                Debug.LogError("On Start Battle Msg");
                OnStartBattleListener?.Invoke();
            });
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


        static bool IsExist => Instance != null;
    }
}