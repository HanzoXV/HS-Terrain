using UnityEngine;

public interface INoiseAlgorithms
{
    public float[,] GenerateHeightmap(int seed, float scale, int octaves, float persistence, float lacunarity);
}
