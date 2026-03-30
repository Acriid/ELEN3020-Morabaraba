using System.Collections.Generic;
using UnityEngine;

public class BoardSpaceManager : MonoBehaviour
{
    [SerializeField] private List<BoardSO> _boardScriptableObjects;
    [SerializeField] private GameObject _boardPrefab;
    
    [SerializeField] private BoardType _boardType;
    [SerializeField] private int _gridSize;
    //Manages board size 
    void Awake()
    {
        InitializeBoard(_boardType);
    }
    private void InitializeGrid()
    {
        
    }
    private void InitializeBoard(BoardType boardType)
    {
        GameObject boardObject = null;
        if(boardType == BoardType.Morabaraba)
        {
            for(int i = 0; i < 7 ; i++)
            {
                for(int j = 0; j < 7 ; j++)
                {
                    boardObject = Instantiate(_boardPrefab,transform);

                    InitializeBoardSpace(boardObject,_boardScriptableObjects[i]);
                }

            }
        }
    }

    private void InitializeBoardSpace(GameObject boardSpace, BoardSO boardSO)
    {
        boardSpace.name = boardSO.BoardID;
        BoardObject boardComponent = boardSpace.GetComponent<BoardObject>();

        boardComponent.BoardSO = boardSO;
    }
}

public enum BoardType
{
    Morabaraba,
    Nine_Mens_Morris,
    Six_Mens_Morris
}
