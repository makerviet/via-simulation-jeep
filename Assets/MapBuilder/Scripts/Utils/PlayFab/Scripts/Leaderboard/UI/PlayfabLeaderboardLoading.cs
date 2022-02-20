using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Koi.Playfab
{
    public class PlayfabLeaderboardLoading : MonoBehaviour
    {

        [SerializeField] Text loadingText;
        [SerializeField] List<string> listString = new List<string>();
        [SerializeField] float frameDuration = 0.75f;

        float timeRemain = 0;
        int curId = 0;

        void Update()
        {
            timeRemain -= Time.deltaTime;
            if (timeRemain <= 0)
            {
                timeRemain = frameDuration;
                curId = (curId + 1) % Mathf.Max(1, listString.Count);
                loadingText.text = listString[curId];
            }
        }
    }
}
