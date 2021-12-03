using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityStandardAssets.Vehicles.Car;
using System;
using System.Text;  
using System.Security.AccessControl;
using NativeWebSocket;

public class CommandServer : MonoBehaviour
{
	private static CommandServer Instance;

    #region socket
    private static WebSocket _webSocket = null;
	long _nextBoundTime = 0;
    #endregion

    #region control on simulator
	[Header("Debug")]
    public Camera FrontFacingCamera;
		public CarRemoteControl CarRemoteControl;
    #endregion

	private void Awake()
	{
	if (Instance == null)
	{
		Instance = this;
		GameObject.DontDestroyOnLoad(this.gameObject);
	}
	else
	{
		GameObject.DestroyImmediate(this.gameObject);
	}
	}


	void InitWebSocket() {
		// Create WebSocket instance
		_webSocket = WebSocketFactory.CreateInstance("ws://127.0.0.1:4567");

		// Add OnOpen event listener
		_webSocket.OnOpen += () => {
			Debug.Log("WS connected!");
			Debug.Log("WS state: " + _webSocket.State.ToString());	
		};

		// Add OnMessage event listener
		_webSocket.OnMessage += (byte[] msg) => {
			try {
				// Debug.Log("WS received message: " + Encoding.UTF8.GetString(msg));
				string receivedString = Encoding.UTF8.GetString(msg);
				Dictionary<string, string> data = JsonHandler.FromJsonToDictionary(receivedString);
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
				OnSteer(throttle, steering);
			} catch (Exception ex) {
				Debug.Log(ex.ToString());
			}
		};

		// Add OnError event listener
		_webSocket.OnError += (string errMsg) => {
			Debug.Log("WS error: " + errMsg);
		};

		// Add OnClose event listener
		_webSocket.OnClose += (WebSocketCloseCode code) => {
			Debug.Log("WS closed with code: " + code.ToString());
		};

		// Connect to the server
		_webSocket.Connect();
	}

	// Use this for initialization
	void Start() {
		InitWebSocket();
	}

	public static void RegisterSimulator(CarRemoteControl pCarRemoteController, Camera pFrontFacingCamera)
	{
		Instance.DoRegisterSimulator(pCarRemoteController, pFrontFacingCamera);
	}

	public static void OnSimulatorDestroyed(CarRemoteControl pCarRemoteController)
	{
		Instance.DoRemoveSimulator(pCarRemoteController);
	}

	void DoRemoveSimulator(CarRemoteControl pCarRemoteController)
	{
		if (this.CarRemoteControl == pCarRemoteController)
		{
			this.CarRemoteControl = null;
			FrontFacingCamera = null;
		}
	}

	void DoRegisterSimulator(CarRemoteControl pCarRemoteController, Camera pFrontFacingCamera)
	{
		this.CarRemoteControl = pCarRemoteController;
		this.FrontFacingCamera = pFrontFacingCamera;
	}

	// Update is called once per frame
	void Update()
	{
		#if !UNITY_WEBGL || UNITY_EDITOR
			_webSocket.DispatchMessageQueue();
		#endif
		EmitTelemetry();
	}

	void OnSteer(float throttle, float steering)
	{
		if (IsOnSimulating)
		{
			CarRemoteControl.UpdateSteering(steering: steering, throttle: throttle);
		}
	}

	void EmitTelemetry()
	{
		if (!IsOnSimulating)
		{
			return;
		}

		// Return if waiting for the next status of websocket
		if (DateTimeOffset.Now.ToUnixTimeMilliseconds() < _nextBoundTime) {
			return;
		}

		// If websocket is closed, reconnect
		// Debug.Log(_webSocket.State.ToString());
		if (_webSocket.State == WebSocketState.Closed) {
			Debug.Log("Trying to reconnect...");
			try {
				_webSocket.Connect();
			} catch (Exception ex) {
				InitWebSocket();
				Debug.Log(ex.ToString());
			}
			
			_nextBoundTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 1000; // Wait to connect
			return;
		} else if (_webSocket.State != WebSocketState.Open) {
			// If websocket is in waiting states (Opening, Closing), 
			// skip to wait it to go to Closed
			// Debug.Log("Waiting for the next websocket state...");
			_nextBoundTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 500; // Wait to connect
			return;
		}

		UnityMainThreadDispatcher.Instance().Enqueue(() =>
		{
			// if ((Input.GetKey(KeyCode.W)) || (Input.GetKey(KeyCode.S))) {}
			Dictionary<string, string> data = new Dictionary<string, string>();
			data["steering_angle"] = CarRemoteControl.CurrentSteerAngle.ToString("N4");
			data["throttle"] = CarRemoteControl.AccelInput.ToString("N4");
			data["speed"] = CarRemoteControl.CurrentSpeed.ToString("N4");
			data["image"] = Convert.ToBase64String(CameraHelper.CaptureFrame(FrontFacingCamera));
			try {
				byte[] bytes = Encoding.UTF8.GetBytes(JsonHandler.FromDictionaryToJson(data));  
				_webSocket.Send(bytes);
			}
				catch (Exception ex)
			{
				Debug.Log(ex.ToString());       
				Debug.Log("Sent failed. Closing to reconnect...");
				_webSocket.Close();
				_nextBoundTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 500;
			}
		});
	}


	bool IsOnSimulating => (CarRemoteControl != null && FrontFacingCamera != null);
}