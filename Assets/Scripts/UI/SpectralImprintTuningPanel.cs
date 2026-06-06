using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpectralImprintTuningPanel : MonoBehaviour
{
    const int MaxRows = 4;

    static SpectralImprintTuningPanel instance;

    [Header("Behavior")]
    [SerializeField] bool showOnStart = false;
    [SerializeField] KeyCode toggleKey = KeyCode.F3;
    [SerializeField] KeyCode beforeAfterToggleKey = KeyCode.F4;
    [SerializeField, Range(0.05f, 1f)] float refreshInterval = 0.15f;

    [Header("Layout")]
    [SerializeField] Vector2 panelSize = new(900f, 600f);
    [SerializeField] Vector2 panelOffset = new(-24f, -132f);
    [SerializeField] int headerFontSize = 50;
    [SerializeField] int statusFontSize = 35;
    [SerializeField] int rowTitleFontSize = 34;
    [SerializeField] int rowDetailFontSize = 31;
    [SerializeField] int rowEffectFontSize = 28;
    [SerializeField] float rowHeight = 100f;
    [SerializeField] float barHeight = 18f;

    [Header("Bar Colors")]
    [SerializeField] Color attenuationColor = new(0.47f, 0.82f, 0.78f);
    [SerializeField] Color muffleColor = new(0.62f, 0.72f, 0.9f);
    [SerializeField] Color echoColor = new(0.75f, 0.58f, 0.9f);
    [SerializeField] Color crushColor = new(0.95f, 0.7f, 0.45f);

    Canvas canvas;
    Text headerText;
    Text statusText;
    Text legendText;
    readonly List<RowView> rows = new();
    readonly StringBuilder builder = new();
    readonly GenerationBucket[] buckets = new GenerationBucket[MaxRows];
    float nextRefreshTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateRuntimePanel()
    {
        if (instance != null) return;

        var go = new GameObject("Spectral Imprint Tuning Panel");
        instance = go.AddComponent<SpectralImprintTuningPanel>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        BuildUi();
        canvas.enabled = showOnStart;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey))
            canvas.enabled = !canvas.enabled;

        if (beforeAfterToggleKey != KeyCode.None && Input.GetKeyDown(beforeAfterToggleKey))
        {
            SpectralImprintSource.ToggleCleanShowcaseMode();
            nextRefreshTime = 0f;
        }

        if (!canvas.enabled || Time.unscaledTime < nextRefreshTime)
            return;

        nextRefreshTime = Time.unscaledTime + refreshInterval;
        Refresh();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        nextRefreshTime = 0f;
    }

    void BuildUi()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        var panel = CreateRect("Panel", transform);
        panel.anchorMin = new Vector2(1f, 1f);
        panel.anchorMax = new Vector2(1f, 1f);
        panel.pivot = new Vector2(1f, 1f);
        panel.anchoredPosition = panelOffset;
        panel.sizeDelta = panelSize;

        var bg = panel.gameObject.AddComponent<Image>();
        bg.color = new Color(0.03f, 0.035f, 0.04f, 0.84f);

        var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 12, 12);
        layout.spacing = 8f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        headerText = CreateText("Header", panel, font, headerFontSize, FontStyle.Bold, new Color(0.92f, 0.96f, 1f));
        headerText.text = "SPECTRAL IMPRINTS";

        statusText = CreateText("Status", panel, font, statusFontSize, FontStyle.Normal, new Color(0.72f, 0.78f, 0.84f));
        statusText.text = "F3 toggle";

        legendText = CreateText("Legend", panel, font, rowEffectFontSize, FontStyle.Normal, new Color(0.72f, 0.78f, 0.84f));
        legendText.supportRichText = true;
        legendText.text =
            "<color=#78D1C7>attenuation</color>   " +
            "<color=#9EB8E6>muffle</color>   " +
            "<color=#BF94E6>echo</color>   " +
            "<color=#F2B373>crush</color>";

        for (int i = 0; i < MaxRows; i++)
            rows.Add(CreateRow(panel, font));
    }

    void Refresh()
    {
        ResetBuckets();
        GatherPlayer();
        GatherGhosts();

        statusText.text = BuildStatusText();

        for (int i = 0; i < rows.Count; i++)
        {
            if (!buckets[i].hasData)
            {
                rows[i].root.gameObject.SetActive(false);
                continue;
            }

            rows[i].root.gameObject.SetActive(true);
            ApplyBucket(rows[i], buckets[i]);
        }
    }

    void ResetBuckets()
    {
        for (int i = 0; i < buckets.Length; i++)
            buckets[i] = new GenerationBucket { generation = i };
    }

    void GatherPlayer()
    {
        var player = GameManager.Instance != null ? GameManager.Instance.CurrentPlayer : null;
        if (player == null) return;

        var source = player.GetComponent<SpectralImprintSource>();
        AddToBucket(0, true, player.activeInHierarchy, source, player.GetComponent<SpectralGenerationVisual>());
    }

    void GatherGhosts()
    {
        if (LifeManager.Instance == null) return;

        foreach (var ghost in LifeManager.Instance.Ghosts)
        {
            if (ghost == null) continue;

            int bucketIndex = Mathf.Clamp(ghost.GenerationIndex, 0, MaxRows - 1);
            AddToBucket(
                bucketIndex,
                false,
                ghost.gameObject.activeInHierarchy,
                ghost.GetComponent<SpectralImprintSource>(),
                ghost.GetComponent<SpectralGenerationVisual>());
        }
    }

    void AddToBucket(
        int bucketIndex,
        bool isPlayer,
        bool activeNow,
        SpectralImprintSource source,
        SpectralGenerationVisual visual)
    {
        var bucket = buckets[bucketIndex];
        bucket.hasData = true;
        bucket.totalCount++;
        if (isPlayer) bucket.playerCount++;
        if (activeNow) bucket.activeCount++;

        if (source != null)
            bucket.snapshot = source.GetDisplaySnapshot();
        else
            bucket.snapshot = FallbackSnapshot(bucketIndex);

        bucket.alpha = visual != null
            ? visual.GetAlphaForGenerationIndex(bucketIndex)
            : GetFallbackAlpha(bucketIndex);

        buckets[bucketIndex] = bucket;
    }

    string BuildStatusText()
    {
        int totalEchoes = 0;
        int activeEchoes = 0;

        for (int i = 0; i < buckets.Length; i++)
        {
            totalEchoes += Mathf.Max(0, buckets[i].totalCount - buckets[i].playerCount);
            activeEchoes += Mathf.Max(0, buckets[i].activeCount - buckets[i].playerCount);
        }

        string mode = SpectralImprintSource.ForceCleanShowcaseMode
            ? "BEFORE clean ghosts"
            : "AFTER spectral DSP";
        string dspBackend = SpectralImprintNativeDsp.IsAvailable ? "C++ DSP" : "C# fallback";

        return $"{mode}   {dspBackend}   alive echoes {totalEchoes}   audible now {activeEchoes}   F3 panel   F4 mode";
    }

    void ApplyBucket(RowView row, GenerationBucket bucket)
    {
        SpectralImprintDisplaySnapshot snapshot = bucket.snapshot;
        string generationName = bucket.generation >= MaxRows - 1 ? "GEN 3+" : $"GEN {bucket.generation}";
        string role = bucket.playerCount > 0 ? "current self" : "past self";
        string count = bucket.totalCount == 1
            ? "1 body"
            : $"{bucket.totalCount} bodies";

        row.title.text = $"{generationName}  {role}  {count}";
        row.detail.text = $"{GetProfileLabel(snapshot.generation, snapshot)}   alpha {Mathf.RoundToInt(bucket.alpha * 100f)}%";

        float attenuationAmount = 1f - Mathf.Clamp01(snapshot.outputGain);

        SetBar(row.attenuationBar, attenuationAmount, attenuationColor);
        SetBar(row.muffleBar, snapshot.muffleAmount, muffleColor);
        SetBar(row.echoBar, snapshot.echoAmount, echoColor);
        SetBar(row.crushBar, snapshot.crushAmount, crushColor);

        builder.Clear();
        builder.Append("attenuation ");
        builder.Append(Mathf.RoundToInt(attenuationAmount * 100f));
        builder.Append("%   muffle ");
        builder.Append(Mathf.RoundToInt(snapshot.muffleAmount * 100f));
        builder.Append("%   echo ");
        builder.Append(Mathf.RoundToInt(snapshot.echoAmount * 100f));
        builder.Append("%   crush ");
        builder.Append(Mathf.RoundToInt(snapshot.crushAmount * 100f));
        builder.Append('%');
        if (snapshot.quietMemory)
            builder.Append("   quiet memory");

        row.effects.text = builder.ToString();
    }

    static string GetProfileLabel(int generation, SpectralImprintDisplaySnapshot snapshot)
    {
        if (generation <= 0) return "clean signal";
        if (snapshot.quietMemory) return "distant memory";
        if (generation == 1) return "softened echo";
        if (generation == 2) return "filtered imprint";
        return "unstable trace";
    }

    static SpectralImprintDisplaySnapshot FallbackSnapshot(int generation)
    {
        return new SpectralImprintDisplaySnapshot
        {
            generation = generation,
            dspActive = generation > 0,
            outputGain = generation >= 3 ? 0.04f : Mathf.Clamp01(1f - generation * 0.1f),
            muffleAmount = generation == 0 ? 0f : generation == 1 ? 0.55f : generation == 2 ? 0.8f : 0.9f,
            echoAmount = generation == 0 ? 0f : generation == 1 ? 0.55f : generation == 2 ? 0.8f : 0.9f,
            crushAmount = generation == 0 ? 0f : generation == 1 ? 0.55f : generation == 2 ? 0.8f : 0.9f,
            instabilityAmount = 0.15f,
            quietMemory = generation >= 3
        };
    }

    static float GetFallbackAlpha(int generation)
    {
        if (generation <= 0) return 1f;
        if (generation == 1) return 0.85f;
        if (generation == 2) return 0.65f;
        return 0.4f;
    }

    void SetBar(BarView bar, float value, Color color)
    {
        float amount = Mathf.Clamp01(value);
        bar.fill.anchorMax = new Vector2(amount, 1f);
        bar.fill.offsetMin = Vector2.zero;
        bar.fill.offsetMax = Vector2.zero;
        bar.fillImage.color = color;
        bar.trackImage.color = new Color(color.r, color.g, color.b, 0.12f);
    }

    RowView CreateRow(RectTransform parent, Font font)
    {
        var root = CreateRect("Generation Row", parent);
        root.sizeDelta = new Vector2(0f, rowHeight);

        var layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 3f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var top = CreateRect("Top Line", root);
        top.sizeDelta = new Vector2(0f, rowTitleFontSize + 10f);
        var topLayout = top.gameObject.AddComponent<HorizontalLayoutGroup>();
        topLayout.childForceExpandWidth = false;
        topLayout.childForceExpandHeight = true;
        topLayout.spacing = 8f;

        Text title = CreateText("Title", top, font, rowTitleFontSize, FontStyle.Bold, new Color(0.9f, 0.94f, 0.96f));
        title.rectTransform.sizeDelta = new Vector2(385f, rowTitleFontSize + 10f);
        Text detail = CreateText("Detail", top, font, rowDetailFontSize, FontStyle.Normal, new Color(0.68f, 0.74f, 0.78f));
        detail.rectTransform.sizeDelta = new Vector2(420f, rowDetailFontSize + 10f);

        var bars = CreateRect("Effect Bars", root);
        bars.sizeDelta = new Vector2(0f, barHeight);
        var barLayout = bars.gameObject.AddComponent<HorizontalLayoutGroup>();
        barLayout.spacing = 4f;
        barLayout.childForceExpandWidth = true;
        barLayout.childForceExpandHeight = true;

        BarView attenuation = CreateBar("Attenuation", bars);
        BarView muffle = CreateBar("Muffle", bars);
        BarView echo = CreateBar("Echo", bars);
        BarView crush = CreateBar("Crush", bars);

        Text effects = CreateText("Effects", root, font, rowEffectFontSize, FontStyle.Normal, new Color(0.58f, 0.64f, 0.68f));
        effects.rectTransform.sizeDelta = new Vector2(0f, rowEffectFontSize + 10f);

        return new RowView
        {
            root = root,
            title = title,
            detail = detail,
            effects = effects,
            attenuationBar = attenuation,
            muffleBar = muffle,
            echoBar = echo,
            crushBar = crush
        };
    }

    BarView CreateBar(string name, RectTransform parent)
    {
        var track = CreateRect(name, parent);
        track.sizeDelta = new Vector2(0f, barHeight);
        var trackImage = track.gameObject.AddComponent<Image>();
        trackImage.color = new Color(1f, 1f, 1f, 0.1f);

        var fill = CreateRect("Fill", track);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;

        var image = fill.gameObject.AddComponent<Image>();
        image.color = Color.clear;

        return new BarView
        {
            fill = fill,
            fillImage = image,
            trackImage = trackImage
        };
    }

    static Text CreateText(
        string name,
        RectTransform parent,
        Font font,
        int size,
        FontStyle style,
        Color color)
    {
        var rect = CreateRect(name, parent);
        var text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        rect.sizeDelta = new Vector2(0f, size + 4f);
        return text;
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.localScale = Vector3.one;
        return rect;
    }

    struct GenerationBucket
    {
        public bool hasData;
        public int generation;
        public int totalCount;
        public int playerCount;
        public int activeCount;
        public float alpha;
        public SpectralImprintDisplaySnapshot snapshot;
    }

    class RowView
    {
        public RectTransform root;
        public Text title;
        public Text detail;
        public Text effects;
        public BarView attenuationBar;
        public BarView muffleBar;
        public BarView echoBar;
        public BarView crushBar;
    }

    class BarView
    {
        public RectTransform fill;
        public Image fillImage;
        public Image trackImage;
    }
}
