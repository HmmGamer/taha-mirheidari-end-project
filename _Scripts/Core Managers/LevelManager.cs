using System.Collections;
using TahaGlobal.ML;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>
/// TODO: Separate the editor-only parts to clean the main architecture
/// </summary>
[RequireComponent(typeof(NextLevelController))]
public class LevelManager : Singleton_Abs<LevelManager>
{
    #region Editor Only
#if UNITY_EDITOR
    [Header("Editor Settings")]
    [SerializeField] bool _useDefaultLevel = true;
    [SerializeField, ConditionalField(nameof(_useDefaultLevel))] int _defaultLevel = 1;
    public bool _useSavedManager = false;
#endif
    #endregion

    [Header("General Settings")]
    [SerializeField] _MapData[] _allLevels;
    [SerializeField] _LevelTypeData[] _allLevelTypes;
    [SerializeField] Transform _levelSpawnPos;
    [SerializeField] BlockController _blockPrefab;
    [SerializeField] RawImage _bgImage;
    [SerializeField] Image _menuImage;
    [SerializeField] Image _extraMenuImage;

    [Header("Objective Attachments")]
    [SerializeField] Text _objectiveText;
    [SerializeField] MLTextController _objectiveMlText;

    public LevelsSavedData _dataBase;

    private Grid _currentTileMap;

    public _LevelTypeData _currentMineType { get; private set; }
    public int _currentLevelIndex { get; set; }
    public _MapData _currentMapData { get; private set; }
    public bool _isUsingSavedData { get; private set; }

    public void Start()
    {
        _CheckSavedData();
        _UpdateCurrentMap();

        if (_currentTileMap == null && _currentMapData != null)
            _currentTileMap = Instantiate(_currentMapData._levelPrefab, _currentMapData._mapSpawnOffset
                , Quaternion.identity);
        else if (_currentTileMap == null && _currentMapData == null)
        {
            Debug.LogError("pass a level in the lobby or add the map to the child of _levelSpawnPos");
            return;
        }
        GridManager._instance._tilemap = _currentTileMap.transform.GetChild(0).GetComponent<Tilemap>();
        _blockPrefab._visualData = _currentMineType._visualData;

        _menuImage.sprite = _currentMineType._menuSprite;
        if (_extraMenuImage)
            _extraMenuImage.sprite = _currentMineType._extraMenuSprite;

        _objectiveText.color = _currentMineType._menuTextColor;
        _SetLevelDescription();

        _blockPrefab._speedMultiplayer = _currentMapData._blockSpeedMultiplier;

        _bgImage.texture = _currentMapData._mapBgSprite.texture;
        _bgImage.color = Color.white;

        Camera.main.orthographicSize = _currentMapData._cameraScale;

        // Initialize win condition tile color system after tilemap is assigned
        // Ensure GridManager has finished caching before initializing tile colors
        StartCoroutine(_InitializeTileColorSystemDelayed());
    }
    private void _UpdateCurrentMap()
    {
        if (_isUsingSavedData)
            return;

        #region Unity Only
#if UNITY_EDITOR
        try
        {
            _currentLevelIndex = PlayerPrefs.GetInt(A.DataKey.currentLevelIndex
                , _useDefaultLevel ? _defaultLevel : A.DataKey.False);

            int i = 6;
            if (_currentLevelIndex > 0)
                while (i > 0)
                {
                    if (_levelSpawnPos.childCount > 0)
                        Destroy(_levelSpawnPos.GetChild(0).gameObject);
                    else
                        break;
                    i--;
                }

            _currentMapData = _allLevels[_currentLevelIndex];

            // to avoid possible bugs later
            PlayerPrefs.DeleteKey(A.DataKey.currentLevelIndex);
        }
        catch
        {
            // this part is mainly for editor tests and wont be included in builds
            if (_levelSpawnPos.transform.childCount > 0)
            {
                for (int i = 0; i < _levelSpawnPos.transform.childCount; i++)
                {
                    if (_levelSpawnPos.transform.GetChild(i).gameObject.activeInHierarchy)
                    {

                        _currentTileMap = _levelSpawnPos.transform.GetChild(i).GetComponent<Grid>();
                        break;
                    }
                }
                for (int i = 1; i < _allLevels.Length; i++)
                {
                    // error means the map is not in the LevelManager or the names dont match 
                    if (_currentTileMap.name == _allLevels[i]._levelPrefab.name)
                    {
                        _currentMapData = _allLevels[i];
                        _currentLevelIndex = i;
                        break;
                    }
                }
            }
        }
        _SetMapType();
        AudioManager._instance._PlayMusicInOrder(_currentMineType._levelMusic);

        if (transform)
            return;
#endif
        #endregion

        _currentLevelIndex = PlayerPrefs.GetInt(A.DataKey.currentLevelIndex, 1);
        _currentMapData = _allLevels[_currentLevelIndex];
        _SetMapType();
        AudioManager._instance._PlayMusicInOrder(_currentMineType._levelMusic);
    }
    private void _CheckSavedData()
    {
        #region Unity Editor
#if UNITY_EDITOR
        if (!_useSavedManager)
        {
            _currentLevelIndex = -1;
            return;
        }
#endif
        #endregion

        // reminder: if the _useSavedManager is false this wont be called!
        _currentLevelIndex = PlayerPrefs.GetInt(A.DataKey.lastUnfinishedLevel, A.DataKey.False);
        if (_currentLevelIndex != A.DataKey.False)
        {
            _isUsingSavedData = true;
            _currentMapData = _allLevels[_currentLevelIndex];
            _SetMapType();

            AudioManager._instance._PlayMusicInOrder(_currentMineType._levelMusic);
        }
    }
    public void _RestartTheLevel()
    {
        GetComponent<NextLevelController>()._StartLevel(_currentLevelIndex);
    }
    private void _SetLevelDescription()
    {
        var condition = _currentMapData._conditions[0];

        if (condition._winCondition == _AllWinTypes.reach_number)
        {
            _objectiveMlText._ChangeText(
                "Dynamic- objective reach number",
                condition._count,
                (int)condition._winNumber
            );
        }
        else // _AllWinTypes.fill_place
        {
            _objectiveMlText._ChangeText(
                "Dynamic- objective fill place",
                (int)condition._fillValue,
                (int)condition._extraValue
            );
        }
    }
    private IEnumerator _InitializeTileColorSystemDelayed()
    {
        // Wait a frame to ensure GridManager has finished its Start() and cached tile lists
        yield return null;

        // Initialize win condition tile color system if instance exists
        if (WinConditionTileColorManager._instance != null)
        {
            WinConditionTileColorManager._instance._InitializeTileColors();
        }
    }
    private void _SetMapType()
    {
        for (int i = 0; i < _allLevelTypes.Length; i++)
        {
            if (_allLevelTypes[i]._levelType == _currentMapData._levelType)
            {
                _currentMineType = _allLevelTypes[i];
                return;
            }
        }
    }
}
#region types
[System.Serializable]
public class _MapData
{
    public Grid _levelPrefab;
    public Sprite _mapBgSprite;
    public _AllLevelType _levelType;
    public Vector2 _mapSpawnOffset;
    public float _coinMultiplayer = 1;

    [Tooltip("_blockSpeedMultiplier")]
    public float _blockSpeedMultiplier = 1;

    [Tooltip("_cameraScale")]
    public float _cameraScale = 7.5f;
    public _WinConditions[] _conditions;
}
[System.Serializable]
public class _LevelTypeData
{
    public _AllLevelType _levelType;
    public BlockVisualData _visualData;
    public Sprite _menuSprite;
    [Tooltip("Extra Menu Sprite")]
    public Sprite _extraMenuSprite;
    public Color _menuTextColor;
    public AudioClip[] _levelMusic;
}
[System.Serializable]
public class _WinConditions
{
    [Tooltip("_winCondition")]
    public _AllWinTypes _winCondition;

    [Tooltip("_winNumber")]
    [ConditionalEnum(nameof(_winCondition), (int)_AllWinTypes.reach_number)]
    public _Numbers _winNumber;
    [Tooltip("_count")]
    [ConditionalEnum(nameof(_winCondition), (int)_AllWinTypes.reach_number)]
    public int _count;

    [Tooltip("_specificValue")]
    [ConditionalEnum(nameof(_winCondition), (int)_AllWinTypes.fill_place)]
    public bool _specificValue;
    [Tooltip("_fillValue")]
    [ConditionalEnum(nameof(_winCondition), (int)_AllWinTypes.fill_place)]
    [ConditionalField(nameof(_specificValue))]
    public _Numbers _fillValue;
    [Tooltip("_extraValue")]
    [ConditionalEnum(nameof(_winCondition), (int)_AllWinTypes.fill_place)]
    [ConditionalField(nameof(_specificValue))]
    public _Numbers _extraValue = _Numbers._0_;
}
#endregion
#region enums
public enum _AllWinTypes
{
    reach_number, fill_place
}
public enum _Numbers
{
    _0_ = 0, _2_1 = 2, _4_2 = 4, _8_3 = 8, _16_4 = 16, _32_5 = 32, _64_6 = 64
        , _128_7 = 128, _256_8 = 256, _512_9 = 512, _1024_10 = 1024, _2048_11 = 2048
        , Stone = 3
}
public enum _AllLevelType
{
    coal, iron, gold, diamond
}
#endregion

#region OutDated
/*
 *     private void _SetLevelDescriptionOld()
    {
        string iObjective = "";

        if (_currentMapData._conditions[0]._winCondition == _AllWinTypes.reach_number)
        {
            iObjective = string.Format("Create {0} block(s) with the value {1} to" +
                " complete the level."
                , _currentMapData._conditions[0]._count
                , (int)_currentMapData._conditions[0]._winNumber);
        }
        else // _AllWinTypes.fill_place
        {
            iObjective = string.Format("Fill all special tiles with a block of value" +
                " {0} or {1} to complete the level."
                , (int)_currentMapData._conditions[0]._fillValue
                , (int)_currentMapData._conditions[0]._extraValue);
        }
        _objectiveText.text = iObjective;
    }
 */
#endregion