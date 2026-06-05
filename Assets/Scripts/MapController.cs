using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("General Settings")]
    [Range(0, 10000)] public int Seed = 42;
    [Range(1, 100)] public float Scale = 50f;
    public bool ComplexGeometry = false;

    [Header("Chunk Settings")]
    [Range(1, 1000)] public int ChunkX = 2;
    [Range(1, 1000)] public int ChunkY = 2;

    [Header("Noise Settings")]
    [Range(1, 8)] public int Octaves = 8;
    [Range(0, 1)] public float Persistence = 0.426f;
    [Range(1, 4)] public float Lacunarity = 2.18f;
    [Range(0, 6)] public int LoD = 0;

    [HideInInspector] public Texture2D HeightMapPreviewTexture;
    [HideInInspector] public GameObject GeneratedMap;

    private MapGenerator _mapGenerator;

    public void GenerateMap()
    {
        if (GeneratedMap != null)
            DestroyImmediate(GeneratedMap);

        MapVariables variables = new MapVariables(Seed, Scale, Octaves, Persistence, Lacunarity, ChunkX, ChunkY, LoD);
        GeneratedMap = _mapGenerator.Create(variables);
    }

    public void GeneratePreview()
    {
        MapVariables variables = new MapVariables(Seed, Scale, Octaves, Persistence, Lacunarity, ChunkX, ChunkY, LoD);
        HeightMapPreviewTexture = _mapGenerator.GeneratePreview(variables);
    }

    private void OnValidate()
    {
        if (_mapGenerator == null)
            _mapGenerator = new MapGenerator();

        GeneratePreview();
    }
}