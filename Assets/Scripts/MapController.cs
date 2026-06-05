using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("General Settings")]
    [Range(0, 10000)] public int Seed = 1337;
    [Range(1, 100)] public float Scale = 12f;
    public bool AutoUpdate = false;
    public bool AdvancedNoise = false;

    [Header("Chunk Settings")]
    [Range(1, 1000)] public int ChunkX = 4;
    [Range(1, 1000)] public int ChunkY = 4;
    [Range(1, 500)] public float ChunkScale = 200f;

    [Header("Height Settings")]
    public AnimationCurve HeightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Range(1, 500)] public float HeightMultiplier = 120f;

    [Header("Mountain Settings")]
    public bool ComplexMountains = true;
    [Range(0f, 1f)] public float MountainThreshold = 0.45f;

    [Header("Noise Settings")]
    [Range(1, 8)] public int Octaves = 7;
    [Range(0, 1)] public float Persistence = 0.5f;
    [Range(1, 4)] public float Lacunarity = 2f;
    [Range(0, 6)] public int LoD = 2;

    [HideInInspector] public Texture2D HeightMapPreviewTexture;
    [HideInInspector] public GameObject GeneratedMap;

    private MapGenerator _mapGenerator;

    public void GenerateMap()
    {
        if (GeneratedMap != null)
            DestroyImmediate(GeneratedMap);

        GeneratedMap = _mapGenerator.Create(BuildVariables());
    }

    public void UpdateMap()
    {
        if (GeneratedMap == null) { GenerateMap(); return; }
        _mapGenerator.UpdateChunks(GeneratedMap, BuildVariables());
    }

    public void GeneratePreview()
    {
        HeightMapPreviewTexture = _mapGenerator.GeneratePreview(BuildVariables());
    }

#if UNITY_EDITOR
    private double _lastChangeTime;
    private bool _callbackRegistered;
    private const double DebounceSeconds = 0.4;

    private void ScheduleRefresh()
    {
        _lastChangeTime = UnityEditor.EditorApplication.timeSinceStartup;
        if (_callbackRegistered) return;
        _callbackRegistered = true;
        UnityEditor.EditorApplication.update += OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (UnityEditor.EditorApplication.timeSinceStartup - _lastChangeTime < DebounceSeconds) return;
        UnityEditor.EditorApplication.update -= OnEditorUpdate;
        _callbackRegistered = false;
        if (this == null) return;
        GeneratePreview();
        if (AutoUpdate && GeneratedMap != null)
            UpdateMap();
    }
#endif

    private void OnValidate()
    {
        if (_mapGenerator == null)
            _mapGenerator = new MapGenerator();

#if UNITY_EDITOR
        ScheduleRefresh();
#endif
    }

    private MapVariables BuildVariables() =>
        new MapVariables(Seed, Scale, Octaves, Persistence, Lacunarity,
                         ChunkX, ChunkY, LoD, ChunkScale,
                         HeightCurve, HeightMultiplier,
                         ComplexMountains, MountainThreshold);
}