using System.Linq;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using UnityEngine;

namespace BrushingAndLinking
{
    /// <summary>
    /// The Product class is assigned to all products that the user sees and interacts with.
    ///
    /// This class manages how the highlighting behaves.
    /// </summary>
    [RequireComponent(typeof(RayInteractable))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(ColliderSurface))]
    public class Product : MonoBehaviour
    {

        public  HighlightTechnique highlightTechnique = HighlightTechnique.None;
        public  LinkTechnique visualLinkTechnique = LinkTechnique.None;
        private bool isHighlighted = false;
        private Highlighter highlighter;
        private Highlighter visualLinker;

        private RayInteractable rayInteractable;
        private new Collider collider;
        private ColliderSurface colliderSurface;

        private void Awake()
        {
            rayInteractable = GetComponent<RayInteractable>();
            collider = GetComponent<Collider>();
            colliderSurface = GetComponent<ColliderSurface>();

            colliderSurface.InjectCollider(collider);
            rayInteractable.InjectSurface(colliderSurface);

            rayInteractable.WhenSelectingInteractorViewAdded += ProductSelected;

            // Rename the gameobject name if it has _Pack at the end of it
            string[] split = gameObject.name.Split('_');
            if (split[^1].ToLower().Contains("pack"))
            {
                gameObject.name = string.Join("_", split.Take(split.Length - 1));
            }
        }

        /// <summary>
        /// Sets the Highlighting mode of this Product
        /// </summary>
        /// <param name="technique"></param>
        public void SetHighlightTechnique(HighlightTechnique technique, bool includeLinks = false)
        {
            // Only proceed if the technique has changed
            if (highlightTechnique == technique)
                return;

            if (highlighter != null)
                Destroy(highlighter);

            if (visualLinker != null)
                Destroy(visualLinker);

            switch (technique)
            {
                case HighlightTechnique.Outline:
                    highlighter = gameObject.AddComponent<OutlineHighlighter>();
                    break;

                case HighlightTechnique.Color:
                    highlighter = gameObject.AddComponent<ColorHighlighter>();
                    break;

                //case HighlightTechnique.Arrow:
                //{
                //        highlighter = gameObject.AddComponent<ArrowHighlighter>();
                //        break;
                //}

                case HighlightTechnique.Link:
                    highlighter = gameObject.AddComponent<LinkHighlighter>();
                    highlighter.enabled = false;
                    highlighter.AlwaysVisualLink = true;
                    highlighter.enabled = true;
                    break;


                case HighlightTechnique.Size:
                    gameObject.GetComponent<MeshCollider>().enabled = false;
                    highlighter = gameObject.AddComponent<SizeHighlighter>();
                    break;
            }

            highlightTechnique = technique;

            if (includeLinks)
                visualLinkTechnique = LinkTechnique.Line;

            if (includeLinks && highlightTechnique != HighlightTechnique.Link)
                visualLinker = gameObject.AddComponent<LinkHighlighter>();

        }

        /// <summary>
        /// Toggles the Highlight state of this Product on or off
        /// </summary>
        public void ToggleHighlight()
        {
            SetHighlightState(!isHighlighted);
        }

        /// <summary>
        /// Sets the Highlight state of this Product based on the given value
        /// </summary>
        /// <param name="value">If true, turns on the Highlight</param>
        public void SetHighlightState(bool value)
        {
            if (highlighter == null)
                SetHighlightTechnique(highlightTechnique);

            if (highlighter == null)
                return;

            if (value && !isHighlighted)
            {
                highlighter.Highlight();
                if (visualLinker != null)
                    visualLinker.Highlight();
            }        
            else if (!value && isHighlighted)
            {
                highlighter.Unhighlight();
                if (visualLinker != null)
                    visualLinker.Unhighlight();
            }

            isHighlighted = value;
        }

        public void SetProductVisibility(bool visibility)
        {
            gameObject.SetActive(visibility);
        }

        private void ProductSelected(IInteractorView view)
        {
            StudyManager.Instance.ProductSelected(this);
        }
    }
}