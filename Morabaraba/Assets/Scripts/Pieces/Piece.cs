using UnityEngine;

public class Piece : MonoBehaviour
{
    public PieceSO data;

    private BoardSO _currentBoardSpace;

    public void SetCurrentSpace(BoardSO newSpace)
    {
        _currentBoardSpace = newSpace;
    }

    public BoardSO GetCurrentSpace()
    {
        return _currentBoardSpace;
    }
}
