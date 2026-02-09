using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : Singleton_Abs<ScoreManager>
{
    public int _currentScore;

    public void _AddScore(int iScore)
    {
        _currentScore += iScore;
    }
}
