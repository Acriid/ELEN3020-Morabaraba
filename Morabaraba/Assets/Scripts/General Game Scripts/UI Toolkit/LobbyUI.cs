using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LobbyUI : MonoBehaviour
{
    private Label playerList;
    private Button readyButton;
    private Button startButton;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        playerList = root.Q<Label>("playerList");
        readyButton = root.Q<Button>("readyButton");
        startButton = root.Q<Button>("startButton");

        readyButton.clicked += OnReadyClicked;
        startButton.clicked += OnStartClicked;
    }

    public void UpdatePlayerList(string players)
    {
        playerList.text = "Players:\n" + players;
    }

    private void OnReadyClicked()
    {
        Debug.Log("Player ready");
        // Send ready state to server
    }

    private void OnStartClicked()
    {
        Debug.Log("Start game");
        // Host triggers game start
        SceneManager.LoadScene("SampleScene");
    }
}
