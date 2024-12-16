using System.Collections;
using BrushingAndLinking;
using UnityEngine;

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

    public static MainManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        InteractionManager.SetActive(true);
        TabletDataVis.SetActive(true);
        StudyMManager.SetActive(true);
    }
    void Start()
    {
        ColorManager.SetActive(true);
        InteractionManager.SetActive(false);
        TabletDataVis.SetActive(false);
        CalibrationManager.SetActive(true);
        StudyMManager.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartDemo()
    {
        ColorManager.SetActive(true);
        InteractionManager.SetActive(true);
        TabletDataVis.SetActive(true);
        CalibrationManager.SetActive(false);
        StudyMManager.SetActive(true);

        StartCoroutine(PlayDemo());
    }

    private IEnumerator PlayDemo()
    {
        yield return new WaitForSeconds(1f);
        StudyManager.Instance.StartDemo();
    }


}
