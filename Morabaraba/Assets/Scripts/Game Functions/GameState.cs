using System.Collections.Generic;

namespace Morabaraba
{
    [System.Serializable]
    public class GameState
    {
        public Dictionary<BoardSO, PieceSO> boardState = new Dictionary<BoardSO, PieceSO>();

        public GameState(List<BoardSO> boardSpaces)
        {
            foreach (var space in boardSpaces)
            {
                boardState[space] = space.GetCurrentPiece();
            }
        }
    }
}