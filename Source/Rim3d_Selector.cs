using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;

namespace Rim3D
{
    [HarmonyPatch(typeof(SelectionDrawer))]
    [HarmonyPatch("DrawSelectionBracketFor")]
    public class Patch_SelectionDrawer_DrawSelectionBracketFor
    {
        [HarmonyPrefix]
        public static bool Prefix(object obj, Material overrideMat = null)
        {
            if (!Core.mode3d)
            {
                return true;
            }

            if (obj is Zone zone)
            {
                GenDraw.DrawFieldEdges(zone.Cells);
            }
            else
            {
                if (!(obj is Thing { CustomRectForSelector: var customRectForSelector, DrawPos: var carryDrawPos } thing))
                {
                    return false;
                }

                Vector3[] bracketLocs = new Vector3[4];
                if (customRectForSelector.HasValue)
                {
                    SelectionDrawerUtility.CalculateSelectionBracketPositionsWorld(bracketLocs, thing, customRectForSelector.Value.CenterVector3, new Vector2(customRectForSelector.Value.Width, customRectForSelector.Value.Height), SelectionDrawer.SelectTimes, Vector2.one, 1f, thing.def.deselectedSelectionBracketFactor);
                }
                else if (thing.SpawnedParentOrMe is Pawn pawn && pawn != thing)
                {
                    carryDrawPos = pawn.DrawPos;
                    PawnRenderUtility.CalculateCarriedDrawPos(pawn, thing, ref carryDrawPos, out var _);
                    SelectionDrawerUtility.CalculateSelectionBracketPositionsWorld(bracketLocs, thing, carryDrawPos, thing.RotatedSize.ToVector2(), SelectionDrawer.SelectTimes, Vector2.one, 1f, thing.def.deselectedSelectionBracketFactor);
                }
                else if (thing.SpawnedParentOrMe is Building_Enterable building_Enterable && building_Enterable != thing)
                {
                    SelectionDrawerUtility.CalculateSelectionBracketPositionsWorld(bracketLocs, thing, building_Enterable.DrawPos + building_Enterable.PawnDrawOffset, thing.RotatedSize.ToVector2(), SelectionDrawer.SelectTimes, Vector2.one, 1f, thing.def.deselectedSelectionBracketFactor);
                }
                else
                {
                    if (!thing.DrawPosHeld.HasValue)
                    {
                        return false;
                    }
                    carryDrawPos = thing.DrawPosHeld.Value;
                    SelectionDrawerUtility.CalculateSelectionBracketPositionsWorld(bracketLocs, thing, carryDrawPos, thing.RotatedSize.ToVector2(), SelectionDrawer.SelectTimes, Vector2.one, 1f, thing.def.deselectedSelectionBracketFactor);
                }

                float scale = (thing.MultipleItemsPerCellDrawn() ? 0.8f : 1f);
                float scaleY = 1f;
                CameraDriver cameraDriver = Find.CameraDriver;
                float fadeOut = Mathf.Clamp01(Mathf.InverseLerp(cameraDriver.config.sizeRange.max * 0.84999996f, cameraDriver.config.sizeRange.max, cameraDriver.ZoomRootSize));
                
                if (thing is Pawn)
                {
                    if (thing.def.Size == IntVec2.One)
                    {
                        scale *= Mathf.Min(1f + fadeOut / 2f, 2f);
                    }
                    else
                    {
                        scaleY = Mathf.Min(1f + fadeOut / 2f, 2f);
                    }
                }

                // Lower height for 3D mode
                for (int i = 0; i < 4; i++)
                {
                    bracketLocs[i].y = 0.1f;
                }

                Material selectionBracketMat = AccessTools.StaticFieldRefAccess<Material>(typeof(SelectionDrawer), "SelectionBracketMat");

                int angle = 0;
                for (int i = 0; i < 4; i++)
                {
                    Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                    Vector3 pos = (bracketLocs[i] - carryDrawPos) * scale + carryDrawPos;
                    Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(pos, rotation, new Vector3(scale, 1f, scale) * scaleY), overrideMat ?? selectionBracketMat, 0);
                    angle -= 90;
                }
            }

            return false;
        }
    }
}