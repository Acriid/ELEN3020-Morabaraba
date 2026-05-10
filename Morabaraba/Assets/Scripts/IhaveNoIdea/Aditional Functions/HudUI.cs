using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class HudUI : MonoBehaviour
{
    [SerializeField] private UndoRedoManager undoRedoManager;

    private Button exitButton;
    private Label joinCodeLabel;
    private RelayManager relayManager;
    private Button undoButton;
    private Button redoButton;


    private void OnEnable()
    {
        DontDestroyOnLoad(gameObject.transform.parent);
        relayManager = FindAnyObjectByType<RelayManager>();

        var root = GetComponent<UIDocument>().rootVisualElement;

        exitButton = root.Q<Button>("ExitButton");
        joinCodeLabel = root.Q<Label>("Code");

        undoButton = root.Q<Button>("undoButton");
        redoButton = root.Q<Button>("redoButton");

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

