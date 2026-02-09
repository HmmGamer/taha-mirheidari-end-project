using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarsManager : Singleton_Abs<StarsManager>
{
    [SerializeField] Text _starCountText;

    [SerializeField] LevelsSavedData _dataBase;
    [SerializeField] _LockPanelData[] _allLockData;

    [ReadOnly, SerializeField] int _starsCount;

    bool _areMinesPurchased;

    public void Start()
    {
        _LoadData();

        _UpdateStarsCount();
        _UpdateUi();
    }
    private void OnEnable()
    {
        CheatManager._onCheatActivationChanged += Start;
    }
    private void OnDisable()
    {
        CheatManager._onCheatActivationChanged -= Start;
    }
    private void _UpdateStarsCount()
    {
        _starsCount = 0;

        if (CheatManager._instance._areCheatsActive)
        {
            _starsCount += 100;
        }

        for (int i = 0; i < _dataBase._allLevelsData.Length; i++)
        {
            _starsCount += _dataBase._allLevelsData[i]._GetStarsCount();
        }
    }
    private void _UpdateUi()
    {
        for (int i = 0; i < _allLockData.Length; i++)
        {
            if (_allLockData[i]._minStars > _starsCount)
                _Try_LockMine(_allLockData[i]);
            else
                _UnlockMines(_allLockData[i]);
        }

        _starCountText.text = _starsCount.ToString();
    }
    private void _Try_LockMine(_LockPanelData iData)
    {
        if (_areMinesPurchased)
        {
            _UnlockMines(iData);
            return;
        }

        iData._starsToUnlockText.text = (iData._minStars - _starsCount).ToString();
        iData._lockPanel.SetActive(true);
    }
    private void _UnlockMines(_LockPanelData iData)
    {
        iData._lockPanel.SetActive(false);
    }

    public void _UnlockMines()
    {
        _areMinesPurchased = true;
        _UpdateUi();
        _SaveData();
    }

    #region Save/Load
    private void _SaveData()
    {
        A.DataKey._SetTrue(A.DataKey.areMinesPurchased);
    }
    private void _LoadData()
    {
        _areMinesPurchased = A.DataKey._IsTrue(A.DataKey.areMinesPurchased);
    }
    [CreateMonoButton("Reset Unlocked Mines")]
    public void _ResetData()
    {
        PlayerPrefs.DeleteKey(A.DataKey.areMinesPurchased);
    }
    #endregion

    #region Types
    [System.Serializable]
    public class _LockPanelData
    {
        public LevelSelectionController[] _selectionController;
        public int _minStars;
        public GameObject _lockPanel;
        public Text _starsToUnlockText;
    }
    #endregion
}
