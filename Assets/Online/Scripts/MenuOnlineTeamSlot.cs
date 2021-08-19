using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuOnlineTeamSlot : MonoBehaviour
{
    Action<MenuOnlineTeamSlot> OnKickClickListener;

    [SerializeField] bool isKickable = true;
    [SerializeField] Text teamNameText;
    [SerializeField] Button kickButton;

    [SerializeField] string _teamId;

    public string TeamId => _teamId;
    public string TeamName => teamNameText.text;

    void Start()
    {
        InitListener();
        kickButton.gameObject.SetActive(isKickable);
    }

    void InitListener()
    {
        kickButton.onClick.AddListener(OnKickClicked);
    }

    public void AddKickClickListener(Action<MenuOnlineTeamSlot> pListener)
    {
        OnKickClickListener -= pListener;
        OnKickClickListener += pListener;
    }

    public void OnTeamJoin(string pName, string pId)
    {
        this.gameObject.SetActive(true);
        this._teamId = pId;
        teamNameText.text = pName;
    }


    public void OnTeamGoOut()
    {
        this.gameObject.SetActive(false);
    }


    void OnKickClicked()
    {
        OnKickClickListener?.Invoke(this);
    }
}
