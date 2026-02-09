using UnityEngine;
using UnityEngine.Events;

public class LoadedTimesManager : Singleton_Abs<LoadedTimesManager>
{
    [SerializeField] _LoadingEvents[] _optionalEvents;

    int _totalEnters;
    bool _hasStarted;

    public int _TotalEnters
    {
        get
        {
            if (!_hasStarted)
            {
                Start();
            }
            return _totalEnters;
        }
        private set
        {
            _totalEnters = value;
        }
    }

    void Start()
    {
        if (_hasStarted)
            return;

        DontDestroyOnLoad(transform.root);
        _hasStarted = true;

        string iKey = A.DataKey.totalLoadTimes;
        int iNewVal = PlayerPrefs.GetInt(iKey, 0) + 1;
        PlayerPrefs.SetInt(iKey, iNewVal);

        _TotalEnters = iNewVal;
        _CheckEvents();
    }
    private void _CheckEvents()
    {
        foreach (var item in _optionalEvents)
            if (item._totalTimes == _totalEnters)
                item._event.Invoke();
    }
    public bool _HasReachedNumber(int iTimes)
    {
        if (!_hasStarted)
            Start();

        if (iTimes == _totalEnters)
            return true;
        return false;
    }
    public bool _HasReachedNumber(int[] iTimes)
    {
        if (!_hasStarted)
            Start();

        foreach (int item in iTimes)
            if (item == _totalEnters)
                return true;
        return false;
    }

    #region Unity Editor
#if UNITY_EDITOR
    [CreateMonoButton("Reset Total Enters")]
    void _ResetTotalEnters()
    {
        string iKey = A.DataKey.totalLoadTimes;
        PlayerPrefs.SetInt(iKey, 0);
        _TotalEnters = 0;
    }
#endif
    #endregion

    [System.Serializable]
    public class _LoadingEvents
    {
        public int _totalTimes;
        public UnityEvent _event;
    }
}
