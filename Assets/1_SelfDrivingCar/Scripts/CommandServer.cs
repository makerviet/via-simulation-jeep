using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using WebSocketSharp;
using WebSocketSharp.Server;
using UnityStandardAssets.Vehicles.Car;
using System;
using System.Security.AccessControl;

public class CommandServer : MonoBehaviour
{
	private static CommandServer Instance;

    #region socket
    private static WebSocketServer _webSocket;
    #endregion

    #region control on simulator
	[Header("Debug")]
    public Camera FrontFacingCamera;
	public CarRemoteControl CarRemoteControl;
	//private CarController _carController;
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
    void Start()
	{
		//_carController = CarRemoteControl.GetComponent<CarController>();
		if (_webSocket == null) {
			_webSocket = new WebSocketServer (4567);
			_webSocket.AddWebSocketService<SocketService>("/simulation", () => new SocketService(this.OnSteer) { IgnoreExtensions = true });
			_webSocket.Start();
		}
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

		UnityMainThreadDispatcher.Instance().Enqueue(() =>
		{
			if ((Input.GetKey(KeyCode.W)) || (Input.GetKey(KeyCode.S))) {
				// Manual mode
				Dictionary<string, string> data = new Dictionary<string, string>();
				data["steering_angle"] = CarRemoteControl.CurrentSteerAngle.ToString("N4");
				data["throttle"] = CarRemoteControl.AccelInput.ToString("N4");
				data["speed"] = CarRemoteControl.CurrentSpeed.ToString("N4");
				data["image"] = Convert.ToBase64String(CameraHelper.CaptureFrame(FrontFacingCamera));
				_webSocket.WebSocketServices["/simulation"].Sessions.Broadcast(JsonHandler.FromDictionaryToJson(data));
			}
			else {
				// Autonomous mode
				Dictionary<string, string> data = new Dictionary<string, string>();
				data["steering_angle"] = CarRemoteControl.CurrentSteerAngle.ToString("N4");
				data["throttle"] = CarRemoteControl.AccelInput.ToString("N4");
				data["speed"] = CarRemoteControl.CurrentSpeed.ToString("N4");
				data["image"] = Convert.ToBase64String(CameraHelper.CaptureFrame(FrontFacingCamera));
				_webSocket.WebSocketServices["/simulation"].Sessions.Broadcast(JsonHandler.FromDictionaryToJson(data));
			}
		});
	}


	bool IsOnSimulating => (CarRemoteControl != null && FrontFacingCamera != null);
}