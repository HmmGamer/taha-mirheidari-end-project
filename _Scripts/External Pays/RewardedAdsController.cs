using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TahaGlobal.MsgBox;

public class RewardedAdsController : MonoBehaviour
{
    [Header("Function Settings")]
    [SerializeField] _RewardTypes _rewardType;
    [SerializeField] AdiveryManager._AdTypes _adType;
    [SerializeField] bool _showMsgBox = false;
    [SerializeField] bool _hasCoolDown = false;
    [Tooltip("True => check if the ads are loaded on each enable")]
    [SerializeField] bool _adLoadedCheckOnEnable;

    [Header("Button Settings")]
    [SerializeField] Button _button;
    [SerializeField] Sprite _opt_outOfAdsSprite;

    [Header("Attachments")]
    [SerializeField, ConditionalField(nameof(_showMsgBox))]
    MsgBoxController _msgBoxController;

    [SerializeField, ConditionalField(nameof(_hasCoolDown))] _TimerNames _timerName;
    [SerializeField, ConditionalField(nameof(_hasCoolDown))] float _adCoolDown;
    [SerializeField, ConditionalField(nameof(_hasCoolDown))] Text _adTimerText;
    [SerializeField, ConditionalField(nameof(_hasCoolDown))] GameObject _adTimerPanel;

    [Header("Events")]
    [SerializeField] bool _showEvents;
    [SerializeField, ConditionalField(nameof(_showEvents))] UnityEvent _afterWatchEvent;
    [SerializeField, ConditionalField(nameof(_showEvents))] UnityEvent _onAdCanceledEvent;
    [SerializeField, ConditionalField(nameof(_showEvents))] UnityEvent _onCooldownStart;
    [SerializeField, ConditionalField(nameof(_showEvents))] UnityEvent _onCooldownFinish;
    [SerializeField, ConditionalField(nameof(_showEvents))] UnityEvent _onOutOfAdsEvent;

    private Coroutine _updateUiRoutine;
    Sprite _originalSprite;
    Image _image;

    private void Start()
    {
        _image = _button.GetComponent<Image>();
        _originalSprite = _image.sprite;

        #region Error Detection
        if (!_image)
        {
            Debug.LogError("the RewardedAdsController's button needs to have an image", _button);
            return;
        }
        #endregion

        _CheckAdCoolDown();

        _afterWatchEvent.AddListener(_GetRewardAction());
        if (_showMsgBox)
            _afterWatchEvent.AddListener(() => _msgBoxController._StartMsg());

        _button.onClick.AddListener(() => AdiveryManager._instance
            ._ShowRewardedAd(_adType, _afterWatchEvent, _onAdCanceledEvent));
    }
    private void OnEnable()
    {
        if (_adLoadedCheckOnEnable)
            _CheckAdsLoaded();
    }
    private void OnDisable()
    {
        if (_updateUiRoutine != null)
            StopCoroutine(_updateUiRoutine);
    }
    private void _CheckAdsLoaded()
    {
        if (AdiveryManager._instance._IsRewardedAdLoaded())
        {
            _button.interactable = true;
            if (_originalSprite)
                _image.sprite = _originalSprite;
        }
        else
        {
            _button.interactable = false;
            if (_opt_outOfAdsSprite)
                _image.sprite = _opt_outOfAdsSprite;
            _onOutOfAdsEvent.Invoke();
        }

    }
    private void _CheckAdCoolDown()
    {
        if (!_hasCoolDown) return;

        if (TimeManager._instance._GetTimerRemainingSec(_timerName) > 0)
            _ActivateAdCooldownPanel(true);
    }
    private void _StartAdCooldown()
    {
        TimeManager._instance._SetNewTimer(_timerName, _adCoolDown, _TimerType.Global);
        _CheckAdCoolDown();
    }
    private void _ActivateAdCooldownPanel(bool iActivation)
    {
        if (iActivation)
        {
            _updateUiRoutine = StartCoroutine(_UpdateUiEachSec());
            _onCooldownStart.Invoke();
        }
        else
        {
            if (_updateUiRoutine != null)
            {
                StopCoroutine(_updateUiRoutine);
                _updateUiRoutine = null;
            }
            _onCooldownFinish.Invoke();
        }

        _adTimerPanel.SetActive(iActivation);

        _button.interactable = !iActivation;
    }
    private UnityAction _GetRewardAction()
    {
        if (_rewardType == _RewardTypes.revive)
            return () => ReviveManager._instance._ReviveAfterLose(true);

        if (_rewardType == _RewardTypes.coin)
        {
            UnityEvent iEvent = new UnityEvent();
            iEvent.AddListener(() => ShopManager._instance._RewardCoinForWatchingAds());
            iEvent.AddListener(() => _StartAdCooldown());

            return iEvent.Invoke;
        }
        return null;
    }
    private IEnumerator _UpdateUiEachSec()
    {
        while (TimeManager._instance._GetTimerRemainingSec(_timerName) > 0)
        {
            _adTimerText.text = TimeManager._instance._GetStringTimerText(_timerName);
            yield return new WaitForSeconds(1);
        }
        _ActivateAdCooldownPanel(false);
    }

    public enum _RewardTypes
    {
        revive, coin
    }
}