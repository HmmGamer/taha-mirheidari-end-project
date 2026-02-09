using System.Collections;
using UnityEngine;

public class AndroidFpsLock : MonoBehaviour
{
    public bool _workInEditor = false;
    const int _fpsLock = 60;
    void Start()
    {
#if UNITY_EDITOR
        if (_workInEditor)
            Application.targetFrameRate = _fpsLock;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
            Application.targetFrameRate = _fpsLock;
#endif
    }
}
