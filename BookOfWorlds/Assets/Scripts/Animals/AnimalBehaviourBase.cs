using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public abstract class AnimalBehaviourBase : IResourceBehaviour
{
    protected ProgressBarUI progressBar;
    protected float cooldownTime;
    protected CancellationTokenSource cts;

    public event Action OnAnimalRespawned;

    public AnimalBehaviourBase(ProgressBarUI progressBar, float cooldownTime)
    {
        this.progressBar = progressBar;
        this.cooldownTime = cooldownTime;
    }

    public virtual void OnCollect(ResourceSource resource)
    {
        if (resource != null)
            OnCollect(resource.transform);
    }

    public virtual void OnCollect(Transform target)
    {
        if (progressBar == null || target == null) return;

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();

        progressBar.Show(target, 0f);
        UpdateProgressAsync(cts.Token).Forget();
    }

    public virtual void OnRespawn(ResourceSource resource)
    {
        cts?.Cancel();
        progressBar?.Hide();
    }

    protected virtual async UniTaskVoid UpdateProgressAsync(CancellationToken token)
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
        OnAnimalRespawned?.Invoke();
    }
}