using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button exitButton;

    [Header("UI")]
    [SerializeField] private CanvasGroup bookUICanvasGroup;

    [Header("Book Animation")]
    [SerializeField] private Animator bookAnimator;
    [SerializeField] private Transform bookTransform;
    [SerializeField] private string animationName = "BookOpen";
    [SerializeField] private int targetFrame = 94;
    [SerializeField] private int totalFrames = 164;
    [SerializeField] private Vector3 finalPosition = new Vector3(0f, 1.2f, -3.9f);
    [SerializeField] private Vector3 finalRotation = new Vector3(0f, -21f, 0f);
    [SerializeField] private float moveDuration = 1.5f;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene";

    private Vector3 startPosition;
    private Vector3 startRotation;
    private float moveProgress = 0f;
    private bool isMoving = false;

    private void Start()
    {
        if (bookUICanvasGroup != null)
        {
            bookUICanvasGroup.alpha = 0f;
            bookUICanvasGroup.interactable = false;
            bookUICanvasGroup.blocksRaycasts = false;
        }

        newGameButton.onClick.AddListener(OnNewGameClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
        resetButton.onClick.AddListener(OnResetClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        bool hasSave = SaveSystem.SaveExists();
        continueButton.interactable = hasSave;
        Debug.Log($"Главное меню: {(hasSave ? "Есть сохранение" : "Нет сохранения")}");

        if (bookTransform != null)
        {
            startPosition = bookTransform.position;
            startRotation = bookTransform.eulerAngles;
        }

        if (bookAnimator != null)
        {
            PlayBookAnimation();
        }
        else
        {
            ShowBookUI();
        }
    }

    private void PlayBookAnimation()
    {
        bookAnimator.Play(animationName, 0, 0f);

        float animationLength = GetAnimationLength(animationName);
        float normalizedTime = (float)targetFrame / totalFrames;
        float targetTime = normalizedTime * animationLength;

        Debug.Log($"Анимация: {animationLength} сек, остановка на {targetTime} сек (кадр {targetFrame}/{totalFrames})");

        Invoke(nameof(FreezeBookAnimation), targetTime);
    }

    private void FreezeBookAnimation()
    {
        if (bookAnimator != null)
        {
            bookAnimator.speed = 0f;
            float normalizedTime = (float)targetFrame / totalFrames;
            bookAnimator.Play(animationName, 0, normalizedTime);
        }

        if (bookTransform != null)
        {
            startPosition = bookTransform.position;
            startRotation = bookTransform.eulerAngles;
            moveProgress = 0f;
            isMoving = true;
            Debug.Log("Начинаем плавное движение книги");
        }
    }

    private void Update()
    {
        if (isMoving && bookTransform != null)
        {
            moveProgress += Time.deltaTime / moveDuration;
            moveProgress = Mathf.Min(moveProgress, 1f);

            float smoothProgress = SmoothStep(moveProgress);

            bookTransform.position = Vector3.Lerp(startPosition, finalPosition, smoothProgress);

            Vector3 currentRotation = new Vector3(
                Mathf.LerpAngle(startRotation.x, finalRotation.x, smoothProgress),
                Mathf.LerpAngle(startRotation.y, finalRotation.y, smoothProgress),
                Mathf.LerpAngle(startRotation.z, finalRotation.z, smoothProgress)
            );
            bookTransform.eulerAngles = currentRotation;

            if (moveProgress >= 1f)
            {
                isMoving = false;
                ShowBookUI();
                Debug.Log($"Книга перемещена в позицию {finalPosition}");
            }
        }
    }

    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private void ShowBookUI()
    {
        if (bookUICanvasGroup != null)
        {
            bookUICanvasGroup.alpha = 1f;
            bookUICanvasGroup.interactable = true;
            bookUICanvasGroup.blocksRaycasts = true;
        }
    }

    private float GetAnimationLength(string name)
    {
        if (bookAnimator == null) return 2f;

        AnimationClip[] clips = bookAnimator.runtimeAnimatorController.animationClips;
        foreach (var clip in clips)
        {
            if (clip.name == name)
                return clip.length;
        }
        return 2f;
    }

    private void OnNewGameClicked()
    {
        Debug.Log("Новая игра");
        SaveSystem.DeleteSave();
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnContinueClicked()
    {
        Debug.Log("Продолжить игру");

        // Проверяем, что сохранение существует и валидное
        if (!SaveSystem.SaveExists())
        {
            Debug.LogWarning("Сохранение не найдено!");
            return;
        }

        // Проверяем, что сохранение можно загрузить
        var testData = SaveSystem.Load();
        if (testData == null)
        {
            Debug.LogWarning("Сохранение повреждено!");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    private void OnResetClicked()
    {
        Debug.Log("Сброс прогресса");
        SaveSystem.DeleteSave();
        continueButton.interactable = false;
    }

    private void OnExitClicked()
    {
        Debug.Log("Выход из игры");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnEnable()
    {
        if (continueButton != null)
        {
            continueButton.interactable = SaveSystem.SaveExists();
        }
    }
}