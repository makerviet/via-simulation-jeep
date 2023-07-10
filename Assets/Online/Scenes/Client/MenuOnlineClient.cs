using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MapDataLoader;

namespace via.match
{
    public class MenuOnlineClient : MonoBehaviour
    {
        [SerializeField] List<MenuOnlineTeamSlot> slots = new List<MenuOnlineTeamSlot>();
        [SerializeField] Button joinButton;
        [SerializeField] Button outButton;
        [SerializeField] InputField teamId;
        [SerializeField] InputField teamName;
        [SerializeField] Button connectButton;

        void Start()
        {
            InitListener();
        }

        void InitListener()
        {
            joinButton.onClick.AddListener(OnJoinClicked);
            outButton.onClick.AddListener(OnMoveOutClicked);
            connectButton.onClick.AddListener(() =>
            {
                OnlineBattleClient.StartConnect(teamId.text, teamName.text);
            });

            OnlineBattleClient.AddTeamListUpdateListener(OnTeamListUpdated);
            OnlineBattleClient.AddStartBattleListener(OnStartBattle);
            OnlineBattleClient.AddKickoutListener(OnKickedOut);
        }



        void OnTeamListUpdated(TeamListMsg teamListMsg)
        {
            var teamDatas = teamListMsg.playerDatas;
            for (int i = 0; i < slots.Count; i++)
            {
                if (i < teamDatas.Count)
                {
                    slots[i].OnTeamJoin(pName: teamDatas[i].name, pId: teamDatas[i].player_id);
                }
                else
                {
                    slots[i].OnTeamGoOut();
                }
            }
        }

        void OnStartBattle()
        {
            Debug.LogError("OnStart Battle");

            List<MapAsset> fixedMapAssets = MapDataLoader.fixedMapAssets;
            var selectAsset = fixedMapAssets[0];

            Debug.LogError("Save Default Map");
            MapDataLoader.SetInstanceMapData(selectAsset.data, selectAsset.texture);


            Debug.LogError("Load Scene");
            SceneManager.LoadScene("OnlineBattleClient");
        }

        void OnJoinClicked()
        {
            OnlineBattleClient.JoinLobby();
        }

        void OnMoveOutClicked()
        {
            OnlineBattleClient.OutLobby();
        }

        void OnKickedOut()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].OnTeamGoOut();
            }
        }
    }
}