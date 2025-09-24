using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Input;
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

        public GameObject Shelves_A;
        public GameObject Shelves_B;
        public GameObject Shelves_C;
        public GameObject Shelves_D;
        public GameObject Shelves_E;

        public GameObject ShelvesVR_A;
        public GameObject ShelvesVR_B;
        public GameObject ShelvesVR_C; //Fridge
        
        public GameObject ShelvesDemo1VR;
        public GameObject ShelvesDemo2VR;
        public GameObject ShelvesDemo1AR;
        public GameObject ShelvesDemo2AR;

        //public GameObject ShelvesInfraestructure;
        public GameObject Shelves1Infraestructure;
        public GameObject Shelves2Infraestructure;
        public GameObject EnvironmnetInfraestructure;
        public Light LightInfraestructure;
        //public GameObject Occluders;
        public GameObject OccludersS1;
        public GameObject OccludersS2;
        public EnvironmentMode EnvironmentMode = EnvironmentMode.AR;
        public ApplicationMode AppMode;

        private Handedness CurrentHandedness;
        private Dictionary<Tuple<ApplicationMode, EnvironmentMode>, List<Product>> ProductDictionary;

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
            EnvironmnetInfraestructure.SetActive(false);
            ReadShelves();
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

        private void ReadShelves()
        {
            ProductDictionary = new Dictionary<Tuple<ApplicationMode, EnvironmentMode>, List<Product>>();

            var keyDemo = new Tuple<ApplicationMode, EnvironmentMode>(ApplicationMode.Demo, EnvironmentMode.AR);
            ProductDictionary.Add(keyDemo, ShelvesDemo1AR.GetComponent<ProductBuilder>().products);
            ProductDictionary[keyDemo].AddRange(ShelvesDemo2AR.GetComponent<ProductBuilder>().products);
            //ProductDictionary.Add(keyDemo, Shelves_A.GetComponent<ProductBuilder>().products);
            //ProductDictionary[keyDemo].AddRange(Shelves_B.GetComponent<ProductBuilder>().products);
            //ProductDictionary[keyDemo].AddRange(Shelves_C.GetComponent<ProductBuilder>().products);
            //ProductDictionary[keyDemo].AddRange(Shelves_D.GetComponent<ProductBuilder>().products);
            //ProductDictionary[keyDemo].AddRange(Shelves_E.GetComponent<ProductBuilder>().products);

            var keyStudyAR = new Tuple<ApplicationMode, EnvironmentMode>(ApplicationMode.Study, EnvironmentMode.AR);
            ProductDictionary.Add(keyStudyAR, ShelvesDemo1AR.GetComponent<ProductBuilder>().products);
            ProductDictionary[keyStudyAR].AddRange(ShelvesDemo2AR.GetComponent<ProductBuilder>().products);
            //ProductDictionary.Add(keyStudyAR, Shelves_A.GetComponent<ProductBuilder>().products);
            //ProductDictionary[keyStudyAR].AddRange(Shelves_B.GetComponent<ProductBuilder>().products);

            var keyStudyVR = new Tuple<ApplicationMode, EnvironmentMode>(ApplicationMode.Study, EnvironmentMode.VR);
            ProductDictionary.Add(keyStudyVR, ShelvesDemo1VR.GetComponent<ProductBuilder>().products);
            ProductDictionary[keyStudyVR].AddRange(ShelvesDemo2VR.GetComponent<ProductBuilder>().products);

            //ProductDictionary.Add(keyStudyVR, ShelvesVR_A.GetComponent<ProductBuilder>().products);
            //ProductDictionary[keyStudyVR].AddRange(ShelvesVR_B.GetComponent<ProductBuilder>().products);
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
                EnvironmnetInfraestructure.SetActive(true);
                OccludersS1.SetActive(false);
                OccludersS2.SetActive(false);
                LightInfraestructure.type = LightType.Point;
            }
            else
            {
                EnvironmnetInfraestructure.SetActive(false);
                OccludersS1.SetActive(true);
                OccludersS2.SetActive(true);
                LightInfraestructure.type = LightType.Directional;
            }

            yield return new WaitForEndOfFrame();

            if (OVRManager.instance != null)
                OVRManager.instance.isInsightPassthroughEnabled = !isVR;
        }

        //private void ShowMaterialInfraestructure(bool show)
        //{
        //    for (int i = 0; i < ShelvesInfraestructure.transform.childCount; i++)
        //    {
        //        var child = ShelvesInfraestructure.transform.GetChild(i);
        //        var pbuilder = child.GetComponent<ProductBuilder>();
        //        if (pbuilder != null && pbuilder.prepertiesCreated)
        //            pbuilder.ShowProductMaterial(show);
        //    }
        //}

        //public void GetVisibility(ApplicationMode mode)
        //{
        //    if (mode == ApplicationMode.Study)
        //    {
        //        if (EnvironmentMode.AR == EnvironmentMode)
        //        {
        //            Shelves_A.SetActive(true);
        //            Shelves_B.SetActive(true);
        //            ShelvesVR_A.SetActive(false);
        //            ShelvesVR_B.SetActive(false);
        //            ShelvesVR_C.SetActive(false);
        //        }
        //        else
        //        {
        //            Shelves_A.SetActive(false);
        //            Shelves_B.SetActive(false);
        //            ShelvesVR_A.SetActive(true);
        //            ShelvesVR_B.SetActive(true);
        //            ShelvesVR_C.SetActive(true);
        //        }
                
        //        Shelves_C.SetActive(false);
        //        Shelves_D.SetActive(false);
        //        Shelves_E.SetActive(false);
        //    }
        //    else if (mode == ApplicationMode.Demo)
        //    {

        //        Shelves_A.SetActive(true);
        //        Shelves_B.SetActive(true);
        //        Shelves_C.SetActive(true);
        //        Shelves_D.SetActive(true);
        //        Shelves_E.SetActive(true);

        //        ShelvesVR_A.SetActive(false);
        //        ShelvesVR_B.SetActive(false);
        //        ShelvesVR_C.SetActive(false);
        //    }
        //    else if (mode == ApplicationMode.None)
        //    {
        //        Shelves_A.SetActive(false);
        //        Shelves_B.SetActive(false);
        //        Shelves_C.SetActive(false);
        //        Shelves_D.SetActive(false);
        //        Shelves_E.SetActive(false);

        //        ShelvesVR_A.SetActive(false);
        //        ShelvesVR_B.SetActive(false);
        //        ShelvesVR_C.SetActive(false);
        //    }
        //}

        public void GetVisibilityDemo(ApplicationMode mode)
        {
            if (mode == ApplicationMode.Study)
            {
                if (EnvironmentMode.AR == EnvironmentMode)
                {
                    ShelvesDemo1AR.SetActive(true);
                    ShelvesDemo2AR.SetActive(true);
                    ShelvesDemo1VR.SetActive(false);
                    ShelvesDemo2VR.SetActive(false);
                }
                else
                {
                    ShelvesDemo1AR.SetActive(false);
                    ShelvesDemo2AR.SetActive(false);
                    ShelvesDemo1VR.SetActive(true);
                    ShelvesDemo2VR.SetActive(true);
                }
                    
            }
            else if (mode == ApplicationMode.Demo)
            {
                ShelvesDemo1AR.SetActive(true);
                ShelvesDemo2AR.SetActive(true);
                ShelvesDemo1VR.SetActive(false);
                ShelvesDemo2VR.SetActive(false);
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

                ShelvesDemo1AR.SetActive(false);
                ShelvesDemo2AR.SetActive(false);
                ShelvesDemo1VR.SetActive(false);
                ShelvesDemo2VR.SetActive(false);
            }
        }

        public List<Product> GetProductsByMode(ApplicationMode mode)//1 = demo, 2 = study, No
        {
            if (mode == ApplicationMode.None)
                mode = ApplicationMode.Demo;
            var key = new Tuple<ApplicationMode, EnvironmentMode>(mode, EnvironmentMode);
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
