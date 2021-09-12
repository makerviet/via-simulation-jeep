using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Vehicles.Car;

[RequireComponent(typeof(CarController))]
public class CarRemoteControl : MonoBehaviour
{
    public enum CarControlState
    {
        S0_Prepare = 0,
        S1_OnControl = 1,
        S2_OnStop = 22
    }

    private CarController m_Car; // the car controller we want to use

    public Camera FrontFacingCamera;
    [SerializeField] CarControlState m_CarControlState = CarControlState.S0_Prepare;

    public float SteeringAngle { get; set; }
    public float Acceleration { get; set; }
    private Steering s;

    private float _throttle = 0;
    private float _steering = 0;
    private bool _controlUpdated = false;

    private void Awake()
    {
        // get the car controller
        m_Car = GetComponent<CarController>();
        s = new Steering();
        s.Start();
    }

    private void Start()
    {
        CommandServer.RegisterSimulator(this, FrontFacingCamera);
    }

    public void UpdateSteering(float steering, float throttle)
    {
        print("Throttle: " + throttle + " Steering: " + steering);
        this._throttle = throttle;
        this._steering = steering;
        this._controlUpdated = true;
    }

    public void StartControl()
    {
        m_CarControlState = CarControlState.S1_OnControl;
    }

    public void StopControl()
    {
        m_CarControlState = CarControlState.S2_OnStop;
    }

    void UpdateCarControl()
    {
        if (_controlUpdated)
        {
            _controlUpdated = false;
            SteeringAngle = _steering;

            if (m_Car.CurrentSpeed / m_Car.MaxSpeed < _throttle)
            {
                Acceleration = 0.5f;
            }
            else
            {
                Acceleration = 0.0f;
            }
        }
    }

    private void FixedUpdate()
    {
        switch (m_CarControlState)
        {
            case CarControlState.S0_Prepare:
                break;

            case CarControlState.S1_OnControl:
                UpdateStateControl();
                break;

            case CarControlState.S2_OnStop:
                break;
        }
    }

    void UpdateStateControl()
    {
        UpdateCarControl();

        // If holding down W or S control the car manually
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            s.UpdateValues();
            m_Car.Move(s.H, s.V, s.V, 0f);
        }
        else
        {
            m_Car.Move(SteeringAngle, Acceleration, Acceleration, 0f);
        }
    }

    public float CurrentSteerAngle => m_Car.CurrentSteerAngle;
    public float AccelInput => m_Car.AccelInput;
    public float CurrentSpeed => m_Car.CurrentSpeed;
}
