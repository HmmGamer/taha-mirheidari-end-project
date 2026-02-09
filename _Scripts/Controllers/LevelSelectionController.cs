using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TODO: clean the _UpdateStarsUi() method
/// reminder : this script's execution order needs to be after the StarsManager
/// </summary>
[RequireComponent(typeof(Button))]
public class LevelSelectionController : MonoBehaviour
{
    [SerializeField] Image[] _starImages;
    [SerializeField] GameObject _lockPanel;

    bool _isLockedForLackOfStars;
    private _FullLevelData _data;
    private Button _button;

    private void OnEnable()
    {
        CheatManager._onCheatActivationChanged += _UpdateCheat;
    }
    private void OnDisable()
    {
        CheatManager._onCheatActivationChanged -= _UpdateCheat;
    }

    public void _LockForLackOfStars()
    {
        _isLockedForLackOfStars = true;
    }
    public void _SetData(_FullLevelData iData)
    {
        _data = iData;
        _UpdateStarsUi();
    }
    private void _UpdateStarsUi()
    {
        // as this script is called via LevelSelectionManager, we update button here
        if (_button == null)
            _button = GetComponent<Button>();

        if (_data._isLevelUnlocked && !_isLockedForLackOfStars)
        {
            _lockPanel.SetActive(false);
            _button.enabled = true;
        }
        else
        {
            _lockPanel.SetActive(true);
            _button.enabled = false;
            return;
        }
        // stars logic
        if (!_data._isLevelFinished)
        {
            // level is not finished
            for (int i = 0; i < _starImages.Length; i++)
            {
                _starImages[i].sprite = LevelSelectionManager._instance._stars_attch._blackStar;
            }
            return;
        }
        else
        {
            // level is finished
            for (int i = 0; i < _starImages.Length; i++)
            {
                _starImages[i].sprite = LevelSelectionManager._instance._stars_attch._whiteStar;
            }
        }

        if (_data._last._isFinished)
        {
            for (int i = 0; i < _starImages.Length; i++)
            {
                _starImages[i].sprite = LevelSelectionManager._instance._stars_attch._purpleStar;
            }
            return;
        }

        int starCount = 0;
        if (_data._stone._isFinished)
            starCount++;
        if (_data._time._isFinished)
            starCount++;
        if (_data._double._isFinished)
            starCount++;

        for (int i = 0; i < starCount; i++)
        {
            _starImages[i].sprite = LevelSelectionManager._instance._stars_attch._goldenStar;
        }
    }

    public void _UpdateCheat()
    {
        if (CheatManager._instance._areCheatsActive)
        {
            _lockPanel.SetActive(false);
            _button.enabled = true;
        }
        else
            _UpdateStarsUi();
    }
}