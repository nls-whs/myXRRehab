using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Astra;
using System;

public class PerformanceView : MonoBehaviour
{
    public Text fps;
    public Text depthFrameFps;
    public Text colorFrameFps;
    public Text bodyFrameFps;
    public Text LeftkneeAngle;
    public Text RightkneeAngle;
    public GameObject bodyview;
    public GameObject messageHandler;
    public GameObject server;

    private int depthFrameCount;
    private int colorFrameCount;
    private int bodyFrameCount;
    private bool useChinese;
    private AstraBodyView bv;
    private UDPServer us;
    private MessageHandler mh;


    void Start()
    {
        // if (Application.platform != RuntimePlatform.Android)
        // {
        // cpu.gameObject.SetActive(false);
        // memory.gameObject.SetActive(false);
        // }

        bv = bodyview.gameObject.GetComponent<AstraBodyView>();
        us = server.GetComponent<UDPServer>();
        mh = messageHandler.GetComponent<MessageHandler>();

        AstraManager.Instance.OnNewDepthFrame.AddListener(OnNewDepthFrame);
        AstraManager.Instance.OnNewColorFrame.AddListener(OnNewColorFrame);
        AstraManager.Instance.OnNewBodyFrame.AddListener(OnNewBodyFrame);
        StartCoroutine(CountBodyFrameFps());

        if (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified)
        {
            useChinese = true;
        }
        else
        {
            useChinese = false;
        }
    }

    private void OnNewDepthFrame(DepthFrame arg0)
    {
        depthFrameCount++;
    }

    private void OnNewColorFrame(ColorFrame arg0)
    {
        colorFrameCount++;
    }

    private void OnNewBodyFrame(Astra.BodyFrame arg0)
    {
        bodyFrameCount++;
    }

    // Update is called once per frame
    void Update()
    {
        if(useChinese)
        {
            fps.text = string.Format("应用帧率 : {0}", PerformanceViewModel.Instance.GetFPS());
            // if (Application.platform == RuntimePlatform.Android)
            // {
            //     cpu.text = string.Format("CPU使用率 : {0}%", PerformanceViewModel.Instance.GetCpuUsage());
            //     memory.text = string.Format("内存占用(MB) : {0}", PerformanceViewModel.Instance.GetMemoryAverage());
            // }
            // skeleton.text = string.Format("骨架帧率 : {0}", PerformanceViewModel.Instance.GetSkeletonFPS());
        }
        else
        {
            fps.text = string.Format("App FPS : {0}", PerformanceViewModel.Instance.GetFPS());
            // if (Application.platform == RuntimePlatform.Android)
            // {
            //     cpu.text = string.Format("CPU usage : {0}%", PerformanceViewModel.Instance.GetCpuUsage());
            //     memory.text = string.Format("Memory(MB) average : {0}", PerformanceViewModel.Instance.GetMemoryAverage());
            // }
            // skeleton.text = string.Format("Skeleton FPS : {0}", PerformanceViewModel.Instance.GetSkeletonFPS());
        }
    }

    private IEnumerator CountBodyFrameFps()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);
            if (useChinese)
            {
                depthFrameFps.text = string.Format("深度帧率 : {0}", depthFrameCount);
                colorFrameFps.text = string.Format("彩色帧率 : {0}", colorFrameCount);
                bodyFrameFps.text = string.Format("骨架帧率 : {0}", bodyFrameCount);
            }
            else
            {
                depthFrameFps.text = string.Format("Depth FPS : {0}", depthFrameCount);
                colorFrameFps.text = string.Format("Color FPS : {0}", colorFrameCount);
                bodyFrameFps.text = string.Format("Skeleton FPS : {0}", bodyFrameCount);
                LeftkneeAngle.text = string.Format("LK Angle: {0}", bv.GetLeftKneeAngle());                
                RightkneeAngle.text = string.Format("RK Angle: {0}", bv.GetRightKneeAngle());
                                              
            }
            depthFrameCount = 0;
            colorFrameCount = 0;
            bodyFrameCount = 0;
        }
    }
}
