using UnityEngine;

namespace BrushingAndLinking
{
    public abstract class Highlighter : MonoBehaviour
    {
        public bool isHighlighted = false;
        protected bool AlwaysVisualLink = false;
        protected bool GradientColor = false;
        protected Color currentColor = ColorManager.Instance.CurrentColor;
        public abstract HighlightTechnique Mode { get; }
        public abstract void Highlight();
        public abstract void Unhighlight();

        public void ActiveGradientColor(bool active)
        {
            if (GradientColor != active)
            {
                GradientColor = active;
                ColorManager.Instance.ActiveGradientColor(active);
            }
        }

        public void ActiveVisualLink(bool active)
        {
            if (AlwaysVisualLink != active)
                EnableVisualLink(active);
        }

        protected void EnableVisualLink(bool visualLink)
        {
            AlwaysVisualLink = visualLink;
            var link = gameObject.GetComponent<LinkHighlighter>();
            if (link == null)
                link = gameObject.AddComponent<LinkHighlighter>();

            link.UpdateVisualLink();
           
        }

    }
}