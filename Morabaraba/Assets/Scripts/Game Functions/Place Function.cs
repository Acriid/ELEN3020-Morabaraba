using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlaceFunction : MonoBehaviour
{
    private InputSystem_Actions _inputActions;
    private InputAction _mouseAction;
    [SerializeField] private List<Piece> _piecesTeam1;
    [SerializeField] private List<Piece> _piecesTeam2;
    //We need something to differentiate the players for their teams
    private int _currentIndex1 = 0;
    private int _currentIndex2 = 0;
    private Team _currentTeam;
    void Awake()
    {

        //Initialize indexes
        _currentIndex1 = 0;
        _currentIndex2 = 0;

        _currentTeam = Team.Player1;

        //Initialize inputs
        _inputActions = new();
        _mouseAction = _inputActions.Player.Click;
        _mouseAction.performed += OnClick;
        _mouseAction.Enable();
    }

    void OnDisable()
    {
        _mouseAction.performed -= OnClick;
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        Vector3 rayOrigin = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin,Vector2.zero);

        if(hit.collider == null) return;
        if(hit.collider.TryGetComponent(out BoardObject component))
        {
            if(component.BoardSO.GetCurrentPiece() == null)
            {
                Piece currentPiece = GetPieceForTeam(_currentTeam);
                //Place piece on board
                component.BoardSO.ChangeCurrentPiece(currentPiece.data);
                currentPiece.gameObject.transform.SetParent(hit.collider.transform);
                currentPiece.gameObject.transform.localPosition = new(0f,0f,0f);

                //Set pieces BoardSpace
                currentPiece.data.SetCurrentBoardSpace(component.BoardSO);
            }
        }
    }

    private Piece GetPieceForTeam(Team team)
    {
        Piece currentPiece = null; 
        if(team == Team.Player1)
        {
            currentPiece =  _piecesTeam1[_currentIndex1++];
            currentPiece.data.Team = Team.Player1;
            _currentTeam = Team.Player2;
        }
        else if(team == Team.Player2)
        {
            currentPiece = _piecesTeam2[_currentIndex2++];
            currentPiece.data.Team = Team.Player2;
            _currentTeam = Team.Player1;
        }
        
        return currentPiece;
    }
}
