using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class WinLooseUI : MonoBehaviour
{
    public static WinLooseUI Instance { get; private set; }

    private Button exitButton;
    private Label resultLabel;

    public static Team LastWinner;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Start()
    {
        setResult(LastWinner);
    }

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

        exitButton.clicked -= OnExitClicked;
        exitButton.clicked += OnExitClicked;
    }

    public void setResult(Team winningTeam)
    {
        if (resultLabel == null) return;

        string winnerText = winningTeam == Team.Player1 ? "Player 1" : "Player 2";

        resultLabel.text = $"{winnerText} wins!";
    }

    private void OnExitClicked()
    {
        Debug.Log("Exiting lobby");
        SceneManager.LoadScene("Setup");
    }
}