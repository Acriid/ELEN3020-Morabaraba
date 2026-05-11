using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    private Button hostButton;
    private Button joinButton;
    private Button exitButton;
    private Label feedback;
    [SerializeField] public TextField ipField;
    private Label joinCode;
    public RelayManager relayManager;
    [SerializeField] private HudUI hudUI;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        hostButton = root.Q<Button>("hostButton");
        joinButton = root.Q<Button>("joinButton");
        exitButton = root.Q<Button>("exitButton");

        ipField = root.Q<TextField>("ipField");
        joinCode = root.Q<Label>("Code");
        feedback = root.Q<Label>("Feedback");

        hostButton.clicked += OnHostClicked;
        joinButton.clicked += OnJoinClicked;
        exitButton.clicked += () => Application.Quit();


    }

    private async void OnHostClicked()
    {
        feedback.text = "Creating Lobby...";

        hostButton.SetEnabled(false);
        joinButton.SetEnabled(false);

        bool success = await relayManager.CreateRelay();

        if (success)
        {
            feedback.text = "Lobby Created!";
            // joinCode.text = $"Join Code: {relayManager.JoinCodeInput}";
            hudUI.SetJoinCode(relayManager.JoinCodeInput);
        }
        else
        {
            feedback.text = "Failed To Create Lobby";

            hostButton.SetEnabled(true);
            joinButton.SetEnabled(true);
        }
    }

    private async void OnJoinClicked()
    {
        string joinCode = ipField.value.Trim();

        if (string.IsNullOrEmpty(joinCode))
        {
            feedback.text = "Enter A Join Code";
            return;
        }

        feedback.text = "Joining Lobby...";

        hostButton.SetEnabled(false);
        joinButton.SetEnabled(false);

        bool success = await relayManager.JoinRelay(joinCode);

        if (!success)
        {
            feedback.text = "Invalid Join Code";

            hostButton.SetEnabled(true);
            joinButton.SetEnabled(true);
            return;
        }

        feedback.text = $"Joined Lobby: {joinCode}";
        hudUI.SetJoinCode(joinCode);

       
    }
}