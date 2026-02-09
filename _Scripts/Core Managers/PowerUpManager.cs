using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TODO : make the code more clean
/// </summary>
public class PowerUpManager : Singleton_Abs<PowerUpManager>
{
    const string _FREE = "Free";
    const int _FREEZE_TURNS = 3;
    const int _UNDER8_POWER = 8;
    const int _REMOVE_SPECIFIC_POWER = 64;

    [SerializeField] InventoryData _invData;
    [SerializeField] PriceData _priceData;

    [Header("Remove Under8")]
    [SerializeField] Button _under8Button;
    [SerializeField] Button _cancelUnder8Button;
    [SerializeField] Text _under8PriceText;

    [Header("Remove Specific")]
    [SerializeField] Button _specificRemoveButton;
    [SerializeField] Button _cancelSpecificButton;
    [SerializeField] Text _specificPriceText;

    [Header("Freeze")]
    [SerializeField] Button _freezeButton;
    [SerializeField] Button _cancelFreezeButton;
    [SerializeField] Text _freezePriceText;

    private _ExPuTypes _currentPowerUp;

    private int _freezeUsedCount = 0;
    private int _under8UsedCount = 0;
    private int _specificUsedCount = 0;

    private int _extraFreezeCount = 0;
    private int _extraUnder8Count = 0;
    private int _extraSpecificCount = 0;

    #region Initialization
    private void Start()
    {
        _InitButtons();
        _UpdatePowerUpsUi_OnPriceChange();
    }
    private void OnEnable()
    {
        GridManager._onBoardChanged.AddListener(_OnPlayerMovements);
        _invData._onCoinChanges += _UpdatePowerUpsUi_OnPriceChange;
    }
    private void OnDisable()
    {
        GridManager._onBoardChanged.RemoveListener(_OnPlayerMovements);
        _invData._onCoinChanges -= _UpdatePowerUpsUi_OnPriceChange;
    }
    private void _InitButtons()
    {
        _under8Button.onClick.AddListener(() => _SelectPowerUp(_ExPuTypes.Under_8));
        _specificRemoveButton.onClick.AddListener(() => _SelectPowerUp(_ExPuTypes.Specific));
        _freezeButton.onClick.AddListener(() => _SelectPowerUp(_ExPuTypes.Freeze));

        _cancelFreezeButton.onClick.AddListener(_CancelPowerUp);
        _cancelSpecificButton.onClick.AddListener(_CancelPowerUp);
        _cancelUnder8Button.onClick.AddListener(_CancelPowerUp);
    }
    #endregion

    #region Power-Up Selection
    public void _SelectPowerUp(_ExPuTypes iType)
    {
        if (!_HasPowerUp(iType))
        {
            return;
        }

        if (GridManager._instance._isMoving)
        {
            return;
        }

        if (_currentPowerUp != _ExPuTypes.None)
            _CancelPowerUp();

        _currentPowerUp = iType;

        _UpdateUiForUes();
        _EnableBlockSelection();
    }
    public void _CancelPowerUp()
    {
        _currentPowerUp = _ExPuTypes.None;
        _DisableBlockSelection();
        _UpdateUiForUes();
    }
    #endregion

    #region Ui Managment
    public void _UpdateUiForUes()
    {
        _DisableCancelButtons();

        if (_currentPowerUp == _ExPuTypes.Under_8)
        {
            _cancelUnder8Button.gameObject.SetActive(true);
        }
        else if (_currentPowerUp == _ExPuTypes.Specific)
        {
            _cancelSpecificButton.gameObject.SetActive(true);
        }
        else if (_currentPowerUp == _ExPuTypes.Freeze)
        {
            _cancelFreezeButton.gameObject.SetActive(true);
        }

        _UpdatePowerUpsUi_OnPriceChange();
        _UpdatePowerUpsUi_ExtraPowerups();
    }
    private void _UpdatePowerUpsUi_ExtraPowerups()
    {
        // this methos needs to be the last ui update

        if (_extraFreezeCount > 0)
            _freezePriceText.text = _FREE;
        if (_extraSpecificCount > 0)
            _specificPriceText.text = _FREE;
        if (_extraUnder8Count > 0)
            _under8PriceText.text = _FREE;
    }
    private void _UpdatePowerUpsUi_OnPriceChange()
    {
        if (_extraUnder8Count <= 0)
        {
            int under8Price = _priceData._GetPuPrice(_ExPuTypes.Under_8, _under8UsedCount);
            bool under8UnLocked = _invData._HasEnoughCoins(under8Price);
            _under8PriceText.text = GeneralTools._GetCoinsFormat(under8Price);
            _under8PriceText.color = under8UnLocked ? Color.white : Color.red;
            _under8Button.interactable = under8UnLocked;
        }

        if (_extraFreezeCount <= 0)
        {
            int freezePrice = _priceData._GetPuPrice(_ExPuTypes.Freeze, _freezeUsedCount);
            bool freezeUnLocked = _invData._HasEnoughCoins(freezePrice);
            _freezePriceText.text = GeneralTools._GetCoinsFormat(freezePrice);
            _freezePriceText.color = freezeUnLocked ? Color.white : Color.red;
            _freezeButton.interactable = freezeUnLocked;
        }
        
        if (_extraSpecificCount <= 0)
        {
            int specificPrice = _priceData._GetPuPrice(_ExPuTypes.Specific, _specificUsedCount);
            bool specificUnLocked = _invData._HasEnoughCoins(specificPrice);
            _specificPriceText.text = GeneralTools._GetCoinsFormat(specificPrice);
            _specificPriceText.color = specificUnLocked ? Color.white : Color.red;
            _specificRemoveButton.interactable = specificUnLocked;
        }

        if (CheatManager._instance._areCheatsActive)
        {
            _freezeButton.interactable = true;
            _under8Button.interactable = true;
            _specificRemoveButton.interactable = true;
            return;
        }
    }
    private void _DisableCancelButtons()
    {
        _cancelUnder8Button.gameObject.SetActive(false);
        _cancelSpecificButton.gameObject.SetActive(false);
        _cancelFreezeButton.gameObject.SetActive(false);
    }
    #endregion

    #region Block Selection and Interaction
    public void _OnBlockClicked(BlockController iBlock)
    {
        if (_currentPowerUp == _ExPuTypes.None) return;

        if (_currentPowerUp == _ExPuTypes.Under_8)
        {
            if (iBlock._value <= _UNDER8_POWER)
            {
                _UseRemoveUnder8();
            }
        }
        else if (_currentPowerUp == _ExPuTypes.Specific)
        {
            if (iBlock._value <= _REMOVE_SPECIFIC_POWER)
            {
                _UseRemoveSpecific(iBlock);
            }
        }
        else if (_currentPowerUp == _ExPuTypes.Freeze)
        {
            _UseFreezeBlock(iBlock);
        }
    }
    private void _EnableBlockSelection()
    {
        List<BlockController> _allBlocks = GridManager._instance._GetAllBlocksControllers();
        foreach (BlockController block in _allBlocks)
        {
            if (_currentPowerUp == _ExPuTypes.Under_8)
            {
                bool _canSelect = (block._value <= _UNDER8_POWER);
                block._canSelect = _canSelect;

                if (_canSelect)
                {
                    block._visualEffects._StartSelectionShine();
                }
            }
            else if (_currentPowerUp == _ExPuTypes.Specific)
            {
                bool _canSelect = (block._value <= _REMOVE_SPECIFIC_POWER);
                block._canSelect = _canSelect;

                if (_canSelect)
                {
                    block._visualEffects._StartSelectionShine();
                }
            }
            else if (_currentPowerUp == _ExPuTypes.Freeze)
            {
                block._canSelect = true;
                block._visualEffects._StartSelectionShine();
            }
        }

        AudioManager._instance._PlayAudio(_AudioType.SFX2, SavedSounds.Powerups_Shine);
    }
    private void _DisableBlockSelection()
    {
        List<BlockController> _allBlocks = GridManager._instance._GetAllBlocksControllers();
        foreach (BlockController block in _allBlocks)
        {
            block._canSelect = false;
            block._visualEffects._StopSelectionShine();
        }
    }
    #endregion

    #region Power-Up Usage
    private void _UseRemoveUnder8()
    {
        List<BlockController> _allBlocks = GridManager._instance._GetAllBlocksControllers();
        List<BlockController> _blocksToRemove = new List<BlockController>();

        foreach (BlockController block in _allBlocks)
        {
            if (block._value <= _UNDER8_POWER)
            {
                _blocksToRemove.Add(block);
            }
        }

        foreach (BlockController block in _blocksToRemove)
        {
            GridManager._instance._UnregisterBlock(block._GetCellPosition());

            block._visualEffects._PlayDestroyAnimation();
        }

        _BuyPowerUp(_ExPuTypes.Under_8);
        _CancelPowerUp();

        AudioManager._instance._PlayAudio(_AudioType.SFX2, SavedSounds.Block_Destroy, true);
    }
    private void _UseRemoveSpecific(BlockController iBlock)
    {
        GridManager._instance._UnregisterBlock(iBlock._GetCellPosition());
        iBlock._visualEffects._PlayDestroyAnimation();

        _BuyPowerUp(_ExPuTypes.Specific);
        _CancelPowerUp();

        AudioManager._instance._PlayAudio(_AudioType.SFX2, SavedSounds.Block_Destroy, true);
    }
    private void _UseFreezeBlock(BlockController iBlock)
    {
        iBlock._SetFrozen(_FREEZE_TURNS);

        _BuyPowerUp(_ExPuTypes.Freeze);
        _CancelPowerUp();
    }
    #endregion

    #region Utility Methods
    private bool _HasPowerUp(_ExPuTypes iType)
    {
        int puPrice = 0;

        if (iType == _ExPuTypes.Under_8)
            puPrice = _priceData._GetPuPrice(iType, _under8UsedCount);
        else if (iType == _ExPuTypes.Specific)
            puPrice = _priceData._GetPuPrice(iType, _specificUsedCount);
        else if (iType == _ExPuTypes.Freeze)
            puPrice = _priceData._GetPuPrice(iType, _freezeUsedCount);

        return _invData._HasEnoughCoins(puPrice);
    }
    private bool _BuyPowerUp(_ExPuTypes iType)
    {
        int puPrice = 0;

        if (iType == _ExPuTypes.Under_8)
        {
            if (_extraUnder8Count > 0)
            {
                _extraUnder8Count--;
                return true;
            }
            puPrice = _priceData._GetPuPrice(iType, _under8UsedCount);
            _under8UsedCount++;
        }
        else if (iType == _ExPuTypes.Specific)
        {
            if (_extraSpecificCount > 0)
            {
                _extraSpecificCount--;
                return true;
            }
            puPrice = _priceData._GetPuPrice(iType, _specificUsedCount);
            _specificUsedCount++;
        }
        else if (iType == _ExPuTypes.Freeze)
        {
            if (_extraFreezeCount > 0)
            {
                _extraFreezeCount--;
                return true;
            }
            puPrice = _priceData._GetPuPrice(iType, _freezeUsedCount);
            _freezeUsedCount++;
        }

        return _invData._ConsumeCoin(puPrice);
    }
    private void _OnPlayerMovements()
    {
        _CancelPowerUp();
    }
    public void _AddExtraPowerUp(_ExPuTypes iType)
    {
        if (iType == _ExPuTypes.Under_8)
            _extraUnder8Count++;
        else if (iType == _ExPuTypes.Specific)
            _extraSpecificCount++;
        else if (iType == _ExPuTypes.Freeze)
            _extraFreezeCount++;

        _UpdatePowerUpsUi_ExtraPowerups();
    }
    #endregion

    #region Getters/Setters
    public UsedPowerUpsSavedData _GetUsedPowerUps()
    {
        //return new UsedPowerUpsSavedData(_isFreezeUsed, _isUnder8Used
        //    , _isRemoveSpecificUsed);

        UsedPowerUpsSavedData item = new UsedPowerUpsSavedData();
        item._freezeUsedCount = _freezeUsedCount;
        item._under8UsedCount = _under8UsedCount;
        item._removeSpecificUsedCount = _specificUsedCount;
        return item;
    }
    public void _SetUsedPowerUps(UsedPowerUpsSavedData iNewData)
    {
        _freezeUsedCount = iNewData._freezeUsedCount;
        _under8UsedCount = iNewData._under8UsedCount;
        _specificUsedCount = iNewData._removeSpecificUsedCount;
    }
    #endregion
}
#region Enums
public enum _InPowerUpTypes
{

}
public enum _ExPuTypes
{
    None, Under_8, Specific, Freeze
}
#endregion