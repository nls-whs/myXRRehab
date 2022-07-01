using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class TextHandler : Singleton<TextHandler>
{
    public TextMeshProUGUI textValues; // Orbbec and HoloLens values
    public TextMeshProUGUI textCalibrate; // calibration text
    public TextMeshProUGUI textStatus; // server and body status
    public TextMesh textInstructions; // instructions
    public TextMesh textCounter; // counter
    public GameObject dialog;
    private CommunicationManager cm;
    private BodyView bv;

    public GameObject comManager;
    public GameObject bodyView;
    public int InstructionSteps = 0;
    private float time, timeDelay;
    private int count = 15;

    private enum BodyStatus
    {
        NotTracking = 0,
        Lost = 1,
        TrackingStarted = 2,
        Tracking = 3
    }
    private void Start()
    {
        cm = comManager.GetComponent<CommunicationManager>();
        bv = bodyView.GetComponent<BodyView>();
        time = 0f;
        timeDelay = 5f;

    }

    private void Update()
    {
        UpdateStatusText();
        time = time + 1f * Time.deltaTime;
        int i = 0;
        if (cm.isClosed)
        {
            if (time >= timeDelay)
            {
                time = 0f;
                UpdateInstructions();
                if (count != 0)
                {
                    UpdateCounter();
                }
                else
                {
                    if (i == 0)
                    {
                        textInstructions.text = "Exercise completed";
                        textCounter.text = string.Empty;
                        i++;
                    }

                }
            }

        }

    }

    private void UpdateCounter()
    {
        if (bv.StartPositionReached && bv.EndPositionReached)
        {
            count--;
            textCounter.text = count.ToString();
            bv.StartPositionReached = false;
            bv.EndPositionReached = false;
        }
    }

    private void UpdateInstructions()
    {
        if (InstructionSteps < 6)
        {
            if (InstructionSteps == 0)
            {
                textInstructions.text = "Be ready for start position";
                InstructionSteps++;
            }
            else if (InstructionSteps == 1)
            {
                textInstructions.text = "Place your feet on the foot print and move to start position";
                InstructionSteps++;
            }
            else if (InstructionSteps == 2)
            {
                dialog.SetActive(true);
                // InstructionSteps++;
            }
            else if (InstructionSteps == 3)
            {
                if (bv.StartPositionReached)
                {
                    textInstructions.text = "Start position reached";
                    InstructionSteps++;
                    bv.StartPositionReached = false;
                }

            }
            else if (InstructionSteps == 4)
            {
                textInstructions.text = "Now start the exercise. You have to repeat 15 steps to complete the exercise";
                InstructionSteps++;
                textCounter.text = count.ToString();

            }
            else if (InstructionSteps == 5)
            {
                textInstructions.text = string.Empty;
                InstructionSteps++;
            }
        }

    }

    /// <summary>
    /// This method is used to update server and body tracking status coming from the server
    /// </summary>
    private void UpdateStatusText()
    {
        StringBuilder sb = new StringBuilder();
        sb.Clear();

        if (cm.isServerConnected)
        {
            sb.Append("Server: Connected" + "  ");
            //  sb.Append("Body: ")
        }
        else
        {
            sb.Append("Server: Disconnected" + "  ");
        }
       sb.Append("Body: " + Enum.GetValues(typeof(BodyStatus)).GetValue(cm._bodyStatus));

        textStatus.text = sb.ToString();
    }

    /// <summary>
    /// This method is called to update Orbbec and HoloLens position values after the data is received from the sensor against the corresponding head position from the HoloLens
    /// </summary>
    public void TextUpdate()
    {
        StringBuilder sb = new StringBuilder();
        sb.Clear();

        if (cm.isReset)
        {
            ClearVariables();
        }
        else
        {
            if (cm.P1O != Vector3.zero && cm.P1H != Vector3.zero)
            {
                sb.Append("P1(H):" + cm.P1H.ToString("f3") + " ");
                sb.Append("P1(O):" + cm.P1O.ToString("f3") + "\n");
            }

            if (cm.P2O != Vector3.zero && cm.P2H != Vector3.zero)
            {
                sb.Append("P2(H):" + cm.P2H.ToString("f3") + " ");
                sb.Append("P2(O):" + cm.P2O.ToString("f3") + "\n");
            }
            if (cm.P3O != Vector3.zero && cm.P3H != Vector3.zero)
            {
                sb.Append("P3(H):" + cm.P3H.ToString("f3") + " ");
                sb.Append("P3(O):" + cm.P3O.ToString("f3") + "\n");
            }

            textValues.text = sb.ToString();
        }

        void ClearVariables()
        {
            textValues.text = string.Empty;
            textInstructions.text = string.Empty;
            textCounter.text = string.Empty;
            InstructionSteps = 0;
            bv.StartPositionReached = false;
            bv.EndPositionReached = false;
            count = 15;
        }
    }

    /// <summary>
    /// This method is called from Unity Inspector on button click "Finish"
    /// </summary>
    /// <param name="s">text to be displayed</param>
    public void TextUpdate(string s)
    {

        if (cm.isFinished)
        {
            textCalibrate.text = s;
        }
        else if (!cm.isFinished)
        {
            textCalibrate.text = @"Calibration failed ! <br><size=7> Please capture all 3 points before selecting Finish. Select Reset to continue.";
        }

    }

}
