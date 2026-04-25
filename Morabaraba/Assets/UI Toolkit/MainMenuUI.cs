using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    private Button hostButton;
    private Button joinButton;
    [SerializeField] public TextField ipField;
    private Label joinCode;
    public RelayManager relayManager;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        hostButton = root.Q<Button>("hostButton");
        joinButton = root.Q<Button>("joinButton");
        ipField = root.Q<TextField>("ipField");
        joinCode = root.Q<Label>("joinCode");

        hostButton.clicked += OnHostClicked;
        joinButton.clicked += OnJoinClicked;
    }

    private void OnHostClicked()
    {
        Debug.Log("Hosting game...");
        relayManager.CreateRelay();
        // NetworkManager.StartHost();
        // SceneManager.LoadScene("Lobby");
    }

    private void OnJoinClicked()
    {
        string ip = ipField.value;
        Debug.Log("Joining: " + ip);
        relayManager.JoinRelay();
        // NetworkManager.Connect(ip);
        // SceneManager.LoadScene("Lobby");
    }
}