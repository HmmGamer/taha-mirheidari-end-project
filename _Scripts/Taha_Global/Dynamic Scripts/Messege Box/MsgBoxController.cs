using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TahaGlobal.MsgBox
{
    public class MsgBoxController : MonoBehaviour
    {
        [SerializeField] _AllMsgTypes _messageType;

        [Header("Functions")]
        [SerializeField] bool _autoAddToButtons;
        [SerializeField, ConditionalField(nameof(_autoAddToButtons))]
        Button _msgButton;

        [Header("Message Info")]
        public string _title;

        [ ConditionalEnum(nameof(_messageType), (int)_AllMsgTypes.yesNo, (int)_AllMsgTypes.confirmation), TextArea]
        public string _description;

        [SerializeField, ConditionalEnum(nameof(_messageType), (int)_AllMsgTypes.yesNo, (int)_AllMsgTypes.confirmation)]
        UnityEvent _confirmEvent;

        private void Start()
        {
            if (_autoAddToButtons)
                _msgButton.onClick.AddListener(_StartMsg);
        }

        /// <summary>
        /// with this method you can call a predesigned msgBox in the code or the editor.
        /// </summary>
        public void _StartMsg()
        {
            if (_CheckForErrors())
                return;

            UnityAction ConfirmInvoke = () => _confirmEvent.Invoke();

            if (_messageType == _AllMsgTypes.notification)
            {
                MsgBoxManager._instance._ShowNotificationMessage(_title);
            }
            else if (_messageType == _AllMsgTypes.yesNo)
            {
                MsgBoxManager._instance._ShowYesNoMessage(_title, _description, ConfirmInvoke);
            }
            else if (_messageType == _AllMsgTypes.confirmation)
            {
                MsgBoxManager._instance._ShowConfirmationMessage(_title, _description, ConfirmInvoke);
            }
        }

        #region New Msg Creation

        /// <summary>
        /// with this method you can start a msg with a one-time event
        /// you can set iAddControllerEvents to true to add this script events as well
        /// </summary>
        public void _StartNewMsg(UnityEvent iConfirmationEvent, bool iAddControllerEvents = false)
        {
            if (_CheckForErrors())
                return;

            UnityAction ConfirmInvoke;

            if (iAddControllerEvents == false)
            {
                ConfirmInvoke = () => iConfirmationEvent.Invoke();
            }
            else
            {
                ConfirmInvoke = () =>
                {
                    iConfirmationEvent.Invoke();
                    _confirmEvent.Invoke();
                };
            }

            if (_messageType == _AllMsgTypes.notification)
            {
                Debug.LogError("You cant call this method with _messageType == notification");
                return;
            }
            else if (_messageType == _AllMsgTypes.yesNo)
            {
                MsgBoxManager._instance._ShowYesNoMessage(_title, _description, ConfirmInvoke);
            }
            else if (_messageType == _AllMsgTypes.confirmation)
            {
                MsgBoxManager._instance._ShowConfirmationMessage(_title, _description, ConfirmInvoke);
            }
        }
        public void _StartNewMsg(UnityAction iConfirmationEvent, bool iAddControllerEvents = false)
        {
            if (_CheckForErrors())
                return;

            UnityAction ConfirmInvoke;

            if (iAddControllerEvents == false)
            {
                ConfirmInvoke = iConfirmationEvent;
            }
            else
            {
                ConfirmInvoke = () =>
                {
                    iConfirmationEvent.Invoke();
                    _confirmEvent.Invoke();
                };
            }

            if (_messageType == _AllMsgTypes.notification)
            {
                Debug.LogError("You cant call this method with _messageType == notification");
                return;
            }
            else if (_messageType == _AllMsgTypes.yesNo)
            {
                MsgBoxManager._instance._ShowYesNoMessage(_title, _description, ConfirmInvoke);
            }
            else if (_messageType == _AllMsgTypes.confirmation)
            {
                MsgBoxManager._instance._ShowConfirmationMessage(_title, _description, ConfirmInvoke);
            }
        }
        #endregion

        #region Event Changing

        /// <summary>
        /// with this method you can add a new action to the _confirmEvent
        /// with iRemoveOtherEvents you can overwrite the event in the unity editor
        /// </summary>
        public void _ChangeEvent(UnityAction iAction, bool iRemoveOtherEvents = false)
        {
            if (iRemoveOtherEvents) _confirmEvent.RemoveAllListeners();

            _confirmEvent.AddListener(iAction);
        }
        public void _ChangeEvent(UnityEvent iAction, bool iRemoveOtherEvents = false)
        {
            if (iRemoveOtherEvents) _confirmEvent.RemoveAllListeners();

            _confirmEvent.AddListener(() => iAction.Invoke());
        }
        #endregion

        #region Error Detection
        private bool _CheckForErrors()
        {
            #region Editor Only
#if UNITY_EDITOR
            if (MsgBoxManager._instance == null)
            {
                Debug.LogError("There is no MsgBoxManager in the scene");
                return true;
            }

            if (MsgBoxManager._instance._IsMsgBoxActive(_messageType))
            {
                //Debug.LogError("There is another open MsgBox in the scene!");
                //return true;
            }
#endif
            return false;
            #endregion
        }
        #endregion
    }
}
