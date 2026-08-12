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
            }
        }

        if (inactiveMaterial == null && useDefaultInactive)
        {
            inactiveMaterial = CreateGrayMaterial();
        }
    }

    /// <summary>
    /// —оздаЄт материал дл€ "серого" состо€ни€ Ч без текстур, бело-серый
    /// </summary>
    private Material CreateGrayMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material grayMat = new Material(shader);

        grayMat.mainTexture = null;
        grayMat.color = new Color(0.9f, 0.9f, 0.9f);

        grayMat.SetFloat("_Smoothness", 0.3f);
        grayMat.SetFloat("_Metallic", 0f);

        return grayMat;
    }

    public void SetGray()
    {
        if (renderers == null || renderers.Length == 0)
        {
            FindRenderers();
            if (renderers == null || renderers.Length == 0) return;
        }

        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.material = inactiveMaterial;
            }
        }
    }

    public void SetColored()
    {
        if (renderers == null || renderers.Length == 0)
        {
            FindRenderers();
            if (renderers == null || renderers.Length == 0) return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && i < originalMaterials.Length && originalMaterials[i] != null)
            {
                renderers[i].material = originalMaterials[i];
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