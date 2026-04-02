using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    public sealed class TargetShootingUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;

        [Header("Target")]
        [SerializeField] private RectTransform targetCircle;
        [SerializeField] private float radiusOverride;

        [Header("Bullet Prefab")]
        [SerializeField] private GameObject bulletPrefab;

        [Header("Coin Popup Prefab")]
        [SerializeField] private GameObject coinPopupPrefab;
        [SerializeField] private float popupLifetime = 1f;

        [Header("Settings")]
        [SerializeField] private float hideDelay = 1f;

        [Header("Coin Thresholds")]
        [SerializeField] private float tier1Threshold = 0.10f;
        [SerializeField] private float tier2Threshold = 0.30f;
        [SerializeField] private float tier3Threshold = 0.60f;

        [Header("Coin Rewards")]
        [SerializeField] private int tier1Coins = 100;
        [SerializeField] private int tier2Coins = 75;
        [SerializeField] private int tier3Coins = 50;
        [SerializeField] private int tier4Coins = 10;

        private GameObject currentBullet;
        private GameObject currentPopup;

        private void Awake()
        {
            if (this.panelRoot != null)
                this.panelRoot.SetActive(false);

            if (targetCircle == null)
                Debug.LogError("[TargetShootingUI] targetCircle is not assigned!", this);

            if (bulletPrefab == null)
                Debug.LogError("[TargetShootingUI] bulletPrefab is not assigned!", this);
        }

        public void ShowAndShoot(Action<int> onCoinsEarned = null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowShootAndHideRoutine(onCoinsEarned));
        }

        public void Shoot(Action<int> onCoinsEarned = null)
        {
            if (targetCircle == null || bulletPrefab == null) return;

            float radius = GetRadius();
            Vector2 localPosition = RandomPointInCircle(radius);
            int coins = SpawnBullet(localPosition, radius);

            onCoinsEarned?.Invoke(coins);
        }

        private IEnumerator ShowShootAndHideRoutine(Action<int> onCoinsEarned)
        {
            ShowPanel();
            yield return null;
            Shoot(onCoinsEarned);
            yield return new WaitForSeconds(hideDelay);
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
            return radiusOverride > 0f
                ? radiusOverride
                : targetCircle.rect.width * 0.5f;
        }

        private static Vector2 RandomPointInCircle(float radius)
        {
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float distance = Mathf.Sqrt(UnityEngine.Random.value) * radius;
            return new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
        }

        private int SpawnBullet(Vector2 localPosition, float radius)
        {
            if (currentBullet != null)
            {
                Destroy(currentBullet);
                currentBullet = null;
            }

            if (currentPopup != null)
            {
                Destroy(currentPopup);
                currentPopup = null;
            }

            currentBullet = Instantiate(bulletPrefab, targetCircle);

            RectTransform bulletRect = currentBullet.GetComponent<RectTransform>();
            if (bulletRect != null)
                bulletRect.anchoredPosition = localPosition;
            else
                currentBullet.transform.localPosition = localPosition;

            float distanceFraction = localPosition.magnitude / radius;
            int coins = CalculateCoins(distanceFraction);

            Debug.Log($"[TargetShootingUI] Hit: {localPosition:F1} | {distanceFraction * 100f:F1}% | +{coins} coins");

            SpawnCoinPopup(localPosition, coins);

            return coins;
        }

        private void SpawnCoinPopup(Vector2 localPosition, int coins)
        {
            if (coinPopupPrefab == null) return;

            currentPopup = Instantiate(coinPopupPrefab, targetCircle);

            RectTransform popupRect = currentPopup.GetComponent<RectTransform>();
            if (popupRect != null)
                popupRect.anchoredPosition = localPosition + Vector2.up * 30f;
            else
                currentPopup.transform.localPosition = new Vector3(localPosition.x, localPosition.y + 30f, 0f);

            TextMeshProUGUI label = currentPopup.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = $"+{coins}";

            Destroy(currentPopup, popupLifetime);
        }

        private int CalculateCoins(float fraction)
        {
            if (fraction <= tier1Threshold) return tier1Coins;
            if (fraction <= tier2Threshold) return tier2Coins;
            if (fraction <= tier3Threshold) return tier3Coins;
            return tier4Coins;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (targetCircle == null) return;

            float r = GetRadius();
            Vector3 center = targetCircle.position;

            void DrawCircle(float radius, Color color)
            {
                Gizmos.color = color;
                const int segments = 64;
                float step = Mathf.PI * 2f / segments;
                for (int i = 0; i < segments; i++)
                {
                    float a1 = step * i, a2 = step * (i + 1);
                    Gizmos.DrawLine(
                        center + new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, 0f),
                        center + new Vector3(Mathf.Cos(a2) * radius, Mathf.Sin(a2) * radius, 0f)
                    );
                }
            }

            DrawCircle(r, Color.white);
            DrawCircle(r * tier3Threshold, Color.yellow);
            DrawCircle(r * tier2Threshold, Color.cyan);
            DrawCircle(r * tier1Threshold, Color.green);
        }
#endif
    }
}
