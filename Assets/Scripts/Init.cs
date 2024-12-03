using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour
{
    public GameObject Managers;
    // Start is called before the first frame update
    private void Awake()
    {
        Managers.SetActive(false);
    }

    public void ActivateManagers()
    {
        Managers.SetActive(true);
    }


}
