using UnityEngine;

public class FractalPerlinNoise : INoiseAlgorithms
{
    public float[,] GenerateHeightmap(int seed, float scale, int octaves, float persistence, float lacunarity)
    {
        return GenerateRegion(seed, scale, octaves, persistence, lacunarity, false, 0f, false, 0f, 0f, 0, 0, 256);
    }

    public float[,] GenerateHeightmap(int seed, float scale, int octaves, float persistence, float lacunarity,
                                       bool complexMountains, float mountainThreshold,
                                       bool useContinentMask, float oceanThreshold, float warpStrength)
    {
        return GenerateRegion(seed, scale, octaves, persistence, lacunarity,
                              complexMountains, mountainThreshold,
                              useContinentMask, oceanThreshold, warpStrength,
                              0, 0, 256);
    }

    public float[,] GenerateRegion(int seed, float scale, int octaves, float persistence, float lacunarity,
                                    bool complexMountains, float mountainThreshold,
                                    bool useContinentMask, float oceanThreshold, float warpStrength,
                                    float worldOriginX, float worldOriginY, int resolution)
    {
        float[,] heightmap = new float[resolution, resolution];

        for (int x = 0; x < resolution; x++)
            for (int y = 0; y < resolution; y++)
                heightmap[x, y] = Sample(seed, scale, octaves, persistence, lacunarity,
                                         complexMountains, mountainThreshold,
                                         useContinentMask, oceanThreshold, warpStrength,
                                         worldOriginX + x, worldOriginY + y);

        return heightmap;
    }

    public float Sample(int seed, float scale, int octaves, float persistence, float lacunarity,
                         bool complexMountains, float mountainThreshold,
                         bool useContinentMask, float oceanThreshold, float warpStrength,
                         float worldX, float worldY)
    {
        System.Random prng = new System.Random(seed);
        float offsetX = prng.Next(-100000, 100000);
        float offsetY = prng.Next(-100000, 100000);

        float wx = 0f;
        float wy = 0f;
        if (warpStrength > 0f)
        {
            System.Random wprng = new System.Random(seed + 12345);
            float wox1 = wprng.Next(-100000, 100000);
            float woy1 = wprng.Next(-100000, 100000);
            float wox2 = wprng.Next(-100000, 100000);
            float woy2 = wprng.Next(-100000, 100000);

            float wsX = worldX * scale * 0.0001f;
            float wsY = worldY * scale * 0.0001f;
            wx = (Mathf.PerlinNoise(wox1 + wsX, woy1 + wsY) * 2f - 1f) * warpStrength;
            wy = (Mathf.PerlinNoise(wox2 + wsX, woy2 + wsY) * 2f - 1f) * warpStrength;
        }

        float continentValue = 1f;
        if (useContinentMask)
        {
            System.Random cprng = new System.Random(seed + 99999);
            float cox = cprng.Next(-100000, 100000);
            float coy = cprng.Next(-100000, 100000);
            float cScale = 1f / (scale * 0.3f * 0.0001f);

            float cv = 0f;
            float ca = 1f;
            float cf = 1f;
            float cMax = 0f;
            for (int o = 0; o < 3; o++)
            {
                cv += Mathf.PerlinNoise(cox + worldX * scale * 0.0001f * 0.3f * cf, coy + worldY * scale * 0.0001f * 0.3f * cf) * ca;
                cMax += ca;
                ca *= 0.5f;
                cf *= 2f;
            }
            continentValue = cMax > 0f ? cv / cMax : 0f;
        }

        float baseShape = 0f;
        if (complexMountains)
        {
            float bsX = offsetX + (worldX + wx) * scale * 0.0001f;
            float bsY = offsetY + (worldY + wy) * scale * 0.0001f;
            baseShape = Mathf.PerlinNoise(bsX, bsY);
        }

        float mountainMask = complexMountains
            ? Mathf.InverseLerp(mountainThreshold - 0.1f, mountainThreshold + 0.1f, baseShape)
            : 0f;

        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;
        float maxAmplitude = 0f;

        for (int o = 0; o < octaves; o++)
        {
            float sX = offsetX + (worldX + wx) * scale * 0.0001f * frequency;
            float sY = offsetY + (worldY + wy) * scale * 0.0001f * frequency;

            float sample = Mathf.PerlinNoise(sX, sY);

            if (complexMountains)
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

        if (useContinentMask)
        {
            if (continentValue < oceanThreshold)
            {
                float t = continentValue / oceanThreshold;
                float smoothT = t * t * (3f - 2f * t);
                normalized = Mathf.Lerp(-0.3f, 0f, smoothT);
            }
            else
            {
                float landBlend = Mathf.InverseLerp(oceanThreshold, oceanThreshold + 0.15f, continentValue);
                normalized = Mathf.Lerp(0f, normalized, landBlend);

                float mountainZone = Mathf.InverseLerp(0.65f, 0.85f, continentValue);
                normalized = Mathf.Lerp(normalized, normalized * (1f + mountainZone * 0.6f), mountainZone);
            }
        }

        return Mathf.Clamp(normalized, -1f, 1f);
    }
}