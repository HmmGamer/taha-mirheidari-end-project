using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TahaGlobal.MsgBox;
using TMPro;

public class FeedBackManager : MonoBehaviour
{
    const int _minLength = 10;
    const int _maxLength = 300;

    const float _feedbackCooldownSec = 300f;
    const string _downloadPlatform = "Myket";
    const string _invalidLengthTitle = "The Feedback Needs To Be Between {0} To {1} Characters";

    const string _formPostUrl =
        "https://docs.google.com/forms/d/e/1FAIpQLScjHeWBeNW75eTEACcxTTFPm_fU4OzPGtDE3up8O_H79jloyQ/formResponse";

    [SerializeField] TMP_Text _Fa_InputText;
    [SerializeField] TMP_Text _En_InputText;

    [SerializeField] MsgBoxController _thanksMsgBox;
    [SerializeField] MsgBoxController _wentWrongMsgBox;
    [SerializeField] MsgBoxController _tooManyReqMsgBox;

    #region Editor Only
#if UNITY_EDITOR
    [CreateMonoButton("Send Random Feedback")]
    public void _SendRandomFeedBack()
    {
        _Fa_InputText.text = "random feedback" + Random.Range(0, 1000);
        _SendFeedBack();
    }
#endif
    #endregion

    public void _SendFeedBack()
    {
        if (!_CanSendFeedback())
        {
            _tooManyReqMsgBox._StartMsg();
            return;
        }

        string text = _GetText();

        if (text == null || text.Length < _minLength || text.Length > _maxLength)
        {
            _SendInvalidLengthMsgBox();
            return;
        }
            
        _StartFeedbackCooldown();
        StartCoroutine(_PostFeedback(text));
    }

    IEnumerator _PostFeedback(string iMsg)
    {
        WWWForm form = new WWWForm();
        form.AddField("entry.807829183", iMsg);
        form.AddField("entry.478378704", Application.version);
        form.AddField("entry.1267767505", _downloadPlatform);

        using (UnityWebRequest request =
            UnityWebRequest.Post(_formPostUrl, form))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Feedback send failed: " + request.error);
                _wentWrongMsgBox._StartMsg();
            }
            else
            {
                _thanksMsgBox._StartMsg();
            }
        }
    }
    private string _GetText()
    {
        if (!string.IsNullOrEmpty(_Fa_InputText.text))
            return _Fa_InputText.text;
        if (!string.IsNullOrEmpty(_En_InputText.text))
            return _En_InputText.text;
        return null;
    }
    private bool _CanSendFeedback()
    {
        return TimeManager._instance._GetTimerStatus(
            _TimerNames.G_FeedbackTimer) != _TimerStatus.InProgress;
    }
    private void _StartFeedbackCooldown()
    {
        TimeManager._instance._SetNewTimer(
            _TimerNames.G_FeedbackTimer,
            _feedbackCooldownSec,
            _TimerType.Global);
    }
    private void _SendInvalidLengthMsgBox()
    {
        string msg = string.Format(_invalidLengthTitle, _minLength, _maxLength); 
        MsgBoxManager._instance._ShowNotificationMessage(msg);
    }
}
