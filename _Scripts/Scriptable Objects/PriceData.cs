using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Taha/Price Data")]
public class PriceData : ScriptableObject
{
    [SerializeField] int[] _freezePrices;
    [SerializeField] int[] _under8Prices;
    [SerializeField] int[] _specificPrices;
    [SerializeField] int[] _revivePrices;

    public int _GetPuPrice(_ExPuTypes iType, int iTotalTimesUsed)
    {
        if (iType == _ExPuTypes.Freeze)
        {
            if (_freezePrices.Length > iTotalTimesUsed)
                return _freezePrices[iTotalTimesUsed];
            else
                return _freezePrices[_freezePrices.Length - 1];
        }
        else if (iType == _ExPuTypes.Under_8)
        {
            if (_under8Prices.Length > iTotalTimesUsed)
                return _under8Prices[iTotalTimesUsed];
            else
                return _under8Prices[_under8Prices.Length - 1];
        }
        else if (iType == _ExPuTypes.Specific)
        {
            if (_specificPrices.Length > iTotalTimesUsed)
                return _specificPrices[iTotalTimesUsed];
            else
                return _specificPrices[_specificPrices.Length - 1];
        }
        return 0;
    }
    public int _GetRevivePrice(int iTotalTimesUsed)
    {
        if (_revivePrices.Length > iTotalTimesUsed)
            return _revivePrices[iTotalTimesUsed];
        else
            return _revivePrices[_freezePrices.Length - 1];
    }
}
