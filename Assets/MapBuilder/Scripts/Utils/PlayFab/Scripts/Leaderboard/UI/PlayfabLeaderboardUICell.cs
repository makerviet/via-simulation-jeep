using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using System.Globalization;

namespace Koi.Playfab
{
    public class PlayfabLeaderboardUICell : MonoBehaviour
    {
        [SerializeField] bool isMedal = false;
        [SerializeField] Text nameText;
        [SerializeField] Text scoreText;
        [SerializeField] Text positionText;
        [SerializeField] int nameLengthMax = 14;
        [SerializeField] public Image highLight;

        PlayerLeaderboardEntry m_data;
        public PlayerLeaderboardEntry data { get { return m_data; } }

        [SerializeField] RectTransform m_RectTransform;
        public RectTransform rectTransform { get { return m_RectTransform; } }

        public void SetupName(string nameString)
        {
            if (string.IsNullOrEmpty(nameString))
            {
                return;
            }
            nameText.text = (nameString.Length <= nameLengthMax || !isMedal) ?
                nameString : (string.Format("{0}...", nameString.Substring(0, nameLengthMax - 3)));
        }

        public void SetupData(PlayerLeaderboardEntry playerEntry, bool needHighlight)
        {
            this.m_data = playerEntry;
            if (highLight != null)
            {
                highLight.enabled = needHighlight;
            }

            string nameString = playerEntry.DisplayName;
            if (string.IsNullOrEmpty(playerEntry.DisplayName))
            {
//                nameString = playerEntry.PlayFabId;
                nameString = "NoName_" + playerEntry.PlayFabId.Substring(0, Mathf.Min(2, playerEntry.PlayFabId.Length)) + "...";
            }
            nameText.text = (nameString.Length <= nameLengthMax || !isMedal) ?
                nameString : (string.Format("{0}...", nameString.Substring(0, nameLengthMax - 3)));
            scoreText.text = playerEntry.StatValue.ToString("N0", new CultureInfo("en-US", false));
            if (positionText != null)
            {
                positionText.text = "" + (playerEntry.Position + 1);
            }
        }

        public void SetupScore(int score, int pos = -1)
        {
            scoreText.text = score.ToString("N0", new CultureInfo("en-US", false));
            if (positionText != null)
            {
                if (pos >= 0)
                {
                    positionText.text = "" + (pos + 1);
                }
                else
                {
                    positionText.text = "***";
                }
            }
        }

//        public int debugScore;
//        public string debugName;
//
//        [ContextMenu("TestSetupData")]
//        void TestSetupData()
//        {
//            PlayerLeaderboardEntry testData = new PlayerLeaderboardEntry();
//            testData.DisplayName = debugName;
//            testData.Position = 10;
//            testData.StatValue = debugScore;
//            SetupData(testData);
//        }
    }
}
