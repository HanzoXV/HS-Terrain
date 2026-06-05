using UnityEngine;

public class FractalPerlinNoise : INoiseAlgorithms
{
    public float[,] GenerateHeightmap(int seed, float scale, int octaves, float persistence, float lacunarity)
    {
        return Generate(seed, scale, octaves, persistence, lacunarity, false, 0f);
    }

    public float[,] GenerateHeightmap(int seed, float scale, int octaves, float persistence, float lacunarity,
                                       bool complexGeometry, float mountainThreshold)
    {
        return Generate(seed, scale, octaves, persistence, lacunarity, complexGeometry, mountainThreshold);
    }

    private float[,] Generate(int seed, float scale, int octaves, float persistence, float lacunarity,
                               bool complexGeometry, float mountainThreshold)
    {
        const int size = 1000;
        float[,] heightmap = new float[size, size];
        System.Random prng = new System.Random(seed);
        float offsetX = prng.Next(-100000, 100000);
        float offsetY = prng.Next(-100000, 100000);
        float[,] baseShape = null;
        if (complexGeometry)
        {
            baseShape = new float[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    float bX = offsetX + (x / (float)(size - 1)) * scale;
                    float bY = offsetY + (y / (float)(size - 1)) * scale;
                    baseShape[x, y] = Mathf.PerlinNoise(bX, bY);
                }
        }

        float minVal = float.MaxValue;
        float maxVal = float.MinValue;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;
                float maxAmplitude = 0f;
                float mountainMask = 0f;
                if (complexGeometry)
                    mountainMask = Mathf.InverseLerp(
                        mountainThreshold - 0.1f,
                        mountainThreshold + 0.1f,
                        baseShape[x, y]);

                for (int o = 0; o < octaves; o++)
                {
                    float sX = offsetX + (x / (float)(size - 1)) * scale * frequency;
                    float sY = offsetY + (y / (float)(size - 1)) * scale * frequency;

                    float sample = Mathf.PerlinNoise(sX, sY);

                    if (complexGeometry)
                    {
                        float ridged = 1f - Mathf.Abs(sample * 2f - 1f);
                        ridged = ridged * ridged;
                        sample = Mathf.Lerp(sample, ridged, mountainMask);
                    }

                    noiseHeight += sample * amplitude;
                    maxAmplitude += amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                float normalized = maxAmplitude > 0f ? noiseHeight / maxAmplitude : 0f;
                heightmap[x, y] = normalized;

                if (normalized < minVal) minVal = normalized;
                if (normalized > maxVal) maxVal = normalized;
            }
        }
        float range = maxVal - minVal;
        if (range > 0f)
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    heightmap[x, y] = (heightmap[x, y] - minVal) / range;

        return heightmap;
    }
}