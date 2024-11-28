using UnityEngine;
using UnityEditor;

namespace BrushingAndLinking
{
    [CustomEditor(typeof(HighlightManager))]
    [CanEditMultipleObjects]
    public class HighlightManagerEditor : Editor
    {
        private HighlightManager highlightManagerScript;
        private string ProductToHighlight;

        private void OnEnable()
        {
            highlightManagerScript = (HighlightManager)target;
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                ProductToHighlight = EditorGUILayout.TextField("Product name to (un)highlight: ", ProductToHighlight);

                if (GUILayout.Button("Highlight Product"))
                    highlightManagerScript.HighlightProductByName(ProductToHighlight);

                if (GUILayout.Button("Unhighlight Product"))
                    highlightManagerScript.HighlightProductByName(ProductToHighlight);
            }

            DrawDefaultInspector();
        }
    }
}