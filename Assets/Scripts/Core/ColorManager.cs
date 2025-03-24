using UnityEngine;

namespace BrushingAndLinking
{
    public class ColorManager: MonoBehaviour
    {
        public static ColorManager Instance { get; private set; }

        public Color InitColor = Constants.DefaultHighlightColor;
        public Color EndColor = Color.yellow;
        public Color CurrentColor = Constants.DefaultHighlightColor;
        private bool GradientColor = false;
        public int numberGradientColorBySequence = 5;
        public float GradientTime = 0f;

        private Gradient Gradient = new();
        private float TotalGradientSeconds = 3f;
        public float timeStep;
        public int currentStep = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else Instance = this;
        }

        private void Start()
        {
            //color gradient
            var colors = new GradientColorKey[2];
            colors[0] = new GradientColorKey(InitColor, 0.0f);
            colors[1] = new GradientColorKey(EndColor, TotalGradientSeconds * 0.1f);

            // Blend alpha from opaque at 0% to transparent at 100%
            var alphas = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
            alphas[1] = new GradientAlphaKey(1.0f, TotalGradientSeconds * 0.1f);

            Gradient.SetKeys(colors, alphas);

            timeStep = TotalGradientSeconds / numberGradientColorBySequence;
        }

        private void Update()
        {
            if (!GradientColor)
                return;

            GradientTime += Time.deltaTime;
           
            if (GradientTime > timeStep * currentStep)
            {
                CurrentColor = Gradient.Evaluate(timeStep * 0.1f * currentStep);
                currentStep++;
            }
                
            if (GradientTime >= TotalGradientSeconds)
            {
                GradientTime = 0f;
                currentStep = 0;
            }
                
        }

        public void ActiveGradientColor(bool active)
        {
            GradientColor = active;

            if (!active)
                CurrentColor = Constants.DefaultHighlightColor;
        }

        public bool IsGradientActive()
        {
            return GradientColor;
        }
    }
}
