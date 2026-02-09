using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO: Improve readability and clean the code
/// </summary>
[CreateAssetMenu(menuName = "Taha/LevelsSavedData")]
public class LevelsSavedData : ScriptableObject
{
    public _FullLevelData[] _allLevelsData;
    public int _currentLevelIndex;
    [HideInInspector] public _FullLevelData _currentLevel;

    private void Awake()
    {
        _LoadData();
    }
    public void _UpdateData(_FullLevelData iData, bool iUnlockNextLevel = false)
    {
        if (iUnlockNextLevel)
        {
            if (_allLevelsData.Length > iData._level + 1)
                _allLevelsData[iData._level + 1]._isLevelUnlocked = true;
            _allLevelsData[iData._level]._isLevelFinished = true;
        }

        for (int i = 1; i < _allLevelsData.Length; i++)
        {
            if (iData._level == _allLevelsData[i]._level)
            {
                _currentLevelIndex = i;
                _currentLevel = iData;
                _allLevelsData[i] = iData;
                _SaveData();
                return;
            }
        }

        Debug.LogError("The data was not found in the level database");
    }
    public void _UpdateChallenges(List<_AllChallengeTypes> iChallenges, int iCurrentLevel)
    {
        _currentLevelIndex = iCurrentLevel;
        _currentLevel = _allLevelsData[_currentLevelIndex];

        // Reset all challenges to inactive first
        _allLevelsData[_currentLevelIndex]._stone._isActiveInLevel = false;
        _allLevelsData[_currentLevelIndex]._time._isActiveInLevel = false;
        _allLevelsData[_currentLevelIndex]._double._isActiveInLevel = false;
        _allLevelsData[_currentLevelIndex]._last._isActiveInLevel = false;

        // Activate challenges based on the list
        foreach (_AllChallengeTypes challengeType in iChallenges)
        {
            switch (challengeType)
            {
                case _AllChallengeTypes.Limited_Moves:
                    _allLevelsData[_currentLevelIndex]._stone._isActiveInLevel = true;
                    break;
                case _AllChallengeTypes.Limited_Time:
                    _allLevelsData[_currentLevelIndex]._time._isActiveInLevel = true;
                    break;
                case _AllChallengeTypes.Double_Spawn:
                    _allLevelsData[_currentLevelIndex]._double._isActiveInLevel = true;
                    break;
                case _AllChallengeTypes.Last_Challenge:
                    _allLevelsData[_currentLevelIndex]._last._isActiveInLevel = true;
                    break;
            }
        }
        _SaveData();
    }
    public bool _IsLevelFinished(int iLevel)
    {
        return _allLevelsData[iLevel]._isLevelFinished;
    }

    #region Save/Load
    [CreateSOButton("Save Data")]
    public void _SaveData()
    {
        #region EditorIndexUpdate
#if UNITY_EDITOR
        _UpdateLevelValues();
#endif
        #endregion

        ES3.Save(A.DataKey.savedData, _allLevelsData);
    }
    public void _LoadData()
    {
        if (ES3.KeyExists(A.DataKey.savedData))
        {
            _allLevelsData = ES3.Load<_FullLevelData[]>(A.DataKey.savedData);
        }
    }

    #endregion
    #region Editor Only
#if UNITY_EDITOR
    [CreateSOButton("Reset All Saved Data")]
    public void _ResetAllSavedData()
    {
        for (int i = 0; i < _allLevelsData.Length; i++)
        {
            _allLevelsData[i]._isLevelUnlocked = false;
            _allLevelsData[i]._isLevelFinished = false;
            _allLevelsData[i]._time._isFinished = false;
            _allLevelsData[i]._stone._isFinished = false;
            _allLevelsData[i]._double._isFinished = false;
            _allLevelsData[i]._last._isFinished = false;
        }
        _allLevelsData[1]._isLevelUnlocked = true;
        _SaveData();
    }
    public void _UpdateLevelValues()
    {
        for (int i = 1; i < _allLevelsData.Length; i++)
            _allLevelsData[i]._level = i;
    }
#endif
    #endregion
}
#region Main Data Types
[Serializable]
public class _FullLevelData
{
    [ReadOnly] public int _level;

    [Header("General Data")]
    public bool _isLevelUnlocked;
    public bool _isLevelFinished;
    public int _highestScore;

    [Header("Challenges Data")]
    public _StoneChallenge _stone;
    public _LimitedTimeChallenge _time;
    public _DoubleSpawnChallenge _double;
    public _lastChallenge _last;

    public int _GetStarsCount()
    {
        int _count = 0;

        if (_stone._isFinished)
            _count++;
        if (_time._isFinished)
            _count++;
        if (_double._isFinished)
            _count++;
        if (_last._isFinished)
            _count += 2;

        return _count;
    }
}
[Serializable]
public enum _AllChallengeTypes
{
    none,
    Limited_Moves,
    Limited_Time,
    Double_Spawn,
    Last_Challenge
}
#endregion
#region Challenges 
[Serializable]
public class _StoneChallenge
{
    [Tooltip("_stonesCount")]
    public int _stonesCount = 1;
    public bool _isFinished = false;
    public bool _isActiveInLevel = false;
}

[Serializable]
public class _LimitedTimeChallenge
{
    [Tooltip("_maxTime")]
    public int _maxTime;
    public bool _isFinished = false;
    public bool _isActiveInLevel = false;
}

[Serializable]
public class _DoubleSpawnChallenge
{
    public bool _isFinished = false;
    public bool _isActiveInLevel = false;
}

[Serializable]
public class _lastChallenge
{
    public bool _isFinished = false;
    public bool _isActiveInLevel = false;
}
#endregion