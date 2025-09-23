using System.Collections;
using System.Collections.Generic;
using BrushingAndLinking;
using Oculus.Interaction.Input;
using TMPro;
using UnityEngine;

public class CalibrationSetup : MonoBehaviour
{
    [Header("Elements from the environment")]
    public GameObject Infraestructure;
    public GameObject Baseplate;
    public GameObject InfraestructureS1VR;
    public GameObject InfraestructureS2VR;
    public GameObject InfraestructureS1AR;
    public GameObject InfraestructureS2AR;
    //public GameObject FloorPoints;
    //public Transform PivotCalibration;

    [Header("Pivot Demo Shelf 1")]
    public GameObject FloorDemo1;
    public Transform PivotDemo1;

    [Header("Pivot Demo Shelf 2")]
    public GameObject FloorDemo2;
    public Transform PivotDemo2;

    public List<Transform> Shelves = new List<Transform>();
    public List<Transform> Points = new List<Transform>();
    public float TimeConfiguration = 0f;

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
    //public bool ShowLog = true;

    private bool visualLog = false;
    private Vector3 CurrentPose;
    private OVRInput.Axis1D GrabTrigger;
    private readonly float ButtonPressThreshold = 0.5f;
    private bool DisplayedShelvesCalibration = false;

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
        if (!visualLog) return;

        if (IsPressedGrabTrigger())
        {
            CurrentPose = SphereTracking.transform.position;
            var canvas = LogAux.transform.GetChild(0);
            canvas.GetComponentInChildren<TextMeshProUGUI>().text = ShowLogAux(CurrentPose);
        }

        if (IsUnpressedGrabTrigger())
        {
            TimeConfiguration = 0f;
        }
    }

    public void ShowLog(bool show)
    {
        visualLog = show;
        if (LogAux != null)
            LogAux.SetActive(show);
    }

    public bool CalibrationInit()
    {
        if (MainManager.Instance.AppMode != ApplicationMode.None)
            return false;

        MenuUI.SetOverallVisibility(true);
        Baseplate.SetActive(true);
        //FloorPoints.SetActive(false);
        FloorDemo1.SetActive(false);
        FloorDemo2.SetActive(false);
        MainManager.Instance.GetVisibilityDemo(ApplicationMode.Demo);
        StartCoroutine(ResetAllCues());

        return true;
    }

    public bool ShowFloorPoints()
    {
        StartCoroutine(ResetAllCues());
        MenuUI.SetOverallVisibility(true);
        Baseplate.SetActive(false);
        //FloorPoints.SetActive(true);
        FloorDemo1.SetActive(true);
        FloorDemo2.SetActive(true);
        MainManager.Instance.GetVisibilityDemo(ApplicationMode.Demo);

        return true;
    }

    public bool ShowShelvesCalibration()
    {
        if (MainManager.Instance.AppMode != ApplicationMode.None)
            return false;

        if (DisplayedShelvesCalibration)
            return false;

        StartCoroutine(ResetAllCues());

        MenuUI.SetOverallVisibility(true);
        Baseplate.SetActive(false);
        //FloorPoints.SetActive(true);
        FloorDemo1.SetActive(true);
        FloorDemo2.SetActive(true);
        FinishShelvesCalibration();

        MainManager.Instance.GetVisibilityDemo(ApplicationMode.Demo);

        StartCoroutine(ShowAllCues());

        return true;
    }


    public void FinishShelvesCalibration()
    {
        //Infraestructure.transform.SetPositionAndRotation(PivotCalibration.transform.position, PivotCalibration.transform.rotation);
        //Baseplate.transform.SetPositionAndRotation(PivotCalibration.transform.position, PivotCalibration.transform.rotation);
        //Infraestructure.transform.SetPositionAndRotation(PivotDemo1.transform.position, PivotDemo1.transform.rotation);
        //Infraestructure.transform.SetPositionAndRotation(PivotDemo2.transform.position, PivotDemo2.transform.rotation);

        InfraestructureS1VR.transform.SetPositionAndRotation(PivotDemo1.transform.position, PivotDemo1.transform.rotation);
        InfraestructureS2VR.transform.SetPositionAndRotation(PivotDemo2.transform.position, PivotDemo2.transform.rotation);
        InfraestructureS1AR.transform.SetPositionAndRotation(PivotDemo1.transform.position, PivotDemo1.transform.rotation);
        InfraestructureS2AR.transform.SetPositionAndRotation(PivotDemo2.transform.position, PivotDemo2.transform.rotation);

        Baseplate.transform.position = Baseplate.transform.position + new Vector3(0f, 0.001f, 0f);
        Baseplate.transform.Rotate(new Vector3(90f, 0, -180));

        //MainManager.Instance.EnvironmnetInfraestructure.transform.SetPositionAndRotation(PivotCalibration.transform.position, PivotCalibration.transform.rotation);

        MainManager.Instance.OccludersS1.transform.SetPositionAndRotation(PivotDemo1.transform.position, PivotDemo1.transform.rotation);
        MainManager.Instance.OccludersS2.transform.SetPositionAndRotation(PivotDemo2.transform.position, PivotDemo2.transform.rotation);
        MainManager.Instance.Shelves1Infraestructure.transform.SetPositionAndRotation(PivotDemo1.transform.position, PivotDemo1.transform.rotation);
        MainManager.Instance.Shelves2Infraestructure.transform.SetPositionAndRotation(PivotDemo2.transform.position, PivotDemo2.transform.rotation);
    }

    private IEnumerator ResetAllCues()
    {
        DisplayedShelvesCalibration = false;
        HighlightManager.Instance.UnhighlightAllProducts();
        LinkHighlighter.VisMarksChanged();

        foreach (var product in MainManager.Instance.GetProductsByMode(ApplicationMode.Demo))
            product.SetHighlightTechnique(HighlightTechnique.None);

        yield return new WaitForEndOfFrame();
    }

    private IEnumerator ShowAllCues()
    {
        yield return new WaitForEndOfFrame();
        foreach (var product in MainManager.Instance.GetProductsByMode(ApplicationMode.Demo))
        {
            product.SetHighlightTechnique(HighlightTechnique.Outline);
            product.SetHighlightState(true);
        }

        DisplayedShelvesCalibration = true;
    }

    public bool StartDemo()
    {
        if (MainManager.Instance.AppMode != ApplicationMode.None)
            return false;

        StartCoroutine(ResetAllCues());
        MenuUI.SetOverallVisibility(false);
        Baseplate.SetActive(false);
        //FloorPoints.SetActive(false);
        FloorDemo1.SetActive(false);
        FloorDemo2.SetActive(false);
        Infraestructure.SetActive(true);

        MainManager.Instance.StartDemo();

        return true;
    }

    public bool StartStudy()
    {
        if (MainManager.Instance.AppMode == ApplicationMode.Demo)
            return false;

        StartCoroutine(ResetAllCues());
        MenuUI.SetOverallVisibility(false);
        Baseplate.SetActive(true);
        //FloorPoints.SetActive(false);
        FloorDemo1.SetActive(false);
        FloorDemo2.SetActive(false);
        Infraestructure.SetActive(true);

        if (MainManager.Instance.AppMode == ApplicationMode.Study)
        {
            MainManager.Instance.RestoreStudy();
            return true;
        }

        MainManager.Instance.StartStudy();
        return true;
    }

    public bool StopStudy()
    {
        if (MainManager.Instance.AppMode != ApplicationMode.Study)
            return false;

        MainManager.Instance.StopStudy();
        return true;
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
        ShowLog(visualLog);
        DisplayedShelvesCalibration = false;
        MenuUI.SetOverallVisibility(true);
    }

    private void OnDisable()
    {
        ShowLog(false);
    }

}
