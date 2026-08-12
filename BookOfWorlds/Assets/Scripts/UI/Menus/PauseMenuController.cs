using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// Контроллер меню паузы.
/// Отвечает только за логику кнопок в меню паузы.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button saveAndQuitButton;
    [SerializeField] private Button quitWithoutSaveButton;

    [Inject] private UIManager uiManager;

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
        uiManager?.OnResumeButtonClick();
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