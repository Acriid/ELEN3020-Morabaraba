using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    private Button hostButton;
    private Button joinButton;
    private TextField ipField;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        hostButton = root.Q<Button>("hostButton");
        joinButton = root.Q<Button>("joinButton");
        ipField = root.Q<TextField>("ipField");

        hostButton.clicked += OnHostClicked;
        joinButton.clicked += OnJoinClicked;
    }

    private void OnHostClicked()
    {
        Debug.Log("Hosting game...");
        // NetworkManager.StartHost();
    }

    private void OnJoinClicked()
    {
        string ip = ipField.value;
        Debug.Log("Joining: " + ip);
        // NetworkManager.Connect(ip);
    }
}