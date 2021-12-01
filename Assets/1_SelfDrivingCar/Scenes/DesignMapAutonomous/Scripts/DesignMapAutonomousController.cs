using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesignMapAutonomousController : MonoBehaviour
{
    [SerializeField] CarRemoteControl mCar;

    private void Start()
    {
        StartCoroutine(DoCountDownToStart());
    }

    IEnumerator DoCountDownToStart()
    {
        for (int i = 3; i >= 1; i--)
        {
            // 
            //yield return new WaitForSeconds(1);
        }
        yield return null;

        mCar.StartControl();
    }
}
