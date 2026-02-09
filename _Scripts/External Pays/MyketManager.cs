using MyketPlugin;
using UnityEngine;

public class MyketManager : Singleton_Abs<MyketManager>
{
    const string _publicKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCUDJH4p9JtC/fXFpw/cS8pXUvlP4u23IngoGpGyQdpnTzgF0OZCS5HFGM4vDL4kPNYldhO7LjvMPZekLf1qzmY6Nd0y6XFFIIiFS3iZptb9NqUzBEUHcpnnLsjZBdkX6wYJvq+oVDH28mo+cYzwJu6gxQsMS1AcqHfeUeFna6hiQIDAQAB";

    const string _1200GoldKey = "1200Gold";
    const string _3000GoldKey = "Gold3000";
    const string _8000GoldKey = "Gold8000";
    const string _20000GoldKey = "Gold20000";
    const string _noAdsKey = "Remove_Ads";
    const string _allMinesKey = "AllMines";

    private _MarketItems _currentPurchase = _MarketItems.None;
    private string _currentKey = "";

    private void Start()
    {
        MyketIAB.init(_publicKey);
    }
    private void OnEnable()
    {
        IABEventManager.purchaseSucceededEvent += _OnSuccessfulPurchase;
        IABEventManager.purchaseFailedEvent += _OnFailedPurchase;
    }
    private void OnDisable()
    {
        IABEventManager.purchaseSucceededEvent -= _OnSuccessfulPurchase;
        IABEventManager.purchaseFailedEvent -= _OnFailedPurchase;
    }
    public void _PurchaseItem(_MarketItems iPurchase)
    {
        if (_currentPurchase != _MarketItems.None) return;

        _currentPurchase = iPurchase;

        switch (iPurchase)
        {
            case _MarketItems.Gold1200:
                {
                    _currentKey = _1200GoldKey;
                    break;
                }
            case _MarketItems.Gold3000:
                {
                    _currentKey = _3000GoldKey;
                    break;
                }
            case _MarketItems.Gold8000:
                {
                    _currentKey = _8000GoldKey;
                    break;
                }
            case _MarketItems.Gold20000:
                {
                    _currentKey = _20000GoldKey;
                    break;
                }
            case _MarketItems.NoAds:
                {
                    _currentKey = _noAdsKey;
                    break;
                }
            case _MarketItems.AllMines:
                {
                    _currentKey = _allMinesKey;
                    break;
                }
        }

        MyketIAB.purchaseProduct(_currentKey);
    }
    private void _OnFailedPurchase(string purchase)
    {
        _currentPurchase = _MarketItems.None;
        _currentKey = "";

        ShopManager._instance._FailedPurchase();
    }
    private void _OnSuccessfulPurchase(MyketPurchase purchase)
    {
        ShopManager._instance._SuccessfulPurchase(_currentPurchase);

        if (_currentPurchase != _MarketItems.NoAds)
            MyketIAB.consumeProduct(_currentKey);

        _currentPurchase = _MarketItems.None;
        _currentKey = "";
    }
}