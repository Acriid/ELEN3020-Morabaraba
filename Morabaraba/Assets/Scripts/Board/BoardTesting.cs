using System.Collections.Generic;
using UnityEngine;

public class BoardTesting : MonoBehaviour
{
    public List<BoardSO> AllBoardSO;

    void Start()
    {
        CheckBoardAdjacency();
    }
    //Test case 1 
    //Checking for if the board adjacency is correct for the default board size
    private void CheckBoardAdjacency()
    {
        Dictionary<string, HashSet<string>> expectedAdjacency = new Dictionary<string, HashSet<string>>()
        {
            { "A7", new HashSet<string>{ "A4", "B6", "D7" } },
            { "A4", new HashSet<string>{ "A7", "A1", "B4" } },
            { "A1", new HashSet<string>{ "A4", "B2", "D1" } },
            { "B6", new HashSet<string>{ "A7", "B4", "C5", "D6" } },
            { "B4", new HashSet<string>{ "A4", "B6", "B2", "C4" } },
            { "B2", new HashSet<string>{ "A1", "B4", "C3", "D2" } },
            { "C5", new HashSet<string>{ "B6", "C4", "D5" } },
            { "C4", new HashSet<string>{ "B4", "C5", "C3" } },
            { "C3", new HashSet<string>{ "B2", "C4", "D3" } },
            { "D7", new HashSet<string>{ "A7", "D6", "G7" } },
            { "D6", new HashSet<string>{ "B6", "D7", "D5", "F6" } },
            { "D5", new HashSet<string>{ "C5", "D6", "E5" } },
            { "D3", new HashSet<string>{ "C3", "D2", "E3" } },
            { "D2", new HashSet<string>{ "B2", "D3", "D1", "F2" } },
            { "D1", new HashSet<string>{ "A1", "D2", "G1" } },
            { "E5", new HashSet<string>{ "D5", "E4", "F6" } },
            { "E4", new HashSet<string>{ "E5", "E3", "F4" } },
            { "E3", new HashSet<string>{ "D3", "E4", "F2" } },
            { "F6", new HashSet<string>{ "D6", "E5", "F4", "G7" } },
            { "F4", new HashSet<string>{ "E4", "F6", "F2", "G4" } },
            { "F2", new HashSet<string>{ "D2", "E3", "F4", "G1" } },
            { "G7", new HashSet<string>{ "D7", "F6", "G4" } },
            { "G4", new HashSet<string>{ "F4", "G7", "G1" } },
            { "G1", new HashSet<string>{ "D1", "F2", "G4" } }
        };

        foreach (BoardSO boardSpace in AllBoardSO)
        {
            if (!expectedAdjacency.ContainsKey(boardSpace.BoardID))
                continue;

            var expected = expectedAdjacency[boardSpace.BoardID];
            var actual = boardSpace.GetAdjacentBoardSpaces();

            foreach (BoardSO board in actual)
            {
                if (!expected.Contains(board.BoardID))
                {
                    Debug.Log($"{boardSpace.BoardID} fault: unexpected adjacency {board.BoardID}");
                }
            }
        }
    }
}
