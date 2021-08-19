using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

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

        public List<PlayerData> onlinePlayers = new List<PlayerData>();
        public List<PlayerData> offlinePlayers = new List<PlayerData>();

        [SerializeField] MatchState m_state = MatchState.S0_Prepare;

        public void OnStartServer()
        {
            m_state = MatchState.S1_Lobby;
        }

        private void Awake()
        {
            
        }

        void InitListener()
        {

        }
    }
}