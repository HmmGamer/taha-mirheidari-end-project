using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinLearpingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform _coinStartPoint;
    [SerializeField] private RectTransform _coinTargetPoint;
    [SerializeField] private Image _coinPrefab;
    [SerializeField] private RectTransform _canvasRect;

    [Header("Settings")]
    [SerializeField] private float _moveDuration = 0.8f;
    [SerializeField] private float _spreadRadius = 80f;
    [SerializeField] private int _coinAmount = 10;
    [SerializeField] private float _spawnDelay = 0.05f;

    private Queue<Image> _coinPool = new Queue<Image>();

    private void Awake()
    {
        _PreloadCoins();
    }

    private void _PreloadCoins()
    {
        if (_coinPrefab == null || _canvasRect == null)
        {
            Debug.LogWarning("Missing prefab or canvas reference.");
            return;
        }

        for (int i = 0; i < _coinAmount; i++)
        {
            Image coin = Instantiate(_coinPrefab, _canvasRect);
            coin.gameObject.SetActive(false);
            _coinPool.Enqueue(coin);
        }
    }

    [CreateMonoButton("Fly Coins")]
    public void _StartCoinTransfer()
    {
        if (_coinStartPoint == null || _coinTargetPoint == null || _coinPrefab == null)
        {
            Debug.LogWarning("Coin transfer missing references.");
            return;
        }

        for (int i = 0; i < _coinAmount; i++)
        {
            if (_coinPool.Count == 0)
            {
                Debug.LogWarning("Coin pool is empty.");
                return;
            }

            Image coin = _coinPool.Dequeue();
            coin.gameObject.SetActive(true);

            Vector2 startPos = _coinStartPoint.anchoredPosition + Random.insideUnitCircle * _spreadRadius;
            coin.rectTransform.anchoredPosition = startPos;
            coin.rectTransform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);
            float delay = i * _spawnDelay;

            coin.rectTransform.DOAnchorPos(_coinTargetPoint.anchoredPosition, _moveDuration)
                .SetEase(Ease.InOutQuad)
                .SetDelay(delay)
                .OnComplete(() =>
                {
                    coin.gameObject.SetActive(false);
                    _coinPool.Enqueue(coin);
                });

            coin.rectTransform.DOScale(0.6f, _moveDuration)
                .SetEase(Ease.InOutQuad)
                .SetDelay(delay);
        }
    }
}
