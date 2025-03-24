using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class DisableMetaPopup : MonoBehaviour
{
    private const string TelemetryEnabledKey = "OVRTelemetry.TelemetryEnabled";

    [ContextMenu("DisablePopup")]
    public void DisablePopup()
    {
        EditorPrefs.SetBool(TelemetryEnabledKey, false);
    }

    private void Awake()
    {
        DisablePopup();
    }
}
#endif
