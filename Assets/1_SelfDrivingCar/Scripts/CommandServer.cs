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
	public CarRemoteControl CarRemoteControl;
	public Camera FrontFacingCamera;
	private CarController _carController;
	private WebSocketServer _webSocket;

	private float _throttle = 0;
	private float _steering = 0;
	private bool _controlUpdated = false;

	// Use this for initialization
	void Start()
	{
		_carController = CarRemoteControl.GetComponent<CarController>();
		_webSocket = new WebSocketServer (4567);
		_webSocket.AddWebSocketService<SocketService>("/simulation", () => new SocketService(this.OnSteer) { IgnoreExtensions = true });
		_webSocket.Start ();
	}

	// Update is called once per frame
	void Update()
	{
		EmitTelemetry();
		UpdateCarControl();
	}

	void UpdateCarControl() {
		if (_controlUpdated) {
			_controlUpdated = false;
			CarRemoteControl.SteeringAngle = _steering;
			if (_carController.CurrentSpeed / _carController.MaxSpeed < _throttle) {
				CarRemoteControl.Acceleration = 0.5f;
			} else {
				CarRemoteControl.Acceleration = 0.0f;
			}
		}
	}

	void OnSteer(float throttle, float steering)
	{
		print("Throttle: " + throttle + " Steering: " + steering);
		this._throttle = throttle;
		this._steering = steering;
		this._controlUpdated = true;
	}

	void EmitTelemetry()
	{
		UnityMainThreadDispatcher.Instance().Enqueue(() =>
		{
			if ((Input.GetKey(KeyCode.W)) || (Input.GetKey(KeyCode.S))) {
				// Manual mode
				Dictionary<string, string> data = new Dictionary<string, string>();
				data["steering_angle"] = _carController.CurrentSteerAngle.ToString("N4");
				data["throttle"] = _carController.AccelInput.ToString("N4");
				data["speed"] = _carController.CurrentSpeed.ToString("N4");
				data["image"] = Convert.ToBase64String(CameraHelper.CaptureFrame(FrontFacingCamera));
				_webSocket.WebSocketServices["/simulation"].Sessions.Broadcast(JsonHandler.FromDictionaryToJson(data));
			}
			else {
				// Autonomous mode
				Dictionary<string, string> data = new Dictionary<string, string>();
				data["steering_angle"] = _carController.CurrentSteerAngle.ToString("N4");
				data["throttle"] = _carController.AccelInput.ToString("N4");
				data["speed"] = _carController.CurrentSpeed.ToString("N4");
				data["image"] = Convert.ToBase64String(CameraHelper.CaptureFrame(FrontFacingCamera));
				_webSocket.WebSocketServices["/simulation"].Sessions.Broadcast(JsonHandler.FromDictionaryToJson(data));
			}
		});
	}

}