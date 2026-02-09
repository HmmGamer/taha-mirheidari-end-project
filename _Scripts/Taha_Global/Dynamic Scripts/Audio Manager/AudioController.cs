using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    [SerializeField] _AudioType _audioType;
    [Tooltip("true => automatically adds _playAudio event to the button")]
    [SerializeField] bool _playOnButtonClick;
    [SerializeField] bool _useSavedAudio;
    [SerializeField] bool _playOneShot = false;
    [SerializeField] bool _playOnAwake = false;
    [SerializeField, Range(0, 1)] float _volume = 1;

    [SerializeField, ConditionalField(nameof(_useSavedAudio))]
    SavedSounds _audioName;
    [SerializeField, ConditionalField(nameof(_useSavedAudio), true)]
    AudioClip _audio;

    private void Start()
    {
        if (_playOnButtonClick)
        {
            Button button = gameObject.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(_PlayAudio);
            else
                Debug.LogError("attach button component to " +
                    gameObject.name + " or deactivate _playOnButtonClick condition");
        }
        if (_playOnAwake)
            _PlayAudioOnAwake();
    }
    private void _PlayAudioOnAwake()
    {
        // this method was used to detect errors of missing audio manager more simply
        #region Editor Only
#if UNITY_EDITOR
        if (AudioManager._instance == null)
        {
            Debug.LogError("there is no audio manager in the scene, or the execution order" +
                "of the Audio manager is lesser than the AudioController's order" +
                "it's recommended to move the Audio Manager Execution order to the top.");
            return;
        }
#endif
        #endregion

        _PlayAudio();
    }
    public void _PlayAudio()
    {
        #region Editor Only
#if UNITY_EDITOR
        if (AudioManager._instance == null)
        {
            Debug.LogError("there is no audio manager in the scene, or the execution order" +
                "of the Audio manager is lesser than the caller of this method, " +
                "it's recommended to move the Audio Manager Execution order to the top.");
            return;
        }
#endif
        #endregion

        if (_useSavedAudio)
            AudioManager._instance._PlayAudio(_audioType, _audioName, _playOneShot, _volume);
        else
            AudioManager._instance._PlayAudio(_audioType, _audio, _playOneShot, _volume);
    }
    public void _StopAudio()
    {

    }
}
