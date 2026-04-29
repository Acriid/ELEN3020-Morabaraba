using UnityEngine;
using UnityEngine.UIElements;

public class HudUI : MonoBehaviour
{

    private Button exitButton;
    private RelayManager relayManager;

    private void OnEnable()
    {
        relayManager = FindAnyObjectByType<RelayManager>();

        var root = GetComponent<UIDocument>().rootVisualElement;

        exitButton = root.Q<Button>("ExitButton");
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
    }
}

