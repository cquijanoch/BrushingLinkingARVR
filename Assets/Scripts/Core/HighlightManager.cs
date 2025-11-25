using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BrushingAndLinking
{
    /// <summary>
    /// The HighlightManager is the main source to enable and disable the highlighting on Products.
    /// </summary>
    public class HighlightManager : MonoBehaviour
    {
        #region Public properties

        // Singleton pattern. The HighlightManager can be accessed from anywhere using HighlightManager.Instance
        public static HighlightManager Instance { get; private set; }

        #endregion Public properties

        #region Public variables

        // A dictionary of products. Key: The name of the Product; Value: A list of Product object references that have that name
        public Dictionary<string, List<Product>> ProductsDictionary;
        // A dictionary of whether a Product name is highlighted. Key: The name of the Product; Value: Whether the Product is highlighted
        public Dictionary<string, bool> HighlightDictionary;

        #endregion

        private void Awake()
        {
            // Assign this object to the Instance property if it isn't already assigned, otherwise delete this object
            if (Instance != null && Instance != this) Destroy(this);
            else Instance = this;

            InitialiseProductReferences();
        }

        private void InitialiseProductReferences()
        {
            ProductsDictionary = new Dictionary<string, List<Product>>();//<parent, children>
            HighlightDictionary = new Dictionary<string, bool>();//name, isHighlighted

            // Find all Products in the scene that are active
            foreach (Product product in FindObjectsOfType<Product>(false))
            {
                // Get the name of this Product. We split using a space in order to account for duplicated GameObjects (i.e., "... (1)")
                string name = product.gameObject.name.Split(' ')[0];
                //string shelf = product.transform.parent.name;

                // Add this Product reference to our dictionary, creating the associated list if it does not yet exist
                List<Product> productList;
                if (!ProductsDictionary.TryGetValue(name, out productList))
                {
                    productList = new List<Product>();
                    ProductsDictionary.Add(name, productList);
                }
                productList.Add(product);

                // Add this Product reference to our other dictionary of whether it is highlighted or not
                if (!HighlightDictionary.TryGetValue(name, out bool b))
                    HighlightDictionary.Add(name, false);
            }
        }

        public void HighlightProductByName(string name, bool unhighlightRest = true)
        {
            if (ProductsDictionary.ContainsKey(name))
            {
                foreach (Product product in ProductsDictionary[name])
                    product.SetHighlightState(true);

                HighlightDictionary[name] = true;

                if (unhighlightRest)
                {
                    foreach (string key in ProductsDictionary.Keys)
                    {
                        if (!key.Equals(name))
                        {
                            foreach (Product product in ProductsDictionary[key])
                                product.SetHighlightState(false);
                        }

                        HighlightDictionary[key] = false;
                    }
                }
            }
            else
            {
                // throw new System.Exception(string.Format("[DebugUnity] Cannot highlight Product with name {0} because it does not exist!", name));
            }

        }

        public void UnhighlightProductByName(string name)
        {
            if (ProductsDictionary.ContainsKey(name))
            {
                foreach (Product product in ProductsDictionary[name])
                    product.SetHighlightState(false);

                HighlightDictionary[name] = false;
            }
            else
            {
                // throw new System.Exception(string.Format("[DebugUnity] Cannot unhighlight Product with name {0} because it does not exist!", name));
            }
        }

        //public void ReHighlightProducts()
        //{
        //    if (HighlightDictionary.Count == 0 || !HighlightDictionary.ContainsValue(true))
        //        return;


        //    foreach (Product product in ProductsDictionary.SelectMany(p => p.Value))
        //    {
        //        if (HighlightDictionary[product.name])
        //        {
        //            product.SetHighlightState(false);
        //            product.SetHighlightState(true);
        //        } 
        //    }
                
        //}

        public void UnhighlightAllProducts()
        {
            foreach (Product product in ProductsDictionary.SelectMany(x => x.Value))
                product.SetHighlightState(false);

            foreach (var key in HighlightDictionary.Keys.ToList())
                HighlightDictionary[key] = false;
        }

        public void HighlightProductsByList(IEnumerable<string> names)
        {
            // Go through the currently highlighted products. If they are not in the incoming list, unhighlight them
            List<string> productsToUnhighlight = new ();
            foreach (var kvp in HighlightDictionary)
            {
                if (kvp.Value && !names.Contains(kvp.Key))
                    productsToUnhighlight.Add(kvp.Key);
            }

            foreach (string key in productsToUnhighlight)
                UnhighlightProductByName(key);

            foreach (string name in names)
                HighlightProductByName(name, false);
        }

        public void ResetProductReferences()
        {
            InitialiseProductReferences();
        }
    }
}