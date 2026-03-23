using UnityEngine;

[CreateAssetMenu(fileName = "Piece", menuName = "Board/BoardPiece")]
public class PieceSO : ScriptableObject
{
    public string pieceID;
    public Sprite pieceSprite;
    public Team team;
    private BoardSO _currentBoardSpace;
}
