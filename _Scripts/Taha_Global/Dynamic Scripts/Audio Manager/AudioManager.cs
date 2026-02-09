using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton_Abs<AudioManager>
{
    #region Default Audio Manager
    [Header("Default Attachments")]
    [SerializeField] AudioSource _audioChanelSFX1;
    [SerializeField] AudioSource _audioChanelSFX2;
    [SerializeField] AudioSource _audioChanelMusic;
    [SerializeField] AudioSource _audioChanelTalk;
    [SerializeField] _SavedAudios[] _allSavedAudios;

    public void _PlayAudio(_AudioType iType, AudioClip iClip, bool _isOneShot = false, float iVolume = 1)
    {
        if (iClip == null) return;

        if (iType == _AudioType.SFX1)
        {
            if (_isOneShot)
            {
                _audioChanelSFX1.PlayOneShot(iClip);
                return;
            }
            _audioChanelSFX1.clip = iClip;
            _audioChanelSFX1.volume = iVolume;

            if (iClip != null)
                _audioChanelSFX1.Play();
            else
                _audioChanelMusic.Stop();
        }
        else if (iType == _AudioType.SFX2)
        {
            if (_isOneShot)
            {
                _audioChanelSFX2.PlayOneShot(iClip);
                return;
            }
            _audioChanelSFX2.clip = iClip;
            _audioChanelSFX2.volume = iVolume;

            if (iClip != null)
                _audioChanelSFX2.Play();
            else
                _audioChanelSFX2.Stop();
        }
        else if (iType == _AudioType.Music)
        {
            if (_isOneShot)
            {
                _audioChanelMusic.PlayOneShot(iClip);
                return;
            }
            _audioChanelMusic.clip = iClip;
            _audioChanelMusic.volume = iVolume;

            if (iClip != null)
                _audioChanelMusic.Play();
            else
                _audioChanelMusic.Stop();
        }
        else if (iType == _AudioType.Talk)
        {
            if (_isOneShot)
            {
                _audioChanelTalk.PlayOneShot(iClip);
                return;
            }
            _audioChanelTalk.clip = iClip;
            _audioChanelTalk.volume = iVolume;

            if (iClip != null)
                _audioChanelMusic.Play();
            else
                _audioChanelMusic.Stop();
        }
    }
    public void _PlayAudio(_AudioType iType, SavedSounds iClipName, bool _isOneShot = false, float iVolume = 1)
    {
        for (int i = 0; i < _allSavedAudios.Length; i++)
        {
            if (_allSavedAudios[i]._audioName == iClipName.ToString())
            {
                _PlayAudio(iType, _allSavedAudios[i]._audio, _isOneShot, iVolume);
            }
        }
    }
    public void _StopGame()
    {
        _audioChanelSFX1.Stop();
        _audioChanelSFX2.Stop();
        _audioChanelTalk.Stop();
    }
    public void _ContinueGame()
    {
        if (_audioChanelSFX1.clip != null)
            _audioChanelSFX1.Play();
        if (_audioChanelSFX2.clip != null)
            _audioChanelSFX2.Play();
        if (_audioChanelTalk.clip != null)
            _audioChanelTalk.Play();
    }
    [CreateMonoButton("Make Enum")]
    public void _MakeEnum()
    {
        EnumGenerator._GenerateEnums("SavedSounds", _allSavedAudios, nameof(_SavedAudios._audioName));
        EnumGenerator._AddValueToFirst("SavedSounds", "None");
    }

    [System.Serializable]
    public class _SavedAudios
    {
        public string _audioName;
        public AudioClip _audio;
    }
    #endregion
    #region 2048 Game Extentions
    const float _SILENT_TIME = 0.3f;

    [Header("Playable Attachments")]
    [SerializeField] _AudioPerMerge[] _allMergeAudios;

    int _currentMergeCount;

    private AudioClip[] _shuffledPlaylist;
    private int _currentTrackIndex = 0;
    private Coroutine _musicPlaybackCoroutine;

    public void _PlayMusicInOrder(AudioClip[] iClip)
    {
        _shuffledPlaylist = ShuffleArray(iClip);
        _currentTrackIndex = 0;

        if (_musicPlaybackCoroutine != null)
        {
            StopCoroutine(_musicPlaybackCoroutine);
        }

        _musicPlaybackCoroutine = StartCoroutine(PlayMusicSequence());
    }
    private AudioClip[] ShuffleArray(AudioClip[] array)
    {
        AudioClip[] shuffled = new AudioClip[array.Length];
        System.Array.Copy(array, shuffled, array.Length);

        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            AudioClip temp = shuffled[i];
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        return shuffled;
    }
    private IEnumerator PlayMusicSequence()
    {
        while (true)
        {
            // Play current track
            AudioClip currentClip = _shuffledPlaylist[_currentTrackIndex];
            _audioChanelMusic.clip = currentClip;
            _audioChanelMusic.Play();

            // Wait for the exact duration of the clip + small buffer
            yield return new WaitForSeconds(currentClip.length + _SILENT_TIME);

            // Move to next track, loop back to beginning if at end
            _currentTrackIndex = (_currentTrackIndex + 1) % _shuffledPlaylist.Length;
        }
    }
    public void _AddToMergeCounter()
    {
        _currentMergeCount++;
    }
    public void _PerformMergeAudio()
    {
        if (_currentMergeCount == 0) return;

        foreach (_AudioPerMerge item in _allMergeAudios)
        {
            if (item._mergeCount == _currentMergeCount)
            {
                _currentMergeCount = 0;
                _PlayAudio(_AudioType.SFX1, item._mergeAudio, true);
                return;
            }
        }
        _PlayAudio(_AudioType.SFX1,
            _allMergeAudios[_allMergeAudios.Length - 1]._mergeAudio, true);

        _currentMergeCount = 0;
    }

    #region Types
    [System.Serializable]
    public class _AudioPerMerge
    {
        public int _mergeCount;
        public AudioClip _mergeAudio;
    }
    #endregion
    #endregion
}
#region Default Audio Manager
public enum _AudioType
{
    SFX1, SFX2, Music, Talk
}
#endregion