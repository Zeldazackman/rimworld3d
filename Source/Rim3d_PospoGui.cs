using UnityEngine;
using Verse;
using RimWorld;

namespace Rim3D
{
    public class PostProcessWindow : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private float lastContentHeight = 580f;

        public PostProcessWindow()
        {
            this.forcePause = false;
            this.draggable = true;
            this.resizeable = true;
            this.doCloseX = true;
            this.doCloseButton = false;
            this.closeOnClickedOutside = false;
            this.preventCameraMotion = false;
        }

        public override Vector2 InitialSize => new Vector2(450f, 400f);

        public override void DoWindowContents(Rect inRect)
        {
            ScrollingContainer container = new ScrollingContainer(inRect);
            container.Begin(ref scrollPosition, lastContentHeight);

            Text.Font = GameFont.Medium;
            container.AddFixedBox(40f, r => Widgets.Label(r, "Post Processing"));
            Text.Font = GameFont.Small;

            container.AddGap();

            container.AddFixedBox(24f, r =>
            {
                bool wasEnabled = Core.settings.enablePostProcess;
                Widgets.CheckboxLabeled(r, "Enable Post Processing", ref Core.settings.enablePostProcess);
                if (wasEnabled != Core.settings.enablePostProcess)
                {
                    PostProcessManager.UpdatePostProcess();
                }
            });

            if (Core.settings.enablePostProcess)
            {
                container.AddGap();

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Exposure: " + Core.settings.pospoExposure.ToString("0.00"));
                    float newExposure = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.pospoExposure, 0f, 10f);
                    if (newExposure != Core.settings.pospoExposure)
                    {
                        Core.settings.pospoExposure = newExposure;
                        PostProcessManager.UpdatePostProcess();
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Contrast: " + Core.settings.pospoContrast.ToString("0.00"));
                    float newContrast = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.pospoContrast, 0f, 3f);
                    if (newContrast != Core.settings.pospoContrast)
                    {
                        Core.settings.pospoContrast = newContrast;
                        PostProcessManager.UpdatePostProcess();
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Saturation: " + Core.settings.pospoSaturation.ToString("0.00"));
                    float newSaturation = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.pospoSaturation, 0f, 3f);
                    if (newSaturation != Core.settings.pospoSaturation)
                    {
                        Core.settings.pospoSaturation = newSaturation;
                        PostProcessManager.UpdatePostProcess();
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Bloom: " + Core.settings.pospoBloom.ToString("0.00"));
                    float newBloom = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.pospoBloom, 0f, 2f);
                    if (newBloom != Core.settings.pospoBloom)
                    {
                        Core.settings.pospoBloom = newBloom;
                        PostProcessManager.UpdatePostProcess();
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Bloom Threshold: " + Core.settings.pospoBloomThreshold.ToString("0.00"));
                    float newBloomThreshold = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.pospoBloomThreshold, 0f, 2f);
                    if (newBloomThreshold != Core.settings.pospoBloomThreshold)
                    {
                        Core.settings.pospoBloomThreshold = newBloomThreshold;
                        PostProcessManager.UpdatePostProcess();
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddGap();

                container.AddFixedBox(30f, r =>
                {
                    if (Widgets.ButtonText(r, "Reset to Defaults"))
                    {
                        Core.settings.pospoExposure = 1f;
                        Core.settings.pospoContrast = 1f;
                        Core.settings.pospoSaturation = 1f;
                        Core.settings.pospoBloom = 0.5f;
                        Core.settings.pospoBloomThreshold = 0.5f;
                        PostProcessManager.UpdatePostProcess();
                    }
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