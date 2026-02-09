using UnityEngine;
using System;
using System.Reflection;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Automatically sets the integer field to match the element's index in its array.
/// The index is automatically updated in the Inspector whenever the array changes or elements are reordered.
/// </summary>

public class _AutoIndexSample : MonoBehaviour
{
    public LevelData[] _allLevelsData; // Each element's _index is set automatically

    [System.Serializable]
    public class LevelData
    {
        [AutoIndex, ReadOnly] public int _level;
        public string _someValue;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class AutoIndexAttribute : PropertyAttribute { }

[Serializable]
public class MyData
{
    [AutoIndex] public int _index;
    public string _someValue;
}

public class MyComponent : MonoBehaviour
{
    public MyData[] _dataArray;
}

public class MyScriptableObject : ScriptableObject
{
    public MyData[] _dataArray;
}

#if UNITY_EDITOR
[CustomEditor(typeof(MyComponent))]
public class MyComponentAutoIndexEditor : AutoIndexEditor { }

[CustomEditor(typeof(MyScriptableObject))]
public class MyScriptableObjectAutoIndexEditor : AutoIndexEditor { }

public class AutoIndexEditor : Editor
{
    private static readonly Dictionary<Type, List<FieldInfo>> _cachedIndexFields = new();
    private static readonly Dictionary<Type, List<FieldInfo>> _cachedArrayFields = new();

    // Cache for serialized properties to avoid repeated lookups
    private Dictionary<string, SerializedProperty> _propertyCache;
    private bool _needsRepaint;

    private void OnEnable()
    {
        _propertyCache = new Dictionary<string, SerializedProperty>();
        CacheArrayFields();
    }

    private void CacheArrayFields()
    {
        var targetType = target.GetType();

        if (_cachedArrayFields.ContainsKey(targetType))
            return;

        var arrayFields = new List<FieldInfo>();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        foreach (var field in targetType.GetFields(flags))
        {
            if (field.FieldType.IsArray &&
                field.GetCustomAttribute<SerializeField>() != null || field.IsPublic)
            {
                var elementType = field.FieldType.GetElementType();
                if (elementType?.IsSerializable == true)
                {
                    arrayFields.Add(field);
                    CacheIndexFieldsForType(elementType);
                }
            }
        }

        _cachedArrayFields[targetType] = arrayFields;
    }

    private void CacheIndexFieldsForType(Type elementType)
    {
        if (_cachedIndexFields.ContainsKey(elementType))
            return;

        var indexFields = new List<FieldInfo>();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        foreach (var field in elementType.GetFields(flags))
        {
            if (field.FieldType == typeof(int) &&
                field.GetCustomAttribute<AutoIndexAttribute>() != null)
            {
                indexFields.Add(field);
            }
        }

        _cachedIndexFields[elementType] = indexFields;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Only update indices when something has changed
        if (GUI.changed || _needsRepaint)
        {
            UpdateAutoIndices();
            _needsRepaint = false;
        }

        DrawDefaultInspector();

        if (serializedObject.ApplyModifiedProperties())
        {
            _needsRepaint = true;
        }
    }

    private void UpdateAutoIndices()
    {
        var targetType = target.GetType();

        if (!_cachedArrayFields.TryGetValue(targetType, out var arrayFields))
            return;

        bool modified = false;

        foreach (var arrayField in arrayFields)
        {
            var elementType = arrayField.FieldType.GetElementType();

            if (!_cachedIndexFields.TryGetValue(elementType, out var indexFields) ||
                indexFields.Count == 0)
                continue;

            // Use SerializedProperty instead of reflection for better performance
            var propertyName = arrayField.Name;
            if (!_propertyCache.TryGetValue(propertyName, out var arrayProperty))
            {
                arrayProperty = serializedObject.FindProperty(propertyName);
                if (arrayProperty != null)
                    _propertyCache[propertyName] = arrayProperty;
            }

            if (arrayProperty?.isArray == true)
            {
                modified |= UpdateArrayIndices(arrayProperty, indexFields);
            }
        }

        if (modified)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private bool UpdateArrayIndices(SerializedProperty arrayProperty, List<FieldInfo> indexFields)
    {
        bool modified = false;

        for (int i = 0; i < arrayProperty.arraySize; i++)
        {
            var elementProperty = arrayProperty.GetArrayElementAtIndex(i);

            foreach (var indexField in indexFields)
            {
                var indexProperty = elementProperty.FindPropertyRelative(indexField.Name);
                if (indexProperty?.propertyType == SerializedPropertyType.Integer)
                {
                    if (indexProperty.intValue != i)
                    {
                        indexProperty.intValue = i;
                        modified = true;
                    }
                }
            }
        }

        return modified;
    }

    // Optional: Add context menu for manual refresh
    [MenuItem("CONTEXT/MyComponent/Refresh Auto Indices")]
    [MenuItem("CONTEXT/MyScriptableObject/Refresh Auto Indices")]
    private static void RefreshAutoIndices(MenuCommand command)
    {
        var target = command.context;
        if (target != null)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif