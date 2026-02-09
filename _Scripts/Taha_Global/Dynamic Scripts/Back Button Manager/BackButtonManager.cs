using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.UI;
using TahaGlobal.MsgBox;
#region Using New InputSystem
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#endregion

namespace TahaGlobal.BackButton
{
    /// <summary>
    /// TODO: test the new input system, reorganize properties, add regens
    /// 
    /// you can read the user manual for more info about the BackButtonManager features
    /// </summary>
    public class BackButtonManager : Singleton_Abs<BackButtonManager>
    {
        [Header("Exit Settings")]
        [SerializeField] bool _autoGameQuit = true;
        [SerializeField, ConditionalField(nameof(_autoGameQuit))] MsgBoxController _exitMsgBox;

        [Tooltip("if true => the game is closed after 2 backButtons")]
        [SerializeField] bool _isDoubleClickExit;

        [Tooltip("minimum delay before second click can trigger exit")]
        [SerializeField, ConditionalField(nameof(_isDoubleClickExit))]
        float _doubleClickExitDelay = 0.3f;

        [Tooltip("time before the first click is expired (recommended to match it with notification time)")]
        [SerializeField, ConditionalField(nameof(_isDoubleClickExit))]
        float _clickExpireTime;

        [Header("Canvas Settings")]
        [SerializeField] Canvas _mainCanvas;
        [SerializeField] Color _raycastBgColor = Color.clear; // default value

        bool _isFirstClickActive;
        bool _isDoubleClickDelayFinished;

        List<_PanelsClass> _registeredPanels = new List<_PanelsClass>();
        Coroutine _firstClickExpireCoroutine;
        Coroutine _doubleClickDelayCoroutine;
        Image _AntiRayCasterImage;
        Button _AntiRaycastButton;

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(transform.root);
            _InitGlobalAntiRayCaster();
        }

        #region Old Input System
#if !ENABLE_INPUT_SYSTEM
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _OnBackButtonPressed();
            }
        }
#endif
        #endregion

        #region New Input System
#if ENABLE_INPUT_SYSTEM
    public void _OnBackInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _OnBackButtonPressed();
        }
    }

    public void _OnClickInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _TryHandleOutsideClick();
        }
    }
#endif
        #endregion

        private void _InitGlobalAntiRayCaster()
        {
            if (_AntiRayCasterImage == null)
            {
                GameObject obj = new GameObject("GlobalRaycastCatcher", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                obj.transform.SetParent(_mainCanvas.transform, false);

                RectTransform rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);

                _AntiRayCasterImage = obj.GetComponent<Image>();
                _AntiRayCasterImage.color = _raycastBgColor;
                _AntiRayCasterImage.raycastTarget = true;
                _AntiRaycastButton = obj.GetComponent<Button>();
                _AntiRaycastButton.onClick.AddListener(() => _TryHandleOutsideClick());
            }
            _AntiRayCasterImage.gameObject.SetActive(false);
        }
        private void _UpdateAntiRayCasterOrder()
        {
            _PanelsClass topPanel = _GetTopActivePanel();
            if (topPanel == null)
            {
                _AntiRayCasterImage.gameObject.SetActive(false);
                return;
            }

            _mainCanvas.sortingOrder = topPanel._bbController._GetCanvasPriority();
            if (topPanel._bbController._GetIsBlockOutsideClick())
            {
                _AntiRayCasterImage.gameObject.SetActive(true);
                _AntiRayCasterImage.color = topPanel._bgColor;
            }
        }
        public void _OnBackButtonPressed()
        {
            // check if a panel is open and close it first
            _PanelsClass topPanel = _GetTopActivePanel();
            if (topPanel != null)
            {
                topPanel._panel.SetActive(false);
                _UnRegisterPanel(topPanel._panel);

                return;
            }

            // no panels are open, so we handle exiting logic
            if (_isDoubleClickExit && _autoGameQuit)
            {
                // if first click already happened
                if (_isFirstClickActive)
                {
                    // if delay before second click is NOT finished, ignore (prevents accidental exit)
                    if (!_isDoubleClickDelayFinished)
                    {
                        return;
                    }

                    if (_firstClickExpireCoroutine != null)
                    {
                        StopCoroutine(_firstClickExpireCoroutine);
                    }

                    _ExitApplication();
                    return;
                }

                // first click
                _isFirstClickActive = true;
                _isDoubleClickDelayFinished = false;

                // start timers (delay + expire)
                _doubleClickDelayCoroutine = StartCoroutine(_doubleClickExitDelayCD());
                _firstClickExpireCoroutine = StartCoroutine(_ExpireFirstClick());

                _exitMsgBox._StartMsg();
            }
            else if (!_isDoubleClickExit && _autoGameQuit)
            {
                if (!MsgBoxManager._instance._IsMsgBoxActive(_AllMsgTypes.yesNo))
                    _exitMsgBox._StartMsg();
            }
        }

        public void _ExitApplication()
        {
            #region Editor Only
#if UNITY_EDITOR
            Debug.Log("you can't exit in play mode!");
#endif
            #endregion

            Application.Quit();
        }
        IEnumerator _ExpireFirstClick()
        {
            yield return new WaitForSeconds(_clickExpireTime);
            _isFirstClickActive = false;
            _firstClickExpireCoroutine = null;
            _isDoubleClickDelayFinished = false;
        }
        IEnumerator _doubleClickExitDelayCD()
        {
            yield return new WaitForSeconds(_doubleClickExitDelay);
            _isDoubleClickDelayFinished = true;
        }

        /// <summary>
        /// registering with the default anti ray caster color
        /// </summary>
        public void _RegisterPanel(GameObject iPanel, BackButtonController iController, int iOrder, UnityEvent iEvent)
        {
            _registeredPanels.Add(new _PanelsClass(iPanel, iController, iOrder, iEvent, _raycastBgColor));
            _UpdateAntiRayCasterOrder();
        }

        /// <summary>
        /// registering with the overwriting the anti ray caster color
        /// </summary>
        public void _RegisterPanel(GameObject iPanel, BackButtonController iController, int iOrder, UnityEvent iEvent, Color iBgColor)
        {
            _registeredPanels.Add(new _PanelsClass(iPanel, iController, iOrder, iEvent, iBgColor));
            _UpdateAntiRayCasterOrder();
        }
        public void _UnRegisterPanel(GameObject iPanel)
        {
            for (int i = _registeredPanels.Count - 1; i >= 0; i--)
            {
                if (_registeredPanels[i]._panel == iPanel)
                {
                    _registeredPanels.RemoveAt(i);
                    break;
                }
            }
            _UpdateAntiRayCasterOrder();
        }
        _PanelsClass _GetTopActivePanel()
        {
            if (_registeredPanels.Count == 0)
            {
                return null;
            }
            return _registeredPanels.OrderByDescending(p => p._priorityOrder).FirstOrDefault(p => p._panel.activeInHierarchy);
        }
        public void _TryHandleOutsideClick()
        {
            _PanelsClass topPanel = _GetTopActivePanel();
            if (topPanel == null)
            {
                return;
            }

            if (!topPanel._bbController._GetIsBlockOutsideClick())
            {
                return;
            }

            if (topPanel._bbController._GetIsExitOnOutsideClick())
            {
                topPanel._panel.SetActive(false);
                _UnRegisterPanel(topPanel._panel);
            }
        }

        [System.Serializable]
        public class _PanelsClass
        {
            public GameObject _panel;
            [HideInInspector] public BackButtonController _bbController;
            public int _priorityOrder;
            public UnityEvent _optionalEvent;
            public Color _bgColor; // the color of the anti ray cast (if activated)

            public _PanelsClass(GameObject iPanel, BackButtonController iController
                , int iOrder, UnityEvent iOptionalEvent, Color iBgColor)
            {
                _panel = iPanel;
                _bbController = iController;
                _priorityOrder = iOrder;
                _optionalEvent = iOptionalEvent;
                _bgColor = iBgColor;
            }
        }
    }
}