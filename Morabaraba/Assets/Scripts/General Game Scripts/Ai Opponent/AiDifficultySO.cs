using UnityEngine;

[CreateAssetMenu(fileName = "Difficulty", menuName = "AI/AiDifficultySO")]
public class AiDifficultySO : ScriptableObject
{
    public AiBrain.AiDifficulty AiDifficulty;
}
