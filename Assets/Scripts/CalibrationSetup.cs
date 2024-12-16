using System.Collections.Generic;
using BrushingAndLinking;
using Oculus.Interaction.Input;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CalibrationSetup : MonoBehaviour
{
    [Header("Elements from the environment")]
    public GameObject Infraestructure;
    public GameObject Baseplate;
    public GameObject FloorPoints;

    public List<Transform> Shelves = new List<Transform>();
    public List<Transform> Points = new List<Transform>();
    public float TimeConfiguration = 0f;

    //public GameObject ShelvesInfraestructure;

    //[SerializeField]
    //private Hand Hand;

    //[SerializeField]
    //private Material LineMaterial;
    [Header("Controllers elements")]
    public Controller LeftController;
    public Controller RightController;
    public GameObject LogAuxPrefab;

    [Header("Tracking Visual Cues")]
    public GameObject SphereTrackingPrefab;
    public GameObject RightControllerMesh;
    public GameObject SphereTracking;

    [Header("UI elements")]
    public MenuUI MenuUI;
    public GameObject LogAux { get; private set; }

    private Vector3 CurrentPose;
    private OVRInput.Axis1D GrabTrigger;
    private readonly float ButtonPressThreshold = 0.5f;

    private void Start()
    {
        LeftController.enabled = true;
        RightController.enabled = true;
        LogAux = Instantiate(LogAuxPrefab, RightController.transform.GetChild(2).transform);
        GrabTrigger = OVRInput.Axis1D.SecondaryHandTrigger;
        SphereTracking = Instantiate(SphereTrackingPrefab, RightControllerMesh.transform);
    }

    private void FixedUpdate()
    {
        if (IsPressedGrabTrigger())
        {
            //hand.GetJointPose(handJointId, out currentPose);
            //RightController.TryGetPose(out CurrentPose);
            CurrentPose = SphereTracking.transform.position;
            var canvas = LogAux.transform.GetChild(0);
            canvas.GetComponentInChildren<TextMeshProUGUI>().text = ShowLogAux(CurrentPose);
            //print("POSE = " + CurrentPose.position);
            //print("ORIENTATION = " + CurrentPose.rotation);
        }

        if (IsUnpressedGrabTrigger())
        {
            TimeConfiguration = 0f;
        }
    }

    public void CalibrationInit()
    {
        MenuUI.SetOverallVisibility(true);
        Baseplate.SetActive(true);
        FloorPoints.SetActive(false);
        Infraestructure.SetActive(true);
    }

    public void ShowFloorPoints()
    {
        MenuUI.SetOverallVisibility(true);
        Baseplate.SetActive(false);
        FloorPoints.SetActive(true);
        Infraestructure.SetActive(true);
    }

    public void ShelvesCalibration()
    {
        MenuUI.SetOverallVisibility(true);
        Baseplate.SetActive(false);
        FloorPoints.SetActive(true);
        Infraestructure.SetActive(true);
        for (int i = 0; i < Infraestructure.transform.childCount; i++)
        {
            var child = Infraestructure.transform.GetChild(i);
            var pbuilder = child.GetComponent<ProductBuilder>();
            if (pbuilder.prepertiesCreated)
            {
                foreach (var p in pbuilder.products)
                {
                    p.GetComponent<BrushingAndLinking.Product>().highlightTechnique = HighlightTechnique.Calibration;
                    var outline = p.GetComponent<OutlineHighlighter>();
                    if (outline == null)
                        outline = p.AddComponent<OutlineHighlighter>();
                    outline.Highlight();
                }     
            }
        }
    }

    public void FinishShelvesCalibration()
    {
        for (int i = 0; i < Infraestructure.transform.childCount; i++)
        {
            var child = Infraestructure.transform.GetChild(i);
            var pbuilder = child.GetComponent<ProductBuilder>();
            if (pbuilder.prepertiesCreated)
            {
                foreach (var p in pbuilder.products)
                {
                    p.GetComponent<BrushingAndLinking.Product>().highlightTechnique = HighlightTechnique.None;
                    var highlighter = p.GetComponent<OutlineHighlighter>();
                    if (highlighter != null)
                        Destroy(highlighter);

                    var outline = p.GetComponent<Outline>();
                    if (outline != null)
                        Destroy(outline);
                }
            }
        }
    }

    public void StartDemo()
    {
        FinishShelvesCalibration();
        MenuUI.SetOverallVisibility(false);
        Baseplate.SetActive(false);
        FloorPoints.SetActive(true);
        Infraestructure.SetActive(true);

        MainManager.Instance.StartDemo();
    }

    private string ShowLogAux(Vector3 position)
    {
        return "Position: " + position.ToString(); 
            //"Rotation: " + rotation.eulerAngles.ToString() + "\n" +
            //"Point#: " + PointsNumber.Count;
    }

    private bool IsPressedGrabTrigger()
    {
        return OVRInput.Get(GrabTrigger, OVRInput.Controller.Active) > ButtonPressThreshold;
    }

    private bool IsUnpressedGrabTrigger()
    {
        return OVRInput.Get(GrabTrigger, OVRInput.Controller.Active) <= ButtonPressThreshold;
    }

    private void OnEnable()
    {
        if (LogAux != null)
            LogAux.SetActive(true);
    }

    private void OnDisable()
    {
        LogAux.SetActive(false);
    }

}
