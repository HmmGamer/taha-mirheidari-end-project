using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using static TahaGlobal.ML.MLData;

namespace TahaGlobal.ML
{
    // <summary>
    // TODO: add CSV file import/export
    // 
    // The MLManager (Multi-Language Manager) is the core translation system that handles
    // text translation, font management, and runtime language switching for both UI Text 
    // and TMP components.
    //
    // This system uses a hybrid structure combining readability and performance:
    // - Each language stores text data in a simple list of entries.
    // - Internally, all texts are hashed for better performance.
    //
    // Main Responsibilities:
    // 1. Load the correct language dataset from MLData.
    // 2. Manage a cached dictionary of hashed keys for fast text lookups.
    // 3. Provide automatic text updates for all registered MLController components.
    // 4. Handle font and TMP_FontAsset switching between languages.
    // 5. Support both static and dynamic text updates(formatted and runtime).
    //
    // Key Features:
    // - Lightweight and editor-friendly data structure.
    // - Supports any number of languages via MLData asset.
    // - logs missing translations (optional).
    // - search system (inside editor)
    // - Automatically updates all visible text elements when language changes.
    //
    // Integration Notes:
    // - Select a language (like English) as the main language and only use that.
    // - Attach an MLController to any Text or TMP_Text component to make it localizable.
    // - In your settings, Use _ChangeLanguage() to change the active language.
    // - Whenever you wanted translation, use _GetText() to translate the text.
    //
    // Important Notes:
    // its strongly recommended to add this system after the main language texts are finalized,
    // otherwise you may need to clean the database as some texts were removed or changed.
    // 
    // </summary>
    public class MLManager : Singleton_Abs<MLManager>
    {
        #region Editor Only
#if UNITY_EDITOR
        [SerializeField] bool _showWarnings = true;
#endif
        #endregion

        public static UnityAction _onLanguageChanged;

        [SerializeField] MLData _dataBase;

        [Tooltip("true => the object will be set as dont destroy on load")]
        [SerializeField] bool _isDontDestroyOnLoad;

        [Tooltip("true => the language is saved in disk when it changes")]
        [SerializeField] bool _autoSaveLastLanguage = true;

        [Tooltip("this is the default language you have at the start of the project, "
            + "this will become invalid when a change in language is saved")]
        [SerializeField] _AllLanguages _defaultStartingLanguage;

        [Tooltip("true => always start with the default language in the Unity's" +
            " editor, the language can still be changed in the play mode")]
        [SerializeField] bool _forceDefaultInEditor = true;

        [SerializeField, ReadOnly] private _AllLanguages _currentLanguage;
        private _FontMeta _currentFontMeta;
        private Dictionary<int, string> _text_HashDirectory;
        private Dictionary<int, Sprite> _sprite_HashDirectory;

        protected override void Awake()
        {
            base.Awake();

            _LoadLastSavedLanguage();
            _LoadLanguageInfo(_currentLanguage);

            #region Unity Editor Error Detection
#if UNITY_EDITOR
            if (_showWarnings)
                _dataBase._CheckForMissingTranslation();
#endif
            #endregion
        }

        private void _LoadLanguageInfo(_AllLanguages iLanguage)
        {
            _currentLanguage = iLanguage;
            _currentFontMeta = _dataBase._GetLanguageMeta(_currentLanguage);

            _text_HashDirectory = MLHashManager._BuildTextHashedDictionary(_dataBase, _currentLanguage);
            _sprite_HashDirectory = MLHashManager._BuildSpriteHashedDictionary(_dataBase, _currentLanguage);
        }
        public void _ChangeLanguage(_AllLanguages iLanguage)
        {
            if (!_IsLanguageChanged(iLanguage))
                return;

            _LoadLanguageInfo(iLanguage);
            _onLanguageChanged?.Invoke();

            if (_autoSaveLastLanguage)
                _SaveCurrentLanguage();
        }

        #region Text Getters Methods
        public string _GetTranslatedText(string iKey)
        {
            int hash = MLHashManager._GetKeyHash(iKey);

            if (_text_HashDirectory.TryGetValue(hash, out string localized))
                return localized;

            Debug.Log("translation for <" + iKey + "> was not found in the DB");
            return iKey;
        }
        public string _GetTranslatedText(string iKey, params object[] iArgs)
        {
            // reminder: we are using _GetTranslatedText()

            string template = _GetTranslatedText(iKey);
            if (iArgs == null || iArgs.Length == 0)
                return template;

            try
            {
                return string.Format(template, iArgs);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(
                "ML FORMAT ERROR\n" +
                "Key: " + iKey + "\n" +
                "Template: " + template + "\n" +
                "Args Count: " + iArgs.Length + "\n" +
                ex
            );
                return template;
            }
        }
        public Font _GetTranslatedFont()
        {
            return _currentFontMeta != null ? _currentFontMeta._font : null;
        }
        public TMP_FontAsset _GetTranslatedFontAsset()
        {
            return _currentFontMeta != null ? _currentFontMeta._fontAsset : null;
        }
        #endregion

        #region Sprite Getter Methods
        public Sprite _GetTranslatedSprite(string iKey)
        {
            int hash = MLHashManager._GetKeyHash(iKey);

            if (_sprite_HashDirectory.TryGetValue(hash, out Sprite sprite))
                return sprite;

            Debug.Log("Sprite translation for <" + iKey + "> was not found in the DB");
            return null;
        }
        #endregion

        #region Save\Load
        private void _LoadLastSavedLanguage()
        {
            if (_forceDefaultInEditor)
            {
                _currentLanguage = _defaultStartingLanguage;
                return;
            }

            int languageIndex = PlayerPrefs.GetInt(A.DataKey.lastLanguage, -1);

            if (languageIndex == -1)
                _currentLanguage = _defaultStartingLanguage;
            else
                _currentLanguage = (_AllLanguages)languageIndex;
        }
        private void _SaveCurrentLanguage()
        {
            PlayerPrefs.SetInt(A.DataKey.lastLanguage, (int)_currentLanguage);
        }
        #endregion

        #region General Getters
        public bool _IsLanguageChanged(_AllLanguages iLanguage)
        {
            return _currentLanguage != iLanguage;
        }
        public _AllLanguages _GetCurrentLanguage()
        {
            return _currentLanguage;
        }
        #endregion

        #region Editor Only
#if UNITY_EDITOR

        [CreateMonoButton("Swap Language")]
        public void _SwapLanguageTesting()
        {
            _AllLanguages iLanguage;
            if ((int)_currentLanguage != 0)
                iLanguage = _AllLanguages.English;
            else
                iLanguage = _AllLanguages.Persian;

            _LoadLanguageInfo(iLanguage);
            _onLanguageChanged?.Invoke();
        }
        public void _AddOrReplaceTextRecordToDb(_MLTextRecord iNewRecord)
        {
            if (_dataBase == null)
            {
                Debug.Log("the the MLManager's database is null, adding failed", gameObject);
                return;
            }

            // Check if the search name already exists in the DB so we can update it
            var existingRecords = _dataBase._GetAllTextRecordsReference();
            for (int i = 0; i < existingRecords.Count; i++)
            {
                // null check
                if (string.IsNullOrEmpty(existingRecords[i]._keyId))
                {
                    Debug.LogError("the <index = " + i + "> of the database is null");
                    continue;
                }

                if (existingRecords[i]._keyId == iNewRecord._keyId)
                {
                    // we cant use direct reference
                    existingRecords[i] = new _MLTextRecord(iNewRecord);

                    EditorUtility.SetDirty(_dataBase);
                    AssetDatabase.SaveAssets();

                    Debug.Log("successes, The Record with <key =" + iNewRecord._keyId +
                        ">was updated in the DB");
                    return;
                }
            }

            // does not exists, add the new record
            existingRecords.Add(new _MLTextRecord(iNewRecord));

            EditorUtility.SetDirty(_dataBase);
            AssetDatabase.SaveAssets();

            Debug.Log("successes, The Record with <key =" + iNewRecord._keyId +
                ">was added to the DB");
        }
        public void _AddOrReplaceSpriteRecordToDb(_MLSpriteRecord iNewRecord)
        {
            if (_dataBase == null)
            {
                Debug.Log("the the MLManager's database is null, adding failed", gameObject);
                return;
            }

            // Check if the search name already exists in the DB so we can update it
            var existingRecords = _dataBase._GetAllSpriteRecordsReference();
            for (int i = 0; i < existingRecords.Count; i++)
            {
                // null check
                if (string.IsNullOrEmpty(existingRecords[i]._keyId))
                {
                    Debug.LogError("the <index = " + i + "> of the database is null");
                    continue;
                }

                if (existingRecords[i]._keyId == iNewRecord._keyId)
                {
                    // we cant use direct reference
                    existingRecords[i] = new _MLSpriteRecord(iNewRecord);

                    EditorUtility.SetDirty(_dataBase);
                    AssetDatabase.SaveAssets();

                    Debug.Log("successes, The Record with <key =" + iNewRecord._keyId +
                        ">was updated in the DB");
                    return;
                }
            }

            // does not exists, add the new record
            existingRecords.Add(new _MLSpriteRecord(iNewRecord));

            EditorUtility.SetDirty(_dataBase);
            AssetDatabase.SaveAssets();

            Debug.Log("successes, The Record with <key =" + iNewRecord._keyId +
                ">was added to the DB");
        }
        public _MLTextRecord _GetTextRecordFromDB(string iKey)
        {
            foreach (_MLTextRecord item in _dataBase._GetAllTextRecordsReference())
            {
                if (item._keyId == iKey)
                {
                    // we cant use the direct reference
                    return new _MLTextRecord(item);
                }
            }

            return null;
        }
        public _MLSpriteRecord _GetSpriteRecordFromDB(string iKey)
        {
            foreach (var item in _dataBase._GetAllSpriteRecordsReference())
                if (item._keyId == iKey)
                    return item;

            return null;
        }
#endif
        #endregion
    }

    /// <summary>
    /// Warning: dont change the index number in the enum, let it index it automatically
    /// 
    /// the first language (index 0) is the primary key, you can change it at will
    /// </summary>
    public enum _AllLanguages
    {
        English, Persian
    }
}