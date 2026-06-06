using UnityEngine;

public class DomainWarper : INoiseAlgorithms
{
    public float[,] GenerateHeightmap(int seed, float scale, int octaves, float persistence, float lacunarity)
    {
        var (warpX, _) = GenerateWarpFields(seed, scale, 1f, 256);
        return warpX;
    }

    public (float[,] warpX, float[,] warpY) GenerateWarpFields(int seed, float scale, float strength, int resolution)
    {
        float[,] warpX = new float[resolution, resolution];
        float[,] warpY = new float[resolution, resolution];

        System.Random prng = new System.Random(seed + 12345);
        float ox1 = prng.Next(-100000, 100000);
        float oy1 = prng.Next(-100000, 100000);
        float ox2 = prng.Next(-100000, 100000);
        float oy2 = prng.Next(-100000, 100000);

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float sX = (x / (float)(resolution - 1)) * scale;
                float sY = (y / (float)(resolution - 1)) * scale;

                warpX[x, y] = (Mathf.PerlinNoise(ox1 + sX, oy1 + sY) * 2f - 1f) * strength;
                warpY[x, y] = (Mathf.PerlinNoise(ox2 + sX, oy2 + sY) * 2f - 1f) * strength;
            }
        }

        return (warpX, warpY);
    }
}