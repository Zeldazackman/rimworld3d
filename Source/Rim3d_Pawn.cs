using HarmonyLib;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using RimWorld;

namespace Rim3D
{
    [HarmonyPatch(typeof(TextureAtlasHelper))]
    [HarmonyPatch("CreateMeshForUV")]
    public class Patch_TextureAtlasHelper_CreateMeshForUV
    {
        [HarmonyPrefix]
        public static bool Prefix(Rect uv, float scale, ref Mesh __result)
        {
            if (!Core.mode3d)
            {
                return true;
            }
            Vector3[] vertices = new Vector3[4];
            Vector2[] uvs = new Vector2[4];
            int[] triangles = new int[6];

            float height = 0.8f;
            float depth = 0.5f;
            float baseHeight = -0.3f;

            vertices[0] = new Vector3(-1f * scale, baseHeight, -1f * scale);
            vertices[1] = new Vector3(-1f * scale, baseHeight + height, depth * scale);
            vertices[2] = new Vector3(1f * scale, baseHeight + height, depth * scale);
            vertices[3] = new Vector3(1f * scale, baseHeight, -1f * scale);

            uvs[0] = uv.min;
            uvs[1] = new Vector2(uv.xMin, uv.yMax);
            uvs[2] = uv.max;
            uvs[3] = new Vector2(uv.xMax, uv.yMin);

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;

            Mesh mesh = new Mesh();
            mesh.name = "PawnMesh3D";
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            __result = mesh;
            return false;
        }
    }

    //Weapons equiped
    [HarmonyPatch(typeof(PawnRenderUtility))]
    [HarmonyPatch("DrawEquipmentAiming")]
    public class Patch_PawnRenderUtility_DrawEquipmentAiming
    {
        [HarmonyPrefix]
        public static bool Prefix(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            if (!Core.mode3d)
            {
                return true;
            }

            float num = aimAngle - 90f;
            Mesh mesh;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh = MeshPool.plane10Flip;
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
            }
            else
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }

            num %= 360f;
            CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
            if (compEquippable != null)
            {
                EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
                drawLoc += drawOffset;
                num += angleOffset;
            }

            drawLoc.y = 0.3f;
            Material material = ((!(eq.Graphic is Graphic_StackCount graphic_StackCount)) ? eq.Graphic.MatSingleFor(eq) : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq));
            Matrix4x4 matrix = Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(num, Vector3.up), new Vector3(eq.Graphic.drawSize.x, 1f, eq.Graphic.drawSize.y));
            Graphics.DrawMesh(mesh, matrix, material, 0);

            return false;
        }
    }
}
