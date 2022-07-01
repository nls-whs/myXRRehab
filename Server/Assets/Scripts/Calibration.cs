using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Calibration : MonoBehaviour
{
    public static Calibration instance;
    // Start is called before the first frame update
    public Text P1_O;
    public Text P2_O;
    public Text P3_O;
    public Text P1_H;
    public Text P2_H;
    public Text P3_H;

    public GameObject messageHandler;
    public GameObject server;
    //public GameObject BodyView;
    
    private UDPServer us;
    private MessageHandler mh;
   // private BodyView bv;
    
    private string message = null;

    public Vector3 P1O, P2O, P3O, P1H, P2H, P3H = Vector3.zero;

    private Vector3 P1H_EA, P2H_EA, P3H_EA;
    private Quaternion P1H_QU, P2H_QU, P3H_QU;

    public bool _isCalibrationCompleted = false;
    public bool _isFinishClicked = false;
    public bool _isReset = false;

    private bool _P1O, _P2O, _P3O, _P1H, _P2H, _P3H = false;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        us = server.GetComponent<UDPServer>();
        mh = messageHandler.GetComponent<MessageHandler>();
       // bv = BodyView.GetComponent<BodyView>();
    }

    void Update()
    {
        message = us.GetMessageFromClient();
        string printMessage = "";
        if (message != null)
        {
            if (message.Equals("Finish"))
            {
                FinishCalibration();
            }
            else if (message.Equals("Reset"))
            {
                ResetCalibration();
            }
            else
            {
                if (message.StartsWith("P1_H") && _P1H == false)
                {
                    Debug.Log("P1_H entered");
                    P1_H.text = "";

                    if (message.Split(',')[0].Equals("P1_H"))
                    {
                        P1H = mh.parseMessage(message, 1);
                        printMessage = "P1H position: " + P1H.ToString("f6") + "\n";
                    }
                    if (message.Split(',')[4].Equals("P1_H_EA"))
                    {
                        P1H_EA = mh.parseMessage(message, 2);
                        printMessage += "P1H Euler Angles: " + P1H_EA.ToString("f6") + "\n";
                    }
                    if (message.Split(',')[8].Equals("P1_H_QU"))
                    {
                        P1H_QU = mh.parseMessageQU(message);
                        printMessage += "P1H Quaternion: " + P1H_QU.ToString("f6") + "\n";
                    }

                    Debug.Log(printMessage);
                    P1_H.text = "P1(H): " + P1H.ToString("f6");
                    _P1H = true;
                    CaptureOB_HeadPos(1);

                }
                else if (message.StartsWith("P2_H") && _P2H == false)
                {
                    Debug.Log("P2_H entered");
                    P2_H.text = "";

                    if (message.Split(',')[0].Equals("P2_H"))
                    {
                        P2H = mh.parseMessage(message, 1);
                        printMessage = "P2H position: " + P2H.ToString("f6") + "\n";
                    }
                    if (message.Split(',')[4].Equals("P2_H_EA"))
                    {
                        P2H_EA = mh.parseMessage(message, 2);
                        printMessage += "P2H Euler Angles: " + P2H_EA.ToString("f6") + "\n";
                    }
                    if (message.Split(',')[8].Equals("P2_H_QU"))
                    {
                        P2H_QU = mh.parseMessageQU(message);
                        printMessage += "P2H Quaternion: " + P2H_QU.ToString("f6") + "\n";
                    }

                    Debug.Log(printMessage);
                    P2_H.text = "P2(H): " + P2H.ToString("f6");
                    _P2H = true;
                    CaptureOB_HeadPos(2);
                }
                else if (message.StartsWith("P3_H") && _P3H == false)
                {
                    Debug.Log("P3_H entered");
                    P3_H.text = "";

                    if (message.Split(',')[0].Equals("P3_H"))
                    {
                        P3H = mh.parseMessage(message, 1);
                        printMessage = "P3H position: " + P3H.ToString("f6") + "\n";
                    }
                    if (message.Split(',')[4].Equals("P3_H_EA"))
                    {
                        P3H_EA = mh.parseMessage(message, 2);
                        printMessage += "P3H Euler Angles: " + P3H_EA.ToString("f6") + "\n";
                    }
                    if (message.Split(',')[8].Equals("P3_H_QU"))
                    {
                        P3H_QU = mh.parseMessageQU(message);
                        printMessage += "P3H Quaternion: " + P3H_QU.ToString("f6") + "\n";
                    }

                    Debug.Log(printMessage);
                    P3_H.text = "P3(H): " + P3H.ToString("f6");
                    _P3H = true;
                    CaptureOB_HeadPos(3);
                }
            }
        }
        else
        {
            Debug.Log("client is not connected / No message received from client");
        }
    }

    /// <summary>
    /// Captures Head position from Orbbec. Function is called on button click "Take point"
    /// </summary>
    public void CaptureOB_HeadPos()
    {
        if (us.isClientConnected)
        {
            _isReset = false;
            if (mh.buildMessage("P1_O") != "P1_O;0;0;0!!!" && _P1O == false)
            {
                Debug.Log("P1_O entered");
                P1_O.text = "";
                us.SendMessage(mh.buildMessage("P1_O"));
                string newString = mh.buildMessage("P1_O");
                P1O = parseMessage(newString);
                if (P1O != null)
                {
                    P1_O.text += "P1(O): " + P1O.ToString("f6");
                    _P1O = true;
                }
                else
                    Debug.Log("p1o is null");
            }
            else if (mh.buildMessage("P2_O") != "P2_O;0;0;0!!!" && _P2O == false)
            {
                Debug.Log("P2_O entered");
                P2_O.text = "";
                us.SendMessage(mh.buildMessage("P2_O"));
                string newString = mh.buildMessage("P2_O");
                P2O = parseMessage(newString);
                P2_O.text += "P2(O): " + P2O.ToString("f6");
                _P2O = true;
            }
            else if (mh.buildMessage("P3_O") != "P3_O;0;0;0!!!" && _P3O == false)
            {
                Debug.Log("P3_O entered");
                P3_O.text = "";
                us.SendMessage(mh.buildMessage("P3_O"));
                string newString = mh.buildMessage("P3_O");
                P3O = parseMessage(newString);
                P3_O.text += "P3(O): " + P3O.ToString("f6");
                _P3O = true;
            }
        }
        else
        {
            Debug.Log("Client not connected");
        }
    }

    public void CaptureOB_HeadPos(int i)
    {
        if (us.isClientConnected)
        {
            _isReset = false;
            if (i == 1)
            {
                if (mh.buildMessage("P1_O") != "P1_O;0;0;0!!!" && _P1O == false)
                {
                    Debug.Log("P1_O entered");
                    P1_O.text = "";
                    string newString = mh.buildMessage("P1_O");
                    us.SendMessage(newString);                    
                    P1O = parseMessage(newString);
                    if (P1O != null)
                    {
                        P1_O.text += "P1(O): " + P1O.ToString("f6");
                        _P1O = true;
                    }
                    else
                        Debug.Log("p1o is null");
                }
            }
            else if (i == 2)
            {
                if (mh.buildMessage("P2_O") != "P2_O;0;0;0!!!" && _P2O == false)
                {
                    Debug.Log("P2_O entered");
                    P2_O.text = "";
                    us.SendMessage(mh.buildMessage("P2_O"));
                    string newString = mh.buildMessage("P2_O");
                    P2O = parseMessage(newString);
                    P2_O.text += "P2(O): " + P2O.ToString("f6");
                    _P2O = true;
                }
            }
            else if (i == 3)
            {
                if (mh.buildMessage("P3_O") != "P3_O;0;0;0!!!" && _P3O == false)
                {
                    Debug.Log("P3_O entered");
                    P3_O.text = "";
                    us.SendMessage(mh.buildMessage("P3_O"));
                    string newString = mh.buildMessage("P3_O");
                    P3O = parseMessage(newString);
                    P3_O.text += "P3(O): " + P3O.ToString("f6");
                    _P3O = true;
                }
            }
        }
        else
        {
            Debug.Log("Client not connected");
        }
    }

    private Vector3 parseMessage(string message)
    {
        string[] msg = message.Split(';');
        msg[3] = msg[3].Replace("!!!", "");

        float x = float.Parse(msg[1]);
        float y = float.Parse(msg[2]);
        float z = float.Parse(msg[3]);

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Calibration is completed. Function is called on button click "Finish"
    /// </summary>
    public void FinishCalibration()
    {
        _isFinishClicked = true;
    }

    /// <summary>
    /// Calibration is reset. Function is called on button click "Reset"
    /// </summary>
    public void ResetCalibration()
    {
        _isReset = true;
        //  us.SendMessage("Reset");
        P1O = P2O = P3O = P1H = P2H = P3H = Vector3.zero;
        _isCalibrationCompleted = false; _isFinishClicked = false;
        _P1O = _P2O = _P3O = _P1H = _P2H = _P3H = false;
        P1_O.text = "P1(O): "; P2_O.text = "P2(O): "; P3_O.text = "P3(O): ";
        P1_H.text = "P1(H): "; P2_H.text = "P2(H): "; P3_H.text = "P3(H): ";

    }


}
