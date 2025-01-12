using HarmonyLib;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace Rim3D
{
    [StaticConstructorOnStartup]
    public static class CarriedThingsTracker
    {
        private static readonly Dictionary<int, Pawn> meshToPawn = new Dictionary<int, Pawn>();

        public static void RegisterMesh(int meshId, Pawn pawn)
        {
            meshToPawn[meshId] = pawn;
        }

        public static Pawn GetPawnForMesh(int meshId)
        {
            if (meshToPawn.TryGetValue(meshId, out Pawn pawn))
                return pawn;
            return null;
        }

        public static void ClearMeshData(int meshId)
        {
            meshToPawn.Remove(meshId);
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "GetBlitMeshUpdatedFrame")]
    class Patch_GetBlitMeshUpdatedFrame
    {
        static void Postfix(PawnRenderer __instance, Mesh __result, PawnTextureAtlasFrameSet frameSet, Rot4 rotation, PawnDrawMode drawMode)
        {
            if (!Core.mode3d) return;

            Pawn pawn = AccessTools.Field(typeof(PawnRenderer), "pawn").GetValue(__instance) as Pawn;
            if (pawn != null && __result != null)
            {
                CarriedThingsTracker.RegisterMesh(__result.GetInstanceID(), pawn);
            }
        }
    }

    [HarmonyPatch(typeof(GenDraw))]
    [HarmonyPatch("DrawMeshNowOrLater")]
    [HarmonyPatch(new[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool) })]
    class Patch_DrawMeshNowOrLater
    {
        static void Postfix(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow)
        {
            if (!Core.mode3d) return;

            if (mesh == null) return;

            Pawn pawn = CarriedThingsTracker.GetPawnForMesh(mesh.GetInstanceID());
            if (pawn?.carryTracker?.CarriedThing == null) return;

            Vector3 pos = loc;
            pos.y += 0.3f;

            PawnRenderUtility.DrawCarriedThing(pawn, pos, pawn.carryTracker.CarriedThing);
            CarriedThingsTracker.ClearMeshData(mesh.GetInstanceID());
        }
    }
}