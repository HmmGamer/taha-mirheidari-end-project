using UnityEngine;
using UnityEngine.UI;

public class SavedContinueManager : Singleton_Abs<SavedContinueManager>
{
    [SerializeField] Button _continueButton;

    private int _lastUnfinishedLevel;
    private bool _hasStarted;

    private void Start()
    {
        if (_hasStarted) return;

        _InitButtons();

        _lastUnfinishedLevel = PlayerPrefs.GetInt(A.DataKey.lastUnfinishedLevel, A.DataKey.False);

        if (_lastUnfinishedLevel == A.DataKey.False)
            _continueButton.interactable = false;

        _hasStarted = true;
    }
    private void _InitButtons()
    {
        _continueButton.onClick.AddListener(_ContinueLastGame);
    }
    public bool _HasUnfinishedLevel()
    {
        if (_hasStarted)
            Start();

        if (_lastUnfinishedLevel == A.DataKey.False) 
            return false;

        return true;
    }
    public void _StartNewGame()
    {
        PlayerPrefs.SetInt(A.DataKey.lastUnfinishedLevel, A.DataKey.False);
    }
    public void _ContinueLastGame()
    {
        LoadingManager._instance._LoadScene(_AllScenes.MainGame);
    }
}
