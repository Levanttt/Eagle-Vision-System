using UnityEngine;

public class RenderQueueDebugger : MonoBehaviour
{
    [Tooltip("Renderer yang mau dicek (kosongin kalau mau auto detect semua di scene)")]
    [SerializeField] private Renderer[] renderers;

    [Tooltip("Tampilkan hasil di Console tiap interval detik")]
    [SerializeField] private float logInterval = 2f;

    private float timer;

    void Start()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = FindObjectsOfType<Renderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= logInterval)
        {
            timer = 0f;
            LogRenderQueues();
        }
    }

    void LogRenderQueues()
    {
        Debug.Log("=== 🔍 RenderQueue Debug ===");
        foreach (var r in renderers)
        {
            if (r == null) continue;
            foreach (var mat in r.sharedMaterials)
            {
                if (mat == null) continue;

                string owner = r.name;
                string shader = mat.shader != null ? mat.shader.name : "(no shader)";
                int queue = mat.renderQueue;
                string queueName = QueueName(queue);

                Debug.Log($"{owner,-20} → Queue: {queue} ({queueName}) | Shader: {shader}");
            }
        }
    }

    string QueueName(int q)
    {
        if (q < 2000) return "Background";
        if (q < 2450) return "Opaque";
        if (q < 3000) return "AlphaTest / Cutout";
        if (q < 4000) return "Transparent";
        return "Overlay";
    }
}
