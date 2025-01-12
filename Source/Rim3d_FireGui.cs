using UnityEngine;
using Verse;
using RimWorld;

namespace Rim3D
{
    public class FireOptionsWindow : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private float lastContentHeight = 580f;

        public FireOptionsWindow()
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
            container.AddFixedBox(40f, r => Widgets.Label(r, "Fire Options"));
            Text.Font = GameFont.Small;

            container.AddGap();

            container.AddFixedBox(24f, r =>
            {
                bool wasCampfire = Core.settings.enableFireCampfire;
                Widgets.CheckboxLabeled(r, "Enable Campfire Effects", ref Core.settings.enableFireCampfire);
                if(wasCampfire != Core.settings.enableFireCampfire)
                {
                    FireEffectsManager.RefreshAllEffects();
                }
            });

            container.AddGap();

            container.AddFixedBox(24f, r =>
            {
                bool wasTorchLamp = Core.settings.enableFireTorchLamp;
                Widgets.CheckboxLabeled(r, "Enable Torch Lamp Effects", ref Core.settings.enableFireTorchLamp);
                if(wasTorchLamp != Core.settings.enableFireTorchLamp)
                {
                    FireEffectsManager.RefreshAllEffects();
                }
            });

            container.AddGap();

            container.AddFixedBox(24f, r =>
            {
                bool wasTorchWallLamp = Core.settings.enableFireTorchWallLamp;
                Widgets.CheckboxLabeled(r, "Enable Wall Torch Effects", ref Core.settings.enableFireTorchWallLamp);
                if(wasTorchWallLamp != Core.settings.enableFireTorchWallLamp)
                {
                    FireEffectsManager.RefreshAllEffects();
                }
            });

            container.AddGap();

            container.AddFixedBox(24f, r =>
            {
                bool wasOtherFires = Core.settings.enableOtherFires;
                Widgets.CheckboxLabeled(r, "Enable Other Fire Effects", ref Core.settings.enableOtherFires);
                if(wasOtherFires != Core.settings.enableOtherFires)
                {
                    FireEffectsManager.RefreshAllEffects();
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