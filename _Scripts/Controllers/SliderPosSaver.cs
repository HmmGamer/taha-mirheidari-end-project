using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// this code saves the slider and menu pos so the player can start from levels canvas
/// instead of the main menu, it also saves the slider pos for easier level tracking
/// </summary>
public class SliderPosSaver : MonoBehaviour
{
    [SerializeField] ScrollRect _scrollRect;
    [SerializeField] GameObject _mainMenuCanvas;
    [SerializeField] GameObject _levelsCanvas;

    private void Start()
    {
        if (_SliderSessionData.StartInLevels)
        {
            _levelsCanvas.SetActive(true);
            _mainMenuCanvas.SetActive(false);

            _scrollRect.normalizedPosition = _SliderSessionData.SavedScrollPos;
        }
        else
        {
            _mainMenuCanvas.SetActive(true);
            _levelsCanvas.SetActive(false);
        }
    }
    private void OnDisable()
    {
        _SavePos();
    }
    public void _ResetPos()
    {
        _SliderSessionData.StartInLevels = false;
        _SliderSessionData.SavedScrollPos = Vector2.zero;
    }
    private void _SavePos()
    {
        //print(_scrollRect.normalizedPosition);
        _SliderSessionData.SavedScrollPos = _scrollRect.normalizedPosition;
        _SliderSessionData.StartInLevels = true;
    }
}
static class _SliderSessionData
{
    public static Vector2 SavedScrollPos = Vector2.zero;
    public static bool StartInLevels = false;
}

