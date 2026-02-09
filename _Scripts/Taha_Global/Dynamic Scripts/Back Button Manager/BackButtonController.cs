using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TahaGlobal.MsgBox;

namespace TahaGlobal.BackButton
{
    /// <summary>
    /// TODO: change the name of _optionalEvent to OnDisableEvent
    /// </summary>
    public class BackButtonController : MonoBehaviour
    {
        const int _MAX_CANVAS_PRIORITY = 9999;

        [Tooltip("false => you can't click out of the current menu Ui buttons. " +
            "Warning: dont activate this for always active menus")]
        [SerializeField] bool _blockOutsideMenuClicks = false;

        [Tooltip("true => if you click out of the current menu, the menu will close")]
        [SerializeField, ConditionalField(nameof(_blockOutsideMenuClicks))]
        bool _exitOnOutsideClick;

        [Tooltip("true => overwrite the original bg color to an optional color")]
        [SerializeField, ConditionalField(nameof(_blockOutsideMenuClicks))]
        bool _overwriteBgColor = false;

        [SerializeField, ConditionalField(nameof(_overwriteBgColor))]
        Color _bgColor;

        [Tooltip("if more than one menu with BB controller is active, " +
            "the one with the most priority will be disabled first")]
        [SerializeField] int _priorityOrder;

        [SerializeField] UnityEvent _onEnableEvent;
        [SerializeField] UnityEvent _onDisableEvent;

        private Canvas _parentCanvas;
        private int _canvasOriginalPriority;

        private void Awake()
        {
            _InitParentCanvas();
        }
        private void OnEnable()
        {
            #region Editor Only
#if UNITY_EDITOR
            if (BackButtonManager._instance == null)
            {
                Debug.LogError("The BB manager needs to be higher than the BB Controller" +
                    " in the execution order");
                return;
            }
#endif
            #endregion

            if (_blockOutsideMenuClicks)
                _ChangeCanvasPriorityToMax();

            if (_overwriteBgColor && _blockOutsideMenuClicks) // overwrite color
                BackButtonManager._instance._RegisterPanel(gameObject, this, _priorityOrder, _onDisableEvent, _bgColor);
            else // register with default Bg color
                BackButtonManager._instance._RegisterPanel(gameObject, this, _priorityOrder, _onDisableEvent);

            _onEnableEvent.Invoke();
        }
        private void OnDisable()
        {
            if (_blockOutsideMenuClicks)
                _ChangeCanvasPriorityToDefault();

            // we check null to avoid possible errors on Scene change and application exit
            if (BackButtonManager._instance != null)
            {
                BackButtonManager._instance._UnRegisterPanel(gameObject);
            }

            _onDisableEvent.Invoke();
        }

        #region Canvas Methdos
        private void _InitParentCanvas()
        {
            Transform current = transform;
            Canvas foundCanvas = null;
            while (current != null)
            {
                foundCanvas = current.GetComponent<Canvas>();
                if (foundCanvas != null)
                {
                    break;
                }
                current = current.parent;
            }
            if (foundCanvas == null)
            {
                MsgBoxController msgBox;
                msgBox = GetComponent<MsgBoxController>();

                if (msgBox != null)
                {
                    if (MsgBoxManager._instance == null)
                    {
                        Debug.LogError("The MsgBox manager needs to be higher than the " +
                            "BB Controller in the execution order");
                        return;
                    }

                    foundCanvas = MsgBoxManager._instance._msgBoxCanvas;
                }
                else
                    Debug.LogError("No Canvas found in the parent hierarchy.");
            }
            if (foundCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogError("Canvas render mode must be ScreenSpaceOverlay.");
            }

            _parentCanvas = foundCanvas;
            _canvasOriginalPriority = _parentCanvas.sortingOrder;

            if (_priorityOrder == 0)
                _priorityOrder = _parentCanvas.sortingOrder;
        }

        /// <summary>
        /// if the panel needs to block raycast, this method changes the priority to top 
        /// so the other panels will be blocked even if they have more canvas priority
        /// </summary>
        private void _ChangeCanvasPriorityToMax()
        {
            _parentCanvas.sortingOrder = _MAX_CANVAS_PRIORITY;
        }
        private void _ChangeCanvasPriorityToDefault()
        {
            _parentCanvas.sortingOrder = _canvasOriginalPriority;
        }
        #endregion

        #region Getters
        public bool _GetIsBlockOutsideClick()
        {
            return _blockOutsideMenuClicks;
        }
        public bool _GetIsExitOnOutsideClick()
        {
            return _exitOnOutsideClick;
        }
        public int _GetCanvasPriority()
        {
            return _parentCanvas.sortingOrder - 1;
        }
        #endregion
    }
}
