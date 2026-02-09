using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WinConditionTileColorManager : Singleton_Abs<WinConditionTileColorManager>
{
    private Dictionary<Vector3Int, Color> _defaultTileColors = new Dictionary<Vector3Int, Color>();
    private bool _isActive = false;
    private _WinConditions _fillPlaceCondition;

    private void OnEnable()
    {
        GridManager._onBoardChanged.AddListener(_UpdateWinConditionTileColors);
    }

    private void OnDisable()
    {
        GridManager._onBoardChanged.RemoveListener(_UpdateWinConditionTileColors);
    }
    public void _InitializeTileColors()
    {
        // Clear previous data
        _defaultTileColors.Clear();
        _isActive = false;
        _fillPlaceCondition = null;

        // Check if current level has fill_place win condition
        _MapData levelData = LevelManager._instance._currentMapData;
        if (levelData == null || levelData._conditions == null || levelData._conditions.Length == 0)
        {
            return;
        }

        // Find fill_place win condition
        foreach (_WinConditions condition in levelData._conditions)
        {
            if (condition._winCondition == _AllWinTypes.fill_place)
            {
                _fillPlaceCondition = condition;
                _isActive = true;
                break;
            }
        }

        // If no fill_place condition found, return
        if (!_isActive)
        {
            return;
        }

        // Cache default colors of all win condition tiles
        Tilemap tilemap = GridManager._instance._tilemap;
        if (tilemap == null)
        {
            return;
        }

        List<Vector3Int> winConditionCells = GridManager._instance._GetWinConditionCells();
        if (winConditionCells.Count == 0)
        {
            _isActive = false;
            return;
        }
        
        foreach (Vector3Int cell in winConditionCells)
        {
            // Get and store the original tile color
            Color originalColor = tilemap.GetColor(cell);
            _defaultTileColors[cell] = originalColor;
        }

        // Set initial colors based on current block states
        _UpdateWinConditionTileColors();
    }

    /// <summary>
    /// Updates tile colors based on current block values at the end of each move turn
    /// </summary>
    private void _UpdateWinConditionTileColors()
    {
        if (!_isActive || _fillPlaceCondition == null)
        {
            return;
        }

        Tilemap tilemap = GridManager._instance._tilemap;
        if (tilemap == null)
        {
            return;
        }

        List<Vector3Int> winConditionCells = GridManager._instance._GetWinConditionCells();

        foreach (Vector3Int winCell in winConditionCells)
        {
            BlockController block = GridManager._instance._GetBlockAt(winCell);
            
            if (block == null)
            {
                // No block present - restore to default color
                if (_defaultTileColors.TryGetValue(winCell, out Color defaultColor))
                {
                    tilemap.SetColor(winCell, defaultColor);
                }
            }
            else
            {
                // Block present - check if value is correct
                if (_IsBlockValueCorrect(block))
                {
                    // Correct value - set to green
                    tilemap.SetColor(winCell, Color.green);
                }
                else
                {
                    // Incorrect value - set to red
                    tilemap.SetColor(winCell, Color.red);
                }
            }
        }
    }

    /// <summary>
    /// Checks if block value matches the win condition requirements
    /// </summary>
    private bool _IsBlockValueCorrect(BlockController block)
    {
        if (block == null || _fillPlaceCondition == null)
        {
            return false;
        }

        // If specific value is required, check against fillValue and extraValue
        if (_fillPlaceCondition._specificValue)
        {
            int fillValue = (int)_fillPlaceCondition._fillValue;
            int extraValue = (int)_fillPlaceCondition._extraValue;
            
            return block._value == fillValue || block._value == extraValue;
        }
        else
        {
            // Any block is acceptable when specificValue is false
            return true;
        }
    }

    /// <summary>
    /// Restores all win condition tiles to their original colors
    /// Useful when level ends or changes
    /// </summary>
    public void _RestoreDefaultColors()
    {
        if (!_isActive)
        {
            return;
        }

        Tilemap tilemap = GridManager._instance._tilemap;
        if (tilemap == null)
        {
            return;
        }

        foreach (KeyValuePair<Vector3Int, Color> kvp in _defaultTileColors)
        {
            tilemap.SetColor(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Cleans up when level changes
    /// </summary>
    public void _Cleanup()
    {
        _RestoreDefaultColors();
        _defaultTileColors.Clear();
        _isActive = false;
        _fillPlaceCondition = null;
    }
}
