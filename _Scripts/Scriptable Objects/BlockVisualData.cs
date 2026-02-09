using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Taha/Block Visual Data")]
public class BlockVisualData : ScriptableObject
{
    [SerializeField] Sprite _stoneSprite;
    [SerializeField] List<VisualEntry> _visuals;

    public Color _GetColorFor(int iValue)
    {
        foreach (VisualEntry entry in _visuals)
        {
            if (entry._value == iValue) return entry._color;
        }
        return Color.white;
    }
    public Sprite _GetSpriteFor(int iValue)
    {
        foreach (VisualEntry entry in _visuals)
        {
            if (entry._value == iValue) return entry._sprite;
        }
        return null;
    }
    public Sprite _GetStoneSprite()
    {
        return _stoneSprite;
    }

    [System.Serializable]
    public class VisualEntry
    {
        public int _value;
        public Color _color;
        public Sprite _sprite;
    }
}
