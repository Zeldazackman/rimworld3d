using UnityEngine;
using Verse;
using HarmonyLib;
using System.Collections.Generic;

namespace Rim3D
{
    [StaticConstructorOnStartup]
    public class NorthMountains
    {
        private static Mesh mountainMesh;
        private static Material mountainMaterial;
        private static bool initialized = false;
        private const int MIN_SEGMENTS_PER_PEAK = 4;
        private const int MAX_SEGMENTS_PER_PEAK = 16;
        private const int MIN_DEPTH_SEGMENTS = 3;
        private const int MAX_DEPTH_SEGMENTS = 12;
        private static int CurrentSegmentsPerPeak => Mathf.RoundToInt(Mathf.Lerp(MIN_SEGMENTS_PER_PEAK, MAX_SEGMENTS_PER_PEAK, Core.settings.mountainQuality));
        private static int CurrentDepthSegments => Mathf.RoundToInt(Mathf.Lerp(MIN_DEPTH_SEGMENTS, MAX_DEPTH_SEGMENTS, Core.settings.mountainQuality));
        private const int PEAKS = 12;
        private static System.Random random;

        static NorthMountains()
        {
            mountainMesh = new Mesh();
            mountainMaterial = new Material(MatBases.FogOfWar);
            mountainMaterial.renderQueue = 2000;
            initialized = false;
        }

        public static void Initialize()
        {
            if (initialized) return;
            
            mountainMesh = new Mesh();
            mountainMaterial = new Material(MatBases.FogOfWar);
            mountainMaterial.renderQueue = 2000;
            GenerateMountainMesh();
            initialized = true;
        }

        private static float GetMountainHeight(float x, float depth, float baseHeight, float noiseOffset1, float noiseOffset2)
        {
            float noise = Mathf.PerlinNoise(x * 0.1f + noiseOffset1, depth * 0.1f + noiseOffset1);
            float secondaryNoise = Mathf.PerlinNoise(x * 0.3f + noiseOffset2, depth * 0.2f + noiseOffset2);
            return baseHeight * (noise + secondaryNoise * 0.5f);
        }

        public static void GenerateMountainMesh()
        {
            Map map = Find.CurrentMap;
            if (map == null) return;

            random = new System.Random(Core.settings.mountainSeed);
            float noiseOffset1 = (float)random.NextDouble() * 1000f;
            float noiseOffset2 = (float)random.NextDouble() * 1000f;

            int mapWidth = map.Size.x;
            int mapHeight = map.Size.z;
            float mountainMaxHeight = Core.settings.mountainHeight;
            float mountainDepth = 30f;
            float baseY = Core.settings.mountainBaseHeight;

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            // North mountains
            {
                float startX = -mapWidth * 0.25f;
                float endX = mapWidth * 1.25f;
                float segmentWidth = (endX - startX) / (PEAKS * CurrentSegmentsPerPeak);
                float mapEdgeZ = map.Size.z + Core.settings.mountainNorthDistance;

                int baseVertex = vertices.Count;

                for (int peakIndex = 0; peakIndex <= PEAKS; peakIndex++)
                {
                    for (int segIndex = 0; segIndex < CurrentSegmentsPerPeak; segIndex++)
                    {
                        float x = startX + (peakIndex * CurrentSegmentsPerPeak + segIndex) * segmentWidth;
                        
                        for (int depthIndex = 0; depthIndex <= CurrentDepthSegments; depthIndex++)
                        {
                            float depthT = depthIndex / (float)CurrentDepthSegments;
                            float z = Mathf.Lerp(mapEdgeZ, mapEdgeZ + mountainDepth, depthT);

                            float baseHeight = mountainMaxHeight * Mathf.Sin(peakIndex / (float)PEAKS * Mathf.PI);
                            float height = GetMountainHeight(x, z, baseHeight, noiseOffset1, noiseOffset2);
                            
                            if (depthT < 0.2f)
                                height *= depthT / 0.2f;
                            else if (depthT > 0.8f)
                                height *= (1 - depthT) / 0.2f;

                            vertices.Add(new Vector3(x, baseY + height, z));
                        }
                    }
                }

                for (int peakIndex = 0; peakIndex < PEAKS; peakIndex++)
                {
                    for (int segIndex = 0; segIndex < CurrentSegmentsPerPeak; segIndex++)
                    {
                        int segmentBaseVertex = baseVertex + (peakIndex * CurrentSegmentsPerPeak + segIndex) * (CurrentDepthSegments + 1);

                        for (int depthIndex = 0; depthIndex < CurrentDepthSegments; depthIndex++)
                        {
                            int nextSegmentBaseVertex = segmentBaseVertex + (CurrentDepthSegments + 1);

                            triangles.Add(segmentBaseVertex + depthIndex);
                            triangles.Add(segmentBaseVertex + depthIndex + 1);
                            triangles.Add(nextSegmentBaseVertex + depthIndex);

                            triangles.Add(nextSegmentBaseVertex + depthIndex);
                            triangles.Add(segmentBaseVertex + depthIndex + 1);
                            triangles.Add(nextSegmentBaseVertex + depthIndex + 1);
                        }
                    }
                }
            }

            // East mountains
            {
                float startZ = -mapHeight * 0.2f;
                float endZ = mapHeight * 1.2f;
                float segmentWidth = (endZ - startZ) / (PEAKS * CurrentSegmentsPerPeak);
                float mapEdgeX = map.Size.x + Core.settings.mountainEastDistance;

                int baseVertex = vertices.Count;

                for (int peakIndex = 0; peakIndex <= PEAKS; peakIndex++)
                {
                    for (int segIndex = 0; segIndex < CurrentSegmentsPerPeak; segIndex++)
                    {
                        float z = startZ + (peakIndex * CurrentSegmentsPerPeak + segIndex) * segmentWidth;
                        
                        for (int depthIndex = 0; depthIndex <= CurrentDepthSegments; depthIndex++)
                        {
                            float depthT = depthIndex / (float)CurrentDepthSegments;
                            float x = Mathf.Lerp(mapEdgeX, mapEdgeX + mountainDepth, depthT);

                            float baseHeight = mountainMaxHeight * Mathf.Sin(peakIndex / (float)PEAKS * Mathf.PI);
                            float height = GetMountainHeight(z, x, baseHeight, noiseOffset2, noiseOffset1);
                            
                            if (depthT < 0.2f)
                                height *= depthT / 0.2f;
                            else if (depthT > 0.8f)
                                height *= (1 - depthT) / 0.2f;

                            vertices.Add(new Vector3(x, baseY + height, z));
                        }
                    }
                }

                for (int peakIndex = 0; peakIndex < PEAKS; peakIndex++)
                {
                    for (int segIndex = 0; segIndex < CurrentSegmentsPerPeak; segIndex++)
                    {
                        int segmentBaseVertex = baseVertex + (peakIndex * CurrentSegmentsPerPeak + segIndex) * (CurrentDepthSegments + 1);

                        for (int depthIndex = 0; depthIndex < CurrentDepthSegments; depthIndex++)
                        {
                            int nextSegmentBaseVertex = segmentBaseVertex + (CurrentDepthSegments + 1);

                            // Invert triangle winding order for east mountains
                            triangles.Add(segmentBaseVertex + depthIndex);
                            triangles.Add(nextSegmentBaseVertex + depthIndex);
                            triangles.Add(segmentBaseVertex + depthIndex + 1);

                            triangles.Add(nextSegmentBaseVertex + depthIndex);
                            triangles.Add(nextSegmentBaseVertex + depthIndex + 1);
                            triangles.Add(segmentBaseVertex + depthIndex + 1);
                        }
                    }
                }
            }

            // West mountains
            {
                float startZ = -mapHeight * 0.2f;
                float endZ = mapHeight * 1.2f;
                float segmentWidth = (endZ - startZ) / (PEAKS * CurrentSegmentsPerPeak);
                float mapEdgeX = -Core.settings.mountainWestDistance;

                int baseVertex = vertices.Count;

                for (int peakIndex = 0; peakIndex <= PEAKS; peakIndex++)
                {
                    for (int segIndex = 0; segIndex < CurrentSegmentsPerPeak; segIndex++)
                    {
                        float z = startZ + (peakIndex * CurrentSegmentsPerPeak + segIndex) * segmentWidth;
                        
                        for (int depthIndex = 0; depthIndex <= CurrentDepthSegments; depthIndex++)
                        {
                            float depthT = depthIndex / (float)CurrentDepthSegments;
                            float x = Mathf.Lerp(mapEdgeX, mapEdgeX - mountainDepth, depthT);

                            float baseHeight = mountainMaxHeight * Mathf.Sin(peakIndex / (float)PEAKS * Mathf.PI);
                            float height = GetMountainHeight(z, x, baseHeight, noiseOffset2, noiseOffset1);
                            
                            if (depthT < 0.2f)
                                height *= depthT / 0.2f;
                            else if (depthT > 0.8f)
                                height *= (1 - depthT) / 0.2f;

                            vertices.Add(new Vector3(x, baseY + height, z));
                        }
                    }
                }

                for (int peakIndex = 0; peakIndex < PEAKS; peakIndex++)
                {
                    for (int segIndex = 0; segIndex < CurrentSegmentsPerPeak; segIndex++)
                    {
                        int segmentBaseVertex = baseVertex + (peakIndex * CurrentSegmentsPerPeak + segIndex) * (CurrentDepthSegments + 1);

                        for (int depthIndex = 0; depthIndex < CurrentDepthSegments; depthIndex++)
                        {
                            int nextSegmentBaseVertex = segmentBaseVertex + (CurrentDepthSegments + 1);

                            // Standard winding order for west mountains
                            triangles.Add(segmentBaseVertex + depthIndex);
                            triangles.Add(segmentBaseVertex + depthIndex + 1);
                            triangles.Add(nextSegmentBaseVertex + depthIndex);

                            triangles.Add(nextSegmentBaseVertex + depthIndex);
                            triangles.Add(segmentBaseVertex + depthIndex + 1);
                            triangles.Add(nextSegmentBaseVertex + depthIndex + 1);
                        }
                    }
                }
            }

            mountainMesh.Clear();
            mountainMesh.SetVertices(vertices);
            mountainMesh.SetTriangles(triangles.ToArray(), 0);
            mountainMesh.RecalculateNormals();
            mountainMesh.RecalculateBounds();
        }

        public static void Render()
        {
            if (!Core.settings.enableMountains) return;
            if (!initialized) Initialize();
            if (mountainMesh == null || mountainMaterial == null) return;

            Graphics.DrawMesh(mountainMesh, Vector3.zero, Quaternion.identity, mountainMaterial, 0, null, 0, null, true);
        }
    }

    [HarmonyPatch(typeof(MapDrawer))]
    [HarmonyPatch("DrawMapMesh")]
    public class Patch_MapDrawer_DrawMapMesh
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            NorthMountains.Render();
        }
    }
}