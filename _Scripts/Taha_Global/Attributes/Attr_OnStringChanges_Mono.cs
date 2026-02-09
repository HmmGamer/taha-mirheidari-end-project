using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
#endif

//Calls a method when a string field changes in the Inspector
[AttributeUsage(AttributeTargets.Field)]
public sealed class OnStringChanges_MonoAttribute : PropertyAttribute
{
    public readonly string MethodName;
    public OnStringChanges_MonoAttribute(string iMethodName)
    {
        MethodName = iMethodName;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(OnStringChanges_MonoAttribute))]
public sealed class OnStringChanges_MonoDrawer : PropertyDrawer
{
    private static readonly Dictionary<int, MethodInfo> _methodCache = new Dictionary<int, MethodInfo>();

    public override void OnGUI(Rect iPosition, SerializedProperty iProperty, GUIContent iLabel)
    {
        if (iProperty.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(iPosition, iProperty, iLabel);
            return;
        }

        UnityEngine.Object target = iProperty.serializedObject.targetObject;

        if (target is not MonoBehaviour)
        {
            EditorGUI.PropertyField(iPosition, iProperty, iLabel);
            return;
        }

        EditorGUI.BeginProperty(iPosition, iLabel, iProperty);

        string newValue = EditorGUI.DelayedTextField(
            iPosition,
            iLabel,
            iProperty.stringValue
        );

        if (newValue != iProperty.stringValue)
        {
            iProperty.stringValue = newValue;
            iProperty.serializedObject.ApplyModifiedProperties();

            _Invoke(target, ((OnStringChanges_MonoAttribute)attribute).MethodName);

            EditorUtility.SetDirty(target);
        }

        EditorGUI.EndProperty();
    }

    private static void _Invoke(UnityEngine.Object iTarget, string iMethodName)
    {
        int key = HashCode.Combine(iTarget.GetType(), iMethodName);

        if (_methodCache.TryGetValue(key, out MethodInfo cached))
        {
            cached.Invoke(iTarget, null);
            return;
        }

        MethodInfo method = iTarget.GetType().GetMethod(
            iMethodName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );

        if (method == null)
        {
            Debug.LogError($"[OnStringChanges_Mono] Method '{iMethodName}' not found on {iTarget.GetType().Name}");
            return;
        }

        _methodCache[key] = method;
        method.Invoke(iTarget, null);
    }
}
#endif