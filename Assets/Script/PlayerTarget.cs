using System.Collections;
using UnityEngine;

public class PlayerTarget : MonoBehaviour
{
    [Header("Renderer targets")]
    [Tooltip("List semua SkinnedMeshRenderer yang mau di-swap materialnya")]
    [SerializeField] private SkinnedMeshRenderer[] targetRenderers;

    [Header("Materials")]
    [SerializeField] private Material playerEagleMaterial;

    [Header("Layer (opsional)")]
    [SerializeField] private int eagleVisionLayer = -1;

    [Header("Fade")]
    [SerializeField] private float fadeSpeed = 5f;

    [Header("Glow Settings")]
    [SerializeField] private float fresnelPowerActive = 1f;
    [SerializeField] private float fresnelPowerDefault = 3f;
    [SerializeField] private Color fresnelColorActive = new Color(0.5f, 1.5f, 2f);
    [SerializeField] private Color fresnelColorDefault = new Color(0.45f, 0.8f, 1f);

    private Material[][] originalSharedMaterials; // 2D array untuk multiple renderers
    private Material[][] eagleMaterials;
    private int originalLayer;
    private bool isEagleActive = false;
    private Coroutine fadeCoroutine;
    private float currentGray = 0f;

    private static readonly int GrayAmountID = Shader.PropertyToID("_GrayAmount");
    private static readonly int EVActiveID = Shader.PropertyToID("_EVActive");
    private static readonly int EVActive2ID = Shader.PropertyToID("_EVActive (1)");
    private static readonly int FresnelPowerID = Shader.PropertyToID("_FresnelPower");
    private static readonly int FresnelColorID = Shader.PropertyToID("FresnelColor");

    void Awake()
    {
        // Kalau array kosong, cari semua SkinnedMeshRenderer di children
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            Debug.LogWarning("[PlayerTarget] Tidak ada SkinnedMeshRenderer ditemukan.");
            return;
        }

        // Simpan original materials untuk setiap renderer
        originalSharedMaterials = new Material[targetRenderers.Length][];
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] != null)
                originalSharedMaterials[i] = targetRenderers[i].sharedMaterials;
        }

        originalLayer = gameObject.layer;
    }

    public void ActivateEagleVision()
    {
        ActivateEagleVision(eagleVisionLayer);
    }

    public void ActivateEagleVision(int highlightLayer)
    {
        if (targetRenderers == null || targetRenderers.Length == 0 || playerEagleMaterial == null)
            return;

        if (isEagleActive)
            return;

        eagleMaterials = new Material[targetRenderers.Length][];

        // Loop untuk setiap renderer (Body, Head_Hands, Lower_Armor)
        for (int r = 0; r < targetRenderers.Length; r++)
        {
            if (targetRenderers[r] == null) continue;

            var shared = originalSharedMaterials[r];
            int len = shared.Length;
            eagleMaterials[r] = new Material[len];

            for (int i = 0; i < len; i++)
            {
                Material m = new Material(playerEagleMaterial);

                if (shared[i] != null)
                {
                    Texture baseTex = null;
                    if (shared[i].HasProperty("_BaseMap"))
                        baseTex = shared[i].GetTexture("_BaseMap");
                    if (baseTex == null && shared[i].HasProperty("_MainTex"))
                        baseTex = shared[i].GetTexture("_MainTex");

                    if (baseTex != null && m.HasProperty("_BaseMap"))
                        m.SetTexture("_BaseMap", baseTex);
                }

                if (m.HasProperty(GrayAmountID))
                    m.SetFloat(GrayAmountID, 0.5f);

                if (m.HasProperty(EVActiveID))
                    m.SetFloat(EVActiveID, 1f);
                
                if (m.HasProperty(EVActive2ID))
                    m.SetFloat(EVActive2ID, 1f);

                if (m.HasProperty(FresnelPowerID))
                    m.SetFloat(FresnelPowerID, fresnelPowerActive);

                if (m.HasProperty(FresnelColorID))
                    m.SetColor(FresnelColorID, fresnelColorActive);

                m.renderQueue = 3500;

                eagleMaterials[r][i] = m;
            }

            targetRenderers[r].materials = eagleMaterials[r];
        }

        isEagleActive = true;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeGrayCoroutine(0.6f));
    }

    public void DeactivateEagleVision()
    {
        if (!isEagleActive)
            return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutAndRestore());
    }

    private IEnumerator FadeOutAndRestore()
    {
        yield return FadeGrayCoroutineInternal(0f);

        if (eagleMaterials != null)
        {
            foreach (var mats in eagleMaterials)
            {
                if (mats == null) continue;
                foreach (var m in mats)
                {
                    if (m == null) continue;
                    
                    if (m.HasProperty(EVActiveID))
                        m.SetFloat(EVActiveID, 0f);
                    if (m.HasProperty(EVActive2ID))
                        m.SetFloat(EVActive2ID, 0f);
                    if (m.HasProperty(FresnelPowerID))
                        m.SetFloat(FresnelPowerID, fresnelPowerDefault);
                    if (m.HasProperty(FresnelColorID))
                        m.SetColor(FresnelColorID, fresnelColorDefault);
                }
            }
        }

        // Restore untuk setiap renderer
        for (int r = 0; r < targetRenderers.Length; r++)
        {
            if (targetRenderers[r] == null || originalSharedMaterials[r] == null) continue;

            Material[] restoredMats = new Material[originalSharedMaterials[r].Length];
            for (int i = 0; i < originalSharedMaterials[r].Length; i++)
            {
                Material m = new Material(originalSharedMaterials[r][i]);
                m.renderQueue = 3500;
                restoredMats[i] = m;
            }
            targetRenderers[r].materials = restoredMats;
        }

        // Cleanup
        if (eagleMaterials != null)
        {
            foreach (var mats in eagleMaterials)
            {
                if (mats == null) continue;
                for (int i = 0; i < mats.Length; i++)
                    if (mats[i] != null)
                        Destroy(mats[i]);
            }
            eagleMaterials = null;
        }

        isEagleActive = false;
        fadeCoroutine = null;
    }

    private IEnumerator FadeGrayCoroutine(float target)
    {
        yield return FadeGrayCoroutineInternal(target);
    }

    private IEnumerator FadeGrayCoroutineInternal(float target)
    {
        float start = currentGray;
        float t = 0f;
        float duration = (fadeSpeed <= 0f) ? 0.01f : (1f / fadeSpeed);

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            currentGray = Mathf.Lerp(start, target, t);

            // Update semua renderer
            for (int r = 0; r < targetRenderers.Length; r++)
            {
                if (targetRenderers[r] == null) continue;
                var mats = targetRenderers[r].materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] != null && mats[i].HasProperty(GrayAmountID))
                        mats[i].SetFloat(GrayAmountID, currentGray);
                }
            }

            yield return null;
        }

        currentGray = target;
        
        // Final update semua renderer
        for (int r = 0; r < targetRenderers.Length; r++)
        {
            if (targetRenderers[r] == null) continue;
            var finalMats = targetRenderers[r].materials;
            for (int i = 0; i < finalMats.Length; i++)
                if (finalMats[i] != null && finalMats[i].HasProperty(GrayAmountID))
                    finalMats[i].SetFloat(GrayAmountID, currentGray);
        }
    }

    public bool IsActiveEagleVision => isEagleActive;
}