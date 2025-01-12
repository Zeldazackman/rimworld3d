using UnityEngine;
using Verse;
using RimWorld;

namespace Rim3D
{
    public class MapMountainsWindow : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private float lastContentHeight = 580f;
        private float newFogHeight;
        private float newFogMaxHeight;

        public MapMountainsWindow()
        {
            this.forcePause = false;
            this.draggable = true;
            this.resizeable = true;
            this.doCloseX = true;
            this.doCloseButton = false;
            this.closeOnClickedOutside = false;
            this.preventCameraMotion = false;
            ReloadSettings();
        }

        public void ReloadSettings()
        {
            this.newFogHeight = Core.settings.fogHeight;
            this.newFogMaxHeight = Core.settings.fogMaxHeight;
        }

        public override Vector2 InitialSize => new Vector2(450f, 250f);

        public override void DoWindowContents(Rect inRect)
        {
            ScrollingContainer container = new ScrollingContainer(inRect);
            container.Begin(ref scrollPosition, lastContentHeight);

            Text.Font = GameFont.Medium;
            container.AddFixedBox(40f, r => Widgets.Label(r, "Map Mountains"));
            Text.Font = GameFont.Small;

            container.AddGap();

            container.AddFixedBox(24f, r =>
            {
                bool wasEnabled = Core.settings.enableMapMountains;
                Widgets.CheckboxLabeled(r, "Enable Map Mountains", ref Core.settings.enableMapMountains);
                if (wasEnabled != Core.settings.enableMapMountains)
                {
                    if (Find.CurrentMap != null)
                    {
                        Find.CurrentMap.mapDrawer.WholeMapChanged(MapMeshFlagDefOf.FogOfWar);
                    }
                }
            });

            if (Core.settings.enableMapMountains)
            {
                container.AddGap();

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Base Height: " + newFogHeight.ToString("0.0"));
                    newFogHeight = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), newFogHeight, 0f, 10f);
                    Core.settings.fogHeight = newFogHeight;
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddGap();

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Max Height: " + newFogMaxHeight.ToString("0.0"));
                    newFogMaxHeight = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), newFogMaxHeight, 0f, 50f);
                    Core.settings.fogMaxHeight = newFogMaxHeight;
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddGap();

                container.AddFixedBox(24f, r =>
                {
                    Widgets.CheckboxLabeled(r, "Fix mountain holes when mine (very slow)", ref Core.settings.refreshAdjacentFog);
                });

                container.AddGap();

                container.AddFixedBox(30f, r =>
                {
                    if (Widgets.ButtonText(r, "Apply"))
                    {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                            "This operation will take a considerable amount of time and may cause temporary lag. Do you want to continue?",
                            () =>
                            {
                                if (Find.CurrentMap != null)
                                {
                                    Find.CurrentMap.mapDrawer.WholeMapChanged(MapMeshFlagDefOf.FogOfWar);
                                }
                            }));
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