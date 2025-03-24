using BrushingAndLinking;
using Oculus.Interaction.Input;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [Header("Left Hand Variables")]
    public Transform LeftHandAnchor;
    public Vector3 LeftHandTranslationOffset;
    public Vector3 LeftHandRotationOffset;


    public ButtonGroup CalibrationButtons;

    private bool visibility = false;

    private OneEuroFilter<Vector3> positionFilter;
    private OneEuroFilter<Quaternion> rotationFilter;
    private readonly float filterFrequency = 120f;

    private void Start()
    {
        positionFilter = new OneEuroFilter<Vector3>(filterFrequency, 0.9f, 10f);
        rotationFilter = new OneEuroFilter<Quaternion>(filterFrequency, 0.9f, 10f);
    }

    public void LateUpdate()
    {
        if (!visibility)
            return;

        transform.SetParent(LeftHandAnchor);
        transform.localPosition = LeftHandTranslationOffset;
        transform.localEulerAngles = LeftHandRotationOffset;
        transform.parent = null;


        // Filter new position and rotation values
        Vector3 unfilteredPosition = transform.position;
        Quaternion unfilteredRotation = transform.rotation;

        Vector3 filteredPosition = positionFilter.Filter(unfilteredPosition);
        Quaternion filteredRotation = rotationFilter.Filter(unfilteredRotation);

        transform.position = filteredPosition;
        transform.rotation = filteredRotation;

    }

    public void SetOverallVisibility(bool visibility)
    {
        this.visibility = visibility;

        if (!visibility)
            transform.position = new Vector3(0, -10, 0);
        else
            CalibrationButtons.UnhighlightButtons();

    }
}
