using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class PieceSO_test
{
    [Test]
    public void PieceSO_test_BoardSpaceSetAndGet()
    {
        PieceSO testPiece = new();
        BoardSO testBoard = new();
        BoardSO expectedBoard = testBoard;

        testPiece.SetCurrentBoardSpace(testBoard);
        BoardSO actualBoard = testPiece.GetCurrentBoardSpace();

        Assert.AreEqual(actualBoard,expectedBoard);
    }

}
