using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this script is mainly to control a safe level transfer
/// </summary>
public class NextLevelController : MonoBehaviour
{
    public void _StartLevel(int iLevel)
    {
        PlayerPrefs.SetInt(A.DataKey.currentLevelIndex, iLevel);

        LoadingManager._instance._LoadScene(_AllScenes.MainGame);
    }
}