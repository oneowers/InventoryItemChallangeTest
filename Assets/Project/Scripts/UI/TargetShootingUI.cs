using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// 2D UI Target Shooting:
    ///  - Spawns a bullet sprite at a random position inside a circular target
    ///  - Destroys the previous bullet before each new shot
    ///  - Shows a coin popup at the hit position
    ///  - Awards coins based on proximity to center (closer = more)
    /// </summary>
    public sealed class TargetShootingUI : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────────────

        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;            // Скрыт при старте (как SlotMachineView)

        [Header("Target")]
        [SerializeField] private RectTransform _targetCircle;     // Circle image RectTransform
        [SerializeField] private float          _radiusOverride;   // 0 = half of _targetCircle.rect.width

        [Header("Bullet Prefab")]
        [SerializeField] private GameObject _bulletPrefab;        // UI Image, pivot = center

        [Header("Coin Popup Prefab")]
        [SerializeField] private GameObject _coinPopupPrefab;     // Prefab с TextMeshProUGUI «+X»
        [SerializeField] private float      _popupLifetime = 1f;  // Сколько секунд показывать попап

        [Header("Settings")]
        [SerializeField] private float _hideDelay = 1f;           // Секунд до скрытия панели

        [Header("Coin Thresholds (0–1 fraction of radius)")]
        [SerializeField] private float _tier1Threshold = 0.10f;   // Bulls-eye
        [SerializeField] private float _tier2Threshold = 0.30f;
        [SerializeField] private float _tier3Threshold = 0.60f;

        [Header("Coin Rewards")]
        [SerializeField] private int _tier1Coins = 100;
        [SerializeField] private int _tier2Coins = 75;
        [SerializeField] private int _tier3Coins = 50;
        [SerializeField] private int _tier4Coins = 10;

        // ─── State ───────────────────────────────────────────────────────────────

        private GameObject _currentBullet;    // Текущая пуля — удаляем перед новым выстрелом
        private GameObject _currentPopup;     // Текущий попап монет

        // ─── Unity Lifecycle ─────────────────────────────────────────────────────

        private void Awake()
        {
            if (this.panelRoot != null)
                this.panelRoot.SetActive(false);

            if (_targetCircle == null)
                Debug.LogError("[TargetShootingUI] _targetCircle is not assigned!", this);

            if (_bulletPrefab == null)
                Debug.LogError("[TargetShootingUI] _bulletPrefab is not assigned!", this);
        }

        // ─── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Показать панель → удалить старую пулю → выстрел → показать монеты → скрыть.
        /// onCoinsEarned вызывается с количеством монет (для обновления модели).
        /// </summary>
        public void ShowAndShoot(Action<int> onCoinsEarned = null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowShootAndHideRoutine(onCoinsEarned));
        }

        /// <summary>Только выстрел (без управления панелью).</summary>
        public void Shoot(Action<int> onCoinsEarned = null)
        {
            if (_targetCircle == null || _bulletPrefab == null) return;

            float   radius        = GetRadius();
            Vector2 localPosition = RandomPointInCircle(radius);
            int     coins         = SpawnBullet(localPosition, radius);

            onCoinsEarned?.Invoke(coins);
        }

        // ─── Private ─────────────────────────────────────────────────────────────

        private IEnumerator ShowShootAndHideRoutine(Action<int> onCoinsEarned)
        {
            ShowPanel();
            yield return null;                      // один кадр — панель успевает появиться
            Shoot(onCoinsEarned);
            yield return new WaitForSeconds(_hideDelay);
            HidePanel();
        }

        private void ShowPanel()
        {
            if (this.panelRoot != null)
                this.panelRoot.SetActive(true);
        }

        private void HidePanel()
        {
            if (this.panelRoot != null)
                this.panelRoot.SetActive(false);
        }

        private float GetRadius()
        {
            return _radiusOverride > 0f
                ? _radiusOverride
                : _targetCircle.rect.width * 0.5f;
        }

        /// <summary>Uniform distribution inside circle (sqrt trick).</summary>
        private static Vector2 RandomPointInCircle(float radius)
        {
            float angle    = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float distance = Mathf.Sqrt(UnityEngine.Random.value) * radius;
            return new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
        }

        /// <summary>Удаляет старую пулю, спавнит новую, показывает попап монет. Возвращает монеты.</summary>
        private int SpawnBullet(Vector2 localPosition, float radius)
        {
            // ── Удалить предыдущую пулю ───────────────────────────────────────────
            if (_currentBullet != null)
            {
                Destroy(_currentBullet);
                _currentBullet = null;
            }

            // ── Удалить предыдущий попап ───────────────────────────────────────────
            if (_currentPopup != null)
            {
                Destroy(_currentPopup);
                _currentPopup = null;
            }

            // ── Спавн пули ────────────────────────────────────────────────────────
            _currentBullet = Instantiate(_bulletPrefab, _targetCircle);

            RectTransform bulletRect = _currentBullet.GetComponent<RectTransform>();
            if (bulletRect != null)
                bulletRect.anchoredPosition = localPosition;
            else
                _currentBullet.transform.localPosition = localPosition;

            // ── Монеты ────────────────────────────────────────────────────────────
            float distanceFraction = localPosition.magnitude / radius; // 0 = center, 1 = edge
            int   coins            = CalculateCoins(distanceFraction);

            Debug.Log(
                $"[TargetShootingUI] Hit at {localPosition:F1} | " +
                $"Distance: {distanceFraction * 100f:F1}% | " +
                $"Coins: +{coins}"
            );

            // ── Попап монет ───────────────────────────────────────────────────────
            SpawnCoinPopup(localPosition, coins);

            return coins;
        }

        private void SpawnCoinPopup(Vector2 localPosition, int coins)
        {
            if (_coinPopupPrefab == null) return;

            // Спавним попап чуть выше пули
            _currentPopup = Instantiate(_coinPopupPrefab, _targetCircle);

            RectTransform popupRect = _currentPopup.GetComponent<RectTransform>();
            if (popupRect != null)
                popupRect.anchoredPosition = localPosition + Vector2.up * 30f;
            else
                _currentPopup.transform.localPosition = new Vector3(localPosition.x, localPosition.y + 30f, 0f);

            // Задать текст
            TextMeshProUGUI label = _currentPopup.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = $"+{coins}";

            // Автоудаление
            Destroy(_currentPopup, _popupLifetime);
        }

        private int CalculateCoins(float fraction)
        {
            if (fraction <= _tier1Threshold) return _tier1Coins;
            if (fraction <= _tier2Threshold) return _tier2Coins;
            if (fraction <= _tier3Threshold) return _tier3Coins;
            return _tier4Coins;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_targetCircle == null) return;

            float   r      = GetRadius();
            Vector3 center = _targetCircle.position;

            void DrawCircle(float radius, Color color)
            {
                Gizmos.color = color;
                const int segments = 64;
                float     step     = Mathf.PI * 2f / segments;
                for (int i = 0; i < segments; i++)
                {
                    float a1 = step * i, a2 = step * (i + 1);
                    Gizmos.DrawLine(
                        center + new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, 0f),
                        center + new Vector3(Mathf.Cos(a2) * radius, Mathf.Sin(a2) * radius, 0f)
                    );
                }
            }

            DrawCircle(r,                   Color.white);
            DrawCircle(r * _tier3Threshold, Color.yellow);
            DrawCircle(r * _tier2Threshold, Color.cyan);
            DrawCircle(r * _tier1Threshold, Color.green);
        }
#endif
    }
}
