using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

namespace BrushingAndLinking
{
    public class ProductBuilder : MonoBehaviour
    {
        public bool prepertiesCreated = false;
        public bool transparentMaterial = false;
        public Material material;
        public GameObject TargetDirection;
        public bool forwardingTarget = false;
        public List<Product> products;

        [Header("Data Logging")]
        public string FolderPath = "C:/Users/quijancr/Desktop";
        public bool LogTransformProducts = false;
        private StreamWriter mainDataStreamWriter;

        private void Awake()
        {
            if (prepertiesCreated)
                return;

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                if (child.name.EndsWith("Background") || child.name.StartsWith("Fridge"))
                    Destroy(child.gameObject);
                else
                {
                    child.AddComponent<MeshCollider>().convex = true;
                    child.AddComponent<ColliderSurface>().enabled = true;
                    child.AddComponent<RayInteractable>();
                    var product = child.AddComponent<Product>();
                    product.ShowOriginalMaterial(!transparentMaterial);

                    if (forwardingTarget)
                        Forwarding(child);
                    
                    product.LinkToChildForwarding = forwardingTarget;
                    products.Add(product);
                }
            }

            prepertiesCreated = true;
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

            // Get the path, which differs if we use a new file per each participant
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

            mainDataStreamWriter.WriteLine(
                string.Format("{0},{1},{2},{3},{4},{5}",
                productTransform.name,
                productTransform.name.Split('_')[1],
                productTransform.name.Split('_')[0],
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

        private void Forwarding(Transform referent)
        {
            TargetDirection = new GameObject("TargetDir_" + referent.name);
           
            TargetDirection.transform.SetPositionAndRotation(referent.position, transform.rotation);
             var pos = new Vector3(0f, -0.005f, 0f);
            TargetDirection.transform.parent = referent;
            TargetDirection.transform.localScale = Vector3.one;
            TargetDirection.transform.localPosition = pos;

        }

    }

}
