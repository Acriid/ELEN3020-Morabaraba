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


    private string nextScene = "Morabaraba PVP";
    private MainMenuUI mainMenuUI;

    [SerializeField] private string selectedBoardType;
    private string joinCodeInput;
    public string JoinCodeInput { get => joinCodeInput; set => joinCodeInput = value; }
    public bool canJoin = false;

    private bool servicesInitialized = false;
    public JoinCodeSO joinCodeSO;
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
            Invoke(nameof(ChangeScene), 1f); // Delay the scene change to allow feedback to be seen

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
        if (mainMenuUI == null)
        {
            mainMenuUI = FindFirstObjectByType<MainMenuUI>();
        }

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCodeSO.JoinCode = joinCode;

            if (mainMenuUI == null)
            {
                Debug.LogError("MainMenuUI not found!");
                return false;
            }

            if (mainMenuUI.ipField == null)
            {
                Debug.LogError("ipField is null!");
                return false;
            }

            // mainMenuUI.ipField.value = joinCodeInput;



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
        mainMenuUI = FindFirstObjectByType<MainMenuUI>();

        if (UnityServices.State == ServicesInitializationState.Initialized) return;

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        servicesInitialized = true;
        Debug.Log("Unity Services Initialized");
    }


    public void SetBoardType(string boardType)
    {
        selectedBoardType = boardType;

        if (selectedBoardType == "Six Men's Morris")
        {
            nextScene = "Six-Mens PVP";
        }
        else if (selectedBoardType == "Nine Men's Morris")
        {
            nextScene = "Nine-Mens PVP";
        }
        else
        {
            nextScene = "Morabaraba PVP";
        }

        Debug.Log("Selected Scene: " + nextScene);

    }

    private void OnServerStarted()
    {
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        Debug.Log(nextScene + " is loading...");
        // Use Netcode's scene manager so in-scene NetworkObjects get spawned properly
        NetworkManager.Singleton.SceneManager.LoadScene(nextScene, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void ChangeScene()
    {
        Debug.Log("Loading Scene: " + nextScene);

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
        NetworkManager.Singleton.SceneManager.LoadScene("Setup", UnityEngine.SceneManagement.LoadSceneMode.Single);


    }
}