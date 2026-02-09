using UnityEngine;
using System.Collections.Generic;

public class CustomAnimationController
{
    private Animator _animator;
    private AnimatorOverrideController _overrideController;
    private RuntimeAnimatorController _originalController;

    private AnimationClip _currentClip;
    private AnimationClip _baseClip;

    public CustomAnimationController(Animator iAnimator)
    {
        _animator = iAnimator;
        _originalController = _animator.runtimeAnimatorController;
        _CreateOverrideController();
    }

    private void _CreateOverrideController()
    {
        if (_originalController == null)
        {
            Debug.LogError("Animator has no RuntimeAnimatorController assigned!");
            return;
        }

        _overrideController = new AnimatorOverrideController(_originalController);
        _animator.runtimeAnimatorController = _overrideController;

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        _overrideController.GetOverrides(overrides);

        if (overrides.Count == 0)
        {
            Debug.LogError("AnimatorOverrideController has no overridable clips!");
            return;
        }

        _baseClip = overrides[0].Key;
    }

    public void _PlayAnimation(AnimationClip iClip)
    {
        if (iClip == null)
        {
            Debug.LogError("Animation clip is null!");
            return;
        }

        if (_overrideController == null)
        {
            Debug.LogError("Override controller is null!");
            return;
        }

        if (_currentClip == iClip) return;

        _currentClip = iClip;
        _overrideController[_baseClip] = iClip;

        _animator.Play(_baseClip.name, 0, 0f);
    }

    public void _PlayAnimation(AnimationClip iClip, float iNormalizedTime)
    {
        if (iClip == null)
        {
            Debug.LogError("Animation clip is null!");
            return;
        }

        if (_overrideController == null)
        {
            Debug.LogError("Override controller is null!");
            return;
        }

        _currentClip = iClip;
        _overrideController[_baseClip] = iClip;

        _animator.Play(_baseClip.name, 0, Mathf.Clamp01(iNormalizedTime));
    }

    public bool _IsAnimationPlaying(AnimationClip iClip)
    {
        if (iClip == null) return false;

        AnimatorClipInfo[] clipInfos = _animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfos.Length == 0) return false;

        return clipInfos[0].clip == iClip;
    }

    public float _GetCurrentAnimationTime()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.loop)
        {
            return stateInfo.normalizedTime % 1f;
        }

        return Mathf.Clamp01(stateInfo.normalizedTime);
    }

    public bool _IsAnimationFinished()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.loop) return false;

        return stateInfo.normalizedTime >= 1f;
    }

    public string _GetCurrentAnimationName(bool iPrint)
    {
        AnimatorClipInfo[] clipInfos = _animator.GetCurrentAnimatorClipInfo(0);

        if (clipInfos.Length == 0) return string.Empty;

        string name = clipInfos[0].clip.name;

        if (iPrint)
        {
            Debug.Log(name);
        }

        return name;
    }
}