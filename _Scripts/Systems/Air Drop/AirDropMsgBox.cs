using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AirDropMsgBox : Singleton_Abs<AirDropMsgBox>
{
    [SerializeField] GameObject _panel;
    [SerializeField] Image _rewardImage;

    public bool _isPanelActive => _panel.activeInHierarchy;

    public void _ShowNewMsg(AirDropData._AirDropChance iDrop)
    {
        _panel.SetActive(true);

        _rewardImage.sprite = iDrop._rewardSprite;
    }
}
