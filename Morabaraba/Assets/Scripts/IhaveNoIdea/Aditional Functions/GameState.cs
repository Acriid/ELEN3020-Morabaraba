using System.Collections.Generic;
using UnityEngine;

namespace Morabaraba
{
    [System.Serializable]
    public class GameState
    {
        public Dictionary<BoardSO, PieceSO> boardState = new();
        public Dictionary<PieceSO, BoardSO> piecePositions = new();
        public Dictionary<PieceSO, bool> pieceActiveState = new();

        public Team currentTeam;
        public int team1Index;
        public int team2Index;
        public int piecesOnBoardTeam1;
        public int piecesOnBoardTeam2;
        public bool waitingForRemoval;
        public Team removalTeam;

        public GameState(
            List<BoardSO> boardSpaces,
            List<Piece> team1Pieces,
            List<Piece> team2Pieces,
            Team currentTeam,
            int team1Index,
            int team2Index,
            int piecesOnBoardTeam1,
            int piecesOnBoardTeam2,
            bool waitingForRemoval,
            Team removalTeam)
        {
            foreach (BoardSO board in boardSpaces)
            {
                boardState[board] = board.GetCurrentPiece();
            }

            foreach (Piece piece in team1Pieces)
            {
                piecePositions[piece.data] = piece.data.GetCurrentBoardSpace();
                pieceActiveState[piece.data] = piece.gameObject.activeSelf;
            }

            foreach (Piece piece in team2Pieces)
            {
                piecePositions[piece.data] = piece.data.GetCurrentBoardSpace();
                pieceActiveState[piece.data] = piece.gameObject.activeSelf;
            }

            this.currentTeam = currentTeam;
            this.team1Index = team1Index;
            this.team2Index = team2Index;
            this.piecesOnBoardTeam1 = piecesOnBoardTeam1;
            this.piecesOnBoardTeam2 = piecesOnBoardTeam2;
            this.waitingForRemoval = waitingForRemoval;
            this.removalTeam = removalTeam;
        }
    }
}