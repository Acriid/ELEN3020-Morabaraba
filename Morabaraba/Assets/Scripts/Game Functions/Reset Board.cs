using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ResetBoard : MonoBehaviour
{
    [SerializeField] private List<BoardObject> _boardSpaces;
    [SerializeField] private List<Piece> _pieces;
    void Awake()
    {
        foreach(BoardObject boardObject in _boardSpaces)
        {
            boardObject.Initialize();
        }

        foreach(Piece piece in _pieces)
        {
            piece.Initialize();
        }
    }
}
