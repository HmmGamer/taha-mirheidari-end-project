using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Method)]
public class CreateSOButtonAttribute : Attribute
{
    public string ButtonName { get; }
    public Vector2 Size { get; }

    public CreateSOButtonAttribute(string buttonName, float xSize = 250, float ySize = 30)
    {
        ButtonName = buttonName;
        Size = new Vector2(xSize, ySize);
    }
}

#if UNITY_EDITOR
// Solution 1: Make ButtonEditorBase a custom editor for ScriptableObject
[CustomEditor(typeof(ScriptableObject), true)]
[CanEditMultipleObjects]
public class ButtonEditorBase : Editor
{
    private struct ButtonData
    {
        public readonly MethodInfo method;
        public readonly GUIContent content;
        public readonly GUILayoutOption[] options;
        public readonly bool requiresDirty;

        public ButtonData(MethodInfo method, CreateSOButtonAttribute attribute)
        {
            this.method = method;
            this.content = new GUIContent(attribute.ButtonName);
            this.options = new GUILayoutOption[]
            {
                GUILayout.Width(attribute.Size.x),
                GUILayout.Height(attribute.Size.y)
            };

            var declaringType = method.DeclaringType;
            this.requiresDirty = typeof(MonoBehaviour).IsAssignableFrom(declaringType) ||
                                 typeof(ScriptableObject).IsAssignableFrom(declaringType);
        }
    }

    private static readonly Dictionary<Type, ButtonData[]> typeButtonCache = new Dictionary<Type, ButtonData[]>();

    private ButtonData[] _cachedButtons;
    private bool _hasButtons = false;

    private void OnEnable()
    {
        CacheButtonMethods();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (_hasButtons)
        {
            EditorGUILayout.Space();
            DrawButtons();
        }
    }

    private void CacheButtonMethods()
    {
        var targetType = target.GetType();

        if (typeButtonCache.TryGetValue(targetType, out _cachedButtons))
        {
            _hasButtons = _cachedButtons.Length > 0;
            return;
        }

        var buttonList = new List<ButtonData>();
        var methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var method in methods)
        {
            if (method.GetParameters().Length != 0) continue;

            var attribute = method.GetCustomAttribute<CreateSOButtonAttribute>();
            if (attribute != null)
            {
                buttonList.Add(new ButtonData(method, attribute));
            }
        }

        _cachedButtons = buttonList.ToArray();
        typeButtonCache[targetType] = _cachedButtons;
        _hasButtons = _cachedButtons.Length > 0;
    }

    private void DrawButtons()
    {
        for (int i = 0; i < _cachedButtons.Length; i++)
        {
            var buttonData = _cachedButtons[i];

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(buttonData.content, buttonData.options))
            {
                InvokeMethod(buttonData);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }

    private void InvokeMethod(ButtonData buttonData)
    {
        try
        {
            buttonData.method.Invoke(target, null);

            if (buttonData.requiresDirty)
            {
                EditorUtility.SetDirty(target);

                if (target is MonoBehaviour mb && mb.gameObject.scene.IsValid())
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mb.gameObject.scene);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to invoke method {buttonData.method.Name}: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public override bool RequiresConstantRepaint() => false;

    [InitializeOnLoadMethod]
    private static void ClearCacheOnRecompile()
    {
        typeButtonCache.Clear();
    }
}
#endif