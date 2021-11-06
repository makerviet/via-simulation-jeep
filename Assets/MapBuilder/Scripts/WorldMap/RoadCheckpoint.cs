﻿using System.Collections;
using System.Collections.Generic;
using Koi.Common;
using UnityEngine;

public class RoadCheckpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var car = UMonoUtils.FindComponentFromChild<CarRemoteControl>(other.transform);
        if (car != null)
        {
            car.OnReachCheckPoint(this);
            gameObject.SetActive(false);
        }
    }
}
