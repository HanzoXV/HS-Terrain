using UnityEngine;

public class MapGenerator
{
    private const int HeightmapSize = 1000;
    private const int CellsPerChunk = 100;

    public Texture2D GeneratePreview(MapVariables variables)
    {
        float[,] heightmap = GenerateHeightmap(variables);

        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;

        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float v = heightmap[x, y];
                pixels[y * width + x] = new Color(v, v, v, 1f);
            }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    public GameObject Create(MapVariables variables)
    {
        float[,] heightmap = GenerateHeightmap(variables);

        GameObject parent = new GameObject("Map");

        for (int cy = 0; cy < variables.ChunkY; cy++)
        {
            for (int cx = 0; cx < variables.ChunkX; cx++)
            {
                Mesh mesh = BuildChunkMesh(heightmap, cx, cy, variables);

                GameObject chunk = new GameObject($"Chunk_{cx}_{cy}");
                chunk.transform.SetParent(parent.transform, false);
                chunk.transform.localPosition = new Vector3(cx * variables.ChunkScale, 0f, cy * variables.ChunkScale);

                chunk.AddComponent<MeshFilter>().mesh = mesh;
                chunk.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }
        }

        return parent;
    }

    public void UpdateChunks(GameObject parent, MapVariables variables)
    {
        float[,] heightmap = GenerateHeightmap(variables);

        for (int cy = 0; cy < variables.ChunkY; cy++)
        {
            for (int cx = 0; cx < variables.ChunkX; cx++)
            {
                string name = $"Chunk_{cx}_{cy}";
                Transform t = parent.transform.Find(name);

                if (t == null) continue;

                MeshFilter mf = t.GetComponent<MeshFilter>();
                if (mf != null)
                    mf.mesh = BuildChunkMesh(heightmap, cx, cy, variables);
            }
        }
    }

    private float[,] GenerateHeightmap(MapVariables variables)
    {
        FractalPerlinNoise perlin = new FractalPerlinNoise();
        return perlin.GenerateHeightmap(
            variables.Seed, variables.Scale,
            variables.Octaves, variables.Persistence, variables.Lacunarity,
            variables.ComplexMountains, variables.MountainThreshold);
    }

    private Mesh BuildChunkMesh(float[,] heightmap, int cx, int cy, MapVariables variables)
    {
        int maxLod = 6;
        int quadsPerAxis = (int)Mathf.Pow(2, maxLod - variables.LoD);
        int vertsPerAxis = quadsPerAxis + 1;
        float step = variables.ChunkScale / quadsPerAxis;

        Vector3[] vertices = new Vector3[vertsPerAxis * vertsPerAxis];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[quadsPerAxis * quadsPerAxis * 6];

        int originX = cx * CellsPerChunk;
        int originY = cy * CellsPerChunk;

        for (int y = 0; y < vertsPerAxis; y++)
        {
            for (int x = 0; x < vertsPerAxis; x++)
            {
                float hmXf = originX + (x / (float)quadsPerAxis) * CellsPerChunk;
                float hmYf = originY + (y / (float)quadsPerAxis) * CellsPerChunk;

                float rawHeight = SampleBilinear(heightmap, hmXf, hmYf);
                float curvedHeight = variables.HeightCurve.Evaluate(rawHeight) * variables.HeightMultiplier;

                int vi = y * vertsPerAxis + x;
                vertices[vi] = new Vector3(x * step, curvedHeight, y * step);
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
        mesh.name = $"ChunkMesh_{cx}_{cy}";
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private float SampleBilinear(float[,] map, float x, float y)
    {
        int maxX = map.GetLength(0) - 1;
        int maxY = map.GetLength(1) - 1;

        int x0 = Mathf.Clamp(Mathf.FloorToInt(x), 0, maxX);
        int x1 = Mathf.Clamp(x0 + 1, 0, maxX);
        int y0 = Mathf.Clamp(Mathf.FloorToInt(y), 0, maxY);
        int y1 = Mathf.Clamp(y0 + 1, 0, maxY);

        float tx = x - x0;
        float ty = y - y0;

        float top = Mathf.Lerp(map[x0, y1], map[x1, y1], tx);
        float bottom = Mathf.Lerp(map[x0, y0], map[x1, y0], tx);
        return Mathf.Lerp(bottom, top, ty);
    }
}