using UnityEngine;

public class WinLooseUI : MonoBehaviour
{
    private Button exitButton;
    private Label resultLabel;
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.Focus();

        exitButton = root.Q<Button>("ExitButton");
        resultLabel = root.Q<Label>("ResultLabel");

        if (exitButton == null)
        {
            Debug.LogWarning("Exit button not found in UI.");
            return;
        }
        exitButton.clicked += OnExitClicked;
    }

    public setResult(Team winningTeam)
    {
        resultLabel.text = $"{winningTeam} wins!";
    }
    private void OnExitClicked()
    {
        Debug.Log("Exiting lobby");
        SceneManager.LoadScene("Menu");
    }
}
