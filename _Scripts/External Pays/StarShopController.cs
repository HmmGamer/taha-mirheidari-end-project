using System.Collections;
using System.Collections.Generic;
using TahaGlobal.MsgBox;
using UnityEngine;

public class StarShopController : MonoBehaviour
{
    [SerializeField] MsgBoxController _msgBox;

    public void _Msg_PurchaseStars()
    {
        _msgBox._StartNewMsg(_PurchaseStars);
    }
    private void _PurchaseStars()
    {
        ShopManager._instance._Purchase(_MarketItems.AllMines);
    }
}
