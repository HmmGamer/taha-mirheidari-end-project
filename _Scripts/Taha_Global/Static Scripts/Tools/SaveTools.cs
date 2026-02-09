using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SaveTools
{
    #region Array
    public static void _SaveArrayToDisk<T>(ref T[] _array, string _fileName)
    {
        string filePath = Application.persistentDataPath + "/" + _fileName;
        string jsonData = JsonUtility.ToJson(new _ArrayContainer<T>(_array), true);
        File.WriteAllText(filePath, jsonData);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }
    public static void _LoadArrayFromDisk<T>(ref T[] _array, string _fileName)
    {
        string filePath = Application.persistentDataPath + "/" + _fileName;
        if (File.Exists(filePath))
        {
            string loadedData = File.ReadAllText(filePath);
            _array = JsonUtility.FromJson<_ArrayContainer<T>>(loadedData)._dataArray;
        }
    }
    public static void _ResetArray<T>(ref T[] _array, string _fileName)
    {
        _array = new T[0];
        _SaveArrayToDisk(ref _array, _fileName);
    }
    [System.Serializable]
    private class _ArrayContainer<T>
    {
        public T[] _dataArray;
        public _ArrayContainer(T[] dataArray) { _dataArray = dataArray; }
    }
    #endregion
    #region List
    public static void _SaveListToDisk<T>(ref List<T> _list, string _fileName)
    {
        string filePath = Application.persistentDataPath + "/" + _fileName;
        string jsonData = JsonUtility.ToJson(new _ListContainer<T>(_list), true);
        File.WriteAllText(filePath, jsonData);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }
    public static void _LoadListFromDisk<T>(ref List<T> _list, string _fileName)
    {
        string filePath = Application.persistentDataPath + "/" + _fileName;
        if (File.Exists(filePath))
        {
            string loadedData = File.ReadAllText(filePath);
            _list = JsonUtility.FromJson<_ListContainer<T>>(loadedData)._dataList;
        }
    }
    public static void _ResetList<T>(ref List<T> _list, string _fileName)
    {
        _list = new List<T>();
        _SaveListToDisk(ref _list, _fileName);
    }
    [System.Serializable]
    private class _ListContainer<T>
    {
        public List<T> _dataList;
        public _ListContainer(List<T> dataList) { _dataList = dataList; }
    }
    #endregion
    #region ScriptableObjects
    public static void _SaveSOToDisk(ScriptableObject _target, string _fileName = null)
    {
        if (_fileName == null)
        {
            _fileName = _target.name + ".json";
        }
        else if (!_fileName.EndsWith(".json"))
        {
            _fileName += ".json";
        }

        string jsonData = JsonUtility.ToJson(_target, true);
        string filePath = Path.Combine(Application.persistentDataPath, _fileName);
        File.WriteAllText(filePath, jsonData);
#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
    }
    public static void _LoadSOFromDisk(ScriptableObject _target, string _fileName)
    {
        if (_fileName == null)
        {
            _fileName = _target.name + ".json";
        }
        else if (!_fileName.EndsWith(".json"))
        {
            _fileName += ".json";
        }

        string filePath = Path.Combine(Application.persistentDataPath, _fileName);
        if (File.Exists(filePath))
        {
            string loadedData = File.ReadAllText(filePath);
            JsonUtility.FromJsonOverwrite(loadedData, _target);
        }
    }
    public static void _ResetSO(ScriptableObject _target, string _fileName = null)
    {
        ScriptableObject iNew = ScriptableObject.CreateInstance(_target.GetType());
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(iNew), _target);
        _SaveSOToDisk(_target, _fileName);
    }
    #endregion
}
class _SaveToolsSample
{
    //#region Save/Load
    //[CreateButton("Save Data")]
    //public void _SaveData()
    //{
    //    SaveTools._SaveSOToDisk(this, "Sample Data " + name);
    //    Debug.Log(name);
    //}
    //public void _LoadData()
    //{
    //    SaveTools._LoadSOFromDisk(this, "Sample Data" + name);
    //}
    //[CreateButton("Reset All Data")]
    //public void _ResetAllData()
    //{
    //    SaveTools._ResetSO(this, "Sample Data" + name);
    //}
    //#endregion
}