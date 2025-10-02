using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RelayManager : MonoBehaviour
{
    // Singleton instance
    private static RelayManager _instance;
    public static RelayManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("RelayManager instance is not available in the scene.");
            }
            return _instance;
        }
    }

    // Private variables
    private Allocation allocation;       // Holds the Relay allocation for host
    private string joinCode;             // Join code for clients
    private string lobbyId;
    private const int maxConnections = 20;
    private const string GameSceneName = "PlayGround3";

    private JoinAllocation joinAllocation; // Holds Relay join allocation for clients
    [SerializeField] TMP_InputField clientJoinCode; // Input field for client to enter join code

    // Ensure Singleton behavior
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject); // Persist instance across scenes
    }

    /// <summary>
    /// Starts the host and sets up the Relay server.
    /// </summary>
    public async Task StartHostAsync()
    {
        // Ensure Unity Services is initialized
        if (!Unity.Services.Core.UnityServices.State.Equals(Unity.Services.Core.ServicesInitializationState.Initialized))
        {
            Debug.LogError("Unity Services is not initialized. Please initialize Unity Services before starting the host.");
            return;
        }

        try
        {
            // Create a Relay allocation for the host
            allocation = await Relay.Instance.CreateAllocationAsync(maxConnections);
            Debug.Log("Relay allocation created.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay allocation failed: {e.Message}");
            return;
        }

        try
        {
            // Generate the join code for clients
            joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Join code generated: {joinCode}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get join code: {e.Message}");
            return;
        }

        // Get NetworkManager and UnityTransport
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager.Singleton is null. Please ensure a NetworkManager exists in the scene.");
            return;
        }

        UnityTransport transport = networkManager.GetComponent<UnityTransport>();
        if (transport == null)
        {
            Debug.LogError("UnityTransport component is missing on NetworkManager. Please attach UnityTransport.");
            return;
        }

        // Set Relay server data
        try
        {
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            transport.SetRelayServerData(relayServerData);
            Debug.Log("Relay server data set successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to set Relay server data: {e.Message}");
            return;
        }

        // Set up the lobby
        try
        {
            var lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "JoinCode", new DataObject(
                            visibility: DataObject.VisibilityOptions.Member,
                            value: joinCode
                        )
                    }
                }
            };

            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync("My Lobby", maxConnections, lobbyOptions);
            lobbyId = lobby.Id;

            StartCoroutine(HeartLobby(15));

            Debug.Log("Lobby created successfully: " + lobbyId);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to create lobby: {ex.Message}");
            Debug.LogException(ex);
        }

        // Start the host and load the game scene
        try
        {
            if (networkManager.StartHost())
            {
                Debug.Log("Host started successfully.");
                networkManager.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogError("Failed to start host.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start host or load scene: {e.Message}");
        }
    }

    private IEnumerator HeartLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    /// <summary>
    /// Starts the client and connects to the Relay server using the provided join code.
    /// </summary>
    public async Task StartClientAsync(string joinCode)
    {
        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.LogError("Join code is empty. Please provide a valid join code.");
            return;
        }

        try
        {
            joinAllocation = await Relay.Instance.JoinAllocationAsync(joinCode);
            Debug.Log("Relay join allocation successful.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to join Relay allocation: {e.Message}");
            return;
        }

        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager.Singleton is null. Please ensure a NetworkManager exists in the scene.");
            return;
        }

        UnityTransport transport = networkManager.GetComponent<UnityTransport>();
        if (transport == null)
        {
            Debug.LogError("UnityTransport component is missing on NetworkManager. Please attach UnityTransport.");
            return;
        }

        try
        {
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            transport.SetRelayServerData(relayServerData);
            Debug.Log("Relay server data set for client.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to set Relay server data for client: {e.Message}");
            return;
        }

        try
        {
            if (networkManager.StartClient())
            {
                Debug.Log("Client started successfully.");
            }
            else
            {
                Debug.LogError("Failed to start client.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error starting client: {e.Message}");
        }
    }

    /// <summary>
    /// Wrapper for starting the host from a UI button.
    /// </summary>
    public async void StartHost()
    {
        await StartHostAsync();
    }

    /// <summary>
    /// Wrapper for starting the client from a UI button.
    /// </summary>
    public async void StartClient()
    {
        await StartClientAsync(clientJoinCode.text);
    }
}
