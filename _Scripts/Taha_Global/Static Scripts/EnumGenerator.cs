using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

//class _EnumGeneratorSample : MonoBehaviour
//{
//    [SerializeField] _Test[] tests;

//    [CreateMonoButton("Make Enum")]
//    private void _MakeEnum()
//    {
//        EnumGenerator._GenerateEnums("testEnum", tests, nameof(_Test._name));
//        EnumGenerator._AddValue("testEnum", "None");
//    }

//    [System.Serializable]
//    public class _Test
//    {
//        public string _name;
//        public int _value;
//    }
//}

public static class EnumGenerator
{
    private const string _DEFAULT_FILE_PATH = "Assets/_Scripts/Taha_Global/Others/GeneratedEnums";

    public static void _GenerateEnums<T>(string iEnumName, T[] iDataArray, string iFieldName) where T : class
    {
#if UNITY_EDITOR
        if (!_CheckForErrors(iDataArray))
            return;

        string[] iNames = new string[iDataArray.Length];
        for (int i = 0; i < iDataArray.Length; i++)
        {
            var iField = iDataArray[i].GetType().GetField(iFieldName);
            if (iField == null)
            {
                Debug.LogError($"Class does not contain a field named '{iFieldName}'.");
                return;
            }
            iNames[i] = iField.GetValue(iDataArray[i]) as string;
        }

        if (!_CheckForBannedSymbols(iNames))
            return;

        Directory.CreateDirectory(_DEFAULT_FILE_PATH);
        string iCompletePath = Path.Combine(_DEFAULT_FILE_PATH, iEnumName + ".cs");

        if (_CheckForDuplicatedFile(iCompletePath))
            return;

        string iCode = "using System;\n\n[Serializable]\npublic enum " + iEnumName + "\n{\n";
        for (int i = 0; i < iDataArray.Length; i++)
        {
            string iValue = iNames[i];
            if (string.IsNullOrEmpty(iValue))
            {
                Debug.LogWarning($"{iFieldName} value is null or empty at index {i}.");
                continue;
            }
            iValue = iValue.Replace(" ", "_").Replace("-", "_");
            iCode += "    " + iValue + (i < iDataArray.Length - 1 ? "," : "") + "\n";
        }
        iCode += "}";
        _WriteFileToDisk(iCompletePath, iCode);
#endif
    }

    /// <summary>
    /// Caution : using this method will change the order of the enum. 
    /// if this was added after first phase of development use _AddValueToLast() instead.
    /// </summary>
    public static void _AddValueToFirst(string iEnumName, string iNewValue)
    {
#if UNITY_EDITOR
        _AddEnumValue(iEnumName, iNewValue, insertAtStart: true);
#endif
    }
    public static void _AddValueToLast(string iEnumName, string iNewValue)
    {
#if UNITY_EDITOR
        _AddEnumValue(iEnumName, iNewValue, insertAtStart: false);
#endif
    }

    private static void _AddEnumValue(string iEnumName, string iNewValue, bool insertAtStart)
    {
#if UNITY_EDITOR
        if (!_CheckForErrors(iEnumName, iNewValue))
            return;

        string iFilePath = Path.Combine(_DEFAULT_FILE_PATH, iEnumName + ".cs");
        if (!File.Exists(iFilePath))
        {
            Debug.LogError($"Enum file '{iEnumName}.cs' does not exist. Generate the enum first.");
            return;
        }

        if (!_CheckForBannedSymbols(new string[] { iNewValue }))
            return;

        string iContent = File.ReadAllText(iFilePath);
        string iCleanValue = iNewValue.Replace(" ", "_").Replace("-", "_");

        if (iContent.Contains("    " + iCleanValue))
        {
            Debug.LogWarning($"Enum value '{iCleanValue}' already exists in {iEnumName}.");
            return;
        }

        int iOpenBrace = iContent.IndexOf("{\n");
        int iLastBrace = iContent.LastIndexOf('}');
        if (iOpenBrace == -1 || iLastBrace == -1)
        {
            Debug.LogError($"Invalid enum file format for '{iEnumName}.cs'.");
            return;
        }

        string iBefore = iContent.Substring(0, iOpenBrace + 2);
        string iAfter = iContent.Substring(iOpenBrace + 2, iLastBrace - (iOpenBrace + 2)).TrimEnd();

        string iNewContent;
        if (string.IsNullOrWhiteSpace(iAfter))
        {
            // Empty enum, just add first value
            iNewContent = iBefore + "    " + iCleanValue + "\n}";
        }
        else
        {
            if (insertAtStart)
                iNewContent = iBefore + "    " + iCleanValue + ",\n" + iAfter + "\n}";
            else
            {
                if (!iAfter.EndsWith(","))
                    iAfter += ",";
                iNewContent = iBefore + iAfter + "\n    " + iCleanValue + "\n}";
            }
        }

        _WriteFileToDisk(iFilePath, iNewContent);
        Debug.Log($"Added '{iCleanValue}' to enum '{iEnumName}'.");
#endif
    }
    private static void _WriteFileToDisk(string iPath, string iContent)
    {
#if UNITY_EDITOR
        File.WriteAllText(iPath, iContent);
        AssetDatabase.Refresh();
#endif
    }

    #region Error Detection
    private static bool _CheckForBannedSymbols(string[] iArrayNames)
    {
        foreach (var item in iArrayNames)
        {
            if (string.IsNullOrEmpty(item))
                continue;

            if (item.Contains(" ") || item.Contains("-") || char.IsDigit(item[0]))
            {
                Debug.LogError($"Enum name '{item}' can't contain spaces, '-' symbols, or start with a number.");
                return false;
            }
        }
        return true;
    }
    private static bool _CheckForErrors<T>(T[] iDataArray)
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Enum Generator Only Works in Editor");
            return false;
        }
        if (iDataArray == null || iDataArray.Length == 0)
        {
            Debug.LogWarning("Data array is empty or null.");
            return false;
        }
        return true;
    }
    private static bool _CheckForErrors(string iEnumName, string iNewValue)
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Enum Generator Only Works in Editor");
            return false;
        }
        if (string.IsNullOrEmpty(iEnumName) || string.IsNullOrEmpty(iNewValue))
        {
            Debug.LogWarning("enum name or new value is empty.");
            return false;
        }
        return true;
    }
    private static bool _CheckForDuplicatedFile(string iPath)
    {
        if (File.Exists(iPath))
        {
            string fullPath = Path.GetFullPath(iPath).Replace("\\", "/");
            string defaultPath = Path.GetFullPath(Path.Combine(_DEFAULT_FILE_PATH, Path.GetFileName(iPath))).Replace("\\", "/");

            if (fullPath != defaultPath)
            {
                Debug.LogWarning($"Enum file '{Path.GetFileName(iPath)}' already exists in a different path: {fullPath}");
                return true;
            }
        }
        return false;
    }

    #endregion
}
