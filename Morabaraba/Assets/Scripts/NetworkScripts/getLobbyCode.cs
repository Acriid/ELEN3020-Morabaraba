using TMPro;
using UnityEngine;

public class GetLobbyCode : MonoBehaviour
{
    private RelayManager relayManager;
    [SerializeField] TextMeshPro codeText; // Changed to UI variant

    void Start() // Changed from Awake - gives RelayManager time to be found
    {
        relayManager = FindAnyObjectByType<RelayManager>();

        if (relayManager == null)
        {
            Debug.LogWarning("RelayManager not found.");
            return;
        }

        string code = relayManager.JoinCodeInput;

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("Join code is empty - relay may not be set up yet.");
            return;
        }

        codeText.text = code;
        Debug.Log($"Lobby Code: {code}");
    }
}