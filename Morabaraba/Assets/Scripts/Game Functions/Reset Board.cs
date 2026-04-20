using System;
using System.Collections.Generic;
using UnityEngine;

public class ResetBoard : MonoBehaviour
{
    [SerializeField] private List<BoardObject> _boardSpaces;
    void Awake()
    {
        foreach(BoardObject boardObject in _boardSpaces)
        {
            boardObject.Initialize();
        }
    }
}
