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
        
        public List<PositionData> pos_datas = new List<PositionData>();

        public void UpdateAddPoseData(PositionData poseData)
        {
            pos_datas.Add(poseData);
        }

        public Vector3 current_pos => (pos_datas.Count > 0) ? pos_datas[pos_datas.Count - 1].pos : Vector3.zero;
    }

    //[System.Serializable]
    //public class PlayerPositionData
    //{
    //    public string name;
    //    public string player_id;
    //    public Vector2 pos;
    //}

    [System.Serializable]
    public class PositionData
    {
        public float time;
        public Vector3 pos;
    }
}