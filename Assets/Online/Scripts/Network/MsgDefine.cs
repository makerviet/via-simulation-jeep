using System;

namespace via.match
{
    public enum MsgDefine
    {
        M00_JoinIn = 0,
        M01_KickOut = 1,
        M02_Disconnect = 2,

        M10_StartBattle = 10,
        M11_UpdatePos = 11,
        M12_PlayerFinished = 12,
        M13_GameOver = 13
    }
}
