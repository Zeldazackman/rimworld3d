using UnityEngine;
using Verse;
using HarmonyLib;
using System.Reflection;

namespace Rim3D
{
    [HarmonyPatch(typeof(MapDrawer))]
    [HarmonyPatch("DrawMapMesh")]
    public class Patch_MapDraw
    {
        [HarmonyPrefix]
        public static bool Prefix(MapDrawer __instance)
        {
            if (!Core.mode3d || !Core.settings.drawFullMap) return true;
            var sections = AccessTools.Field(typeof(MapDrawer), "sections").GetValue(__instance) as Section[,];
            if (sections == null) return true;
            for (int i = 0; i < sections.GetLength(0); i++)
            {
                for (int j = 0; j < sections.GetLength(1); j++)
                {
                    if (sections[i, j] != null)
                    {
                        sections[i, j].DrawSection();
                    }
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(UI))]
    [HarmonyPatch("UIToMapPosition")]
    [HarmonyPatch(new[] { typeof(Vector2) })]
    public class Patch_UI_UIToMapPosition
    {
        [HarmonyPrefix]
        public static bool Prefix(Vector2 screenLoc, ref Vector3 __result)
        {
            if (!Core.mode3d) return true;

            Ray ray = Find.Camera.ScreenPointToRay(screenLoc * Prefs.UIScale);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float enter = 0.0f;

            if (groundPlane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                __result = new Vector3(hitPoint.x, 0f, hitPoint.z);
                return false;
            }

            __result = new Vector3(ray.origin.x, 0f, ray.origin.z);
            return false;
        }
    }
    [HarmonyPatch]
    public class Patch_CameraDriver_ApplyPositionToGameObject
    {
        static MethodBase TargetMethod()
        {
            var type = typeof(CameraDriver);
            return AccessTools.Method(type, "ApplyPositionToGameObject");
        }

        static bool Prefix(CameraDriver __instance, ref float ___rootSize, ref Vector3 ___rootPos, Camera ___cachedCamera)
        {
            if (!Core.mode3d)
            {
                return true;
            }

            float originalRootSize = ___rootSize;
            //DebugLog.Log($"ApplyPositionToGameObject - Original RootSize: {originalRootSize}");
            //DebugLog.Log($"ApplyPositionToGameObject - Core.settings.z: {Core.settings.z}");

            ___rootPos.y = 15f + (Core.settings.z - __instance.config.sizeRange.min) / (__instance.config.sizeRange.max - __instance.config.sizeRange.min) * 50f;
            ___cachedCamera.orthographicSize = Core.settings.z;
            ___cachedCamera.transform.position = ___rootPos;

            GameObject reverbDummy = AccessTools.Field(typeof(CameraDriver), "reverbDummy").GetValue(__instance) as GameObject;
            if (reverbDummy != null)
            {
                Vector3 position = __instance.transform.position;
                position.y = 65f;
                reverbDummy.transform.position = position;
            }

            return false;
        }
    }
    public class CameraController
    {
        private bool setuped = false;
        private float d_speed;
        private float d_far;
        private Vector3 d_angle;
        private float d_fov;
        private Vector3 d_pos;
        private float d_orth;
        private int t = 0;

        public float ZGap => 35f - 0.8f * Core.settings.angle;
        public Vector3 Pos => AccessTools.Field(typeof(CameraDriver), "rootPos").GetValue(Current.CameraDriver) as Vector3? ?? Vector3.zero;
        public float Size => AccessTools.Field(typeof(CameraDriver), "rootSize").GetValue(Current.CameraDriver) as float? ?? 0f;
        public const float ZoomSize = 60f;

        public void Tick()
        {
            if (t == 0)
            {
                t++;
            }
            else if (t == 1)
            {
                d_speed = Current.CameraDriver.config.moveSpeedScale;
                d_far = Current.Camera.farClipPlane;
                d_angle = Current.Camera.transform.localEulerAngles;
                d_fov = Current.Camera.fieldOfView;
                d_pos = Current.Camera.transform.localPosition;
                setuped = true;
                t++;
            }
        }

        public void UpdateCamera()
        {
            if (!setuped) return;
            if (Current.ProgramState != ProgramState.Playing) return;

            if (Core.mode3d)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll != 0)
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        Core.settings.angle = Mathf.Clamp(Core.settings.angle - scroll * 1f, 0f, 90f);
                    }
                    else
                    {
                        Core.settings.z = Mathf.Clamp(Core.settings.z - scroll * 2f, 1f, 120f);
                    }
                }

                Current.Camera.orthographic = Core.settings.orthographic;
                Current.Camera.farClipPlane = Core.settings.far;
                Current.Camera.transform.localEulerAngles = new Vector3(Core.settings.angle, 0f, 0f);

                if (Core.settings.orthographic)
                {
                    Current.Camera.orthographicSize = Core.settings.orthographicSize;
                }
                else
                {
                    Current.Camera.fieldOfView = Core.settings.fov;
                }

                Current.CameraDriver.SetRootPosAndSize(Pos, Core.settings.orthographic ? Core.settings.orthographicSize * 2 : ZoomSize);
                Current.Camera.transform.localPosition = new Vector3(Current.Camera.transform.localPosition.x, Core.settings.z, Current.Camera.transform.localPosition.z);

                float cosAngle = Mathf.Cos(Core.settings.angle * Mathf.Deg2Rad);
                float speedMultiplier = Mathf.Max(0.2f, cosAngle);
                Current.CameraDriver.config.moveSpeedScale = (2f / (Core.settings.orthographic ? Core.settings.orthographicSize : Core.settings.fov) * Core.settings.camSpeed) * speedMultiplier;
            }
            else
            {
                d_pos = Current.Camera.transform.localPosition;
                d_orth = Size;
            }
        }

        public float DefaultSpeed => d_speed;
        public float DefaultFar => d_far;
        public Vector3 DefaultAngle => d_angle;
        public float DefaultFov => d_fov;
        public float DefaultOrth => d_orth;
    }
}