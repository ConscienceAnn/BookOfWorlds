using UnityEngine;

public class PauseService
{
    private static PauseService instance;
    public static PauseService Instance => instance;

    private bool isPaused = false;

    public PauseService()
    {
        instance = this;
    }

    public bool IsPaused => isPaused;

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log($" Пауза: {(isPaused ? "ВКЛ" : "ВЫКЛ")}");
    }
}