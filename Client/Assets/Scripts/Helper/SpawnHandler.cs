using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
    public GameObject footPrint_Prefab;
    private GameObject foot;

    

    // Start is called before the first frame update
    void Start()
    {
              
    }

    // Update is called once per frame
    void Update()
    {
        if (CommunicationManager.Instance.isFinished)
        {
            if (foot == null )
            {
                InstantiatePrefab();
                foot.transform.position = new Vector3(-0.1f, -0.6f, 1f);
            }
            
        }
        if (CommunicationManager.Instance.isReset)
        {
            Destroy(foot);
        }
        /*if (foot != null)
        {
            foot.transform.position = Camera.main.transform.forward;
        }*/
        
    }

    private void InstantiatePrefab()
    {
        foot = Instantiate(footPrint_Prefab, transform);
    }
}
