using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

/// <summary>
/// TODO: Separate the grid and movement system for a cleaner script
/// </summary>
public class GridManager : Singleton_Abs<GridManager>
{
    public static UnityEvent _onBoardChanged = new UnityEvent();

    const float _EXTRA_DELAY_BEFORE_NEXT_MOVE = 0.1f;

    [HideInInspector] public Tilemap _tilemap;

    [HideInInspector] public Dictionary<Vector3Int, BlockController> _blockPositions = new Dictionary<Vector3Int, BlockController>();
    private List<BlockController> _allBlocks = new List<BlockController>(); // Cached

    private List<Vector3Int> _cachedSpawnableCells;
    private List<Vector3Int> _cachedFloorCells;
    private List<Vector3Int> _cachedWinConditionCells;
    private Dictionary<Vector3Int, TileType> _cachedTileTypes;
    private HashSet<Vector3Int> _cachedVoidCells;

    public bool _hasBoardChangedThisTurn { get; set; }
    public bool _mapHasVoid { get; private set; }
    public bool _mapHasOneWays { get; private set; }
    public bool _isMoving { get; private set; }

    #region Starter
    private void Start()
    {
        _mapHasVoid = _DoesMapHasVoid();
        _mapHasOneWays = _DoesMapHaveOneWay();

        _CacheAllTileLists();
        SaveManager._instance._LoadData();
    }
    private void OnEnable()
    {
        InputManager._onNewInputDirection.AddListener(_HandleMoveInput);
    }
    private void OnDisable()
    {
        InputManager._onNewInputDirection.RemoveListener(_HandleMoveInput);
    }
    #endregion
    #region Cacheing
    /// <summary>
    /// Cache all tile lists once at startup since tilemap doesn't change during gameplay
    /// </summary>
    private void _CacheAllTileLists()
    {
        _cachedSpawnableCells = new List<Vector3Int>();
        _cachedFloorCells = new List<Vector3Int>();
        _cachedWinConditionCells = new List<Vector3Int>();
        _cachedTileTypes = new Dictionary<Vector3Int, TileType>();
        _cachedVoidCells = new HashSet<Vector3Int>();

        BoundsInt bounds = _tilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = _tilemap.GetTile(pos);
            if (tile is GameTile gameTile && gameTile._data != null)
            {
                TileType type = gameTile._data._type;
                _cachedTileTypes[pos] = type;

                if ((type == TileType.Floor || type == TileType.One_Way) && gameTile._data._canSpawnBlocks)
                    _cachedSpawnableCells.Add(pos);

                if (type == TileType.Floor || type == TileType.One_Way)
                    _cachedFloorCells.Add(pos);

                if (gameTile._data._isWinCondition)
                    _cachedWinConditionCells.Add(pos);

                if (type == TileType.Void)
                    _cachedVoidCells.Add(pos);
            }
            else
            {
                _cachedTileTypes[pos] = TileType.Empty;
            }
        }
    }
    #endregion
    #region Input Handling
    /// <summary>
    /// Handles player input for block movement
    /// </summary>
    private void _HandleMoveInput(Vector3Int iDirection)
    {
        if (_isMoving) return;
        StartCoroutine(_MoveAllBlocks(iDirection));
    }

    #endregion
    #region Block Movement
    /// <summary>
    /// Main coroutine that handles moving all blocks in the specified direction
    /// Includes spawn management and win/lose condition checking
    /// </summary>
    private IEnumerator _MoveAllBlocks(Vector3Int iDirection)
    {
        _isMoving = true;
        _hasBoardChangedThisTurn = false;

        List<BlockController> blocks = _GetBlocksInMoveOrder(iDirection);

        for (int i = 0; i < blocks.Count; i++)
            blocks[i]._ResetMergeFlag();

        // we us hash instead of list to improve internal performance
        HashSet<BlockController> movingBlocks = new HashSet<BlockController>();
        for (int i = 0; i < blocks.Count; i++)
        {
            StartCoroutine(blocks[i]._TryMove(iDirection));
            movingBlocks.Add(blocks[i]);
        }

        // Check and remove blocks as they stop moving
        while (movingBlocks.Count > 0)
        {
            movingBlocks.RemoveWhere(b => !b._isMoving);
            yield return new WaitForEndOfFrame();
        }

        if (!_hasBoardChangedThisTurn && blocks.Count > 0)
        {
            if (!SpawnManager._instance._CanSpawn())
            {
                _isMoving = false;
                yield break;
            }
        }
        _onBoardChanged?.Invoke();

        AudioManager._instance._PerformMergeAudio();
        AudioManager._instance._PlayAudio(_AudioType.SFX1, SavedSounds.Slide_Tile, true);

        SpawnManager._instance._SpawnBlock();
        ChallengeManager._instance._TryDoubleSpawn();

        yield return new WaitForSeconds(_EXTRA_DELAY_BEFORE_NEXT_MOVE);
        _isMoving = false;

        SaveManager._instance._TempSaveData();

        WinAndLoseManager._instance._CheckForLose();
        WinAndLoseManager._instance._CheckForWin();
    }

    /// <summary>
    /// Orders blocks based on movement direction to prevent collision during movement
    /// </summary>
    private List<BlockController> _GetBlocksInMoveOrder(Vector3Int iDirection)
    {
        // We yse the cached list instead of creating from dictionary values
        List<BlockController> blocks = new List<BlockController>(_allBlocks);

        if (iDirection.x == 1) blocks = blocks.OrderByDescending(b => b._GetCellPosition().x).ToList();
        else if (iDirection.x == -1) blocks = blocks.OrderBy(b => b._GetCellPosition().x).ToList();
        else if (iDirection.y == 1) blocks = blocks.OrderByDescending(b => b._GetCellPosition().y).ToList();
        else if (iDirection.y == -1) blocks = blocks.OrderBy(b => b._GetCellPosition().y).ToList();
        return blocks;
    }
    #endregion
    #region Block Management
    /// <summary>
    /// Methods for registering, unregistering, and moving blocks in the grid system
    /// </summary>
    public List<BlockController> _GetAllBlocksControllers()
    {
        return _allBlocks;
    }
    public void _RegisterBlock(Vector3Int iCell, BlockController iBlock)
    {
        if (!_blockPositions.ContainsKey(iCell))
        {
            _blockPositions.Add(iCell, iBlock);
            _allBlocks.Add(iBlock);
        }
    }
    public void _UnregisterBlock(Vector3Int iCell)
    {
        if (_blockPositions.TryGetValue(iCell, out BlockController block))
        {
            _blockPositions.Remove(iCell);
            _allBlocks.Remove(block);
        }
    }
    public void _MoveBlock(Vector3Int iFrom, Vector3Int iTo)
    {
        if (_blockPositions.TryGetValue(iFrom, out BlockController block))
        {
            _blockPositions.Remove(iFrom);
            _blockPositions[iTo] = block;
            // _allBlocks doesn't need updating since it's the same block reference
        }
    }
    public bool _IsCellOccupied(Vector3Int iCell)
    {
        return _blockPositions.ContainsKey(iCell);
    }
    public BlockController _GetBlockAt(Vector3Int iCell)
    {
        _blockPositions.TryGetValue(iCell, out BlockController block);
        return block; // Returns null if not found
    }
    #endregion
    #region Tile System
    /// <summary>
    /// Methods for querying tile properties and types
    /// </summary>
    public TileType _GetTileType(Vector3Int iCell)
    {
        if (_cachedTileTypes.TryGetValue(iCell, out TileType type))
            return type;
        return TileType.Empty;
    }
    public bool _IsCellWalkable(Vector3Int iCell)
    {
        TileType tileType = _GetTileType(iCell);
        return tileType == TileType.Floor || tileType == TileType.One_Way || tileType == TileType.Void;
    }
    #endregion
    #region Movement Validation
    /// <summary>
    /// Methods for validating block movement including one-way tile restrictions
    /// </summary>
    public bool _IsValidMove(Vector3Int fromCell, Vector3Int toCell, Vector3Int direction)
    {
        // First check if we can move FROM the current cell
        if (!_CanMoveFromCell(fromCell, direction))
        {
            return false;
        }

        // Then check if the target cell is walkable and we can move TO it
        if (!_IsCellWalkable(toCell))
        {
            return false;
        }

        // Finally check if we can move TO the target cell in this direction
        return _CanMoveToCell(toCell, direction);
    }
    public bool _CanMoveFromCell(Vector3Int fromCell, Vector3Int direction)
    {
        TileBase tile = _tilemap.GetTile(fromCell);
        if (tile is GameTile gameTile && gameTile._data != null)
        {
            if (gameTile._data._type == TileType.One_Way)
            {
                // Check if the movement direction matches the allowed direction
                Directions moveDirection = _GetDirectionFromVector(direction);
                return gameTile._data._wayDirection == moveDirection;
            }
        }

        // For non-one-way tiles, movement is always allowed
        return true;
    }
    public bool _CanMoveToCell(Vector3Int toCell, Vector3Int direction)
    {
        TileBase tile = _tilemap.GetTile(toCell);
        if (tile is GameTile gameTile && gameTile._data != null)
        {
            if (gameTile._data._type == TileType.One_Way)
            {
                // We can enter a one-way tile if we're moving in the same direction it allows
                Directions moveDirection = _GetDirectionFromVector(direction);
                return gameTile._data._wayDirection == moveDirection;
            }
        }

        // For non-one-way tiles, can always move to them if they're walkable
        return _IsCellWalkable(toCell);
    }
    private Directions _GetDirectionFromVector(Vector3Int direction)
    {
        if (direction == Vector3Int.up) return Directions.Up;
        if (direction == Vector3Int.down) return Directions.Down;
        if (direction == Vector3Int.left) return Directions.Left;
        if (direction == Vector3Int.right) return Directions.Right;

        Debug.LogError("Invalid direction vector: " + direction);
        return Directions.Up; // Default fallback
    }
    #endregion
    #region Cell Queries
    /// <summary>
    /// Methods for getting various types of cells from the tilemap
    /// </summary>
    public bool _DoesMapHasVoid()
    {
        BoundsInt bounds = _tilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = _tilemap.GetTile(pos);
            if (tile is GameTile gameTile && gameTile._data != null)
            {
                if (gameTile._data._type == TileType.Void)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public bool _DoesMapHaveOneWay()
    {
        BoundsInt bounds = _tilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = _tilemap.GetTile(pos);
            if (tile is GameTile gameTile && gameTile._data != null)
            {
                if (gameTile._data._type == TileType.One_Way)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public bool _IsVoidCell(Vector3Int iCell)
    {
        return _cachedVoidCells.Contains(iCell);
    }

    public List<Vector3Int> _GetAllSpawnableCells()
    {
        List<Vector3Int> availableCells = new List<Vector3Int>();

        // Filter cached spawnable cells for unoccupied ones
        foreach (Vector3Int cell in _cachedSpawnableCells)
        {
            if (!_IsCellOccupied(cell))
            {
                availableCells.Add(cell);
            }
        }

        return availableCells;
    }
    public List<Vector3Int> _GetAllFloorCells()
    {
        return new List<Vector3Int>(_cachedFloorCells);
    }
    public List<Vector3Int> _GetAllOccupiedCells()
    {
        return new List<Vector3Int>(_blockPositions.Keys);
    }
    public List<Vector3Int> _GetWinConditionCells()
    {
        return new List<Vector3Int>(_cachedWinConditionCells); // Return copy to prevent external modification
    }
    public List<Vector3Int> _GetAllVoidCells()
    {
        return new List<Vector3Int>(_cachedVoidCells);
    }
    #endregion
    #region Coordinate Conversion
    public Vector3 _GetWorldPosition(Vector3Int iCell)
    {
        return _tilemap.GetCellCenterWorld(iCell);
    }
    public Vector3Int _GetCellPosition(Vector3 iWorldPos)
    {
        return _tilemap.WorldToCell(iWorldPos);
    }
    #endregion
}