using System;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    [SerializeField] private string joinCodeInput;
    public string nextScene;
    public MainMenuUI mainMenuUI;
    public string JoinCodeInput { get => joinCodeInput; set => joinCodeInput = value; }
    public bool canJoin = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe once here so both host and client paths are covered
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
    }

    public async Task<bool> JoinRelay(string code)
    {
        try
        {
            if (string.IsNullOrEmpty(code))
                return false;

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();

            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Join Error: {e}");
            return false;
        }
    }

    public async Task<bool> CreateRelay()
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

            // Wait for the server to be ready before loading the scene
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.StartHost();
            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Create Error: {e}");
            return false;
        }
    }


    async void Start()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized) return;

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }



    private void OnServerStarted()
    {
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;

        // Use Netcode's scene manager so in-scene NetworkObjects get spawned properly
        NetworkManager.Singleton.SceneManager.LoadScene(nextScene, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }



    public void ChangeScene()
    {
        SceneManager.LoadScene(nextScene);
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
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;

            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            Destroy(NetworkManager.Singleton.gameObject);
        }

        // Use Netcode's scene manager so in-scene NetworkObjects get spawned properly
        NetworkManager.Singleton.SceneManager.LoadScene("MainMenu", UnityEngine.SceneManagement.LoadSceneMode.Single);


    }
}