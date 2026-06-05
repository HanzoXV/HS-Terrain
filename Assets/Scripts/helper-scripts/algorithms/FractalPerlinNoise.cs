using UnityEngine;

public class FractalPerlinNoise : INoiseAlgorithms
{
    public float[,] GenerateHeightmap(int seed, float scale, int octaves, float persistence, float lacunarity)
    {
        int width = 100;
        int height = 100; 
        float[,] heightmap = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x / scale) * frequency + seed;
                    float sampleY = (y / scale) * frequency + seed;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                heightmap[x, y] = noiseHeight;
            }
        }
        return heightmap;
    }
}
