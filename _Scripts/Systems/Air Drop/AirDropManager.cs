using UnityEngine;
using UnityEngine.Events;

public class AirdropManager : Singleton_Abs<AirdropManager>
{
    [Header("General Settings")]
    [SerializeField] int _minMovesToDrop = 70;
    [SerializeField] int _maxMovesToDrop = 110;

    [Header("Attachments")]
    [SerializeField] AirDropData _data;
    [SerializeField] AirDropController _airDropController;
    [SerializeField] InventoryData _invData;
    [SerializeField] RectTransform _spawnArea;
    [SerializeField] RectTransform _endArea;

    [Header("Optional Events")]
    [SerializeField] _Events _events;

    int _remainingMovesForDrop;

    private void Start()
    {
        _SetNewRandomRemainingMoves();
    }
    private void OnEnable()
    {
        GridManager._onBoardChanged.AddListener(_PerformMove);
    }
    private void OnDisable()
    {
        GridManager._onBoardChanged.RemoveListener(_PerformMove);
    }

    private void _PerformMove()
    {
        _remainingMovesForDrop--;
        if (_remainingMovesForDrop <= 0)
        {
            _SpawnAirDrop();
            _SetNewRandomRemainingMoves();
        }
    }
    private void _SetNewRandomRemainingMoves()
    {
        _remainingMovesForDrop = Random.Range(_minMovesToDrop, _maxMovesToDrop);
    }
    private void _SpawnAirDrop()
    {
        _events._onDropStartEvent.Invoke();

        Vector2 iStart = _GetSpawnPos();
        Vector2 iEnd = _GetEndPos();

        _airDropController._SetRewardAndActivate(_CollectReward);
        
        AirdropMover._instance._StartSequence(iStart, iEnd);
    }
    private Vector2 _GetSpawnPos()
    {
        Vector3[] iCorners = new Vector3[4];
        _spawnArea.GetWorldCorners(iCorners);
        float iX = Random.Range(iCorners[0].x, iCorners[2].x);
        float iY = iCorners[1].y;
        return new Vector2(iX, iY);
    }
    private Vector2 _GetEndPos()
    {
        Vector3[] iCorners = new Vector3[4];
        _endArea.GetWorldCorners(iCorners);
        float iX = Random.Range(iCorners[0].x, iCorners[2].x);
        float iY = Random.Range(iCorners[0].y, iCorners[1].y);
        return new Vector2(iX, iY);
    }
    private void _CollectReward()
    {
        AirDropData._AirDropChance iReward = _data._GetRandomAirDrop();
        if (iReward._reward == AirDropData._AirdropRewards.coin)
            _invData._AddCoin(iReward._coinsCount);
        else if (iReward._reward == AirDropData._AirdropRewards.under8)
            PowerUpManager._instance._AddExtraPowerUp(_ExPuTypes.Under_8);
        else if (iReward._reward == AirDropData._AirdropRewards.freeze)
            PowerUpManager._instance._AddExtraPowerUp(_ExPuTypes.Freeze);
        else if (iReward._reward == AirDropData._AirdropRewards.specific)
            PowerUpManager._instance._AddExtraPowerUp(_ExPuTypes.Specific);

        _events._onDropStartEvent.Invoke();

        AirDropMsgBox._instance._ShowNewMsg(iReward); // visual only
    }

    [System.Serializable]
    public class _Events
    {
        public UnityEvent _onDropStartEvent;
        public UnityEvent _onCollectEvent;
    }
}