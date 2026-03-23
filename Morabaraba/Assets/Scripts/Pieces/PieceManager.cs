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
   
}
