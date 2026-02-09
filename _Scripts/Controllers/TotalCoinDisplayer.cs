using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TotalCoinDisplayer : MonoBehaviour
{
    [SerializeField] Text _coinsText;
    [SerializeField] InventoryData _invData;

    private void OnEnable()
    {
        _invData._onCoinChanges += _UpdateCoinUi;

        // dont put this in start as its not always active, we need refresh on enable
        _UpdateCoinUi();
    }
    private void OnDisable()
    {
        _invData._onCoinChanges -= _UpdateCoinUi;
    }
    private void _UpdateCoinUi()
    {
        _coinsText.text = GeneralTools._GetCoinsFormat(_invData._GetTotalCoins());
    }
}
