using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Threading;
using System.Linq;
using UnityEngine.UIElements;
using System.Net.NetworkInformation;

public class Client : MonoBehaviour
{
    int logicFrame = 0;
    public Rigidbody rig_1P;
    public Rigidbody rig_2P;
   
    Thread clientThread;
    Thread recvThread;

    public string serverIP ;

    Socket clientSocket;
    string msg;
    byte[] content;
    
    int MsgIndex = 0;
    // Start is called before the first frame update

    void Start()
    {

        pos1P = rig_1P.position;
        pos2P = rig_2P.position;
        content = new byte[100];
        clientThread = new Thread(SendMsg);
        clientThread.IsBackground = true;
        clientThread.Start();
    }
    const int frameDelay = 20;
    // Update is called once per frame
    void Update()
    {
      

    }
    bool connected = false;

    void ConnectCallback(IAsyncResult iar)
    {
        Socket client = (Socket)iar.AsyncState;
        try
        {   
            client.EndConnect(iar);
            Debug.Log("Successfully connected to ["+ client.RemoteEndPoint+"]");
            recvThread = new Thread(recvLoop);
            recvThread.IsBackground = true;
            recvThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("Connection Failed! "+e.Message);

        }
        finally
        {

        }
    }

    Vector3 pos1P;
    Vector3 pos2P;
    Vector3 v1P;
    Vector3 v2P;
    Vector3 av1P;
    Vector3 av2P;
    
    void recvLoop() {
        Debug.Log("client recv loop on");
        int recievedLength = 0;
        byte[] buffer = new byte[1000];
        while (true) {  
            byte[] recv = new byte[72];
            int get = clientSocket.Receive(recv);
            if (get == 0)
                continue;
            Debug.Log("Client get:" + get);
            Array.Copy(recv, 0, buffer, recievedLength, get);
            recievedLength += get;    
            //clientSocket.BeginReceive(client.readBuff, client.bufferCount, client.BufferRemain(), SocketFlags.None, ReceiveCallBack, client);
            Debug.Log("packed data:" + recievedLength);
            if (recievedLength >= 72 )
            {
                recievedLength -= 72;
                pos1P = new Vector3(BitConverter.ToSingle(buffer, 0), BitConverter.ToSingle(buffer, 4), BitConverter.ToSingle(buffer, 8));
                pos2P = new Vector3(BitConverter.ToSingle(buffer, 12), BitConverter.ToSingle(buffer, 16), BitConverter.ToSingle(buffer, 20));
                v1P = new Vector3(BitConverter.ToSingle(buffer, 24), BitConverter.ToSingle(buffer, 28), BitConverter.ToSingle(buffer, 32));
                v2P = new Vector3(BitConverter.ToSingle(buffer, 36), BitConverter.ToSingle(buffer, 40), BitConverter.ToSingle(buffer, 44));
                av1P = new Vector3(BitConverter.ToSingle(buffer, 48), BitConverter.ToSingle(buffer, 52), BitConverter.ToSingle(buffer, 56));
                av2P = new Vector3(BitConverter.ToSingle(buffer, 60), BitConverter.ToSingle(buffer, 64), BitConverter.ToSingle(buffer, 68));
            }
        }
    }


    void ConnectToServer() {

        try
        {
            if (clientSocket == null) {
                Debug.Log("Trying to connect to the server...");
                IPHostEntry test = Dns.Resolve("31vp988125.qicp.vip");
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IAsyncResult result = clientSocket.BeginConnect(test.AddressList[0], 17900, new AsyncCallback(ConnectCallback), clientSocket);
                //IAsyncResult result = clientSocket.BeginConnect(IPAddress.Parse("127.0.0.1"), 5473, new AsyncCallback(ConnectCallback), clientSocket);
                connected = result.AsyncWaitHandle.WaitOne(1000, false);
            }
            
        }
        catch (Exception ex)
        {
            Debug.Log("Connection Failed! " + ex.Message);
        }
    }

    static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.     
            Socket handler = (Socket)ar.AsyncState;
            // Complete sending the data to the remote device.     
            int bytesSent = handler.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    float inputX = 0;
    float inputY = 0;

    void FixedUpdate() {
        logicFrame++;
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");
        rig_1P.position = pos1P;
        rig_2P.position = pos2P;

        //rig_1P.velocity = v1P;
        //rig_2P.velocity = v2P;

        //rig_1P.angularVelocity = av1P;
        //rig_2P.angularVelocity = av2P;
    }

    void SendFrameData() {
        //local input
        byte[] b1 = System.BitConverter.GetBytes(inputX);
        byte[] b2 = System.BitConverter.GetBytes(inputY);
        
        b1 = b1.Concat(b2).ToArray();
        clientSocket.Send(b1);
        MsgIndex++;
    }

    private void SendMsg()
    {
        while (true) {
            if (!connected)
            {
                ConnectToServer();
            }
            else
            {
                byte[] temp = new byte[100];
                Thread.Sleep(frameDelay);
                SendFrameData();
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (recvThread != null) {
            recvThread.Interrupt();
            recvThread.Abort();
        }
        if (clientThread != null)
        {
            clientThread.Interrupt();
            clientThread.Abort();
        }
        if (clientSocket != null) {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
            
    }
}
