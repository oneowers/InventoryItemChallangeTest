using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public sealed class ReelView : MonoBehaviour
{
    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private RectTransform content;

    [SerializeField]
    private List<Image> symbolImages = new List<Image>();

    [SerializeField]
    private float symbolStep = 96f;

    [SerializeField]
    private float maxSpeed = 1400f;

    private readonly List<BaseItemSO> symbols = new List<BaseItemSO>();

    public void Initialize(IReadOnlyList<BaseItemSO> availableSymbols)
    {
        this.symbols.Clear();

        for (int index = 0; index < availableSymbols.Count; index++)
        {
            if (availableSymbols[index] != null)
            {
                this.symbols.Add(availableSymbols[index]);
            }
        }

        this.RenderRandomStrip();
    }

    public void SpinAndStop(BaseItemSO targetItem, float duration, Action onStop)
    {
        this.StopAllCoroutines();
        this.StartCoroutine(this.SpinAndStopRoutine(targetItem, duration, onStop));
    }

    private IEnumerator SpinAndStopRoutine(BaseItemSO targetItem, float duration, Action onStop)
    {
        if (this.symbols.Count == 0 || this.symbolImages.Count == 0)
        {
            onStop?.Invoke();
            yield break;
        }

        float elapsed = 0f;
        float offset = 0f;

        while (elapsed < duration)
        {
            float normalized = elapsed / duration;
            float speed = Mathf.Lerp(this.maxSpeed, this.symbolStep * 2f, normalized * normalized);
            offset += speed * Time.deltaTime;

            while (offset >= this.symbolStep)
            {
                offset -= this.symbolStep;
                this.ShiftSymbols();
            }

            this.content.anchoredPosition = new Vector2(this.content.anchoredPosition.x, -offset);
            elapsed += Time.deltaTime;
            yield return null;
        }

        this.content.anchoredPosition = new Vector2(this.content.anchoredPosition.x, 0f);
        this.SetStoppedStrip(targetItem);
        onStop?.Invoke();
    }

    private void RenderRandomStrip()
    {
        for (int index = 0; index < this.symbolImages.Count; index++)
        {
            BaseItemSO item = this.symbols[Random.Range(0, this.symbols.Count)];
            this.symbolImages[index].sprite = item.Icon;
            this.symbolImages[index].color = Color.white;
        }
    }

    private void ShiftSymbols()
    {
        for (int index = this.symbolImages.Count - 1; index > 0; index--)
        {
            this.symbolImages[index].sprite = this.symbolImages[index - 1].sprite;
            this.symbolImages[index].color = this.symbolImages[index - 1].color;
        }

        BaseItemSO item = this.symbols[Random.Range(0, this.symbols.Count)];
        this.symbolImages[0].sprite = item.Icon;
        this.symbolImages[0].color = Color.white;
    }

    private void SetStoppedStrip(BaseItemSO targetItem)
    {
        int centerIndex = this.symbolImages.Count / 2;

        for (int index = 0; index < this.symbolImages.Count; index++)
        {
            BaseItemSO item = index == centerIndex ? targetItem : this.symbols[Random.Range(0, this.symbols.Count)];
            this.symbolImages[index].sprite = item != null ? item.Icon : null;
            this.symbolImages[index].color = item != null ? Color.white : new Color(1f, 1f, 1f, 0f);
        }

        if (this.scrollRect != null)
        {
            this.scrollRect.verticalNormalizedPosition = 0.5f;
        }
    }
}
