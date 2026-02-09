using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// make sure the execution order is lesser than grid manager
/// </summary>
public class LavaTileManager : MonoBehaviour
{
    [SerializeField] private GameObject _lavaShaderPrefab;

    private void Start()
    {
        _SpawnLavaShaders();
    }

    private void _SpawnLavaShaders()
    {
        List<Vector3Int> voidPositions = GridManager._instance._GetAllVoidCells();

        foreach (Vector3Int cellPos in voidPositions)
        {
            Vector3 worldPos = GridManager._instance._GetWorldPosition(cellPos);
            GameObject voidObj = Instantiate(_lavaShaderPrefab, worldPos, Quaternion.identity, transform);
        }
    }
}
