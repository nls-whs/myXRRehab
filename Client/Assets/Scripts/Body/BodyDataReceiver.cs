using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyDataReceiver : Singleton<BodyDataReceiver>
{
    private Dictionary<ulong, string[]> _BodiesJoints = new Dictionary<ulong, string[]>();
    private Dictionary<string, Vector3> _BodiesPos = new Dictionary<string, Vector3>();
    private Dictionary<string, Quaternion> _BodiesRot = new Dictionary<string, Quaternion>();
    private Dictionary<string, float> _BodiesAngle = new Dictionary<string, float>();

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
    public Dictionary<string, Vector3> GetPosData()
    {
        return _BodiesPos;
    }

    public Dictionary<string, Quaternion> GetRotData()
    {
        return _BodiesRot;
    }
    public Dictionary<ulong, string[]> GetJointsData()
    {
        return _BodiesJoints;
    }

    public Dictionary<string, float> GetAngleData()
    {
        return _BodiesAngle;
    }


    // Called when reading in Kinect body data
    public void UpdateBodyData(string s)
    {
        // Parse the message
        Debug.Log("Getting messages");

        string[] msg = s.Split(new string[] { "endOfMsg" }, StringSplitOptions.None);
        msg[0] = msg[0].Replace("!!!", "");
        msg = msg[0].Split(';');

        if (msg[0] == "Angle")
        {
            float lk_angle = float.Parse(msg[1]);
            float rk_angle = float.Parse(msg[2]);

            _BodiesAngle["LeftKnee"] = lk_angle;
            _BodiesAngle["RightKnee"] = rk_angle;
        }
        else
        {
            _BodiesJoints.Clear();
            //Tracking ID
            ulong trackingID = (ulong)Convert.ToInt64(msg[1]);
            //Joint name
            string name = msg[2];
            //Vector3
            float vx = float.Parse(msg[3]);
            float vy = float.Parse(msg[4]);
            float vz = float.Parse(msg[5]);

            //Quaternion
            float qx = float.Parse(msg[6]);
            float qy = float.Parse(msg[7]);
            float qz = float.Parse(msg[8]);
            float qw = float.Parse(msg[9]);

            string[] names = new string[19];
            for (JointType jt = JointType.LeftHip; jt <= JointType.RightFoot; jt++)
            {
                names[(int)jt] = jt.ToString();
            }
            _BodiesJoints[trackingID] = names;
            _BodiesPos[name] = new Vector3(vx, -vy + 0.11f, vz);
            _BodiesRot[name] = new Quaternion(qx, qy, qz, qw);
        }
    }
}
