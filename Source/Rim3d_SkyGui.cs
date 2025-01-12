using UnityEngine;
using Verse;
using RimWorld;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Rim3D
{
    public class SkyboxWindow : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private float lastContentHeight = 580f;
        private List<string> skyboxOptions = new List<string>();
        private int selectedSkyboxIndex = 0;

        public SkyboxWindow()
        {
            this.forcePause = false;
            this.draggable = true;
            this.resizeable = true;
            this.doCloseX = true;
            this.doCloseButton = false;
            this.closeOnClickedOutside = false;
            this.preventCameraMotion = false;

            string skyboxPath = Path.Combine(GenFilePaths.ModsFolderPath, "rim3d-majaus", "Resources", "Skys");
            if (Directory.Exists(skyboxPath))
            {
                var files = Directory.GetFiles(skyboxPath)
                    .Where(f => f.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                    .Select(Path.GetFileName)
                    .ToList();
                skyboxOptions.AddRange(files);

                if (!string.IsNullOrEmpty(Core.settings.selectedSkybox))
                {
                    selectedSkyboxIndex = skyboxOptions.IndexOf(Core.settings.selectedSkybox);
                    if (selectedSkyboxIndex == -1)
                    {
                        selectedSkyboxIndex = 0;
                    }
                }
            }
            else
            {
                Log.Error("[Rim3D] Skybox directory not found!");
            }
        }

        public override Vector2 InitialSize => new Vector2(450f, 400f);

        public override void DoWindowContents(Rect inRect)
        {
            ScrollingContainer container = new ScrollingContainer(inRect);
            container.Begin(ref scrollPosition, lastContentHeight);

            Text.Font = GameFont.Medium;
            container.AddFixedBox(40f, r => Widgets.Label(r, "Skybox Settings"));
            Text.Font = GameFont.Small;

            container.AddGap();

            if (skyboxOptions.Count > 0)
            {
                container.AddFixedBox(30f, r =>
                {
                    Rect labelRect = new Rect(r.x, r.y, r.width * 0.3f, r.height);
                    Rect comboRect = new Rect(r.x + r.width * 0.3f, r.y, r.width * 0.7f, r.height);

                    Widgets.Label(labelRect, "Skybox Texture:");
                    if (Widgets.ButtonText(comboRect, skyboxOptions[selectedSkyboxIndex]))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        for (int i = 0; i < skyboxOptions.Count; i++)
                        {
                            int index = i;
                            options.Add(new FloatMenuOption(skyboxOptions[i], () =>
                            {
                                selectedSkyboxIndex = index;
                                SkyManager.SetCustomSkyboxTexture(skyboxOptions[index]);
                                if (Core.mode3d && Core.settings.enableSkybox)
                                {
                                    Core.Instance.stateManager.UpdateSkybox();
                                }
                                else
                                {
                                    Log.Message($"[Rim3D] Not updating skybox - mode3d: {Core.mode3d}, enableSkybox: {Core.settings.enableSkybox}");
                                }
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                });

                container.AddGap();

                container.AddFixedBox(30f, r =>
                {
                    if (Widgets.ButtonText(r, "Open Skybox Folder"))
                    {
                        string skyboxPath = Path.Combine(GenFilePaths.ModsFolderPath, "rim3d-majaus", "Resources", "Skys");
                        if (Directory.Exists(skyboxPath))
                        {
                            Application.OpenURL("file://" + skyboxPath);
                        }
                        else
                        {
                            Log.Error("[Rim3D] Skybox directory not found!");
                        }
                    }
                });

                container.AddGap();
            }

            container.AddFixedBox(30f, r =>
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(r.LeftHalf(), "Vertical Position: " + Core.settings.skyboxDegrees.ToString("0.0"));
                float newDegrees = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.skyboxDegrees, -400f, 400f);
                if (newDegrees != Core.settings.skyboxDegrees)
                {
                    Core.settings.skyboxDegrees = newDegrees;
                    Core.Instance.stateManager.UpdateSkybox();
                }
                Text.Anchor = TextAnchor.UpperLeft;
            });

            container.AddFixedBox(30f, r =>
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(r.LeftHalf(), "Horizontal Angle: " + Core.settings.skyboxDegreesHorizontal.ToString("0.0"));
                float newDegreesHorizontal = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.skyboxDegreesHorizontal, 0f, 360f);
                if (newDegreesHorizontal != Core.settings.skyboxDegreesHorizontal)
                {
                    Core.settings.skyboxDegreesHorizontal = newDegreesHorizontal;
                    Core.Instance.stateManager.UpdateSkybox();
                }
                Text.Anchor = TextAnchor.UpperLeft;
            });

            container.AddFixedBox(30f, r =>
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(r.LeftHalf(), "Vertical Offset: " + Core.settings.skyboxVerticalOffset.ToString("0.0"));
                float newVerticalOffset = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.skyboxVerticalOffset, -1f, 1f);
                if (newVerticalOffset != Core.settings.skyboxVerticalOffset)
                {
                    Core.settings.skyboxVerticalOffset = newVerticalOffset;
                    Core.Instance.stateManager.UpdateSkybox();
                }
                Text.Anchor = TextAnchor.UpperLeft;
            });

            container.AddGap();

            container.AddFixedBox(24f, r =>
            {
                bool wasGameControl = Core.settings.gameControlSky;
                bool gameControlSky = Core.settings.gameControlSky;
                Widgets.CheckboxLabeled(r, "Game Control Sky", ref gameControlSky);
                if (gameControlSky != wasGameControl)
                {
                    Core.settings.gameControlSky = gameControlSky;
                }
            });

            if (Core.settings.gameControlSky)
            {
                container.AddFixedBox(24f, r =>
                {
                    bool wasAutoExposure = Core.settings.enableAutoExposure;
                    bool autoExposure = Core.settings.enableAutoExposure;
                    Widgets.CheckboxLabeled(r, "Enable Auto Exposure", ref autoExposure);
                    if (autoExposure != wasAutoExposure)
                    {
                        Core.settings.enableAutoExposure = autoExposure;
                        if (!autoExposure)
                        {
                            Core.settings.skyboxExposure = 1f;
                        }
                        Core.Instance.stateManager.UpdateSkybox();
                    }
                });
            }

            if (!Core.settings.gameControlSky)
            {
                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Rotation: " + Core.settings.skyboxRotation.ToString("0.0"));
                    float newRotation = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.skyboxRotation, 0f, 360f);
                    if (newRotation != Core.settings.skyboxRotation)
                    {
                        Core.settings.skyboxRotation = newRotation;
                        Core.Instance.stateManager.UpdateSkybox();
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Exposure: " + Core.settings.skyboxExposure.ToString("0.0"));
                    float newExposure = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.skyboxExposure, 0f, 2f);
                    if (newExposure != Core.settings.skyboxExposure)
                    {
                        Core.settings.skyboxExposure = newExposure;
                        Core.Instance.stateManager.UpdateSkybox();
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                });
            }

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