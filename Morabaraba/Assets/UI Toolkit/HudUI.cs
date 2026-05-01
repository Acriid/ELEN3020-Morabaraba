using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class HudUI : MonoBehaviour
{

    private Button exitButton;
    private Label joinCodeLabel;
    private RelayManager relayManager;

    private void OnEnable()
    {
        DontDestroyOnLoad(gameObject.transform.parent);
        relayManager = FindAnyObjectByType<RelayManager>();

        var root = GetComponent<UIDocument>().rootVisualElement;

        exitButton = root.Q<Button>("ExitButton");
        joinCodeLabel = root.Q<Label>("Code");

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
        if (joinCodeLabel != null)
        {
            joinCodeLabel.text = "Join Code : " + code;
        }
    }
}

