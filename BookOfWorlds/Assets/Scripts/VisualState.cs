using UnityEngine;

public class VisualState : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private bool useDefaultInactive = true;

    private Renderer[] renderers;
    private Color[] originalColors;
    private Material[] originalMaterials;

    private void Awake()
    {
        FindRenderers();
    }

    public void ForceRefresh()
    {
        Debug.Log($"[VisualState] ForceRefresh() для {gameObject.name}");
        FindRenderers();
        SetColored();
    }

    private void FindRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        originalColors = new Color[renderers.Length];
        originalMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                originalColors[i] = renderers[i].material.color;
                originalMaterials[i] = renderers[i].material;
                Debug.Log($"[VisualState] Найден рендер: {renderers[i].name}, цвет: {originalColors[i]}");
            }
        }

        if (renderers.Length == 0)
        {
            Debug.LogWarning($"[VisualState] Нет рендеров для {gameObject.name}!");
        }

        if (inactiveMaterial == null && useDefaultInactive)
        {
            inactiveMaterial = CreateGrayMaterial();
        }
    }

    /// <summary>
    /// Создаёт материал для "серого" состояния — без текстур, бело-серый
    /// </summary>
    private Material CreateGrayMaterial()
    {
        //  Создаём материал с базовым шейдером
        Material grayMat;

        // Пробуем создать через URP
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        grayMat = new Material(shader);

        //  Убираем текстуру — модель становится "голой"
        grayMat.mainTexture = null;

        //  Делаем очень светлый серый (почти белый, но с оттенком)
        // Используй любой из вариантов:
        // Color.gray - средний серый
        // new Color(0.7f, 0.7f, 0.7f) - светлый серый
        // new Color(0.85f, 0.85f, 0.85f) - почти белый
        grayMat.color = new Color(0.9f, 0.9f, 0.9f); // ИЗМЕНИ ЗДЕСЬ!

        // Настройки для URP
        grayMat.SetFloat("_Smoothness", 0.3f);
        grayMat.SetFloat("_Metallic", 0f);

        Debug.Log($"[VisualState] Создан серый материал без текстур: цвет={grayMat.color}");
        return grayMat;
    }

    public void SetGray()
    {
        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogWarning($"[VisualState] Нет рендеров для {gameObject.name}, пробуем найти...");
            FindRenderers();
            if (renderers == null || renderers.Length == 0) return;
        }

        Debug.Log($"[VisualState] SetGray() для {gameObject.name}, рендеров: {renderers.Length}");

        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                //  Меняем ВЕСЬ материал на серый (без текстур)
                renderer.material = inactiveMaterial;
                Debug.Log($"[VisualState] Материал заменён на серый для {renderer.name}");
            }
        }
    }

    public void SetColored()
    {
        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogWarning($"[VisualState] Нет рендеров для {gameObject.name}, пробуем найти...");
            FindRenderers();
            if (renderers == null || renderers.Length == 0) return;
        }

        Debug.Log($"[VisualState] SetColored() для {gameObject.name}, рендеров: {renderers.Length}");

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && i < originalMaterials.Length && originalMaterials[i] != null)
            {
                //  Восстанавливаем оригинальный материал (с текстурой)
                renderers[i].material = originalMaterials[i];
                Debug.Log($"[VisualState] Материал восстановлен для {renderers[i].name}");
            }
        }
    }

    public void RestoreOriginalMaterials()
    {
        SetColored();
    }

    private void OnEnable()
    {
        if (renderers == null || renderers.Length == 0)
        {
            FindRenderers();
        }
        SetColored();
    }
}