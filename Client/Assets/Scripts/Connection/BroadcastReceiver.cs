using System.Text;
using TMPro;
using UnityEngine;
using System;
using System.Net;
#if UNITY_EDITOR
using System.Net.Sockets;
using System.Threading;
#else
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.IO;
using Windows.Networking;
#endif



public class BroadcastReceiver : MonoBehaviour, IDisposable
{
    public static BroadcastReceiver instance;

    //OnMessageReceived
    public delegate void AddOnMessageReceivedDelegate(string message, IPEndPoint remoteEndpoint);
    public event AddOnMessageReceivedDelegate MessageReceived;
    private void OnMessageReceivedEvent(string message, IPEndPoint remoteEndpoint)
    {
        Debug.Log("in receive event");
        UnityMainThreadDispatcher.Instance().Enqueue(CommunicationManager.Instance.ProcessMessage(message));
    }

#if UNITY_EDITOR

    private Thread _ReadThread;
    private UdpClient _Socket;

#else
 
    DatagramSocket _Socket = null;
 
#endif

    #region COMMON
    string messageReceived = "";
    public GameObject commManager;
    private CommunicationManager cm;
    private bool isReceiving = false;
    public string ipAddress;
    IPEndPoint anyIP;
    private IPAddress ip;
    public int port;

    #endregion
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (IPAddress.TryParse(ipAddress, out ip))
        {
            anyIP = new IPEndPoint(ip, port);
        }

        Debug.Log("IP: " + ip.ToString());
        cm = commManager.GetComponent<CommunicationManager>();
        instance.MessageReceived += OnMessageReceivedEvent;
        Receive(port);

    }


#if UNITY_EDITOR

    public void SendMessageString(string message)
    {
        Debug.Log("message to sensor: " + message);
        byte[] data = Encoding.UTF8.GetBytes(message);
        SendUDPMessage(data);
    }

    public void SendUDPMessage(byte[] data)
    {
        try
        {
            _Socket.Send(data, data.Length, anyIP);
            Debug.Log("Message sent to server");
        }
        catch (Exception err)
        {
            Debug.LogError(err.ToString());
        }
    }
    public void Receive(int port)
    {
        if (isReceiving) return;

        isReceiving = true;

        // create thread for reading UDP messages
        _ReadThread = new Thread(new ThreadStart(delegate
        {
            try
            {
                // Start client 
                _Socket = new UdpClient();
                SendMessageString("hello from client");
                Debug.LogFormat("Receiving on port {0}", port);
            }
            catch (Exception err)
            {
                Debug.LogError(err.ToString());
                return;
            }
            while (true)
            {
                try
                {
                    byte[] data = _Socket.Receive(ref anyIP);

                    // encode UTF8-coded bytes to text format
                    string message = Encoding.UTF8.GetString(data);

                    if (MessageReceived != null)
                    {
                        MessageReceived(message, anyIP);
                    }
                    else
                        Debug.Log("message event not fired");

                    messageReceived = message;

                }
                catch (Exception err)
                {
                    Debug.Log(err.ToString());
                }
            }

        }));
        _ReadThread.IsBackground = true;
        _ReadThread.Start();
    }

    public void Dispose()
    {
        instance.MessageReceived -= OnMessageReceivedEvent;
        if (_ReadThread.IsAlive)
        {
            _ReadThread.Abort();
        }
        if (_Socket != null)
        {
            _Socket.Close();
            _Socket = null;
        }
        isReceiving = false;
    }


#else
 
//Variable declaration for HoloLens 
    
    string message = null;
    string portStr = null;
    string externalIP = null;
  
    
    public async void Receive(int port)
    {
        portStr = port.ToString();
        externalIP = ipAddress;

        if (isReceiving) return;
 
        isReceiving = true;
        
        // start the client
        try
        {
            _Socket = new DatagramSocket();
            _Socket.MessageReceived += _Socket_MessageReceived;
 
            await _Socket.BindServiceNameAsync(portStr);
 
            //await _Socket.BindEndpointAsync(null, portStr);
 
            //await _Socket.ConnectAsync(new HostName("255.255.255.255"), portStr.ToString());
 
 
            //HostName hostname = Windows.Networking.Connectivity.NetworkInformation.GetHostNames().FirstOrDefault();
            //var ep = new EndpointPair(hostname, portStr, new HostName("255.255.255.255"), portStr);
            //await _Client.ConnectAsync(ep);
 
            Debug.Log(string.Format("Receiving on {0}", portStr));
 
            await Task.Delay(3000);

            // send out a message, otherwise receiving does not work !

            message = "Hello from HoloLens";
            SendMessageString(message);
            //var outputStream = await _Socket.GetOutputStreamAsync(new HostName(externalIP), portStr);
            //DataWriter writer = new DataWriter(outputStream);
            
            //writer.WriteString(message);
            //await writer.StoreAsync();
        }
        catch (Exception ex)
        {           
            _Socket.Dispose();
            _Socket = null;
            messageReceived = ex.ToString();
            Debug.LogError(ex.ToString());
            Debug.LogError(Windows.Networking.Sockets.SocketError.GetStatus(ex.HResult).ToString());
        }
    }

    public void SendMessageString(string message)
    {
         SendUDPMessage( message);
    }

    //Send a UDP-Packet
    private async void SendUDPMessage( string message)
    {
        await _SendUDPMessage( message);
    }

    
    private async System.Threading.Tasks.Task _SendUDPMessage( string message)
    {
        try
        {
            var outputStream = await _Socket.GetOutputStreamAsync(new HostName(externalIP), portStr);
            DataWriter writer = new DataWriter(outputStream);
            
            writer.WriteString(message);
            await writer.StoreAsync();
            await outputStream.FlushAsync();
        }
        catch(Exception ex)
        {
            messageReceived = ex.ToString();
        }
        
    }
 
    private async void _Socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
    {
        try
        {

            // previous code causing memory leak issue
             //  Stream streamIn = args.GetDataStream().AsStreamForRead();
         //   StreamReader reader = new StreamReader(streamIn, Encoding.UTF8);
 
         //   string message = await reader.ReadLineAsync();

            using (var reader = args.GetDataReader())
            {
                var buf = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(buf);
                string message = Encoding.UTF8.GetString(buf);
                    
                IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Parse(args.RemoteAddress.RawName), Convert.ToInt32(args.RemotePort));

                //OnMessageReceivedEvent(message, remoteEndpoint);

                if (MessageReceived != null)
                {
                    MessageReceived(message, remoteEndpoint);
                }
                messageReceived = message;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
             isReceiving = false;
        }
    }
 
    public void Dispose()
    {
    _Socket.MessageReceived -= _Socket_MessageReceived;
        if (_Socket != null)
        {
            _Socket.Dispose();
            _Socket = null;
        }
        isReceiving = false;
    }
#endif

    void Update()
    {
        if (messageReceived != null)
        {
            //txtLog.text = messageReceived;
            // updateSphere();
            messageReceived = null;
        }

    }

    private void OnDisable()
    {
        Dispose();
    }

}