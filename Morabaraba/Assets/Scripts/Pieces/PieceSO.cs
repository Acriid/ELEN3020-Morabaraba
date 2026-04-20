using Unity.VisualScripting;
using UnityEngine;

public enum Team
{
    Player1,
    Player2
}

[CreateAssetMenu(fileName = "Piece", menuName = "Board/BoardPiece")]
public class PieceSO : ScriptableObject
{
    public string PieceID;
    public Sprite PieceSprite;
    public Team Team;
    [SerializeField] private BoardSO _currentBoardSpace;

    public void Initialize()
    {
        _currentBoardSpace = null;
    }
    //Added by James to get piece location 
    public BoardSO GetCurrentBoardSpace()
    {
        return _currentBoardSpace;
    }
    // Added by James to set piece location
    public void SetCurrentBoardSpace(BoardSO newBoardSpace)
    {
        _currentBoardSpace = newBoardSpace;
    }
}
