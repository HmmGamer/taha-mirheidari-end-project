using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntentController : MonoBehaviour
{
    [SerializeField] _Intents _intent;
    [SerializeField] bool _autoAddToButtons;
    [SerializeField, ConditionalField(nameof(_autoAddToButtons))] Button _button;
    private void Start()
    {
        if (_autoAddToButtons)
            _button.onClick.AddListener(_OpenIntent);
    }
    public void _OpenIntent()
    {
        #region Editor Checks
#if UNITY_EDITOR
        if (IntentManager._instance == null)
        {
            Debug.LogError("There is no IntentManager in the scene");
            return;
        }
        Debug.Log("Intents only work in android builds");

        if (transform)
            return;
#endif
        #endregion

        IntentManager._instance._OpenIntent(_intent);
    }
}
