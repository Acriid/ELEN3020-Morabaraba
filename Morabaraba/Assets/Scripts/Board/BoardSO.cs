using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Space", menuName = "Board/BoardSpace")]
public class BoardSO : ScriptableObject
{
    public string BoardID;
    [SerializeField] private List<BoardSO> _adjacentBoardSpaces;
    //[SerializeField] private PieceSO _currentHeldPiece;

    /*public PieceSO GetCurrentPiece()
    {
        return _currentHeldPiece;
    }*/

    /*public PieceSO ChangeCurrentPiece(PieceSO newPiece)
    {
        _currentHeldPiece = newPiece;
    }*/

    public List<BoardSO> GetAdjacentBoardSpaces()
    {
        return _adjacentBoardSpaces;
    }

    /*public Dictionary<BoardSO, PieceSO> GetAdjacentPieces()
    {
        Dictionary<BoardSO, PieceSO> returnDictionary = new();
        foreach(BoardSO boardSpace in _adjacentBoardSpaces)
        {
            returnDictionary[boardSpace] = boardSpace.GetCurrentPiece();
        }
        return returnDictionary;
    }*/
}
