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

    private bool isInputEnabled = true;

    [Inject]
    public void Construct(PlayerInput input)
    {
        playerInput = input;
        SetupInputActions();
    }

    private void SetupInputActions()
    {
        if (playerInput == null || playerInput.actions == null) return;

        var playerActionMap = playerInput.actions.FindActionMap("Player");
        if (playerActionMap == null) return;

        moveAction = playerActionMap.FindAction("Move");
        zoomAction = playerActionMap.FindAction("Zoom");
        resetZoomAction = playerActionMap.FindAction("ResetZoom");
        pauseAction = playerActionMap.FindAction("Pause");
        collectAction = playerActionMap.FindAction("Collect");

        if (moveAction != null)
        {
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
        }

        if (zoomAction != null)
        {
            zoomAction.performed += OnZoomPerformed;
        }

        if (resetZoomAction != null)
        {
            resetZoomAction.performed += OnResetZoomPerformed;
        }

        if (pauseAction != null)
        {
            pauseAction.performed += OnPausePerformed;
        }

        if (collectAction != null)
        {
            collectAction.performed += OnCollectPerformed;
            collectAction.canceled += OnCollectCanceled;
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!isInputEnabled) return;
        OnMovementInput?.Invoke(context.ReadValue<Vector2>());
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (!isInputEnabled) return;
        OnMovementInput?.Invoke(Vector2.zero);
    }

    private void OnZoomPerformed(InputAction.CallbackContext context)
    {
        if (!isInputEnabled) return;

        float value = context.ReadValue<float>();
        float normalizedValue = 0f;

        if (context.control != null)
        {
            string path = context.control.path;
            if (path.Contains("scroll/up") || path.Contains("Scroll/Up"))
            {
                normalizedValue = 1f;
            }
            else if (path.Contains("scroll/down") || path.Contains("Scroll/Down"))
            {
                normalizedValue = -1f;
            }
            else
            {
                normalizedValue = Mathf.Sign(value);
            }
        }
        else
        {
            normalizedValue = Mathf.Sign(value);
        }

        OnZoomInput?.Invoke(normalizedValue);
    }

    private void OnResetZoomPerformed(InputAction.CallbackContext context)
    {
        if (!isInputEnabled) return;
        OnResetZoomInput?.Invoke();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        // PAUSE ÂÑÅÃÄÀ ÐÀÁÎÒÀÅÒ
        OnPauseInput?.Invoke();
    }

    private void OnCollectPerformed(InputAction.CallbackContext context)
    {
        if (!isInputEnabled) return;
        OnCollectInput?.Invoke();
    }

    private void OnCollectCanceled(InputAction.CallbackContext context)
    {
        // Collect òîëüêî ïî íàæàòèþ, áåç îòìåíû
    }

    private void OnDisable()
    {
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

    // ===== PUBLIC METHODS =====

    public void SetInputEnabled(bool enabled)
    {
        isInputEnabled = enabled;
        Debug.Log($"PlayerInput: input enabled = {enabled}");
    }

    public void LockCursor(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        Debug.Log($"Cursor locked = {locked}");
    }

    public bool IsInputEnabled() => isInputEnabled;
}