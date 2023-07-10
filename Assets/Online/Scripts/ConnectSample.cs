using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class ConnectSample : MonoBehaviour
{
    WebSocket ws;

    //{"type": string, "data": {"id": string, "data": json}}
    [System.Serializable]
    public class MessageData
    {
        public string type;
        public string data;
    }

    [System.Serializable]
    public class CoreData
    {
        public string id;
        public string data;
    }


    
    [SerializeField] InputField socketUrlText;
    [SerializeField] InputField user_id;
    [SerializeField] InputField msg_data;


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Connect();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            StopConnect();
        }
    }

    public void Connect()
    {
        string url = socketUrlText.text;
        ws = new WebSocket(url);
        ws.SetCookie(new WebSocketSharp.Net.Cookie("user_name", user_id.text));// "Stupid_Wizard"));

        //ws.OnMessage += (sender, e) =>
        //{
        //    Debug.LogError("Receive data: " + e.Data);
        //};
        ws.OnMessage -= OnMessage;
        ws.OnMessage += OnMessage;

        //ws.OnOpen += (sender, e) => {
        //    Debug.LogError("OnOpen data: " + e.ToString());
        //};
        ws.OnOpen -= OnOpen;
        ws.OnOpen += OnOpen;
        ws.Connect();
    }

    void OnMessage(object sender, MessageEventArgs e)
    {
        Debug.LogError("Receive data: " + e.Data);
    }

    void OnOpen(object sender, EventArgs e)
    {
        Debug.LogError("OnOpen data: " + e.ToString());
    }

    public void StopConnect()
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

    public void DoSendMessage()
    {
        MessageData data = new MessageData();
        data.type = "test_send";
        CoreData coreData = new CoreData();
        coreData.id = user_id.text;
        coreData.data = msg_data.text;
        data.data = JsonUtility.ToJson(coreData);
        string jsonData = JsonUtility.ToJson(data);
        ws.Send(jsonData);
        Debug.LogError("SendData: json = " + jsonData);
    }
}
