using UnityEngine;

public class ViewTimeController : MonoBehaviour
{
    private Camera Cam;
    public bool IsViewing = false;
    public Transform ObjectTarget;
    public float timeViewing = 0f;
    private bool StopController = true;


    void Start()
    {
        Cam = Camera.main;
        IsViewing = false;
        StopController = true;
    }

    public void Restart()
    {
        IsViewing = false;
        timeViewing = 0f;
        StopController = false;
    }

    public void Stop()
    {
        StopController = true;
    }


    void Update()
    {
        if (StopController)
            return;

        Vector3 viewPos = Cam.WorldToViewportPoint(ObjectTarget.position);

        IsViewing = (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0);

        if (IsViewing)
            timeViewing += Time.deltaTime; 
    }
}
