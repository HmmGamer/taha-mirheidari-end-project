using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ReviveManager : Singleton_Abs<ReviveManager>
{
    // after this time the player cant revive anymore and will lose instantly
    const float _WAIT_FOR_REVIVE_TIME = 11;
    const int _MAX_REVIVES = 3;
    const int _MAX_AD_REVIVES = 1;

    const float _EMPTY_BLOCK_PERCENT = 0.25f; // between 0 and 1
    const int _MAX_DELETABLE_BLOCK = 32;

    [SerializeField] PriceData _priceData;
    [SerializeField] InventoryData _invData;

    [SerializeField] GameObject _revivePanel;
    [SerializeField] GameObject _reviveWithAdsButton;   // can revive with ads loaded
    [SerializeField] GameObject _reviveOutOfAdsImage;   // the ads are not loaded image
    [SerializeField] GameObject _goldRevivePanel;       // normal golds use to revive

    [SerializeField] Image _reviveTimer;
    [SerializeField] Text _revivePriceText;
    [SerializeField] Button _goldReviveButton;
    [SerializeField] Button _exitButton;

    public int _currentReviveCount { get; set; }

    private bool _areAdsActive;

    private void Start()
    {
        _goldReviveButton.onClick.AddListener(() => _ReviveAfterLose(false));
        _exitButton.onClick.AddListener(WinAndLoseManager._instance._PerformLose);
    }
    private void OnEnable()
    {
        _reviveTimer.fillMethod = Image.FillMethod.Radial360;
        _reviveTimer.fillOrigin = (int)Image.Origin360.Bottom;
        _reviveTimer.fillClockwise = true;
        AdiveryManager._onRewardedAdStart += _OnAdStart;
        AdiveryManager._onRewardedAdFinish += _OnAdFinish;
    }
    private void OnDisable()
    {
        AdiveryManager._onRewardedAdStart -= _OnAdStart;
        AdiveryManager._onRewardedAdFinish -= _OnAdFinish;
    }
    public void _CheckForPossibleRevive() // + activate Ui
    {
        if (_currentReviveCount <= _MAX_REVIVES)
        {
            _revivePanel.SetActive(true);
            _goldRevivePanel.SetActive(true);
            _StartRevivalTimer();
        }
        else
        {
            WinAndLoseManager._instance._PerformLose();
            _revivePanel.SetActive(false);
            return;
        }

        _UpdateAdsPanelUi();
        _UpdateCoinsUi();
    }
    public void _ReviveAfterLose(bool iIsWatchingAd)
    {
        if (!iIsWatchingAd)
            _invData._ConsumeCoin(_priceData._GetRevivePrice(_currentReviveCount));

        _revivePanel.SetActive(false);
        _currentReviveCount++;

        int emptyCount = GridManager._instance._GetAllSpawnableCells().Count;
        int neededEmptyBlocks = (int)(_EMPTY_BLOCK_PERCENT
            * GridManager._instance._GetAllFloorCells().Count) + 1;

        #region Editor Only
#if UNITY_EDITOR
        if (neededEmptyBlocks == 1)
            neededEmptyBlocks = 2;
#endif
        #endregion

        if (emptyCount < neededEmptyBlocks)
        {
            List<BlockController> blocks = GridManager._instance._GetAllBlocksControllers();
            blocks = blocks.OrderBy(b => Random.value).ToList();

            foreach (BlockController block in blocks)
            {
                if (block._value <= _MAX_DELETABLE_BLOCK && block._isBlockMergeable)
                {
                    GridManager._instance._UnregisterBlock(block._GetCellPosition());
                    block._visualEffects._PlayDestroyAnimation();
                    emptyCount++;
                    if (emptyCount >= neededEmptyBlocks) break;
                }
            }
        }

        WinAndLoseManager._instance._RevivePlayer();

        ChallengeManager._instance._ReviveTimer();
        ChallengeManager._instance._B_UnPauseTimer();

        InputManager._instance.gameObject.SetActive(true);
    }
    private void _UpdateCoinsUi()
    {
        int revivePrice = _priceData._GetRevivePrice(_currentReviveCount);
        _revivePriceText.text = GeneralTools._GetCoinsFormat(revivePrice);

        bool hasEnoughMoney = _invData._HasEnoughCoins(revivePrice);
        _revivePriceText.color = hasEnoughMoney ? Color.white : Color.red;
        _goldReviveButton.interactable = hasEnoughMoney;
    }
    private void _UpdateAdsPanelUi()
    {
        if (_currentReviveCount < _MAX_AD_REVIVES)
        {
            if (AdiveryManager._instance._IsRewardedAdLoaded())
            {
                _reviveWithAdsButton.SetActive(true);
                _reviveOutOfAdsImage.SetActive(false);
            }
            else
            {
                _reviveWithAdsButton.SetActive(false);
                _reviveOutOfAdsImage.SetActive(true);
            }
        }
        else
        {
            _reviveWithAdsButton.SetActive(false);
            _reviveOutOfAdsImage.SetActive(false);
        }
    }
    private void _OnAdStart()
    {
        _areAdsActive = true;
    }
    private void _OnAdFinish()
    {
        _areAdsActive = false;
    }
    private void _StartRevivalTimer()
    {
        if (_reviveTimer == null) return;

        StopAllCoroutines();
        StartCoroutine(_RevivalTimeExpireCD());
    }
    private IEnumerator _RevivalTimeExpireCD()
    {
        _reviveTimer.fillAmount = 1;
        float _currentTime = 0;
        float _step = 0.05f;

        while (_currentTime < _WAIT_FOR_REVIVE_TIME)
        {
            while (_areAdsActive)
                yield return new WaitForSeconds(0.2f);

            _currentTime += _step;
            _reviveTimer.fillAmount = _currentTime / _WAIT_FOR_REVIVE_TIME;

            yield return new WaitForSeconds(_step);
        }

        VibrateManager._instance._MediumVibrate();

        _revivePanel.SetActive(false);
        WinAndLoseManager._instance._PerformLose();
    }
}
