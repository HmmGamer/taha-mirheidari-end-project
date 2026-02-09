using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// Handles Async scene loading and transitions with optional fade animations and progress visualization.
/// This manager persists across scenes and provides events for before-load and after-load stages.
/// 
/// - Global Behavior: The loading process runs independently of gameplay time (uses real-time delays).
///   Meaning it works even if Time.timeScale = 0.
/// - Local Behavior: Animation delays such as fade-in and fade-out rely on Unity’s internal time flow.
/// 
/// Key Features:
/// • Supports both slider-based, step-based and none progress displays.
/// • Allows smooth fade animations using Animator triggers.
/// • useful events for controlling the scene states.
/// • Can automatically load a scene on start, or be triggered manually via _LoadScene().
/// • supporting both Async and immediate (emergency) loading.
/// </summary>

public class LoadingManager : Singleton_Abs<LoadingManager>
{
    // _onNewScene is always called after all of the new scene objects are initialized
    // meaning methods => start, awake, onEnabled are all called before this event
    public static event UnityAction<_AllScenes> _onNewSceneLoaded;

    // this is called before the loading of going to the new scene
    public static event UnityAction _beforeLoadingNextScene;

    [Header("General Settings")]
    [SerializeField] bool _autoLoadNextScene;
    [SerializeField, ConditionalField(nameof(_autoLoadNextScene))] _AllScenes _nextScene;
    [SerializeField] _ProgressShowModes _progressMode;

    [Header("Animation Settings")]
    [SerializeField] bool _hasAnimation;
    [SerializeField, ConditionalField(nameof(_hasAnimation))] Animator _anim;
    [Tooltip("this delay is added to have more time in playing the fade in animation")]
    [SerializeField, ConditionalField(nameof(_hasAnimation))] float _fadeInDelay = 0.3f;
    [SerializeField, ConditionalField(nameof(_hasAnimation))] string _fadeInTrigger;
    [SerializeField, ConditionalField(nameof(_hasAnimation))] string _fadeOutTrigger;

    [Header("Attachments")]
    [SerializeField] Canvas _canvas;
    [SerializeField] Image _loadingImage;

    [Header("Progress Settings")]
    [SerializeField, ConditionalEnum(nameof(_progressMode), (int)_ProgressShowModes.Steps)]
    GameObject[] _steps;
    [SerializeField, ConditionalEnum(nameof(_progressMode), (int)_ProgressShowModes.Slider)]
    Slider _loadingSlider;

    [Header("Optional Load Settings")]
    [SerializeField] UnityEvent _beforeLoadEvent;
    [SerializeField] UnityEvent _afterLoadEvent;

    private _AllScenes _currentLoadingProgressScene;
    AsyncOperation _loadOperation;

    private void Start()
    {
        if (_autoLoadNextScene)
            _LoadScene(_nextScene);
    }
    public void _LoadScene(_AllScenes iNextScene)
    {
        _beforeLoadEvent.Invoke();
        _beforeLoadingNextScene?.Invoke();
        StartCoroutine(_LoadAsync(iNextScene));
    }

    /// <summary>
    /// this event should be called in the animation.
    /// use case: put this at the END of fade-out animation as an Animation Event
    /// reminder: you can optionally use EventController for the event calling
    /// </summary>
    public void _A_GoToNextScene()
    {
        _canvas.gameObject.SetActive(false);
    }

    /// <summary>
    ///  use with caution.
    ///  the _onNewScene event may have some problems as not all scripts are loaded
    /// </summary>
    public void _LoadSceneImmediately(_AllScenes iScene)
    {
        _beforeLoadEvent.Invoke();
        _beforeLoadingNextScene?.Invoke();
        SceneManager.LoadScene((int)iScene);
        _onNewSceneLoaded?.Invoke(iScene);
    }

    /// <summary>
    /// check if the scene is what you think.
    /// use case: you have a single code in multiple scenes and they have different behaviour based on the scene
    /// </summary>
    public bool _IsInCurrentScene(_AllScenes iCurrentScene)
    {
        if (SceneManager.GetActiveScene().buildIndex == (int)iCurrentScene)
            return true;
        return false;
    }

    private IEnumerator _LoadAsync(_AllScenes iNextScene)
    {
        // TODO => clean this method more than this.

        _canvas.gameObject.SetActive(true);
        _currentLoadingProgressScene = iNextScene;

        if (_hasAnimation && _anim)
            _anim.SetTrigger(_fadeInTrigger);

        yield return new WaitForSeconds(_fadeInDelay);

        _loadOperation = SceneManager.LoadSceneAsync((int)iNextScene);
        _loadOperation.allowSceneActivation = false;

        while (_loadOperation.progress < 0.9f)
        {
            float iProgress = Mathf.Clamp01(_loadOperation.progress / 0.9f);
            if (_progressMode == _ProgressShowModes.Steps && _steps != null && _steps.Length > 0)
            {
                float stepThreshold = 1f / _steps.Length;
                for (int i = 0; i < _steps.Length; i++)
                {
                    if (iProgress >= stepThreshold * (i + 1))
                        _steps[i].SetActive(true);
                    else
                        _steps[i].SetActive(false);
                }
            }
            else if (_progressMode == _ProgressShowModes.Slider && _loadingSlider != null)
            {
                _loadingSlider.value = iProgress;
            }
            yield return null;
        }

        if (_progressMode == _ProgressShowModes.Slider && _loadingSlider != null)
            _loadingSlider.value = 1f;
        if (_progressMode == _ProgressShowModes.Steps && _steps != null)
        {
            foreach (var step in _steps)
                step.SetActive(true);
        }

        _loadOperation.allowSceneActivation = true;

        // we add this to ensure the new scene is loaded completely
        while (!_loadOperation.isDone)
        {
            yield return new WaitForSeconds(0.05f); // ~20fps
        }

        _onNewSceneLoaded?.Invoke(_currentLoadingProgressScene);
        _afterLoadEvent.Invoke();

        if (_hasAnimation && _anim)
            _anim.SetTrigger(_fadeOutTrigger); // play fade-out animation
        else
            _canvas.gameObject.SetActive(false);
    }
    public enum _ProgressShowModes
    {
        Slider, Steps, None // None => we dont show the progress
    }
}

// save the index as in the build settings
public enum _AllScenes
{
    Lobby = 1, MainGame = 2
}
