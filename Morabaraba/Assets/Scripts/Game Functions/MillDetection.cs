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

    public bool DetectMill(string boardToCheck)
    {
        foreach(HashSet<string> strings in _mills)
        {
            if(strings.Contains(boardToCheck))
            {
                
            }
        }

        return false;
    }
}
