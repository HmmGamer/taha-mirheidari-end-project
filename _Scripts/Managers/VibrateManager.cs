using CandyCoded.HapticFeedback;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibrateManager : Singleton_Abs<VibrateManager>
{
    [SerializeField, ReadOnly] bool _isVibrationActive;

    private void Start()
    {
        DontDestroyOnLoad(transform.root);

        _LoadData();
    }
    public void _ChangeVibrateActivation(bool iActivation)
    {
        _isVibrationActive = iActivation;
        _SaveData();
    }
    public bool _IsVibrationActive()
    {
        return _isVibrationActive;
    }

    #region Save/Load
    private void _SaveData()
    {
        int isActive = _isVibrationActive ? A.DataKey.True : A.DataKey.False;
        PlayerPrefs.SetInt(A.DataKey.isVibrateActive, isActive);
    }
    private void _LoadData()
    {
        _isVibrationActive = A.DataKey._IsTrue(A.DataKey.isVibrateActive, true);
    }
    [CreateMonoButton("Reset Vibration Settings")]
    public void _ResetData()
    {
        PlayerPrefs.DeleteKey(A.DataKey.isVibrateActive);
    }
    #endregion

    #region Vibrate Methods
    public void _LightVibrate()
    {
        if (!_isVibrationActive)
            HapticFeedback.LightFeedback();
    }
    public void _MediumVibrate()
    {
        if (!_isVibrationActive)
            HapticFeedback.MediumFeedback();
    }
    public void _HeavyVibrate()
    {
        if (!_isVibrationActive)
            HapticFeedback.HeavyFeedback();
    }
    #endregion
}
