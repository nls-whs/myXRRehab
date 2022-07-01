using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;

public class UDPServer : Singleton<UDPServer>
{
    // For connection
    private IPEndPoint sender;
    private int recv;
    private Socket serverSocket;
    private Thread receiveThread;
    private EndPoint remote;
    private byte[] data = new byte[1024];
    public GameObject msgHandler;
    private MessageHandler mh;
    private string clientMessage;
    private string message = null;


    [HideInInspector]
    public bool isClientConnected = false;

    // Start is called before the first frame update
    void Start()
    {
        mh = msgHandler.GetComponent<MessageHandler>();
        BindSocket();

        // Define local endpoint from which messages are send
        // New Thread for incoming messages
        receiveThread = new Thread(new ThreadStart(ReceiveConnection));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void BindSocket()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 5432);
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        serverSocket.Bind(ipep);
        message = "Waiting for a client...";

    }

    // Update is called once per frame
    void Update()
    {
        if (message != null)
        {
            mh.SetLogText(message);

            message = null;
        }
    }

    private void ReceiveConnection()
    {
        //initialize sender 
        sender = new IPEndPoint(IPAddress.Any, 0);
        remote = (EndPoint)sender;

        byte[] data = new byte[2048];
        try
        {
            //receive connection
            while (true)
            {
                recv = serverSocket.ReceiveFrom(data, ref remote);
                Debug.Log("receiving value: " + recv);
                if (recv > 0) isClientConnected = true;
                else isClientConnected = false;
                // Converte Bytes with UTF8-Code to Text
                string text = Encoding.ASCII.GetString(data, 0, recv);
                SetMessageFromClient(text);
                message = "Message received from client: " + text;
                Debug.Log(message);
                if (text.ToLower().Contains("hello"))
                {
                    SendMessage("SC"); // server connected
                }
            }
        }
        catch (Exception ex)
        {
            isClientConnected = false;
            Debug.Log(ex.ToString());
        }
    }

    public void SetMessageFromClient(string text)
    {
        clientMessage = text;
    }

    public string GetMessageFromClient()
    {
        return clientMessage;
    }
    public void SendMessage(string input)
    {
        SendDatatoClient(remote, input);
    }

    private void SendDatatoClient(EndPoint rem, string input)
    {

        try
        {
            byte[] dataPacket = Encoding.ASCII.GetBytes(input);
            if (isClientConnected)
            {
                serverSocket.SendTo(dataPacket, dataPacket.Length, SocketFlags.None, rem);
            }

            Debug.Log("Message sent to client: " + input);

        }
        catch (Exception ex)
        {
            print(ex.ToString());
        }

    }

    void OnDisable()
    {
        if (receiveThread != null)
            receiveThread.Abort();
        SendMessage("SD;"); // server disconnect
        serverSocket.Close();
        isClientConnected = false;
        Debug.Log("socket close");
    }


}