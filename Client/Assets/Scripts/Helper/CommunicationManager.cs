using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

public class CommunicationManager : Singleton<CommunicationManager>
{
    // message style variables
    private string endOfMsg = "!!!";
    private char varSep = ';';

    [HideInInspector]
    public Vector3 headPosition = Vector3.zero, headForward = Vector3.zero;

    Quaternion headRotation;
    Vector3 headRotationVector;

    [HideInInspector]
    public Vector3 P1O, P2O, P3O, P1H, P2H, P3H;

    public GameObject receiver;
    private BroadcastReceiver br;
    private Camera cam;

    [HideInInspector]
    public bool isServerConnected = false;

    private Vector3 jointVector, origin2Head;
    private Quaternion jointQuaternion, direction2Head;

    private Vector3 headVector, LeftElbowVector, RightElbowVector;

    private bool _P1H = false, _P2H = false, _P3H = false;

    [HideInInspector]
    public bool isReset = false, isFinished = false, isClosed = false;

    [HideInInspector]
    public int _bodyStatus;


    // Start is called before the first frame update
    void Start()
    {
        br = receiver.GetComponent<BroadcastReceiver>();
        cam = Camera.main;
    }


    // Update is called once per frame
    void Update()
    {
        headPosition = cam.transform.position;
        headForward = cam.transform.forward;
        headRotation = cam.transform.rotation;
        headRotationVector = cam.transform.eulerAngles;

    }

    public IEnumerator ProcessMessage(string s)
    {
        string[] msg = s.Split(new string[] { "endOfMsg" }, StringSplitOptions.None);
        msg[0] = msg[0].Replace(endOfMsg, "");
        msg = msg[0].Split(varSep);

        switch (msg[0])
        {
            case "Reset":
                br.SendMessageString("Reset client");
                break;
            case "SC": // Server connected
                isServerConnected = true;
                Debug.Log("server has been connected successfully");
                break;
            case "P1_O":
                P1O = ParseData(msg);
                TextHandler.Instance.TextUpdate();
                break;
            case "P2_O":
                P2O = ParseData(msg);
                TextHandler.Instance.TextUpdate();
                break;
            case "P3_O":
                P3O = ParseData(msg);
                TextHandler.Instance.TextUpdate();
                break;
            case "UB": //Update Body
                BodyDataReceiver.Instance.UpdateBodyData(s);
                break;
            case "SD": // Server disconnected
                isServerConnected = false;
                break;
            case "BS": // Body status
                ParseStatus(msg);
                break;
            case "Angle": // knee angle
                BodyDataReceiver.Instance.UpdateBodyData(s);
                break;
            default:

                break;
        }
        yield return null;
    }

    private void ParseStatus(string[] msg)
    {
        _bodyStatus = int.Parse(msg[1]);
    }

    private Vector3 ParseData(string[] msg)
    {
        //Vector3
        float vx = float.Parse(msg[1]);
        float vy = float.Parse(msg[2]);
        float vz = float.Parse(msg[3]);

        return new Vector3(vx, vy, vz);
    }

    /// <summary>
    /// Used by Capture button in HoloLens
    /// </summary>
    public void SendHeadCoordinate()
    {
        isReset = false;
        if (_P1H == false)
        {
            P1H = SendHLHeadPos(1);

            _P1H = true;
        }
        else if (_P2H == false)
        {
            P2H = SendHLHeadPos(2);
            _P2H = true;
        }
        else if (_P3H == false)
        {
            P3H = SendHLHeadPos(3);
            _P3H = true;
        }
    }

    private Vector3 SendHLHeadPos(int num)
    {
        Vector3 v = Vector3.zero;
        if (isServerConnected)
        {
            br.SendMessageString("P" + num + "_H," + headPosition.x.ToString("f6") + "," + headPosition.y.ToString("f6") + "," + headPosition.z.ToString("f6") + ","
                           + "P" + num + "_H_EA," + headRotationVector.x.ToString("f6") + "," + headRotationVector.y.ToString("f6") + "," + headRotationVector.z.ToString("f6") + ","
                           + "P" + num + "_H_QU," + headRotation.x.ToString("f6") + "," + headRotation.y.ToString("f6") + "," + headRotation.z.ToString("f6") + "," + headRotation.w.ToString("f6"));
            v = headPosition;
        }
        else
            Debug.Log("server is not connected");

        return v;
    }

    /// <summary>
    /// Used by Reset button in HoloLens
    /// </summary>
    public void ResetCalibration()
    {
        isReset = true;
        isFinished = false;
        br.SendMessageString("Reset");
        _P1H = _P2H = _P3H = false;
        P1H = P2H = P3H = Vector3.zero;
        P1O = P2O = P3O = Vector3.zero;
    }

    /// <summary>
    /// Used by Finish button in HoloLens
    /// </summary>
    public void FinishCalibration()
    {
        if (_P1H == true && _P2H == true && _P3H == true)
        {
            br.SendMessageString("Finish");
            isFinished = true;
        }
        else
            isFinished = false;

    }

    /// <summary>
    /// Used by Close button in HoloLens
    /// </summary>
    public void CloseMenu()
    {
        isClosed = true;
    }


}
