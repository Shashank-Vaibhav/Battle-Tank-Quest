using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyList : MonoBehaviour
{
    [SerializeField] private Transform lobbyItemParent;
    [SerializeField] private LobbyItem lobbyItemPrefab;
    private bool isJoining;
    private bool isRefreshing;

    private void OnEnable()
    {
        RefreshList();
    }

    public async void JoinAsync(Lobby lobby)
    {
        if (isJoining) { return; }
        isJoining = true;
        
        try
        {
            Lobby joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joincode = joiningLobby.Data["JoinCode"].Value;

            await RelayManager.Instance.StartClientAsync(joincode);
        }
        catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
        isJoining = false;
    }


    public async void RefreshList()
    {
       if(isRefreshing) { return; }

        isRefreshing = true;
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"),
                new QueryFilter(
                    field: QueryFilter.FieldOptions.IsLocked,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0")
            };
            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            foreach(Transform child in lobbyItemParent)
            {
                Destroy(child.gameObject);
            }
            foreach(Lobby lobby in lobbies.Results )
            {
                LobbyItem lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
                lobbyItem.Initialise(this, lobby);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }

        isRefreshing = false;
    }
}
