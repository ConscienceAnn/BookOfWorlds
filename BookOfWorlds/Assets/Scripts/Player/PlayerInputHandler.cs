using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerInputHandler : MonoBehaviour
{
    public event System.Action<Vector2> OnMovementInput;
    public event System.Action<float> OnZoomInput;
    public event System.Action OnResetZoomInput;
    public event System.Action OnPauseInput;
    public event System.Action OnCollectInput;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction zoomAction;
    private InputAction resetZoomAction;
    private InputAction pauseAction;
    private InputAction collectAction;

    [Inject]
    public void Construct(PlayerInput input)
    {
        Debug.Log("========================================");
        Debug.Log(" PlayerInputHandler.Construct() called!");
        Debug.Log("========================================");

        playerInput = input;

        if (playerInput == null)
        {
            Debug.LogError(" PlayerInput is NULL! Zenject didn't inject it!");
            return;
        }

        Debug.Log($" PlayerInput found! GameObject: {playerInput.name}");
        Debug.Log($" PlayerInput Actions: {playerInput.actions}");
        Debug.Log($" PlayerInput Default Map: {playerInput.defaultActionMap}");

        SetupInputActions();
    }

    private void SetupInputActions()
    {
        Debug.Log("--- SetupInputActions() ---");

        if (playerInput == null)
        {
            Debug.LogError(" playerInput is null in SetupInputActions!");
            return;
        }

        if (playerInput.actions == null)
        {
            Debug.LogError(" playerInput.actions is null!");
            return;
        }

        Debug.Log($" Total Action Maps: {playerInput.actions.actionMaps.Count}");

        // Выводим все Action Maps - ИСПОЛЬЗУЕМ ДРУГОЕ ИМЯ
        foreach (var actionMap in playerInput.actions.actionMaps)
        {
            Debug.Log($"  Action Map: {actionMap.name} (Actions: {actionMap.actions.Count})");
        }

        // Ищем карту "Player" - ТЕПЕРЬ НЕТ КОНФЛИКТА ИМЕН
        var playerActionMap = playerInput.actions.FindActionMap("Player");

        if (playerActionMap == null)
        {
            Debug.LogError(" Action Map 'Player' NOT FOUND!");
            Debug.Log(" Проверьте: в PlayerInputActions есть Action Map с именем 'Player'?");
            Debug.Log(" Проверьте: в PlayerInput компоненте Default Map = 'Player'?");
            return;
        }

        Debug.Log($" Action Map 'Player' found! Actions count: {playerActionMap.actions.Count}");

        // Проверяем каждое действие
        moveAction = playerActionMap.FindAction("Move");
        zoomAction = playerActionMap.FindAction("Zoom");
        resetZoomAction = playerActionMap.FindAction("ResetZoom");
        pauseAction = playerActionMap.FindAction("Pause");
        collectAction = playerActionMap.FindAction("Collect");

        // Логируем результат поиска
        Debug.Log($"    Move Action: {(moveAction != null ? " FOUND" : " NOT FOUND")}");
        Debug.Log($"   Zoom Action: {(zoomAction != null ? " FOUND" : " NOT FOUND")}");
        Debug.Log($"   ResetZoom Action: {(resetZoomAction != null ? " FOUND" : " NOT FOUND")}");
        Debug.Log($"   Pause Action: {(pauseAction != null ? "FOUND" : " NOT FOUND")}");
        Debug.Log($"   Collect Action: {(collectAction != null ? "FOUND" : " NOT FOUND")}");

        // Подписываемся на Move
        if (moveAction != null)
        {
            Debug.Log("Subscribing to Move Action...");
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
            Debug.Log("Move Action subscribed!");

            // Выводим информацию о привязках
            Debug.Log($"   Binding count: {moveAction.bindings.Count}");
            foreach (var binding in moveAction.bindings)
            {
                Debug.Log($"      - {binding.effectivePath} (groups: {binding.groups})");
            }
        }
        else
        {
            Debug.LogError(" Move Action NOT FOUND! Проверьте Input Actions Asset!");
        }

        // Подписываемся на Zoom
        if (zoomAction != null)
        {
            zoomAction.performed += OnZoomPerformed;
            Debug.Log(" Zoom Action subscribed!");
        }

        // Подписываемся на ResetZoom
        if (resetZoomAction != null)
        {
            resetZoomAction.performed += OnResetZoomPerformed;
            Debug.Log(" ResetZoom Action subscribed!");
        }

        // Подписываемся на Pause
        if (pauseAction != null)
        {
            pauseAction.performed += OnPausePerformed;
            Debug.Log(" Pause Action subscribed!");
        }

        // Подписываемся на Collect
        if (collectAction != null)
        {
            collectAction.performed += OnCollectPerformed;
            collectAction.canceled += OnCollectCanceled;
            Debug.Log(" Collect Action subscribed!");
        }

        Debug.Log("--- SetupInputActions() complete ---");
        Debug.Log("========================================");
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        Debug.Log($" MOVE PERFORMED! Value: {value}, Phase: {context.phase}");
        OnMovementInput?.Invoke(value);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        Debug.Log($" MOVE CANCELED! Phase: {context.phase}");
        OnMovementInput?.Invoke(Vector2.zero);
    }

    private void OnZoomPerformed(InputAction.CallbackContext context)
    {
        // Читаем значение
        float value = context.ReadValue<float>();

        // Определяем направление по имени binding
        float normalizedValue = 0f;

        // Проверяем, какое binding вызвало событие
        if (context.control != null)
        {
            string path = context.control.path;
            Debug.Log($"ZOOM: path={path}, value={value}");

            // Определяем направление по пути контрола
            if (path.Contains("scroll/up") || path.Contains("Scroll/Up"))
            {
                normalizedValue = 1f; // Приближение
            }
            else if (path.Contains("scroll/down") || path.Contains("Scroll/Down"))
            {
                normalizedValue = -1f; // Отдаление
            }
            else
            {
                // Fallback: используем знак значения
                normalizedValue = Mathf.Sign(value);
            }
        }
        else
        {
            // Если не можем определить путь, используем знак
            normalizedValue = Mathf.Sign(value);
        }

        Debug.Log($"ZOOM PERFORMED! Raw: {value}, Normalized: {normalizedValue}");
        OnZoomInput?.Invoke(normalizedValue);
    }

    private void OnResetZoomPerformed(InputAction.CallbackContext context)
    {
        Debug.Log($" RESET ZOOM PERFORMED!");
        OnResetZoomInput?.Invoke();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        Debug.Log($" PAUSE PERFORMED!");
        OnPauseInput?.Invoke();
    }

    private void OnCollectPerformed(InputAction.CallbackContext context)
    {
        Debug.Log($" COLLECT PERFORMED! (E key pressed)");
        OnCollectInput?.Invoke();
    }

    private void OnCollectCanceled(InputAction.CallbackContext context)
    {
        Debug.Log($" COLLECT CANCELED! (E key released)");
        OnCollectInput?.Invoke();
    }

    private void OnDisable()
    {
        Debug.Log(" PlayerInputHandler OnDisable - Unsubscribing...");

        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
        }
        if (zoomAction != null)
            zoomAction.performed -= OnZoomPerformed;
        if (resetZoomAction != null)
            resetZoomAction.performed -= OnResetZoomPerformed;
        if (pauseAction != null)
            pauseAction.performed -= OnPausePerformed;
        if (collectAction != null)
        {
            collectAction.performed -= OnCollectPerformed;
            collectAction.canceled -= OnCollectCanceled;
        }
    }
}