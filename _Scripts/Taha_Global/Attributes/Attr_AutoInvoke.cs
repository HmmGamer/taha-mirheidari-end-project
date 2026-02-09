//using System;
//using UnityEngine;
//using UnityEditor;
//using System.Reflection;
//using System.Collections.Generic;

//// The AutoInvoke attribute
//[AttributeUsage(AttributeTargets.Field)]
//public class AutoInvokeAttribute : PropertyAttribute
//{
//    public string methodName;

//    public AutoInvokeAttribute(string methodName)
//    {
//        this.methodName = methodName;
//    }
//}

//#if UNITY_EDITOR
//// Cached reflection data to avoid repeated reflection calls
//public static class AutoInvokeCache
//{
//    public struct FieldMethodPair
//    {
//        public FieldInfo field;
//        public MethodInfo method;
//    }

//    private static Dictionary<Type, FieldMethodPair[]> typeCache = new Dictionary<Type, FieldMethodPair[]>();

//    public static FieldMethodPair[] GetCachedFieldsAndMethods(Type type)
//    {
//        if (typeCache.TryGetValue(type, out FieldMethodPair[] cached))
//            return cached;

//        // Build cache for this type
//        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//        var pairs = new List<FieldMethodPair>();

//        foreach (var field in fields)
//        {
//            var autoInvoke = field.GetCustomAttribute<AutoInvokeAttribute>();
//            if (autoInvoke != null)
//            {
//                var method = type.GetMethod(autoInvoke.methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//                if (method != null)
//                {
//                    pairs.Add(new FieldMethodPair { field = field, method = method });
//                }
//            }
//        }

//        var result = pairs.ToArray();
//        typeCache[type] = result;
//        return result;
//    }

//    public static bool HasAutoInvokeFields(Type type)
//    {
//        return GetCachedFieldsAndMethods(type).Length > 0;
//    }
//}

//// Ultra-lightweight editor that only activates for types with AutoInvoke fields
//[CustomEditor(typeof(MonoBehaviour), true)]
//[CanEditMultipleObjects]
//public class AutoInvokeEditor : Editor
//{
//    private object[] initialValues;
//    private AutoInvokeCache.FieldMethodPair[] fieldMethodPairs;
//    private bool hasAutoInvokeFields;

//    private void OnEnable()
//    {
//        // Early exit if this type has no AutoInvoke fields
//        hasAutoInvokeFields = AutoInvokeCache.HasAutoInvokeFields(target.GetType());
//        if (!hasAutoInvokeFields) return;

//        // Cache the reflection data once
//        fieldMethodPairs = AutoInvokeCache.GetCachedFieldsAndMethods(target.GetType());

//        // Store initial values in a simple array (faster than dictionary)
//        initialValues = new object[fieldMethodPairs.Length];
//        for (int i = 0; i < fieldMethodPairs.Length; i++)
//        {
//            initialValues[i] = fieldMethodPairs[i].field.GetValue(target);
//        }
//    }

//    private void OnDisable()
//    {
//        if (!hasAutoInvokeFields || target == null) return;

//        // Compare current values with initial values
//        for (int i = 0; i < fieldMethodPairs.Length; i++)
//        {
//            object currentValue = fieldMethodPairs[i].field.GetValue(target);

//            if (!object.Equals(initialValues[i], currentValue))
//            {
//                // Value changed - invoke the cached method directly
//                try
//                {
//                    fieldMethodPairs[i].method.Invoke(target, null);
//                }
//                catch (Exception e)
//                {
//                    Debug.LogError($"[AutoInvoke] Failed to invoke method on {target.name}: {e.Message}");
//                }
//            }
//        }
//    }
//}

//// Minimal play mode handler
//[InitializeOnLoad]
//public static class AutoInvokePlayModeHandler
//{
//    private struct ComponentSnapshot
//    {
//        public MonoBehaviour component;
//        public object[] values;
//        public AutoInvokeCache.FieldMethodPair[] fieldMethodPairs;
//    }

//    private static ComponentSnapshot[] snapshots;

//    static AutoInvokePlayModeHandler()
//    {
//        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
//    }

//    private static void OnPlayModeStateChanged(PlayModeStateChange state)
//    {
//        if (state == PlayModeStateChange.ExitingEditMode)
//        {
//            StoreSnapshots();
//        }
//        else if (state == PlayModeStateChange.EnteredEditMode)
//        {
//            CheckSnapshots();
//            snapshots = null; // Free memory
//        }
//    }

//    private static void StoreSnapshots()
//    {
//        var components = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
//        var validSnapshots = new List<ComponentSnapshot>();

//        foreach (var component in components)
//        {
//            if (component == null) continue;

//            var fieldMethodPairs = AutoInvokeCache.GetCachedFieldsAndMethods(component.GetType());
//            if (fieldMethodPairs.Length == 0) continue;

//            var values = new object[fieldMethodPairs.Length];
//            for (int i = 0; i < fieldMethodPairs.Length; i++)
//            {
//                values[i] = fieldMethodPairs[i].field.GetValue(component);
//            }

//            validSnapshots.Add(new ComponentSnapshot
//            {
//                component = component,
//                values = values,
//                fieldMethodPairs = fieldMethodPairs
//            });
//        }

//        snapshots = validSnapshots.ToArray();
//    }

//    private static void CheckSnapshots()
//    {
//        if (snapshots == null) return;

//        foreach (var snapshot in snapshots)
//        {
//            if (snapshot.component == null) continue;

//            for (int i = 0; i < snapshot.fieldMethodPairs.Length; i++)
//            {
//                object currentValue = snapshot.fieldMethodPairs[i].field.GetValue(snapshot.component);

//                if (!object.Equals(snapshot.values[i], currentValue))
//                {
//                    try
//                    {
//                        snapshot.fieldMethodPairs[i].method.Invoke(snapshot.component, null);
//                    }
//                    catch (Exception e)
//                    {
//                        Debug.LogError($"[AutoInvoke] Failed to invoke method on {snapshot.component.name}: {e.Message}");
//                    }
//                }
//            }
//        }
//    }
//}
//#endif