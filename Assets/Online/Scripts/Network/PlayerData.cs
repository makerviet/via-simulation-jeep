using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace via.match
{
    [System.Serializable]
    public class PlayerData
    {
        public string name;
        public string player_id;
        public Vector2 current_pos;
        public List<PositionData> positionDatas = new List<PositionData>();
    }

    [System.Serializable]
    public class PositionData
    {
        public float time;
        public Vector2 pos;
    }
}