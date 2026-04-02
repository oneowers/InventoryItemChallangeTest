using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SlotMachineView : MonoBehaviour
{
    [SerializeField]
    private GameObject panelRoot;

    [SerializeField]
    private List<ReelView> reels = new List<ReelView>();

    [SerializeField]
    private float firstReelDuration = 1.5f;

    [SerializeField]
    private float stopDelay = 0.5f;

    [SerializeField]
    private float durationStep = 0.5f;

    [SerializeField]
    private float jackpotFlashDuration = 0.15f;

    private readonly List<BaseItemSO> symbols = new List<BaseItemSO>();

    private void Awake()
    {
        if (this.panelRoot != null)
        {
            this.panelRoot.SetActive(false);
        }
    }

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

        for (int index = 0; index < this.reels.Count; index++)
        {
            this.reels[index].Initialize(this.symbols);
        }
    }

    public void Spin(SpinResult result, Action onComplete)
    {
        this.ShowPanel();
        this.StopAllCoroutines();
        this.StartCoroutine(this.SpinRoutine(result, onComplete));
    }

    private IEnumerator SpinRoutine(SpinResult result, Action onComplete)
    {
        int stoppedReels = 0;

        for (int index = 0; index < this.reels.Count; index++)
        {
            float duration = this.firstReelDuration + (this.durationStep * index);
            int reelIndex = index;
            this.reels[index].SpinAndStop(
                result.GetItem(index),
                duration,
                () => { stoppedReels++; });

            if (reelIndex < this.reels.Count - 1)
            {
                yield return new WaitForSeconds(this.stopDelay);
            }
        }

        while (stoppedReels < this.reels.Count)
        {
            yield return null;
        }

        if (result.GetItem(0) != null && result.GetItem(0).ItemType == ItemType.JackpotSymbol
            || result.GetItem(1) != null && result.GetItem(1).ItemType == ItemType.JackpotSymbol
            || result.GetItem(2) != null && result.GetItem(2).ItemType == ItemType.JackpotSymbol)
        {
            yield return this.PlayJackpotAnimation();
        }

        onComplete?.Invoke();
        this.HidePanel();
    }

    private IEnumerator PlayJackpotAnimation()
    {
        for (int flashIndex = 0; flashIndex < 4; flashIndex++)
        {
            for (int index = 0; index < this.reels.Count; index++)
            {
                this.reels[index].transform.localScale = Vector3.one * 1.08f;
            }

            yield return new WaitForSeconds(this.jackpotFlashDuration);

            for (int index = 0; index < this.reels.Count; index++)
            {
                this.reels[index].transform.localScale = Vector3.one;
            }

            yield return new WaitForSeconds(this.jackpotFlashDuration);
        }
    }

    private void ShowPanel()
    {
        if (this.panelRoot != null)
        {
            this.panelRoot.SetActive(true);
        }
    }

    private void HidePanel()
    {
        if (this.panelRoot != null)
        {
            this.panelRoot.SetActive(false);
        }
    }
}
