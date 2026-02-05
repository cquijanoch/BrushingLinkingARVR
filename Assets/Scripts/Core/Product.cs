using System.Collections.Generic;
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
        public bool LinkToChildForwarding = false;

        private bool isHighlighted = false;
        private Highlighter highlighter;
        private Highlighter visualLinker;

        private RayInteractable rayInteractable;
        private new Collider collider;
        private ColliderSurface colliderSurface;
        private Dictionary<Renderer, Material[]> originalMaterials;
        

        private void Awake()
        {
            originalMaterials = new Dictionary<Renderer, Material[]>();
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
                originalMaterials[renderer] = renderer.materials;

            rayInteractable = GetComponent<RayInteractable>();
            collider = GetComponent<Collider>();
            colliderSurface = GetComponent<ColliderSurface>();

            colliderSurface.InjectCollider(collider);
            rayInteractable.InjectSurface(colliderSurface);

            rayInteractable.WhenSelectingInteractorViewAdded += ProductSelected;

        }

        /// <summary>
        /// Sets the Highlighting mode of this Product
        /// </summary>
        /// <param name="technique"></param>
        public void SetHighlightTechnique(HighlightTechnique technique)
        {
            if (highlightTechnique == technique)
                return;

            if (highlighter != null)
                Destroy(highlighter);

            if (visualLinker != null)
                Destroy(visualLinker);

            bool gradientColor = false;

            switch (technique)
            {
                case HighlightTechnique.Outline:
                    highlighter = gameObject.AddComponent<OutlineHighlighter>();
                    highlighter.enabled = false;
                    break;

                case HighlightTechnique.AnimatedOutline:
                    highlighter = gameObject.AddComponent<OutlineHighlighter>();
                    highlighter.enabled = false;
                    gradientColor = true;
                    break;

                case HighlightTechnique.AnimatedOutlineLink:
                    highlighter = gameObject.AddComponent<OutlineHighlighter>();
                    highlighter.enabled = false;
                    visualLinkTechnique = LinkTechnique.Line;
                    visualLinker = gameObject.AddComponent<LinkHighlighter>();
                    visualLinker.enabled = false;
                    gradientColor = true;
                    
                    break;

                case HighlightTechnique.Color:
                    highlighter = gameObject.AddComponent<ColorHighlighter>();
                    highlighter.enabled = false;
                    break;

                //case HighlightTechnique.Arrow:
                //{
                //        highlighter = gameObject.AddComponent<ArrowHighlighter>();
                //        break;
                //}

                case HighlightTechnique.Link:
                    visualLinker = gameObject.AddComponent<LinkHighlighter>();
                    visualLinker.enabled = false;
                    break;

                case HighlightTechnique.None:
                    visualLinker = null;
                    highlighter = null;
                    break;

                //case HighlightTechnique.Size:
                //    gameObject.GetComponent<MeshCollider>().enabled = false;
                //    highlighter = gameObject.AddComponent<SizeHighlighter>();
                //    break;
            }

            highlightTechnique = technique;
            isHighlighted = false;

            if (highlighter != null)
            {
                highlighter.enabled = true;
                highlighter.ActiveGradientColor(gradientColor);
            }

            if (visualLinker != null)
                visualLinker.enabled = true;
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
            //if (highlighter == null)
            //    SetHighlightTechnique(highlightTechnique);

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
            if (StudyManager.Instance == null) return;

            StudyManager.Instance.ProductSelected(this);
        }

        public void ShowOriginalMaterial(bool show, Material optionalMaterial = null)
        {
            foreach (var kvp in originalMaterials)
            {
                if (show)
                    kvp.Key.materials = kvp.Value;
                else if (optionalMaterial != null)
                    kvp.Key.materials = new Material[1] { optionalMaterial };
                else
                {
                    Material[] newMaterials = new Material[kvp.Key.materials.Count()];
                    for (int i = 0; i < newMaterials.Length; i++)
                        newMaterials[i] = Resources.Load<Material>("Materials/TransparentProduct");
                    kvp.Key.materials = newMaterials;


                }
                    
            }        
        }
    }
}