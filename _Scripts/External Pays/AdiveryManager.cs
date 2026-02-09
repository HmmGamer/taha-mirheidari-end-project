using AdiveryUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AdiveryManager : Singleton_Abs<AdiveryManager>
{
    const string APP_ID = "a9831a8f-7b83-4c98-8243-5416dcb4cccd";
    const string PLACEMENT_INTER_ID = "87727f61-b876-4d0a-9bc3-f05ffe642cb4";
    const string _REWARD_REVIVE_ID = "691743a0-fadb-48e1-a6a6-30abbb4641ce";
    const string _REWARD_COIN_ID = "1312f989-fecc-4c27-8027-4a62bb806e9d";

    public static UnityAction _onRewardedAdStart;
    public static UnityAction _onRewardedAdFinish;

    UnityEvent _currentReward;
    UnityEvent _failedEvent;

    AdiveryListener listener;
    private bool _areAdsRemoved = false;

    public void Start()
    {
        DontDestroyOnLoad(transform.root);

        _LoadAdRemovalData();

        Adivery.Configure(APP_ID);
        _PrepareAds();

        listener = new AdiveryListener();
        listener.OnRewardedAdClosed += _OnRewardedClosed;

        Adivery.AddListener(listener);
    }
    private void _PrepareAds()
    {
        Adivery.PrepareInterstitialAd(PLACEMENT_INTER_ID);
        Adivery.PrepareRewardedAd(_REWARD_REVIVE_ID);
        Adivery.PrepareRewardedAd(_REWARD_COIN_ID);
    }

    public void _OnRewardedClosed(object caller, AdiveryReward reward)
    {
        _onRewardedAdFinish?.Invoke();
        if (reward.IsRewarded)
        {
            _RewardPlayer();
        }
    }
    public bool _IsIntraAdLoaded()
    {
        if (Adivery.IsLoaded(PLACEMENT_INTER_ID))
        {
            return true;
        }
        return false;
    }
    public bool _IsRewardedAdLoaded()
    {
        if (Adivery.IsLoaded(_REWARD_REVIVE_ID))
        {
            return true;
        }
        return false;
    }
    public void _ShowInterAd()
    {
        if (Adivery.IsLoaded(PLACEMENT_INTER_ID))
        {
            if (_areAdsRemoved) return;

            Adivery.Show(PLACEMENT_INTER_ID);
        }
    }
    public void _ShowRewardedAd(_AdTypes iType, UnityEvent iReward, UnityEvent iFailedAction = null)
    {
        if (!_IsIntraAdLoaded())
        {
            iFailedAction.Invoke();
            return;
        }

        _currentReward = iReward;
        _failedEvent = iFailedAction;

        #region Editor Only
#if UNITY_EDITOR
        if (iReward != null) // always true
        {
            _RewardPlayer();
            return;
        }
#endif
        #endregion

        if (CheatManager._instance._areCheatsActive)
        {
            _RewardPlayer();
            _onRewardedAdStart?.Invoke();
            return;
        }

        if (iType == _AdTypes.revive && Adivery.IsLoaded(_REWARD_REVIVE_ID))
        {
            Adivery.Show(_REWARD_REVIVE_ID);
            _onRewardedAdStart?.Invoke();
        }
        else if (iType == _AdTypes.coin && Adivery.IsLoaded(_REWARD_COIN_ID))
        {
            Adivery.Show(_REWARD_COIN_ID);
            _onRewardedAdStart?.Invoke();
        }
        else
        {
            _failedEvent.Invoke();
        }
    }
    private void _RewardPlayer()
    {
        if (_currentReward != null)
            _currentReward?.Invoke();
        _currentReward = null;
    }

    #region Ads Removeal
    public void _RemoveAds()
    {
        _areAdsRemoved = true;
        PlayerPrefs.SetInt(A.DataKey.areAdsRemoved, A.DataKey.True);
    }
    public bool _AreAdsRemoved()
    {
        // we dont use _areAdsRemoved because it may not be loaded in time
        return PlayerPrefs.GetInt(A.DataKey.areAdsRemoved, 0) == A.DataKey.True;
    }
    private void _LoadAdRemovalData()
    {
        _areAdsRemoved =
            PlayerPrefs.GetInt(A.DataKey.areAdsRemoved, 0) == A.DataKey.True;
    }
    #endregion

    #region Ads Renew
    //private IEnumerator _AdRenew()
    //{

    //}
    #endregion

    #region Types
    public enum _AdTypes
    {
        revive, coin
    }
    #endregion
}
