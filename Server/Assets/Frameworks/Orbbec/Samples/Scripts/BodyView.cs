using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//using UnityEditor.Experimental.GraphView;
using Astra;
using UnityEditor;
using System.Runtime.CompilerServices;

public class BodyView : MonoBehaviour
{
    public int bodyIndex;
    
    public bool user = false;


    private Dictionary<Astra.JointType, GameObject> jointGOs;
    private LineRenderer[] jointLines;

    [HideInInspector]
    public float rk_angle;
    [HideInInspector]
    public float lk_angle;
    [HideInInspector]
    public Vector3 headPosition = Vector3.zero, leftHandpos = Vector3.zero, leftKneePos = Vector3.zero, leftHip = Vector3.zero, leftAnkle = Vector3.zero,
        pelvis = Vector3.zero;

    [HideInInspector]
    public Quaternion head_QR, lknee_QR, lhip_QR, lfoot_QR;
    

    // Use this for initialization
    void Start()
    {
        StreamViewModel viewModel = StreamViewModel.Instance;
        viewModel.bodyStream.onValueChanged += OnBodyStreamChanged;

        jointGOs = new Dictionary<Astra.JointType, GameObject>();
        for (int i = 0; i < 19; ++i)
        {
            var jointGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            jointGO.name = ((Astra.JointType)i).ToString();
            jointGO.transform.localScale = new Vector3(100f, 100f, 100f);
            jointGO.transform.SetParent(transform, false);
            jointGO.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
            jointGO.SetActive(false);
            jointGOs.Add((Astra.JointType)i, jointGO);
        }

        jointLines = new LineRenderer[15];
        for (int i = 0; i < jointLines.Length; ++i)
        {
            var jointLineGO = new GameObject("Line");
            jointLineGO.transform.SetParent(transform, false);
            var jointLine = jointLineGO.AddComponent<LineRenderer>();
            jointLine.material = new Material(Shader.Find("Diffuse"));
            jointLine.SetWidth(0.1f, 0.1f);
            jointLineGO.SetActive(false);
            jointLines[i] = jointLine;
        }
    }

    private void OnBodyStreamChanged(bool value)
    {
        gameObject.SetActive(value);
    }

    // Update is called once per frame
    void Update()
    {
        if (!AstraManager.Instance.Initialized || !AstraManager.Instance.IsBodyOn)
        {
            return;
        }
        var bodies = AstraManager.Instance.Bodies;
        if (bodies == null)
        {
            user = false;
            return;
        }
        int i = 0;
        foreach (var body in bodies)
        {
            if(body != null) i++;
            if(i != bodyIndex) continue;
            var joints = body.Joints;
           
            if (joints != null)
            {
                foreach (var joint in joints)
                {
                    if (joint.Status == Astra.JointStatus.Tracked)
                    {
                        user = true;
                        var jointPos = joint.WorldPosition;
                        var jointOr = joint.Orientation;
                        jointGOs[joint.Type].transform.localPosition = new Vector3(jointPos.X, jointPos.Y, jointPos.Z);
                        jointGOs[joint.Type].SetActive(true);

                        if (joint.Type == Astra.JointType.LeftHand)
                        {
                            if (body.HandPoseInfo.LeftHand == Astra.HandPose.Grip)
                            {
                                jointGOs[joint.Type].transform.localScale = new Vector3(100f, 100f, 100f) * 0.7f;
                                jointGOs[joint.Type].GetComponent<MeshRenderer>().material.color = Color.blue;
                            }
                            else
                            {
                                jointGOs[joint.Type].transform.localScale = new Vector3(100f, 100f, 100f);
                                jointGOs[joint.Type].GetComponent<MeshRenderer>().material.color = Color.white;                                
                            }                          
                        }
                        if (joint.Type == Astra.JointType.RightHand)
                        {
                            if (body.HandPoseInfo.RightHand == Astra.HandPose.Grip)
                            {
                                jointGOs[joint.Type].transform.localScale = new Vector3(100f, 100f, 100f) * 0.7f;
                                jointGOs[joint.Type].GetComponent<MeshRenderer>().material.color = Color.blue;
                            }
                            else
                            {
                                jointGOs[joint.Type].transform.localScale = new Vector3(100f, 100f, 100f);
                                jointGOs[joint.Type].GetComponent<MeshRenderer>().material.color = Color.white;
                            }
                          
                        }
                   
                    }
                    else
                    {
                        jointGOs[joint.Type].SetActive(false);
                    }
                }

                DrawLine(joints[0], joints[1], 0);
                DrawLine(joints[1], joints[2], 1);
                DrawLine(joints[1], joints[5], 2);
                DrawLine(joints[2], joints[3], 3);
                DrawLine(joints[3], joints[4], 4);
                DrawLine(joints[5], joints[6], 5);
                DrawLine(joints[6], joints[7], 6);
                DrawLine(joints[1], joints[8], 7);
                DrawLine(joints[8], joints[9], 8);
                DrawLine(joints[9], joints[10], 9);
                DrawLine(joints[9], joints[13], 10);
                DrawLine(joints[10], joints[11], 11);
                DrawLine(joints[11], joints[12], 12);
                DrawLine(joints[13], joints[14], 13);
                DrawLine(joints[14], joints[15], 14);

                break;
            }
        }
    }

    //round off
    private float roundoff(float articulacion)
    {
        return Mathf.Round(articulacion * Mathf.Pow(10, 2)) / 100;
    }
    private Vector3 GetVector3FromJoint(Astra.Joint joint)
    {
        return new Vector3(joint.WorldPosition.X /1000f, joint.WorldPosition.Y /1000f, joint.WorldPosition.Z /1000f);
    }

    private Quaternion ConvertMatrixToQuaternion(Matrix3x3 m)
    {
        //float qw = Mathf.Sqrt(1.0f + m.M00 + m.M11 + m.M22) / 2.0f;
        //float w4 = 4.0f * qw;
        //float qx = (m.M21 - m.M12) / w4;
        //float qy = (m.M02 - m.M20) / w4;
        //float qz = (m.M10 - m.M01) / w4;

        float tr = m.M00 + m.M11 + m.M22;
        float qw, qx, qy, qz, S;
        if (tr > 0)
        {
            S = 0.5f / Mathf.Sqrt(tr + 1.0f); // S=4*qw 
            qw = 0.25f / S;
            qx = (m.M21 - m.M12) * S;
            qy = (m.M02 - m.M20) * S;
            qz = (m.M10 - m.M01) * S;
        }
        else if ((m.M00 > m.M11) & (m.M00 > m.M22))
        {
            S = Mathf.Sqrt(1.0f + m.M00 - m.M11 - m.M22) * 2; // S=4*qx 
            qw = (m.M21 - m.M12) / S;
            qx = 0.25f * S;
            qy = (m.M01 + m.M10) / S;
            qz = (m.M02 + m.M20) / S;
        }
        else if (m.M11 > m.M22)
        {
            S = Mathf.Sqrt(1.0f + m.M11 - m.M00 - m.M22) * 2; // S=4*qy
            qw = (m.M02 - m.M20) / S;
            qx = (m.M01 + m.M10) / S;
            qy = 0.25f * S;
            qz = (m.M12 + m.M21) / S;
        }
        else
        {
            S = Mathf.Sqrt(1.0f + m.M22 - m.M00 - m.M11) * 2; // S=4*qz
            qw = (m.M10 - m.M01) / S;
            qx = (m.M02 + m.M20) / S;
            qy = (m.M12 + m.M21) / S;
            qz = 0.25f * S;
        }

        return new Quaternion(qx,qy,qz,qw);

    }

    private void SetJointOrientation(Quaternion orientation, string ind)
    {
        switch (ind)
        {
            case "head_QR":
                head_QR = orientation;
                break;
            case "lknee_QR":
                lknee_QR = orientation;
                break;
            case "lhip_QR":
                lhip_QR = orientation;
                break;
            case "lfoot_QR":
                lfoot_QR = orientation;
                break;
            default:
                break;
        }
    }

    public Quaternion GetJointOrientation(string ind)
    {
        
        switch (ind)
        {
            case "head_QR":
                return head_QR;
            case "lknee_QR":
                return lknee_QR;
            case "lfoot_QR":
                return lfoot_QR;
            case "lhip_QR":
                return lhip_QR;
            default:
                return Quaternion.identity;
        }
    }

    private void SetJointPositionVector(string ind, Vector3 v)
    {
        switch (ind)
        {
            case "LeftHand":
                leftHandpos = new Vector3(v.x, v.y, v.z);
                break;
            case "LeftKnee":
                leftKneePos = new Vector3(v.x, v.y, v.z);
                break;
            case "Head":
                headPosition = new Vector3(v.x, v.y, v.z);
                break;
            case "LeftHip":
                leftHip = new Vector3(v.x, v.y, v.z);
                break;
            case "LeftAnkle":
                leftAnkle = new Vector3(v.x, v.y, v.z);
                break;
            case "Pelvis":
                pelvis = new Vector3(v.x, v.y, v.z);
                break;
            default:
                break;
        }
    }

    public Vector3 GetJointPositionVector(string ind)
    {
        switch (ind)
        {
            case "LeftHand":
                return leftHandpos;
            case "LeftKnee":
                return leftKneePos;
            case "Head":
                return headPosition;
            case "LeftHip":
                return leftHip;
            case "LeftAnkle":
                return leftAnkle;
            case "Pelvis":
                return pelvis;
            default:
                return Vector3.zero;
        }
    }

    private void SetLeftHandPosition(Vector3 v)
    {
        leftHandpos = new Vector3(v.x, v.y, v.z);
    }

    public Vector3 GetLeftHandPosition()
    {
        return leftHandpos;
    }

    private void SetLeftKneePosition(Vector3 v)
    {
        leftKneePos = new Vector3(v.x, v.y, v.z);
    }

    public Vector3 GetLeftKneePosition()
    {
        return leftKneePos;
    }

    /// <summary>
    /// By Asma. Convert values from mm to m
    /// </summary>
    /// <param name="x"> joint world position x value</param>
    /// <param name="y"> joint world position y value</param>
    /// <param name="z"> joint world position z value</param>
    /// <returns></returns>
    private Vector3 ConvertValuesToM(float x, float y, float z)
    {
         x = x / 1000f;
         y = y / 1000f;
         z = z / 1000f;

        return new Vector3(x, y, z);
    }

    public float GetRightKneeAngle()
    {
        return rk_angle;
    }

    private void SetRightKneeAngle(float a)
    {
        rk_angle = a;
    }

    public float GetLeftKneeAngle()
    {
        return lk_angle;
    }

    private void SetLeftKneeAngle(float a)
    {
        lk_angle = a;
    }

    private void SetHeadPosition(Vector3 v)
    {
        headPosition = new Vector3(v.x, v.y, v.z);
    }

    public Vector3 GetHeadPosition()
    {
        return headPosition;
    }

    private void DrawLine(Astra.Joint startJoint, Astra.Joint endJoint, int index)
    {
        if (startJoint.Status == Astra.JointStatus.Tracked && endJoint.Status == Astra.JointStatus.Tracked)
        {
            var jointPos = startJoint.WorldPosition ;
            var startPos = transform.TransformVector(jointPos.X , jointPos.Y , jointPos.Z );           
            jointPos = endJoint.WorldPosition;
            var endPos = transform.TransformVector(jointPos.X , jointPos.Y , jointPos.Z );
            jointLines[index].SetPositions(new Vector3[] { startPos, endPos });
            jointLines[index].gameObject.SetActive(true);
        }
        else
        {
            jointLines[index].gameObject.SetActive(false);
        }
    }

    public  float map(float x, float x1, float x2, float y1, float y2)
    {
        var m = (y2 - y1) / (x2 - x1);
        var c = y1 - m * x1; // point of interest: c is also equal to y2 - m * x2, though float math might lead to slightly different results.

        return m * x + c;
    }


}
