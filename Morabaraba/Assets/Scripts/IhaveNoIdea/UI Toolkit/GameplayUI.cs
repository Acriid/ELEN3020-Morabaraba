using UnityEngine;
using UnityEngine.UIElements;

public class GameplayUI : MonoBehaviour
{
    [SerializeField] private UIDocument _document;
    [SerializeField] private GameManager _gameManager;

    private Label _turnLabel;
    private Label _playerLabel;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _turnLabel = root.Q<Label>("TurnLabel");
        _gameManager.OnCurrentTeamChanged += UpdateTurnUI;
        UpdateTurnUI(_gameManager.GetCurrentTeam());

        _playerLabel = root.Q<Label>("PlayerLabel");
        _playerLabel.text = $"You are {_gameManager.GetLocalPlayerTeam()}";
    }

    private void OnDestroy()
    {
        if (_gameManager != null)
            _gameManager.OnCurrentTeamChanged -= UpdateTurnUI;
    }

    private void UpdateTurnUI(Team team)
    {
        _turnLabel.text = $"{team}'s Turn";
    }
}