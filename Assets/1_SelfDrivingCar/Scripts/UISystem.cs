using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityStandardAssets.Vehicles.Car;
using UnityEngine.SceneManagement;

public class UISystem : MonoSingleton<UISystem> {

    public CarController carController;
    public string GoodCarStatusMessage;
    public string BadSCartatusMessage;
    public Text MPH_Text;
    public Image MPH_Animation;
    public Text Angle_Text;
	public Text DriveStatus_Text;
	public Text SaveStatus_Text;
	public bool isTraining = false;

    private float topSpeed;


    // Use this for initialization
    void Start() {
		Debug.Log (isTraining);
        topSpeed = carController.MaxSpeed;
		DriveStatus_Text.text = "";
		SetAngleValue(0);
        SetMPHValue(0);
		if (!isTraining) {
			DriveStatus_Text.text = "Mode: Autonomous";
		} 
    }

    public void SetAngleValue(float value)
    {
        Angle_Text.text = value.ToString("N2") + "°";
    }

    public void SetMPHValue(float value)
    {
        MPH_Text.text = (value * 1.609344).ToString("N2");
        //Do something with value for fill amounts
        MPH_Animation.fillAmount = value/topSpeed;
    }
	
    void UpdateCarValues()
    {
        SetMPHValue(carController.CurrentSpeed * carController.MaxSpeed);
        SetAngleValue(carController.CurrentSteerAngle);
    }

	// Update is called once per frame
	void Update () {

		if (carController.getSaveStatus ()) {
			SaveStatus_Text.text = "Capturing Data: " + (int)(100 * carController.getSavePercent ()) + "%";
		} 

		if (!isTraining) 
		{
			if ((Input.GetKey(KeyCode.W)) || (Input.GetKey(KeyCode.S))) 
			{
				DriveStatus_Text.color = Color.red;
				DriveStatus_Text.text = "Mode: Manual";
			} 
			else 
			{
				DriveStatus_Text.color = Color.white;
				DriveStatus_Text.text = "Mode: Autonomous";
			}
		}

	    if(Input.GetKeyDown(KeyCode.Escape))
        {
            //Do Menu Here
            SceneManager.LoadScene("MenuScene");
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            //Do Console Here
        }

        UpdateCarValues();
    }
}
