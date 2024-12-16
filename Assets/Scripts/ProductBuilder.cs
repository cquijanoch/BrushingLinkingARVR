using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace BrushingAndLinking
{
    public class ProductBuilder : MonoBehaviour
    {
        public bool prepertiesCreated = false;
        public bool transparentMaterial = false;
        //public Material material;
        public List<Transform> products;

        private void Awake()
        {
            if (prepertiesCreated)
                return;

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                ////child.name = transform.name + child.name;
                //if (!child.name.StartsWith("Shelf") &&
                //    !child.name.EndsWith("1"))
                //    child.gameObject.SetActive(false);

                if (!child.name.EndsWith("Background"))
                {
                    if (transparentMaterial)
                    {
                        var renderer = child.GetComponent<Renderer>();
                        renderer.materials = new Material[0];
                    }

                    //var renderer = child.GetComponent<Renderer>();
                    //renderer.material = material;
                    //renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    child.AddComponent<MeshCollider>().convex = true;
                    child.AddComponent<ColliderSurface>().enabled = false;
                    child.AddComponent<RayInteractable>();
                    var product = child.AddComponent<Product>();
                    //product.SetHighlightTechnique(HighlightTechnique.Outline);
                    //product.SetHighlightState(true);
                    products.Add(child);
                }
                else
                    child.gameObject.SetActive(false);
            }

            prepertiesCreated = true;
        }
    }

}
