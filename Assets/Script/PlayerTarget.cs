using System.Collections;
using UnityEngine;

public class PlayerTarget : MonoBehaviour
{
    [Header("Renderer target (Lower_Armor)")]
    [Tooltip("Jika kosong, script akan mencari child bernama 'Lower_Armor' atau fallback ke first SkinnedMeshRenderer.")]
    [SerializeField] private SkinnedMeshRenderer targetRenderer;

    [Header("Materials")]
    [Tooltip("Material shader Eagle Vision (shader graph) — harus punya property _BaseMap, _GrayAmount, dan _EVActive.")]
    [SerializeField] private Material playerEagleMaterial;

    [Header("Layer (opsional)")]
    [Tooltip("Layer index yang dipakai oleh highlight camera. Jika -1, tidak akan memindahkan layer.")]
    [SerializeField] private int eagleVisionLayer = -1;

    [Header("Fade")]
    [SerializeField] private float fadeSpeed = 5f;

    [Header("Glow Settings")]
    [SerializeField] private float fresnelPowerActive = 1f;
    [SerializeField] private float fresnelPowerDefault = 3f;
    [SerializeField] private Color fresnelColorActive = new Color(0.5f, 1.5f, 2f);
    [SerializeField] private Color fresnelColorDefault = new Color(0.45f, 0.8f, 1f);

    private Material[] originalSharedMaterials;
    private Material[] eagleMaterials;
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
        if (targetRenderer == null)
        {
            Transform child = transform.Find("Lower_Armor");
            if (child != null)
                targetRenderer = child.GetComponent<SkinnedMeshRenderer>();

            if (targetRenderer == null)
                targetRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        }

        if (targetRenderer == null)
        {
            Debug.LogWarning("[PlayerTarget] SkinnedMeshRenderer tidak ditemukan pada player atau child 'Lower_Armor'.");
            return;
        }

        originalSharedMaterials = targetRenderer.sharedMaterials;
        originalLayer = gameObject.layer;
    }

    public void ActivateEagleVision()
    {
        ActivateEagleVision(eagleVisionLayer);
    }

    public void ActivateEagleVision(int highlightLayer)
    {
        if (targetRenderer == null || playerEagleMaterial == null)
            return;

        if (isEagleActive)
            return;

        var shared = originalSharedMaterials ?? targetRenderer.sharedMaterials;
        int len = shared.Length;
        eagleMaterials = new Material[len];

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

            // TAMBAHAN BARU - Force render queue ke Geometry (2000)
            // Ini memastikan player render lebih dulu sebelum transparent objects
            m.renderQueue = 3500;

            eagleMaterials[i] = m;
        }

        targetRenderer.materials = eagleMaterials;
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
            foreach (var m in eagleMaterials)
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

        // SEBELUM restore ke original material, set render queue original material juga
        if (targetRenderer != null && originalSharedMaterials != null)
        {
            // Clone original materials dan set render queue tinggi
            Material[] restoredMats = new Material[originalSharedMaterials.Length];
            for (int i = 0; i < originalSharedMaterials.Length; i++)
            {
                Material m = new Material(originalSharedMaterials[i]);
                m.renderQueue = 3500; // Tetap tinggi
                restoredMats[i] = m;
            }
            targetRenderer.materials = restoredMats;
        }

        // Cleanup
        if (eagleMaterials != null)
        {
            for (int i = 0; i < eagleMaterials.Length; i++)
                if (eagleMaterials[i] != null)
                    Destroy(eagleMaterials[i]);
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

            var mats = targetRenderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && mats[i].HasProperty(GrayAmountID))
                    mats[i].SetFloat(GrayAmountID, currentGray);
            }

            yield return null;
        }

        currentGray = target;
        var finalMats = targetRenderer.materials;
        for (int i = 0; i < finalMats.Length; i++)
            if (finalMats[i] != null && finalMats[i].HasProperty(GrayAmountID))
                finalMats[i].SetFloat(GrayAmountID, currentGray);
    }

    public bool IsActiveEagleVision => isEagleActive;
}