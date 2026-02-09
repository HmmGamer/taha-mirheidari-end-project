using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : Singleton_Abs<SaveManager>
{
    private List<InGridBlockSaveData> _tempBlockPositions;

    #region Exit Detection
    private void OnApplicationPause(bool iIsPause)
    {
        if (!iIsPause) return;

        // we dont save the data for an empty scene
        if (GridManager._instance._blockPositions.Count != 0)
            _SaveDataToDisk();
        else
            _DeleteData();
    }
    private void OnApplicationQuit()
    {
        // we dont save the data for an empty scene
        if (GridManager._instance._blockPositions.Count != 0)
            _SaveDataToDisk();
        else
            _DeleteData();
    }
    private void OnDisable()
    {
        // we dont save the data for an empty scene
        if (GridManager._instance._blockPositions.Count != 0)
            _SaveDataToDisk();
        else
            _DeleteData();
    }
    #endregion

    #region Save and Load
    /// <summary>
    /// Lightweight and cheap temporary save
    /// Called after each block movement to avoid block data loss
    /// </summary>
    public void _TempSaveData()
    {
        _tempBlockPositions = new List<InGridBlockSaveData>();

        // Only save block positions - this is the data that changes during movement
        foreach (var item in GridManager._instance._blockPositions)
        {
            InGridBlockSaveData blockData = item.Value._GetBlockInfo();
            _tempBlockPositions.Add(blockData);
        }
    }
    public void _SaveDataToDisk()
    {
        if (WinAndLoseManager._instance._isGameEnded)
        {
            PlayerPrefs.SetInt(A.DataKey.lastUnfinishedLevel, A.DataKey.False);
            return;
        }
        LastGameSavedData saveData = new LastGameSavedData();

        if (!GridManager._instance._isMoving)
        {
            // Blocks are not moving so safe we read the current GridManager's data
            saveData._blocks = new List<InGridBlockSaveData>();
            foreach (var item in GridManager._instance._blockPositions)
            {
                InGridBlockSaveData blockData = item.Value._GetBlockInfo();
                saveData._blocks.Add(blockData);
            }
        }
        else
        {
            // Blocks are moving, we can't get them so we use cached data
            if (_tempBlockPositions != null)
            {
                saveData._blocks = new List<InGridBlockSaveData>(_tempBlockPositions);
            }
            else
            {
                saveData._blocks = new List<InGridBlockSaveData>();
                foreach (var item in GridManager._instance._blockPositions)
                {
                    InGridBlockSaveData blockData = item.Value._GetBlockInfo();
                    saveData._blocks.Add(blockData);
                }
            }
        }

        saveData._isSpawnUpdated = SpawnManager._instance._isSpawnUpdated;
        saveData._powerUpsData = PowerUpManager._instance._GetUsedPowerUps();
        saveData._timeChallenge = ChallengeManager._instance._currentTime;
        saveData._currentReviveCount = ReviveManager._instance._currentReviveCount;
        saveData._currentScore = ScoreManager._instance._currentScore;

        ES3.Save("gridData", saveData);
        PlayerPrefs.SetInt(A.DataKey.lastUnfinishedLevel, LevelManager._instance._currentLevelIndex);
    }
    public void _LoadData()
    {
        #region Editor Only
#if UNITY_EDITOR
        // reminder: if the level manager's _useSavedManager is false this wont be called!
        if (!LevelManager._instance._isUsingSavedData)
            return;
#endif
        #endregion

        if (!ES3.KeyExists("gridData")
            || PlayerPrefs.GetInt(A.DataKey.lastUnfinishedLevel) == A.DataKey.False)
            return;

        LastGameSavedData saveData = ES3.Load<LastGameSavedData>("gridData");

        // Restore blocks
        foreach (InGridBlockSaveData blockData in saveData._blocks)
        {
            SpawnManager._instance._SpawnBlockAtPosition(blockData.position, blockData.value, blockData.blockType);
        }

        // Restore game state
        SpawnManager._instance._isSpawnUpdated = saveData._isSpawnUpdated;
        PowerUpManager._instance._SetUsedPowerUps(saveData._powerUpsData);
        ChallengeManager._instance._currentTime = saveData._timeChallenge;
        ReviveManager._instance._currentReviveCount = saveData._currentReviveCount;
        ScoreManager._instance._currentScore = saveData._currentScore;
    }
    /// <summary>
    /// int this class, we always save the data and wont ever actually delete it
    /// so when we say delete the data we mean to not load it later
    /// </summary>
    public void _DeleteData()
    {
        PlayerPrefs.SetInt(A.DataKey.lastUnfinishedLevel, A.DataKey.False);
    }
    #endregion
}
#region Save/Load Types
[System.Serializable]
public class LastGameSavedData
{
    public List<InGridBlockSaveData> _blocks;
    public UsedPowerUpsSavedData _powerUpsData;
    public int _timeChallenge;
    public int _currentReviveCount;
    public bool _isSpawnUpdated;
    public int _currentScore;
}

[System.Serializable]
public class InGridBlockSaveData
{
    public Vector3Int position;
    public int blockType;
    public int value;
}

[System.Serializable]
public class UsedPowerUpsSavedData
{
    public int _freezeUsedCount;
    public int _under8UsedCount;
    public int _removeSpecificUsedCount;
}
#endregion