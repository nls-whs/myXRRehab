using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.Events;

#if !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Networking;
#else
using System.Net;
using System.Net.Sockets;
#endif

/// <summary>
/// UDP communication script for the pose recognition system.
/// Is in principle generalized and could be merged with the other
/// UDP communication script.
/// </summary>
public class UDPConnection : MonoBehaviour
{
    // prefs
    public string ipAdress;
    public int port;
#pragma warning disable CS0414
    // Variables for Hololens stuff
    [Tooltip("Port to open on HoloLens to send or listen")]
    private string internalPort = "11000";

    [Tooltip("IP address to send to")]
    private string externalIP = "192.168.1.130";

    [Tooltip("Port to send to")]
    private string externalPort = "11000";

    [Tooltip("Send a message on startup")]
    private bool sendPingAtStart = true;

    [Tooltip("Contents of the startup message")]
    private string PingMessage = "Let there be UDP.";

    [Tooltip("Functions to invoke on packet reception")]
    private UDPMessageEvent udpEvent = null;

    private readonly Queue<Action> ExecuteOnMainThread = new Queue<Action>();
#pragma warning restore CS0414

    // Signal for other scripts to see if a new message arrived
    public bool ReceivedNewMessage = false;
    // The new message, if it arrived
    public string LastMessage = "";

#if UNITY_EDITOR


    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;
    Thread ReceiveThread;

    public void Start()
    {
        // If no object tracking is happening, the UDP connection does also not
        // need to start.
      //  if (!SetupRobotarm.Instance.StartObjectTracking) return;
        // Define endPoint from which messages are send
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAdress), port);
       client = new UdpClient();

        WorldDebugOutput.ActivateLog();
        WorldDebugOutput.LogAppend(ipAdress + " : " + port);

        SendMessageString(PingMessage);
        StartUDPMessageReceive();
       
    }


    public void SendMessageString(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        SendUDPMessage(data);
    }

    public void SendUDPMessage(byte[] data)
    {
        try
        {
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            Debug.LogError(err.ToString());
        }
    }


    private void StartUDPMessageReceive()
    {
        if (ReceiveThread != null && ReceiveThread.IsAlive)
        {
            ReceiveThread.Abort();
        }
        ReceiveThread = new Thread(new ThreadStart(ReceivedThread));
        ReceiveThread.IsBackground = true;
        ReceiveThread.Start();

    }

    /// <summary>
    /// Endlessly running loop, which collects all UDP messages.
    /// </summary>
    private void ReceivedThread()
    {
      //  client = new UdpClient(port);
        while (true)
        {
            byte[] Data = client.Receive(ref remoteEndPoint);
            if (Data != null && Data.Length > 0)
            {
                string DataS = Encoding.UTF8.GetString(Data);

                LastMessage = DataS;
                ReceivedNewMessage = true;
            }
        }
        
    }


#else

    void UDPMessageReceived(string host, string port, byte[] data)
    {
        string message = Encoding.UTF8.GetString(data);
        LastMessage = message;
        ReceivedNewMessage = true;
    }

    public void SendMessageString(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        SendUDPMessage(data);
    }

    public async void SendUDPMessage(byte[] data)
    {
        await _SendUDPMessage(externalIP, externalPort, data);
    }

    DatagramSocket socket;

    async void Start()
    {
        internalPort = port.ToString();
        externalPort = port.ToString();
        externalIP = ipAdress;

        // Adds the message received procedure to the event list in order to
        // call it, when we receive something. Because we receive messages on
        // a different thread, we cannot just call the procedure.
        if (udpEvent == null)
        {
            udpEvent = new UDPMessageEvent();
            udpEvent.AddListener(UDPMessageReceived);
        }
        
        socket = new DatagramSocket();
        socket.MessageReceived += Socket_MessageReceived;

        HostName IP = null;
        try
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            IP = NetworkInformation.GetHostNames().SingleOrDefault(hn => hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId);

            await socket.BindEndpointAsync(IP, internalPort);
            //await socket.BindServiceNameAsync(internalPort);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());

            WorldDebugOutput.LogAppend("Starting PoseUDP had following errors:");
            WorldDebugOutput.LogAppend(e.ToString());
            WorldDebugOutput.LogAppend(SocketError.GetStatus(e.HResult).ToString());
            return;
        }

        // Sends a ping message to the other side to enable receiving messages.
        // It does _not_ receive anything without warning, if this is not done.
        if (externalIP != null && externalPort != null && sendPingAtStart)
        {
            if (PingMessage == null)
            {
                PingMessage = "Ping!";
            }
            SendUDPMessage(Encoding.UTF8.GetBytes(PingMessage));
        }
    }
         
    private async System.Threading.Tasks.Task _SendUDPMessage(string externalIP, string externalPort, byte[] data)
    {
        Debug.Log(externalIP +" "+ externalPort);
        using (var stream = await socket.GetOutputStreamAsync(new Windows.Networking.HostName(externalIP), externalPort))
        {
            using (var writer = new Windows.Storage.Streams.DataWriter(stream))
            {
                writer.WriteBytes(data);
                await writer.StoreAsync();

            }
        }
    }


    static MemoryStream ToMemoryStream(Stream input)
    {
        try
        {                                         
            byte[] block = new byte[0x1000];      // Read and write in blocks of 4K. 
            MemoryStream ms = new MemoryStream();
            while (true)
            {
                int bytesRead = input.Read(block, 0, block.Length);
                if (bytesRead == 0) return ms;
                ms.Write(block, 0, bytesRead);
            }
        }
        finally { }
    }

    void Update()
    {
        if (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
    }

    private void Socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
    {
        //Read the message that was received from the UDP  client.
        Stream streamIn = args.GetDataStream().AsStreamForRead();
        MemoryStream ms = ToMemoryStream(streamIn);
        byte[] msgData = ms.ToArray();

        if (ExecuteOnMainThread.Count == 0)
        {
            ExecuteOnMainThread.Enqueue(() =>
            {
                if (udpEvent != null) udpEvent.Invoke(args.RemoteAddress.DisplayName, internalPort, msgData);
            });
        }
    }

#endif
}

[System.Serializable]
public class UDPMessageEvent : UnityEvent<string, string, byte[]> { }
