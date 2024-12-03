using System.Collections.Generic;
using Oculus.Interaction.Input;
using TMPro;
using UnityEngine;

public class CalibrationSetup : MonoBehaviour
{
    public List<Pose> PointsNumber = new List<Pose>();
    public List<GameObject> shelvesPrefabs = new List<GameObject>();

    private OVRInput.Axis1D GrabTrigger;
    private readonly float ButtonPressThreshold = 0.5f;
    private readonly float TimePressThreshold = 3.5f;
    public float TimeConfiguration = 0f;

    //[SerializeField]
    //private Hand Hand;

    [SerializeField]
    private Material LineMaterial;

    public Controller LeftController;
    public Controller RightController;
    public GameObject LogAuxPrefab;

    private GameObject LogAux;
    private Pose CurrentPose;
    private int countLines = 0;

    void Start()
    {
        LeftController.enabled = false;
        RightController.enabled = true;
        LogAux = GameObject.Instantiate(LogAuxPrefab, RightController.transform.GetChild(2).transform);
        GrabTrigger = OVRInput.Axis1D.SecondaryHandTrigger;

    }

    private void FixedUpdate()
    {
        if (IsPressedGrabTrigger())
        {
            //hand.GetJointPose(handJointId, out currentPose);
            RightController.TryGetPose(out CurrentPose);
            var canvas = LogAux.transform.GetChild(0);
            canvas.GetComponentInChildren<TextMeshProUGUI>().text = ShowLogAux(CurrentPose.position, CurrentPose.rotation);
            //print("POSE = " + CurrentPose.position);
            //print("ORIENTATION = " + CurrentPose.rotation);
            if (TimeConfiguration > TimePressThreshold)
            {
                PointsNumber.Add(CurrentPose);
                canvas.GetComponentInChildren<TextMeshProUGUI>().text = ShowLogAux(CurrentPose.position, CurrentPose.rotation);
                TimeConfiguration = 0f;

                if (PointsNumber.Count / 2 > countLines)
                {
                    DrawLine(PointsNumber[PointsNumber.Count - 2].position, PointsNumber[PointsNumber.Count - 1].position, Color.red);
                    countLines++;
                }
            }

            TimeConfiguration += Time.fixedDeltaTime;
        }

        if (IsUnpressedGrabTrigger())
        {
            TimeConfiguration = 0f;
        }
    }

    private string ShowLogAux(Vector3 position, Quaternion rotation)
    {
        return "Position: " + position.ToString() + "\n" +
            "Rotation: " + rotation.eulerAngles.ToString() + "\n" +
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
        start = start + new Vector3(0, 0, 0.03f);
        end = end + new Vector3(0, 0, 0.03f);
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
}
