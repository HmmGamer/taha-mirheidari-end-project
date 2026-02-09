using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeManager : Singleton_Abs<ChallengeManager>
{
    const float RED_THERSHOLD = 0.1f; // between 0 and 1
    const float YELOW_THERSHOLD = 0.4f; // between 0 and 1
    const float TIME_REVIVE_PERCENT = 0.4f; // between 0 and 1
    const float DOUBLE_SPAWN_TIME_PENALTY = 0.35f; // between 0 and 1 (e.g: -35% time)
    const int _TICK_TOCK_THERSHOLD = 11; // +1 for < check

    [Header("General Attachments")]
    [SerializeField] LevelsSavedData _database;

    [Header("Time")]
    [SerializeField] Text _remainingTimeText;
    [SerializeField] GameObject _timePanel;
    [ReadOnly, SerializeField] bool _isTimeChallengeActive;
    int _maxTime = 0;
    public int _currentTime; // public so the save manager can access it

    [Header("Stone")]
    [ReadOnly, SerializeField] bool _isStoneChallengeActive;

    [Header("Double")]
    [ReadOnly, SerializeField] bool _isDoubleChallengeActive;

    private _FullLevelData _data;
    private bool _isGamePaused;
    
    #region Basic Functions
    private void Start()
    {
        _data = _database._allLevelsData[_database._currentLevelIndex];

        _UpdateCurrentChallenges();

        _SetupTimeChallenge();
        _TryLoadStoneChallenge();
    }
    private void OnEnable()
    {
        GridManager._onBoardChanged.AddListener(_UnPauseTimerOnInput);
    }
    private void OnDisable()
    {
        GridManager._onBoardChanged.RemoveListener(_UnPauseTimerOnInput);
    }
    private void _UpdateCurrentChallenges()
    {
        _isTimeChallengeActive = _data._time._isActiveInLevel;
        _isStoneChallengeActive = _data._stone._isActiveInLevel;
        _isDoubleChallengeActive = _data._double._isActiveInLevel;
    }
    public void _CompleteCurrentChallenges()
    {
        StopAllCoroutines(); // stop's the timer coroutine

        _data._time._isFinished = _data._time._isFinished || _data._time._isActiveInLevel;
        _data._stone._isFinished = _data._stone._isFinished || _data._stone._isActiveInLevel;
        _data._double._isFinished = _data._double._isFinished || _data._double._isActiveInLevel;
        _data._last._isFinished = _data._last._isFinished || _data._last._isActiveInLevel;

        _database._UpdateData(_data, true);
    }
    #endregion

    #region Time
    /// <summary>
    /// reminder: we pause the timer, so the timer starts after player's first input
    /// </summary>
    private void _SetupTimeChallenge()
    {
        if (!_isTimeChallengeActive)
        {
            _timePanel.SetActive(false);
            return;
        }

        _maxTime = _data._time._maxTime;
        if (_isDoubleChallengeActive)
            _maxTime = (int)(_maxTime * (1 - DOUBLE_SPAWN_TIME_PENALTY));

        _timePanel.SetActive(true);
        _UpdateTimeTextUi();

        _B_PauseTimer(); // we pause the timer, so it starts after player's first input
        StartCoroutine(_UpdateCurrentTime());
    }
    public void _UnPauseTimerOnInput()
    {
        if (_isGamePaused)
            _B_UnPauseTimer();
    }
    public void _B_PauseTimer()
    {
        _isGamePaused = true;

        if (ServerDataCollector._instance != null)
            ServerDataCollector._instance._PauseTime();
    }
    public void _B_UnPauseTimer()
    {
        _isGamePaused = false;

        if (ServerDataCollector._instance != null)
            ServerDataCollector._instance._UnPauseTime();
    }
    private void _UpdateTimeTextUi()
    {
        int remainingTime = _maxTime - _currentTime;

        if (remainingTime >= 0)
        {
            _remainingTimeText.text = TimeTools.TotalStringTime(remainingTime);
        }

        if (remainingTime < RED_THERSHOLD * _maxTime)
            _remainingTimeText.color = Color.red;
        else if (remainingTime < YELOW_THERSHOLD * _maxTime)
            _remainingTimeText.color = Color.red;
        else
            _remainingTimeText.color = Color.black;
    }

    /// <summary>
    /// called when we revive the player in revive manager
    /// </summary>
    public void _ReviveTimer()
    {
        _currentTime -= (int)(_maxTime * TIME_REVIVE_PERCENT);
        if (_currentTime < 0)
        {
            _currentTime = 0;
        }
    }

    /// <summary>
    /// we use this instead of Update() to minimize the loop Cycles
    /// </summary>
    private IEnumerator _UpdateCurrentTime()
    {
        if (_isGamePaused)
        {
            while (_isGamePaused)
                yield return new WaitForSeconds(0.1f); // ~10fps
        }

        _currentTime += 1;
        _UpdateTimeTextUi();
        if (_currentTime >= _maxTime)
        {
            WinAndLoseManager._instance._PendingLose_WaitForRevival();
            yield break;
        }

        if (_maxTime - _currentTime < _TICK_TOCK_THERSHOLD)
            AudioManager._instance._PlayAudio(_AudioType.SFX2, SavedSounds.Clock_Tick_Tock, true);

        yield return new WaitForSeconds(1);
        StartCoroutine(_UpdateCurrentTime());
    }
    #endregion
    #region Stone
    private void _TryLoadStoneChallenge()
    {
        if (LevelManager._instance._isUsingSavedData) return;

        if (_isStoneChallengeActive)
            for (int i = 0; i < _data._stone._stonesCount; i++)
                SpawnManager._instance._SpawnBlock(false);
    }
    #endregion
    #region Double
    public void _TryDoubleSpawn()
    {
        if (_isDoubleChallengeActive)
            SpawnManager._instance._SpawnBlock();
    }
    #endregion
    #region Getters

    /// <summary>
    /// used to calculate the _CHALLANGE_MP in coin manager and stars count on win
    /// </summary>
    public int _GetChallengeCount()
    {
        int _count = 0;
        if (_isTimeChallengeActive)
            _count++;
        if (_isStoneChallengeActive)
            _count++;
        if (_isDoubleChallengeActive)
            _count++;
        if (_count == 3) // it means all of them are active at the same time
            _count++;

        return _count;
    }
    #endregion
}