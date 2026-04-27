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
    [SerializeField] private string joinCodeInput;

    public string nextScene;

    public MainMenuUI mainMenuUI;

    public string JoinCodeInput { get => joinCodeInput; set => joinCodeInput = value; }

    // Make this not delete on load so it can persist across scenes
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        // Must initialize Unity Services once at start
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay()
    {
        try
        {
            // Request allocation for up to 4 players
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Display the code so you can give it to friends
            joinCodeInput = joinCode;

            mainMenuUI.ipField.value = joinCodeInput;

            // Set up Transport
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

            // Subscribe before starting so we catch host disconnects
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
        // If we are a client and the host dropped us
        if (!NetworkManager.Singleton.IsHost && clientId == NetworkManager.ServerClientId)
        {
            LeaveRelay();
        }
    }

    public void LeaveRelay()
    {
        try
        {
            // Unsubscribe to avoid calling this twice
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