using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyPlayersText;

    private LobbyList lobbyList;
    private Lobby lobby;
    public void Initialise(LobbyList lobbyList, Lobby lobby)
    {
        this.lobbyList = lobbyList;
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
        lobbyPlayersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void Join()
    {
        lobbyList.JoinAsync(lobby);
    }
}
