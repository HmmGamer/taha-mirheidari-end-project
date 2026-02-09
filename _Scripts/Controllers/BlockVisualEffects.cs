using UnityEngine;
public class BlockVisualEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] SpriteRenderer _shineBg;
    [SerializeField] IceTileFreeze _freezeEffect;
    [SerializeField] BurnTrigger _burnEffect;
    [SerializeField] Animator _animator;

    // we used this because of a bug in animation system, can be changed later
    CustomAnimationController _animationController;
    bool _isShining;
    bool _isFrozen;

    #region Starter
    private void Awake()
    {
        _InitializeAnimationSystem();
    }
    private void OnEnable()
    {
        _isShining = false;
        _isFrozen = false;

        _animationController._PlayAnimation(BlockAnimationClips._instance._spawnClip);

        _burnEffect._onBurnFinished += _A_DestroyBlock;
    }
    private void OnDisable()
    {
        _burnEffect._onBurnFinished -= _A_DestroyBlock;
    }
    private void _InitializeAnimationSystem()
    {
        #region Editor Null Check
#if UNITY_EDITOR
        if (BlockAnimationClips._instance == null)
        {
            Debug.LogError("Add BlockAnimationClips Script to the scene and fill the values");
            return;
        }
#endif
        #endregion

        _animationController = new CustomAnimationController(_animator);
    }
    #endregion
    #region Color Management
    private void _RestoreOriginalColor()
    {
        // dont ask me why, idk, but dont change it as there is an unknown bug
        _shineBg.enabled = false;
    }
    #endregion
    #region Animation Control
    public void _StartSelectionShine()
    {
        if (_isShining) return;

        _isShining = true;
        _shineBg.enabled = true;
        _animationController._PlayAnimation(BlockAnimationClips._instance._ShineClip);
    }
    public void _StopSelectionShine()
    {
        if (!_isShining) return;
        _isShining = false;

        if (!_isFrozen)
            _PlayIdleAnimation();

        _RestoreOriginalColor();
    }
    public void _PlayDestroyAnimation()
    {
        _animationController._PlayAnimation(BlockAnimationClips._instance._DestroyClip);
    }
    public void _PlayBurnAnimation()
    {
        _animationController._PlayAnimation(BlockAnimationClips._instance._burnClip);
        _burnEffect._TriggerBurn();
    }
    public void _PlayFreezeAnimation()
    {
        _isFrozen = true;
        _freezeEffect._StartFreeze();
        _animationController._PlayAnimation(BlockAnimationClips._instance._FreezeClip);
        AudioManager._instance._PlayAudio(_AudioType.SFX1, SavedSounds.Powerups_Freez, true);
    }
    public void _PlayMergeAnimation()
    {
        _animationController._PlayAnimation(BlockAnimationClips._instance._mergeClip);
    }
    public void _PlayIdleAnimation()
    {
        _animationController._PlayAnimation(BlockAnimationClips._instance._IdleClip);
    }
    public void _RemoveFreezeEffect()
    {
        _isFrozen = false;
        _freezeEffect._UnFreeze();

        if (_isShining)
        {
            _animationController._PlayAnimation(BlockAnimationClips._instance._ShineClip);
        }
        else
        {
            _PlayIdleAnimation();
        }
        AudioManager._instance._PlayAudio(_AudioType.SFX1, SavedSounds.Powerups_UnFreez, true);
    }
    #endregion
    #region Animation Events
    public void _A_OnIdleAnimationStart()
    {
        _RestoreOriginalColor();
    }
    public void _A_DestroyBlock()
    {
        PoolManager._instance._Despawn(gameObject);
    }
    #endregion
}