using UnityEngine;
using Zenject;

/// <summary>
/// ќбработчик дебаг-хоткеев.
/// ћожно удалить после финальной сборки.
/// </summary>
public class DebugHotkeyHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float restartHoldDuration = 1.5f;
    [SerializeField] private bool enableDebugLogs = true;

    [Inject] private LevelManager levelManager;
    [Inject] private UIManager uiManager;

    private float restartHoldTime = 0f;
    private bool isRestartKeyHeld = false;

    private void Update()
    {
        // ===== “»Ћ№ƒј (~) Ч –≈—“ј–“ ”–ќ¬Ќя =====
        HandleRestartHotkey();

        // ===== TAB Ч ѕј”«ј =====
        HandlePauseHotkey();
    }

    private void HandleRestartHotkey()
    {
        bool isTildePressed = UnityEngine.InputSystem.Keyboard.current != null &&
                              UnityEngine.InputSystem.Keyboard.current.backquoteKey.isPressed;

        if (isTildePressed)
        {
            if (!isRestartKeyHeld)
            {
                isRestartKeyHeld = true;
                restartHoldTime = 0f;
                if (enableDebugLogs) Debug.Log("[DebugHotkey] «ажата тильда...");
            }

            restartHoldTime += Time.unscaledDeltaTime;

            if (restartHoldTime >= restartHoldDuration)
            {
                restartHoldTime = 0f;
                isRestartKeyHeld = false;
                Debug.Log("[DebugHotkey] “ильда зажата 1.5 секунды Ч –≈—“ј–“!");
                RestartLevel();
            }
        }
        else
        {
            if (isRestartKeyHeld && enableDebugLogs)
            {
                Debug.Log($"[DebugHotkey] “ильда отпущена (зажато {restartHoldTime:F2} сек)");
            }
            isRestartKeyHeld = false;
            restartHoldTime = 0f;
        }
    }

    private void HandlePauseHotkey()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame)
        {
            Debug.Log("[DebugHotkey] TAB нажат Ч ѕј”«ј!");
            TogglePause();
        }
    }

    private void RestartLevel()
    {
        if (levelManager != null)
        {
            // «акрываем все панели перед рестартом
            uiManager?.CloseAllPanels();
            levelManager.RetryLevel();
        }
        else
        {
            Debug.LogWarning("[DebugHotkey] LevelManager не найден!");
        }
    }

    private void TogglePause()
    {
        if (uiManager != null)
        {
            uiManager.OnPauseButtonClick();
        }
        else
        {
            // Fallback
            Time.timeScale = Time.timeScale > 0 ? 0f : 1f;
            Debug.Log($"[DebugHotkey] Time.timeScale = {Time.timeScale}");
        }
    }
}