using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

#if !UNITY_EDITOR
using System.Threading.Tasks;
#endif

public class TCPConnection : MonoBehaviour
{

    //String variable that will be sent to the server
    [SerializeField]
    private string clientMessage;

    public GameObject tm;
    private TextMeshPro tmp;
    
    public string ipAddress;
    public string port;

#if !UNITY_EDITOR
    private bool _useUWP = true;
    private Windows.Networking.Sockets.StreamSocket socket;
    private Task exchangeTask;
#endif

#if UNITY_EDITOR
    private bool _useUWP = false;
    System.Net.Sockets.TcpClient client;
    System.Net.Sockets.NetworkStream stream;
    private Thread exchangeThread;
#endif

    private Byte[] bytes = new Byte[256];
    private StreamWriter writer;
    private StreamReader reader;

    public void Start()
    {
        
        //Server ip address and port
        Connect(ipAddress, port);
        RestartExchange();

       
        tmp = tm.GetComponent<TextMeshPro>();
    }

    public void Connect(string host, string port)
    {
        if (_useUWP)
        {
            ConnectUWP(host, port);
        }
        else
        {
            ConnectUnity(host, port);
        }
    }

#if UNITY_EDITOR
    private void ConnectUWP(string host, string port)
#else
    private async void ConnectUWP(string host, string port)
#endif
    {
#if UNITY_EDITOR
        errorStatus = "UWP TCP client used in Unity!";
#else
        try
        {
            if (exchangeTask != null) StopExchange();

            socket = new Windows.Networking.Sockets.StreamSocket();
            Windows.Networking.HostName serverHost = new Windows.Networking.HostName(host);
            await socket.ConnectAsync(serverHost, port);

            Stream streamOut = socket.OutputStream.AsStreamForWrite();
            writer = new StreamWriter(streamOut) { AutoFlush = true };

            Stream streamIn = socket.InputStream.AsStreamForRead();
            reader = new StreamReader(streamIn);

            //RestartExchange();
            successStatus = "Connected!";
        }
        catch (Exception e)
        {
            errorStatus = e.ToString();
        }
#endif
    }

    private void ConnectUnity(string host, string port)
    {
#if !UNITY_EDITOR
        errorStatus = "Unity TCP client used in UWP!";
#else
        try
        {
            if (exchangeThread != null) StopExchange();

            client = new System.Net.Sockets.TcpClient(host, Int32.Parse(port));
            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };

          //  RestartExchange();
            successStatus = "Connected!";
        }
        catch (Exception e)
        {
            errorStatus = e.ToString();
        }
#endif
    }

    private bool exchanging = false;
    private bool exchangeStopRequested = false;
    private string lastPacket = null;

    private string errorStatus = null;
    private string successStatus = null;

    public void RestartExchange()
    {
#if UNITY_EDITOR
        if (exchangeThread != null) StopExchange();
        exchangeStopRequested = false;
        exchangeThread = new System.Threading.Thread(ExchangePackets);
        exchangeThread.IsBackground = true;
        exchangeThread.Start();
#else
        if (exchangeTask != null) StopExchange();
        exchangeStopRequested = false;
        exchangeTask = Task.Run(() => ExchangePackets());
#endif
    }

    public void Update()
    {
        if (lastPacket != null)
        {
            Debug.Log("got a packet, need to update text");
            changeText();
            lastPacket = null;
        }
        /*if (errorStatus != null)
        {
            Debug.Log(errorStatus);
            errorStatus = null;
        }
        if (successStatus != null)
        {
            Debug.Log(successStatus);
            successStatus = null;
        }*/
    }

    private void changeText()
    {
        if (tmp != null && lastPacket != null)
        {
            Debug.Log("text mesh ref is not null");
           tmp.text = lastPacket;      
        }
        else
        {
            Debug.Log("text mesh pro object is null");
        }
        
    }

    public void ExchangePackets()
    {
        try
        {

            
                exchanging = true;
            if (clientMessage == "Ping")
            {
                writer.Write("Ping from HoloLens");
            }
                
                Debug.Log("Sent data!");
                string received = null;
                byte[] bytes = null;

#if UNITY_EDITOR
           
                bytes = new byte[client.SendBufferSize];


                int recv = 0;
                while (true)
                {
                    Debug.Log("reading data from server");
                    recv = stream.Read(bytes, 0, client.SendBufferSize);
                    if (recv > 0)
                    {
                        Debug.Log("data received");
                        received += Encoding.UTF8.GetString(bytes, 0, recv);
                        Debug.Log("message from server: " + received);
                        if (received.EndsWith("\n")) break;
                    lastPacket = received;
                    received = null;
                    }
                    else
                    {
                        Debug.Log("no data received");
                    }
                }

#else
            received = reader.ReadLine();
            lastPacket = received;
#endif

            //  lastPacket = received;
            //  Debug.Log("Read data: " + received);

            exchanging = false;
               

        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

    }


    public void StopExchange()
    {
        exchangeStopRequested = true;

#if UNITY_EDITOR
        if (exchangeThread != null)
        {
            exchangeThread.Abort();
            stream.Close();
            client.Close();
            writer.Close();
            reader.Close();

            stream = null;
            exchangeThread = null;
        }
#else
        if (exchangeTask != null)
        {
            exchangeTask.Wait();
            socket.Dispose();
            writer.Dispose();
            reader.Dispose();

            socket = null;
            exchangeTask = null;
        }
#endif
        writer = null;
        reader = null;
    }

    public void OnDestroy()
    {
        StopExchange();
    }

}