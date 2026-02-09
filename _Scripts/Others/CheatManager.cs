using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// reminder : this script needs to be the last script in execution order
/// </summary>
public class CheatManager : Singleton_Abs<CheatManager>
{
    public static UnityAction _onCheatActivationChanged;

    [SerializeField] InventoryData _invData;

    [SerializeField] Button _enableCheats;
    [SerializeField] Button _disableCheats;

    public bool _areCheatsActive;

    private void Start()
    {
        DontDestroyOnLoad(transform.root);

        _enableCheats.onClick.AddListener(() => _ChangeCheatActivation(false));
        _disableCheats.onClick.AddListener(() => _ChangeCheatActivation(true));
    }
    private void OnEnable()
    {
        LoadingManager._onNewSceneLoaded += _OnNewSceneLoaded;
    }
    private void OnDisable()
    {
        LoadingManager._onNewSceneLoaded -= _OnNewSceneLoaded;
    }
    private void _OnNewSceneLoaded(_AllScenes iScene)
    {
        // so the scripts in new scene are updated based on new cheats value
        _onCheatActivationChanged?.Invoke();
    }
    private void _ChangeCheatActivation(bool iActivation)
    {
        _areCheatsActive = iActivation;

        if (iActivation)
        {
            _enableCheats.gameObject.SetActive(true);
            _disableCheats.gameObject.SetActive(false);
        }
        else
        {
            _enableCheats.gameObject.SetActive(false);
            _disableCheats.gameObject.SetActive(true);
        }

        _onCheatActivationChanged?.Invoke();
    }
}
