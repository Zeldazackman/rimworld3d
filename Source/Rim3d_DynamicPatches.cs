using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;

namespace Rim3D
{
    [HarmonyPatch(typeof(Building_Door))]
    [HarmonyPatch("DrawAt")]
    public static class Patch_Building_Door_DrawAt
    {
        [HarmonyPrefix]
        public static bool Prefix(Building_Door __instance, Vector3 drawLoc, bool flip = false)
        {
            if (!Core.mode3d) return true;
            if (__instance.def.size == IntVec2.One)
            {
                Door3D.DrawAt(__instance, drawLoc, flip);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Building_MultiTileDoor))]
    [HarmonyPatch("DrawAt")]
    public static class Patch_Building_MultiTileDoor_DrawAt
    {
        private static readonly float UpperMoverAltitude = AltitudeLayer.DoorMoveable.AltitudeFor() + 1f / 52f;
        private static readonly Vector3 MoverDrawScale = new Vector3(0.5f, 1f, 1f);

        [HarmonyPrefix]
        public static bool Prefix(Building_MultiTileDoor __instance, Vector3 drawLoc, bool flip = false)
        {
            if (!Core.mode3d) return true;
            Door3D.DrawAtMultiTile(__instance, drawLoc, flip, UpperMoverAltitude, MoverDrawScale);
            return false;
        }
    }

    public static class Door3D
    {
        private static readonly FieldInfo ticksSinceOpenField = AccessTools.Field(typeof(Building_Door), "ticksSinceOpen");
        private static readonly MethodInfo ticksToOpenNowMethod = AccessTools.Method(typeof(Building_Door), "get_TicksToOpenNow");
        private static readonly FieldInfo suppportGraphicField = AccessTools.Field(typeof(Building_MultiTileDoor), "suppportGraphic");
        private static readonly FieldInfo topGraphicField = AccessTools.Field(typeof(Building_MultiTileDoor), "topGraphic");
        private static readonly PropertyInfo stuckOpenProperty = AccessTools.Property(typeof(Building_Door), "StuckOpen");

        public static void DrawAtMultiTile(Building_MultiTileDoor door, Vector3 drawLoc, bool flip, float upperMoverAltitude, Vector3 moverDrawScale)
        {
            DoorPreDraw(door);
            bool stuckOpen = (bool)stuckOpenProperty.GetValue(door);

            if (!stuckOpen)
            {
                int ticksSinceOpen = (int)ticksSinceOpenField.GetValue(door);
                int ticksToOpenNow = (int)ticksToOpenNowMethod.Invoke(door, null);
                float openPct = Mathf.Clamp01((float)ticksSinceOpen / (float)ticksToOpenNow);

                bool isHorizontal = door.Rotation == Rot4.North || door.Rotation == Rot4.South;

                if (isHorizontal)
                {
                    float baseAltitude = 0.98f;
                    float offsetDist = 0.25f + 0.35000002f * openPct;

                    DrawBasicDoor(door, drawLoc, door.Graphic, baseAltitude, moverDrawScale, door.Graphic.ShadowGraphic, offsetDist);

                    if (door.def.building.upperMoverGraphic != null)
                    {
                        float offsetDist2 = 0.25f + 0.35000002f * Mathf.Clamp01(openPct * 2.5f);
                        DrawBasicDoor(door, drawLoc, door.def.building.upperMoverGraphic.Graphic, baseAltitude + 0.2f, moverDrawScale, null, offsetDist2);
                    }

                    DrawHorizontalBigDoorSouthPiece(door, drawLoc, offsetDist, moverDrawScale);
                    DrawHorizontalBigDoorArchFrame(door, drawLoc);
                }
                else
                {
                    float offsetDist = 0.25f + 0.35000002f * openPct;
                    DrawVerticalBigDoorCeilingPiece(door, drawLoc, offsetDist, moverDrawScale);

                    if (door.def.building.upperMoverGraphic != null)
                    {
                        float offsetDist2 = 0.25f + 0.35000002f * Mathf.Clamp01(openPct * 2.5f);
                        DrawVerticalBigDoorCeilingPiece(door, drawLoc, offsetDist2, moverDrawScale);
                    }

                    DrawVerticalBigDoorLeftPiece(door, drawLoc, offsetDist, moverDrawScale);
                    DrawVerticalBigDoorRightPiece(door, drawLoc, offsetDist, moverDrawScale);
                }
            }
        }

        private static void DrawVerticalBigDoorCeilingPiece(Building_MultiTileDoor door, Vector3 drawLoc, float offsetDist, Vector3 moverDrawScale)
        {
            Vector3 ceilingDrawLoc = drawLoc;
            ceilingDrawLoc.y = 0.98f;

            ThingDef def = door.def;

            for (int i = 0; i < 2; i++)
            {
                Mesh mesh;
                Vector3 vector;
                if (i == 0)
                {
                    vector = new Vector3(0f, 0f, -def.size.x);
                    mesh = MeshPool.plane10Flip;
                }
                else
                {
                    vector = new Vector3(0f, 0f, def.size.x);
                    mesh = MeshPool.plane10;
                }

                Vector3 vector2 = ceilingDrawLoc;
                vector2 += vector * offsetDist;

                Graphics.DrawMesh(
                    mesh,
                    Matrix4x4.TRS(vector2, door.Rotation.AsQuat, new Vector3((float)def.size.x * moverDrawScale.x, moverDrawScale.y, (float)def.size.z * moverDrawScale.z)),
                    door.Graphic.MatAt(door.Rotation, door),
                    0
                );
                door.Graphic.ShadowGraphic?.DrawWorker(vector2, door.Rotation, def, door, 0f);
            }
        }

        private static void DrawVerticalBigDoorLeftPiece(Building_MultiTileDoor door, Vector3 drawLoc, float offsetDist, Vector3 moverDrawScale)
        {
            Vector3 leftDrawLoc = drawLoc;
            leftDrawLoc.y = 0.5f;
            leftDrawLoc.x -= 0.47f;

            ThingDef def = door.def;

            for (int i = 0; i < 2; i++)
            {
                Mesh mesh;
                Vector3 vector;
                if (i == 0)
                {
                    vector = new Vector3(0f, 0f, -def.size.x);
                    mesh = MeshPool.plane10;
                }
                else
                {
                    vector = new Vector3(0f, 0f, def.size.x);
                    mesh = MeshPool.plane10Flip;
                }

                Vector3 vector2 = leftDrawLoc;
                vector2 += vector * offsetDist;

                Graphics.DrawMesh(
                    mesh,
                    Matrix4x4.TRS(vector2, Quaternion.Euler(90f, -90f, 0f), new Vector3((float)def.size.x * moverDrawScale.x, moverDrawScale.y, (float)def.size.z * moverDrawScale.z)),
                    door.Graphic.MatAt(door.Rotation, door),
                    0
                );
                door.Graphic.ShadowGraphic?.DrawWorker(vector2, door.Rotation, def, door, 0f);
            }
        }

        private static void DrawVerticalBigDoorRightPiece(Building_MultiTileDoor door, Vector3 drawLoc, float offsetDist, Vector3 moverDrawScale)
        {
            Vector3 rightDrawLoc = drawLoc;
            rightDrawLoc.y = 0.5f;
            rightDrawLoc.x += 0.47f;

            ThingDef def = door.def;

            for (int i = 0; i < 2; i++)
            {
                Mesh mesh;
                Vector3 vector;
                if (i == 0)
                {
                    vector = new Vector3(0f, 0f, -def.size.x);
                    mesh = MeshPool.plane10Flip;
                }
                else
                {
                    vector = new Vector3(0f, 0f, def.size.x);
                    mesh = MeshPool.plane10;
                }

                Vector3 vector2 = rightDrawLoc;
                vector2 += vector * offsetDist;

                Graphics.DrawMesh(
                    mesh,
                    Matrix4x4.TRS(vector2, Quaternion.Euler(90f, 90f, 0f), new Vector3((float)def.size.x * moverDrawScale.x, moverDrawScale.y, (float)def.size.z * moverDrawScale.z)),
                    door.Graphic.MatAt(door.Rotation, door),
                    0
                );
                door.Graphic.ShadowGraphic?.DrawWorker(vector2, door.Rotation, def, door, 0f);
            }
        }

        private static void DrawHorizontalBigDoorSouthPiece(Building_MultiTileDoor door, Vector3 drawLoc, float offsetDist, Vector3 moverDrawScale)
        {
            Vector3 southDrawLoc = drawLoc;
            southDrawLoc.y = 0.5f;
            southDrawLoc.z -= 0.47f;

            ThingDef def = door.def;
            Rot4 rotation = door.Rotation;

            for (int i = 0; i < 2; i++)
            {
                Mesh mesh;
                Vector3 vector;
                if (i == 0)
                {
                    vector = new Vector3(-def.size.x, 0f, 0f);
                    mesh = MeshPool.plane10Flip;
                }
                else
                {
                    vector = new Vector3(def.size.x, 0f, 0f);
                    mesh = MeshPool.plane10;
                }
                Vector3 vector2 = southDrawLoc;
                vector2 += vector * offsetDist;

                Quaternion rotation90 = Quaternion.Euler(90f, 180f, 0f);
                Graphics.DrawMesh(
                    mesh,
                    Matrix4x4.TRS(vector2, rotation90, new Vector3((float)def.size.x * moverDrawScale.x, moverDrawScale.y, (float)def.size.z * moverDrawScale.z)),
                    door.Graphic.MatAt(door.Rotation, door),
                    0
                );
                door.Graphic.ShadowGraphic?.DrawWorker(vector2, door.Rotation, def, door, 0f);
            }
        }

        private static void DrawHorizontalBigDoorArchFrame(Building_MultiTileDoor door, Vector3 drawLoc)
        {
            Vector3 drawPos = door.DrawPos;
            drawPos.z += 0.1f;
            drawPos.y = AltitudeLayer.BuildingOnTop.AltitudeFor() + 1f;

            ThingDef def = door.def;
            Vector3 scale = new Vector3(def.size.x * 1.8f, 1f, def.size.z * 1.8f);

            Graphic supportGraphic = (Graphic)suppportGraphicField.GetValue(door);
            if (supportGraphic != null)
            {
                Vector3 supportPos = drawPos;
                Quaternion supportRot = Quaternion.Euler(-90f, 0f, 0f);
                Matrix4x4 matrix = Matrix4x4.TRS(supportPos, supportRot, scale);
                Graphics.DrawMesh(MeshPool.plane10, matrix, supportGraphic.MatAt(door.Rotation, door), 0);
            }

            Graphic topGraphic = (Graphic)topGraphicField.GetValue(door);
            if (topGraphic != null)
            {
                Vector3 topPos = drawPos;
                topPos.y = AltitudeLayer.BuildingOnTop.AltitudeFor() + 1f;
                Quaternion topRot = Quaternion.Euler(-90f, 0f, 0f);
                Matrix4x4 matrix = Matrix4x4.TRS(topPos, topRot, scale);
                Graphics.DrawMesh(MeshPool.plane10, matrix, topGraphic.MatAt(door.Rotation, door), 0);
            }
        }
        private static void DrawBasicDoor(Building_Door door, Vector3 drawPos, Graphic graphic, float altitude, Vector3 drawScaleFactor, Graphic_Shadow shadowGraphic, float offsetDist)
        {
            ThingDef def = door.def;
            Rot4 rotation = door.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);

            for (int i = 0; i < 2; i++)
            {
                Mesh mesh;
                Vector3 vector;
                if (i == 0)
                {
                    vector = new Vector3(0f, 0f, -def.size.x);
                    mesh = MeshPool.plane10;
                }
                else
                {
                    vector = new Vector3(0f, 0f, def.size.x);
                    mesh = MeshPool.plane10Flip;
                }
                vector = rotation.AsQuat * vector;
                Vector3 vector2 = drawPos;
                vector2.y = altitude;
                vector2 += vector * offsetDist;
                Graphics.DrawMesh(
                    mesh,
                    Matrix4x4.TRS(vector2, door.Rotation.AsQuat, new Vector3((float)def.size.x * drawScaleFactor.x, drawScaleFactor.y, (float)def.size.z * drawScaleFactor.z)),
                    graphic.MatAt(door.Rotation, door),
                    0
                );
                shadowGraphic?.DrawWorker(vector2, door.Rotation, def, door, 0f);
            }
        }

        private static void DrawHorizontalMultiDoor(Building_MultiTileDoor door, Vector3 drawLoc, float openPct, float upperMoverAltitude, Vector3 moverDrawScale)
        {
            Vector3 ceilingPos = drawLoc;
            ceilingPos.y = 0.80f;
            DrawBasicDoor(door, ceilingPos, door.Graphic, AltitudeLayer.DoorMoveable.AltitudeFor(), moverDrawScale, door.Graphic.ShadowGraphic, 0f);

            Vector3 drawPos = door.DrawPos;
            drawPos.z += 0.1f;
            drawPos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();

            Graphic supportGraphic = (Graphic)suppportGraphicField.GetValue(door);
            if (supportGraphic != null)
            {
                supportGraphic.Draw(drawPos, door.Rotation, door);
            }

            drawPos.y = AltitudeLayer.Blueprint.AltitudeFor();
            Graphic topGraphic = (Graphic)topGraphicField.GetValue(door);
            if (topGraphic != null)
            {
                topGraphic.Draw(drawPos, door.Rotation, door);
            }
        }

        public static void DrawAt(Building_Door door, Vector3 drawLoc, bool flip = false)
        {
            DoorPreDraw(door);
            int ticksSinceOpen = (int)ticksSinceOpenField.GetValue(door);
            int ticksToOpenNow = (int)ticksToOpenNowMethod.Invoke(door, null);
            float openPct = Mathf.Clamp01((float)ticksSinceOpen / (float)ticksToOpenNow);
            float offsetDist = 0.45f * openPct;

            bool isHorizontal = door.Rotation == Rot4.North || door.Rotation == Rot4.South;
            if (isHorizontal)
            {
                Vector3 baseOffset = Vector3.zero;
                if (door.Rotation == Rot4.North)
                {
                    baseOffset = Vector3.right * offsetDist;
                }
                else
                {
                    baseOffset = Vector3.left * offsetDist;
                }

                {
                    Vector3 ceilingPos = drawLoc;
                    ceilingPos.y = 0.80f;
                    ceilingPos.z -= 0.05f;
                    DrawDoorPanels(door, door.Graphic, ceilingPos, door.Rotation, baseOffset);
                }

                {
                    Vector3 wallPos = drawLoc;
                    wallPos.y = 0.5f;
                    wallPos.z -= 0.4f;
                    DrawDoorPanels(door, door.Graphic, wallPos, Rot4.South, -baseOffset);
                }
            }
            else
            {
                Vector3 baseOffset = Vector3.zero;
                if (door.Rotation == Rot4.East)
                {
                    baseOffset = Vector3.forward * offsetDist;
                }
                else
                {
                    baseOffset = Vector3.back * offsetDist;
                }

                {
                    Vector3 ceilingPos = drawLoc;
                    ceilingPos.y = 0.80f;
                    DrawCeilingPanel(door, door.Graphic, ceilingPos, baseOffset);
                }

                {
                    Vector3 westWallPos = drawLoc;
                    westWallPos.y = 0.5f;
                    westWallPos.x -= 0.35f;
                    DrawWallPanel(door, door.Graphic, westWallPos, true, -baseOffset);
                }

                {
                    Vector3 eastWallPos = drawLoc;
                    eastWallPos.y = 0.5f;
                    eastWallPos.x += 0.35f;
                    DrawWallPanel(door, door.Graphic, eastWallPos, false, baseOffset);
                }
            }

            SilhouetteUtility.DrawGraphicSilhouette(door, drawLoc);
        }

        private static void DrawCeilingPanel(Building_Door door, Graphic graphic, Vector3 pos, Vector3 moveOffset)
        {
            for (int i = 0; i < 2; i++)
            {
                Mesh mesh = i == 0 ? MeshPool.plane10Flip : MeshPool.plane10;
                Vector3 panelOffset = moveOffset * (i == 0 ? -1f : 1f);
                Vector3 finalPos = pos + panelOffset;

                Graphics.DrawMesh(mesh,
                    Matrix4x4.TRS(finalPos, door.Rotation.AsQuat, Vector3.one),
                    graphic.MatAt(door.Rotation, door),
                    0);
            }
        }

        private static void DrawWallPanel(Building_Door door, Graphic graphic, Vector3 pos, bool isWest, Vector3 moveOffset)
        {
            for (int i = 0; i < 2; i++)
            {
                Mesh mesh = i == 0 ? MeshPool.plane10Flip : MeshPool.plane10;
                Vector3 panelOffset = moveOffset * (i == 0 ? -1f : 1f);
                Vector3 finalPos = pos + panelOffset;

                float rotationAngle = isWest ? -90f : 90f;
                Quaternion finalRot = Quaternion.Euler(90f, rotationAngle, 0f);

                Graphics.DrawMesh(mesh,
                    Matrix4x4.TRS(finalPos, finalRot, Vector3.one),
                    graphic.MatAt(isWest ? Rot4.West : Rot4.East, door),
                    0);
            }
        }

        private static void DrawDoorPanels(Building_Door door, Graphic graphic, Vector3 pos, Rot4 rotation, Vector3 moveOffset)
        {
            ThingDef def = door.def;

            for (int i = 0; i < 2; i++)
            {
                Mesh mesh = i == 0 ? MeshPool.plane10 : MeshPool.plane10Flip;
                Vector3 panelOffset = moveOffset * (i == 0 ? -1f : 1f);
                Vector3 finalPos = pos + panelOffset;

                Quaternion finalRot = rotation == Rot4.South ?
                    Quaternion.Euler(90f, 180f, 0f) :
                    rotation.AsQuat;

                Graphics.DrawMesh(mesh,
                    Matrix4x4.TRS(finalPos, finalRot, Vector3.one),
                    graphic.MatAt(rotation, door),
                    0);
            }
        }

        private static void DoorPreDraw(Building_Door door)
        {
            if (door.def.size == IntVec2.One)
            {
                door.Rotation = DoorUtility.DoorRotationAt(door.Position, door.Map, door.def.building.preferConnectingToFences);
            }
        }
    }
}