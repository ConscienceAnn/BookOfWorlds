using UnityEngine;

public class VisualState : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private bool useDefaultInactive = true;

    private Renderer[] renderers;
    private Material[] originalMaterials;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                originalMaterials[i] = renderers[i].sharedMaterial;
            }
        }

        if (activeMaterial == null && renderers.Length > 0 && renderers[0] != null)
        {
            activeMaterial = originalMaterials[0];
        }

        if (inactiveMaterial == null && useDefaultInactive)
        {
            inactiveMaterial = CreateGrayMaterial();
        }
    }

    private Material CreateGrayMaterial()
    {
        Material grayMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (grayMat != null)
        {
            grayMat.color = Color.gray;
            grayMat.SetFloat("_Smoothness", 0.2f);
        }
        else
        {
            // Fallback для Standard
            grayMat = new Material(Shader.Find("Standard"));
            grayMat.color = Color.gray;
        }
        return grayMat;
    }

    public void SetGray()
    {
        Debug.Log($"[VisualState] SetGray() ВЫЗВАН для {gameObject.name}");
        Debug.Log($"   - renderers: {(renderers != null ? renderers.Length.ToString() : "NULL")}");

        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogWarning($"   - renderers = null или пустой!");
            return;
        }

        if (inactiveMaterial == null)
        {
            Debug.LogWarning($"   - inactiveMaterial = NULL, создаём...");
            inactiveMaterial = CreateGrayMaterial();
            if (inactiveMaterial == null)
            {
                Debug.LogError($"   - НЕ УДАЛОСЬ создать серый материал!");
                return;
            }
            Debug.Log($"   - серый материал создан");
        }

        int changedCount = 0;
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.sharedMaterial = inactiveMaterial;
                changedCount++;
                Debug.Log($"   - материал изменён для {renderer.name}");
            }
        }
        Debug.Log($"   - изменено материалов: {changedCount}");
    }

    public void SetColored()
    {
        Debug.Log($" [VisualState] SetColored() ВЫЗВАН для {gameObject.name}");
        Debug.Log($"   - renderers: {(renderers != null ? renderers.Length.ToString() : "NULL")}");
        Debug.Log($"   - originalMaterials: {(originalMaterials != null ? originalMaterials.Length.ToString() : "NULL")}");

        if (renderers == null || renderers.Length == 0) return;

        int restoredCount = 0;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && i < originalMaterials.Length && originalMaterials[i] != null)
            {
                renderers[i].sharedMaterial = originalMaterials[i];
                restoredCount++;
                Debug.Log($"   - материал восстановлен для {renderers[i].name}");
            }
        }
        Debug.Log($"  - восстановлено материалов: {restoredCount}");
    }

    public void RestoreOriginalMaterials()
    {
        SetColored();
    }
}