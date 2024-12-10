using System.Collections.Generic;
using Oculus.Interaction.Input;
using TMPro;
using UnityEngine;

public class CalibrationSetup : MonoBehaviour
{
    public List<Vector3> PointsNumber = new List<Vector3>();
    public List<GameObject> ShelvesPrefabs = new List<GameObject>();
    private List<GameObject> Shelves = new List<GameObject>();

    private OVRInput.Axis1D GrabTrigger;
    private readonly float ButtonPressThreshold = 0.5f;
    private readonly float TimePressThreshold = 3.5f;
    public float TimeConfiguration = 0f;

    public GameObject ShelvesInfraestructure;

    //[SerializeField]
    //private Hand Hand;

    [SerializeField]
    private Material LineMaterial;

    public Controller LeftController;
    public Controller RightController;
    public GameObject LogAuxPrefab;

    [Header("Tracking Visual Cues")]
    public GameObject SphereTrackingPrefab;
    public GameObject RightControllerMesh;
    public GameObject SphereTracking;

    private GameObject LogAux;
    private Vector3 CurrentPose;
    private int countLines = 0;
    private const int PointsByShelf = 3;

    void Start()
    {
        LeftController.enabled = false;
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
            if (TimeConfiguration > TimePressThreshold)
            {
                PointsNumber.Add(CurrentPose);
                canvas.GetComponentInChildren<TextMeshProUGUI>().text = ShowLogAux(CurrentPose);
                TimeConfiguration = 0f;

                if (PointsNumber.Count / PointsByShelf > countLines)
                {
                    for (int i = 0; i < PointsByShelf - 1; i++)
                    {
                        DrawLine(PointsNumber[PointsNumber.Count - 2 - i], PointsNumber[PointsNumber.Count - 1 - i], Color.red);
                        countLines++;
                    }
                }
            }

            TimeConfiguration += Time.fixedDeltaTime;
        }

        if (IsUnpressedGrabTrigger())
        {
            TimeConfiguration = 0f;
        }

        if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch))
            DisplayShelves();
    }

    private string ShowLogAux(Vector3 position)
    {
        return "Position: " + position.ToString() + "\n" +
            //"Rotation: " + rotation.eulerAngles.ToString() + "\n" +
            "Point#: " + PointsNumber.Count;
    }

    private bool IsPressedGrabTrigger()
    {
        return OVRInput.Get(GrabTrigger, OVRInput.Controller.Active) > ButtonPressThreshold;
    }

    private bool IsUnpressedGrabTrigger()
    {
        return OVRInput.Get(GrabTrigger, OVRInput.Controller.Active) <= ButtonPressThreshold;
    }

    private void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        //start = start; //+ new Vector3(0, 0, 0.03f);
        //end = end;// + new Vector3(0, 0, 0.03f);
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        LineRenderer lr = myLine.AddComponent<LineRenderer>();
        lr.sharedMaterial = LineMaterial;
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    private void DisplayShelves()
    {
        if (PointsNumber.Count / PointsByShelf != ShelvesPrefabs.Count)
        {
            UnityEngine.Debug.LogError("Shelves amount is not equal to points group of " + PointsByShelf);
            return;
        }
            
        foreach (var shelfPrefab in ShelvesPrefabs)
        {
            var newShelf = Instantiate(shelfPrefab, ShelvesInfraestructure.transform);
            newShelf.transform.position = PointsNumber[0];
            var normal = Vector3.Cross(PointsNumber[2] - PointsNumber[1], PointsNumber[0] - PointsNumber[1]);
            newShelf.transform.position += normal.normalized / 5f;
            newShelf.transform.rotation = Quaternion.LookRotation(normal, PointsNumber[2] - PointsNumber[1]);
            //newShelf.transform.eulerAngles = new Vector3(-90, 0, 0);
            //newShelf.transform.rotation *= Quaternion.FromToRotation(PointsNumber[0], PointsNumber[1]);
            //newShelf.transform.rotation *= Quaternion.FromToRotation(PointsNumber[0], PointsNumber[2]);


            Shelves.Add(newShelf);
        }
    }
}
