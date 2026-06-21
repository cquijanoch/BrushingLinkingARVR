using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BrushingAndLinking
{
    public class ProductBuilder : MonoBehaviour
    {
        public bool prepertiesCreated = false;
        public bool transparentMaterial = false;
        public Material material;
        public List<Product> products;

        [Header("Data Logging")]
        public string FolderPath = "C:/Users/quijancr/Desktop";
        public bool LogTransformProducts = false;
        private StreamWriter mainDataStreamWriter;

        private static readonly Regex ProductNameRegex =
            new Regex(@"^C([1-9]|[1-3][0-9]|4[0-4])$", RegexOptions.Compiled);

        private void Awake()
        {
            if (prepertiesCreated)
                return;

            if (products == null)
                products = new List<Product>();
            else
                products.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                string cleanName = CleanProductName(child.name);

                // Remove old demo-only background/fridge objects if they exist.
                if (child.name.EndsWith("Background") || child.name.StartsWith("Fridge"))
                {
                    Destroy(child.gameObject);
                    continue;
                }

                // Science Day shelves: only C1-C44 are real products.
                // Ignore boards, pillars, shelf structure, empty parents, etc.
                if (!IsScienceDayProduct(cleanName))
                    continue;

                child.name = cleanName;

                MeshCollider meshCollider = child.GetComponent<MeshCollider>();
                if (meshCollider == null)
                    meshCollider = child.gameObject.AddComponent<MeshCollider>();
                meshCollider.convex = true;

                ColliderSurface colliderSurface = child.GetComponent<ColliderSurface>();
                if (colliderSurface == null)
                    colliderSurface = child.gameObject.AddComponent<ColliderSurface>();
                colliderSurface.enabled = true;

                if (child.GetComponent<RayInteractable>() == null)
                    child.gameObject.AddComponent<RayInteractable>();

                Product product = child.GetComponent<Product>();
                if (product == null)
                    product = child.gameObject.AddComponent<Product>();

                product.ShowOriginalMaterial(!transparentMaterial);
                products.Add(product);
            }

            prepertiesCreated = true;
        }

        private static string CleanProductName(string objectName)
        {
            string cleanName = objectName.Replace("(Clone)", "").Trim();

            // Unity/Blender duplicate suffixes such as C1.001 should become C1.
            int dotIndex = cleanName.IndexOf('.');
            if (dotIndex >= 0)
                cleanName = cleanName.Substring(0, dotIndex);

            return cleanName;
        }

        private static bool IsScienceDayProduct(string objectName)
        {
            return ProductNameRegex.IsMatch(objectName);
        }

        private void Start()
        {
            if (LogTransformProducts)
                InitialiseDataLogging();
        }

        public void ShowProductMaterial(bool show)
        {
            if (!prepertiesCreated)
                return;

            foreach (var prod in products)
                prod.ShowOriginalMaterial(show);
        }

        private void InitialiseDataLogging()
        {
            if (!LogTransformProducts)
                return;

#if PLATFORM_ANDROID
            FolderPath = Application.persistentDataPath;
#endif
            if (!FolderPath.EndsWith('/'))
                FolderPath += '/';

            string path = string.Format("{0}Product_location_{1}.csv", FolderPath, gameObject.name);

            bool writeHeaders = !File.Exists(path);
            mainDataStreamWriter = new StreamWriter(path, true);

            if (writeHeaders)
                mainDataStreamWriter.WriteLine("Name,ID,Category,PositionX,PositionY,PositionZ");

            mainDataStreamWriter.AutoFlush = true;

            foreach (var prod in products)
                WriteDataLogging(prod.transform);
        }

        private void WriteDataLogging(Transform productTransform)
        {
            if (!LogTransformProducts)
                return;

            string productName = productTransform.name;

            mainDataStreamWriter.WriteLine(
                string.Format("{0},{1},{2},{3},{4},{5}",
                    productName,
                    productName,
                    "ScienceDay",
                    productTransform.position.x.ToString(),
                    productTransform.position.y.ToString(),
                    productTransform.position.z.ToString()
                )
            );
        }

        private void OnApplicationQuit()
        {
            if (LogTransformProducts && mainDataStreamWriter != null)
                mainDataStreamWriter.Close();
        }
    }
}