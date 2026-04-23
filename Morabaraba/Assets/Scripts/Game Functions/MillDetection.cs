using System.Collections.Generic;
using UnityEngine;

public class MillDetection : MonoBehaviour
{
    public List<HashSet<string>> _mills = new()
    {
        // Vertical
        new HashSet<string>{ "A7","A4","A1" },
        new HashSet<string>{ "B6","B4","B2" },
        new HashSet<string>{ "C5","C4","C3" },
        new HashSet<string>{ "D7","D6","D5" },
        new HashSet<string>{ "D3","D2","D1" },
        new HashSet<string>{ "E5","E4","E3" },
        new HashSet<string>{ "F6","F4","F2" },
        new HashSet<string>{ "G7","G4","G1" },

        // Horizontal
        new HashSet<string>{ "A7","D7","G7" },
        new HashSet<string>{ "B6","D6","F6" },
        new HashSet<string>{ "C5","D5","E5" },
        new HashSet<string>{ "A4","B4","C4" },
        new HashSet<string>{ "E4","F4","G4" },
        new HashSet<string>{ "C3","D3","E3" },
        new HashSet<string>{ "B2","D2","F2" },
        new HashSet<string>{ "A1","D1","G1" },

        // Diagonals
        new HashSet<string>{ "A7","B6","C5" },
        new HashSet<string>{ "G7","F6","E5" },

        new HashSet<string>{ "A1","B2","C3" },
        new HashSet<string>{ "G1","F2","E3" },
    };

    public bool DetectMill(BoardObject boardToCheck)
    {
        List<BoardSO> adjacentBoardSpaces = boardToCheck.BoardSO.GetAdjacentBoardSpaces();


        string boardID = boardToCheck.BoardSO.BoardID;
        PieceSO pieceToCheck = boardToCheck.BoardSO.GetCurrentPiece();
        Team pieceTeam = pieceToCheck.Team;

        List<BoardSO> nextBoards = new();
        HashSet<string> boardIDs = new()
        {
            boardID
        };

        List<HashSet<string>> checkMills = new();
        foreach(HashSet<string> strings in _mills)
        {
            if(strings.Contains(boardID))
            {
                checkMills.Add(strings);
            }
        }

        foreach(BoardSO board in adjacentBoardSpaces)
        {
            if(board.CheckIfSameTeam(pieceTeam))
            {
                nextBoards.Add(board);
            }
        }

        if(nextBoards.Count == 2)
        {
            foreach(BoardSO board in nextBoards)
            {
                boardIDs.Add(board.BoardID);
            }

            foreach(HashSet<string> strings in checkMills)
            {
                if(boardIDs.IsSubsetOf(strings))
                {
                    return true;
                }
            }
        }
        else if(nextBoards.Count == 1)
        {
            BoardSO nextBoard = nextBoards[0];
            PieceSO checkPiece = nextBoard.GetCurrentPiece();
            Team checkTeam = checkPiece.Team;
            List<BoardSO> checkBoards = nextBoard.GetAdjacentBoardSpaces();

            boardIDs.Add(nextBoard.BoardID);

            foreach(BoardSO board in checkBoards)
            {
                if(board.CheckIfSameTeam(checkTeam))
                {
                    
                }
            }
        }




        
        //12:39
        //13:45


        return false;
    }
}
