using UnityEngine;
using UnityEngine.UI;

namespace TahaGlobal.ML
{
    public class MLSpriteController : MonoBehaviour
    {
        [Header("Only Assign One Of Them")]
        [SerializeField] Image _uiImage;
        [SerializeField] SpriteRenderer _spriteRenderer;

        [Header("In Database Record")]
        [SerializeField] MLData._MLSpriteRecord _data;

        private _AllLanguages _currentLanguage;

        private void Awake()
        {
#if UNITY_EDITOR
            if (MLManager._instance == null)
            {
                Debug.LogError("MLManager is missing or execution order is incorrect");
                return;
            }
            if (string.IsNullOrWhiteSpace(_data._keyId))
                Debug.Log("the keyId is empty", this);
#endif
        }
        private void OnEnable()
        {
            MLManager._onLanguageChanged += _OnLanguageChange;
            if (MLManager._instance._IsLanguageChanged(_currentLanguage))
                _OnLanguageChange();
        }
        private void OnDisable()
        {
            MLManager._onLanguageChanged -= _OnLanguageChange;
        }
        private void _OnLanguageChange()
        {
            _currentLanguage = MLManager._instance._GetCurrentLanguage();
            _RefreshSprite();
        }
        private void _RefreshSprite()
        {
            Sprite sprite = MLManager._instance._GetTranslatedSprite(_data._keyId);
            if (sprite != null)
                _SetSprite(sprite);
        }

        public void _ChangeSprite(string iKey)
        {
            Sprite sprite = MLManager._instance._GetTranslatedSprite(iKey);
            if (sprite != null)
                _SetSprite(sprite);
        }

        #region Sprite Get/Set
        private void _SetSprite(Sprite iSprite)
        {
            if (_uiImage != null)
                _uiImage.sprite = iSprite;
            else if (_spriteRenderer != null)
                _spriteRenderer.sprite = iSprite;
            else
                Debug.LogError("The controller needs an Image/SpriteRenderer", gameObject);
        }
        #endregion

        #region Editor Only
#if UNITY_EDITOR

        private MLManager _MLManager;

        private bool _CheckMLManagerLoaded()
        {
            // already found, return true
            if (_MLManager != null)
                return true;

            // try finding it
            if (Application.isPlaying)
            {
                _MLManager = MLManager._instance;
            }
            else
            {
                _MLManager = GameObject.FindFirstObjectByType<MLManager>();
            }

            // not found, give the error
            if (_MLManager == null)
            {
                Debug.LogError("the MLManager is missing in the scene or " +
                    "it's execution order is lower than the MLController's order");
            }

            return _MLManager != null;
        }

        [CreateMonoButton("Try Load From DB")]
        private void _TryLoadFromDB()
        {
            if (!_CheckMLManagerLoaded()) return;

            if (string.IsNullOrEmpty(_data._keyId)) return;


            MLData._MLSpriteRecord temp =
                _MLManager._GetSpriteRecordFromDB(_data._keyId);

            if (temp != null)
            {
                _data = temp;
                Debug.Log("successes, the sprite record for <keyId = " + _data._keyId
                    + "> was loaded from the DB", this);
            }
            else
            {
                Debug.Log("failed, the sprite record with <keyId = " + _data._keyId
                    + "> was not found in the DB", this);
            }
        }

        [CreateMonoButton("Save To DB")]
        private void _TryAddToDb()
        {
            if (!_CheckMLManagerLoaded())
                return;

            _MLManager._AddOrReplaceSpriteRecordToDb(_data);
        }
#endif
        #endregion
    }
}
