using UnityEngine;

public class MapGenerator
{
    private const int PreviewResolution = 256;
    private const int ChunkResolution = 128;

    public Texture2D GeneratePreview(MapVariables variables)
    {
        FractalPerlinNoise noise = new FractalPerlinNoise();
        float[,] region = noise.GenerateRegion(
            variables.Seed, variables.Scale,
            variables.Octaves, variables.Persistence, variables.Lacunarity,
            variables.ComplexMountains, variables.MountainThreshold,
            variables.UseContinentMask, variables.OceanThreshold, variables.WarpStrength,
            0f, 0f, PreviewResolution);

        Texture2D texture = new Texture2D(PreviewResolution, PreviewResolution, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[PreviewResolution * PreviewResolution];
        for (int y = 0; y < PreviewResolution; y++)
            for (int x = 0; x < PreviewResolution; x++)
            {
                float v = region[x, y];
                pixels[y * PreviewResolution + x] = new Color(v, v, v, 1f);
            }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    public GameObject Create(MapVariables variables)
    {
        GameObject parent = new GameObject("Map");

        for (int cy = 0; cy < variables.ChunkY; cy++)
            for (int cx = 0; cx < variables.ChunkX; cx++)
                SpawnChunk(parent, cx, cy, variables);

        return parent;
    }

    public void UpdateChunks(GameObject parent, MapVariables variables)
    {
        for (int cy = 0; cy < variables.ChunkY; cy++)
        {
            for (int cx = 0; cx < variables.ChunkX; cx++)
            {
                Transform t = parent.transform.Find($"Chunk_{cx}_{cy}");
                if (t == null) continue;

                MeshFilter mf = t.GetComponent<MeshFilter>();
                if (mf != null)
                    mf.mesh = BuildChunkMesh(cx, cy, variables);
            }
        }
    }

    private void SpawnChunk(GameObject parent, int cx, int cy, MapVariables variables)
    {
        Mesh mesh = BuildChunkMesh(cx, cy, variables);

        GameObject chunk = new GameObject($"Chunk_{cx}_{cy}");
        chunk.transform.SetParent(parent.transform, false);
        chunk.transform.localPosition = new Vector3(cx * variables.ChunkScale, 0f, cy * variables.ChunkScale);

        chunk.AddComponent<MeshFilter>().mesh = mesh;
        chunk.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }

    private Mesh BuildChunkMesh(int cx, int cy, MapVariables variables)
    {
        FractalPerlinNoise noise = new FractalPerlinNoise();

        int quadsPerAxis = (int)Mathf.Pow(2, 7 - variables.LoD);
        int vertsPerAxis = quadsPerAxis + 1;
        float step = variables.ChunkScale / quadsPerAxis;
        float worldStepX = variables.ChunkScale / quadsPerAxis;
        float worldStepY = variables.ChunkScale / quadsPerAxis;

        Vector3[] vertices = new Vector3[vertsPerAxis * vertsPerAxis];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[quadsPerAxis * quadsPerAxis * 6];

        float chunkWorldX = cx * variables.ChunkScale;
        float chunkWorldY = cy * variables.ChunkScale;

        for (int y = 0; y < vertsPerAxis; y++)
        {
            for (int x = 0; x < vertsPerAxis; x++)
            {
                float worldX = chunkWorldX + x * worldStepX;
                float worldY = chunkWorldY + y * worldStepY;

                float raw = noise.Sample(
                    variables.Seed, variables.Scale,
                    variables.Octaves, variables.Persistence, variables.Lacunarity,
                    variables.ComplexMountains, variables.MountainThreshold,
                    variables.UseContinentMask, variables.OceanThreshold, variables.WarpStrength,
                    worldX, worldY);

                float height = variables.HeightCurve.Evaluate(Mathf.Clamp(raw, -1f, 1f)) * variables.HeightMultiplier;

                int vi = y * vertsPerAxis + x;
                vertices[vi] = new Vector3(x * step, height, y * step);
                uvs[vi] = new Vector2((float)x / quadsPerAxis, (float)y / quadsPerAxis);
            }
        }

        int ti = 0;
        for (int y = 0; y < quadsPerAxis; y++)
        {
            for (int x = 0; x < quadsPerAxis; x++)
            {
                int bl = y * vertsPerAxis + x;
                int br = bl + 1;
                int tl = bl + vertsPerAxis;
                int tr = tl + 1;

                triangles[ti++] = bl; triangles[ti++] = tl; triangles[ti++] = tr;
                triangles[ti++] = bl; triangles[ti++] = tr; triangles[ti++] = br;
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.name = $"ChunkMesh_{cx}_{cy}";
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}