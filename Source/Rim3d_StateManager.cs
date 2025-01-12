using UnityEngine;
using Verse;
using RimWorld;

namespace Rim3D
{
    public class StateManager
    {
        public void on()
        {
            try
            {
                if (Current.Camera == null)
                {
                    Log.Message("[Rim3D] ERROR: Camera is null");
                    return;
                }

                SkyManager.ActivateSkybox();

                Current.Camera.orthographic = Core.settings.orthographic;
                Current.CameraDriver.config.moveSpeedScale = 2f / Core.settings.fov * Core.settings.camSpeed;
                Current.Camera.farClipPlane = Core.settings.far;
                Current.Camera.transform.localEulerAngles = new Vector3(Core.settings.angle, 0f, 0f);
                Current.Camera.fieldOfView = Core.settings.fov;
                Current.Camera.nearClipPlane = 0.1f;
                Vector3 newPos = new Vector3(Core.Instance.cameraController.Pos.x, Core.settings.z,
                    Core.Instance.cameraController.Pos.z - Core.Instance.cameraController.ZGap);
                Current.CameraDriver.SetRootPosAndSize(newPos, CameraController.ZoomSize);

                if (Find.CurrentMap != null)
                {
                    foreach (var cell in Find.CurrentMap.AllCells)
                    {
                        Find.CurrentMap.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Things);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[Rim3D] Error in state change to 3D: {ex}");
            }
        }

        public void off()
        {
            try
            {
                if (Current.Camera == null)
                {
                    Log.Error("[Rim3D] Camera is null");
                    return;
                }

                SkyManager.DeactivateSkybox();

                Current.Camera.orthographic = true;
                Current.CameraDriver.config.moveSpeedScale = Core.Instance.cameraController.DefaultSpeed;
                Current.Camera.farClipPlane = Core.Instance.cameraController.DefaultFar;
                Current.Camera.transform.localEulerAngles = Core.Instance.cameraController.DefaultAngle;
                Current.Camera.fieldOfView = Core.Instance.cameraController.DefaultFov;

                Vector3 newPos = Core.Instance.cameraController.Pos + new Vector3(0f, 0f, Core.Instance.cameraController.ZGap);
                Current.CameraDriver.SetRootPosAndSize(newPos, Core.Instance.cameraController.DefaultOrth);

                if (Find.CurrentMap != null)
                {
                    foreach (var cell in Find.CurrentMap.AllCells)
                    {
                        Find.CurrentMap.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Things);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[Rim3D] Error in state change to 2D: {ex}");
            }
        }

        public void UpdateProjection()
        {
            try
            {
                if (Current.Camera == null)
                {
                    Log.Error("[Rim3D] Camera is null");
                    return;
                }
                Current.Camera.orthographic = Core.settings.orthographic;
            }
            catch (System.Exception ex)
            {
                Log.Error($"[Rim3D] Error updating projection: {ex}");
            }
        }

        public void UpdateSkybox()
        {
            SkyManager.UpdateSkybox();
        }
    }
}