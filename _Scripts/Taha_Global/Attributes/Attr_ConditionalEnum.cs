using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ConditionalField_Sample : MonoBehaviour
{
    [SerializeField] _AllFields _fields;

    [SerializeField, ConditionalEnum(nameof(_fields), (int)_AllFields.field1)]
    float field1;

    [SerializeField, ConditionalEnum(nameof(_fields), (int)_AllFields.field2_3)]
    float field2;

    [SerializeField, ConditionalEnum(nameof(_fields), (int)_AllFields.field2_3)]
    float field3;

    public enum _AllFields
    {
        field1, field2_3, none
    }
}

public class ConditionalEnumAttribute : PropertyAttribute
{
    public string _enumField;
    public HashSet<int> _targetEnumValues;

    public ConditionalEnumAttribute(string enumFieldName, params int[] targetEnumValues)
    {
        _enumField = enumFieldName;
        _targetEnumValues = new HashSet<int>(targetEnumValues);
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ConditionalEnumAttribute), true)]
public class ConditionalEnumDrawer : PropertyDrawer
{
    private SerializedProperty _cachedEnumProperty;
    private string _lastPropertyPath;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalEnumAttribute attribute = (ConditionalEnumAttribute)this.attribute;

        // Update cache if property path changed or serialized object changed
        if (_cachedEnumProperty == null ||
            _cachedEnumProperty.serializedObject != property.serializedObject ||
            _lastPropertyPath != property.propertyPath)
        {
            _cachedEnumProperty = GetEnumProperty(property, attribute._enumField);
            _lastPropertyPath = property.propertyPath;
        }

        // Early return if enum property not found or value doesn't match
        if (_cachedEnumProperty == null || !attribute._targetEnumValues.Contains(_cachedEnumProperty.enumValueIndex))
        {
            return;
        }

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalEnumAttribute attribute = (ConditionalEnumAttribute)this.attribute;

        // Update cache if property path changed or serialized object changed
        if (_cachedEnumProperty == null ||
            _cachedEnumProperty.serializedObject != property.serializedObject ||
            _lastPropertyPath != property.propertyPath)
        {
            _cachedEnumProperty = GetEnumProperty(property, attribute._enumField);
            _lastPropertyPath = property.propertyPath;
        }

        // Return 0 height if enum property not found or value doesn't match
        if (_cachedEnumProperty == null || !attribute._targetEnumValues.Contains(_cachedEnumProperty.enumValueIndex))
        {
            return 0;
        }

        return EditorGUI.GetPropertyHeight(property, true);
    }

    private SerializedProperty GetEnumProperty(SerializedProperty property, string enumField)
    {
        // Handle nested properties properly
        string propertyPath = property.propertyPath;

        // Try different path resolution strategies for nested classes
        SerializedProperty enumProperty = null;

        // Strategy 1: Replace last property name with enum field name
        int lastDotIndex = propertyPath.LastIndexOf('.');
        if (lastDotIndex >= 0)
        {
            string basePath = propertyPath.Substring(0, lastDotIndex + 1);
            enumProperty = property.serializedObject.FindProperty(basePath + enumField);
        }

        // Strategy 2: If that fails, try relative path from current property
        if (enumProperty == null)
        {
            string relativePath = propertyPath.Replace(property.name, enumField);
            enumProperty = property.serializedObject.FindProperty(relativePath);
        }

        // Strategy 3: If still fails, try sibling property search
        if (enumProperty == null && property.depth > 0)
        {
            SerializedProperty parent = GetParentProperty(property);
            if (parent != null)
            {
                SerializedProperty child = parent.Copy();
                if (child.Next(true))
                {
                    do
                    {
                        if (child.name == enumField)
                        {
                            enumProperty = child.Copy();
                            break;
                        }
                    } while (child.Next(false));
                }
            }
        }

        // Strategy 4: Last resort - search from root
        if (enumProperty == null)
        {
            enumProperty = property.serializedObject.FindProperty(enumField);
        }

        return enumProperty;
    }

    private SerializedProperty GetParentProperty(SerializedProperty property)
    {
        string path = property.propertyPath;
        int lastDotIndex = path.LastIndexOf('.');

        if (lastDotIndex < 0)
            return null;

        string parentPath = path.Substring(0, lastDotIndex);
        return property.serializedObject.FindProperty(parentPath);
    }
}
#endif