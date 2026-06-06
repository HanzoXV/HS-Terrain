using UnityEngine;

public class ContinentMask : INoiseAlgorithms
{
    public float[,] GenerateHeightmap(int seed, float scale, int octaves, float persistence, float lacunarity)
    {
        const int size = 256;
        float[,] mask = new float[size, size];

        System.Random prng = new System.Random(seed + 99999);
        float offsetX = prng.Next(-100000, 100000);
        float offsetY = prng.Next(-100000, 100000);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float value = 0f;
                float maxAmp = 0f;

                for (int o = 0; o < octaves; o++)
                {
                    float sX = offsetX + (x / (float)(size - 1)) * scale * frequency;
                    float sY = offsetY + (y / (float)(size - 1)) * scale * frequency;
                    value += Mathf.PerlinNoise(sX, sY) * amplitude;
                    maxAmp += amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                mask[x, y] = maxAmp > 0f ? value / maxAmp : 0f;
            }
        }

        return mask;
    }

    public float Sample(int seed, float scale, float worldX, float worldY)
    {
        System.Random prng = new System.Random(seed + 99999);
        float offsetX = prng.Next(-100000, 100000);
        float offsetY = prng.Next(-100000, 100000);

        float sX = offsetX + worldX / scale;
        float sY = offsetY + worldY / scale;
        return Mathf.PerlinNoise(sX, sY);
    }
}