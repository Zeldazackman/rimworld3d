using UnityEngine;
using Verse;
using RimWorld;

namespace Rim3D
{
    public class WindowRim3D : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private float lastContentHeight = 580f;
        private bool hasLoadedSettings;

        public WindowRim3D()
        {
            this.forcePause = false;
            this.draggable = true;
            this.resizeable = true;
            this.doCloseX = true;
            this.doCloseButton = false;
            this.closeOnClickedOutside = false;
            this.preventCameraMotion = false;
            this.hasLoadedSettings = Core.settings.AreSettingsSaved();
        }

        public override Vector2 InitialSize => new Vector2(450f, 650f);

        public override void DoWindowContents(Rect inRect)
        {
            ScrollingContainer container = new ScrollingContainer(inRect);
            container.Begin(ref scrollPosition, lastContentHeight);

            Text.Font = GameFont.Medium;
            container.AddFixedBox(40f, r => Widgets.Label(r, "Majaus's Rimworld 3D"));
            Text.Font = GameFont.Small;

            container.AddGap();

            container.AddFixedBox(30f, r =>
            {
                if (Widgets.ButtonText(r, "Object Configuration"))
                {
                    Find.WindowStack.Add(new ObjectWindow());
                }
            });

            container.AddGap();

            container.AddFixedBox(30f, r =>
            {
                if (Widgets.ButtonText(r, "Map Mountains"))
                {
                    Find.WindowStack.Add(new MapMountainsWindow());
                }
            });

            container.AddGap();

            container.AddFixedBox(30f, r =>
            {
                if (Widgets.ButtonText(r, "Border Mountains"))
                {
                    Find.WindowStack.Add(new MountainsWindow());
                }
            });

            container.AddGap();

            container.AddFixedBox(30f, r =>
            {
                if (Widgets.ButtonText(r, "Fire Options"))
                {
                    Find.WindowStack.Add(new FireOptionsWindow());
                }
            });

            container.AddGap();

            container.AddFixedBox(30f, r =>
            {
                if (Widgets.ButtonText(r, "Post Processing"))
                {
                    Find.WindowStack.Add(new PostProcessWindow());
                }
            });

            container.AddGap();

            container.AddFixedBox(30f, r =>
            {
                if (Widgets.ButtonText(r, "Weather Layer"))
                {
                    Find.WindowStack.Add(new WeatherLayerWindow());
                }
            });

            container.AddGap();

            container.AddFixedBox(24f, r =>
            {
                bool wasEnabled = Core.settings.enableTransition;
                Widgets.CheckboxLabeled(r, "Enable Transition Effect", ref Core.settings.enableTransition);
            });

            if (Core.settings.enableTransition)
            {
                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Transition Duration: " + Core.settings.transitionDuration.ToString("0.0") + "s");
                    Core.settings.transitionDuration = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.transitionDuration, 0.1f, 4f);
                    Text.Anchor = TextAnchor.UpperLeft;
                });
                container.AddGap();
            }

            container.AddFixedBox(30f, r =>
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(r.LeftHalf(), "Camera Speed: " + Core.settings.camSpeed.ToString("0.0"));
                Core.settings.camSpeed = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.camSpeed, 0.1f, 10f);
                Text.Anchor = TextAnchor.UpperLeft;
            });

            container.AddFixedBox(30f, r =>
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(r.LeftHalf(), "Camera Angle: " + Core.settings.angle.ToString("0.0"));
                Core.settings.angle = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.angle, 0f, 90f);
                Text.Anchor = TextAnchor.UpperLeft;
            });

            container.AddFixedBox(30f, r =>
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(r.LeftHalf(), "Field of View: " + Core.settings.fov.ToString("0.0"));
                Core.settings.fov = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.fov, 10f, 70f);
                Text.Anchor = TextAnchor.UpperLeft;
            });

            container.AddFixedBox(30f, r =>
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(r.LeftHalf(), "Camera Height: " + Core.settings.z.ToString("0.0"));
                Core.settings.z = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.z, 1f, 120f);
                Text.Anchor = TextAnchor.UpperLeft;
            });

            container.AddFixedBox(30f, r =>
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(r.LeftHalf(), "View Distance: " + Core.settings.far.ToString("0.0"));
                Core.settings.far = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.far, 10f, 1000f);
                Text.Anchor = TextAnchor.UpperLeft;
            });

            container.AddFixedBox(24f, r =>
            {
                bool wasDrawFullMap = Core.settings.drawFullMap;
                Widgets.CheckboxLabeled(r, "Draw Full Map", ref Core.settings.drawFullMap);
            });

            container.AddFixedBox(30f, r =>
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Rect leftRect = r.LeftHalf();
                Rect rightRect = r.RightHalf();
                float btnWidth = 60f;
                Rect sliderRect = new Rect(rightRect.x, rightRect.y, rightRect.width - btnWidth - 5f, rightRect.height);
                Rect buttonRect = new Rect(rightRect.xMax - btnWidth, rightRect.y, btnWidth, rightRect.height);

                Widgets.Label(leftRect, "Lighting Overlay Height: " + Core.settings.lightingOverlayHeight.ToString("0.0"));
                float newHeight = Widgets.HorizontalSlider(sliderRect.ContractedBy(0, 2), Core.settings.lightingOverlayHeight, -2.5f, 7.5f);
                if (Core.settings.lightingOverlayHeight != newHeight)
                {
                    Core.settings.lightingOverlayHeight = newHeight;
                }

                if (Widgets.ButtonText(buttonRect, "Apply"))
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                        "Lighting overlay height will be applied after game restart. Remember to save settings before restarting.",
                        null));
                }
                Text.Anchor = TextAnchor.UpperLeft;
            });

            container.AddFixedBox(24f, r =>
            {
                bool wasOrthographic = Core.settings.orthographic;
                Widgets.CheckboxLabeled(r, "Orthographic Camera", ref Core.settings.orthographic);
                if (wasOrthographic != Core.settings.orthographic && Core.mode3d)
                {
                    Core.Instance.stateManager.UpdateProjection();
                }
            });

            if (Core.settings.orthographic)
            {
                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Orthographic Size: " + Core.settings.orthographicSize.ToString("0.000"));
                    Core.settings.orthographicSize = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.orthographicSize, 5f, 50f);
                    Text.Anchor = TextAnchor.UpperLeft;
                });
            }

            container.AddGap();

            container.AddFixedBox(24f, r =>
            {
                bool wasPlantShaderFix = Core.settings.plantShaderFix;
                Widgets.CheckboxLabeled(r, "Enable Plant Shader Fix", ref Core.settings.plantShaderFix);
                if (wasPlantShaderFix != Core.settings.plantShaderFix)
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                        "Plant shader fix will be applied after game restart. Remember to save settings before restarting.",
                        null));
                }
            });

            container.AddGap();

            container.AddFixedBox(24f, r =>
            {
                bool wasSkyboxEnabled = Core.settings.enableSkybox;
                Widgets.CheckboxLabeled(r, "Enable Skybox", ref Core.settings.enableSkybox);
                if (wasSkyboxEnabled != Core.settings.enableSkybox)
                {
                    Core.Instance.stateManager.UpdateSkybox();
                }
            });

            if (Core.settings.enableSkybox)
            {
                container.AddFixedBox(30f, r =>
                {
                    if (Widgets.ButtonText(r, "Skybox Settings"))
                    {
                        Find.WindowStack.Add(new SkyboxWindow());
                    }
                });
            }

            container.AddGap();

            container.AddFixedBox(30f, r =>
            {
                if (Widgets.ButtonText(r, "Default Settings"))
                {
                    Core.settings.ResetToDefaults();
                    hasLoadedSettings = false;
                }
            });

            container.AddFixedBox(30f, r =>
            {
                if (Widgets.ButtonText(r, "Save Settings"))
                {
                    Core.settings.SaveCurrentSettings();
                    hasLoadedSettings = true;
                }
            });

            if (hasLoadedSettings)
            {
                container.AddFixedBox(30f, r =>
                {
                    if (Widgets.ButtonText(r, "Load Settings"))
                    {
                        Core.settings.LoadSavedSettings();
                    }
                });
            }

            container.AddGap();

            container.AddFixedBox(30f, r =>
            {
                if (Widgets.ButtonText(r, "Delete Saved Data"))
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                        "Are you sure you want to delete all saved configurations?",
                        () =>
                        {
                            ConfigSerializer.DeleteAllData();
                            Core.settings.ResetToDefaults();
                            hasLoadedSettings = false;
                        }
                    ));
                }
            });

            lastContentHeight = container.ContentHeight;
            container.End();
        }

        public override void PostOpen()
        {
            base.PostOpen();
            this.windowRect.x = UI.screenWidth - this.windowRect.width - 50f;
            this.windowRect.y = 50f;
        }
    }
}