using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TahaGlobal.MsgBox;

[RequireComponent(typeof(NextLevelController))]
public class LevelSelectionManager : Singleton_Abs<LevelSelectionManager>
{
    [SerializeField] Button[] _allLevelButtons;
    [SerializeField] LevelsSavedData _allSavedData;

    [Header("General Attachments")]
    [SerializeField] GameObject _infoPanel;
    [SerializeField] Image _levelImage;
    [SerializeField] Button _StartButton; // continue is handled in saved continue manager
    [SerializeField] MsgBoxController _msgBox;
    [SerializeField] Sprite[] _levelSprites;

    [Header("Challenges Attachments")]
    [SerializeField] _Ch_Attach _ch_attch;

    [Header("Stars Attachments")]
    public _St_Attach _stars_attch;

    private int _selectedLevel = -1;
    private _FullLevelData _levelData;

    private void Start()
    {
        _InitButtons();
    }
    private void _InitButtons()
    {
        #region Editor Only
#if UNITY_EDITOR
        if (_allLevelButtons[0] != null)
        {
            Debug.LogError("we dont have a level 0 so the first index in _allLevelButtons should be null");
            return;
        }
#endif
        #endregion

        for (int i = 1; i < _allLevelButtons.Length; i++)
        {
            #region EditorBugReporter
#if UNITY_EDITOR
            if (i >= _allSavedData._allLevelsData.Length)
            {
                Debug.LogError("there is no data for level " + i + " in LevelsSavedData");
                return;
            }
#endif
            #endregion

            int b = i;
            _allLevelButtons[i].onClick.AddListener(() => _SelectLevel(_allSavedData._allLevelsData[b]));
            _allLevelButtons[i].GetComponent<LevelSelectionController>()._SetData(_allSavedData._allLevelsData[b]);
        }

        _StartButton.onClick.AddListener(_TryStartNewLevel);
    }
    public void _SelectLevel(_FullLevelData iData)
    {
        _selectedLevel = iData._level;
        _levelData = iData;
        _UpdateInfoPanelUi();
    }
    private void _UpdateInfoPanelUi()
    {
        _OpenInfoPanel();
        _levelImage.sprite = _levelSprites[_levelData._level];

        _UpdateChallengeInfo();

        bool _areChallengesUnlocked = _allSavedData._IsLevelFinished(_levelData._level);
        ToggleManager._instance._TogglesActivation(_areChallengesUnlocked);
    }
    private void _UpdateChallengeInfo()
    {
        // time challenge
        _ch_attch._timeStar.SetActive(_levelData._time._isFinished);

        // stone challenge
        _ch_attch._stoneStar.SetActive(_levelData._stone._isFinished);

        // doubleBlock challenge
        _ch_attch._doubleStar.SetActive(_levelData._double._isFinished);

        // finished all challenges
        _ch_attch._lastStar.SetActive(_levelData._last._isFinished);
    }
    private void _OpenInfoPanel()
    {
        _infoPanel.SetActive(true);
    }
    private void _TryStartNewLevel()
    {
        if (SavedContinueManager._instance._HasUnfinishedLevel())
            _msgBox._StartNewMsg(_StartNewNextLevel);
        else
            _StartNewNextLevel();
    }
    private void _StartNewNextLevel()
    {
        SavedContinueManager._instance._StartNewGame();
        ToggleManager._instance._SetNewChallengesData(_selectedLevel);
        GetComponent<NextLevelController>()._StartLevel(_selectedLevel);
    }

    #region Types
    [System.Serializable]
    private class _Ch_Attach
    {
        public GameObject _timeStar;

        public GameObject _stoneStar;

        public GameObject _doubleStar;

        public GameObject _lastStar;
    }
    [System.Serializable]
    public class _St_Attach
    {
        public Sprite _blackStar;
        public Sprite _whiteStar;
        public Sprite _goldenStar;
        public Sprite _purpleStar;
    }
    #endregion
}
