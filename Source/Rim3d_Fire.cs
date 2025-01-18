using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Rim3D
{
    [StaticConstructorOnStartup]
    public static class FireEffectsManager
    {
        public static AssetBundle fireBundle;
        public static GameObject firePrefab;
        private static Dictionary<int, (GameObject Effect, Thing Thing)> activeFireEffects = new Dictionary<int, (GameObject Effect, Thing Thing)>();

        static FireEffectsManager()
        {
            string bundlePath = Path.Combine(Core.Instance.Content.RootDir, "Resources", "AssetBundles", "campfire");

            fireBundle = AssetBundle.LoadFromFile(bundlePath);
            if (fireBundle != null)
            {
                firePrefab = fireBundle.LoadAsset<GameObject>("Campfire.prefab");
            }
        }

        private static bool ShouldCreateEffect(Thing thing)
        {
            if (thing == null) return false;

            string thingDefName = thing.def.defName;
            if (thingDefName == "Campfire")
                return Core.settings.enableFireCampfire;
            if (thingDefName == "TorchLamp")
                return Core.settings.enableFireTorchLamp;
            if (thingDefName == "TorchWallLamp")
                return Core.settings.enableFireTorchWallLamp;
            if (thing is Fire)
                return Core.settings.enableOtherFires;

            return Core.settings.enableOtherFires;
        }

        public static void CreateFireEffect(Thing thing, Vector3 position)
        {
            if (thing == null || firePrefab == null || !Core.mode3d) return;

            if (!ShouldCreateEffect(thing))
            {
                DestroyFireEffect(thing);
                return;
            }

            int effectId = thing.thingIDNumber;
            if (!activeFireEffects.ContainsKey(effectId))
            {
                GameObject particleObj = UnityEngine.Object.Instantiate(firePrefab, position, Quaternion.identity);
                particleObj.transform.localScale = Vector3.one;
                activeFireEffects[effectId] = (particleObj, thing);
            }
        }

        public static void CreateFireEffect(Fire fire)
        {
            if (fire == null || firePrefab == null || !Core.mode3d) return;

            if (!Core.settings.enableOtherFires)
            {
                DestroyFireEffect(fire);
                return;
            }

            List<Thing> things = fire.Map.thingGrid.ThingsListAt(fire.Position);
            CreateFireEffect(fire, fire.DrawPos);
        }

        public static void DestroyFireEffect(Thing thing)
        {
            if (thing == null) return;

            if (activeFireEffects.TryGetValue(thing.thingIDNumber, out var effectData))
            {
                UnityEngine.Object.Destroy(effectData.Effect);
                activeFireEffects.Remove(thing.thingIDNumber);
            }
        }

        public static void DestroyAllEffects()
        {
            foreach (var effect in activeFireEffects.Values)
            {
                if (effect.Effect != null)
                {
                    UnityEngine.Object.Destroy(effect.Effect);
                }
            }
            activeFireEffects.Clear();
        }

        public static void RefreshAllEffects()
        {
            var effectsToDestroy = new List<int>();

            foreach (var pair in activeFireEffects)
            {
                Thing thing = pair.Value.Thing;
                if (thing != null && !ShouldCreateEffect(thing))
                {
                    effectsToDestroy.Add(pair.Key);
                }
            }

            foreach (int id in effectsToDestroy)
            {
                if (activeFireEffects.TryGetValue(id, out var effectData))
                {
                    UnityEngine.Object.Destroy(effectData.Effect);
                    activeFireEffects.Remove(id);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Building))]
    [HarmonyPatch("DeSpawn")]
    public class Patch_Building_DeSpawn
    {
        [HarmonyPrefix]
        public static void Prefix(Building __instance)
        {
            if (!Core.mode3d) return;
            if (__instance.GetComp<CompFireOverlay>() != null)
            {
                FireEffectsManager.DestroyFireEffect(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(CompFireOverlay))]
    [HarmonyPatch("PostDraw")]
    public class Patch_CompFireOverlay_PostDraw
    {
        [HarmonyPrefix]
        public static bool Prefix(CompFireOverlay __instance)
        {
            if (!Core.mode3d) return true;

            Thing thing = __instance.parent;
            CompRefuelable refuelableComp = __instance.parent.GetComp<CompRefuelable>();
            bool shouldHaveFire = refuelableComp == null || refuelableComp.HasFuel;

            if (shouldHaveFire)
            {
                Vector3 drawPos = __instance.parent.DrawPos;
                drawPos.y += 1f / 26f;
                FireEffectsManager.CreateFireEffect(thing, drawPos);
            }
            else
            {
                FireEffectsManager.DestroyFireEffect(thing);
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Fire))]
    [HarmonyPatch("SpawnSetup")]
    public class Patch_Fire_SpawnSetup
    {
        [HarmonyPostfix]
        public static void Postfix(Fire __instance)
        {
            if (!Core.mode3d) return;
            FireEffectsManager.CreateFireEffect(__instance);
        }
    }

    [HarmonyPatch(typeof(Fire))]
    [HarmonyPatch("DeSpawn")]
    public class Patch_Fire_DeSpawn
    {
        [HarmonyPrefix]
        public static void Prefix(Fire __instance)
        {
            if (!Core.mode3d) return;
            FireEffectsManager.DestroyFireEffect(__instance);
        }
    }

    [HarmonyPatch(typeof(Graphic_Flicker))]
    [HarmonyPatch("DrawWorker")]
    [HarmonyPatch(new[] { typeof(Vector3), typeof(Rot4), typeof(ThingDef), typeof(Thing), typeof(float) })]
    public class Patch_Fire_DrawWorker
    {
        private static float INITIAL_HEIGHT = -0.04f;

        [HarmonyPrefix]
        public static bool Prefix(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation, Graphic_Flicker __instance)
        {
            if (!Core.mode3d) return true;

            if (thingDef == null)
            {
                Log.ErrorOnce("Fire DrawWorker with null thingDef: " + loc, 3427324);
                return false;
            }

            var subGraphics = AccessTools.Field(typeof(Graphic_Collection), "subGraphics").GetValue(__instance) as Graphic[];
            if (subGraphics == null)
            {
                Log.ErrorOnce("Graphic_Flicker has no subgraphics " + thingDef, 358773632);
                return false;
            }

            int num = Find.TickManager.TicksGame;
            if (thing != null)
            {
                num += Mathf.Abs(thing.thingIDNumber ^ 0x80FD52);
            }

            int num2 = num / 15;
            int num3 = Mathf.Abs(num2 ^ ((thing?.thingIDNumber ?? 0) * 391)) % subGraphics.Length;
            float num4 = 1f;
            CompFireOverlayBase compFireOverlayBase = null;
            Fire fire = thing as Fire;
            CompProperties_FireOverlay compProperties = thingDef.GetCompProperties<CompProperties_FireOverlay>();

            if (fire != null)
            {
                num4 = fire.fireSize;
            }
            else if (thing != null)
            {
                compFireOverlayBase = thing.TryGetComp<CompFireOverlayBase>();
                if (compFireOverlayBase != null)
                {
                    num4 = compFireOverlayBase.FireSize;
                }
                else
                {
                    compFireOverlayBase = thing.TryGetComp<CompDarklightOverlay>();
                    if (compFireOverlayBase != null)
                    {
                        num4 = compFireOverlayBase.FireSize;
                    }
                }
            }
            else if (compProperties != null)
            {
                num4 = compProperties.fireSize;
            }

            if (num3 < 0 || num3 >= subGraphics.Length)
            {
                Log.ErrorOnce("Fire drawing out of range: " + num3, 7453435);
                num3 = 0;
            }

            Graphic graphic = subGraphics[num3];
            float num5 = ((compFireOverlayBase == null) ? Mathf.Min(num4 / 1.2f, 1.2f) : num4);

            Vector3 vector = GenRadial.RadialPattern[num2 % GenRadial.RadialPattern.Length].ToVector3() / GenRadial.MaxRadialPatternRadius;
            vector *= 0.05f;
            Vector3 pos = loc + vector * num4;

            if (thing?.Graphic?.data != null)
            {
                pos += thing.Graphic.data.DrawOffsetForRot(rot);
            }
            if (compFireOverlayBase != null)
            {
                pos += compFireOverlayBase.Props.DrawOffsetForRot(rot);
            }

            Vector3 s = new Vector3(num5, 1f, num5);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, Quaternion.identity, s);

            float height = 0.75f;

            Vector3[] vertices = new Vector3[4] {
                new Vector3(-0.5f, INITIAL_HEIGHT, -0.5f),
                new Vector3(-0.5f, INITIAL_HEIGHT + height, -0.4f),
                new Vector3(0.5f, INITIAL_HEIGHT + height, -0.4f),
                new Vector3(0.5f, INITIAL_HEIGHT, -0.5f)
            };

            int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            Vector2[] uvs = MeshPool.plane10.uv;

            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uvs
            };
            mesh.RecalculateNormals();

            Graphics.DrawMesh(mesh, matrix, graphic.MatSingle, 0);

            return false;
        }
    }
}