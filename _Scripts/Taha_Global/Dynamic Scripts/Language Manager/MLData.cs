using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TahaGlobal.ML
{
    [CreateAssetMenu(menuName = "Taha/Multi Language Data")]
    public class MLData : ScriptableObject
    {
        [Header("General Settings")]
        [SerializeField] List<_FontMeta> _fontSettings = new List<_FontMeta>();

        [Header("Text Translation DataBase")]
        [SerializeField] List<_MLTextRecord> _translationDb = new List<_MLTextRecord>();

        [Header("Sprite Translation DataBase")]
        [SerializeField] List<_MLSpriteRecord> _spriteDb = new List<_MLSpriteRecord>();

        public _FontMeta _GetLanguageMeta(_AllLanguages iLanguage)
        {
            for (int i = 0; i < _fontSettings.Count; i++)
                if (_fontSettings[i]._language == iLanguage)
                    return _fontSettings[i];
            return null;
        }

        public List<_MLTextRecord> _GetAllTextRecordsReference()
        {
            return _translationDb;
        }
        public List<_MLSpriteRecord> _GetAllSpriteRecordsReference()
        {
            return _spriteDb;
        }

        #region Editor Only
#if UNITY_EDITOR
        [Header("Search => Moves founded Items To The Top")]

        [SerializeField] string _searchKeyId;

        [CreateSOButton("Search Text Database")]
        private void _SearchTextDb()
        {
            SearchTools._SearchAndSortList_Full(_translationDb, _searchKeyId, i => i._keyId);
        }
        [CreateSOButton("Search Sprite Database")]
        private void _SearchSpriteDb()
        {
            SearchTools._SearchAndSortList_Full(_spriteDb, _searchKeyId, i => i._keyId);
        }

        public void _CheckForMissingTranslation()
        {
            int expectedCount = System.Enum.GetValues(typeof(_AllLanguages)).Length;

            // ---------------- TEXT CHECK ----------------
            foreach (var record in _translationDb)
            {
                if (record._translations == null || record._translations.Length != expectedCount)
                {
                    Debug.Log("The Text record <" + record._translations?[0] +
                        "> does not have correct translation count");
                    continue;
                }

                for (int i = 0; i < record._translations.Length; i++)
                {
                    if (string.IsNullOrEmpty(record._translations[i]))
                    {
                        Debug.Log("The Text record <" + record._translations[0] +
                            "> has missing text translation at index " + i);
                    }
                }
            }

            // ---------------- SPRITE CHECK ----------------
            foreach (var record in _spriteDb)
            {
                if (record._sprites == null || record._sprites.Length != expectedCount)
                {
                    Debug.Log("A sprite record does not have correct sprite count");
                    continue;
                }

                Sprite baseSprite = record._sprites[0];

                if (baseSprite == null)
                {
                    Debug.Log("A sprite record has no base sprite (index 0)");
                    continue;
                }

                string baseName = baseSprite.name;

                for (int i = 0; i < record._sprites.Length; i++)
                {
                    Sprite sprite = record._sprites[i];

                    if (sprite == null)
                    {
                        Debug.LogWarning("Sprite translation missing for <" + baseName +
                            "> at _spriteDb index " + i, this);
                        continue;
                    }
                }
            }

            // ---------------- FONTS CHECK ----------------
            int languageCount = System.Enum.GetValues(typeof(_AllLanguages)).Length;
            bool[] languageSeen = new bool[languageCount];

            for (int i = 0; i < _fontSettings.Count; i++)
            {
                var item = _fontSettings[i];
                if (item == null)
                    continue;

                int langIndex = (int)item._language;

                if (langIndex < 0 || langIndex >= languageCount)
                {
                    Debug.Log("Font meta has invalid language enum value at index " + i);
                    continue;
                }

                if (languageSeen[langIndex])
                {
                    Debug.Log("Duplicate font meta found for language <" + item._language + ">");
                    continue;
                }

                languageSeen[langIndex] = true;
            }

            for (int i = 0; i < languageSeen.Length; i++)
            {
                if (!languageSeen[i])
                {
                    Debug.Log("Missing font meta for language <" + (_AllLanguages)i + ">");
                }
            }

        }
#endif
        #endregion

        #region Classes

        [System.Serializable]
        public class _FontMeta
        {
            public _AllLanguages _language;
            public bool _isRTL;
            public Font _font;
            public TMP_FontAsset _fontAsset;
        }
        //-------------------------------
        [System.Serializable]
        public class _MLTextRecord
        {
            [Tooltip("this is the main unique key and it's used to search the list")]
            public string _keyId;

            public string[] _translations;

            public _MLTextRecord() { }
            public _MLTextRecord(_MLTextRecord iObject)
            {
                _keyId = iObject._keyId;
                _translations = (string[])iObject._translations.Clone();
            }

            public string _GetTextForLanguage(_AllLanguages iLanguageIndex)
            {
                int languageIndex = (int)iLanguageIndex;

                #region Input Validation
                if (_translations == null)
                {
                    Debug.LogError("This key is not defined in the translations Db");
                    return null;
                }
                if (languageIndex < 0 || languageIndex >= _translations.Length)
                {
                    Debug.LogError("This translation is not defined in the translations Db");
                    return null;
                }
                #endregion

                return _translations[languageIndex];
            }
        }
        //-------------------------------
        [System.Serializable]
        public class _MLSpriteRecord
        {
            [Tooltip("this is the main unique key and it's used to search the list")]
            public string _keyId;

            public Sprite[] _sprites; // index = _AllLanguages enum index

            public _MLSpriteRecord() { }
            public _MLSpriteRecord(_MLSpriteRecord iObject)
            {
                _keyId = iObject._keyId;
                _sprites = (Sprite[])iObject._sprites.Clone();
            }

            public Sprite _GetSpriteForLanguage(_AllLanguages iLanguageIndex)
            {
                int languageIndex = (int)iLanguageIndex;

                #region Input Validation
                if (_sprites == null)
                {
                    Debug.LogError("This sprites is not defined in the _sprites Db");
                    return null;
                }
                if (languageIndex < 0 || languageIndex >= _sprites.Length)
                {
                    Debug.LogError("This sprites is not defined in the _sprites Db");
                    return null;
                }
                #endregion

                return _sprites[languageIndex];
            }
        }
        #endregion
    }
}