using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Astra;
using System;

public class BodyData
{
    private float lk_angle, rk_angle;
    private int bodyStatus;

    public float LK_angle
    {
        get { return lk_angle; }
        set { lk_angle = value; }
    }

    public float RK_angle
    {
        get { return rk_angle; }
        set { rk_angle = value; }
    }

    public int _BodyStatus
    {
        get { return bodyStatus; }
        set { bodyStatus = value; }
    }
}

public class AstraBodyView : MonoBehaviour
{

    //Dictionary of Bodies and their respective ID's
    private AstraManager _BodyManager;
    private Astra.Body[] _bodies;
    private Dictionary<Astra.JointType, GameObject> jointGOs;
    public GameObject conversion;
    private CoordinateConversion coordinate_conversion;
    private BodyData _bodyData = new BodyData();
    public Dictionary<Astra.JointType, GameObject> GetBody()
    {
        return jointGOs;
    }

    //Dictionary of Joints of the body in the Orbbec 
    private Dictionary<Astra.JointType, Astra.JointType> _BoneMap = new Dictionary<Astra.JointType, Astra.JointType>()
    {
        //{ Astra.JointType.LeftFoot, Astra.JointType.AnkleLeft },
        { Astra.JointType.LeftFoot, Astra.JointType.LeftKnee },
        { Astra.JointType.LeftKnee, Astra.JointType.LeftHip },
        { Astra.JointType.LeftHip, Astra.JointType.BaseSpine },

     //   { Astra.JointType.FootRight, Astra.JointType.AnkleRight },
        { Astra.JointType.RightFoot, Astra.JointType.RightKnee },
        { Astra.JointType.RightKnee, Astra.JointType.RightHip },
        { Astra.JointType.RightHip, Astra.JointType.BaseSpine },

    //    { Astra.JointType.HandTipLeft, Astra.JointType.HandLeft },
     //   { Astra.JointType.ThumbLeft, Astra.JointType.HandLeft },
        { Astra.JointType.LeftHand, Astra.JointType.LeftWrist },
        { Astra.JointType.LeftWrist, Astra.JointType.LeftElbow },
        { Astra.JointType.LeftElbow, Astra.JointType.LeftShoulder },
        { Astra.JointType.LeftShoulder, Astra.JointType.ShoulderSpine },

      //  { Astra.JointType.HandTipRight, Astra.JointType.HandRight },
      //  { Astra.JointType.ThumbRight, Astra.JointType.HandRight },
        { Astra.JointType.RightHand, Astra.JointType.RightWrist },
        { Astra.JointType.RightWrist, Astra.JointType.RightElbow },
        { Astra.JointType.RightElbow, Astra.JointType.RightShoulder },
        { Astra.JointType.RightShoulder, Astra.JointType.ShoulderSpine },

        { Astra.JointType.BaseSpine, Astra.JointType.MidSpine },
        { Astra.JointType.MidSpine, Astra.JointType.ShoulderSpine },
        { Astra.JointType.ShoulderSpine, Astra.JointType.Neck },
        { Astra.JointType.Neck, Astra.JointType.Head },
    };

    // Jill Zombie Avatar from Mixamo

    /* private Dictionary<string, string> _RigMap = new Dictionary<string, string>()
     {
         {"BaseSpine", "mixamorig:Hips"},
         {"RightHip", "mixamorig:Hips/mixamorig:RightUpLeg"},
         {"LeftHip", "mixamorig:Hips/mixamorig:LeftUpLeg"},
         {"RightKnee", "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg"},
         {"LeftKnee", "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg"},
         {"RightFoot", "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot"},
         {"LeftFoot", "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot"},

         {"MidSpine", "mixamorig:Hips/mixamorig:Spine"},
         {"ShoulderSpine", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2"},
         {"RightShoulder", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm"},
         {"LeftShoulder", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm"},
         {"RightElbow", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm"},
         {"LeftElbow", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm"},
         {"RightWrist", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand"},
         {"LeftWrist", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand"},

        // {"HandLeft", "Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand"},
        // {"HandRight", "Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm/LeftHand"},

         {"Neck", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck"},
         {"Head", "Hmixamorig:ips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head"},

     };


 */
    // Start is called before the first frame update
    void Start()
    {
        _BodyManager = FindObjectOfType<AstraManager>();
        StreamViewModel viewModel = StreamViewModel.Instance;
        viewModel.bodyStream.onValueChanged += OnBodyStreamChanged;
        jointGOs = new Dictionary<Astra.JointType, GameObject>();

        coordinate_conversion = conversion.GetComponent<CoordinateConversion>();

        CreateBodyObject();
    }
    private void OnBodyStreamChanged(bool value)
    {
        gameObject.SetActive(value);
    }

    // Update is called once per frame
    void Update()
    {
        //Initialize Astra
        if (!AstraManager.Instance.Initialized)
        {
            Debug.Log("sdk not initialized");
            return;
        }
        _bodies = AstraManager.Instance.Bodies;

        if (_bodies == null)
        {
            Debug.Log("no bodies found");
            return;
        }

        foreach (var body in _bodies)
        {

            if (body == null)
            {
                continue;
            }
            if (body.Status == BodyStatus.Tracking)
            {
                RefreshBodyObject(body);
                _bodyData._BodyStatus = (int)body.Status;
                if (UDPServer.Instance.isClientConnected)
                {
                    MessageHandler.Instance.SendBodyStatus(_bodyData._BodyStatus);
                }
            }

        }

    }

    #region Body
    private void CreateBodyObject()
    {
        //create a body and give it an id

        for (JointType jt = JointType.Head; jt <= JointType.Neck; jt++)
        {
            var jointGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            jointGO.name = jt.ToString();
            jointGO.transform.localScale = new Vector3(70f, 70f, 70f);
            jointGO.transform.SetParent(transform, false);
            jointGO.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

            // add line renderer
            LineRenderer lr = jointGO.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Diffuse"));
            lr.SetWidth(0.1f, 0.1f);

            jointGO.SetActive(false);
            jointGOs.Add(jt, jointGO);
        }

    }
    private void RefreshBodyObject(Astra.Body body)
    {

        for (Astra.JointType jt = Astra.JointType.Head; jt <= Astra.JointType.Neck; jt++)
        {
            //get joints
            Astra.Joint sourceJoint = body.Joints[(int)jt];
            Astra.Joint targetJoint = null;

            if (_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[(int)_BoneMap[jt]];
            }

            if (sourceJoint.Status == JointStatus.Tracked)
            {
                jointGOs[jt].transform.localPosition = GetVector3FromJoint(sourceJoint);

                // convert Astra 3x3 rotation matrix to 4x4 matrix
                Matrix4x4 m = coordinate_conversion.ConverttoMatrix4x4(sourceJoint.Orientation);

                // Multiple converted matrix with R(H<--O) : Rotation Orbbec to HoloLens
                Matrix4x4 new4Matrix = coordinate_conversion.Multiply(m);

                jointGOs[jt].transform.localRotation = ConvertMatrixToQuaternion(new4Matrix);
                jointGOs[jt].SetActive(true);

                //Set line renderer positions                                                                         
                LineRenderer lr = jointGOs[jt].GetComponent<LineRenderer>();
                if (targetJoint != null)
                {
                    var jointPos = sourceJoint.WorldPosition;
                    var startPos = transform.TransformVector(jointPos.X, jointPos.Y, jointPos.Z);
                    jointPos = targetJoint.WorldPosition;
                    var endPos = transform.TransformVector(jointPos.X, jointPos.Y, jointPos.Z);
                    lr.SetPositions(new Vector3[] { startPos, endPos });
                }
                else
                {
                    lr.enabled = false;
                }


                // Calculate knee angles
                if (sourceJoint.Type == JointType.RightKnee)
                {
                    float rk_angle = GetAngle(sourceJoint);
                    _bodyData.RK_angle = rk_angle;

                }
                if (sourceJoint.Type == JointType.LeftKnee)
                {
                    float lk_angle = GetAngle(sourceJoint);
                    _bodyData.LK_angle = lk_angle;
                }
            }
            else
            {
                jointGOs[jt].SetActive(false);
            }

        }
    }
    public float GetLeftKneeAngle()
    {
        return _bodyData.LK_angle;
    }
    public float GetRightKneeAngle()
    {
        return _bodyData.RK_angle;
    }

    public int GetBodyStatus()
    {
        return _bodyData._BodyStatus;
    }
    private float GetAngle(Astra.Joint joint)
    {
        Vector3 knee = Vector3.zero;
        Vector3 hip = Vector3.zero;
        Vector3 ankle = Vector3.zero;

        //Grab joints
        if (joint.Type == JointType.RightKnee)
        {
            hip = jointGOs[Astra.JointType.RightHip].transform.position;
            knee = jointGOs[Astra.JointType.RightKnee].transform.position;
            ankle = jointGOs[Astra.JointType.RightFoot].transform.position;
        }
        else if (joint.Type == JointType.LeftKnee)
        {
            hip = jointGOs[Astra.JointType.LeftHip].transform.position;
            knee = jointGOs[Astra.JointType.LeftKnee].transform.position;
            ankle = jointGOs[Astra.JointType.LeftFoot].transform.position;
        }

        //Get vectors
        Vector3D vector1 = new Vector3D
        {
            X = hip.x - knee.x,
            Y = hip.y - knee.y,
            Z = hip.z - knee.z
        };
        Vector3D vector2 = new Vector3D
        {
            X = ankle.x - knee.x,
            Y = ankle.y - knee.y,
            Z = ankle.z - knee.z
        };

        //Vector lengths
        float length1 = (float)Math.Sqrt((vector1.X * vector1.X) + (vector1.Y * vector1.Y) + (vector1.Z * vector1.Z));
        float length2 = (float)Math.Sqrt((vector2.X * vector2.X) + (vector2.Y * vector2.Y) + (vector2.Z * vector2.Z));

        //Dot product
        float dot = (vector1.X * vector2.X) + (vector1.Y * vector2.Y) + (vector1.Z * vector2.Z);

        //Measure angle
        float ang = (float)Math.Acos(dot / (length1 * length2)); // Radians
        ang *= 180.0f / (float)Math.PI; // Degrees  

        return ang;
    }

    private Color GetColorForState(Astra.JointStatus state)
    {
        switch (state)
        {
            case JointStatus.Tracked:
                return Color.green;

            case JointStatus.NotTracked:
                return Color.red;

            default:
                return Color.black;
        }
    }

    /* private void SetAvatarScale(GameObject bodyObject)
     {
         Transform avatar = bodyObject.transform.Find("Avatar");
         if (avatar != null)
         {
             if (avatar.localScale.x != 1)
             {
                 return;
             }

             //Scale avatar based on torso distance
             Transform hips = avatar.Find("mixamorig:Hips");
             Transform spineBase = bodyObject.transform.Find("BaseSpine");
             Transform spineShoulder = bodyObject.transform.Find("ShoulderSpine");
             float bodyScale = Vector3.Magnitude(spineShoulder.position - spineBase.position);
             Transform neck = avatar.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head");
             float avatarScale = Vector3.Magnitude(neck.position - hips.position);
             float scaleFactor = bodyScale / avatarScale;
             avatar.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
         }

     }*/

    #endregion

    #region Calculations
    private Quaternion ConvertMatrixToQuaternion(Matrix4x4 m)
    {
        float tr = m.m00 + m.m11 + m.m22;
        float qw, qx, qy, qz, S;
        if (tr > 0)
        {
            S = 0.5f / Mathf.Sqrt(tr + 1.0f); // S=4*qw 
            qw = 0.25f / S;
            qx = (m.m21 - m.m12) * S;
            qy = (m.m02 - m.m20) * S;
            qz = (m.m10 - m.m01) * S;
        }
        else if ((m.m00 > m.m11) & (m.m00 > m.m22))
        {
            S = Mathf.Sqrt(1.0f + m.m00 - m.m11 - m.m22) * 2; // S=4*qx 
            qw = (m.m21 - m.m12) / S;
            qx = 0.25f * S;
            qy = (m.m01 + m.m10) / S;
            qz = (m.m02 + m.m20) / S;
        }
        else if (m.m11 > m.m22)
        {
            S = Mathf.Sqrt(1.0f + m.m11 - m.m00 - m.m22) * 2; // S=4*qy
            qw = (m.m02 - m.m20) / S;
            qx = (m.m01 + m.m10) / S;
            qy = 0.25f * S;
            qz = (m.m12 + m.m21) / S;
        }
        else
        {
            S = Mathf.Sqrt(1.0f + m.m22 - m.m00 - m.m11) * 2; // S=4*qz
            qw = (m.m10 - m.m01) / S;
            qx = (m.m02 + m.m20) / S;
            qy = (m.m12 + m.m21) / S;
            qz = 0.25f * S;
        }

        return new Quaternion(qx, qy, qz, qw);

    }

    private Vector3 GetVector3FromJoint(Astra.Joint joint)
    {
        return new Vector3(joint.WorldPosition.X, joint.WorldPosition.Y, joint.WorldPosition.Z);
    }

    public Vector3 GetVectorFromJoint(Astra.JointType jointtype)
    {
        Vector3 v = Vector3.zero;

        for (Astra.JointType jt = JointType.Head; jt < JointType.Neck; jt++)
        {
            if (jt.ToString() == jointtype.ToString())
            {
                foreach (var body in _bodies)
                {
                    if (body != null)
                    {
                        if (body.Status == BodyStatus.Tracking)
                        {
                            Astra.Joint joint = body.Joints[(int)jt];
                            v = new Vector3(joint.WorldPosition.X, joint.WorldPosition.Y, joint.WorldPosition.Z);
                        }
                    }
                }
            }
        }
        return v;
    }


    #endregion
}
