using System;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    [SerializeField] private string joinCodeInput;
    public string nextScene;
    public MainMenuUI mainMenuUI;
    public string JoinCodeInput { get => joinCodeInput; set => joinCodeInput = value; }

    private void Awake()
    {
        // If an instance already exists and it's not this one, destroy this duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        // Guard so this doesn't run again on a surviving instance
        if (UnityServices.State == ServicesInitializationState.Initialized) return;

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCodeInput = joinCode;
            mainMenuUI.ipField.value = joinCodeInput;

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
            NetworkManager.Singleton.StartHost();
            SceneManager.LoadScene(nextScene);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Create Error: {e}");
        }
    }

    public async void JoinRelay()
    {
        try
        {
            string code = mainMenuUI.ipField.value;
            if (string.IsNullOrEmpty(code)) return;

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
            NetworkManager.Singleton.StartClient();
            SceneManager.LoadScene(nextScene);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Join Error: {e}");
        }
    }

    private void OnDisconnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsHost && clientId == NetworkManager.ServerClientId)
        {
            LeaveRelay();
        }
    }

    public void LeaveRelay()
    {
        try
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;
            }

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }

            SceneManager.LoadScene("MainMenu");
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Leave Error: {e}");
        }
    }
}