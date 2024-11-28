using UnityEngine;

namespace BrushingAndLinking
{
    public class SizeHighlighter : Highlighter
    {
        public override HighlightTechnique Mode { get { return HighlightTechnique.Size; } }
        public float SizeFactor = 1.5f;
        private Vector3 originalScale = Vector3.one;

        public override void Highlight()
        {
            transform.localScale = originalScale * SizeFactor;
            isHighlighted = true;
        }

        public override void Unhighlight()
        {
            transform.localScale = originalScale;
            isHighlighted = false;
        }

        private void OnEnable()
        {
            originalScale = transform.localScale;
        }

        public void OnDisable()
        {
            Unhighlight();
        }
    }
}