using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class setupUI : MonoBehaviour
{
    private UIDocument uiDocument;

    private DropdownField gameModeDropdown;
    private DropdownField boardTypeDropdown;
    private DropdownField aiDifficultyDropdown;

    private VisualElement aiDifficultyContainer;

    private Button startButton;
    private Button backButton;

    private Label feedbackLabel;

    public string selectedGameMode;
    public string selectedBoardType;
    public string selectedDifficulty;

    private string nextScene;

    private AiBrain aibrain;

    public void Start()
    {
        aibrain = FindFirstObjectByType<AiBrain>();
    }
    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();

        VisualElement root = uiDocument.rootVisualElement;

        gameModeDropdown = root.Q<DropdownField>("gameModeDropdown");
        boardTypeDropdown = root.Q<DropdownField>("boardTypeDropdown");
        aiDifficultyDropdown = root.Q<DropdownField>("aiDifficultyDropdown");

        aiDifficultyContainer = root.Q<VisualElement>("aiDifficultyContainer");

        startButton = root.Q<Button>("startButton");
        backButton = root.Q<Button>("backButton");

        feedbackLabel = root.Q<Label>("feedbackLabel");

        RegisterCallbacks();

        InitializeUI();
    }

    private void OnDisable()
    {
        UnregisterCallbacks();
    }

    private void RegisterCallbacks()
    {
        gameModeDropdown.RegisterValueChangedCallback(OnGameModeChanged);

        startButton.clicked += OnStartClicked;
        backButton.clicked += OnBackClicked;
    }

    private void UnregisterCallbacks()
    {
        gameModeDropdown.UnregisterValueChangedCallback(OnGameModeChanged);

        startButton.clicked -= OnStartClicked;
        backButton.clicked -= OnBackClicked;
    }

    private void InitializeUI()
    {
        selectedGameMode = gameModeDropdown.value;
        selectedBoardType = boardTypeDropdown.value;
        selectedDifficulty = aiDifficultyDropdown.value;

        UpdateAIVisibility();
    }

    private void OnGameModeChanged(ChangeEvent<string> evt)
    {
        selectedGameMode = evt.newValue;

        UpdateAIVisibility();
    }

    private void UpdateAIVisibility()
    {
        bool isAI = selectedGameMode == "Player vs AI";

        aiDifficultyContainer.style.display =
            isAI ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnStartClicked()
    {
        selectedGameMode = gameModeDropdown.value;
        selectedBoardType = boardTypeDropdown.value;
        selectedDifficulty = aiDifficultyDropdown.value;

        Debug.Log("START GAME");

        Debug.Log("Mode: " + selectedGameMode);
        Debug.Log("Board: " + selectedBoardType);

        RelayManager.Instance.SetBoardType(selectedBoardType);


        if (selectedGameMode == "Player vs AI")
        {
            if (selectedDifficulty == "Easy")
            {
                aibrain.SetAIDifficulty(AiBrain.AiDifficulty.Easy);
            }
            else if (selectedDifficulty == "Medium")
            {
                aibrain.SetAIDifficulty(AiBrain.AiDifficulty.Normal);
            }
            else if (selectedDifficulty == "Hard")
            {
                aibrain.SetAIDifficulty(AiBrain.AiDifficulty.Hard);
            }
        }

        feedbackLabel.text = "Starting game...";

        if (selectedGameMode == "Player vs Player")
        {
            SceneManager.LoadScene("Menu");
        }
        else if (selectedGameMode == "Player vs AI")
        {
            if (selectedBoardType == "Morabaraba")
            {
                nextScene = "Morabaraba AI";
                SceneManager.LoadScene(nextScene);
            }
            else if (selectedBoardType == "Nine Men's Morris")
            {
                nextScene = "Nine-Mens AI";
                SceneManager.LoadScene(nextScene);
            }
            else
            {
                nextScene = "Six-Mens AI";
                SceneManager.LoadScene(nextScene);
            }
        }
    }

    private void OnBackClicked()
    {
        Debug.Log("BACK");

        feedbackLabel.text = "Returning...";
        Application.Quit();
    }
}
