using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public GameObject UserRig;
    public float speed = 2f;

    void Update()
    {
        var input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        UserRig.transform.Translate(new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z) * speed * input.y * Time.deltaTime, Space.World);
        UserRig.transform.Translate(new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z) * speed * input.x * Time.deltaTime, Space.World);
    }
}
