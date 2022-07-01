using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyView : MonoBehaviour
{
    public GameObject bodyReceiver;
    private BodyDataReceiver _bodyReceiver;
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    Dictionary<ulong, string[]> bodies_joints = null;
    Dictionary<string, Vector3> bodies_pos = null;
    Dictionary<string, Quaternion> bodies_rot = null;
    Dictionary<string, float> bodies_angle = null;

    public GameObject prefab;
    public Material sphereMaterial;
    private bool _isBodyCreated = false;

    [HideInInspector]
    public bool StartPositionReached = false;
    [HideInInspector]
    public bool EndPositionReached = false;

    private GameObject _body;
    private enum JointType
    {

        Head = 0,
        ShoulderSpine = 1,
        LeftShoulder = 2,
        LeftElbow = 3,
        LeftHand = 4,
        RightShoulder = 5,
        RightElbow = 6,
        RightHand = 7,
        MidSpine = 8,
        BaseSpine = 9,
        LeftHip = 10,
        LeftKnee = 11,
        LeftFoot = 12,
        RightHip = 13,
        RightKnee = 14,
        RightFoot = 15,
        LeftWrist = 16,
        RightWrist = 17,
        Neck = 18,
        Unknown = 255

    }

    // Start is called before the first frame update
    void Start()
    {
        _bodyReceiver = bodyReceiver.GetComponent<BodyDataReceiver>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!_isBodyCreated)
        {
            _body = CreateBodyObject();
            _isBodyCreated = true;
        }
        if (_bodyReceiver != null)
        {
            bodies_pos = _bodyReceiver.GetPosData();
            bodies_rot = _bodyReceiver.GetRotData();
            bodies_joints = _bodyReceiver.GetJointsData();
            bodies_angle = _bodyReceiver.GetAngleData();
        }

        foreach (ulong id in bodies_joints.Keys)
        {
            if (CommunicationManager.Instance.isServerConnected == true)
            {
                RefreshBodyObject(_body, bodies_pos, bodies_rot);
                RefreshBodyAngle(_body, bodies_angle);
            }
        }
    }

    private void RefreshBodyAngle(GameObject body, Dictionary<string, float> bd_angle)
    {
        int startCount = 0, endCount = 0;

        if (body != null)
        {
            for (JointType jt = JointType.LeftHip; jt <= JointType.RightFoot; jt++)
            {
                Transform t = body.transform.Find(jt.ToString());
                Material m = t.GetComponent<MeshRenderer>().material;
                Material original = m;
                if (bd_angle.ContainsKey(jt.ToString()))
                {
                    if (Mathf.Clamp(bd_angle[jt.ToString()], 85, 95) == bd_angle[jt.ToString()])
                    {
                        m.SetColor("_BaseColor", Color.green);
                        startCount++;
                    }
                    else if (Mathf.Clamp(bd_angle[jt.ToString()], 164, 177) == bd_angle[jt.ToString()])
                    {
                        m.SetColor("_BaseColor", Color.green);
                        endCount++;
                    }

                    else if (Mathf.Clamp(bd_angle[jt.ToString()], 0, 84) == bd_angle[jt.ToString()])
                    {
                        m.SetColor("_BaseColor", Color.red); // dark red                                                                               
                    }
                    else if (Mathf.Clamp(bd_angle[jt.ToString()], 178, 360) == bd_angle[jt.ToString()])
                    {
                        m.SetColor("_BaseColor", Color.red);
                    }
                    else
                    {
                        m.SetColor("_BaseColor", sphereMaterial.GetColor("_BaseColor"));
                    }
                }
                if (startCount == 2)
                {
                    StartPositionReached = true;
                    startCount = 0;
                }
                if (endCount == 2)
                {
                    EndPositionReached = true;
                    endCount = 0;
                }

            }
        }
    }

    private GameObject CreateBodyObject()
    {
        GameObject body = Instantiate(prefab, transform);
        return body;
    }

    private void RefreshBodyObject(GameObject bodyObject, Dictionary<string, Vector3> bodies_pos, Dictionary<string, Quaternion> bodies_rot)
    {
        if (bodyObject != null)
        {
            for (JointType jt = JointType.LeftHip; jt <= JointType.RightFoot; jt++)
            {
                Transform t = bodyObject.transform.Find(jt.ToString());
                if (t != null)
                {

                    t.localPosition = Vector3.Lerp(t.localPosition, bodies_pos[jt.ToString()], Time.deltaTime * 5f);
                    t.localRotation = Quaternion.Lerp(t.localRotation, bodies_rot[jt.ToString()], Time.deltaTime * 5f);
                    t.gameObject.SetActive(true);
                }
            }
        }
    }

    /* private Matrix4x4 Calibrate()
     {
         var headPos = CommunicationManager.Instance.ReadVector3("Head");
         var rarmPos = CommunicationManager.Instance.ReadVector3("RightElbow");
         var larmPos = CommunicationManager.Instance.ReadVector3("LeftElbow");

         var cameraOffset = -0.15f * Camera.main.transform.forward;
         var cameraPos = Camera.main.transform.position + cameraOffset;
         var cameraRotation = Camera.main.transform.rotation;
         var cameraRotationY = Quaternion.AngleAxis(cameraRotation.eulerAngles.y, Vector3.up);

         var lToR = rarmPos - larmPos;
         var angle = -Mathf.Atan2(lToR.z, lToR.x) * Mathf.Rad2Deg;
         var headRotation = Quaternion.AngleAxis(angle, Vector3.up);

         var kinect2Head = Matrix4x4.TRS(headPos, headRotation, Vector3.one);
         var origin2Head = Matrix4x4.TRS(cameraPos, cameraRotationY, Vector3.one);

         var origin2Kinect = origin2Head * kinect2Head.inverse;
         Vector3 position = origin2Kinect.GetColumn(3);
         Quaternion rotation = Quaternion.LookRotation(
             origin2Kinect.GetColumn(2),
             origin2Kinect.GetColumn(1)
         );
         //bodyObject.transform.position = position;
         // bodyObject.transform.rotation = rotation;

         return origin2Kinect;
     }*/
}
