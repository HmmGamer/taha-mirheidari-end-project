using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ConditionalFieldAttribute : PropertyAttribute
{
    public string BoolVariableName { get; private set; }
    public bool Reverse { get; private set; }

    public ConditionalFieldAttribute(string boolVariableName, bool reverse = false)
    {
        BoolVariableName = boolVariableName;
        Reverse = reverse;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
public class ConditionalFieldDrawer : PropertyDrawer
{
    private struct BoolPropertyCache
    {
        public string PropertyPath;
        public bool LastValue;
    }

    private static readonly Dictionary<string, BoolPropertyCache> PropertyCache = new Dictionary<string, BoolPropertyCache>();
    private static readonly HashSet<string> CacheKeysToRemove = new HashSet<string>();
    private static int _accessCounter;
    private const int ClearCacheInterval = 1000;
    private static bool _repaintScheduled;
    private static readonly List<SerializedObject> ObjectsToRepaint = new List<SerializedObject>();

    static ConditionalFieldDrawer()
    {
        AssemblyReloadEvents.beforeAssemblyReload += ClearCache;
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (ShouldShow(property))
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return ShouldShow(property) ? EditorGUI.GetPropertyHeight(property, label, true) : 0;
    }

    private bool ShouldShow(SerializedProperty property)
    {
        CheckCacheCleanup();

        ConditionalFieldAttribute attr = attribute as ConditionalFieldAttribute;
        if (attr == null) return true;

        string cacheKey = $"{property.serializedObject.targetObject.GetInstanceID()}.{property.propertyPath}";

        SerializedProperty boolProperty = FindBoolProperty(property, attr.BoolVariableName);
        if (boolProperty == null) return false;

        bool currentValue = boolProperty.boolValue ^ attr.Reverse;

        if (PropertyCache.TryGetValue(cacheKey, out BoolPropertyCache cache))
        {
            if (cache.LastValue != currentValue)
            {
                PropertyCache[cacheKey] = new BoolPropertyCache
                {
                    PropertyPath = boolProperty.propertyPath,
                    LastValue = currentValue
                };
                ScheduleRepaint(property.serializedObject);
            }
        }
        else
        {
            PropertyCache[cacheKey] = new BoolPropertyCache
            {
                PropertyPath = boolProperty.propertyPath,
                LastValue = currentValue
            };
        }

        return currentValue;
    }

    private static void CheckCacheCleanup()
    {
        _accessCounter++;
        if (_accessCounter % ClearCacheInterval == 0)
        {
            foreach (var key in PropertyCache.Keys)
            {
                var instanceId = int.Parse(key.Split('.')[0]);
                if (EditorUtility.InstanceIDToObject(instanceId) == null)
                {
                    CacheKeysToRemove.Add(key);
                }
            }

            foreach (var key in CacheKeysToRemove)
            {
                PropertyCache.Remove(key);
            }
            CacheKeysToRemove.Clear();
        }
    }

    private static void ScheduleRepaint(SerializedObject serializedObject)
    {
        if (ObjectsToRepaint.Contains(serializedObject)) return;

        ObjectsToRepaint.Add(serializedObject);

        if (!_repaintScheduled)
        {
            _repaintScheduled = true;
            EditorApplication.delayCall += () =>
            {
                foreach (var obj in ObjectsToRepaint)
                {
                    if (obj != null) RepaintInspector(obj);
                }
                ObjectsToRepaint.Clear();
                _repaintScheduled = false;
            };
        }
    }

    private static void PlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode ||
            state == PlayModeStateChange.ExitingPlayMode)
        {
            ClearCache();
        }
    }

    private static void ClearCache()
    {
        PropertyCache.Clear();
    }

    private SerializedProperty FindBoolProperty(SerializedProperty property, string boolName)
    {
        // Wave 1: Direct relative
        var result = property.FindPropertyRelative(boolName);
        if (result != null) return result;

        // Wave 2: Sibling property
        if (property.propertyPath.Contains("."))
        {
            string parentPath = property.propertyPath.Substring(0, property.propertyPath.LastIndexOf('.'));
            var parentProperty = property.serializedObject.FindProperty(parentPath);
            result = parentProperty?.FindPropertyRelative(boolName);
            if (result != null) return result;
        }

        // Wave 3: Root-level
        return property.serializedObject.FindProperty(boolName);
    }

    private static void RepaintInspector(SerializedObject serializedObject)
    {
        if (serializedObject == null || serializedObject.targetObject == null) return;

        var editors = Resources.FindObjectsOfTypeAll<Editor>();
        foreach (var editor in editors)
        {
            if (editor.serializedObject?.targetObject == serializedObject.targetObject)
            {
                editor.Repaint();
                break;
            }
        }
    }
}
#endif