using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class HudUIai : MonoBehaviour
{
    [SerializeField] private UndoRedoManager undoRedoManager;

    private Button exitButton;
    private Label joinCodeLabel;
    private Button undoButton;
    private Button redoButton;


    private void OnEnable()
    {
        DontDestroyOnLoad(gameObject.transform.parent);



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

        undoButton = doContainer.Q<Button>("undoButton");
        redoButton = doContainer.Q<Button>("redoButton");

        undoButton.clicked += OnUndoClicked;
        redoButton.clicked += OnRedoClicked;

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
        SceneManager.LoadScene("Setup");
        joinCodeLabel.text = "";

    }

    public void SetJoinCode(string code)
    {
        if (joinCodeLabel != null)
        {
            joinCodeLabel.text = " " + code;
        }
    }

    private void OnUndoClicked()
    {
        Debug.Log("UNDO CLICKED");
        undoRedoManager.Undo();
    }

    private void OnRedoClicked()
    {
        Debug.Log("REDO CLICKED");
        undoRedoManager.Redo();
    }
}

