using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Taha/AirDrop Data")]
public class AirDropData : ScriptableObject
{
    [SerializeField] _AirDropChance[] _airDropChances;

    public _AirDropChance _GetRandomAirDrop()
    {
        float totalChance = 0f;
        foreach (var iData in _airDropChances)
        {
            totalChance += iData._dropChance;
        }

        float pick = Random.Range(0f, totalChance);
        float current = 0f;

        foreach (var iDrop in _airDropChances)
        {
            current += iDrop._dropChance;
            if (pick <= current)
                return iDrop;
        }

        return _airDropChances[0];
    }
    [System.Serializable]
    public class _AirDropChance
    {
        public _AirdropRewards _reward;
        [ ConditionalEnum(nameof(_reward), (int)_AirdropRewards.coin)] public int _coinsCount;
        [Range(0, 100)] public float _dropChance;
        public Sprite _rewardSprite;
    }
    public enum _AirdropRewards
    {
        under8, freeze, specific, coin
    }
}
