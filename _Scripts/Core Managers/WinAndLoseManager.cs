using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class WinAndLoseManager : Singleton_Abs<WinAndLoseManager>
{
    [SerializeField] LevelsSavedData _database;

    [SerializeField] GameObject _winAndLoseCanvas;
    [SerializeField] GameObject _losePanel;
    [SerializeField] GameObject _winPanel;
    [SerializeField] GameObject _menuCanvas;
    [SerializeField] GameObject _powerUpsCanvas;
    //[SerializeField] GameObject _restartPanel;

    [Header("Events & Time Lines")]
    [SerializeField] _AllTimeLines _allTimeLines;
    [SerializeField] _Events _events;

    bool _mapHasVoid;
    bool _mapHasOneWays;

    public bool _isGameEnded { get; private set; }
    public bool _isWin { get; private set; } = false;

    private void Start()
    {
        //_restartPanel.SetActive(true);
        _winAndLoseCanvas.SetActive(false);
        _mapHasVoid = GridManager._instance._mapHasVoid;
        _mapHasOneWays = GridManager._instance._mapHasOneWays;
    }
    #region Win And Lose Checking
    public void _CheckForWin()
    {
        _MapData levelData = LevelManager._instance._currentMapData;

        if (levelData._conditions.Length == 0)
        {
            Debug.LogError("there is no win condition in the level");
            return;
        }

        List<BlockController> blocks = GridManager._instance._GetAllBlocksControllers();

        for (int i = 0; i < levelData._conditions.Length; i++)
        {
            _WinConditions condition = levelData._conditions[i];

            if (condition._winCondition == _AllWinTypes.reach_number)
            {
                int target = (int)condition._winNumber;
                int matchCount = 0;

                for (int j = 0; j < blocks.Count; j++)
                {
                    if (blocks[j]._value == target)
                    {
                        matchCount++;
                        if (condition._count <= 1) break;
                    }
                }

                if (condition._count <= 1 && matchCount < 1) return;
                if (condition._count > 1 && matchCount < condition._count) return;
            }
            else if (condition._winCondition == _AllWinTypes.fill_place)
            {
                List<Vector3Int> winConditionCells = GridManager._instance._GetWinConditionCells();

                if (winConditionCells.Count == 0)
                {
                    Debug.LogWarning("No win condition tiles found in the level!");
                    continue; // Skip this condition
                }

                int validFills = 0;

                foreach (Vector3Int winCell in winConditionCells)
                {
                    BlockController block = GridManager._instance._GetBlockAt(winCell);
                    if (block != null)
                    {
                        // Check if we need a specific value or any block is acceptable
                        if (condition._specificValue)
                        {
                            if (block._value == (int)condition._fillValue || block._value == (int)condition._extraValue)
                            {
                                validFills++;
                            }
                        }
                        else
                        {
                            // Any block counts as a valid fill
                            validFills++;
                        }
                    }
                }

                // For fill_place, we always require ALL win condition cells to be filled
                // This is the dynamic count based on how many win condition tiles exist
                int requiredFills = winConditionCells.Count;

                if (validFills < requiredFills)
                {
                    return; // Win condition not met
                }
            }
        }
        _PerformWin();
    }
    public void _CheckForLose()
    {
        if (_mapHasVoid) return;

        List<Vector3Int> allFloors = GridManager._instance._GetAllFloorCells();
        List<Vector3Int> allBlocks = GridManager._instance._GetAllOccupiedCells();

        // If there are still empty spawnable cells, game is not over
        if (allBlocks.Count < allFloors.Count) return;

        if (_mapHasOneWays)
        {
            // Use advanced checking for one-way tiles
            if (_HasValidMovesInAnyDirection())
            {
                return; // Game can continue
            }
        }
        else
        {
            // Use simple adjacent checking (original logic)
            if (_HasValidMovesSimple())
            {
                return; // Game can continue
            }
        }

        _PendingLose_WaitForRevival();
    }
    /// <summary>
    /// Simple move checking for maps without one-way tiles (original logic)
    /// Much faster performance when one-way tiles aren't present
    /// </summary>
    private bool _HasValidMovesSimple()
    {
        List<Vector3Int> allBlocks = GridManager._instance._GetAllOccupiedCells();

        foreach (Vector3Int cell in allBlocks)
        {
            BlockController block = GridManager._instance._GetBlockAt(cell);
            if (block == null) continue;

            foreach (Vector3Int dir in _directions)
            {
                Vector3Int neighbor = cell + dir;
                BlockController other = GridManager._instance._GetBlockAt(neighbor);
                if (other != null && other._value == block._value && block._isBlockMergeable)
                {
                    return true; // Found a valid merge, exit immediately
                }
            }
        }
        return false;
    }
    /// <summary>
    /// Advanced move checking for maps with one-way tiles
    /// Includes movement validation for one-way tiles
    /// </summary>
    private bool _HasValidMovesInAnyDirection()
    {
        // Cache blocks list to avoid repeated calls
        List<Vector3Int> allBlocks = GridManager._instance._GetAllOccupiedCells();

        // Early exit if no blocks
        if (allBlocks.Count == 0) return false;

        // Check each of the four possible movement directions
        foreach (Vector3Int direction in _directions)
        {
            if (_HasValidMovesInDirection(direction, allBlocks))
            {
                return true; // Found a valid move, exit immediately
            }
        }
        return false;
    }
    /// <summary>
    /// Checks if there are any valid moves possible in a specific direction
    /// Takes into account one-way tile restrictions and block merging possibilities
    /// </summary>
    private bool _HasValidMovesInDirection(Vector3Int direction, List<Vector3Int> allBlocks)
    {
        foreach (Vector3Int blockCell in allBlocks)
        {
            // Quick check: Can this block even move FROM its current position?
            if (!GridManager._instance._CanMoveFromCell(blockCell, direction))
            {
                continue; // Skip this block entirely
            }

            BlockController block = GridManager._instance._GetBlockAt(blockCell);
            if (block == null) continue;

            // Check if this block can move in the given direction
            if (_CanBlockMoveInDirection(block, blockCell, direction))
            {
                return true; // Found a valid move, exit immediately
            }
        }

        return false;
    }
    /// <summary>
    /// Checks if a specific block can move in a given direction
    /// This includes checking for valid target positions and merge possibilities
    /// </summary>
    private bool _CanBlockMoveInDirection(BlockController block, Vector3Int fromCell, Vector3Int direction)
    {
        Vector3Int currentPos = fromCell;

        // Keep checking cells in the movement direction
        while (true)
        {
            Vector3Int nextPos = currentPos + direction;

            // Check if the next position is a valid move target
            if (!GridManager._instance._IsValidMove(currentPos, nextPos, direction))
            {
                break; // Can't move further in this direction
            }

            currentPos = nextPos;

            // Check if this position is empty or contains a mergeable block
            BlockController otherBlock = GridManager._instance._GetBlockAt(currentPos);

            if (otherBlock == null)
            {
                // Empty cell - this is a valid move, return immediately
                return true;
            }
            else if (otherBlock._value == block._value &&
                     block._isBlockMergeable &&
                     otherBlock._isBlockMergeable)
            {
                // Found a mergeable block - this is a valid move, return immediately
                return true;
            }
            else
            {
                // Found a non-mergeable block - can't move further
                break;
            }
        }

        // No valid move found for this block in this direction
        return false;
    }
    #endregion
    public void _PerformLose()
    {
        _isWin = false;

        _losePanel.SetActive(true);
        _events._gameOverEvent.Invoke();

        _PlayEndTimeLine(false);
        CoinManager._instance._AddCoinsToInventory();

        SaveManager._instance._DeleteData();
    }
    public void _PendingLose_WaitForRevival()
    {
        // we open revival panel, if the revival menu is closed the player loses
        _ReadyToEndLevel();
        ReviveManager._instance._CheckForPossibleRevive();
    }
    public void _RevivePlayer()
    {
        // most of logic is handled in ReviveManager

        _winAndLoseCanvas.SetActive(false);
        _menuCanvas.SetActive(true);
        _powerUpsCanvas.SetActive(true);

        _isGameEnded = false;
    }
    private void _PerformWin()
    {
        // tip: the level unlocking is handled in ChallengeManager to improve performance
        _isWin = true;

        _ReadyToEndLevel();
        ChallengeManager._instance._CompleteCurrentChallenges();
        _winPanel.SetActive(true);

        _events._winEvent.Invoke();
        _PlayEndTimeLine(true);
        CoinManager._instance._AddCoinsToInventory();

        SaveManager._instance._DeleteData();
    }
    private void _ReadyToEndLevel()
    {
        _isGameEnded = true;

        _winAndLoseCanvas.SetActive(true);
        _menuCanvas.SetActive(false);
        _powerUpsCanvas.SetActive(false);

        ChallengeManager._instance._B_PauseTimer();
        InputManager._instance.gameObject.SetActive(false);

        CoinManager._instance._CalculateTotalCoins(true);

        VibrateManager._instance._MediumVibrate();
    }
    private void _PlayEndTimeLine(bool iIsWin)
    {
        if (iIsWin)
        {
            int iStars = ChallengeManager._instance._GetChallengeCount();
            _allTimeLines._GetCurrentTimeline(iStars).Play();
        }
        else
        {
            _allTimeLines._GetCurrentTimeline(-1).Play();
        }
    }
    #region Types
    [System.Serializable]
    public class _Events
    {
        public UnityEvent _gameOverEvent;
        public UnityEvent _reviveEvent;
        public UnityEvent _winEvent;
    }
    [System.Serializable]
    public class _AllTimeLines
    {
        [SerializeField] PlayableDirector _loseTimeLine;
        [SerializeField] PlayableDirector _0StarTimeLine;
        [SerializeField] PlayableDirector _1StarTimeLine;
        [SerializeField] PlayableDirector _2StarTimeLine;
        [SerializeField] PlayableDirector _3StarTimeLine;
        [SerializeField] PlayableDirector _5StarTimeLine;

        public PlayableDirector _GetCurrentTimeline(int iStarCount)
        {
            switch (iStarCount)
            {
                case -1: return _loseTimeLine;
                case 0: return _0StarTimeLine;
                case 1: return _1StarTimeLine;
                case 2: return _2StarTimeLine;
                case 3: return _3StarTimeLine;
                case 4: return _5StarTimeLine;
            }
            return null;
        }
    }
    #endregion
    #region Extra
    public static readonly Vector3Int[] _directions = new Vector3Int[]
{
        Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
};
    #endregion
}
