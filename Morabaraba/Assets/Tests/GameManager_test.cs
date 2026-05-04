using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class GameManager_test
{
    private GameManager _gameManager;
    private List<BoardSO> boardSOs;
    [SetUp]
    public void SetUp()
    {
        _gameManager = new();
        MillDetection millDetection = new();
        _gameManager.SetMillDetection(millDetection);

        List<Piece> _team1Pieces = new();
        InitializePieces(ref _team1Pieces, Team.Player1);

        List<Piece> _team2Pieces = new();
        InitializePieces(ref _team2Pieces, Team.Player2);

        _gameManager.SetTeam1Pieces(_team1Pieces);
        _gameManager.SetTeam2Pieces(_team2Pieces);

        boardSOs = new();

        for(int i = 0 ; i < 7 ; i++)
        {
            BoardSO boardSO = new();
            boardSOs.Add(boardSO);
        }

        boardSOs[0].BoardID = "A7";
        boardSOs[1].BoardID = "B6";
        boardSOs[2].BoardID = "B4";
        boardSOs[3].BoardID = "B2";
        boardSOs[4].BoardID = "C5";
        boardSOs[5].BoardID = "D6";
        boardSOs[6].BoardID = "F6";

        _gameManager.SetBoardScriptableObjects(boardSOs);

        _gameManager.Initialize();

    }
    private void InitializePieces(ref List<Piece> pieceList, Team pieceTeam)
    {
        for(int i = 0 ; i < 4 ; i++)
        {
            Piece newPiece = new();
            PieceSO pieceSO = new();

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
        GameObject boardGameObject = new();
        BoardObject boardComponent = boardGameObject.AddComponent<BoardObject>();
        boardComponent.BoardSO = boardSOs[0];

        _gameManager.SetCurrentTeam(Team.Player1);
        Piece placedPiece = _gameManager.PlacePiece(boardGameObject);

        Assert.AreEqual(placedPiece.data,boardComponent.BoardSO.GetCurrentPiece());
    }

}
