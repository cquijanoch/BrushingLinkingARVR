using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Oculus.Interaction.Input;
using Unity.VisualScripting;
using UnityEngine;

namespace BrushingAndLinking
{
    /// <summary>
    /// The MainManager class is the starting point to deploy the application.
    ///
    /// The gameobject should never be disabled.
    /// </summary>
    public class MainManager : MonoBehaviour
    {
        public GameObject ColorManager;
        public GameObject InteractionManager;
        public GameObject CalibrationManager;
        public GameObject StudyMManager;
        public GameObject TabletDataVis;

        [Header("Shelves Poster-based AR")]
        public GameObject Shelves_A;
        public GameObject Shelves_B;
        public GameObject Shelves_C;
        public GameObject Shelves_D;
        public GameObject Shelves_E;
        public GameObject Occluders;

        [Header("Shelves Poster-based VR")]
        public GameObject ShelvesVR_A;
        public GameObject ShelvesVR_B;
        public GameObject ShelvesVR_C; //Fridge
        public GameObject EnvironmentRoom;

        [Header("Shelves Real-based ARVR")]
        public List<GameObject> ShelvesReal;
        public GameObject EnvironmentRoom2;

        public Light LightInfrastructure;
        
        public EnvironmentMode EnvironmentMode = EnvironmentMode.AR;
        public SupermarketVersion supermarketVersion = SupermarketVersion.None;
        public ApplicationMode AppMode;

        private Handedness CurrentHandedness;
        private Dictionary<(ApplicationMode, EnvironmentMode, SupermarketVersion), List<Product>> ProductDictionary;

        public static MainManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(this);
            else Instance = this;

            AppMode = ApplicationMode.None;
            CurrentHandedness = Handedness.Right;
            InteractionManager.SetActive(true);
            TabletDataVis.SetActive(true);
        }
        void Start()
        {
            EnvironmentRoom.SetActive(false);
            StudyMManager.SetActive(true);
            ColorManager.SetActive(true);
            StartCalibration();
        }

        private void FixedUpdate()
        {
            if (AppMode == ApplicationMode.Study)
            {
                if (OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch) &&
                   OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch) &&
                   OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch))
                {
                    StudyManager.Instance.PauseStudy();
                    StartCalibration();
                    return;
                }
            }

            if (AppMode == ApplicationMode.Study && StudyManager.Instance.Status == StudyMode.Questionnaire)
            {
                if (OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch))
                    StudyManager.Instance.ContinueQuestionnaire();
            }
        }

        public void ReadShelves(SupermarketVersion supermarket)
        {
            CleanProductsDictionary();
            switch (supermarket)
            {
                case SupermarketVersion.None:
                    break;
                case SupermarketVersion.SupermarketPoster:
                    ReadShelvesPoster();
                    break;
                case SupermarketVersion.SupermarketReal:
                    ReadShelvesReal();
                    break;
            }

            supermarketVersion = supermarket;

        }

        private void ReadShelvesReal()
        {
            var keyDemoAR = (ApplicationMode.Demo, EnvironmentMode.AR, SupermarketVersion.SupermarketReal);
            
            foreach (var shelfReal in ShelvesReal)
                if (ProductDictionary.ContainsKey(keyDemoAR))
                    ProductDictionary[keyDemoAR].AddRange(shelfReal.GetComponent<ProductBuilder>().products);
                else
                    ProductDictionary.Add(keyDemoAR, ShelvesReal[0].GetComponent<ProductBuilder>().products.ToList());

            var keyDemoVR = (ApplicationMode.Demo, EnvironmentMode.VR, SupermarketVersion.SupermarketReal);

            foreach (var shelfReal in ShelvesReal)
                if (ProductDictionary.ContainsKey(keyDemoVR))
                    ProductDictionary[keyDemoVR].AddRange(shelfReal.GetComponent<ProductBuilder>().products);
                else
                    ProductDictionary.Add(keyDemoVR, ShelvesReal[0].GetComponent<ProductBuilder>().products.ToList());
        }

        private void ReadShelvesPoster()
        {
            var keyDemoAR = (ApplicationMode.Demo, EnvironmentMode.AR, SupermarketVersion.SupermarketPoster);
            ProductDictionary.Add(keyDemoAR, Shelves_A.GetComponent<ProductBuilder>().products.ToList());
            ProductDictionary[keyDemoAR].AddRange(Shelves_B.GetComponent<ProductBuilder>().products);
            ProductDictionary[keyDemoAR].AddRange(Shelves_C.GetComponent<ProductBuilder>().products);
            ProductDictionary[keyDemoAR].AddRange(Shelves_D.GetComponent<ProductBuilder>().products);
            ProductDictionary[keyDemoAR].AddRange(Shelves_E.GetComponent<ProductBuilder>().products);

            var keyDemoVR = (ApplicationMode.Demo, EnvironmentMode.VR, SupermarketVersion.SupermarketPoster);
            ProductDictionary.Add(keyDemoVR, Shelves_A.GetComponent<ProductBuilder>().products.ToList());
            ProductDictionary[keyDemoVR].AddRange(Shelves_B.GetComponent<ProductBuilder>().products);
            ProductDictionary[keyDemoVR].AddRange(Shelves_C.GetComponent<ProductBuilder>().products);
            ProductDictionary[keyDemoVR].AddRange(Shelves_D.GetComponent<ProductBuilder>().products);
            ProductDictionary[keyDemoVR].AddRange(Shelves_E.GetComponent<ProductBuilder>().products);

            var keyStudyAR = (ApplicationMode.Study, EnvironmentMode.AR, SupermarketVersion.SupermarketPoster);
            ProductDictionary.Add(keyStudyAR, Shelves_A.GetComponent<ProductBuilder>().products.ToList());
            ProductDictionary[keyStudyAR].AddRange(Shelves_B.GetComponent<ProductBuilder>().products);

            var keyStudyVR = (ApplicationMode.Study, EnvironmentMode.VR, SupermarketVersion.SupermarketPoster);
            ProductDictionary.Add(keyStudyVR, ShelvesVR_A.GetComponent<ProductBuilder>().products.ToList());
            ProductDictionary[keyStudyVR].AddRange(ShelvesVR_B.GetComponent<ProductBuilder>().products);
        }

        private void CleanProductsDictionary()
        {
            if (ProductDictionary == null)
            { 
                ProductDictionary = new Dictionary<(ApplicationMode, EnvironmentMode, SupermarketVersion), List<Product>>();
                return;
            }

                foreach (var list in ProductDictionary.Values)
                    list?.Clear();

            ProductDictionary.Clear();
        }

        public void StartCalibration()
        {
            StartCoroutine(ChangeEnvironment(EnvironmentMode.AR));
            InteractionManager.SetActive(false);
            CalibrationManager.SetActive(true);
            StudyMManager.SetActive(false);

            SetHandedness(Handedness.Right);
            InteractionManager.GetComponent<ControllerHandler>().SetHandedness(CurrentHandedness);

        }

        public void RestoreStudy()
        {
            if (AppMode == ApplicationMode.Study)
            {
                InteractionManager.SetActive(true);
                CalibrationManager.SetActive(false);
                StudyMManager.SetActive(true);
                StudyManager.Instance.RestoreStudy();
            }   
        }

        public void StartDemo()
        {
            AppMode = ApplicationMode.Demo;

            InteractionManager.SetActive(true);
            CalibrationManager.SetActive(false);
            StudyMManager.SetActive(true);
            StartCoroutine(PlayDemo());
        }

        private IEnumerator PlayDemo()
        {
            yield return new WaitForEndOfFrame();
            StudyManager.Instance.StartDemo();
        }

        public void StopStudy()
        {
            if (AppMode != ApplicationMode.Study)
                return;

            StudyManager.Instance.StopStudy();
            AppMode = ApplicationMode.None;
        }

        public void StartStudy()
        {
            AppMode = ApplicationMode.Study;

            InteractionManager.SetActive(true);
            CalibrationManager.SetActive(false);
            StudyMManager.SetActive(true);

            InteractionManager.GetComponent<ControllerHandler>().SetHandedness(CurrentHandedness);
            StudyManager.Instance.SetHandedness(CurrentHandedness);

            StartCoroutine(PlayStudy());
        }

        private IEnumerator PlayStudy()
        {
            yield return new WaitForSeconds(1f);
            StudyManager.Instance.StartStudy();
        }

        public IEnumerator ChangeEnvironment(EnvironmentMode environment)
        {
            if (EnvironmentMode == environment)
                yield return null;

            EnvironmentMode = environment;
            bool isVR = EnvironmentMode == EnvironmentMode.VR;

            if (isVR)
            {
                EnvironmentRoom.SetActive(true);
                Occluders.SetActive(false);
                LightInfrastructure.type = LightType.Point;
            }
            else
            {
                EnvironmentRoom.SetActive(false);
                Occluders.SetActive(true);
                LightInfrastructure.type = LightType.Directional;
            }

            yield return new WaitForEndOfFrame();

            if (OVRManager.instance != null)
                OVRManager.instance.isInsightPassthroughEnabled = !isVR;
        }

        public void GetVisibility(ApplicationMode mode)
        {               
            if (supermarketVersion == SupermarketVersion.SupermarketPoster)
            {
                if (mode == ApplicationMode.Study)
                {
                    if (EnvironmentMode.AR == EnvironmentMode)
                    {
                        Shelves_A.SetActive(true);
                        Shelves_B.SetActive(true);
                        ShelvesVR_A.SetActive(false);
                        ShelvesVR_B.SetActive(false);
                        ShelvesVR_C.SetActive(false);
                    }
                    else
                    {
                        Shelves_A.SetActive(false);
                        Shelves_B.SetActive(false);
                        ShelvesVR_A.SetActive(true);
                        ShelvesVR_B.SetActive(true);
                        ShelvesVR_C.SetActive(true);
                    }

                    Shelves_C.SetActive(false);
                    Shelves_D.SetActive(false);
                    Shelves_E.SetActive(false);
                }
                else if (mode == ApplicationMode.Demo)
                {

                    Shelves_A.SetActive(true);
                    Shelves_B.SetActive(true);
                    Shelves_C.SetActive(true);
                    Shelves_D.SetActive(true);
                    Shelves_E.SetActive(true);

                    ShelvesVR_A.SetActive(false);
                    ShelvesVR_B.SetActive(false);
                    ShelvesVR_C.SetActive(false);
                }
                else if (mode == ApplicationMode.None)
                {
                    Shelves_A.SetActive(false);
                    Shelves_B.SetActive(false);
                    Shelves_C.SetActive(false);
                    Shelves_D.SetActive(false);
                    Shelves_E.SetActive(false);

                    ShelvesVR_A.SetActive(false);
                    ShelvesVR_B.SetActive(false);
                    ShelvesVR_C.SetActive(false);
                }
            }
            
        }

        public List<Product> GetProducts(ApplicationMode mode)
        {
            if (mode == ApplicationMode.None)
                mode = ApplicationMode.Demo;
            var key = (mode, EnvironmentMode, supermarketVersion);
            if (ProductDictionary == null)
                return new List<Product>();

            return ProductDictionary[key];

        }

        public void SetHandedness(Handedness handedness)
        {
            CurrentHandedness = handedness;
        }
    }

}
