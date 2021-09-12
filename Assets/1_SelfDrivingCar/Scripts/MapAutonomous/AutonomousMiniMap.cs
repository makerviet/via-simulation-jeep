using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using via.match;

public class AutonomousMiniMap : MonoBehaviour
{
    [System.Serializable]
    public class PlayerPointer
    {
        public string name;
        public string player_id;
        public Image image;
        public Vector2 pos;

        public void Active(string name, string id)
        {
            image.gameObject.SetActive(true);
            this.name = name;
            this.player_id = id;
        }

        public void Disable()
        {
            image.gameObject.SetActive(false);
        }
    }

    [SerializeField] RawImage miniMap;
    [SerializeField] List<PlayerPointer> players = new List<PlayerPointer>();

    [SerializeField] float mapWidth;
    [SerializeField] float mapHeight;
    [SerializeField] Vector3 rootPos;

    List<PlayerData> playerDatas = new List<PlayerData>();

    public bool IsHavePlayerDatas => playerDatas.Count > 0;


    private void Start()
    {
        
    }

    public void SetupMapData(float mapWidth, float mapHeight, Vector3 posRoot)
    {
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.rootPos = posRoot;
    }

    public void SetupPlayerData(List<PlayerData> pPlayerDatas)
    {
        for (int i = 0; i < pPlayerDatas.Count; i++)
        {
            var data = pPlayerDatas[i];
            players[i].Active(data.name, data.player_id);
        }
        for (int i = pPlayerDatas.Count; i < players.Count; i++)
        {
            players[i].Disable();
        }
    }

    public void OnCallPositionUpdated(string pPlayerId, PlayerPoseMsg pos)
    {
        OnCallPositionUpdated(pPlayerId, pos.pos.pos);
    }

    public void OnCallPositionUpdated(string pPlayerId, Vector3 pos)
    {
        PlayerPointer pointer = PointerOf(pPlayerId);
        if (pointer != null)
        {
            UpdatePointer(pointer, pos);
        }
    }

    void UpdatePointer(PlayerPointer pointer, Vector3 pos)
    {
        Vector3 offset = pos - rootPos;
        float xRatio = offset.x / mapWidth;
        float yRatio = offset.z / mapHeight;
        float pX = xRatio * miniMap.rectTransform.sizeDelta.x;
        float pY = yRatio * miniMap.rectTransform.sizeDelta.y;
        pointer.image.rectTransform.anchoredPosition = new Vector2(pX, pY);
    }

    PlayerPointer PointerOf(string playerId)
    {
        foreach (var pointer in players)
        {
            if (pointer.player_id.CompareTo(playerId) == 0)
            {
                return pointer;
            }
        }
        return null;
    }
}
