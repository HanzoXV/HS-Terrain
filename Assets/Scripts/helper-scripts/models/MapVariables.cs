using UnityEngine;

public class MapVariables
{
    public int Seed;
    public float Scale;
    public int Octaves;
    public float Persistence;
    public float Lacunarity;
    public int ChunkX;
    public int ChunkY;
    public int LoD;
    public float ChunkScale;
    public AnimationCurve HeightCurve;
    public float HeightMultiplier;
    public bool ComplexMountains;
    public float MountainThreshold;

    public MapVariables(int seed, float scale, int octaves, float persistence, float lacunarity,
                        int chunkX, int chunkY, int lod, float chunkScale,
                        AnimationCurve heightCurve, float heightMultiplier,
                        bool complexMountains, float mountainThreshold)
    {
        Seed = seed;
        Scale = scale;
        Octaves = octaves;
        Persistence = persistence;
        Lacunarity = lacunarity;
        ChunkX = chunkX;
        ChunkY = chunkY;
        LoD = lod;
        ChunkScale = chunkScale;
        HeightCurve = heightCurve;
        HeightMultiplier = heightMultiplier;
        ComplexMountains = complexMountains;
        MountainThreshold = mountainThreshold;
    }
}