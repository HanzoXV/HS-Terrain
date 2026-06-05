using UnityEngine;

public class MapGenerator
{
    private const int HeightmapSize = 1000;

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

    // ChunkX * ChunkY chunks are built from origin (0,0).
    // Each chunk covers exactly 1x1 world unit = (HeightmapSize/HeightmapSize) = 1 cell per chunk.
    // ChunkX=3, ChunkY=2 -> 3x2 world units of terrain, sampling the first 3x2 cells of the heightmap.
    public GameObject Create(MapVariables variables)
    {
        float[,] heightmap = GenerateHeightmap(variables);

        GameObject parent = new GameObject("Map");

        for (int cy = 0; cy < variables.ChunkY; cy++)
        {
            for (int cx = 0; cx < variables.ChunkX; cx++)
            {
                Mesh mesh = BuildChunkMesh(heightmap, cx, cy, variables.LoD);

                GameObject chunk = new GameObject($"Chunk_{cx}_{cy}");
                chunk.transform.SetParent(parent.transform, false);
                chunk.transform.localPosition = new Vector3(cx, 0f, cy);

                chunk.AddComponent<MeshFilter>().mesh = mesh;
                chunk.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }
        }

        return parent;
    }

    private float[,] GenerateHeightmap(MapVariables variables)
    {
        FractalPerlinNoise perlin = new FractalPerlinNoise();
        return perlin.GenerateHeightmap(
            variables.Seed, variables.Scale,
            variables.Octaves, variables.Persistence, variables.Lacunarity);
    }

    // Each chunk samples exactly 1 cell from the heightmap (originX=cx, originY=cy).
    // LoD subdivides that cell into a denser mesh for smoother appearance.
    // vertsPerAxis = 2^LoD + 1, so LoD 0 = 1 quad, LoD 3 = 8x8 quads per chunk.
    private Mesh BuildChunkMesh(float[,] heightmap, int cx, int cy, int lod)
    {
        int quadsPerAxis = (int)Mathf.Pow(2, lod);   // LoD 0=1, 1=2, 2=4, 3=8 quads
        int vertsPerAxis = quadsPerAxis + 1;
        float step = 1f / quadsPerAxis;         // world-space step between verts (chunk is 1x1)

        Vector3[] vertices = new Vector3[vertsPerAxis * vertsPerAxis];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[quadsPerAxis * quadsPerAxis * 6];

        for (int y = 0; y < vertsPerAxis; y++)
        {
            for (int x = 0; x < vertsPerAxis; x++)
            {
                // Sample heightmap: chunk (cx,cy) starts at heightmap cell (cx, cy)
                // Sub-verts interpolate to neighbouring cells for smooth LoD
                float hmXf = cx + x * step;
                float hmYf = cy + y * step;

                int vi = y * vertsPerAxis + x;
                float h = SampleBilinear(heightmap, hmXf, hmYf);
                vertices[vi] = new Vector3(x * step, h, y * step);
                uvs[vi] = new Vector2(x * step, y * step);
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

    // Bilinear sample so sub-cell LoD verts interpolate smoothly between heightmap cells
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