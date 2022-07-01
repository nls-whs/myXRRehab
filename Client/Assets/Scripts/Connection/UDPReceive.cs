using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive : MonoBehaviour
{
    public int port;
    private Thread receiveThread;
    private UdpClient client;


    // infos
    public string lastReceivedUDPPacket = "";


    // start from unity3d
    public void Start()
    {
        init();
    }

    // init
    private void init()
    {
        // Define lokael endpoint from which messages are send
        // New Thread for incoming messages
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

    }

    // receive thread
    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            try
            {
                // Receive Bytes
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);

                // Converte Bytes with UTF8-Code to Text
                string text = Encoding.UTF8.GetString(data);
                //Debug.Log("Received: "+text);
                //UnityMainThreadDispatcher.Instance().Enqueue(UDPCommunicationManager.Instance.processUDPMessage(text));
                //UDPCommunicationManager.Instance.processUDPMessage(text);
                // latest UDPpacket
                lastReceivedUDPPacket = text;
                //Debug.Log(text);

            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null)
            receiveThread.Abort();

        client.Close();
    }
}
