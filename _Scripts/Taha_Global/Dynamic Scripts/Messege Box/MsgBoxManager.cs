using System.Collections;
using System.Collections.Generic;
using TahaGlobal.ML;
using UnityEngine;
using UnityEngine.Events;
using static TahaGlobal.MsgBox.MsgBoxController;

namespace TahaGlobal.MsgBox
{
    public class MsgBoxManager : Singleton_Abs<MsgBoxManager>
    {
        [SerializeField] YesNoPanelController _yesNoController;
        [SerializeField] NotificationController _NotificationController;
        [SerializeField] ConfirmationController _confirmationController;
        public Canvas _msgBoxCanvas;

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(transform.root);
        }

        #region Show Msg
        public void _ShowYesNoMessage(string iTitle, string iDescription, UnityAction iYesActions)
        {
            _yesNoController._OpenMenu(iTitle, iDescription, iYesActions);
        }
        public void _ShowConfirmationMessage(string iTitle, string iDescription, UnityAction iYesActions)
        {
            _confirmationController._OpenMenu(iTitle, iDescription, iYesActions);
        }
        public void _ShowNotificationMessage(string iTitle)
        {
            _NotificationController._ShowNotification(iTitle);
        }
        #endregion

        #region Cancel Msg
        public void _CancelYesNoMessage()
        {
            _yesNoController._CloseMenu();
        }
        public void _CancelConfirmationMessage()
        {
            _confirmationController._CloseMenu();
        }
        #endregion

        #region Others

        /// <summary>
        /// notifications are not included
        /// </summary>
        public bool _IsMsgBoxActive(_AllMsgTypes iType)
        {
            if (iType == _AllMsgTypes.yesNo)
                return _yesNoController._IsActive();
            else if (iType == _AllMsgTypes.confirmation)
                return _confirmationController._IsActive();
            return false;
        }
        public bool _IsAnyMsgBoxActive()
        {
            return _yesNoController._IsActive() || _yesNoController._IsActive();
        }
        #endregion
    }
}

#region Types
public enum _AllMsgTypes
{
    notification, yesNo, confirmation
}
#endregion