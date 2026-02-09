using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingManager : Singleton_Abs<SettingManager>
{
    private const string _Music = "Music";
    private const string _SFX = "SFX";

    private const float _minVolume = -80f;
    private const float _maxVolume = 0f;

    [Header("Audio Attachments")]
    [SerializeField] AudioMixer _mainAudioMixer;
    [SerializeField] Button _musicOffButton;
    [SerializeField] Button _musicOnButton;
    [SerializeField] Button _sfxOffButton;
    [SerializeField] Button _sfxOnButton;

    [Header("Vibrate Attachments")]
    [SerializeField] Button _vibrateOnButton;
    [SerializeField] Button _vibrateOffButton;

    private bool _isMusicEnabled = true;
    private bool _isSfxEnabled = true;

    private void Start()
    {
        _LoadAudioSettings();

        _musicOffButton.onClick.AddListener(() => _OnMusicChange(true));
        _musicOnButton.onClick.AddListener(() => _OnMusicChange(false));

        _sfxOffButton.onClick.AddListener(() => _OnSfxChange(true));
        _sfxOnButton.onClick.AddListener(() => _OnSfxChange(false));

        if (_vibrateOffButton && _vibrateOnButton)
        {
            _vibrateOffButton.onClick.AddListener(() => _OnVibrateChange(true));
            _vibrateOnButton.onClick.AddListener(() => _OnVibrateChange(false));
            _LoadVibrateUi();
        }

        _ApplyAudioSettings();
        _UpdateButtonStates();
    }

    #region Change Listeners
    public void _OnMusicChange(bool iActivation)
    {
        _isMusicEnabled = iActivation;
        _ApplyMusicSettings();
        _UpdateButtonStates();
        _SaveAudioSettings();
    }
    public void _OnSfxChange(bool iActivation)
    {
        _isSfxEnabled = iActivation;
        _ApplySfxSettings();
        _UpdateButtonStates();
        _SaveAudioSettings();
    }
    public void _OnVibrateChange(bool iActivation)
    {
        VibrateManager._instance._ChangeVibrateActivation(iActivation);
        _LoadVibrateUi();
    }
    #endregion

    private void _LoadVibrateUi()
    {
        bool isActive = VibrateManager._instance._IsVibrationActive();

        _vibrateOnButton.gameObject.SetActive(isActive);
        _vibrateOffButton.gameObject.SetActive(!isActive);
    }

    #region Audio Settings
    private void _ApplyAudioSettings()
    {
        _ApplyMusicSettings();
        _ApplySfxSettings();
    }
    private void _ApplyMusicSettings()
    {
        if (_isMusicEnabled)
        {
            _mainAudioMixer.SetFloat(_Music, _maxVolume);
        }
        else
        {
            _mainAudioMixer.SetFloat(_Music, _minVolume);
        }
    }
    private void _ApplySfxSettings()
    {
        if (_isSfxEnabled)
        {
            _mainAudioMixer.SetFloat(_SFX, _maxVolume);
        }
        else
        {
            _mainAudioMixer.SetFloat(_SFX, _minVolume);
        }
    }
    private void _UpdateButtonStates()
    {
        _musicOnButton.gameObject.SetActive(_isMusicEnabled);
        _musicOffButton.gameObject.SetActive(!_isMusicEnabled);
        _sfxOnButton.gameObject.SetActive(_isSfxEnabled);
        _sfxOffButton.gameObject.SetActive(!_isSfxEnabled);
    }
    private void _SaveAudioSettings()
    {
        PlayerPrefs.SetInt(_Music, _isMusicEnabled ? 1 : 0);
        PlayerPrefs.SetInt(_SFX, _isSfxEnabled ? 1 : 0);
    }
    private void _LoadAudioSettings()
    {
        _isMusicEnabled = PlayerPrefs.GetInt(_Music, 1) == 1;
        _isSfxEnabled = PlayerPrefs.GetInt(_SFX, 1) == 1;
    }
    #endregion
}