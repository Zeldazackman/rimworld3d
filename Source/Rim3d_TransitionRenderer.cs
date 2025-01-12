using UnityEngine;
using Verse;

namespace Rim3D
{
    [StaticConstructorOnStartup]
    public static class TransitionRenderer
    {
        private static RenderTexture transitionTexture;
        private static float currentScale = 1f;
        private static float transitionProgress = 0f;
        private static bool isTransitioning;
        private static bool waitingForModeChange;
        private static bool isFadingIn;

        public static bool IsTransitioning => isTransitioning;

        public static void StartTransition()
        {
            if (isTransitioning) return;

            isTransitioning = true;
            waitingForModeChange = false;
            isFadingIn = false;
            transitionProgress = 0f;
            currentScale = 1f;
            CreateTransitionTexture(1f);
        }

        public static void UpdateTransition()
        {
            if (!isTransitioning) return;

            if (!isFadingIn)
            {
                transitionProgress += Time.deltaTime / (Core.settings.transitionDuration * 0.5f);
                float scale = Mathf.Lerp(1f, 0.05f, transitionProgress);
                
                if (transitionProgress >= 1f && !waitingForModeChange)
                {
                    waitingForModeChange = true;
                    Core.mode3d = !Core.mode3d;
                    GlobalTextureAtlasManager.rebakeAtlas = true;
                    if (Core.mode3d)
                    {
                        Core.Instance.stateManager.on();
                    }
                    else
                    {
                        Core.Instance.stateManager.off();
                    }
                    isFadingIn = true;
                    transitionProgress = 0f;
                }

                if (!Mathf.Approximately(currentScale, scale))
                {
                    SetResolutionScale(scale);
                }
            }
            else
            {
                transitionProgress += Time.deltaTime / (Core.settings.transitionDuration * 0.5f);
                float scale = Mathf.Lerp(0.05f, 1f, transitionProgress);

                if (transitionProgress >= 1f)
                {
                    isTransitioning = false;
                    CleanupTexture();
                    return;
                }

                if (!Mathf.Approximately(currentScale, scale))
                {
                    SetResolutionScale(scale);
                }
            }
        }

        private static void SetResolutionScale(float scale)
        {
            currentScale = scale;
            if (transitionTexture != null)
            {
                CleanupTexture();
            }
            CreateTransitionTexture(scale);
        }

        private static void CreateTransitionTexture(float scale)
        {
            int width = Mathf.RoundToInt(UI.screenWidth * scale);
            int height = Mathf.RoundToInt(UI.screenHeight * scale);
            
            transitionTexture = new RenderTexture(width, height, 24);
            transitionTexture.filterMode = FilterMode.Point;
            
            if (Find.Camera != null)
            {
                Find.Camera.targetTexture = transitionTexture;
            }
        }

        private static void CleanupTexture()
        {
            if (Find.Camera != null)
            {
                Find.Camera.targetTexture = null;
            }

            if (transitionTexture != null)
            {
                transitionTexture.Release();
                Object.Destroy(transitionTexture);
                transitionTexture = null;
            }
        }

        public static void RenderTransitionTexture()
        {
            if (!isTransitioning || transitionTexture == null) return;

            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(0, 0, UI.screenWidth, UI.screenHeight), transitionTexture);
            GUI.color = Color.white;
        }
    }
}