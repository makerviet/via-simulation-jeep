using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

namespace via.match
{
    public class NetworkManager : MonoBehaviour
    {
        public enum NetworkState
        {
            S0_Idle = 0,
            S1_Connecting = 1,
            S2_Connected = 2
        }

        //{"type": string, "data": {"id": string, "data": json}}
        [System.Serializable]
        public class MessageData
        {
            public string type;     // normal_send or "disconnected"
            public string data;
        }

        [System.Serializable]
        public class CoreData
        {
            public string id;       // user_id
            public string data;     // data of battle message
        }

        [System.Serializable]
        public class BattleMsgData
        {
            public int battle_channel = 0;
            public string user_id;       // user_id
            public MsgDefine msg_define = MsgDefine.M00_JoinIn;
            public string data;     // data of battle message

            public bool IsOnBattleChannel => battle_channel == BATTLE_CHANNEL;
        }

        const string SERVER_URL = "ws://188.166.221.68/ws/socket";
        static int BATTLE_CHANNEL = 1;

        static NetworkManager Instance;
        static Action OnConnectedListener;
        static Action<BattleMsgData> OnReceivedMsgListener;
        static Action OnDisconectedListener;

        WebSocket ws;
        [SerializeField] NetworkState m_state = NetworkState.S0_Idle;


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
                return;
            }
        }

        private void Start()
        {
            UnityThread.initUnityThread();
        }

        public static void Connect(string user_id)
        {
            if (IsExist)
            {
                Instance.DoConnect(user_id);
            }
        }

        private void OnDestroy()
        {
            DoStopConnect();
        }

        void DoConnect(string user_id)
        {
            DoStopConnect();

            ws = new WebSocket(SERVER_URL);
            ws.SetCookie(new WebSocketSharp.Net.Cookie("user_name", user_id));

            ws.OnMessage -= OnMessage;
            ws.OnMessage += OnMessage;

            ws.OnOpen -= OnOpen;
            ws.OnOpen += OnOpen;
            ws.Connect();
        }

        private void Update()
        {
            // auto ping
        }

        public static void AddReceiveMsgListener(Action<BattleMsgData> pListener)
        {
            OnReceivedMsgListener -= pListener;
            OnReceivedMsgListener += pListener;
        }

        void OnMessage(object sender, MessageEventArgs e)
        {
            Debug.LogError("Receive data: " + e.Data);

            string jsonData = e.Data;
            MessageData messageData = JsonUtility.FromJson<MessageData>(jsonData);
            if (messageData != null)
            {
                CoreData coreData = JsonUtility.FromJson<CoreData>(messageData.data);
                if (coreData != null)
                {
                    if (messageData.type.CompareTo("disconnected") == 0)
                    {
                        // an other player disconnected
                        BattleMsgData battleMsgData = new BattleMsgData()
                        {
                            user_id = coreData.id,
                            msg_define = MsgDefine.M02_Disconnect//,
                            //data = coreData.data
                        };
                        OnReceivedMsgListener?.Invoke(battleMsgData);
                    }
                    else
                    {
                        BattleMsgData battleMsgData = JsonUtility.FromJson<BattleMsgData>(coreData.data);
                        if (battleMsgData != null && battleMsgData.IsOnBattleChannel)
                        {
                            Debug.LogError("Msg On channel");
                            OnReceivedMsgListener?.Invoke(battleMsgData);
                        }
                    }
                }
            }
        }


        public static void AddConnectedListener(Action pListener)
        {
            OnConnectedListener -= pListener;
            OnConnectedListener += pListener;
        }

        void OnOpen(object sender, EventArgs e)
        {
            Debug.LogError("OnOpen data: " + e.ToString());
            OnConnectedListener?.Invoke();
        }

        public static void StopConnect()
        {
            if (IsExist)
            {
                Instance.DoStopConnect();
            }
        }

        void DoStopConnect()
        {
            try
            {
                if (ws != null)
                {
                    ws.OnMessage -= OnMessage;
                    ws.OnOpen -= OnOpen;
                    ws.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Stop connect error: " + ex.Message);
            }
        }

        public static void SendMessage(string user_id, MsgDefine msgDefine, string msg_data)
        {
            if (IsExist)
            {
                Instance.DoSendMessage(user_id, msgDefine, msg_data);
            }
        }

        void DoSendMessage(string user_id, MsgDefine msgDefine, string msg_data)
        {
            MessageData data = new MessageData();
            data.type = "battle";

            BattleMsgData battleMsgData = new BattleMsgData();
            battleMsgData.battle_channel = BATTLE_CHANNEL;
            battleMsgData.user_id = user_id;
            battleMsgData.msg_define = msgDefine;
            battleMsgData.data = msg_data;

            CoreData coreData = new CoreData();
            coreData.id = user_id;
            coreData.data = JsonUtility.ToJson(battleMsgData);

            data.data = JsonUtility.ToJson(coreData);
            string jsonData = JsonUtility.ToJson(data);
            ws.Send(jsonData);
            //Debug.LogError("SendData: json = " + jsonData);
        }


        static bool IsExist => Instance != null;
        public static bool IsConnected => IsExist ? Instance.mIsConnected : false;
        public static bool CanConnectNow => IsExist ? Instance.mCanConnectNow : false;

        bool mIsConnected => m_state == NetworkState.S2_Connected;
        bool mCanConnectNow => m_state == NetworkState.S0_Idle;
    }
}
