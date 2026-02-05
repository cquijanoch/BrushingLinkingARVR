using System.Collections;
using System.Collections.Generic;
using BrushingAndLinking;
using Oculus.Interaction.Input;
using TMPro;
using UnityEngine;

public class CalibrationSetup : MonoBehaviour
{
    [Header("General")]
    //public SupermarketVersion SupermarketVersion;
    public List<Transform> Shelves = new List<Transform>();
    public List<Transform> Points = new List<Transform>();
    public float TimeConfiguration = 0f;

    [Header("Elements for the environment 1")]
    public GameObject Infrastructure;
    public GameObject Baseplate;
    public GameObject FloorPointsInfra1;
    private Transform PivotCalibrationInfra1;

    [Header("Elements for the environment 2")]
    public GameObject Infrastructure2;
    public GameObject FloorPointsInfra2;
    private Transform PivotCalibrationInfra2;

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
        PivotCalibrationInfra1 = FloorPointsInfra1.transform.GetChild(0);
        PivotCalibrationInfra2 = FloorPointsInfra2.transform.GetChild(0);
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
            TimeConfiguration = 0f;
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
        MoveBasePlate();
        Baseplate.SetActive(true);
        FloorPointsInfra1.SetActive(false);
        FloorPointsInfra2.SetActive(false);
        MainManager.Instance.GetVisibility(ApplicationMode.Demo);
        StartCoroutine(ResetAllCues());
        ActivateSupermarket(true);
        return true;
    }

    private void ActivateSupermarket(bool active)
    {
        switch (MainManager.Instance.supermarketVersion)
        {
            case SupermarketVersion.None:
                Infrastructure.SetActive(!active);
                Infrastructure2.SetActive(!active);
                break;
            case SupermarketVersion.All:
                Infrastructure.SetActive(active);
                Infrastructure2.SetActive(active);
                break;
            case SupermarketVersion.SupermarketPoster:
                Infrastructure.SetActive(active);
                Infrastructure2.SetActive(!active);
                break;

            case SupermarketVersion.SupermarketReal:
                Infrastructure.SetActive(!active);
                Infrastructure2.SetActive(active);
                break;

        }
    }

    public bool ShowFloorPoints()
    {
        StartCoroutine(ResetAllCues());
        MenuUI.SetOverallVisibility(true);
        Baseplate.SetActive(false);
        ActiveFloorPoints(true);
        MainManager.Instance.GetVisibility(ApplicationMode.Demo);

        return true;
    }

    public bool ShowShelvesCalibration()
    {
        if (MainManager.Instance.AppMode != ApplicationMode.None)
            return false;

        if (DisplayedShelvesCalibration)
            return false;

        StartCoroutine(ResetAllCues());
        ActivateSupermarket(true);
        MenuUI.SetOverallVisibility(true);
        Baseplate.SetActive(false);
        ActiveFloorPoints(true);
        FinishShelvesCalibration();

        MainManager.Instance.GetVisibility(ApplicationMode.Demo);

        StartCoroutine(ShowAllCues());

        return true;
    }


    public void FinishShelvesCalibration()
    {
        var pivot = GetPivotFloorPoints();
        GetInfraestructure().transform.SetPositionAndRotation(pivot.position, pivot.rotation);
        Baseplate.transform.SetPositionAndRotation(pivot.position, pivot.rotation);
        Baseplate.transform.position = Baseplate.transform.position + new Vector3(0f, 0.001f, 0f);
        Baseplate.transform.Rotate(new Vector3(90f, 0, -180));

        MainManager.Instance.GetVREnvironment().transform.SetPositionAndRotation(pivot.position, pivot.rotation);
    }

    private IEnumerator ResetAllCues()
    {
        DisplayedShelvesCalibration = false;
        HighlightManager.Instance.UnhighlightAllProducts();
        LinkHighlighter.VisMarksChanged();

        foreach (var product in MainManager.Instance.GetProducts(ApplicationMode.Demo))
            product.SetHighlightTechnique(HighlightTechnique.None);

        yield return new WaitForEndOfFrame();
    }

    private IEnumerator ShowAllCues()
    {
        yield return new WaitForEndOfFrame();
        foreach (var product in MainManager.Instance.GetProducts(ApplicationMode.Demo))
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
        ActiveFloorPoints(false);
        Infrastructure.SetActive(true);

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
        ActiveFloorPoints(false);
        Infrastructure.SetActive(true);

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

    private void ActiveFloorPoints(bool active)
    {
        if (active)
        {
            FloorPointsInfra1.SetActive(MainManager.Instance.supermarketVersion == SupermarketVersion.SupermarketPoster);
            FloorPointsInfra2.SetActive(MainManager.Instance.supermarketVersion == SupermarketVersion.SupermarketReal);
            return;
        }

        FloorPointsInfra1.SetActive(active);
        FloorPointsInfra2.SetActive(active);
    }

    private void MoveBasePlate()
    {
        Baseplate.transform.position = GetPivotFloorPoints().position + new Vector3(0, 0.01f, 0);
    }

    private Transform GetPivotFloorPoints()
    {
        if (MainManager.Instance.supermarketVersion == SupermarketVersion.SupermarketPoster)
            return PivotCalibrationInfra1;

        return PivotCalibrationInfra2;
    }

    private GameObject GetInfraestructure()
    {
        if (MainManager.Instance.supermarketVersion == SupermarketVersion.SupermarketPoster)
            return Infrastructure;

        return Infrastructure2;
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
