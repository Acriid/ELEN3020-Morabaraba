using System.Collections.Generic;
using Morabaraba;
using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    private Stack<GameState> undoStack = new Stack<GameState>();
    private Stack<GameState> redoStack = new Stack<GameState>();

    [SerializeField] private List<BoardSO> boardSpaces;

    public void SaveState()
    {
        undoStack.Push(new GameState(boardSpaces));
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
        {
            entry.Key.ChangeCurrentPiece(entry.Value);
        }
    }
}