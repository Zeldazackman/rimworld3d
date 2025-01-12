using HarmonyLib;
using UnityEngine;
using RimWorld;
using Verse;
using System;

namespace Rim3D
{
    [HarmonyPatch(typeof(PawnRenderer))]
    [HarmonyPatch("RenderPawnAt")]
    public class Patch_PawnRenderer_RenderPawnAt
    {
        private static Type pawnPreRenderResultsType;

        [HarmonyPrefix]
        public static bool Prefix(PawnRenderer __instance, Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
        {
            if (!Core.mode3d)
            {
                return true;
            }
            var resultsField = AccessTools.Field(typeof(PawnRenderer), "results");
            if (pawnPreRenderResultsType == null)
            {
                pawnPreRenderResultsType = resultsField.FieldType;
            }

            var results = resultsField.GetValue(__instance);
            bool valid = (bool)AccessTools.Field(pawnPreRenderResultsType, "valid").GetValue(results);

            if (!valid)
            {
                AccessTools.Method(typeof(PawnRenderer), "EnsureGraphicsInitialized").Invoke(__instance, null);
                AccessTools.Method(typeof(PawnRenderer), "ParallelPreRenderPawnAt").Invoke(__instance, new object[] { drawLoc, rotOverride, neverAimWeapon });
                results = resultsField.GetValue(__instance);
            }

            bool draw = (bool)AccessTools.Field(pawnPreRenderResultsType, "draw").GetValue(results);
            if (!draw)
            {
                return false;
            }

            Pawn pawn = AccessTools.Field(typeof(PawnRenderer), "pawn").GetValue(__instance) as Pawn;
            bool useCached = (bool)AccessTools.Field(pawnPreRenderResultsType, "useCached").GetValue(results);
            bool alwayCache = true;

            if (alwayCache)
            {
                if (GlobalTextureAtlasManager.TryGetPawnFrameSet(pawn, out var frameSet, out var _))
                {
                    using (new ProfilerBlock("Draw Cached Mesh"))
                    {
                        var parmsField = AccessTools.Field(pawnPreRenderResultsType, "parms");
                        var parms = parmsField.GetValue(results);
                        Type parmsType = parmsField.FieldType;

                        Rot4 facing = (Rot4)AccessTools.Field(parmsType, "facing").GetValue(parms);
                        Vector3 bodyPos = (Vector3)AccessTools.Field(pawnPreRenderResultsType, "bodyPos").GetValue(results);
                        float bodyAngle = (float)AccessTools.Field(pawnPreRenderResultsType, "bodyAngle").GetValue(results);
                        bool showBody = (bool)AccessTools.Field(pawnPreRenderResultsType, "showBody").GetValue(results);
                        PawnDrawMode drawMode = (!showBody) ? PawnDrawMode.HeadOnly : PawnDrawMode.BodyAndHead;

                        Material mat = AccessTools.Method(typeof(PawnRenderer), "OverrideMaterialIfNeeded").Invoke(__instance, new object[] {
                            MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Cutout)),
                            PawnRenderFlags.None
                        }) as Material;

                        GenDraw.DrawMeshNowOrLater(
                            AccessTools.Method(typeof(PawnRenderer), "GetBlitMeshUpdatedFrame").Invoke(__instance, new object[] { frameSet, facing, drawMode }) as Mesh,
                            bodyPos,
                            Quaternion.AngleAxis(bodyAngle, Vector3.up),
                            mat,
                            drawNow: false
                        );

                        Vector3 drawPos = bodyPos.WithYOffset(PawnRenderUtility.AltitudeForLayer((facing == Rot4.North) ? (-10f) : 90f));
                        PawnRenderFlags flags = (PawnRenderFlags)AccessTools.Field(parmsType, "flags").GetValue(parms);
                        PawnRenderUtility.DrawEquipmentAndApparelExtras(pawn, drawPos, facing, flags);
                    }
                }
                else
                {
                    Log.ErrorOnce($"Attempted to use a cached frame set for pawn {pawn.Name} but failed to get one.", Gen.HashCombine(pawn.GetHashCode(), 5938111));
                }
            }
            else
            {
                using (new ProfilerBlock("Render Pawn Internal"))
                {
                    var parms = AccessTools.Field(pawnPreRenderResultsType, "parms").GetValue(results);
                    AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal").Invoke(__instance, new object[] { parms });
                }
            }

            PawnPosture posture = (PawnPosture)AccessTools.Field(pawnPreRenderResultsType, "posture").GetValue(results);
            var parms2 = AccessTools.Field(pawnPreRenderResultsType, "parms").GetValue(results);
            Type parms2Type = AccessTools.Field(pawnPreRenderResultsType, "parms").FieldType;
            PawnRenderFlags flags2 = (PawnRenderFlags)AccessTools.Field(parms2Type, "flags").GetValue(parms2);

            if (posture == PawnPosture.Standing && !flags2.HasFlag(PawnRenderFlags.Invisible))
            {
                using (new ProfilerBlock("Draw Shadow Internal"))
                {
                    AccessTools.Method(typeof(PawnRenderer), "DrawShadowInternal").Invoke(__instance, new object[] { drawLoc });
                }
            }

            if (pawn.Spawned && !pawn.Dead)
            {
                pawn.stances.StanceTrackerDraw();
                pawn.pather.PatherDraw();
                pawn.roping.RopingDraw();
            }

            Graphic bodyGraphic = AccessTools.Property(typeof(PawnRenderer), "BodyGraphic").GetValue(__instance) as Graphic;
            Graphic graphic = pawn.RaceProps.Humanlike ?
                pawn.ageTracker.CurLifeStage.silhouetteGraphicData.Graphic :
                (pawn.ageTracker.CurKindLifeStage.silhouetteGraphicData == null ?
                    bodyGraphic :
                    pawn.ageTracker.CurKindLifeStage.silhouetteGraphicData.Graphic);

            Vector3 bodyPos2 = (Vector3)AccessTools.Field(pawnPreRenderResultsType, "bodyPos").GetValue(results);
            AccessTools.Method(typeof(PawnRenderer), "SetSilhouetteData").Invoke(__instance, new object[] { graphic, bodyPos2 });
            AccessTools.Method(typeof(PawnRenderer), "DrawDebug").Invoke(__instance, null);

            var defaultValue = Activator.CreateInstance(pawnPreRenderResultsType);
            resultsField.SetValue(__instance, defaultValue);

            return false;
        }
    }
}