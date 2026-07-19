using UnityEngine;

public class VisualState : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Material activeMaterial;      // Оригинальный материал
    [SerializeField] private Material inactiveMaterial;    // Серый/бесцветный материал
    [SerializeField] private bool useDefaultInactive = true; // Использовать стандартный серый

    private Renderer[] renderers;
    private Material[] originalMaterials;

    private void Awake()
    {
        // Находим все рендеры на объекте и дочерних
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];

        // Сохраняем оригинальные материалы
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].sharedMaterial;
        }

        // Если активный материал не задан — используем оригинальный
        if (activeMaterial == null && renderers.Length > 0)
        {
            activeMaterial = originalMaterials[0];
        }

        // Если неактивный материал не задан — создаём серый
        if (inactiveMaterial == null && useDefaultInactive)
        {
            inactiveMaterial = CreateGrayMaterial();
        }
    }

    private Material CreateGrayMaterial()
    {
        Material grayMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        grayMat.color = Color.gray;
        grayMat.SetFloat("_Smoothness", 0.2f);
        return grayMat;
    }

    public void SetActive(bool isActive)
    {
        if (renderers == null || renderers.Length == 0) return;

        foreach (var renderer in renderers)
        {
            renderer.sharedMaterial = isActive ? activeMaterial : inactiveMaterial;
        }
    }

    public void SetActiveMaterial(Material material)
    {
        if (renderers == null || renderers.Length == 0) return;

        foreach (var renderer in renderers)
        {
            renderer.sharedMaterial = material;
        }
    }

    public void RestoreOriginalMaterials()
    {
        if (renderers == null || renderers.Length == 0) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (i < originalMaterials.Length)
            {
                renderers[i].sharedMaterial = originalMaterials[i];
            }
        }
    }

    public void SetGray()
    {
        SetActive(false);
    }

    public void SetColored()
    {
        RestoreOriginalMaterials();
    }

    // Визуализация в Editor
    private void OnDrawGizmosSelected()
    {
        if (renderers != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
                }
            }
        }
    }
}