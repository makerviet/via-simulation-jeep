using System;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Text;


public class SocketService : WebSocketBehavior
{
        
    private string     _name;
    private static int _number = 0;
    private string     _prefix;
    private Action<float, float> _onMsgCallback = null;

    public SocketService () {
        _prefix = "anon#";
    }

    public SocketService (Action<float, float> onMsgCallback)
    {
        _onMsgCallback = onMsgCallback;
        _prefix = "anon#";   
    }

    public SocketService (Action<float, float> onMsgCallback, string prefix)
    {
        _prefix = !prefix.IsNullOrEmpty () ? prefix : "anon#";
    }

    private string getName ()
    {
        var name = Context.QueryString["name"];
        return !name.IsNullOrEmpty () ? name : _prefix + getNumber ();
    }

    private static int getNumber ()
    {
        return Interlocked.Increment (ref _number);
    }

    protected override void OnClose (CloseEventArgs e)
    {
        Sessions.Broadcast (String.Format ("{0} got logged off...", _name));
    }

    protected override void OnMessage (MessageEventArgs e)
    {

        string receivedString = Encoding.UTF8.GetString(e.RawData);
        Debug.Log("Received message! - " + receivedString);
        Dictionary<string, string> data = JsonHandler.FromJsonToDictionary(receivedString);
        if (_onMsgCallback != null) {
            if (!data.ContainsKey("throttle")) {
                Debug.Log("Missing throttle value in message!");
                return;
            }
            if (!data.ContainsKey("steering")) {
                Debug.Log("Missing steering value in message!");
                return;
            }
            float throttle = float.Parse(data["throttle"]);
            float steering = float.Parse(data["steering"]);
            _onMsgCallback(throttle, steering);
        }
        
    }

    protected override void OnOpen ()
    {
        Console.WriteLine("Open new connection");
        _name = getName ();
    }
}