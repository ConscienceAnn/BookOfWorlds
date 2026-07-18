using UnityEngine;
using Cysharp.Threading.Tasks;

public class ResourceRespawner
{
    private readonly float respawnDuration;
    private readonly System.Action onRespawnComplete;

    private float elapsed = 0f;
    private bool isWaiting = false;

    public ResourceRespawner(float respawnDuration, System.Action onRespawnComplete)
    {
        this.respawnDuration = respawnDuration;
        this.onRespawnComplete = onRespawnComplete;
    }

    public void StartRespawn()
    {
        if (isWaiting) return;

        elapsed = 0f;
        isWaiting = true;
        WaitForRespawnAsync().Forget();
    }

    private async UniTaskVoid WaitForRespawnAsync()
    {
        while (elapsed < respawnDuration)
        {
            // Если игра на паузе — ждём, не увеличивая таймер
            if (PauseService.Instance != null && PauseService.Instance.IsPaused)
            {
                await UniTask.Yield();
                continue;
            }

            elapsed += Time.unscaledDeltaTime;
            await UniTask.Yield();
        }

        isWaiting = false;
        onRespawnComplete?.Invoke();
    }

    public void Cancel()
    {
        isWaiting = false;
    }

    public float GetProgress()
    {
        return isWaiting ? elapsed / respawnDuration : 0f;
    }
}