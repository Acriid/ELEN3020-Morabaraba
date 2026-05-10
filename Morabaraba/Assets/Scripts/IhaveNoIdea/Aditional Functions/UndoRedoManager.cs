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

    private bool _isRestoring = false;

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

        SaveState();
    }

    public void SaveState()
    {
        if (_isRestoring) return;

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

        if (undoStack.Count > 100)
        {
            GameState[] states = undoStack.ToArray();
            undoStack.Clear();

            for (int i = states.Length - 2; i >= 0; i--)
            {
                undoStack.Push(states[i]);
            }
        }

        redoStack.Clear();
    }

    public void Undo()
    {
        if (undoStack.Count <= 1) return;

        _isRestoring = true;

        GameState currentState = undoStack.Pop();
        redoStack.Push(currentState);

        GameState previousState = undoStack.Peek();

        ApplyState(previousState);

        _isRestoring = false;
    }

    public void Redo()
    {
        if (redoStack.Count == 0) return;

        _isRestoring = true;

        GameState redoState = redoStack.Pop();

        undoStack.Push(redoState);

        ApplyState(redoState);

        _isRestoring = false;
    }

    private void ApplyState(GameState state)
    {
        foreach (var board in boardSpaces)
        {
            board.ChangeCurrentPiece(null);
        }

        foreach (var entry in state.boardState)
        {
            entry.Key.ChangeCurrentPiece(entry.Value);
        }

        foreach (var entry in state.piecePositions)
        {
            PieceSO pieceData = entry.Key;
            BoardSO targetBoard = entry.Value;

            if (!pieceMap.TryGetValue(pieceData, out Piece piece))
                continue;

            piece.gameObject.SetActive(state.pieceActiveState[pieceData]);

            pieceData.SetCurrentBoardSpace(targetBoard);

            if (targetBoard != null)
            {
                if (boardMap.TryGetValue(targetBoard, out BoardObject boardObject))
                {
                    piece.transform.SetParent(boardObject.transform);
                    piece.transform.localPosition = Vector3.zero;
                }
            }
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