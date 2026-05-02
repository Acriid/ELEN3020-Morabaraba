using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class BoardSO_test
{
    //BoardSpaces
    private BoardSO boardSpace1 = new();
    private BoardSO adjacentSpace1 = new();
    private BoardSO adjacentSpace2 = new();
    private BoardSO adjacentSpace3 = new();
    //Pieces
    private PieceSO piece1 = new();
    private PieceSO adjacentPiece1 = new();
    private PieceSO adjacentPiece2 = new();
    private PieceSO adjacentPiece3 = new();

    [SetUp]
    public void SetUp()
    {
        List<BoardSO> adjacentList = new()
        {
            adjacentSpace1,
            adjacentSpace2,
            adjacentSpace3
        };
        boardSpace1.SetAdjacentBoardSpaces(adjacentList);

        boardSpace1.ChangeCurrentPiece(piece1);
        piece1.Team = Team.Player1;

        adjacentSpace1.ChangeCurrentPiece(adjacentPiece1);
        adjacentSpace2.ChangeCurrentPiece(adjacentPiece2);
        adjacentSpace3.ChangeCurrentPiece(adjacentPiece3);
    }

    [Test]
    public void BoardSO_test_AdjacencyList()
    {

        List<BoardSO> expectedList = new()
        {
            adjacentSpace1,
            adjacentSpace2,
            adjacentSpace3          
        };

        List<BoardSO> resultList = boardSpace1.GetAdjacentBoardSpaces();

        Assert.AreEqual(resultList,expectedList);

    }

    [Test]
    public void BoardSO_test_GetAdjacentPieces()
    {
        Dictionary<BoardSO,PieceSO> expectedDictionary = new()
        {
            {adjacentSpace1,adjacentPiece1},
            {adjacentSpace2,adjacentPiece2},
            {adjacentSpace3,adjacentPiece3},
        };

        Dictionary<BoardSO,PieceSO> actualDictionary = boardSpace1.GetAdjacentPieces();

        Assert.AreEqual(expectedDictionary,actualDictionary);
    }

    [Test]
    public void BoardSO_test_GettingTeam()
    {
        bool expectedValue = true;
        
        bool actualValue = boardSpace1.CheckIfSameTeam(Team.Player1);

        Assert.AreEqual(expectedValue,actualValue);
    }

    [Test]
    public void BoardSO_test_SettingPiece()
    {
        PieceSO testPiece = new();

        boardSpace1.ChangeCurrentPiece(testPiece);

        PieceSO boardPiece = boardSpace1.GetCurrentPiece();

        Assert.AreEqual(testPiece,boardPiece);
    }
}
