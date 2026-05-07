using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AiBrain : MonoBehaviour
{
    [SerializeField] private Team _aiTeam;
    [SerializeField] private GameManager _gameManager;
    private List<BoardSO> _boardList;
    private List<BoardSO> _boardInterestList;
    [SerializeField] private List<BoardObject> _boardObjects;

    private  Dictionary<BoardSO, BoardObject> _boardLookup = new();

    public  void InitializeBoard(IEnumerable<BoardObject> allBoardSpaces)
    {
        _boardLookup.Clear();
        foreach (BoardObject board in allBoardSpaces)
        {
            _boardLookup[board.BoardSO] = board;
        }
    }


    public void SetGameManager(GameManager gameManager)
    {
        _gameManager = gameManager;
        _boardList = _gameManager.GetBoard();
    }
    public void SetTeam(Team newTeam)
    {
        _aiTeam = newTeam;
    }
    public Team GetTeam()
    {
        return _aiTeam;
    }

    void Start()
    {
        InitializeBoard(_boardObjects);
        _gameManager.SetCurrentTeam(_gameManager.GetOppositeTeam(_aiTeam));
        _gameManager.onMoveDone += Move;
        _gameManager.onMillGot += RemovePiece;
        _boardInterestList = new();
        // for(int i = 0 ; i < 12 ; i++)
        // {
        //     PlacePiece();
        // }
    }
    void OnDisable()
    {
        _gameManager.onMoveDone -= Move;
        _gameManager.onMillGot -= RemovePiece;
    }
    private void Move()
    {
        if(_gameManager.GetCurrentPhase() == GameManager.GamePhase.Place)
        {
            if(_gameManager.GetCurrentTeam() == _aiTeam) return;
            _gameManager.SetCurrentTeam(_aiTeam);
            Place(); 
        }
        else if(_gameManager.GetCurrentPhase() == GameManager.GamePhase.Move)
        {
            Debug.Log($"Want to move {_boardLookup[GetMovePiece()].gameObject.name}");
        }

        _gameManager.SetCurrentTeam(_gameManager.GetOppositeTeam(_aiTeam));
    }
    private void RemovePiece()
    {
        if(_gameManager.GetCurrentTeam() != _aiTeam) return;

        _gameManager.HandleRemovalClick(_boardLookup[GetRemoveBoard()].gameObject);
        _gameManager.SetCurrentTeam(_gameManager.GetOppositeTeam(_aiTeam));
    }
    //Look at board
    //See enemy positions
    //Get possible mills
    //Could do something like wave function collapse
    private BoardSO DecideNextMove()
    {
        List<BoardSO> blockList = _gameManager.GetFinalMillBoard()[_gameManager.GetOppositeTeam(_aiTeam)];
        List<BoardSO> millList = _gameManager.GetFinalMillBoard()[_aiTeam];
        List<BoardSO> millBlockList = GetMillAndBlockSpaces();

        BoardSO boardMove = null;
        if(millBlockList.Count > 0)
        {
            boardMove = millBlockList[Random.Range(0,millBlockList.Count)];
            return boardMove;
        }

        if(millList.Count > 0)
        {
            boardMove = millList[Random.Range(0,millList.Count)];
            Debug.Log($"Mill Found At {boardMove.BoardID}");
            return boardMove;
        }

        if(blockList.Count > 0)
        {
            boardMove = blockList[Random.Range(0,blockList.Count)];
            return boardMove;
        }


        return boardMove;
    }
    private BoardSO GetRemoveBoard()
    {
        List<BoardSO> boardsToRemove = _gameManager.GetPotentialMills(_gameManager.GetOppositeTeam(_aiTeam));
        BoardSO boardToRemove = boardsToRemove[Random.Range(0,boardsToRemove.Count)];
        return boardToRemove;
    }

    private BoardSO GetMovePiece()
    {
        List<BoardSO> potentialMills = _gameManager.GetPotentialMills(_aiTeam);
        List<BoardSO> potentialMoves = new(_boardInterestList);

        potentialMills.RemoveAll(item => potentialMills.Contains(item));
        if(potentialMills.Count !> 0)
        {
            //Return a random piece to move
            //Currently Returns null
            Debug.Log("No piece to move.");
            return null;
        }
        BoardSO movePiece = potentialMills[Random.Range(0,potentialMills.Count)];

        return movePiece;
    }
    private BoardSO RandomBoardPlaceSpot()
    {
        BoardSO moveSpot = null;
        while(moveSpot == null || moveSpot.GetCurrentPiece() != null)
        {
            moveSpot = _boardObjects[Random.Range(0,_boardObjects.Count)].BoardSO;
        }

        return moveSpot;
    }
    private BoardSO AdjacentBoardPlaceSpot()
    {
        if(_boardInterestList.Count == 0) return null;

        List<BoardSO> possibleSpots = new();
        List<BoardSO> finaleSpots = new();
        foreach(BoardSO board in _boardInterestList)
        {
            possibleSpots.AddRange(board.GetAdjacentBoardSpaces());
        }
        finaleSpots.AddRange(possibleSpots);
        foreach(BoardSO board in possibleSpots)
        {
            if(board.GetCurrentPiece() != null)
            {
                finaleSpots.Remove(board);
            }
        }

        return finaleSpots[Random.Range(0,finaleSpots.Count)];
    }

    public List<BoardSO> GetMillAndBlockSpaces()
    {
        Dictionary<Team, List<BoardSO>> possibleMills = _gameManager.GetFinalMillBoard();

        HashSet<BoardSO> team1Spaces = new(possibleMills[Team.Player1]);
        HashSet<BoardSO> team2Spaces = new(possibleMills[Team.Player2]);

        team1Spaces.IntersectWith(team2Spaces);

        return new List<BoardSO>(team1Spaces);
    }    

    public void Place()
    {

        BoardSO move = DecideNextMove();


        if(move == null && _boardInterestList.Count == 0)
        {
            move = RandomBoardPlaceSpot();
        }
        else if(move == null && _boardInterestList.Count > 0)
        {
            move = AdjacentBoardPlaceSpot();
            if(move == null)
            {
                move = RandomBoardPlaceSpot();
            }
        }


        _boardInterestList.Add(move);


        _gameManager.PlacePiece(_boardLookup[move].gameObject);
        Debug.Log($"Played {move.BoardID}");
    }
    public enum AiDifficulty
    {
        Easy,
        Normal,
        Hard
    }
}


