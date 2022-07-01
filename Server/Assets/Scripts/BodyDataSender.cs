using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyDataSender : MonoBehaviour
{
    private float time, timeDelay;
    public GameObject AstraBodyView;

    private AstraBodyView _AstraBodyView;
    // Start is called before the first frame update
    void Start()
    {
        time = 0f;
        timeDelay = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if (AstraBodyView == null)
        {
            return;
        }

        _AstraBodyView = AstraBodyView.GetComponent<AstraBodyView>();
        if (_AstraBodyView == null)
        {
            return;
        }

        //The delay is kept to sync the elapsed time between server and client (HoloLens)

        time = time + 1f * Time.deltaTime;


        if (UDPServer.Instance.isClientConnected)
        {
            if (Calibration.instance._isCalibrationCompleted == true)
            {
                if (time >= timeDelay)
                {
                    time = 0f;
                    MessageHandler.Instance.SendBodyData(1, _AstraBodyView.GetBody());
                    MessageHandler.Instance.SendBodyAngle();

                }
            }

        }
    }
}
