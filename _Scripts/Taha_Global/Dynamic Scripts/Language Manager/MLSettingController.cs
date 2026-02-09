using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TahaGlobal.ML
{
    public class MLSettingController : MonoBehaviour
    {
        [SerializeField] _LanguageButtons[] _settingsData;

        private void OnEnable()
        {
            MLManager._onLanguageChanged += _RefreshButtonsUI;
        }
        private void OnDisable()
        {
            MLManager._onLanguageChanged -= _RefreshButtonsUI;
        }
        private void Start()
        {
            _InitButtonEvents();
            _RefreshButtonsUI();

            #region Editor Only Error Check
#if UNITY_EDITOR
            _DetectErrors();
#endif
            #endregion
        }

        private void _InitButtonEvents()
        {
            for (int i = 0; i < _settingsData.Length; i++)
            {
                int index = i;

                _settingsData[index]._button.onClick.AddListener(() =>
                {
                    _OnLanguageButtonPressed(index);
                });
            }
        }

        private void _OnLanguageButtonPressed(int iIndex)
        {
            _LanguageButtons data = _settingsData[iIndex];

            if (data._isActiveLanguage)
                return;

            MLManager._instance._ChangeLanguage(data._language);
            data._onSelectLanguage?.Invoke();
        }
        private void _RefreshButtonsUI()
        {
            _AllLanguages current = MLManager._instance._GetCurrentLanguage();

            for (int i = 0; i < _settingsData.Length; i++)
            {
                _LanguageButtons data = _settingsData[i];

                bool isActive = data._language == current;

                // Only react if state actually changed (to invoke the select on start)
                if (data._isActiveLanguage != isActive)
                {
                    data._isActiveLanguage = isActive;

                    if (isActive)
                        data._onSelectLanguage?.Invoke();
                }

                data._button.interactable = !isActive;
            }
        }

        #region Editor Only Error Check
#if UNITY_EDITOR
        public void _DetectErrors()
        {
            if (_settingsData == null || _settingsData.Length == 0)
            {
                Debug.LogError($"{nameof(MLSettingController)}: Settings data is empty.", this);
                return;
            }

            int enumCount = System.Enum.GetValues(typeof(_AllLanguages)).Length;

            if (_settingsData.Length != enumCount)
            {
                Debug.LogError(
                    $"{nameof(MLSettingController)}: Settings data count ({_settingsData.Length}) " +
                    $"does not match the enum count ({enumCount}).",
                    this);
            }

            HashSet<_AllLanguages> usedLanguages = new HashSet<_AllLanguages>();

            for (int i = 0; i < _settingsData.Length; i++)
            {
                _LanguageButtons data = _settingsData[i];

                if (data._button == null)
                {
                    Debug.LogError(
                        $"{nameof(MLSettingController)}: Button is NULL at index {i} ({data._language}).",
                        this);
                }

                if (!usedLanguages.Add(data._language))
                {
                    Debug.LogError(
                        $"{nameof(MLSettingController)}: Duplicate language detected: {data._language}.",
                        this);
                }
            }
        }
#endif
        #endregion

        [System.Serializable]
        public class _LanguageButtons
        {
            public _AllLanguages _language;
            public Button _button;
            public UnityEvent _onSelectLanguage;
            [ReadOnly] public bool _isActiveLanguage;
        }
    }
}