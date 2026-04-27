using TMPro;
using UnityEngine;

public class getLobbyCode : MonoBehaviour
{
    private RelayManager relayManager;

    [SerializeField] TextMeshPro codeText;

    public void GetCode()
    {
        relayManager = FindAnyObjectByType<RelayManager>();
        string code = relayManager.JoinCodeInput;
        codeText.text = code;
        Debug.Log($"Lobby Code: {code}");
    }

    void Awake()
    {
        GetCode();
    }
}
