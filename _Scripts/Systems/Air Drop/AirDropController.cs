using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AirDropController : MonoBehaviour
{
    [SerializeField] Animator _anim;
    [SerializeField] Button _button;
    UnityAction _reward;

    private void Start()
    {
        _button.onClick.AddListener(_B_CollectAirDrop);
    }
    public void _SetRewardAndActivate(UnityAction iReward)
    {
        gameObject.SetActive(true);
        _reward = iReward;
    }
    public void _B_CollectAirDrop()
    {
        AudioManager._instance._PlayAudio(_AudioType.SFX2, SavedSounds.AirDrop_Collect);

        _anim.SetTrigger(A.Anim.t_collectAirdrop);

        if (_reward != null)
        {
            _reward.Invoke();
            _reward = null;
        }

        AirdropMover._instance._SetCollectRewardBool();
    }

    /// <summary>
    /// disable's the reward's box after collection animation is finished
    /// </summary>
    public void _A_DisableReward()
    {
        gameObject.SetActive(false);
    }
}
