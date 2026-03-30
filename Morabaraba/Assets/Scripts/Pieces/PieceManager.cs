using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
    [SerializeField] private List<BoardSO> _board;

    private Dictionary<BoardSO, Piece> _boardState = new();

    private void Awake()
    {
        foreach (BoardSO space in _board)
        {
            _boardState[space] = null;
        }
    }
    public void PlacePiece(Piece piece, BoardSO space)
    {
        _boardState[space] = piece;
        piece.SetCurrentSpace(space);
    }
    public void MovePiece(Piece piece, BoardSO newSpace)
    {
        BoardSO oldSpace = piece.GetCurrentSpace();

        if (oldSpace != null)
        {
            _boardState[oldSpace] = null;
        }

        _boardState[newSpace] = piece;
        piece.SetCurrentSpace(newSpace);
    }
    public Piece GetPiece(BoardSO space)
    {
        return _boardState[space];
    }
}