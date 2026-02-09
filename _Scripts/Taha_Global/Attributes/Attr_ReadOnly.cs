using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif


[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ReadOnlyAttribute : PropertyAttribute
{
    // No constructor needed since it's always the same behavior
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    // Cache the GUI color to avoid creating new Color objects every frame
    private static readonly Color _readOnlyColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    // Cache height calculations to avoid repeated expensive operations
    private static readonly System.Collections.Generic.Dictionary<string, float> _heightCache =
        new System.Collections.Generic.Dictionary<string, float>();

    // Cache GUI state to avoid repeated property lookups
    private struct GUIState
    {
        public bool enabled;
        public Color color;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Cache original GUI state in a struct (more efficient than individual variables)
        var originalState = new GUIState
        {
            enabled = GUI.enabled,
            color = GUI.color
        };

        // Apply read-only styling (batch state changes)
        GUI.enabled = false;
        GUI.color = _readOnlyColor;

        // Draw the property field
        EditorGUI.PropertyField(position, property, label, true);

        // Restore original GUI state (batch restore)
        GUI.enabled = originalState.enabled;
        GUI.color = originalState.color;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Create a cache key based on property path and type
        string cacheKey = $"{property.propertyPath}_{property.propertyType}_{property.isArray}";

        // Check cache first
        if (_heightCache.TryGetValue(cacheKey, out float cachedHeight))
        {
            return cachedHeight;
        }

        // Calculate height and cache it
        float height = EditorGUI.GetPropertyHeight(property, label, true);

        // Limit cache size to prevent memory leaks
        if (_heightCache.Count < 1000)
        {
            _heightCache[cacheKey] = height;
        }

        return height;
    }

    // Clear cache when entering play mode or when scripts reload
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        _heightCache.Clear();
    }
}


// Alternative ultra-lightweight version for maximum performance
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawerUltraLight : PropertyDrawer
{
    private static readonly Color _readOnlyColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Skip state restoration for maximum performance (less safe but faster)
        var wasEnabled = GUI.enabled;
        var wasColor = GUI.color;

        GUI.enabled = false;
        GUI.color = _readOnlyColor;

        EditorGUI.PropertyField(position, property, label, true);

        GUI.enabled = wasEnabled;
        GUI.color = wasColor;
    }

    // For simple properties, return a constant height to avoid calculations
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Fast path for simple properties
        if (property.propertyType == SerializedPropertyType.Integer ||
            property.propertyType == SerializedPropertyType.Float ||
            property.propertyType == SerializedPropertyType.String ||
            property.propertyType == SerializedPropertyType.Boolean)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        // Fall back to default calculation for complex properties
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif