using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using UnityEngine.UI;

public class Server : MonoBehaviour
{
    private Thread serverThread;
    private Thread serverSendThread;

    byte[] content;

    Socket serverSocket;
    public Rigidbody rig_1P;
    public Rigidbody rig_2P;
    // Start is called before the first frame update
    Vector3 p1BornPos = new Vector3(2, 0, 0);
    Vector3 p2BornPos = new Vector3(-2, 0, 0);

    public Text t_1p;
    public Text t_2p;
    int score_1P=0;
    int score_2P=0;

    int dealyTime = 20;
    void Start()
    {
        
        content = new byte[100];
        IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12472);
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(serverEP);
        serverSocket.Listen(10);

        serverThread = new Thread(StartServer);
        serverThread.IsBackground = true;
        serverThread.Start();


    }

    void ResetWorld() {
        rig_1P.velocity = Vector3.zero;
        rig_1P.angularVelocity = Vector3.zero;
        rig_1P.position = p1BornPos;
        rig_2P.velocity = Vector3.zero;
        rig_2P.angularVelocity = Vector3.zero;
        rig_2P.position = p2BornPos;
    }

    void Reborn(int id) {
        if (id.Equals(0))
        {
            rig_1P.velocity = Vector3.zero;
            rig_1P.angularVelocity = Vector3.zero;
            rig_1P.position = p1BornPos;
        }
        else {
            rig_2P.velocity = Vector3.zero;
            rig_2P.angularVelocity = Vector3.zero;
            rig_2P.position = p2BornPos;
        }
    }

    private void AcceptCallBack(IAsyncResult ar)
    {
        try
        {
            Socket server1 = (Socket)ar.AsyncState;
            serverSocket = server1.EndAccept(ar);
            //sendSocket = server1;
            //server.BeginAccept(AcceptCallBack, server);

        }
        catch (Exception e)
        {
            Debug.Log("Accept Failed! :" + e.Message);
        }
    }

    void CopyToByte(byte[] dst, byte[] ori, int pos) {
        dst[pos] = ori[0];
        dst[pos+1] = ori[1];
        dst[pos+2] = ori[2];
        dst[pos+3] = ori[3];
    }

    void ServerSendLoop() {
        Debug.Log("Server Send loop on");
        while (true) {
            byte[] positionData = new byte[72];
            //position.
            byte[] x1 = System.BitConverter.GetBytes(Pos1P.x);
            byte[] y1 = System.BitConverter.GetBytes(Pos1P.y);
            byte[] z1 = System.BitConverter.GetBytes(Pos1P.z);
            byte[] x2 = System.BitConverter.GetBytes(Pos2P.x);
            byte[] y2 = System.BitConverter.GetBytes(Pos2P.y);
            byte[] z2 = System.BitConverter.GetBytes(Pos2P.z);
            //velocity
            byte[] vx1 = System.BitConverter.GetBytes(v1P.x);
            byte[] vy1 = System.BitConverter.GetBytes(v1P.y);
            byte[] vz1 = System.BitConverter.GetBytes(v1P.z);
            byte[] vx2 = System.BitConverter.GetBytes(v2P.x);
            byte[] vy2 = System.BitConverter.GetBytes(v2P.y);
            byte[] vz2 = System.BitConverter.GetBytes(v2P.z);
            //angular velocity
            byte[] avx1 = System.BitConverter.GetBytes(av1P.x);
            byte[] avy1 = System.BitConverter.GetBytes(av1P.y);
            byte[] avz1 = System.BitConverter.GetBytes(av1P.z);
            byte[] avx2 = System.BitConverter.GetBytes(av2P.x);
            byte[] avy2 = System.BitConverter.GetBytes(av2P.y);
            byte[] avz2 = System.BitConverter.GetBytes(av2P.z);

            CopyToByte(positionData, x1, 0);
            CopyToByte(positionData, y1, 4);
            CopyToByte(positionData, z1, 8);
            CopyToByte(positionData, x2, 12);
            CopyToByte(positionData, y2, 16);
            CopyToByte(positionData, z2, 20);
            CopyToByte(positionData, vx1, 24);
            CopyToByte(positionData, vy1, 28);
            CopyToByte(positionData, vz1, 32);
            CopyToByte(positionData, vx2, 36);
            CopyToByte(positionData, vy2, 40);
            CopyToByte(positionData, vz2, 44);
            CopyToByte(positionData, avx1, 48);
            CopyToByte(positionData, avy1, 52);
            CopyToByte(positionData, avz1, 56);
            CopyToByte(positionData, avx2, 60);
            CopyToByte(positionData, avy2, 64);
            CopyToByte(positionData, avz2, 68);
            sendSocket.Send(positionData);
            Thread.Sleep(dealyTime);
        }
    }

    Socket sendSocket;
    void StartServer()
    {
        Debug.Log("Waiting for Connection...");
        Socket temp = serverSocket.Accept();
        sendSocket = temp;

       // serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), serverSocket);

        Debug.Log("serverSendThread start");
        serverSendThread = new Thread(ServerSendLoop);
        serverSendThread.IsBackground = true;
        serverSendThread.Start();

        while (true)
        {
            int datalength = temp.Receive(content);
            //int datalength = 0;
            if (datalength != 0)
            {
                _2P_INPUT_X_AXIS = BitConverter.ToSingle(content, 0);
                _2P_INPUT_Y_AXIS = BitConverter.ToSingle(content, 4);
            }
            
        }
    }

    float _1P_INPUT_X_AXIS = 0;
    float _1P_INPUT_Y_AXIS = 0;
    float _2P_INPUT_X_AXIS = 0;
    float _2P_INPUT_Y_AXIS = 0;

    // Update is called once per frame
    void Update()
    {
        
    }
    int logicFrame = 0;

    Vector3 Pos1P;
    Vector3 Pos2P;
    Vector3 v1P;
    Vector3 v2P;
    Vector3 av1P;
    Vector3 av2P;
    void FixedUpdate()
    {
        logicFrame++;
        _1P_INPUT_X_AXIS = Input.GetAxis("Horizontal");
        _1P_INPUT_Y_AXIS = Input.GetAxis("Vertical");

        Pos1P = rig_1P.position;
        Pos2P = rig_2P.position;
        v1P = rig_1P.velocity;
        v2P = rig_2P.velocity;
        av1P = rig_1P.angularVelocity;
        av2P = rig_2P.angularVelocity;
        float rebornHeight = -5;
        if (rig_1P.position.y < rebornHeight)  
        {
            Reborn(0);
            score_1P++;
            t_1p.text = score_1P.ToString();
        }

        if (rig_2P.position.y < rebornHeight) {
            Reborn(1);
            score_2P++;
            t_2p.text = score_2P.ToString();
        }
          
        rig_1P.AddForce(new Vector3(_1P_INPUT_X_AXIS, 0, _1P_INPUT_Y_AXIS));
        rig_2P.AddForce(new Vector3(_2P_INPUT_X_AXIS, 0, _2P_INPUT_Y_AXIS));
    }

    private void OnApplicationQuit()
    {
        if(serverSendThread!=null)
        {
            serverSendThread.Interrupt();
            serverSendThread.Abort();
        }
        
        if (serverThread != null)
        {
            serverThread.Interrupt();
            serverThread.Abort();
        }
        if (serverSocket != null) {

            serverSocket.Shutdown(SocketShutdown.Both);
            serverSocket.Close();
        }   


    }
}
