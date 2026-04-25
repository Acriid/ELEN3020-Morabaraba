using System.Collections.Generic;
using UnityEngine;

public  class MillDetection : MonoBehaviour
{
    public  List<HashSet<string>> _mills = new()
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


    private  Dictionary<string, BoardSO> _boardLookup = new();


    public  void InitializeBoard(IEnumerable<BoardSO> allBoardSpaces)
    {
        _boardLookup.Clear();
        foreach (BoardSO board in allBoardSpaces)
        {
            _boardLookup[board.BoardID] = board;
        }
    }


    public  bool DetectMill(BoardObject boardToCheck)
    {
        string boardID = boardToCheck.BoardSO.BoardID;

        PieceSO piece = boardToCheck.BoardSO.GetCurrentPiece();
        if (piece == null) return false;          

        Team team = piece.Team;

        foreach (HashSet<string> mill in _mills)
        {
            if (!mill.Contains(boardID)) continue;

            if (IsMillComplete(mill, team))
                return true;
        }

        return false;
    }


    private  bool IsMillComplete(HashSet<string> mill, Team team)
    {
        foreach (string id in mill)
        {

            if (!_boardLookup.TryGetValue(id, out BoardSO board))
                return false;


            if (!board.CheckIfSameTeam(team))
                return false;
        }

        return true;
    }
}
