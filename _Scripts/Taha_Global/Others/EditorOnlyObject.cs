using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorOnlyObject : MonoBehaviour
{
#if !UNITY_EDITOR
    private void Start()
    {
        Destroy(gameObject);
    }
#endif
}
