using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    //Board components
    //Teams
    [SerializeField] private List<Piece> _piecesTeam1;
    private int _piecesOnBoardTeam1 = 0;
    private int _team1Index = 0;
    [SerializeField] private List<Piece> _piecesTeam2;
    private int _team2Index = 0;
    private int _piecesOnBoardTeam2 = 0;
    [SerializeField] private List<BoardSO> _boardSOs;
    [SerializeField] private MillDetection _millDetection;
    private Team _currentTeam = Team.Player1;
    //Phases
    private GamePhase _currentPhase = GamePhase.Place;
    private event Action<GamePhase> OnPhaseChange;

    //Move phase
    private BoardObject _selectedBoard = null;

    //Inputs
    private InputSystem_Actions _inputActions;
    private InputAction _mouseAction;


    //Remove
    private bool _waitingForRemoval = false;
    private Team _removalTeam;

    public event Action onMoveDone;
    public event Action onMillGot;

    [SerializeField] private UndoRedoManager _undoRedoManager;

    public int GetTeam1Index() => _team1Index;
    public int GetTeam2Index() => _team2Index;
    public Team GetRemovalTeam() => _removalTeam;

    //New lookup dictionary - maps BoardID string -> BoardObject
    // This lets ClientRpcs find the right scene object from just a string ID
    private Dictionary<string, BoardObject> _boardObjectLookup = new();

    // OnNetworkSpawn fires on all clients once the object is fully spawned
    // on the network, which is the right time to initialise everything
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
    }

    void Awake()
    {
        // Input must always register immediately - don't wait for network
        InitializeInput();
    }

    void OnDisable()
    {
        CleanUp();
    }
    public void Initialize()
    {
        Debug.Log($"Initialize called - _boardSOs count: {(_boardSOs == null ? "NULL" : _boardSOs.Count.ToString())}");

        _millDetection.InitializeBoard(_boardSOs);

        // Build the BoardObject lookup once at startup
        // Both clients have the same scene so both get the same dictionary
        _boardObjectLookup.Clear();
        foreach (BoardObject bo in FindObjectsByType<BoardObject>(FindObjectsSortMode.None))
            _boardObjectLookup[bo.BoardSO.BoardID] = bo;


        OnPhaseChange += ChangePhase;
        onMoveDone += FinishedMove;
    }
    public void CleanUp()
    {
        CleanUpInputs();

        OnPhaseChange -= ChangePhase;
        onMoveDone -= FinishedMove;
    }


    // Two helpers to know which team the local player controls
    // Convention: host = Player1, joining client = Player2
    private Team GetLocalPlayerTeam()
    {
        return NetworkManager.Singleton.IsHost ? Team.Player1 : Team.Player2;
    }

    private bool IsMyTurn()
    {
        return _currentTeam == GetLocalPlayerTeam();
    }


    private void OnClick(InputAction.CallbackContext ctx)
    {
        Debug.Log($"Click detected - IsHost: {NetworkManager.Singleton.IsHost}, CurrentTeam: {_currentTeam}, IsMyTurn: {IsMyTurn()}");

        // Raycast
        Vector3 rayOrigin = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero);

        if (!hit) return;

        GameObject hitObject = hit.collider.gameObject;

        if (hitObject == null) return;

        // Get BoardObject
        if (!hitObject.TryGetComponent<BoardObject>(out BoardObject boardComponent) &&
            !hitObject.transform.parent.TryGetComponent<BoardObject>(out boardComponent))
        {
            return;
        }

        string boardID = boardComponent.BoardSO.BoardID;

        if (_waitingForRemoval)
        {
            // Only the player who formed the mill can remove
            if (GetLocalPlayerTeam() == _removalTeam)
                return;

            PieceSO pieceData = boardComponent.BoardSO.GetCurrentPiece();

            if (pieceData == null || pieceData.Team != _removalTeam)
                return;

            bool pieceIsInMill = _millDetection.IsPieceInAMill(boardComponent.BoardSO);
            bool allInMills = _millDetection.AllTeamPiecesInMills(_removalTeam, _boardSOs);

            if (pieceIsInMill && !allInMills)
            {
                Debug.Log("Cannot remove a piece that is part of a mill");
                return;
            }

            SubmitRemovePieceServerRpc(boardID);
            return;
        }

        if (!IsMyTurn())
            return;

        if (_currentPhase == GamePhase.Place)
        {
            if (boardComponent.BoardSO.GetCurrentPiece() != null)
                return;

            SubmitPlacePieceServerRpc(boardID);
            return;
        }

        if (IsCurrentPlayerFlying())
        {
            HandleFlySelectionLocally(boardID, boardComponent);
        }
        else
        {
            HandleMoveSelectionLocally(boardID, boardComponent);
        }
    }

    private bool IsCurrentPlayerFlying()
    {
        if (_currentTeam == Team.Player1)
        {
            return _piecesOnBoardTeam1 == 3 &&
                   _team1Index == _piecesTeam1.Count;
        }

        return _piecesOnBoardTeam2 == 3 &&
               _team2Index == _piecesTeam2.Count;
    }

    // =========================================================================
    // PLACE - RPC PAIR
    // =========================================================================

    // Step 1: The active client tells the server which board space they clicked

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SubmitPlacePieceServerRpc(string boardID)
    {
        Debug.Log($"ServerRpc received - boardID: {boardID}, lookup count: {_boardObjectLookup.Count}");

        BoardObject boardObj = GetBoardObject(boardID);
        if (boardObj == null) { Debug.Log("FAILED: boardID not found in lookup"); return; }

        if (boardObj.BoardSO.GetCurrentPiece() != null) { Debug.Log("FAILED: space already occupied"); return; }
        if (_currentPhase != GamePhase.Place) { Debug.Log($"FAILED: wrong phase - {_currentPhase}"); return; }

        int pieceIndex = _currentTeam == Team.Player1 ? _team1Index : _team2Index;

        // Detect mill here on the server where the lookup is correctly populated
        Piece piece = _currentTeam == Team.Player1 ? _piecesTeam1[pieceIndex] : _piecesTeam2[pieceIndex];
        piece.data.Team = _currentTeam;
        boardObj.BoardSO.ChangeCurrentPiece(piece.data);
        piece.data.SetCurrentBoardSpace(boardObj.BoardSO);

        bool millDetected = _millDetection.DetectMill(boardObj);

        // Clean up the temporary state - clients will apply it properly
        boardObj.BoardSO.ChangeCurrentPiece(null);
        piece.data.SetCurrentBoardSpace(null);

        Debug.Log($"ServerRpc passed - pieceIndex: {pieceIndex}, phase: {_currentPhase}, mill: {millDetected}");
        ExecutePlacePieceClientRpc(boardID, pieceIndex, _currentTeam, millDetected);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ExecutePlacePieceClientRpc(string boardID, int pieceIndex, Team team, bool millDetected)
    {
        Debug.Log($"ClientRpc reached on {(NetworkManager.Singleton.IsHost ? "Host" : "Client")}");

        BoardObject boardObj = GetBoardObject(boardID);
        if (boardObj == null) { Debug.Log("ClientRpc FAILED: board not found"); return; }

        Piece piece = team == Team.Player1 ? _piecesTeam1[pieceIndex] : _piecesTeam2[pieceIndex];
        piece.data.Team = team;

        boardObj.BoardSO.ChangeCurrentPiece(piece.data);
        piece.data.SetCurrentBoardSpace(boardObj.BoardSO);

        piece.transform.SetParent(boardObj.transform);
        piece.transform.localPosition = Vector3.zero;

        if (team == Team.Player1) { _team1Index++; _piecesOnBoardTeam1++; }
        else { _team2Index++; _piecesOnBoardTeam2++; }

        // Use the server's result instead of re-detecting
        if (millDetected)
        {
            OnMill(GetOppositeTeam(team));
        }
        else
        {
            _currentTeam = GetOppositeTeam(team);
            onMoveDone?.Invoke();
        }

        if (_team1Index == _piecesTeam1.Count && _team2Index == _piecesTeam2.Count)
        {
            bool team1ShouldFly = _piecesOnBoardTeam1 == 3;
            bool team2ShouldFly = _piecesOnBoardTeam2 == 3;

            if (team1ShouldFly || team2ShouldFly)
                OnPhaseChange?.Invoke(GamePhase.Fly);
            else
                OnPhaseChange?.Invoke(GamePhase.Move);
        }

        _undoRedoManager?.SaveState();
    }

    // =========================================================================
    // MOVE - selection is local (just UI state), the actual move is an RPC pair
    // =========================================================================

    // Handles the two-click flow locally (select then submit)
    // _selectedBoard is only used on the active player's client, which is fine
    private void HandleMoveSelectionLocally(string boardID, BoardObject boardComponent)
    {
        if (_selectedBoard == null)
        {
            if (!boardComponent.BoardSO.CheckIfSameTeam(_currentTeam)) return;
            _selectedBoard = boardComponent;
            Debug.Log($"Selected piece at {boardID}");
            return;
        }

        // Deselect
        if (boardComponent == _selectedBoard)
        {
            _selectedBoard = null;
            Debug.Log("Deselected piece");
            return;
        }

        // Reselect a different friendly piece
        if (boardComponent.BoardSO.CheckIfSameTeam(_currentTeam))
        {
            _selectedBoard = boardComponent;
            Debug.Log($"Reselected piece at {boardID}");
            return;
        }

        // Must be an empty adjacent space
        if (boardComponent.BoardSO.GetCurrentPiece() != null) return;

        List<BoardSO> adjacent = _selectedBoard.BoardSO.GetAdjacentBoardSpaces();
        if (!adjacent.Contains(boardComponent.BoardSO))
        {
            Debug.Log("Not an adjacent space");
            return;
        }

        // Move confirmed - send to server
        SubmitMovePieceServerRpc(_selectedBoard.BoardSO.BoardID, boardID);
        _selectedBoard = null;
    }

    // Step 1: Active client sends from/to board IDs
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SubmitMovePieceServerRpc(string fromBoardID, string toBoardID)
    {
        if (!_boardObjectLookup.TryGetValue(fromBoardID, out BoardObject fromObj)) return;
        if (!_boardObjectLookup.TryGetValue(toBoardID, out BoardObject toObj)) return;

        PieceSO movingPiece = fromObj.BoardSO.GetCurrentPiece();
        if (movingPiece == null || movingPiece.Team != _currentTeam) return;
        if (toObj.BoardSO.GetCurrentPiece() != null) return;

        List<BoardSO> adjacent = fromObj.BoardSO.GetAdjacentBoardSpaces();
        if (!adjacent.Contains(toObj.BoardSO)) return;

        // Temporarily apply move to detect mill on server
        fromObj.BoardSO.ChangeCurrentPiece(null);
        movingPiece.SetCurrentBoardSpace(toObj.BoardSO);
        toObj.BoardSO.ChangeCurrentPiece(movingPiece);

        bool millDetected = _millDetection.DetectMill(toObj);

        // Revert temporary state - clients will apply it properly
        toObj.BoardSO.ChangeCurrentPiece(null);
        movingPiece.SetCurrentBoardSpace(fromObj.BoardSO);
        fromObj.BoardSO.ChangeCurrentPiece(movingPiece);

        ExecuteMovePieceClientRpc(fromBoardID, toBoardID, millDetected);
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void ExecuteMovePieceClientRpc(string fromBoardID, string toBoardID, bool millDetected)
    {
        if (!_boardObjectLookup.TryGetValue(fromBoardID, out BoardObject fromObj)) return;
        if (!_boardObjectLookup.TryGetValue(toBoardID, out BoardObject toObj)) return;

        PieceSO movingPiece = fromObj.BoardSO.GetCurrentPiece();

        fromObj.BoardSO.ChangeCurrentPiece(null);
        movingPiece.SetCurrentBoardSpace(toObj.BoardSO);
        toObj.BoardSO.ChangeCurrentPiece(movingPiece);

        Piece pieceObject = FindPieceObject(movingPiece);
        if (pieceObject != null)
        {
            pieceObject.transform.SetParent(toObj.transform);
            pieceObject.transform.localPosition = Vector3.zero;
        }

        Debug.Log($"Moved piece from {fromBoardID} to {toBoardID}");

        if (millDetected)
        {
            OnMill(GetOppositeTeam(_currentTeam));
            // Don't switch _currentTeam - the miller still needs to click to remove
        }
        else
        {
            _currentTeam = GetOppositeTeam(_currentTeam); // fix - update internally
            onMoveDone?.Invoke();
        }
    }

    // =========================================================================
    // FLY - same two-click pattern as Move, but no adjacency check
    // =========================================================================

    private void HandleFlySelectionLocally(string boardID, BoardObject boardComponent)
    {
        if (_selectedBoard == null)
        {
            if (!boardComponent.BoardSO.CheckIfSameTeam(_currentTeam)) return;
            _selectedBoard = boardComponent;
            Debug.Log($"Selected piece at {boardID}");
            return;
        }

        if (boardComponent == _selectedBoard)
        {
            _selectedBoard = null;
            Debug.Log("Deselected piece");
            return;
        }

        if (boardComponent.BoardSO.CheckIfSameTeam(_currentTeam))
        {
            _selectedBoard = boardComponent;
            Debug.Log($"Reselected piece at {boardID}");
            return;
        }

        if (boardComponent.BoardSO.GetCurrentPiece() != null) return;

        SubmitFlyPieceServerRpc(_selectedBoard.BoardSO.BoardID, boardID);
        _selectedBoard = null;
    }



    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SubmitFlyPieceServerRpc(string fromBoardID, string toBoardID)
    {
        if (!_boardObjectLookup.TryGetValue(fromBoardID, out BoardObject fromObj)) return;
        if (!_boardObjectLookup.TryGetValue(toBoardID, out BoardObject toObj)) return;

        PieceSO movingPiece = fromObj.BoardSO.GetCurrentPiece();
        if (movingPiece == null || movingPiece.Team != _currentTeam) return;
        if (toObj.BoardSO.GetCurrentPiece() != null) return;

        // Temporarily apply move to detect mill on server
        fromObj.BoardSO.ChangeCurrentPiece(null);
        movingPiece.SetCurrentBoardSpace(toObj.BoardSO);
        toObj.BoardSO.ChangeCurrentPiece(movingPiece);

        bool millDetected = _millDetection.DetectMill(toObj);

        // Revert temporary state - clients will apply it properly
        toObj.BoardSO.ChangeCurrentPiece(null);
        movingPiece.SetCurrentBoardSpace(fromObj.BoardSO);
        fromObj.BoardSO.ChangeCurrentPiece(movingPiece);

        ExecuteFlyPieceClientRpc(fromBoardID, toBoardID, millDetected);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ExecuteFlyPieceClientRpc(string fromBoardID, string toBoardID, bool millDetected)
    {
        if (!_boardObjectLookup.TryGetValue(fromBoardID, out BoardObject fromObj)) return;
        if (!_boardObjectLookup.TryGetValue(toBoardID, out BoardObject toObj)) return;

        PieceSO movingPiece = fromObj.BoardSO.GetCurrentPiece();

        fromObj.BoardSO.ChangeCurrentPiece(null);
        movingPiece.SetCurrentBoardSpace(toObj.BoardSO);
        toObj.BoardSO.ChangeCurrentPiece(movingPiece);

        Piece pieceObject = FindPieceObject(movingPiece);
        if (pieceObject != null)
        {
            pieceObject.transform.SetParent(toObj.transform);
            pieceObject.transform.localPosition = Vector3.zero;
        }

        Debug.Log($"Flew piece from {fromBoardID} to {toBoardID}");

        if (millDetected)
        {
            OnMill(GetOppositeTeam(_currentTeam));
        }
        else
        {
            _currentTeam = GetOppositeTeam(_currentTeam); // fix - update internally
            onMoveDone?.Invoke();
        }
    }

    // =========================================================================
    // REMOVE - RPC PAIR
    // =========================================================================

    // Step 1: Active client sends the board ID of the piece to remove
    // (local pre-validation already happened in OnClick before this is called)
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]

    private void SubmitRemovePieceServerRpc(string boardID)
    {
        BoardObject boardObj = GetBoardObject(boardID);
        if (boardObj == null) { Debug.Log("boardObj was null"); return; }

        PieceSO pieceData = boardObj.BoardSO.GetCurrentPiece();
        if (pieceData == null || pieceData.Team != _removalTeam) return;

        // Server re-validates mill rules
        bool pieceIsInMill = _millDetection.IsPieceInAMill(boardObj.BoardSO);
        bool allInMills = _millDetection.AllTeamPiecesInMills(_removalTeam, _boardSOs);
        if (pieceIsInMill && !allInMills) return;

        ExecuteRemovePieceClientRpc(boardID);
    }

    // Step 2: All clients remove the piece
    [Rpc(SendTo.ClientsAndHost)]
    private void ExecuteRemovePieceClientRpc(string boardID)
    {
        BoardObject boardObj = GetBoardObject(boardID);
        if (boardObj == null) { Debug.Log("boardObj was null"); return; }

        PieceSO pieceData = boardObj.BoardSO.GetCurrentPiece();
        if (pieceData == null) return;

        Piece piece = FindPieceObject(pieceData);
        if (piece == null) return;

        // Clear ScriptableObject state
        boardObj.BoardSO.ChangeCurrentPiece(null);
        piece.data.SetCurrentBoardSpace(null);

        // Update counter
        if (piece.data.Team == Team.Player1) _piecesOnBoardTeam1--;
        else if (piece.data.Team == Team.Player2) _piecesOnBoardTeam2--;

        piece.gameObject.SetActive(false);
        _waitingForRemoval = false;

        Debug.Log($"Removed piece {piece.data.PieceID}");

        // Switch to opponent's turn now that removal is done
        _currentTeam = _removalTeam;

        // Check whether this triggers the fly phase
        if (_piecesOnBoardTeam1 == 3 && _team1Index == _piecesTeam1.Count)
            OnPhaseChange?.Invoke(GamePhase.Fly);
        else if (_piecesOnBoardTeam2 == 3 && _team2Index == _piecesTeam2.Count)
            OnPhaseChange?.Invoke(GamePhase.Fly);

        // Check for loss
        if (DidTeamLose(piece.data.Team))
        {
            Team winningTeam = GetOppositeTeam(piece.data.Team);
            Debug.Log($"{piece.data.Team} lost");
            EndGame(winningTeam);
        }

        onMoveDone?.Invoke();
        // _undoRedoManager.SaveState();
    }


    private void FinishedMove()
    {
        if (_currentPhase != GamePhase.Move) return;
        if (!CurrentTeamHasValidMove())
        {
            Team winningTeam = GetOppositeTeam(_currentTeam);
            EndGame(winningTeam);
        }
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
        if (_mouseAction == null) return;


        _mouseAction.performed -= OnClick;

        _mouseAction.Disable();
    }


    //OnClick now guards on turn ownership,
    // and routes to ServerRpcs instead of calling game logic directly


    public void FlyPiece(GameObject hitObject)
    {
        if (!hitObject.TryGetComponent<BoardObject>(out BoardObject boardComponent) &&
        !hitObject.transform.parent.TryGetComponent<BoardObject>(out boardComponent)) return;

        if (_selectedBoard == null)
        {
            if (!boardComponent.BoardSO.CheckIfSameTeam(_currentTeam)) return;

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
        if (boardComponent.BoardSO.CheckIfSameTeam(_currentTeam))
        {
            _selectedBoard = boardComponent;
            Debug.Log($"Reselected piece at {_selectedBoard.BoardSO.BoardID}");
            return;
        }

        // must be empty adjacent space
        if (boardComponent.BoardSO.GetCurrentPiece() != null) return;

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
            OnMill(GetOppositeTeam(_currentTeam));
        }
        else
        {
            onMoveDone?.Invoke();
        }

        // _undoRedoManager.SaveState();


    }

    public void MovePiece(GameObject hitObject)
    {



        if (!hitObject.TryGetComponent<BoardObject>(out BoardObject boardComponent) &&
        !hitObject.transform.parent.TryGetComponent<BoardObject>(out boardComponent)) return;

        if (_selectedBoard == null)
        {
            if (!boardComponent.BoardSO.CheckIfSameTeam(_currentTeam)) return;

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
        if (boardComponent.BoardSO.CheckIfSameTeam(_currentTeam))
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
            OnMill(GetOppositeTeam(_currentTeam));
        }
        else
        {
            onMoveDone?.Invoke();
        }

        // _undoRedoManager.SaveState();


    }

    public Team GetOppositeTeam(Team currentTeam)
    {
        if (currentTeam == Team.Player1)
        {
            return Team.Player2;
        }
        else
        {
            return Team.Player1;
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




    public Piece PlacePiece(GameObject hitObject)
    {
        if (!hitObject.TryGetComponent<BoardObject>(out BoardObject boardComponent)) return null;
        if (boardComponent.BoardSO.GetCurrentPiece() != null) return null;

        Piece currentPiece = GetPieceForTeam(_currentTeam);

        if (currentPiece == null) return null;


        Transform currentPieceTransform = currentPiece.transform;

        boardComponent.BoardSO.ChangeCurrentPiece(currentPiece.data);
        currentPiece.data.SetCurrentBoardSpace(boardComponent.BoardSO);

        currentPieceTransform.SetParent(hitObject.transform);
        currentPieceTransform.localPosition = Vector3Int.zero;

        if (_millDetection.DetectMill(boardComponent))
        {
            OnMill(GetOppositeTeam(_currentTeam));
        }
        else
        {
            onMoveDone?.Invoke();
        }


        if (_team1Index == _piecesTeam1.Count && _team2Index == _piecesTeam2.Count)
        {
            if (GetPiecesOnBoardForTeam(_currentTeam) != 3)
            {
                OnPhaseChange?.Invoke(GamePhase.Move);
            }
            else if (GetPiecesOnBoardForTeam(_currentTeam) == 3)
            {
                OnPhaseChange?.Invoke(GamePhase.Fly);
            }

        }

        // _undoRedoManager.SaveState();

        return currentPiece;
    }

    public int GetPiecesOnBoardForTeam(Team teamToGet)
    {
        int result = teamToGet == Team.Player1 ? _piecesOnBoardTeam1 : _piecesOnBoardTeam2;
        return result;
    }
    public void RemovePiece(Piece piece)
    {
        BoardSO currentSpace = piece.data.GetCurrentBoardSpace();

        if (currentSpace != null)
        {
            currentSpace.ChangeCurrentPiece(null);
            piece.data.SetCurrentBoardSpace(null);
        }

        if (piece.data.Team == Team.Player1)
        {
            _piecesOnBoardTeam1--;
        }
        else if (piece.data.Team == Team.Player2)
        {
            _piecesOnBoardTeam2--;
        }

        piece.gameObject.SetActive(false);
        Debug.Log($"Removed piece {piece.data.PieceID}");

        if (_piecesOnBoardTeam1 == 3 && _team1Index == _piecesTeam1.Count)
        {
            OnPhaseChange?.Invoke(GamePhase.Fly);
        }
        else if (_piecesOnBoardTeam2 == 3 && _team2Index == _piecesTeam2.Count)
        {
            OnPhaseChange?.Invoke(GamePhase.Fly);
        }

        if (DidTeamLose(piece.data.Team))
        {
            Team winningTeam = GetOppositeTeam(piece.data.Team);
            Debug.Log($"{piece.data.Team} lost");
            EndGame(winningTeam);
        }
        onMoveDone?.Invoke();

        // _undoRedoManager.SaveState();
    }

    private void EndGame(Team winningTeam)
    {
        //Need to implement
        Debug.Log($"{winningTeam} won");
    }

    public bool DidTeamLose(Team teamToCheck)
    {
        if (GetPiecesOnBoardForTeam(teamToCheck) < 3 && (_currentPhase == GamePhase.Move || _currentPhase == GamePhase.Fly))
            return true;
        return false;
    }
    public void WaitAndRemovePiece(Team team)
    {
        _waitingForRemoval = true;
        _removalTeam = team;
        Debug.Log($"Waiting to remove a {team} piece");
    }

    public void HandleRemovalClick(GameObject hitObject)
    {
        if (!hitObject.TryGetComponent<BoardObject>(out BoardObject boardComponent) &&
            !hitObject.transform.parent.TryGetComponent<BoardObject>(out boardComponent)) return;

        PieceSO pieceData = boardComponent.BoardSO.GetCurrentPiece();
        if (pieceData == null || pieceData.Team != _removalTeam) return;

        Piece piece = FindPieceObject(pieceData);
        if (piece == null) return;

        bool pieceIsInMill = _millDetection.IsPieceInAMill(boardComponent.BoardSO);
        bool allInMills = _millDetection.AllTeamPiecesInMills(_removalTeam, _boardSOs);

        if (pieceIsInMill && !allInMills)
        {
            Debug.Log("Cannot remove a piece that is part of a mill");
            return;
        }

        RemovePiece(piece);
        _waitingForRemoval = false;
    }










    private void OnMill(Team team)
    {
        WaitAndRemovePiece(team);
        onMillGot?.Invoke();
    }

    private Piece GetPieceForTeam(Team team)
    {
        Piece currentPiece = null;

        if (team == Team.Player1)
        {
            if (_team1Index >= _piecesTeam1.Count) return null;

            currentPiece = _piecesTeam1[_team1Index++];
            _piecesOnBoardTeam1++;
            currentPiece.data.Team = Team.Player1;
        }
        else
        {
            if (_team2Index >= _piecesTeam2.Count) return null;

            currentPiece = _piecesTeam2[_team2Index++];
            _piecesOnBoardTeam2++;
            currentPiece.data.Team = Team.Player2;
        }

        return currentPiece;
    }

    public void SetCurrentTeam(Team newTeam)
    {
        _currentTeam = newTeam;
    }
    public Team GetCurrentTeam()
    {
        return _currentTeam;
    }
    private void ChangePhase(GamePhase newPhase)
    {
        _currentPhase = newPhase;
        Debug.Log($"Phase changed to {newPhase}");
    }


    #region Private Initialization
    public void SetTeam1Pieces(List<Piece> newList)
    {
        _piecesTeam1 = new(newList);
    }
    public void SetTeam2Pieces(List<Piece> newList)
    {
        _piecesTeam2 = new(newList);
    }


    // If any external script calls SetBoardScriptableObjects after Awake,
    //  the lookup is built from an empty/wrong list and then the real list
    //   replaces it without re - initialising the lookup.
    public void SetBoardScriptableObjects(List<BoardSO> newList)
    {
        Debug.Log($"SetBoardScriptableObjects called - count: {_boardSOs.Count}");

        _boardSOs = new(newList);
        _millDetection.InitializeBoard(_boardSOs); // re-build lookup with correct list

    }
    public void SetMillDetection(MillDetection newMillDetection)
    {
        _millDetection = newMillDetection;
        _millDetection.InitializeBoard(_boardSOs); // initialise the new instance too

    }
    #endregion

    public bool GetMillDetected()
    {
        return _waitingForRemoval;
    }

    public List<BoardSO> GetBoard()
    {
        return _boardSOs;
    }

    // New helper to get the BoardObject from a BoardID string - used by RPCs to find the right scene object so pieces move correctly on all clients
    private BoardObject GetBoardObject(string boardID)
    {
        if (_boardObjectLookup.Count == 0)
        {
            foreach (BoardObject bo in FindObjectsByType<BoardObject>(FindObjectsSortMode.None))
                _boardObjectLookup[bo.BoardSO.BoardID] = bo;
        }

        // Temporary - log all keys so we can see what's actually stored
        if (!_boardObjectLookup.ContainsKey(boardID))
        {
            string allKeys = string.Join(", ", _boardObjectLookup.Keys);
            Debug.Log($"'{boardID}' not found. All keys: {allKeys}");
        }

        _boardObjectLookup.TryGetValue(boardID, out BoardObject result);
        return result;
    }

    public Dictionary<Team, List<BoardSO>> GetFinalMillBoard()
    {
        return _millDetection.GetFinalMillBoard();
    }
    public List<BoardSO> GetPotentialMills(Team teamToGet)
    {
        return _millDetection.GetPotentialMills(teamToGet);
    }
    public GamePhase GetCurrentPhase()
    {
        return _currentPhase;
    }
    public MillDetection GetMillDetection()
    {
        return _millDetection;
    }

    public bool CurrentTeamHasValidMove()
    {
        if (_currentPhase == GamePhase.Fly)
        {
            // In fly phase, a move is valid if there is any empty space on the board
            return _boardSOs.Exists(b => b.GetCurrentPiece() == null);
        }

        // In move phase, at least one friendly piece must have an empty adjacent space
        foreach (BoardSO board in _boardSOs)
        {
            PieceSO piece = board.GetCurrentPiece();
            if (piece == null || piece.Team != _currentTeam) continue;

            if (board.GetAdjacentBoardSpaces().Exists(b => b.GetCurrentPiece() == null))
                return true;
        }

        return false;
    }
    public enum GamePhase
    {
        Place,
        Move,
        Fly
    }

    public void RestoreState(
    Team currentTeam,
    int team1Index,
    int team2Index,
    int piecesOnBoardTeam1,
    int piecesOnBoardTeam2,
    bool waitingForRemoval,
    Team removalTeam)
    {
        _currentTeam = currentTeam;
        _team1Index = team1Index;
        _team2Index = team2Index;
        _piecesOnBoardTeam1 = piecesOnBoardTeam1;
        _piecesOnBoardTeam2 = piecesOnBoardTeam2;
        _waitingForRemoval = waitingForRemoval;
        _removalTeam = removalTeam;
        if (_team1Index < _piecesTeam1.Count || _team2Index < _piecesTeam2.Count)
        {
            _currentPhase = GamePhase.Place;
        }
        else if (_piecesOnBoardTeam1 == 3 || _piecesOnBoardTeam2 == 3)
        {
            _currentPhase = GamePhase.Fly;
        }
        else
        {
            _currentPhase = GamePhase.Move;
        }
    }
}

