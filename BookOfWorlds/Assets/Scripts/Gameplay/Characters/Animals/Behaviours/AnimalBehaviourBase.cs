using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public abstract class AnimalBehaviourBase : IResourceBehaviour
{
    protected ProgressBarUI progressBar;
    protected float cooldownTime;
    protected CancellationTokenSource cts;
    protected float currentCooldownTime;

    public event Action OnAnimalRespawned;

    public AnimalBehaviourBase(ProgressBarUI progressBar, float cooldownTime)
    {
        this.progressBar = progressBar;
        this.cooldownTime = cooldownTime;

        // Защита от нулевого множителя
        float multiplier = Mathf.Max(0.1f, RespawnSettings.Multiplier);
        this.currentCooldownTime = cooldownTime / multiplier;

        EventBus.OnRespawnMultiplierChanged += OnRespawnMultiplierChanged;
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

        // Защита от нулевого множителя
        float multiplier = Mathf.Max(0.1f, RespawnSettings.Multiplier);
        currentCooldownTime = cooldownTime / multiplier;

        // Дополнительная защита: если время меньше 0.1 секунды, устанавливаем минимум
        if (currentCooldownTime < 0.1f)
            currentCooldownTime = 0.1f;

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
        float currentTime = currentCooldownTime;

        // Защита от нулевого времени
        if (currentTime <= 0)
            currentTime = 0.1f;

        while (elapsed < currentTime)
        {
            if (token.IsCancellationRequested) return;

            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / currentTime);
            progressBar?.SetProgress(progress);

            await UniTask.Yield(token);
        }

        progressBar?.Hide();
        OnAnimalRespawned?.Invoke();
    }

    private void OnRespawnMultiplierChanged()
    {
        // Защита от нулевого множителя
        float multiplier = Mathf.Max(0.1f, RespawnSettings.Multiplier);
        float newCooldown = cooldownTime / multiplier;

        if (newCooldown < 0.1f)
            newCooldown = 0.1f;

        if (progressBar == null || !progressBar.IsActive)
        {
            currentCooldownTime = newCooldown;
            return;
        }

        float currentProgress = progressBar.GetProgress();
        float elapsed = currentProgress * currentCooldownTime;

        currentCooldownTime = newCooldown;
        float newProgress = Mathf.Clamp01(elapsed / currentCooldownTime);

        progressBar.SetProgress(newProgress);
    }

    public virtual void Dispose()
    {
        EventBus.OnRespawnMultiplierChanged -= OnRespawnMultiplierChanged;
        cts?.Cancel();
        cts?.Dispose();
    }
}