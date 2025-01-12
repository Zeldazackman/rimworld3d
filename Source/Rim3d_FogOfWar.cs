using HarmonyLib;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using RimWorld;

namespace Rim3D
{
    public class MountainGroup
    {
        public int Id { get; set; }
        public List<Vector2Int> Cells { get; set; }
        public List<Vector2Int> BorderCells { get; set; }
        public Vector2 Center { get; set; }
        public float MaxHeight { get; set; }

        public MountainGroup(int id)
        {
            Id = id;
            Cells = new List<Vector2Int>();
            BorderCells = new List<Vector2Int>();
            Center = Vector2.zero;
            MaxHeight = Core.settings.fogMaxHeight;
        }
    }

    public static class FogOfWarManager
    {
        private static Dictionary<int, MountainGroup> mountainGroups = new Dictionary<int, MountainGroup>();
        private static bool[,] processed;
        private static int currentGroupId = 0;

        public static void RegenerateMountains(Map map)
        {
            if (map == null || !Core.settings.enableMapMountains) return;

            mountainGroups.Clear();
            currentGroupId = 0;
            processed = new bool[map.Size.x, map.Size.z];

            for (int x = 0; x < map.Size.x; x++)
            {
                for (int z = 0; z < map.Size.z; z++)
                {
                    if (!processed[x, z] && map.fogGrid.IsFogged(new IntVec3(x, 0, z)))
                    {
                        FloodFill(map, x, z);
                    }
                }
            }

            CalculateMountainCenters(map);
        }

        public static void ClearMeshData(LayerSubMesh subMesh)
        {
            subMesh.verts.Clear();
            subMesh.tris.Clear();
            subMesh.mesh.Clear();
        }

        private static void FloodFill(Map map, int startX, int startZ)
        {
            if (processed[startX, startZ]) return;

            MountainGroup group = new MountainGroup(currentGroupId++);
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            Vector2Int start = new Vector2Int(startX, startZ);
            queue.Enqueue(start);
            processed[startX, startZ] = true;

            int[] dx = { -1, 0, 1, 0 };
            int[] dz = { 0, 1, 0, -1 };

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                group.Cells.Add(current);

                for (int i = 0; i < 4; i++)
                {
                    int nx = current.x + dx[i];
                    int nz = current.y + dz[i];

                    if (nx >= 0 && nx < map.Size.x && nz >= 0 && nz < map.Size.z &&
                        !processed[nx, nz] && map.fogGrid.IsFogged(new IntVec3(nx, 0, nz)))
                    {
                        processed[nx, nz] = true;
                        queue.Enqueue(new Vector2Int(nx, nz));
                    }
                }
            }

            mountainGroups[group.Id] = group;
        }

        private static void CalculateMountainCenters(Map map)
        {
            foreach (var group in mountainGroups.Values)
            {
                float sumX = 0, sumZ = 0;
                foreach (var cell in group.Cells)
                {
                    sumX += cell.x;
                    sumZ += cell.y;
                }
                group.Center = new Vector2(sumX / group.Cells.Count, sumZ / group.Cells.Count);

                foreach (var cell in group.Cells)
                {
                    bool isBorder = false;
                    int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
                    int[] dz = { -1, 0, 1, -1, 1, -1, 0, 1 };

                    for (int i = 0; i < 8; i++)
                    {
                        int nx = cell.x + dx[i];
                        int nz = cell.y + dz[i];

                        if (nx >= 0 && nx < map.Size.x && nz >= 0 && nz < map.Size.z &&
                            !map.fogGrid.IsFogged(new IntVec3(nx, 0, nz)))
                        {
                            isBorder = true;
                            break;
                        }
                    }

                    if (isBorder)
                    {
                        group.BorderCells.Add(cell);
                    }
                }
            }
        }

        public static float GetVertexHeight(Vector3 vertex, Map map)
        {
            if (!Core.settings.enableMapMountains) return Core.settings.fogHeight;

            float baseHeight = Core.settings.fogHeight;
            Vector2Int cell = new Vector2Int(Mathf.RoundToInt(vertex.x), Mathf.RoundToInt(vertex.z));

            foreach (var group in mountainGroups.Values)
            {
                if (group.Cells.Contains(cell))
                {
                    if (group.BorderCells.Contains(cell))
                    {
                        return baseHeight;
                    }

                    float minDistToBorder = float.MaxValue;
                    foreach (var borderCell in group.BorderCells)
                    {
                        float dist = Vector2.Distance(new Vector2(cell.x, cell.y), new Vector2(borderCell.x, borderCell.y));
                        minDistToBorder = Mathf.Min(minDistToBorder, dist);
                    }

                    float noiseValue = Mathf.PerlinNoise(cell.x * 0.1f, cell.y * 0.1f);
                    float heightFactor = Mathf.Clamp01(minDistToBorder / 5f);
                    return baseHeight + heightFactor * group.MaxHeight * (0.8f + 0.4f * noiseValue);
                }
            }

            return baseHeight;
        }
    }

    [HarmonyPatch(typeof(FogGrid))]
    [HarmonyPatch("UnfogWorker")]
    public class Patch_FogGrid_UnfogWorker
    {
        [HarmonyPostfix]
        public static void Postfix(FogGrid __instance, IntVec3 c, Map ___map)
        {
            if (!Core.mode3d || !Core.settings.refreshAdjacentFog || !Core.settings.enableMapMountains) return;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    IntVec3 neighborPos = new IntVec3(c.x + dx * 17, 0, c.z + dz * 17);
                    if (neighborPos.InBounds(___map))
                    {
                        ___map.mapDrawer.MapMeshDirty(neighborPos, MapMeshFlagDefOf.FogOfWar);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(SectionLayer_FogOfWar))]
    [HarmonyPatch("Regenerate")]
    public class Patch_FogOfWar_Regenerate
    {
        [HarmonyPrefix]
        public static void Prefix(SectionLayer_FogOfWar __instance)
        {
            if (!Core.settings.enableMapMountains) return;

            LayerSubMesh subMesh = __instance.GetSubMesh(MatBases.FogOfWar);
            FogOfWarManager.ClearMeshData(subMesh);
            FogOfWarManager.RegenerateMountains(Find.CurrentMap);
        }
    }

    [HarmonyPatch(typeof(SectionLayerGeometryMaker_Solid))]
    [HarmonyPatch("MakeBaseGeometry")]
    public class Patch_FogOfWar_MakeBaseGeometry
    {
        [HarmonyPrefix]
        public static bool Prefix(Section section, LayerSubMesh sm, AltitudeLayer altitudeLayer)
        {
            if (altitudeLayer == AltitudeLayer.FogOfWar && Core.settings.enableMapMountains)
            {
                sm.Clear(MeshParts.Verts | MeshParts.Tris);
                CellRect cellRect = new CellRect(section.botLeft.x, section.botLeft.z, 17, 17);
                cellRect.ClipInsideMap(section.map);
                sm.verts.Capacity = cellRect.Area * 9;

                for (int i = cellRect.minX; i <= cellRect.maxX; i++)
                {
                    for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
                    {
                        Vector3[] positions = new Vector3[]
                        {
                            new Vector3(i, 0, j),
                            new Vector3(i, 0, j + 0.5f),
                            new Vector3(i, 0, j + 1),
                            new Vector3(i + 0.5f, 0, j + 1),
                            new Vector3(i + 1, 0, j + 1),
                            new Vector3(i + 1, 0, j + 0.5f),
                            new Vector3(i + 1, 0, j),
                            new Vector3(i + 0.5f, 0, j),
                            new Vector3(i + 0.5f, 0, j + 0.5f)
                        };

                        for (int v = 0; v < positions.Length; v++)
                        {
                            positions[v].y = FogOfWarManager.GetVertexHeight(positions[v], section.map);
                            sm.verts.Add(positions[v]);
                        }
                    }
                }

                int num = cellRect.Area * 8 * 3;
                sm.tris.Capacity = num;
                int num2 = 0;
                while (sm.tris.Count < num)
                {
                    sm.tris.Add(num2 + 7);
                    sm.tris.Add(num2);
                    sm.tris.Add(num2 + 1);
                    sm.tris.Add(num2 + 1);
                    sm.tris.Add(num2 + 2);
                    sm.tris.Add(num2 + 3);
                    sm.tris.Add(num2 + 3);
                    sm.tris.Add(num2 + 4);
                    sm.tris.Add(num2 + 5);
                    sm.tris.Add(num2 + 5);
                    sm.tris.Add(num2 + 6);
                    sm.tris.Add(num2 + 7);
                    sm.tris.Add(num2 + 7);
                    sm.tris.Add(num2 + 1);
                    sm.tris.Add(num2 + 8);
                    sm.tris.Add(num2 + 1);
                    sm.tris.Add(num2 + 3);
                    sm.tris.Add(num2 + 8);
                    sm.tris.Add(num2 + 3);
                    sm.tris.Add(num2 + 5);
                    sm.tris.Add(num2 + 8);
                    sm.tris.Add(num2 + 5);
                    sm.tris.Add(num2 + 7);
                    sm.tris.Add(num2 + 8);
                    num2 += 9;
                }
                sm.FinalizeMesh(MeshParts.Verts | MeshParts.Tris);
                return false;
            }
            return true;
        }
    }
}