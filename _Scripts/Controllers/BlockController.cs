using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlockController : MonoBehaviour
{
    const int _UPDATE_SPAWNS_THERHOLD = 256; // 2^8
    const float _DEFAULT_BLOCK_SPEED = 10;

    public int _value;
    public bool _isBlockMergeable = true;

    public BlockVisualData _visualData;
    public BlockVisualEffects _visualEffects;
    [HideInInspector] public float _speedMultiplayer = 1;
    [SerializeField] SpriteRenderer _background;

    public bool _isMoving { get; private set; }
    [HideInInspector] public bool _canSelect;

    private Vector3Int _cellPosition;
    private bool _hasMerged = true; // to avoid double merging
    private bool _mapHasVoid; // to avoid void check if map doesn't have one
    private int _moveCooldown = 0; // freeze cooldown

    #region Unity Methods
    private void Start()
    {
        _mapHasVoid = GridManager._instance._mapHasVoid;
    }
    private void OnMouseDown()
    {
        if (!_canSelect) return;
        PowerUpManager._instance._OnBlockClicked(this);
    }
    private void OnEnable()
    {
        _isBlockMergeable = true;
        _isMoving = true;
        _hasMerged = true;
        _moveCooldown = 0;
        _cellPosition = GridManager._instance._GetCellPosition(transform.position);
        GridManager._instance._RegisterBlock(_cellPosition, this);
        transform.position = GridManager._instance._GetWorldPosition(_cellPosition);
    }
    private void OnDisable()
    {
        if (!_isBlockMergeable)
            SpawnManager._instance._AddStoneToSpawnList();
    }
    #endregion
    #region Movement
    public void _SetFrozen(int iMoves)
    {
        _moveCooldown = -iMoves;
        _visualEffects._PlayFreezeAnimation();
    }
    private void _CheckFreezeStatus()
    {
        if (_moveCooldown == 0)
        {
            _visualEffects._RemoveFreezeEffect();
        }
    }
    public bool _CanMove()
    {
        if (_moveCooldown < 0)
        {
            _moveCooldown++;
            _CheckFreezeStatus();
            return false;
        }
        return true;
    }
    public IEnumerator _TryMove(Vector3Int iDirection)
    {
        if (!_CanMove()) yield break;

        _isMoving = true;
        GridManager grid = GridManager._instance;

        while (true)
        {
            Vector3Int target = _cellPosition + iDirection;

            if (!grid._IsValidMove(_cellPosition, target, iDirection))
                break;

            if (grid._IsCellOccupied(target))
            {
                BlockController other = grid._GetBlockAt(target);
                if (other != null && other._value == _value && _isBlockMergeable &&
                    !other._HasMergedThisTurn() && !_hasMerged)
                {
                    grid._UnregisterBlock(_cellPosition);
                    _visualEffects._PlayDestroyAnimation();
                    other._PreformMerge();
                    other._SetMergedFlag();
                    GridManager._instance._hasBoardChangedThisTurn = true;
                }
                break;
            }

            grid._MoveBlock(_cellPosition, target);
            _cellPosition = target;
            GridManager._instance._hasBoardChangedThisTurn = true;

            yield return _AnimateTo(grid._GetWorldPosition(_cellPosition));

            if (_mapHasVoid && grid._IsVoidCell(_cellPosition))
            {
                grid._UnregisterBlock(_cellPosition);
                _visualEffects._PlayBurnAnimation();
                _isMoving = false;
                AudioManager._instance._PlayAudio(_AudioType.SFX1, SavedSounds.Void_Destroy, true);
                yield break;
            }
        }

        _isMoving = false;
    }
    #endregion
    #region Functional
    public void _ResetMergeFlag()
    {
        _hasMerged = false;
    }
    public void _ChangeValue(int iValue)
    {
        _value = iValue;
        _UpdateVisual();
    }
    private IEnumerator _AnimateTo(Vector3 iTarget)
    {
        Vector3 start = transform.position;
        float distance = Vector3.Distance(start, iTarget);
        float duration = distance / (_DEFAULT_BLOCK_SPEED * _speedMultiplayer);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, iTarget, elapsed / duration);
            yield return null;
        }
        transform.position = iTarget;
    }
    private void _SetMergedFlag()
    {
        _hasMerged = true;
    }
    private void _UpdateVisual()
    {
        #region Editor Null Detection
#if UNITY_EDITOR
        if (_visualEffects == null)
        {
            Debug.LogError("you need to assign _visualEffects in BlockPrefab");
            return;
        }
#endif
        #endregion

        if (!_isBlockMergeable)
            return;

        _background.sprite = _visualData._GetSpriteFor(_value);
    }
    public void _SetBlockToStone()
    {
        _isBlockMergeable = false;
        _background.sprite = _visualData._GetStoneSprite();
        _value = (int)_Numbers.Stone;
    }
    #endregion
    #region Hard Coded Part
    public void _PreformMerge()
    {
        _value *= 2;
        _UpdateVisual();
        _CheckValue();

        AudioManager._instance._AddToMergeCounter();
        _visualEffects._PlayMergeAnimation();

        ScoreManager._instance._AddScore(_value);
    }
    private void _CheckValue()
    {
        if (_value == _UPDATE_SPAWNS_THERHOLD)
            SpawnManager._instance._UpdateSpawnedBlocks();
    }
    #endregion
    #region Getters
    public Vector3Int _GetCellPosition()
    {
        return _cellPosition;
    }
    public bool _HasMergedThisTurn()
    {
        return _hasMerged || !_isBlockMergeable;
    }
    public InGridBlockSaveData _GetBlockInfo()
    {
        InGridBlockSaveData blockInfo = new InGridBlockSaveData();
        blockInfo.position = _cellPosition;
        blockInfo.value = _value;
        blockInfo.blockType = _isBlockMergeable ? 0 : 1; // 0 = normal block, 1 = stone

        return blockInfo;
    }
    #endregion
}