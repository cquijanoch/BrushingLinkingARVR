
namespace BrushingAndLinking
{
    //[DisallowMultipleComponent]
    public class OutlineHighlighter : Highlighter
    {
        public override HighlightTechnique Mode { get { return HighlightTechnique.Outline; } }

        private Outline outlineScript;
        private readonly float outlineWidth = 15.0f;

        private void Awake()
        {
            outlineScript = gameObject.AddComponent<Outline>();
            outlineScript.enabled = false;
            outlineScript.OutlineWidth = outlineWidth;
            outlineScript.precomputeOutline = true;
            outlineScript.OutlineMode = Outline.Mode.OutlineAll;
        }

        private void LateUpdate()
        {
            if (outlineScript != null && outlineScript.enabled && GradientColor)
            {
                currentColor = ColorManager.Instance.CurrentColor;
                outlineScript.UpdateColor(currentColor);
            }
        }

        public override void Highlight()
        {
            if (!isHighlighted)
            {
                outlineScript.enabled = true;
                isHighlighted = true;
            }
        }

        public override void Unhighlight()
        {
            if (isHighlighted)
            {
                outlineScript.enabled = false;
                isHighlighted = false;
            }
        }

        public void OnDisable()
        {
            Unhighlight();
        }

        public void OnDestroy()
        {
           Destroy(outlineScript);
        }
    }
}