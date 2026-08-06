using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class RabbitBehaviour : IResourceBehaviour
{
    private ProgressBarUI progressBar;
    private float cooldownTime;
    private CancellationTokenSource cts;

    public event Action OnRabbitRespawned;

    public RabbitBehaviour(ProgressBarUI progressBar, float cooldownTime)
    {
        this.progressBar = progressBar;
        this.cooldownTime = cooldownTime;
    }

    public void OnCollect(ResourceSource resource)
    {
        if (resource != null)
            OnCollect(resource.transform);
    }

    public void OnCollect(Transform target)
    {
        if (progressBar == null || target == null)
        {
            Debug.LogWarning($"RabbitBehaviour: progressBar или target = null!");
            return;
        }

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();

        progressBar.Show(target, 0f);
        UpdateProgressAsync(cts.Token).Forget();
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
        OnRabbitRespawned?.Invoke();
    }
}