using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class HudUI : MonoBehaviour
{
    private Button exitButton;
    private Label joinCodeLabel;
    private RelayManager relayManager;
    public JoinCodeSO JoinCode;


    private void OnEnable()
    {

        relayManager = FindAnyObjectByType<RelayManager>();


        var root = GetComponent<UIDocument>().rootVisualElement;
        root.Focus();

        var doContainer = root.Q<VisualElement>(className: "undo-container");


        var codeContainer = root.Q<VisualElement>(className: "code-container");

        if (codeContainer == null)
        {
            Debug.LogError("code-container not found");
        }

        exitButton = root.Q<Button>("ExitButton");
        // exitButton = codeContainer.Q<Button>("ExitButton");
        joinCodeLabel = codeContainer.Q<Label>("Code");



        if (joinCodeLabel != null)
        {
            joinCodeLabel.text = "Join Code : " + JoinCode.JoinCode;
        }

        if (exitButton == null)
        {
            Debug.LogWarning("Exit button not found in UI.");
            return;
        }
        exitButton.clicked += OnExitClicked;
    }

    private void OnExitClicked()
    {
        Debug.Log("Exiting lobby");
        relayManager.LeaveRelay();
        joinCodeLabel.text = "";


    }

    public void SetJoinCode(string code)
    {

    }

}
