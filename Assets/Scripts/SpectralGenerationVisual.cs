using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpectralGenerationVisual : MonoBehaviour
{
    [Header("Generation Alpha")]
    [SerializeField, Range(0f, 1f)] float generation0Alpha = 1f;
    [SerializeField, Range(0f, 1f)] float generation1Alpha = 0.85f;
    [SerializeField, Range(0f, 1f)] float generation2Alpha = 0.65f;
    [SerializeField, Range(0f, 1f)] float generation3PlusAlpha = 0.4f;

    SpriteRenderer spriteRenderer;
    int generationIndex;

    public int GenerationIndex => generationIndex;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyAlpha();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        ApplyAlpha();
    }
#endif

    public void SetGenerationIndex(int value)
    {
        generationIndex = Mathf.Max(0, value);
        ApplyAlpha();
    }

    public float GetAlphaForGenerationIndex(int generation) => GetAlphaForGeneration(Mathf.Max(0, generation));

    void ApplyAlpha()
    {
        if (spriteRenderer == null) return;

        Color color = spriteRenderer.color;
        color.a = GetAlphaForGeneration(generationIndex);
        spriteRenderer.color = color;
    }

    float GetAlphaForGeneration(int generation)
    {
        if (generation <= 0) return generation0Alpha;
        if (generation == 1) return generation1Alpha;
        if (generation == 2) return generation2Alpha;
        return generation3PlusAlpha;
    }
}
