using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleManager : Singleton_Abs<ToggleManager>
{
    [Header("Toggle References")]
    public Toggle _timeTog;
    public Toggle _limitTog;
    public Toggle _doubleTog;
    public Toggle _lastTog;

    [SerializeField] GameObject _lockPanel;
    [SerializeField] LevelsSavedData _dataBase;

    private bool _isUpdatingToggles = false; // Prevent infinite loops

    private void Start()
    {
        _InitEvents();
    }
    private void _InitEvents()
    {
        _timeTog.onValueChanged.AddListener(delegate { _OnToggleValueChange(); });
        _limitTog.onValueChanged.AddListener(delegate { _OnToggleValueChange(); });
        _doubleTog.onValueChanged.AddListener(delegate { _OnToggleValueChange(); });
        _lastTog.onValueChanged.AddListener(delegate { _OnLastToggleChange(); });
    }
    public void _OnToggleValueChange()
    {
        if (_isUpdatingToggles) return;

        _isUpdatingToggles = true;

        // Check if all three main challenges are active
        bool allThreeActive = _timeTog.isOn && _limitTog.isOn && _doubleTog.isOn;

        // Auto-activate/deactivate last challenge based on the three main challenges
        _lastTog.isOn = allThreeActive;

        _isUpdatingToggles = false;
    }
    private void _OnLastToggleChange()
    {
        if (_isUpdatingToggles) return;

        _isUpdatingToggles = true;

        // If last challenge is being activated, activate all three main challenges
        if (_lastTog.isOn)
        {
            _timeTog.isOn = true;
            _limitTog.isOn = true;
            _doubleTog.isOn = true;
        }
        // If last challenge is being deactivated, deactivate all three main challenges
        else if (_timeTog.isOn && _limitTog.isOn && _doubleTog.isOn)
        {
            _timeTog.isOn = false;
            _limitTog.isOn = false;
            _doubleTog.isOn = false;
            _lastTog.isOn = false;
        }

        _isUpdatingToggles = false;
    }
    public void _SetNewChallengesData(int iLevel)
    {
        List<_AllChallengeTypes> challengeTypes = new List<_AllChallengeTypes>();

        if (_timeTog.isOn)
            challengeTypes.Add(_AllChallengeTypes.Limited_Time);
        if (_limitTog.isOn)
            challengeTypes.Add(_AllChallengeTypes.Limited_Moves);
        if (_doubleTog.isOn)
            challengeTypes.Add(_AllChallengeTypes.Double_Spawn);
        if (_lastTog.isOn)
            challengeTypes.Add(_AllChallengeTypes.Last_Challenge);

        _dataBase._UpdateChallenges(challengeTypes, iLevel);
    }
    public void _TogglesActivation(bool iActivation)
    {
        _lockPanel.gameObject.SetActive(!iActivation);
        _timeTog.interactable = iActivation;
        _limitTog.interactable = iActivation;
        _doubleTog.interactable = iActivation;
        _lastTog.interactable = iActivation;
    }
}