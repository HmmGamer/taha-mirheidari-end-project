using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// a simple and practical script to keep track of in game events.
/// 
/// useful for invoking events in animations, timelines and keeping the editor clean.
/// </summary>
public class EventController : MonoBehaviour
{
    #region Editor Only - visual field
#if UNITY_EDITOR
    [Tooltip("this is an editor only description for more development clarity")]
    [SerializeField, TextArea(1, 2)] string _jobDescription;
#endif
    #endregion

    [Header("Function Settings")]
    [Tooltip("this is an event manually called from another script/editorEvent")]
    [SerializeField] bool _hasManualEvent;
    [SerializeField] bool _invokeOnEnable;
    [SerializeField] bool _invokeOnDisable;

    [SerializeField, ConditionalField(nameof(_hasManualEvent))] UnityEvent _manualEvent;
    [SerializeField, ConditionalField(nameof(_invokeOnEnable))] UnityEvent _onEnableEvent;
    [SerializeField, ConditionalField(nameof(_invokeOnDisable))] UnityEvent _onDisableEvent;

    public void _InvokeManualEvent()
    {
        if (_hasManualEvent)
            _manualEvent.Invoke();
    }
    public void OnEnable()
    {
        if (_invokeOnEnable)
            _onEnableEvent.Invoke();
    }
    public void OnDisable()
    {
        if (_invokeOnDisable)
            _onDisableEvent.Invoke();
    }
}
