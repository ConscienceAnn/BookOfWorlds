using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// Контроллер панели завершения уровня.
/// Отвечает только за логику кнопок.
/// </summary>
public class LevelCompleteController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [Inject] private UIManager uiManager;

    private void Start()
    {
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void SetNextLevelInteractable(bool interactable)
    {
        if (nextLevelButton != null)
            nextLevelButton.interactable = interactable;
    }

    private void OnNextLevelClicked()
    {
        uiManager?.OnNextLevelButtonClick();
    }

    private void OnRetryClicked()
    {
        uiManager?.OnRetryButtonClick();
    }

    private void OnMainMenuClicked()
    {
        uiManager?.OnMainMenuFromCompleteButtonClick();
    }

    private void OnDestroy()
    {
        if (nextLevelButton != null)
            nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);

        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetryClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
    }
}