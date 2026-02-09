using System;
using System.Collections.Generic;
using UnityEngine;

class _TimeManagerSample : MonoBehaviour
{
    private void _Start()
    {
        // Start a global timer for 10 seconds - counts out of game as well
        TimeManager._instance._SetNewTimer(_TimerNames.G_CoinAdTimer, 10f
            , _TimerType.Global);

        // Check timer status
        _TimerStatus status = TimeManager._instance._GetTimerStatus(_TimerNames.G_CoinAdTimer);
        Debug.Log("Timer Status: " + status);

        // Get remaining seconds
        double remaining = TimeManager._instance._GetTimerRemainingSec(_TimerNames.G_CoinAdTimer);
        Debug.Log("Time Remaining: " + remaining);

        // Get formatted string for display
        string timerText = TimeManager._instance._GetStringTimerText(_TimerNames.G_CoinAdTimer);
        Debug.Log("Timer Text: " + timerText);
    }
}

/// <summary>
/// Manages multiple timers with global or local scope:
/// - Global timers count the real time, so even when the game is closed it works
/// - Local timers only count the in-game time, so it stops when the game is not open
/// - Temp timers reset when the application starts.
/// 
/// Use _SetNewTimer to start a timer, _GetTimerStatus to check its state,
/// _GetTimerRemainingSec to get the remaining seconds, and _GetStringTimerText to get a formatted string.
/// </summary>
public class TimeManager : Singleton_Abs<TimeManager>
{
    private List<_TimerData> _allTimersData = new List<_TimerData>();

    private bool _haveTimersLoaded; // to avoid using this script when its not loaded yet
    private DateTime _sessionStartTimeUtc; // the time that this script starting
    private double _cachedSessionSeconds;  // like Time.time but for this script

    private void Start()
    {
        DontDestroyOnLoad(transform.root);

        _sessionStartTimeUtc = DateTime.UtcNow;
        _CheckTimersLoaded();
    }
    private void OnApplicationPause(bool iIsPause)
    {
        if (iIsPause)
            _SaveData();
    }
    private void OnApplicationQuit()
    {
        _SaveData();
    }

    public void _SetNewTimer(_TimerNames iName, float iDuration, _TimerType iType)
    {
        _CheckTimersLoaded();

        /// this is to avoid cases like : 4.999 being shown as 4 instead of 5 at the start
        /// the timer status is usually called right after creating a timer but there is 
        /// some milliseconds difference that cause us problems
        iDuration += 0.1f;

        DateTime iStartTime = DateTime.UtcNow;
        DateTime iEndTime = iStartTime.AddSeconds(iDuration);

        _TimerData timer = _allTimersData.Find(t => t._name == iName);
        if (timer != null)
            _allTimersData.Remove(timer);

        _allTimersData.Add(new _TimerData(iName, iStartTime, iEndTime, iType));
        _SaveData();
    }

    #region Timer Getters
    public _TimerStatus _GetTimerStatus(_TimerNames iName)
    {
        _CheckTimersLoaded();

        _TimerData timer = _allTimersData.Find(t => t._name == iName);
        if (timer == null)
            return _TimerStatus.NotFound;

        if (timer._timerType == _TimerType.Local)
        {
            _UpdateLocalTimersFromSession();

            return timer._elapsedSeconds >= timer._targetSeconds
                ? _TimerStatus.Finished : _TimerStatus.InProgress;
        }

        if (DateTime.UtcNow >= timer._EndTimeUtc)
            return _TimerStatus.Finished;
        else
            return _TimerStatus.InProgress;
    }
    public double _GetTimerRemainingSec(_TimerNames iName)
    {
        _CheckTimersLoaded();

        _TimerData timer = _allTimersData.Find(t => t._name == iName);

        if (timer == null)
            return -1;

        if (timer._timerType == _TimerType.Local)
        {
            _UpdateLocalTimersFromSession();

            if (timer._elapsedSeconds >= timer._targetSeconds)
                return 0;
            else
                return timer._targetSeconds - timer._elapsedSeconds;
        }

        if (DateTime.UtcNow >= timer._EndTimeUtc)
            return 0;
        else
            return (timer._EndTimeUtc - DateTime.UtcNow).TotalSeconds;
    }

    /// <summary>
    /// change the Time Format as you see fit
    /// </summary>
    public string _GetStringTimerText(_TimerNames iName)
    {
        _CheckTimersLoaded();

        double remainingSec = _GetTimerRemainingSec(iName);
        if (remainingSec <= 0)
            return "0";

        TimeSpan ts = TimeSpan.FromSeconds(remainingSec);

        if (ts.Hours > 0)
            return string.Format("{0}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
        //else if (ts.Minutes > 0)
        return string.Format("{0}:{1:00}", ts.Minutes, ts.Seconds);
        //else
        //return string.Format("{0}", ts.Seconds.ToString("00"));
    }
    #endregion

    #region Private Methods
    private void _CheckTimersLoaded()
    {
        if (!_haveTimersLoaded)
        {
            _LoadData();
            _haveTimersLoaded = true;

            for (int i = 0; i < _allTimersData.Count; i++)
            {
                _allTimersData[i]._CheckTempTimers();
            }
        }
    }
    private void _UpdateLocalTimersFromSession()
    {
        double sessionDuration = (DateTime.UtcNow - _sessionStartTimeUtc).TotalSeconds - _cachedSessionSeconds;
        if (sessionDuration <= 0) return;

        for (int i = 0; i < _allTimersData.Count; i++)
        {
            var timer = _allTimersData[i];
            if (timer._timerType == _TimerType.Local && timer._elapsedSeconds < timer._targetSeconds)
            {
                timer._elapsedSeconds += sessionDuration;
            }
        }

        _cachedSessionSeconds += sessionDuration;
    }
    #endregion

    #region Save/Load
    [CreateMonoButton("Save Data")]
    public void _SaveData()
    {
        _UpdateLocalTimersFromSession();

        SaveTools._SaveListToDisk(ref _allTimersData, A.DataKey.timersData);
    }
    public void _LoadData()
    {
        SaveTools._LoadListFromDisk(ref _allTimersData, A.DataKey.timersData);
    }
    #endregion

    [Serializable]
    private class _TimerData
    {
        public _TimerNames _name;
        public long _startTimeTicks;
        public long _endTimeTicks;     // only used for global-temp timers
        public double _elapsedSeconds; // only used for local timers
        public double _targetSeconds;  // only used for local timers
        public _TimerType _timerType;

        public _TimerData(_TimerNames iName, DateTime iStartTime, DateTime iEndTime, _TimerType iTimerType)
        {
            _name = iName;
            _StartTimeUtc = iStartTime;
            _EndTimeUtc = iEndTime;
            _timerType = iTimerType;
            _elapsedSeconds = 0;
            _targetSeconds = (iEndTime - iStartTime).TotalSeconds;
        }
        public _TimerData(_TimerNames iName, float iDuration, _TimerType iTimerType)
        {
            _name = iName;
            _timerType = iTimerType;
            _targetSeconds = iDuration;
            _elapsedSeconds = 0;
            _StartTimeUtc = DateTime.UtcNow;
            _EndTimeUtc = _StartTimeUtc.AddSeconds(iDuration);
        }

        public DateTime _StartTimeUtc
        {
            get { return new DateTime(_startTimeTicks, DateTimeKind.Utc); }
            set { _startTimeTicks = value.Ticks; }
        }
        public DateTime _EndTimeUtc
        {
            get { return new DateTime(_endTimeTicks, DateTimeKind.Utc); }
            set { _endTimeTicks = value.Ticks; }
        }

        /// <summary>
        /// call this method on loading the timer so it will reset temp timers
        /// </summary>
        public void _CheckTempTimers()
        {
            // this resets the timer
            if (_timerType == _TimerType.Temp)
                _endTimeTicks = _startTimeTicks;
        }
    }
}
#region Enums
// Optional Naming : G => Global, L => Local (in game time), T => Temp ( reset's on exit)
public enum _TimerNames
{
    G_CoinAdTimer,   // in shop, for seeing rewarded ads to get coins
    G_FeedbackTimer, // in feedback menu, min time before each feedback
    L_FirstAdTimer,  // in game time to start showing ads (one-time timer)
    T_IntraAdTimer   // in intra ad manager, for showing new intra ads
}
public enum _TimerStatus
{
    NotFound = -1, Finished = 0, InProgress = 1
}
public enum _TimerType
{
    Global, Local, Temp
}
#endregion