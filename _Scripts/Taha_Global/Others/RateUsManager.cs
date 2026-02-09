using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TahaGlobal.MsgBox;
public class RateUsManager : MonoBehaviour
{
    [SerializeField] int[] _showCd;
    [SerializeField] MsgBoxController _msgBox;

    bool _hasShownRateUs;

    private void Start()
    {
        _LoadData();
        _CheckShowRateUs();
    }
    private void _CheckShowRateUs()
    {
        if (_hasShownRateUs)
            if (LoadedTimesManager._instance._HasReachedNumber(_showCd))
                _ShowRateUs_WithConfirmation();
    }
    private void _ShowRateUs_WithConfirmation()
    {
        UnityEvent _event = new UnityEvent();
        _event.AddListener(() => IntentManager._instance._OpenIntent(_Intents.RateUs));
        _event.AddListener(_SaveHasShownRateUs);

        _msgBox._StartNewMsg(_event);
    }
    private void _SaveHasShownRateUs()
    {
        A.DataKey._SetTrue(A.DataKey.hasShownRateUs);
    }
    private void _LoadData()
    {
        _hasShownRateUs = A.DataKey._IsTrue(A.DataKey.hasShownRateUs);
    }
}
