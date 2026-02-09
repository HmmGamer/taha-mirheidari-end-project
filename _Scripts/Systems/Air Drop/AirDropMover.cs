using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AirdropMover : Singleton_Abs<AirdropMover>
{
    [SerializeField] float _descendSpeed;
    [SerializeField] float _ascendSpeed;
    [SerializeField] float _bottomWaitDuration; // wait time in the bottom

    [Tooltip("True => Wait Until The AirMsg Panel To Be Closed")]
    [SerializeField] bool _waitForCollectClose;

    [Tooltip("Fixed Wait Time After Collection")]
    [SerializeField] float _fixedCollectedWait = 0.5f;
    [SerializeField] Transform _moverObject;
    [SerializeField] CanvasGroup _canvasGroup;

    [Header("Optional Events")]
    public UnityEvent _onDropFinished;

    bool _isCollected;
    Vector2 _startPos;
    Vector2 _endPos;
    Coroutine _routine;

    private void Start()
    {
        _canvasGroup.alpha = 0;
    }
    public void _StartSequence(Vector2 iStart, Vector2 iEnd)
    {
        _startPos = iStart;
        _endPos = iEnd;
        _isCollected = false;
        _canvasGroup.alpha = 1;
        if (_routine != null)
            StopCoroutine(_routine);
        _routine = StartCoroutine(_SequenceRoutine());
    }
    public void _SetCollectRewardBool()
    {
        _isCollected = true;
    }

    #region Coroutines
    private IEnumerator _SequenceRoutine()
    {
        yield return _Descend();

        if (!_isCollected) // skip if already (collected on descend)
            yield return _WaitAtBottom();

        yield return _Ascend();

        _onDropFinished.Invoke();
        _canvasGroup.alpha = 0;
        _routine = null;
    }
    private IEnumerator _Descend()
    {
        AudioManager._instance._PlayAudio(_AudioType.SFX2, SavedSounds.AirDrop_StartDescend);

        float iX = _startPos.x;
        _moverObject.position = _startPos;

        while (_IsAtButton())
        {
            if (_isCollected)
                yield break;

            float iNewY = Mathf.MoveTowards(_moverObject.position.y, _endPos.y, _descendSpeed * Time.deltaTime);
            _moverObject.position = new Vector2(iX, iNewY);
            yield return new WaitForEndOfFrame();
        }
    }
    private IEnumerator _WaitAtBottom()
    {
        AudioManager._instance._PlayAudio(_AudioType.SFX2, SavedSounds.AirDrop_Wait);

        float t = 0f;
        while (t < _bottomWaitDuration)
        {
            if (_isCollected)
                yield break;
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    private IEnumerator _WaitOnCollect()
    {
        if (_waitForCollectClose)
        {
            AirDropMsgBox msgBox = AirDropMsgBox._instance;

            while (msgBox._isPanelActive)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        float t = 0f;
        while (t < _fixedCollectedWait)
        {
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    private IEnumerator _Ascend()
    {
        AudioManager._instance._PlayAudio(_AudioType.SFX2, SavedSounds.AirDrop_StartAscend);

        float iX = _startPos.x;
        bool hasWaitedForCollection = false;

        while (_IsAtTop())
        {
            if (!hasWaitedForCollection && _isCollected)
            {
                hasWaitedForCollection = true;
                yield return _WaitOnCollect();
            }

            float iNewY = Mathf.MoveTowards(_moverObject.position.y, _startPos.y, _ascendSpeed * Time.deltaTime);
            _moverObject.position = new Vector2(iX, iNewY);
            yield return new WaitForEndOfFrame();
        }

        _canvasGroup.alpha = 0;
    }

    #endregion

    #region Position Calculators
    private bool _IsAtButton()
    {
        return _moverObject.position.y > _endPos.y;
    }
    private bool _IsAtTop()
    {
        return _moverObject.position.y < _startPos.y;
    }
    #endregion
}
