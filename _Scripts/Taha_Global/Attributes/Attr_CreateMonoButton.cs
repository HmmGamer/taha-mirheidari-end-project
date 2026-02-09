using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

// The attribute class
[AttributeUsage(AttributeTargets.Method)]
public class CreateMonoButtonAttribute : PropertyAttribute
{
    public string ButtonText { get; private set; }
    public float Width { get; private set; }
    public float Height { get; private set; }

    public CreateMonoButtonAttribute(string buttonText, float width = 250f, float height = 30f)
    {
        ButtonText = buttonText;
        Width = width;
        Height = height;
    }
}

#if UNITY_EDITOR
// Cached method info for performance
public struct CachedMethodInfo
{
    public MethodInfo method;
    public CreateMonoButtonAttribute attribute;

    public CachedMethodInfo(MethodInfo method, CreateMonoButtonAttribute attribute)
    {
        this.method = method;
        this.attribute = attribute;
    }
}

// Custom property drawer for the attribute
[CustomEditor(typeof(MonoBehaviour), true)]
public class CreateButtonEditor : Editor
{
    // Cache to avoid repeated reflection calls
    private static readonly Dictionary<Type, CachedMethodInfo[]> _methodCache = new Dictionary<Type, CachedMethodInfo[]>();

    // GUIStyle cache to avoid recreation
    private static GUIStyle _cachedButtonStyle;

    // Lazy initialization of button style
    private static GUIStyle ButtonStyle
    {
        get
        {
            if (_cachedButtonStyle == null)
            {
                _cachedButtonStyle = new GUIStyle(GUI.skin.button);
            }
            return _cachedButtonStyle;
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MonoBehaviour targetScript = (MonoBehaviour)target;
        Type targetType = targetScript.GetType();

        // Get cached methods or create cache entry
        CachedMethodInfo[] cachedMethods = GetCachedMethods(targetType);

        // Draw buttons for cached methods
        foreach (var cachedMethod in cachedMethods)
        {
            DrawButton(targetScript, cachedMethod);
        }
    }

    private CachedMethodInfo[] GetCachedMethods(Type targetType)
    {
        // Check cache first
        if (_methodCache.TryGetValue(targetType, out CachedMethodInfo[] cachedMethods))
        {
            return cachedMethods;
        }

        // Cache miss - perform reflection and cache results
        var methodList = new List<CachedMethodInfo>();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        MethodInfo[] methods = targetType.GetMethods(flags);

        foreach (MethodInfo method in methods)
        {
            var buttonAttribute = method.GetCustomAttribute<CreateMonoButtonAttribute>();
            if (buttonAttribute != null)
            {
                methodList.Add(new CachedMethodInfo(method, buttonAttribute));
            }
        }

        // Cache the results
        cachedMethods = methodList.ToArray();
        _methodCache[targetType] = cachedMethods;

        return cachedMethods;
    }

    private void DrawButton(MonoBehaviour targetScript, CachedMethodInfo cachedMethod)
    {
        var method = cachedMethod.method;
        var buttonAttribute = cachedMethod.attribute;

        GUILayout.Space(5);

        // Center the button horizontally
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();

            // Create button with custom dimensions and cached style
            if (GUILayout.Button(buttonAttribute.ButtonText, ButtonStyle,
                GUILayout.Width(buttonAttribute.Width),
                GUILayout.Height(buttonAttribute.Height)))
            {
                InvokeMethod(targetScript, method);
            }

            GUILayout.FlexibleSpace();
        }
    }

    private void InvokeMethod(MonoBehaviour targetScript, MethodInfo method)
    {
        // Check parameter count (cached reflection result)
        if (method.GetParameters().Length == 0)
        {
            try
            {
                method.Invoke(targetScript, null);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error invoking method '{method.Name}': {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Method '{method.Name}' has parameters and cannot be called from CreateButton attribute. Only parameterless methods are supported.");
        }
    }

    // Clear cache when scripts are reloaded to prevent stale data
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        _methodCache.Clear();
        _cachedButtonStyle = null;
    }
}
#endif