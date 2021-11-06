using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoadCheckpointObject : MonoBehaviour
{
    [SerializeField] InputField m_ScoreInputField;
    [SerializeField] Text m_CheckpointOrder;

    [SerializeField] int m_Order = 1;
    [SerializeField] int m_Score = 15;

    public void Setup(int pOrder, int pScore)
    {
        this.m_Order = pOrder;
        this.m_Score = pScore;

        m_CheckpointOrder.text = string.Format("{0}", pOrder);
        m_ScoreInputField.text = string.Format("{0}", pScore);
    }

    public void SetupOrder(int pOrder)
    {
        this.m_Order = pOrder;
        m_CheckpointOrder.text = string.Format("{0}", pOrder);
    }



    public int OrderInPath => m_Order;
    public int Score => m_Score;
}
