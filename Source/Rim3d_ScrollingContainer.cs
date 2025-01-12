using UnityEngine;
using Verse;

namespace Rim3D
{
    public class ScrollingContainer
    {
        private readonly Rect outRect;
        private readonly float width;
        private float currentHeight;

        public float ContentHeight => currentHeight;

        public ScrollingContainer(Rect rect)
        {
            this.outRect = rect;
            this.width = rect.width;
            Reset();
        }

        public void Reset()
        {
            currentHeight = 0f;
        }

        public void Begin(ref Vector2 scrollPosition, float lastContentHeight)
        {
            Rect viewRect = new Rect(0f, 0f, width - 16f, lastContentHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        }

        public void End()
        {
            Widgets.EndScrollView();
        }

        public Rect GetRect(float height)
        {
            Rect result = new Rect(0f, currentHeight, width - 16f, height);
            currentHeight += height;
            return result;
        }

        public void AddGap(float gapHeight = 12f)
        {
            currentHeight += gapHeight;
        }

        public void AddFixedBox(float height, System.Action<Rect> drawer)
        {
            Rect rect = GetRect(height);
            drawer(rect);
        }
    }
}
