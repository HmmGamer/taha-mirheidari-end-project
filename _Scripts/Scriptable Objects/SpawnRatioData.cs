using UnityEngine;

[CreateAssetMenu(menuName = "Taha/Spawn Ratio")]
public class SpawnRatioData : ScriptableObject
{
    [SerializeField] _SpawnData[] _spawnData;

    [System.Serializable]
    public class _SpawnData
    {
        public _Numbers _number;
        [Range(0, 100)] public float _spawnChance;
    }
    public int _GetRandomValue()
    {
        float iTotalChance = 0f;
        foreach (var iData in _spawnData)
        {
            iTotalChance += iData._spawnChance;
        }

        float iPick = Random.Range(0f, iTotalChance);

        float iCurrent = 0f;
        foreach (var iData in _spawnData)
        {
            iCurrent += iData._spawnChance;
            if (iPick <= iCurrent)
            {
                return (int)iData._number;
            }
        }

        return (int)_spawnData[0]._number;
    }
}
