using UnityEngine;
using UnityEngine.UIElements;

public class GameplayUI : MonoBehaviour
{
    [SerializeField] private UIDocument _document;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GameManagerAI _gameManagerAI;

    private Label _turnLabel;
    private Label _playerLabel;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _turnLabel = root.Q<Label>("TurnLabel");
        _playerLabel = root.Q<Label>("PlayerLabel");
        if(_gameManager != null)
        {
            _gameManager.OnCurrentTeamChanged += UpdateTurnUI;
            UpdateTurnUI(_gameManager.GetCurrentTeam());
            _playerLabel.text = $"You are {_gameManager.GetLocalPlayerTeam()}";
        }

        if(_gameManagerAI != null)
        {
            //Player is always player 1
            _gameManagerAI.onMoveDone += UpdateTurnUI;
            UpdateTurnUI(Team.Player1);
            _playerLabel.text = $"You are {Team.Player1}";
        }

        

        
    }

    private void OnDestroy()
    {
        if (_gameManager != null)
            _gameManager.OnCurrentTeamChanged -= UpdateTurnUI;
        if(_gameManagerAI != null)
            _gameManagerAI.onMoveDone -= UpdateTurnUI;
    }

    private void UpdateTurnUI(Team team)
    {
        _turnLabel.text = $"{team}'s Turn";
    }
    private void UpdateTurnUI()
    {
        _turnLabel.text = $"{_gameManagerAI.GetCurrentTeam()}'s Turn";
    }
}