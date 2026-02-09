using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinManager : Singleton_Abs<CoinManager>
{
    #region Consts
    const float _CHALLANGE_MP = 0.05f;   // added not Mp
    const float _MINE_TYPE_MP = 0.15f;   // added not Mp
    const float _WIN_MP = 1;
    const float _LOSE_MP = 0.7f;
    const int _SCORE_TO_COIN_RATIO = 10; // average for a 2048 => 60000 score

    // these part is to balance time to coin reward
    const int _LOW_SCORE_LIMIT = 3000;
    const float _LOW_SCORE_PENALTY_MP = 0f; // between 0 to 1
    const int _HIGH_SCORE_LIMIT = 18000;
    const float _HIGH_SCORE_REWARD_MP = 0.15f; // between 0 to 1
    const int _ULTRA_HIGH_SCORE_LIMIT = 52000;
    const float _ULTRA_HIGH_SCORE_REWARD_MP = 0.25f; // between 0 to 1
    #endregion

    [SerializeField] InventoryData _invData;
    [SerializeField] Text _gainedCoinText;
    [SerializeField] Text _gainedCoin2Text;

    #region Coin Calculation
    private float _coinMP = 1;
    private int _baseCoins;
    public int _completeCalculatedCoins { get; private set; }

    private void _CalculateCoinMP()
    {
        _coinMP = 1;

        _coinMP += ChallengeManager._instance._GetChallengeCount() * _CHALLANGE_MP;

        _coinMP += (int)LevelManager._instance._currentMineType._levelType * _MINE_TYPE_MP;

        _coinMP *= WinAndLoseManager._instance._isWin ? _WIN_MP : _LOSE_MP;

        if (ScoreManager._instance._currentScore < _LOW_SCORE_LIMIT)
            _coinMP *= 1 - _LOW_SCORE_PENALTY_MP;
        else if (ScoreManager._instance._currentScore > _ULTRA_HIGH_SCORE_LIMIT)
            _coinMP *= 1 + _ULTRA_HIGH_SCORE_REWARD_MP;
        else if (ScoreManager._instance._currentScore > _HIGH_SCORE_LIMIT)
            _coinMP *= 1 + _HIGH_SCORE_REWARD_MP;

        _coinMP *= LevelManager._instance._currentMapData._coinMultiplayer;
    }
    private void _CalculateCoinPerScore()
    {
        _baseCoins = ScoreManager._instance._currentScore / _SCORE_TO_COIN_RATIO;
    }
    private void _UpdateCoinTextUi()
    {
        // text1 and text2 are for the win and lose
        _gainedCoinText.text = GeneralTools._GetCoinsFormat(_completeCalculatedCoins);
        _gainedCoin2Text.text = GeneralTools._GetCoinsFormat(_completeCalculatedCoins);
    }
    public void _CalculateTotalCoins(bool iUpdateUi = false)
    {
        _CalculateCoinPerScore();
        _CalculateCoinMP();

        _completeCalculatedCoins = (int)(_baseCoins * _coinMP);

        if (iUpdateUi)
        {
            ServerDataCollector._instance._SendDataToServer();
            _UpdateCoinTextUi();
        }
    }
    public void _AddCoinsToInventory()
    {
        if (_completeCalculatedCoins == 0)
            _CalculateTotalCoins();

        _invData._AddCoin(_completeCalculatedCoins);
    }
    #endregion
    #region Coin Usage
    public bool _hasEnoughMoney(int iPrice)
    {
        return _invData._HasEnoughCoins(iPrice);
    }
    public bool _PurchaseItem(int iPrice)
    {
        if (!_hasEnoughMoney(iPrice))
            return false;

        _invData._ConsumeCoin(iPrice);
        return true;
    }
    #endregion
}
