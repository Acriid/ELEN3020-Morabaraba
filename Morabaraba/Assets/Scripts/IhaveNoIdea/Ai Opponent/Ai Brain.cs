using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AiBrain : MonoBehaviour
{
    [SerializeField] private Team _aiTeam;
    [SerializeField] private GameManagerAI _gameManager;
    private List<BoardSO> _boardInterestList;
    [SerializeField] private List<BoardObject> _boardObjects;

    private  Dictionary<BoardSO, BoardObject> _boardLookup = new();
    [SerializeField] private AiDifficulty? aiDifficulty = null;

    public  void InitializeBoard(IEnumerable<BoardObject> allBoardSpaces)
    {
        _boardLookup.Clear();
        foreach (BoardObject board in allBoardSpaces)
        {
            _boardLookup[board.BoardSO] = board;
        }
    }

    public void SetAIDifficulty(AiDifficulty aiDifficulty)
    {
        this.aiDifficulty = aiDifficulty;
        Debug.Log($"Ai is {aiDifficulty}");
    }

    public void SetGameManager(GameManagerAI gameManager)
    {
        _gameManager = gameManager;
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
    }
    void OnDisable()
    {
        _gameManager.onMoveDone -= Move;
        _gameManager.onMillGot -= RemovePiece;
    }


    private void RemovePiece()
    {
        if(_gameManager.GetCurrentTeam() != _aiTeam) return;

        _gameManager.HandleRemovalClick(_boardLookup[GetRemoveBoard()].gameObject);
        _gameManager.SetCurrentTeam(_gameManager.GetOppositeTeam(_aiTeam));
    }

    private BoardSO DecideNextPlace()
    {
        List<BoardSO> blockList = _gameManager.GetFinalMillBoard()[_gameManager.GetOppositeTeam(_aiTeam)];
        List<BoardSO> millList = _gameManager.GetFinalMillBoard()[_aiTeam];
        List<BoardSO> millBlockList = GetMillAndBlockSpaces();

        BoardSO boardMove = null;

        if(aiDifficulty == AiDifficulty.Hard)
        {
            if(millList.Count > 0)
            {
                boardMove = millList[Random.Range(0,millList.Count)];
                Debug.Log($"Mill Found At {boardMove.BoardID}");
                return boardMove;
            }
        }

        if(aiDifficulty == AiDifficulty.Normal || aiDifficulty == AiDifficulty.Hard)
        {
            if(blockList.Count > 0)
            {
                boardMove = blockList[Random.Range(0,blockList.Count)];
                return boardMove;
            }
        }


        if(millBlockList.Count > 0)
        {
            boardMove = millBlockList[Random.Range(0,millBlockList.Count)];
            return boardMove;
        }




        return boardMove;
    }
    private BoardSO GetRemoveBoard()
    {
        Team opponent = _gameManager.GetOppositeTeam(_aiTeam);
        List<BoardSO> potentialMills = new(_gameManager.GetPotentialMills(opponent));

        // Prefer a potential-mill piece that isn't protected by a completed mill
        List<BoardSO> validTargets = potentialMills.FindAll(b => 
            !_gameManager.GetMillDetection().IsPieceInAMill(b));

        if(aiDifficulty == AiDifficulty.Normal || aiDifficulty == AiDifficulty.Hard)
        {
            if (validTargets.Count > 0)
                return validTargets[Random.Range(0, validTargets.Count)];
        }

        // Fall back: any opponent piece not in a completed mill
        List<BoardSO> anyUnprotected = _gameManager.GetBoard().FindAll(b =>
        {
            PieceSO piece = b.GetCurrentPiece();
            return piece != null 
                && piece.Team == opponent 
                && !_gameManager.GetMillDetection().IsPieceInAMill(b);
        });

        if (anyUnprotected.Count > 0)
            return anyUnprotected[Random.Range(0, anyUnprotected.Count)];

        // Last resort: all opponent pieces are in mills, pick any occupied space
        List<BoardSO> anyOpponentSpace = _gameManager.GetBoard().FindAll(b =>
        {
            PieceSO piece = b.GetCurrentPiece();
            return piece != null && piece.Team == opponent;
        });

        return anyOpponentSpace[Random.Range(0, anyOpponentSpace.Count)];
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

        if(finaleSpots.Count > 0)
        return finaleSpots[Random.Range(0,finaleSpots.Count)];
        else
        return null;
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

        BoardSO move = DecideNextPlace();


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


    private void Move()
    {
        if (_gameManager.GetCurrentTeam() == _aiTeam) return;

        _gameManager.SetCurrentTeam(_aiTeam);

        if (_gameManager.GetCurrentPhase() == GameManagerAI.GamePhase.Place)
        {
            Place();
        }
        else if (_gameManager.GetCurrentPhase() == GameManagerAI.GamePhase.Move)
        {
            AiMovePiece();
        }
        else if (_gameManager.GetCurrentPhase() == GameManagerAI.GamePhase.Fly && _gameManager.GetPiecesOnBoardForTeam(_aiTeam) == 3)
        {
            AiFlyPiece();
        }
        else
        {
            AiMovePiece();
        }

        _gameManager.SetCurrentTeam(_gameManager.GetOppositeTeam(_aiTeam));
    }

    private void AiMovePiece()
    {
        List<BoardSO> aiSpaces = GetAiOccupiedSpaces();
        List<BoardSO> potentialMills = _gameManager.GetPotentialMills(_aiTeam);
        List<BoardSO> blockMills = _gameManager.GetFinalMillBoard()[_gameManager.GetOppositeTeam(_aiTeam)];
        List<BoardSO> finalMills = _gameManager.GetFinalMillBoard()[_aiTeam];
        if(aiDifficulty == AiDifficulty.Hard)
        {
            foreach (BoardSO target in finalMills)
            {
                if (target.GetCurrentPiece() != null) continue;
                foreach (BoardSO source in aiSpaces)
                {
                    if (WouldBreakPotentialMill(source)) continue;
                    if (source.GetAdjacentBoardSpaces().Contains(target))
                    {
                        ExecuteMove(source, target);
                        return;
                    }
                }
            } 
        }

        List<BoardSO> shuffled = aiSpaces.OrderBy(_ => Random.value).ToList();
        if(aiDifficulty == AiDifficulty.Normal || aiDifficulty == AiDifficulty.Hard)
        {
            // Only block if the opponent has a piece adjacent to the target
            foreach (BoardSO target in blockMills)
            {
                if (!OpponentCanReachMove(target)) continue;

                foreach (BoardSO source in aiSpaces)
                {
                    if (source.GetAdjacentBoardSpaces().Contains(target))
                    {
                        ExecuteMove(source, target);
                        return;
                    }
                }
            }

            
            foreach (BoardSO source in shuffled)
            {
                if (WouldBreakPotentialMill(source)) continue;

                List<BoardSO> emptyAdjacent = source.GetAdjacentBoardSpaces()
                    .FindAll(b => b.GetCurrentPiece() == null);

                if (emptyAdjacent.Count > 0)
                {
                    ExecuteMove(source, emptyAdjacent[Random.Range(0, emptyAdjacent.Count)]);
                    return;
                }
            }
            
        }

        foreach (BoardSO source in shuffled)
        {
            List<BoardSO> emptyAdjacent = source.GetAdjacentBoardSpaces()
                .FindAll(b => b.GetCurrentPiece() == null);

            if (emptyAdjacent.Count > 0)
            {
                ExecuteMove(source, emptyAdjacent[Random.Range(0, emptyAdjacent.Count)]);
                return;
            }
        }

        Debug.Log("AI has no valid moves.");
    }

    private void AiFlyPiece()
    {
        List<BoardSO> aiSpaces = GetAiOccupiedSpaces();
        List<BoardSO> blockMills = _gameManager.GetFinalMillBoard()[_gameManager.GetOppositeTeam(_aiTeam)];
        List<BoardSO> finalMills = _gameManager.GetFinalMillBoard()[_aiTeam];
        List<BoardSO> emptySpaces = _gameManager.GetBoard()
            .FindAll(b => b.GetCurrentPiece() == null);
        
        if(aiDifficulty == AiDifficulty.Hard)
        {
            foreach (BoardSO target in finalMills)
            {
                if (target.GetCurrentPiece() != null) continue;
                foreach (BoardSO source in aiSpaces)
                {
                    if (WouldBreakPotentialMill(source)) continue;
                    ExecuteFly(source, target);
                    return;
                }
            }    
        }

        if(aiDifficulty == AiDifficulty.Normal || aiDifficulty == AiDifficulty.Hard)
        {
            // In fly phase the opponent can always reach any empty space — always an immediate threat
            foreach (BoardSO target in blockMills)
            {
                foreach (BoardSO source in aiSpaces)
                {
                    ExecuteFly(source, target);
                    return;
                }
            }
        }

        List<BoardSO> shuffled = aiSpaces.OrderBy(_ => Random.value).ToList();
        foreach (BoardSO source in shuffled)
        {
            ExecuteFly(source, emptySpaces[Random.Range(0, emptySpaces.Count)]);
            return;
        }

        Debug.Log("AI has no valid moves.");
    }

    private void ExecuteMove(BoardSO from, BoardSO to)
    {
        _gameManager.MovePiece(_boardLookup[from].gameObject);
        _gameManager.MovePiece(_boardLookup[to].gameObject);
    }

    private void ExecuteFly(BoardSO from, BoardSO to)
    {
        _gameManager.FlyPiece(_boardLookup[from].gameObject);
        _gameManager.FlyPiece(_boardLookup[to].gameObject);
    }

    private List<BoardSO> GetAiOccupiedSpaces()
    {
        List<BoardSO> result = new();
        foreach (BoardObject bo in _boardObjects)
        {
            PieceSO piece = bo.BoardSO.GetCurrentPiece();
            if (piece != null && piece.Team == _aiTeam)
                result.Add(bo.BoardSO);
        }
        return result;
    }

    private bool WouldBreakPotentialMill(BoardSO source)
    {
        List<HashSet<string>> mills = _gameManager.GetMillDetection().ActiveMills;

        foreach (HashSet<string> mill in mills)
        {
            // source is not part of this mill
            if (!mill.Contains(source.BoardID))
                continue;

            int aiPieces = 0;
            int emptySpaces = 0;

            foreach (string id in mill)
            {
                BoardSO board = _gameManager.GetBoard()
                    .Find(b => b.BoardID == id);

                // simulate removing source
                if (board == source)
                {
                    emptySpaces++;
                    continue;
                }

                PieceSO piece = board.GetCurrentPiece();

                if (piece == null)
                {
                    emptySpaces++;
                }
                else if (piece.Team == _aiTeam)
                {
                    aiPieces++;
                }
            }

            // if this was a valid potential mill before moving,
            // and removing the piece destroys it
            if (aiPieces == 1 && emptySpaces == 2)
            {
                return true;
            }
        }

        return false;
    }
    private bool OpponentCanReachMove(BoardSO target)
    {
        Team opponent = _gameManager.GetOppositeTeam(_aiTeam);
        return target.GetAdjacentBoardSpaces()
            .Exists(b => b.GetCurrentPiece() != null && b.GetCurrentPiece().Team == opponent);
    }
    public enum AiDifficulty
    {
        Easy,
        Normal,
        Hard
    }
}


