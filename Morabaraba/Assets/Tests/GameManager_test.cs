using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

public class GameManager_test
{
    private GameManager _gameManager;
    private List<BoardSO> boardSOs;
    private List<Piece> _team1Pieces;
    private List<Piece> _team2Pieces;
    [SetUp]
    public void SetUp()
    {
        _gameManager = new();
        MillDetection millDetection = new();
        _gameManager.SetMillDetection(millDetection);

        _team1Pieces = new();
        InitializePieces(ref _team1Pieces, Team.Player1);

        _team2Pieces = new();
        InitializePieces(ref _team2Pieces, Team.Player2);

        _gameManager.SetTeam1Pieces(_team1Pieces);
        _gameManager.SetTeam2Pieces(_team2Pieces);

        boardSOs = new();

        for(int i = 0 ; i < 7 ; i++)
        {
            BoardSO boardSO = ScriptableObject.CreateInstance<BoardSO>();
            boardSOs.Add(boardSO);
        }

        boardSOs[0].BoardID = "A7";
        boardSOs[1].BoardID = "B6";
        boardSOs[2].BoardID = "B4";
        boardSOs[3].BoardID = "B2";
        boardSOs[4].BoardID = "C5";
        boardSOs[5].BoardID = "D6";
        boardSOs[6].BoardID = "F6";

        foreach(BoardSO board in boardSOs)
        {
            board.ChangeCurrentPiece(null);
        }

        List<BoardSO> adjacencyBoard = new()
        {
            boardSOs[1]
        };
        boardSOs[0].SetAdjacentBoardSpaces(adjacencyBoard);

        adjacencyBoard = new()
        {
            boardSOs[0],
            boardSOs[2],
            boardSOs[4],
            boardSOs[5],
        };
        boardSOs[1].SetAdjacentBoardSpaces(adjacencyBoard);

        adjacencyBoard = new()
        {
            boardSOs[1],
            boardSOs[3],
        };
        boardSOs[2].SetAdjacentBoardSpaces(adjacencyBoard);

        adjacencyBoard = new()
        {
            boardSOs[2]
        };
        boardSOs[3].SetAdjacentBoardSpaces(adjacencyBoard);

        adjacencyBoard = new()
        {
            boardSOs[1],
        };
        boardSOs[4].SetAdjacentBoardSpaces(adjacencyBoard);

        adjacencyBoard = new()
        {
            boardSOs[1],
            boardSOs[6],
        };
        boardSOs[5].SetAdjacentBoardSpaces(adjacencyBoard);

        adjacencyBoard = new()
        {
            boardSOs[5]
        };
        boardSOs[6].SetAdjacentBoardSpaces(adjacencyBoard);

        _gameManager.SetBoardScriptableObjects(boardSOs);

        _gameManager.Initialize();

    }

    [TearDown]
    public void ClearObjects()
    {
        foreach(BoardSO board in boardSOs)
        {
            Object.DestroyImmediate(board);
        }
        foreach(Piece piece in _team1Pieces)
        {
            Object.DestroyImmediate(piece.data);
            Object.DestroyImmediate(piece);
        }
        foreach(Piece piece in _team2Pieces)
        {
            Object.DestroyImmediate(piece.data);
            Object.DestroyImmediate(piece);
        }
    }

    private void InitializePieces(ref List<Piece> pieceList, Team pieceTeam)
    {
        for(int i = 0 ; i < 4 ; i++)
        {
            GameObject pieceObject = new()
            {
                name = $"{pieceTeam} {i}"
            };
            Piece newPiece = pieceObject.AddComponent<Piece>();
            PieceSO pieceSO = ScriptableObject.CreateInstance<PieceSO>();

            newPiece.data = pieceSO;
            newPiece.data.Team = pieceTeam;

            pieceList.Add(newPiece);
        }
    }

    [Test]
    public void GameManager_test_TeamChangeChangesTeam()
    {
        _gameManager.SetCurrentTeam(Team.Player1);
        Assert.AreEqual(_gameManager.GetCurrentTeam(),Team.Player1);

        _gameManager.SetCurrentTeam(Team.Player2);
        Assert.AreEqual(_gameManager.GetCurrentTeam(),Team.Player2);
    }

    [Test]
    public void GameManager_test_TestPlacePieceOnEmptySpace()
    {
        //Place piece on A7
        GameObject boardGameObject = new();
        BoardObject boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[0];

        _gameManager.SetCurrentTeam(Team.Player1);
        Piece placedPiece = _gameManager.PlacePiece(boardGameObject);

        Assert.AreEqual(placedPiece.data,boardComponent.BoardSO.GetCurrentPiece());

    }

    [Test]
    public void GameManager_test_TestPlacePieceOnNonEmptySpace()
    {
        //Place piece on A7 again
        GameObject boardGameObject = new();
        BoardObject boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[0];

        _gameManager.SetCurrentTeam(Team.Player1);
        _gameManager.PlacePiece(boardGameObject);
        Piece placedPiece = _gameManager.PlacePiece(boardGameObject);

        Assert.AreEqual(placedPiece,null);
    }

    [Test]
    public void GameManager_test_TestMillDetection()
    {
        //Place piece on A7, B6 and C5 making a mill
        GameObject boardGameObject = new();
        BoardObject boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[0];

        _gameManager.SetCurrentTeam(Team.Player1);
        _gameManager.PlacePiece(boardGameObject);

        boardGameObject = new();
        boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[1];

        _gameManager.SetCurrentTeam(Team.Player1);
        _gameManager.PlacePiece(boardGameObject);

        boardGameObject = new();
        boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[4];

        _gameManager.SetCurrentTeam(Team.Player1);
        _gameManager.PlacePiece(boardGameObject);


        Assert.AreEqual(true,_gameManager.GetMillDetected());
    }

    [Test]
    public void GameManager_test_TestMillDetectionDifferentTeam()
    {
        //Place piece on A7, B6 and C5 making a mill
        GameObject boardGameObject = new();
        BoardObject boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[0];

        _gameManager.SetCurrentTeam(Team.Player1);
        _gameManager.PlacePiece(boardGameObject);

        boardGameObject = new();
        boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[1];

        //Place team 2 piece on B6
        _gameManager.SetCurrentTeam(Team.Player2);
        _gameManager.PlacePiece(boardGameObject);

        boardGameObject = new();
        boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[4];

        _gameManager.SetCurrentTeam(Team.Player1);
        _gameManager.PlacePiece(boardGameObject);


        Assert.AreEqual(false,_gameManager.GetMillDetected());
    }

    [Test]
    public void GameManager_test_TestMovePieceToEmptySpace()
    {
        //Place piece on A7
        GameObject boardGameObject = new();
        BoardObject boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[0];

        _gameManager.SetCurrentTeam(Team.Player1);
        Piece placedPiece = _gameManager.PlacePiece(boardGameObject);


        GameObject boardGameObject2 = new();
        BoardObject boardComponent2 = boardGameObject2.AddComponent<BoardObject>();
        boardComponent2.BoardSO = boardSOs[1];

        //Move piece to B6
        _gameManager.MovePiece(boardGameObject);
        _gameManager.MovePiece(boardGameObject2);

        Assert.AreEqual(boardComponent.BoardSO.GetCurrentPiece(),null);
        Assert.AreEqual(boardComponent2.BoardSO.GetCurrentPiece(),placedPiece.data);

    }
    [Test]
    public void GameManager_test_CantMoveDifferentTeamPiece()
    {
        //Place piece on A7
        GameObject boardGameObject = new();
        BoardObject boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[0];

        _gameManager.SetCurrentTeam(Team.Player1);
        Piece placedPiece = _gameManager.PlacePiece(boardGameObject);


        GameObject boardGameObject2 = new();
        BoardObject boardComponent2 = boardGameObject2.AddComponent<BoardObject>();
        boardComponent2.BoardSO = boardSOs[1];

        //Move piece to B6
        _gameManager.SetCurrentTeam(Team.Player2);
        _gameManager.MovePiece(boardGameObject);
        _gameManager.MovePiece(boardGameObject2);

        Assert.AreEqual(boardComponent.BoardSO.GetCurrentPiece(),placedPiece.data);
        Assert.AreEqual(boardComponent2.BoardSO.GetCurrentPiece(),null);

    }
    [Test]
    public void GameManager_test_CantMoveToNonAdjacentSquare()
    {
        //Place piece on A7
        GameObject boardGameObject = new();
        BoardObject boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[0];

        _gameManager.SetCurrentTeam(Team.Player1);
        Piece placedPiece = _gameManager.PlacePiece(boardGameObject);


        GameObject boardGameObject2 = new();
        BoardObject boardComponent2 = boardGameObject2.AddComponent<BoardObject>();
        boardComponent2.BoardSO = boardSOs[4];

        //Move piece to C5
        _gameManager.MovePiece(boardGameObject);
        _gameManager.MovePiece(boardGameObject2);

        Assert.AreEqual(boardComponent.BoardSO.GetCurrentPiece(),placedPiece.data);
        Assert.AreEqual(boardComponent2.BoardSO.GetCurrentPiece(),null);

    }
    [Test]
    public void GameManager_test_CanFlyToNonAdjacentSquare()
    {
        //Place piece on A7
        GameObject boardGameObject = new();
        BoardObject boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[0];

        _gameManager.SetCurrentTeam(Team.Player1);
        Piece placedPiece = _gameManager.PlacePiece(boardGameObject);


        GameObject boardGameObject2 = new();
        BoardObject boardComponent2 = boardGameObject2.AddComponent<BoardObject>();
        boardComponent2.BoardSO = boardSOs[4];

        //Move piece to C5
        _gameManager.FlyPiece(boardGameObject);
        _gameManager.FlyPiece(boardGameObject2);

        Assert.AreEqual(boardComponent.BoardSO.GetCurrentPiece(),null);
        Assert.AreEqual(boardComponent2.BoardSO.GetCurrentPiece(),placedPiece.data);

    }

    [Test]
    public void GameManager_test_CanRemoveOppositeTeamPieceWhenMill()
    {
        //Place piece on A7, B6 and C5 making a mill
        GameObject boardGameObject = new();
        BoardObject boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[0];

        _gameManager.SetCurrentTeam(Team.Player1);
        _gameManager.PlacePiece(boardGameObject);

        boardGameObject = new();
        boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[1];

        _gameManager.SetCurrentTeam(Team.Player1);
        _gameManager.PlacePiece(boardGameObject);

        GameObject boardGameObject2 = new();
        BoardObject boardComponent2 = boardGameObject2.AddComponent<BoardObject>();
        boardComponent2.BoardSO = boardSOs[5];

        _gameManager.SetCurrentTeam(Team.Player2);
        _gameManager.PlacePiece(boardGameObject2);


        boardGameObject = new();
        boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[4];

        _gameManager.SetCurrentTeam(Team.Player1);
        _gameManager.PlacePiece(boardGameObject);

        Assert.AreNotEqual(boardComponent2.BoardSO.GetCurrentPiece(),null);

        if(_gameManager.GetMillDetected())
            _gameManager.HandleRemovalClick(boardGameObject2);

        Assert.AreEqual(boardComponent2.BoardSO.GetCurrentPiece(),null);

    }
}
