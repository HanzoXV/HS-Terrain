using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("General Settings")]
    [Range(0, 10000)] public int Seed = 1337;
    [Range(1, 100)] public float Scale = 12f;
    public bool AdvancedNoise = false;

    [Header("Chunk Settings")]
    [Range(1, 1000)] public int ChunkX = 4;
    [Range(1, 1000)] public int ChunkY = 4;
    [Range(1, 500)] public float ChunkScale = 200f;

    [Header("Height Settings")]
    public AnimationCurve HeightCurve = AnimationCurve.EaseInOut(-1f, -0.3f, 1f, 1f);
    [Range(1, 500)] public float HeightMultiplier = 120f;

    [Header("Mountain Settings")]
    public bool ComplexMountains = true;
    [Range(0f, 1f)] public float MountainThreshold = 0.62f;
    [Range(0f, 5f)] public float WarpStrength = 1.5f;

    [Header("World Settings")]
    public bool UseContinentMask = true;
    [Range(0f, 1f)] public float OceanThreshold = 0.35f;

    [Header("Noise Settings")]
    [Range(1, 8)] public int Octaves = 6;
    [Range(0, 1)] public float Persistence = 0.38f;
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

    public void GeneratePreview()
    {
        HeightMapPreviewTexture = _mapGenerator.GeneratePreview(BuildVariables());
    }

    private void OnValidate()
    {
        if (_mapGenerator == null)
            _mapGenerator = new MapGenerator();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            GeneratePreview();
        };
#endif
    }

    private MapVariables BuildVariables() =>
        new MapVariables(Seed, Scale, Octaves, Persistence, Lacunarity,
                         ChunkX, ChunkY, LoD, ChunkScale,
                         HeightCurve, HeightMultiplier,
                         ComplexMountains, MountainThreshold,
                         UseContinentMask, OceanThreshold, WarpStrength);
}