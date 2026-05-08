using System.Collections.Generic;
using Morabaraba;
using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    private readonly Stack<GameState> undoStack = new();
    private readonly Stack<GameState> redoStack = new();

    [SerializeField] private List<BoardSO> boardSpaces;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private List<Piece> team1Pieces;
    [SerializeField] private List<Piece> team2Pieces;

    private readonly Dictionary<PieceSO, Piece> pieceMap = new();
    private readonly Dictionary<BoardSO, BoardObject> boardMap = new();

    private void Start()
    {
        foreach (var piece in team1Pieces)
            pieceMap[piece.data] = piece;
        foreach (var piece in team2Pieces)
            pieceMap[piece.data] = piece;

        var allBoardObjects = FindObjectsByType<BoardObject>(FindObjectsSortMode.None);

        foreach (var boardSO in boardSpaces)
        {
            foreach (var bo in allBoardObjects)
            {
                if (bo.BoardSO == boardSO)
                {
                    boardMap[boardSO] = bo;
                    break;
                }
            }
        }
    }

    public void SaveState()
    {
        var state = new GameState(
            boardSpaces,
            team1Pieces,
            team2Pieces,
            gameManager.GetCurrentTeam(),
            gameManager.GetTeam1Index(),
            gameManager.GetTeam2Index(),
            gameManager.GetPiecesOnBoardForTeam(Team.Player1),
            gameManager.GetPiecesOnBoardForTeam(Team.Player2),
            gameManager.GetMillDetected(),
            gameManager.GetRemovalTeam()
        );
        undoStack.Push(state);
        redoStack.Clear();
    }

    public void Undo()
    {
        if (undoStack.Count <= 1) return;
        GameState current = undoStack.Pop();
        redoStack.Push(current);
        ApplyState(undoStack.Peek());
    }

    public void Redo()
    {
        if (redoStack.Count == 0) return;
        GameState state = redoStack.Pop();
        undoStack.Push(state);
        ApplyState(state);
    }

    private void ApplyState(GameState state)
    {
        foreach (var entry in state.boardState)
            entry.Key.ChangeCurrentPiece(entry.Value);

        foreach (var entry in state.piecePositions)
        {
            PieceSO pieceData = entry.Key;
            BoardSO targetBoard = entry.Value;

            if (!pieceMap.TryGetValue(pieceData, out Piece piece)) continue;

            pieceData.SetCurrentBoardSpace(targetBoard);

            if (targetBoard != null && boardMap.TryGetValue(targetBoard, out BoardObject boardObj))
            {
                piece.transform.SetParent(boardObj.transform);
                piece.transform.localPosition = Vector3.zero;
            }

            piece.gameObject.SetActive(state.pieceActiveState[pieceData]);
        }

        gameManager.RestoreState(
            state.currentTeam,
            state.team1Index,
            state.team2Index,
            state.piecesOnBoardTeam1,
            state.piecesOnBoardTeam2,
            state.waitingForRemoval,
            state.removalTeam
        );
    }
}