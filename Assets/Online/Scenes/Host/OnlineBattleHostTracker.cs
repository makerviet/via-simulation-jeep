using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace via.match
{
    public class OnlineBattleHostTracker : MonoBehaviour
    {
        public enum BattleState
        {
            S0_Loading = 0,
            S1_Running = 1,
            S2_Finished = 2
        }

        [SerializeField] AutonomousMiniMap m_MiniMap;
        [SerializeField] WorldMapBuilder m_MapBuilder;
        [SerializeField] Transform m_Car;

        [SerializeField] BattleState m_BattleState = BattleState.S0_Loading;

        [SerializeField] float lastTrackedTime = 0;

        void Start()
        {
            OnlineBattleHost.AddPlayerPosUpdateListener(OnCallPositionUpdated);
        }

        void SetupMiniMap()
        {
            m_MiniMap.SetupMapData(m_MapBuilder.mapWidth, m_MapBuilder.mapHeight, m_MapBuilder.posRoot);
            m_MiniMap.SetupPlayerData(OnlineBattleHost.JoinedPlayers);
        }

        public void OnCallPositionUpdated(string pPlayerId, PlayerPoseMsg pos)
        {
            Debug.LogError("On Host receive player pos update " + pos.pos.pos.ToString());
            m_MiniMap.OnCallPositionUpdated(pPlayerId, pos.pos.pos);
        }

        private void Update()
        {
            switch (m_BattleState)
            {
                case BattleState.S0_Loading:
                    UpdateLoading();
                    break;

                case BattleState.S1_Running:
                    UpdateRunning();
                    break;

                case BattleState.S2_Finished:
                    UpdateFinished();
                    break;
            }
        }


        void UpdateLoading()
        {
            float waitTime = Time.realtimeSinceStartup - OnlineBattleClient.TimeStartBattle;
            if (waitTime >= 5)
            {
                m_BattleState = BattleState.S1_Running;
            }

            if (m_MapBuilder.IsLoadedMap && !m_MiniMap.IsHavePlayerDatas)
            {
                SetupMiniMap();
            }
        }

        void UpdateRunning()
        {
            //if (lastTrackedTime + 1 < Time.realtimeSinceStartup)
            //{
            //    lastTrackedTime = Time.realtimeSinceStartup;
            //}
        }

        void UpdateFinished()
        {
        }
    }
}