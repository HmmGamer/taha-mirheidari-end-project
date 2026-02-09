using UnityEngine;

public class BlockAnimationClips : Singleton_Abs<BlockAnimationClips>
{
    [Header("Animation Clips")]
    public AnimationClip _IdleClip;
    public AnimationClip _ShineClip;
    public AnimationClip _DestroyClip;
    public AnimationClip _FreezeClip;
    public AnimationClip _spawnClip;
    public AnimationClip _mergeClip;
    public AnimationClip _burnClip;

    #region Editor Null Check
#if UNITY_EDITOR
    private void Start()
    {
        if (!(_IdleClip && _ShineClip && _DestroyClip && _FreezeClip 
            && _spawnClip && _burnClip))
        {
            Debug.LogError("BlockAnimationClips animations have a null value", gameObject);
        }
    }
#endif
    #endregion
}