using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHandler : MonoBehaviour
{
    public GameObject foot;

    public GameObject marker1, marker2, marker3, marker4;

    private Vector3 FindMiddleofMarkers()
    {
        Vector3 middle = (marker1.transform.position + marker2.transform.position + marker3.transform.position + marker4.transform.position) / 4;
        return middle;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 m = FindMiddleofMarkers();
        foot.transform.position = new Vector3(m.x, m.y, marker1.transform.position.z);
    }
}
