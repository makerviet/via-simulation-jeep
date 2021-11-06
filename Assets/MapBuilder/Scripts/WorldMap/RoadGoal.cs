using System;
using System.Collections;
using System.Collections.Generic;
using Koi.Common;
using UnityEngine;

public class RoadGoal : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        var car = UMonoUtils.FindComponentFromChild<CarRemoteControl>(other.transform);
        if (car != null)
        {
            car.OnReachGoal(transform, transform.forward);
        }
    }
}
