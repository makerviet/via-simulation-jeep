using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityStandardAssets.Vehicles.Car;
using System;
using System.Text;  
using System.Security.AccessControl;
using HybridWebSocket;

public class CommandServer : MonoBehaviour
{
	private static CommandServer Instance;

    #region socket
    private static WebSocket _webSocket = null;
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

	// Use this for initialization
	void Start() {

		// Create WebSocket instance
		_webSocket = WebSocketFactory.CreateInstance("ws://127.0.0.1:4567");

		// Add OnOpen event listener
		_webSocket.OnOpen += () => {
				Debug.LogWarning("WS connected!");
				Debug.LogWarning("WS state: " + _webSocket.GetState().ToString());	
		};

		// Add OnMessage event listener
		_webSocket.OnMessage += (byte[] msg) => {
			try {
				// Debug.LogWarning("WS received message: " + Encoding.UTF8.GetString(msg));
				string receivedString = Encoding.UTF8.GetString(msg);
				Debug.Log("Received message! - " + receivedString);
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
				Debug.LogWarning(ex.ToString());
			}
		};

		// Add OnError event listener
		_webSocket.OnError += (string errMsg) => {
				Debug.LogWarning("WS error: " + errMsg);
		};

		// Add OnClose event listener
		_webSocket.OnClose += (WebSocketCloseCode code) => {
				Debug.LogWarning("WS closed with code: " + code.ToString());
		};

		// Connect to the server
		_webSocket.Connect();
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

		if (_webSocket.GetState() == WebSocketState.Closed) {
			_webSocket.Connect();
			return;
		} else if (_webSocket.GetState() != WebSocketState.Open) {
			// If websocket is in waiting states (Opening, Closing), 
			// skip to wait it to go to Closed
			return;
		}

		UnityMainThreadDispatcher.Instance().Enqueue(() =>
		{
			if ((Input.GetKey(KeyCode.W)) || (Input.GetKey(KeyCode.S))) {
				// Manual mode
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
					Debug.LogWarning(ex.ToString());
					Debug.LogWarning("Sent failed. Trying to reconnect....");
					_webSocket.Close();
				}
			}
			else {
				// Autonomous mode
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
					Debug.LogWarning(ex.ToString());       
					Debug.LogWarning("Sent failed. Trying to reconnect....");
					_webSocket.Close();
				}
			}
		});
	}


	bool IsOnSimulating => (CarRemoteControl != null && FrontFacingCamera != null);
}