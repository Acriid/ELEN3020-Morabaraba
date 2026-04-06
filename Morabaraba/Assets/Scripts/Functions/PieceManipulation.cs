using UnityEngine;

public class PieceManipulation : MonoBehaviour
{

    public bool CheckAvailibilty(PieceSO piece, BoardSO targetBoardSpace)
    {
        // Check if piece and target space are valid
        if (piece == null || targetBoardSpace == null)
            return false;

        //Check if piece is already on the board
        var currentBoardSpace = piece.GetCurrentBoardSpace();
        if (currentBoardSpace == null)
            return false; // piece not placed yet

        // Check if target space is empty
        if (targetBoardSpace.GetCurrentPiece() != null)
            return false;

        return true;

    }

    //placing a piece on an empty intersection on the board
    public void PlacePiece(PieceSO piece, BoardSO targetBoardSpace)
    {
        if (CheckAvailibilty(piece, targetBoardSpace))
        {
            var currentBoardSpace = piece.GetCurrentBoardSpace();
            currentBoardSpace.ChangeCurrentPiece(null);
            targetBoardSpace.ChangeCurrentPiece(piece);
            piece.SetCurrentBoardSpace(targetBoardSpace);
        }
    }

    //of moving a piece to an empty adjacent intersection
    public void MovePiece(PieceSO piece, BoardSO targetBoardSpace)
    {
        if (CheckAvailibilty(piece, targetBoardSpace))
        {
            //Check if target space is adjacent to current space
            var currentBoardSpace = piece.GetCurrentBoardSpace();
            // GetAdjacentBoardSpaces() returns a list of adjacent spaces
            if (currentBoardSpace.GetAdjacentBoardSpaces().Contains(targetBoardSpace))
            {
                currentBoardSpace.ChangeCurrentPiece(null);
                targetBoardSpace.ChangeCurrentPiece(piece);
                piece.SetCurrentBoardSpace(targetBoardSpace);
            }
        }
    }

    //allowed to "fly" to any empty intersection, not just adjacent ones

    public void FlyPiece(PieceSO piece, BoardSO targetSpace)
    {
        if (CheckAvailibilty(piece, targetSpace))
        {
            var currentSpace = piece.GetCurrentBoardSpace();
            currentSpace.ChangeCurrentPiece(null);
            targetSpace.ChangeCurrentPiece(piece);
            piece.SetCurrentBoardSpace(targetSpace);
        }


    }
}
