using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : Singleton_Abs<UiManager>
{
    [Header("After Game Is Finished")]
    [SerializeField] Button _restartBtn;
    [SerializeField] Button _restartBtn2;
    [SerializeField] Button _toLobbyBtn;
    [SerializeField] Button _toLobbyBtn2;

    [Header("Menu Options")]
    [SerializeField] Button _menuRestartBtn;
    [SerializeField] Button _menuToLobbyBtn;

    [Header("General Attachments")]
    [SerializeField] Text _objectiveText;

    private void Start()
    {
        _InitEvents();
    }
    private void _InitEvents()
    {
        // add MsgBox later
        _restartBtn.onClick.AddListener(LevelManager._instance._RestartTheLevel);
        _restartBtn2.onClick.AddListener(LevelManager._instance._RestartTheLevel);

        _toLobbyBtn.onClick.AddListener(() => LoadingManager._instance._LoadScene(_AllScenes.Lobby));
        _toLobbyBtn2.onClick.AddListener(() => LoadingManager._instance._LoadScene(_AllScenes.Lobby));

        _menuRestartBtn.onClick.AddListener(LevelManager._instance._RestartTheLevel);
        _menuToLobbyBtn.onClick.AddListener(() => LoadingManager._instance._LoadScene(_AllScenes.Lobby));
    }
}
