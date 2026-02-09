using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace TahaGlobal.MsgBox
{
    public class ConfirmationController : MonoBehaviour
    {
        [Header("Attachments")]
        [SerializeField] Button _confirmButton;
        [SerializeField] TMP_Text _title;
        [SerializeField] TMP_Text _description;

        CanvasGroup _canvasGroup;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        public void _OpenMenu(string iTitle, string iDescription, UnityAction iYesActions)
        {
            _ActivateMenu(true);

            _title.text = iTitle;
            _description.text = iDescription;

            _confirmButton.onClick.RemoveAllListeners();

            _confirmButton.onClick.AddListener(iYesActions);
            _confirmButton.onClick.AddListener(_CloseMenu);
        }
        public void _CloseMenu()
        {
            _ActivateMenu(false);
        }
        private void _ActivateMenu(bool iActivation)
        {
            _canvasGroup.blocksRaycasts = iActivation;
            _canvasGroup.alpha = iActivation ? 1 : 0;
            _canvasGroup.interactable = iActivation;
        }
        public bool _IsActive()
        {
            return _canvasGroup.alpha != 0;
        }
    }
}