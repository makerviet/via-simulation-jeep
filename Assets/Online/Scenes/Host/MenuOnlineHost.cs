using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOnlineHost : MonoBehaviour
{
    Action<string> OnTeamKickedListener;

    [SerializeField] List<MenuOnlineTeamSlot> slots = new List<MenuOnlineTeamSlot>();

    void Start()
    {
        InitListener();
    }

    void InitListener()
    {
        foreach (var slot in slots)
        {
            slot.AddKickClickListener(OnKickedTeam);
        }
    }

    public void AddTeamKickedListener(Action<string> pListener)
    {
        OnTeamKickedListener -= pListener;
        OnTeamKickedListener += pListener;
    }

    void OnKickedTeam(MenuOnlineTeamSlot pTeam)
    {
        pTeam.OnTeamGoOut();

        OnTeamKickedListener?.Invoke(pTeam.TeamId);
    }

    public void OnTeamJoin(string teamId, string teamName)
    {
        MenuOnlineTeamSlot slot = FindAvailableSlot();
        if (slot != null)
        {
            slot.OnTeamJoin(pName: teamName, pId: teamId);
        }
    }


    MenuOnlineTeamSlot FindAvailableSlot()
    {
        foreach (var slot in slots)
        {
            if (!slot.gameObject.activeInHierarchy)
            {
                slot.gameObject.SetActive(true);
                return slot;
            }
        }
        return null;
    }
}
