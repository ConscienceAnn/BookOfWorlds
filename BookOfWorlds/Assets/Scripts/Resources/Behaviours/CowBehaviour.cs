using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class CowBehaviour : IResourceBehaviour
{
    private ProgressBarUI progressBar;
    private float cooldownTime;
    private CancellationTokenSource cts;

    public event Action OnCowRespawned;

    public CowBehaviour(ProgressBarUI progressBar, float cooldownTime)
    {
        this.progressBar = progressBar;
        this.cooldownTime = cooldownTime;
    }

    public void OnCollect(ResourceSource resource)
    {
        // Если есть ResourceSource — используем его Transform
        if (resource != null)
        {
            OnCollect(resource.transform);
        }
        else
        {
            Debug.LogWarning("CowBehaviour: resource is NULL, cannot show progress bar");
        }
    }

    public void OnCollect(Transform target)
    {
        Debug.Log($"=== CowBehaviour.OnCollect(Transform) ===");
        Debug.Log($"  - target: {(target != null ? target.name : "NULL")}");
        Debug.Log($"  - progressBar: {(progressBar != null ? "ЕСТЬ" : "НЕТ")}");

        if (progressBar == null)
        {
            Debug.LogError("CowBehaviour: progressBar is NULL!");
            return;
        }

        if (target == null)
        {
            Debug.LogError("CowBehaviour: target is NULL!");
            return;
        }

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();

        Debug.Log($"  - progressBar.gameObject.activeSelf ДО Show: {progressBar.gameObject.activeSelf}");

        progressBar.Show(target, 0f);

        Debug.Log($"  - progressBar.gameObject.activeSelf ПОСЛЕ Show: {progressBar.gameObject.activeSelf}");

        UpdateProgressAsync(cts.Token).Forget();

        Debug.Log("=== CowBehaviour.OnCollect(Transform) END ===");
    }

    public void OnRespawn(ResourceSource resource)
    {
        cts?.Cancel();
        progressBar?.Hide();
    }

    private async UniTaskVoid UpdateProgressAsync(CancellationToken token)
    {
        float elapsed = 0f;

        while (elapsed < cooldownTime)
        {
            if (token.IsCancellationRequested) return;

            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / cooldownTime);

            progressBar?.SetProgress(progress);

            await UniTask.Yield(token);
        }

        progressBar?.Hide();
        OnCowRespawned?.Invoke();
    }
}