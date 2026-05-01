using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Space", menuName = "Board/BoardSpace")]
public class BoardSO : ScriptableObject
{
    public string BoardID;
    [SerializeField] private List<BoardSO> _adjacentBoardSpaces;
    [SerializeField] private PieceSO _currentHeldPiece;
    public event Action<string> OnPieceChange;
    public void Initialize()
    {
        _currentHeldPiece = null;

    }
    public PieceSO GetCurrentPiece()
    {
        return _currentHeldPiece;
    }
    public void ChangeCurrentPiece(PieceSO newPiece)
    {
        _currentHeldPiece = newPiece;
        OnPieceChange?.Invoke(BoardID);
    }
    public bool CheckIfSameTeam(Team checkTeam)
    {
        if(_currentHeldPiece == null) return false;
        if(_currentHeldPiece.Team != checkTeam) return false;

        return true;
    }
    public List<BoardSO> GetAdjacentBoardSpaces()
    {
        return _adjacentBoardSpaces;
    }

    public Dictionary<BoardSO, PieceSO> GetAdjacentPieces()
    {
        Dictionary<BoardSO, PieceSO> returnDictionary = new();
        foreach(BoardSO boardSpace in _adjacentBoardSpaces)
        {
            returnDictionary[boardSpace] = boardSpace.GetCurrentPiece();
        }
        return returnDictionary;
    }
}
