using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Taha/Inventory Data")]
public class InventoryData : ScriptableObject
{
    public UnityAction _onCoinChanges;
    public UnityAction _onPowerUpChanges;

    [SerializeField] int _totalCoins = 100;
    [SerializeField] int _under8Vouchers;
    [SerializeField] int _specificVouchers;
    [SerializeField] int _freezeVouchers;

    public bool _removeAds;

    #region Power Ups
    public bool _HasPoweUp(_ExPuTypes iPowerUpType)
    {
        if (iPowerUpType == _ExPuTypes.Under_8)
            return _under8Vouchers > 0;
        else if (iPowerUpType == _ExPuTypes.Specific)
            return _specificVouchers > 0;
        else if (iPowerUpType == _ExPuTypes.Freeze)
            return _freezeVouchers > 0;

        return false;
    }
    public bool _ConsumePowerUp(_ExPuTypes iPowerUpType, int iCount = 1)
    {
        if (_HasPoweUp(iPowerUpType)) 
            return false;

        if (iPowerUpType == _ExPuTypes.Under_8)
            _under8Vouchers -= iCount;
        else if (iPowerUpType == _ExPuTypes.Specific)
            _specificVouchers -= iCount;
        else if (iPowerUpType == _ExPuTypes.Freeze)
            _freezeVouchers -= iCount;

        _onPowerUpChanges?.Invoke();
        return true;
    }
    public void _AddPowerUp(_ExPuTypes iPowerUpType, int iCount = 1)
    {
        if (iPowerUpType == _ExPuTypes.Under_8)
            _under8Vouchers += iCount;
        else if (iPowerUpType == _ExPuTypes.Specific)
            _specificVouchers += iCount;
        else if (iPowerUpType == _ExPuTypes.Freeze)
            _freezeVouchers += iCount;

        _onPowerUpChanges?.Invoke();
    }
    #endregion

    #region Ads
    public void _BuyRemoveAds()
    {
        _removeAds = true;
        _SaveData();
    }
    #endregion

    #region Coins
    public bool _HasEnoughCoins(int iPrice)
    {
        if (CheatManager._instance._areCheatsActive) return true;

        return _totalCoins >= iPrice;
    }
    public void _AddCoin(int iCount)
    {
        _totalCoins += iCount;
        _SaveData();
        _onCoinChanges?.Invoke();
    }
    public bool _ConsumeCoin(int iCount)
    {
        if (CheatManager._instance._areCheatsActive) return true;
        if (!_HasEnoughCoins(iCount)) return false;

        _totalCoins -= iCount;
        _SaveData();
        _onCoinChanges?.Invoke();
        return true;
    }
    public int _GetTotalCoins()
    {
        return _totalCoins;
    }
    #endregion

    #region Save/Load
    private void OnEnable()
    {
        _LoadData();
    }
    [CreateSOButton("Save Data")]
    public void _SaveData()
    {
        SaveTools._SaveSOToDisk(this, "invData");
    }
    public void _LoadData()
    {
        SaveTools._LoadSOFromDisk(this, "invData");
    }
    [CreateSOButton("Reset All Data")]
    public void _ResetAllData()
    {
        SaveTools._ResetSO(this, "invData");
    }
    #endregion
}