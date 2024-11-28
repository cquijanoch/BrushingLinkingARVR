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
        //public Material material;
        public List<Transform> products;

        private void Awake()
        {
            if (prepertiesCreated)
                return;

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                //child.name = transform.name + child.name;
                if (!child.name.StartsWith("Shelf") &&
                    !child.name.EndsWith("1"))
                    child.gameObject.SetActive(false);
                
                if (!child.name.StartsWith("Shelf") &&
                    child.name.EndsWith("1"))
                {
                    
                    //var renderer = child.GetComponent<Renderer>();
                    //renderer.material = material;
                    //renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    child.AddComponent<MeshCollider>().convex = true;
                    child.AddComponent<ColliderSurface>().enabled = false;
                    child.AddComponent<RayInteractable>();
                    child.AddComponent<Product>();
                    products.Add(child);
                }
            }

            prepertiesCreated = true;
        }
    }

}
