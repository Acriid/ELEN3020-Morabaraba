using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;

public class SetStatus : NetworkBehaviour

{

    [SerializeField] private TMP_InputField ipAddressField = null;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private TextMeshProUGUI lobbyCode;
    [SerializeField] private UnityTransport unityTransport;
    public void setAsHost()
    {
        NetworkManager networkManager = FindAnyObjectByType<NetworkManager>();
        networkManager.StartHost();
        this.enabled = false;
        hostButton.interactable = false;
        lobbyCode.text = "127.0.0.1";
    }
    public void setAsClient()
    {
        NetworkManager networkManager = FindAnyObjectByType<NetworkManager>();
        string ipAddress = ipAddressField.text;
        networkManager.StartClient();
        clientButton.interactable = false;
    }
}
