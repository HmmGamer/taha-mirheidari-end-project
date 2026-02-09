using System.Collections.Generic;
using UnityEngine;

namespace TahaGlobal.ML
{
    public static class MLHashManager
    {
        private static Dictionary<string, int> _textHashCache = new Dictionary<string, int>(1024);

        private static Dictionary<_AllLanguages, Dictionary<int, string>> _textDictionaryCache =
            new Dictionary<_AllLanguages, Dictionary<int, string>>();

        private static Dictionary<_AllLanguages, Dictionary<int, Sprite>> _spriteDictionaryCache =
            new Dictionary<_AllLanguages, Dictionary<int, Sprite>>();

        /// <summary>
        /// to avoid problems when the DB is changed in play mode or via editor tools
        /// </summary>
        public static void _ClearCaches()
        {
            _textDictionaryCache.Clear();
            _spriteDictionaryCache.Clear();
        }


        #region System Hashing
        public static int _GetKeyHash(string iText)
        {
            if (string.IsNullOrEmpty(iText))
                return 0;

            if (_textHashCache.TryGetValue(iText, out int hash))
                return hash;

            hash = Animator.StringToHash(iText);
            _textHashCache[iText] = hash;
            return hash;
        }

        public static Dictionary<int, string> _BuildTextHashedDictionary(MLData iData, _AllLanguages iLanguage)
        {
            if (_textDictionaryCache.TryGetValue(iLanguage, out var cachedDict))
                return cachedDict;

            int recordCount = iData._GetAllTextRecordsReference()?.Count ?? 0;
            var dict = new Dictionary<int, string>(recordCount);

            var records = iData._GetAllTextRecordsReference();

            for (int i = 0; i < recordCount; i++)
            {
                var record = records[i];
                string recordKey = record._keyId;

                if (string.IsNullOrEmpty(recordKey))
                    continue;

                int hash = _GetKeyHash(recordKey);

                string translated = record._GetTextForLanguage(iLanguage);
                if (string.IsNullOrEmpty(translated))
                    translated = recordKey;

                if (!dict.ContainsKey(hash))
                    dict.Add(hash, translated);
            }

            _textDictionaryCache[iLanguage] = dict;
            return dict;
        }
        #endregion

        #region Sprite Hashing
        public static Dictionary<int, Sprite> _BuildSpriteHashedDictionary(MLData iData, _AllLanguages iLanguage)
        {
            if (_spriteDictionaryCache.TryGetValue(iLanguage, out var cachedDict))
                return cachedDict;

            int recordCount = iData._GetAllSpriteRecordsReference()?.Count ?? 0;
            var dict = new Dictionary<int, Sprite>(recordCount);

            var records = iData._GetAllSpriteRecordsReference();

            for (int i = 0; i < recordCount; i++)
            {
                var record = records[i];
                if (record == null)
                    continue;

                string recordKey = record._keyId;
                if (string.IsNullOrEmpty(recordKey))
                    continue;

                int hash = _GetKeyHash(recordKey);

                Sprite translatedSprite = record._GetSpriteForLanguage(iLanguage);
                if (translatedSprite == null)
                    continue;

                if (!dict.ContainsKey(hash))
                    dict.Add(hash, translatedSprite);
            }

            _spriteDictionaryCache[iLanguage] = dict;
            return dict;
        }
        #endregion
    }
}
