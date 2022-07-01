using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System;

public class MessageHandler : Singleton<MessageHandler>
{
    public GameObject text;
    public GameObject bodyView;
    public GameObject Coordinate_conversion;

    private Text logText;
    private AstraBodyView bv;
    private CoordinateConversion conv;

    private string endOfMsg = "!!!";
    private char varSep = ';';

    // Start is called before the first frame update
    void Start()
    {
        logText = text.GetComponent<Text>();
        bv = bodyView.GetComponent<AstraBodyView>();
        conv = Coordinate_conversion.GetComponent<CoordinateConversion>();
    }


    public void SetLogText(string s)
    {
        logText.text = s;
    }

    public string buildMessage(string s)
    {
        string command = s;
        StringBuilder sb = new StringBuilder(command);
        switch (command)
        {
            case "P1_O":
                sb.Append(varSep);
                sb.Append(bv.GetVectorFromJoint(Astra.JointType.Head).x.ToString("f6"));
                sb.Append(varSep);
                sb.Append(bv.GetVectorFromJoint(Astra.JointType.Head).y.ToString("f6"));
                sb.Append(varSep);
                sb.Append(bv.GetVectorFromJoint(Astra.JointType.Head).z.ToString("f6"));
                sb.Append(endOfMsg);
                break;
            case "P2_O":
                sb.Append(varSep);
                sb.Append(bv.GetVectorFromJoint(Astra.JointType.Head).x.ToString("f6"));
                sb.Append(varSep);
                sb.Append(bv.GetVectorFromJoint(Astra.JointType.Head).y.ToString("f6"));
                sb.Append(varSep);
                sb.Append(bv.GetVectorFromJoint(Astra.JointType.Head).z.ToString("f6"));
                sb.Append(endOfMsg);
                break;
            case "P3_O":
                sb.Append(varSep);
                sb.Append(bv.GetVectorFromJoint(Astra.JointType.Head).x.ToString("f6"));
                sb.Append(varSep);
                sb.Append(bv.GetVectorFromJoint(Astra.JointType.Head).y.ToString("f6"));
                sb.Append(varSep);
                sb.Append(bv.GetVectorFromJoint(Astra.JointType.Head).z.ToString("f6"));
                sb.Append(endOfMsg);
                break;

            default:
                break;
        }


        return sb.ToString();
    }

    public Vector3 parseMessage(string msg, int i)
    {
        string[] array = msg.Split(',');
        Vector3 vec = Vector3.zero;
        if (i == 1)
        {
            float x = float.Parse(array[1]);
            float y = float.Parse(array[2]);
            float z = float.Parse(array[3]);
            vec = new Vector3(x, y, z);
        }
        else if (i == 2)
        {
            float x = float.Parse(array[5]);
            float y = float.Parse(array[6]);
            float z = float.Parse(array[7]);
            vec = new Vector3(x, y, z);
        }

        return vec;
    }

    public Quaternion parseMessageQU(string msg)
    {
        string[] array = msg.Split(',');

        float x = float.Parse(array[9]);
        float y = float.Parse(array[10]);
        float z = float.Parse(array[11]);
        float w = float.Parse(array[12]);

        return new Quaternion(x, y, z, w); ;
    }

    public void SendBodyAngle()
    {
        StringBuilder sb = new StringBuilder("Angle;");
        sb.Append(bv.GetLeftKneeAngle() + ";");
        sb.Append(bv.GetRightKneeAngle() + "!!!");
        if (UDPServer.Instance.isClientConnected && Calibration.instance._isCalibrationCompleted == true)
        {
            UDPServer.Instance.SendMessage(sb.ToString());
        }
        else
        {
            Debug.Log("client not connected");
        }
        sb.Clear();

    }

    public void SendBodyStatus(int i)
    {
        StringBuilder sb = new StringBuilder("BS;"); // Body Status
        sb.Append(i + "!!!");
        if (UDPServer.Instance.isClientConnected)
        {
            UDPServer.Instance.SendMessage(sb.ToString());
        }
        else
        {
            Debug.Log("client not connected");
        }
        sb.Clear();

    }

    public void SendBodyData(ulong trackingID, Dictionary<Astra.JointType, GameObject> bodyData)
    {
       
        StringBuilder sb = new StringBuilder();

        for (Astra.JointType jt = Astra.JointType.LeftHip; jt <= Astra.JointType.RightFoot; jt++)
        {
            sb.Append("UB;" + trackingID + ";");
            Transform bodyItem = bodyData[jt].transform;
            sb.Append(jt.ToString() + ";");
            AppendTransform(sb, bodyItem);
            if (UDPServer.Instance.isClientConnected && Calibration.instance._isCalibrationCompleted == true)
            {
                UDPServer.Instance.SendMessage(sb.ToString());
            }
            else
            {
                Debug.Log("client not connected");
            }
            sb.Clear();
        }
    }

    private void AppendTransform(StringBuilder sb, Transform transform)
    {
        if (!float.IsNaN(transform.position.x) && !float.IsNaN(transform.position.y) && !float.IsNaN(transform.position.z))
        {
            //changing back from m to mm for complete coordinate transformation
            AppendVector3(sb, conv.Multiply(transform.localPosition));
            AppendQuaternion(sb, transform.localRotation);
        }
        else
        {
            Debug.Log("invalid position value");
        }

    }
    void AppendVector3(StringBuilder sb, Vector3 vector)
    {
        if (!float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z))
        {
            sb.Append(vector.x + ";");
            sb.Append(vector.y + ";");
            sb.Append(vector.z + ";");
        }
        else
        {
            Debug.Log("invalid position value");
        }
    }

    void AppendQuaternion(StringBuilder sb, Quaternion rotation)
    {
        sb.Append(rotation.x + ";");
        sb.Append(rotation.y + ";");
        sb.Append(rotation.z + ";");
        sb.Append(rotation.w + "!!!");
    }
}
