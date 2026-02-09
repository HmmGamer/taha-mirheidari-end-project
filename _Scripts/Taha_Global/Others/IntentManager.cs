using System;
using UnityEngine;

/// <summary>
/// TODO: Automate store detection based on packages
/// </summary>
public class IntentManager : Singleton_Abs<IntentManager>
{
    const string _M_UPDATE_APP_URL = "myket://check-update?id=";
    const string _M_SHARE_APP_WEB_URL = " ";
    const string _M_RATE_APP_URL = "myket://comment?id=";

    [SerializeField] bool _autoCheckUpdates = true;

    private void Start()
    {
        if (_autoCheckUpdates)
            _OpenIntent(_Intents.Update);
    }
    public void _OpenIntent(_Intents iIntent)
    {
        string packageName = Application.identifier;

        if (iIntent == _Intents.Share_NotWorking)
            TryOpenUrl(_M_SHARE_APP_WEB_URL + packageName);
        else if (iIntent == _Intents.RateUs)
            TryOpenUrl(_M_RATE_APP_URL + packageName);
        else if (iIntent == _Intents.Update)
            TryOpenUrl(_M_UPDATE_APP_URL + packageName);
    }
    private bool TryOpenUrl(string url)
    {
        #region Editor Only
#if !UNITY_ANDROID || UNITY_EDITOR
        if (transform)
            return false;
#endif
        #endregion

        try
        {
            Application.OpenURL(url);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"TryOpenUrl failed for '{url}': {e.Message}");
            return false;
        }
    }
}
public enum _Intents
{
    Share_NotWorking, RateUs, Update
}