using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TahaGlobal.MsgBox;

public class ShopManager : Singleton_Abs<ShopManager>
{
    const int _AD_COIN_COUNT = 2000;

    [SerializeField] InventoryData _invData;
    [SerializeField] _Buttons _allButtons;
    [SerializeField] GameObject _shopCanvas;
    [SerializeField] MsgBoxController _failedMsgBox;
    [SerializeField] MsgBoxController _SuccessfulMsgBox;

    #region Starter
    private void Start()
    {
        _InitEvents();

        DontDestroyOnLoad(transform.root);
    }
    private void _InitEvents()
    {
        _allButtons._1200GoldButton.onClick.AddListener
            (() => _Purchase(_MarketItems.Gold1200));
        _allButtons._3000GoldButton.onClick.AddListener
            (() => _Purchase(_MarketItems.Gold3000));
        _allButtons._8000GoldButton.onClick.AddListener
            (() => _Purchase(_MarketItems.Gold8000));
        _allButtons._20000GoldButton.onClick.AddListener
            (() => _Purchase(_MarketItems.Gold20000));

        _allButtons._removeAdsButton.onClick.AddListener
            (() => _Purchase(_MarketItems.NoAds));
    }
    private void _CheckRemoveAds()
    {
        if (AdiveryManager._instance._AreAdsRemoved())
            _allButtons._removeAdsButton.interactable = false;
    }
    #endregion

    #region Purchase Methods
    public void _Purchase(_MarketItems iShopItem)
    {
        #region Editor Only
#if UNITY_EDITOR
        if (transform != null)
        {
            _SuccessfulPurchase(iShopItem);
            return;
        }
#endif
        #endregion

        if (CheatManager._instance._areCheatsActive)
        {
            _SuccessfulPurchase(iShopItem);
            return;
        }

        MyketManager._instance._PurchaseItem(iShopItem);
    }
    public void _SuccessfulPurchase(_MarketItems iPurchase)
    {
        _SuccessfulMsgBox._StartMsg();

        if (iPurchase == _MarketItems.Gold1200)
            _invData._AddCoin(12000);
        else if (iPurchase == _MarketItems.Gold3000)
            _invData._AddCoin(30000);
        else if (iPurchase == _MarketItems.Gold8000)
            _invData._AddCoin(80000);
        else if (iPurchase == _MarketItems.Gold20000)
            _invData._AddCoin(200000);

        else if (iPurchase == _MarketItems.NoAds)
        {
            AdiveryManager._instance._RemoveAds();
            _CheckRemoveAds();
            return;
        }

        else if (iPurchase == _MarketItems.AllMines)
        {
            StarsManager._instance._UnlockMines();
        }
    }
    public void _FailedPurchase()
    {
        _failedMsgBox._StartMsg();
    }
    public void _RewardCoinForWatchingAds()
    {
        _invData._AddCoin(_AD_COIN_COUNT);
    }
    #endregion

    public void _ShopCanvasActivation(bool iActivation)
    {
        // reminder : you need to have a helper script to avoid missing OnDisable
        _shopCanvas.SetActive(iActivation);

        if (iActivation)
        {
            if (ChallengeManager._instance != null)
                ChallengeManager._instance._B_PauseTimer();
        }
        else
        {
            if (ChallengeManager._instance != null)
                ChallengeManager._instance._B_UnPauseTimer();
        }
    }

    #region types

    [System.Serializable]
    public class _Buttons
    {
        public Button _1200GoldButton;
        public Button _3000GoldButton;
        public Button _8000GoldButton;
        public Button _20000GoldButton;

        public Button _removeAdsButton;
    }
    #endregion
}
public enum _MarketItems
{
    None, Gold1200, Gold3000, Gold8000, Gold20000, NoAds, AllMines
}