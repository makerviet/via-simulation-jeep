using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MapDataLoader;

namespace via.match
{

    public class MenuOnlineHost : MonoBehaviour
    {
        Action<string> OnTeamKickedListener;

        [SerializeField] List<MenuOnlineTeamSlot> slots = new List<MenuOnlineTeamSlot>();
        [SerializeField] Button startButton;

        void Start()
        {
            InitListener();
        }

        void InitListener()
        {
            foreach (var slot in slots)
            {
                slot.AddKickClickListener(OnKickedTeam);
            }

            startButton.onClick.AddListener(OnStartBattle);

            OnlineBattleHost.AddPlayerJoinLobbyListener(OnTeamJoin);
            OnlineBattleHost.AddPlayerOutLobbyListener(OnTeamOut);
        }

        public void AddTeamKickedListener(Action<string> pListener)
        {
            OnTeamKickedListener -= pListener;
            OnTeamKickedListener += pListener;
        }

        void OnStartBattle()
        {
            OnlineBattleHost.StartBattle();

            Debug.LogError("OnStart Battle");

            List<MapAsset> fixedMapAssets = MapDataLoader.fixedMapAssets;
            var selectAsset = fixedMapAssets[0];

            Debug.LogError("Save Default Map");
            MapDataLoader.SetInstanceMapData(selectAsset.data, selectAsset.texture);


            Debug.LogError("Load Scene");
            SceneManager.LoadScene("OnlineBattleHost");
        }

        void OnKickedTeam(MenuOnlineTeamSlot pTeam)
        {
            pTeam.OnTeamGoOut();

            OnlineBattleHost.KickTeam(pTeam.TeamId);
            OnTeamKickedListener?.Invoke(pTeam.TeamId);
        }

        void OnTeamJoin(PlayerData playerData)
        {
            Debug.LogError("On Team Join " + JsonUtility.ToJson(playerData));
            MenuOnlineTeamSlot slot = FindAvailableSlot();
            if (slot != null)
            {
                Debug.LogError("Exist slot");
                slot.OnTeamJoin(pName: playerData.name, pId: playerData.player_id);
            }
            else
            {
                Debug.LogError("Dont have slot");
            }
        }

        void OnTeamOut(PlayerData pPlayerData)
        {
            foreach (var slot in slots)
            {
                if (slot.gameObject.activeInHierarchy
                    && slot.TeamId.CompareTo(pPlayerData.player_id) == 0)
                {
                    slot.gameObject.SetActive(false);
                }
            }
        }


        MenuOnlineTeamSlot FindAvailableSlot()
        {
            foreach (var slot in slots)
            {
                if (!slot.gameObject.activeInHierarchy)
                {
                    slot.gameObject.SetActive(true);
                    return slot;
                }
            }
            return null;
        }
    }
}