using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : Singleton_Abs<SpawnManager>
{
    [SerializeField] private GameObject _blockPrefab;
    [SerializeField] SpawnRatioData _simpleSpawnData;
    [SerializeField] SpawnRatioData _upgradedSpawnData;
    [SerializeField] Transform _spawnParent;

    [HideInInspector] public bool _isSpawnUpdated;
    int _stoneInSpawnList;

    public void _AddStoneToSpawnList()
    {
        _stoneInSpawnList++;
    }
    public void _SpawnBlock(bool iIsMergeable = true)
    {
        List<Vector3Int> spawnable = GridManager._instance._GetAllSpawnableCells();

        if (spawnable.Count == 0)
            return;

        Vector3Int randomCell = spawnable[Random.Range(0, spawnable.Count)];
        Vector3 worldPos = GridManager._instance._GetWorldPosition(randomCell);
        GameObject spawnedObj;

        spawnedObj = PoolManager._instance._Instantiate(_PoolType.block, _blockPrefab, worldPos, Quaternion.identity, _spawnParent);

        BlockController blockController = spawnedObj.GetComponent<BlockController>();

        if (_stoneInSpawnList > 0)
        {
            _stoneInSpawnList--;
            iIsMergeable = false;
        }

        if (!iIsMergeable)
            blockController._SetBlockToStone();
        else
        {
            if (!_isSpawnUpdated)
                blockController._ChangeValue(_simpleSpawnData._GetRandomValue());
            else
                blockController._ChangeValue(_upgradedSpawnData._GetRandomValue());
        }

        //spawnedObj.name += Random.Range(0, 10);
    }
    public void _SpawnBlockAtPosition(Vector3Int position, int value, int blockType)
    {
        Vector3 worldPos = GridManager._instance._GetWorldPosition(position);
        GameObject spawnedObj;
        spawnedObj = PoolManager._instance._Instantiate(_PoolType.block, _blockPrefab, worldPos, Quaternion.identity, _spawnParent);
        BlockController blockController = spawnedObj.GetComponent<BlockController>();

        if (blockType == 1) // Stone block
        {
            blockController._SetBlockToStone();
        }
        else // Normal block
        {
            blockController._ChangeValue(value);
        }
    }
    public void _UpdateSpawnedBlocks()
    {
        if (_isSpawnUpdated) return;

        _isSpawnUpdated = true;

        foreach (var item in GridManager._instance._blockPositions)
        {
            if (item.Value._value == 2)
            {
                item.Value._PreformMerge();
            }
        }
    }
    public bool _CanSpawn()
    {
        return GridManager._instance._GetAllSpawnableCells().Count > 0;
    }
}
