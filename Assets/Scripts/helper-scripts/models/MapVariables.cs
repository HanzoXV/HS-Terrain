using NaughtyAttributes;
using UnityEngine;

public class MapVariables 
{
    public int Seed = 42;
    public float Scale = 50f;
    public int Octaves = 8;
    public float Persistence = 0.426f;
    public float Lacunarity = 2.18f;
    public int ChunkX = 2;
    public int ChunkY = 2;
    public int LoD = 0;

    public MapVariables(int seed, float scale, int octaves, float persistence, float lacunarity, int chunkX, int chunkY, int lod)
    {
        Seed = seed;
        Scale = scale;
        Octaves = octaves;
        Persistence = persistence;
        Lacunarity = lacunarity;
        ChunkX = chunkX;
        ChunkY = chunkY;
        LoD = lod;
    }
}
