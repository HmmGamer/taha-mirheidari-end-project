using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Empty,
    Floor,
    Obstacle,
    One_Way,
    Void
}

[CreateAssetMenu(fileName = "TileData", menuName = "Taha/Tile Data")]
public class TileData : ScriptableObject
{
    public bool _canSpawnBlocks;
    public bool _isWinCondition;
    public TileType _type;

    [ConditionalEnum(nameof(_type), (int)TileType.One_Way)]
    public Directions _wayDirection;
}
