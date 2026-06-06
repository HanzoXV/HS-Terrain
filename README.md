# HS-Terrain
(Head-start terrain, to give you a head-start)

A modular, editor-driven procedural terrain generation system built for Unity. This framework uses a decoupled Bilinear Sampling Chunk Mesh Pipeline to dynamically generate seamless 3D terrain grids directly within the Unity Editor.

---

## Project Goal

The primary objective of this project is to build a highly optimized, expandable procedural generation framework capable of producing diverse, infinite 3D landscapes. 

While the current implementation relies entirely on Fractal Perlin Noise to shape the terrain, the underlying architecture is decoupled to allow seamless integration of additional noise algorithms (such as Simplex, Cellular/Worley, and value noise). The ultimate goal is a multi-algorithm generation pipeline where different noise types can be layered or blended to create complex biomes, realistic erosion patterns, and varied geographical features.

---

## Current Core Features

* **Fractal Perlin Noise Implementation:** The current version uses layered Perlin noise with adjustable octaves, persistence, and lacunarity to create organic, natural-looking height variations.
* **Decoupled Chunk System:** Dynamically segments massive heightmaps into independent, lightweight sub-meshes to optimize rendering overhead and lay the foundation for infinite world loading.
* **Bilinear Height Sampling:** Uses cross-boundary mathematical interpolation to eliminate mesh-seam gaps or tearing between adjoining terrain chunks.
* **Variable Level of Detail (LoD):** Power-of-two vertex scaling ($2^{LoD}$) per chunk, allowing optimization control over grid-density and polycount.
* **Custom Editor Dashboard:** Features an inline real-time 1000 x 1000 texture preview window, automated dirty-state validation, and reactive UI grouping.

---

## Updates
* **06/JUNE/2026:** Added base mesh generation using fractal perlin noise and a custom unity UI Inspector.

---

## System Architecture

The project splits the mathematical generation pipeline from the engine interface, prioritizing performance and modular separation of concerns.

```text
       [ MapController ]  <--->  [ Custom Editor Script ]
               │                     (Handles Preview & UI)
               ▼
        [ MapVariables ]  (Drives Configuration Data)
               │
               ▼
       [ MapGenerator ]
        ├── FractalPerlinNoise  ──> Generates Data Array
        ├── SampleBilinear     ──> Smooths Vertex Coordinates
        └── BuildChunkMesh     ──> Allocates Unity Vertices/Triangles
