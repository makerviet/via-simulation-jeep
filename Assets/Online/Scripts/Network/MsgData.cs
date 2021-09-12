using System;
using System.Collections.Generic;

namespace via.match
{
    [System.Serializable]
    public class PlayerMsg
    {
        public string name;
    }

    [System.Serializable]
    public class PlayerPoseMsg : PlayerMsg
    {
        public PositionData pos;
    }

    [System.Serializable]
    public class TeamListMsg
    {
        public List<PlayerData> playerDatas;
    }
}