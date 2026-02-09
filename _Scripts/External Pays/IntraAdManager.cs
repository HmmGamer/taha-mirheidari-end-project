using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntraAdManager : MonoBehaviour
{
    const int _MAX_AD_SHOW_ATTEMPTS = 5;

    [SerializeField] InventoryData _invData;
    [SerializeField] int _intraAdCooldown = 300;

    [Tooltip("the times lobby is loaded before showing ads")]
    [SerializeField] int _minLoadsForAds = 5;

    private void Start()
    {
        _TryShowingIntraAd();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    private void _TryShowingIntraAd()
    {
        if (TimeManager._instance._GetTimerRemainingSec(_TimerNames.T_IntraAdTimer) > 0)
            return;
        if (LoadedTimesManager._instance._TotalEnters < _minLoadsForAds)
            return;

        if (!AdiveryManager._instance._IsIntraAdLoaded())
        {
            // on start of the game, the ads may take time to load so we wait some time
            StartCoroutine(_WaitForAdsLoading(_MAX_AD_SHOW_ATTEMPTS));
            return;
        }

        AdiveryManager._instance._ShowInterAd();
        TimeManager._instance._SetNewTimer(_TimerNames.T_IntraAdTimer, _intraAdCooldown, _TimerType.Temp);
    }
    private IEnumerator _WaitForAdsLoading(int iCurrentAttempts)
    {
        if (!LoadingManager._instance._IsInCurrentScene(_AllScenes.Lobby))
            yield break;

        if (AdiveryManager._instance._IsIntraAdLoaded())
        {
            _TryShowingIntraAd();
            yield break;
        }
        yield return new WaitForSeconds(0.2f);

        if (iCurrentAttempts > 0)
            StartCoroutine(_WaitForAdsLoading(iCurrentAttempts - 1));
    }
}
