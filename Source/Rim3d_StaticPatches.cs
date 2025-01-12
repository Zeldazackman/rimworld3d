using HarmonyLib;
using UnityEngine;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace Rim3D
{
    [HarmonyPatch(typeof(Printer_Plane))]
    [HarmonyPatch("PrintPlane")]
    public class Printer_Print_Patch
    {
        private static readonly Dictionary<Thing, bool> processingStates = new Dictionary<Thing, bool>();
        private static readonly HashSet<string> alwaysFlat = new HashSet<string> {
            "Filth", "RubbleRock", "SteamGeyser"
        };

        private static readonly HashSet<ThingCategory> flatCategories = new HashSet<ThingCategory> {
            ThingCategory.Filth
        };

        [HarmonyPrefix]
        public static bool Prefix(SectionLayer layer, Vector3 center, Vector2 size, Material mat, float rot = 0f, bool flipUv = false, Vector2[] uvs = null, Color32[] colors = null, float topVerticesAltitudeBias = 0.01f, float uvzPayload = 0f)
        {
            if (!Core.mode3d || PrintingTracker.CurrentThing == null) return true;

            if (!processingStates.ContainsKey(PrintingTracker.CurrentThing))
                processingStates[PrintingTracker.CurrentThing] = false;

            if (processingStates[PrintingTracker.CurrentThing]) return true;

            try
            {
                processingStates[PrintingTracker.CurrentThing] = true;

                string defName = PrintingTracker.CurrentThing.def?.defName ?? "";
                bool isPlant = PrintingTracker.CurrentThing is Plant;
                bool shouldBePlane = alwaysFlat.Any(text => defName.Contains(text)) ||
                                   flatCategories.Contains(PrintingTracker.CurrentThing.def.category);

                var config = ObjectConfigManager.GetConfigForThing(PrintingTracker.CurrentThing);

                if (config != null)
                {
                    shouldBePlane = config.IsPlane;
                }

                if (colors == null)
                {
                    colors = AccessTools.Field(typeof(Printer_Plane), "defaultColors").GetValue(null) as Color32[];
                }
                if (uvs == null)
                {
                    uvs = flipUv ?
                        AccessTools.Field(typeof(Printer_Plane), "defaultUvsFlipped").GetValue(null) as Vector2[] :
                        AccessTools.Field(typeof(Printer_Plane), "defaultUvs").GetValue(null) as Vector2[];
                }

                if (isPlant)
                {
                    float randomOffset = Rand.Range(-0.1f, 0.1f);
                    center.z += randomOffset;
                }

                if (PrintingTracker.CurrentThing.def.category == ThingCategory.Item)
                {
                    center.y -= 0.2f;
                }

                LayerSubMesh subMesh = layer.GetSubMesh(mat);

                if (config != null && config.IsCube)
                {
                    if (!config.BoxSettings.Any())
                    {
                        config.InitializeDefaultBoxSettings(size);
                    }
                    CubeRenderer.RenderCube(subMesh, center, size, rot, uvs, colors, topVerticesAltitudeBias, uvzPayload);
                    CubeRenderer.RenderCube(subMesh, center, size, rot, uvs, colors, topVerticesAltitudeBias, uvzPayload);
                    CubeRenderer.RenderCube(subMesh, center, size, rot, uvs, colors, topVerticesAltitudeBias, uvzPayload);
                    CubeRenderer.RenderCube(subMesh, center, size, rot, uvs, colors, topVerticesAltitudeBias, uvzPayload);
                    return false;
                }
                else if (shouldBePlane)
                {
                    int count = subMesh.verts.Count;
                    subMesh.verts.Add(new Vector3(-0.5f * size.x, config?.HeightOffset ?? 0f, -0.5f * size.y));
                    subMesh.verts.Add(new Vector3(-0.5f * size.x, (config?.HeightOffset ?? 0f) + topVerticesAltitudeBias, 0.5f * size.y));
                    subMesh.verts.Add(new Vector3(0.5f * size.x, (config?.HeightOffset ?? 0f) + topVerticesAltitudeBias, 0.5f * size.y));
                    subMesh.verts.Add(new Vector3(0.5f * size.x, config?.HeightOffset ?? 0f, -0.5f * size.y));

                    if (rot != 0f)
                    {
                        float angle = rot * ((float)System.Math.PI / 180f) * -1f;
                        for (int i = 0; i < 4; i++)
                        {
                            Vector3 before = subMesh.verts[count + i];
                            float x = before.x;
                            float z = before.z;
                            float cos = Mathf.Cos(angle);
                            float sin = Mathf.Sin(angle);
                            subMesh.verts[count + i] = new Vector3(
                                x * cos - z * sin,
                                before.y,
                                x * sin + z * cos
                            );
                        }
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 before = subMesh.verts[count + i];
                        subMesh.verts[count + i] += center;
                        subMesh.uvs.Add(new Vector3(uvs[i].x, uvs[i].y, uvzPayload));
                        subMesh.colors.Add(colors[i]);
                    }

                    subMesh.tris.Add(count);
                    subMesh.tris.Add(count + 1);
                    subMesh.tris.Add(count + 2);
                    subMesh.tris.Add(count);
                    subMesh.tris.Add(count + 2);
                    subMesh.tris.Add(count + 3);

                    return false;
                }
                else
                {
                    center.y -= 0.2f;
                    if (config != null)
                    {
                        center.y += config.HeightOffset;
                    }
                    int count = subMesh.verts.Count;
                    subMesh.verts.Add(new Vector3(-0.5f * size.x, 0f, -0.5f * size.y));
                    subMesh.verts.Add(new Vector3(-0.5f * size.x, size.y, 0f));
                    subMesh.verts.Add(new Vector3(0.5f * size.x, size.y, 0f));
                    subMesh.verts.Add(new Vector3(0.5f * size.x, 0f, -0.5f * size.y));

                    if (rot != 0f)
                    {
                        float angle = rot * ((float)System.Math.PI / 180f) * -1f;
                        for (int i = 0; i < 4; i++)
                        {
                            Vector3 before = subMesh.verts[count + i];
                            float x = before.x;
                            float z = before.z;
                            float cos = Mathf.Cos(angle);
                            float sin = Mathf.Sin(angle);
                            subMesh.verts[count + i] = new Vector3(
                                x * cos - z * sin,
                                before.y,
                                x * sin + z * cos
                            );
                        }
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 before = subMesh.verts[count + i];
                        subMesh.verts[count + i] += center;
                        subMesh.uvs.Add(new Vector3(uvs[i].x, uvs[i].y, uvzPayload));
                        subMesh.colors.Add(colors[i]);
                    }

                    subMesh.tris.Add(count);
                    subMesh.tris.Add(count + 1);
                    subMesh.tris.Add(count + 2);
                    subMesh.tris.Add(count);
                    subMesh.tris.Add(count + 2);
                    subMesh.tris.Add(count + 3);

                    return false;
                }
            }
            finally
            {
                processingStates[PrintingTracker.CurrentThing] = false;
            }
        }
    }
}