using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PauseMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button saveAndQuitButton;
    [SerializeField] private Button quitWithoutSaveButton;

    [Inject] private UIManager uiManager;
    [Inject] private PauseService _pauseService;
    [Inject] private AudioHelper _audioHelper;

    private void Start()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (saveAndQuitButton != null)
            saveAndQuitButton.onClick.AddListener(OnSaveAndQuitClicked);

        if (quitWithoutSaveButton != null)
            quitWithoutSaveButton.onClick.AddListener(OnQuitWithoutSaveClicked);
    }

    private void OnContinueClicked()
    {
        // Проверяем, что игра действительно на паузе
        if (_pauseService == null || !_pauseService.IsPaused) return;

        _audioHelper?.PlaySound("ui_click");
        _pauseService.TogglePause();

        // Не вызываем uiManager.OnResumeButtonClick() — это вызывает дублирование
    }

    private void OnSaveAndQuitClicked()
    {
        uiManager?.OnSaveAndQuitButtonClick();
    }

    private void OnQuitWithoutSaveClicked()
    {
        uiManager?.OnQuitWithoutSaveButtonClick();
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);

        if (saveAndQuitButton != null)
            saveAndQuitButton.onClick.RemoveListener(OnSaveAndQuitClicked);

        if (quitWithoutSaveButton != null)
            quitWithoutSaveButton.onClick.RemoveListener(OnQuitWithoutSaveClicked);
    }
}