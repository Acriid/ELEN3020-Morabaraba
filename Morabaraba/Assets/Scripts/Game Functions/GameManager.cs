using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    //Board components
    //Teams
    [SerializeField] private List<Piece> _piecesTeam1;
    private int _team1Index = 0;
    [SerializeField] private List<Piece> _piecesTeam2;
    private int _team2Index = 0;
    [SerializeField] private List<BoardSO> _boardSOs;
    [SerializeField] private MillDetection _millDetection;

    //Phases
    private GamePhase _currentPhase = GamePhase.Place;
    private event Action<GamePhase> OnPhaseChange;

    //Move phase
    private BoardObject _selectedBoard = null;

    //Inputs
    private InputSystem_Actions _inputActions;
    private InputAction _mouseAction;

    void Awake()
    {   
        _millDetection.InitializeBoard(_boardSOs);
        InitializeInput();

        OnPhaseChange += ChangePhase;
    }
    void OnDisable()
    {
        CleanUpInputs();

        OnPhaseChange -= ChangePhase;
    }
    private void InitializeInput()
    {
        _inputActions = new();
        _mouseAction = _inputActions.Player.Click;

        
        _mouseAction.performed += OnClick;

        _mouseAction.Enable();
    }

    private void CleanUpInputs()
    {
        _mouseAction.performed -= OnClick;

        _mouseAction.Disable();
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        Vector3 rayOrigin = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin,Vector2.zero);

        if(hit == false) return;

        GameObject hitObject = hit.collider.gameObject;

        if(hitObject == null) return;

        if(_currentPhase == GamePhase.Place)
        {
            PlacePiece(hitObject);
        }
        else if(_currentPhase == GamePhase.Move)
        {
            MovePiece(hitObject);
        }
    }

    private void MovePiece(GameObject hitObject)
    {



        if (!hitObject.TryGetComponent<BoardObject>(out BoardObject boardComponent) && 
        !hitObject.transform.parent.TryGetComponent<BoardObject>(out boardComponent)) return;

        if (_selectedBoard == null)
        {
            if (!boardComponent.BoardSO.CheckIfSameTeam(Team.Player1)) return;

            _selectedBoard = boardComponent;
            Debug.Log($"Selected piece at {_selectedBoard.BoardSO.BoardID}");
            return;
        }

        //deselect
        if (boardComponent == _selectedBoard)
        {
            _selectedBoard = null;
            Debug.Log("Deselected piece");
            return;
        }

        // reselect
        if (boardComponent.BoardSO.CheckIfSameTeam(Team.Player1))
        {
            _selectedBoard = boardComponent;
            Debug.Log($"Reselected piece at {_selectedBoard.BoardSO.BoardID}");
            return;
        }

        // must be empty adjacent space
        if (boardComponent.BoardSO.GetCurrentPiece() != null) return;

        List<BoardSO> adjacent = _selectedBoard.BoardSO.GetAdjacentBoardSpaces();
        if (!adjacent.Contains(boardComponent.BoardSO))
        {
            Debug.Log("Not an adjacent space");
            return;
        }

        //move
        PieceSO movingPiece = _selectedBoard.BoardSO.GetCurrentPiece();

        _selectedBoard.BoardSO.ChangeCurrentPiece(null);        
        movingPiece.SetCurrentBoardSpace(boardComponent.BoardSO);
        boardComponent.BoardSO.ChangeCurrentPiece(movingPiece); 


        Piece pieceObject = FindPieceObject(movingPiece);
        if (pieceObject != null)
        {
            pieceObject.transform.SetParent(hitObject.transform);
            pieceObject.transform.localPosition = Vector3Int.zero;
        }

        Debug.Log($"Moved piece from {_selectedBoard.BoardSO.BoardID} to {boardComponent.BoardSO.BoardID}");

        _selectedBoard = null;

        if (_millDetection.DetectMill(boardComponent))
        {
            OnMill(Team.Player1);
        }
    }

    private Piece FindPieceObject(PieceSO pieceData)
    {
        foreach (Piece p in _piecesTeam1)
            if (p.data == pieceData) return p;
        foreach (Piece p in _piecesTeam2)
            if (p.data == pieceData) return p;
        return null;
    }

    private void PlacePiece(GameObject hitObject)
    {
        if(!hitObject.TryGetComponent<BoardObject>(out BoardObject boardComponent)) return;
        if(boardComponent.BoardSO.GetCurrentPiece() != null) return;

        Piece currentPiece = GetPieceForTeam(Team.Player1);

        if(currentPiece == null) return;


        Transform currentPieceTransform = currentPiece.transform;

        boardComponent.BoardSO.ChangeCurrentPiece(currentPiece.data);
        currentPiece.data.SetCurrentBoardSpace(boardComponent.BoardSO);

        currentPieceTransform.SetParent(hitObject.transform);
        currentPieceTransform.localPosition = Vector3Int.zero;

        if(_millDetection.DetectMill(boardComponent))
        {
            OnMill(Team.Player1);
        }


        if(_team1Index == _piecesTeam1.Count)
        {
            OnPhaseChange?.Invoke(GamePhase.Move);
        }
    }

    private void OnMill(Team team)
    {
        Debug.Log("Mill");
        
    }

    private Piece GetPieceForTeam(Team team)
    {
        Piece currentPiece = null;

        if(team == Team.Player1)
        {
            if(_team1Index >= _piecesTeam1.Count) return null;

            currentPiece = _piecesTeam1[_team1Index++];
            currentPiece.data.Team = Team.Player1;
        }
        else
        {
            if(_team2Index >= _piecesTeam2.Count) return null;

            currentPiece = _piecesTeam2[_team2Index++];
            currentPiece.data.Team = Team.Player2;          
        }

        return currentPiece;
    }

    private void ChangePhase(GamePhase newPhase)
    {
        _currentPhase = newPhase;
        Debug.Log($"Phase changed to {newPhase}");
    }
    private enum GamePhase
    {
        Place,
        Move,
        Fly
    }
}

