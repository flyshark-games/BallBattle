using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using System.Linq;

public class NetWork : MonoBehaviour
{
    public static NetWork netWork = null;

    public string host = "127.0.0.1";
    public int port = 10002;

    private TcpClient client;
    private bool __connected = false;

    public bool connected
    {
        get { return __connected; }
    }
    private NetworkStream stream;
    private Thread recvProcess = null, sendProcess = null;
    //public InputField hostText, portText;
    private volatile bool stopSendProcess, stopRecvProcess;

    //发送队列
    public Queue<string> sendQueue;

    //半包缓冲区
    private string recvData;
    private int maxBufferSize = 4096;

    private Queue<string> recvQueue;

    private void Awake()
    {
        //保证全局唯一
       
        if (netWork != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            netWork = this;
        }

        sendQueue = new Queue<string>();
        recvQueue = new Queue<string>();
        sendQueue.Enqueue("的撒法");
        sendQueue.Enqueue("2");
        sendQueue.Enqueue("fsdf");
        sendQueue.Enqueue("双方的");
    }

    private void Start()
    {
        netWork = this;
        sendProcess = new Thread(new ThreadStart(SendProcess));
        sendProcess.IsBackground = true;
        sendProcess.Start();
        stopSendProcess = false;
        stopRecvProcess = false;
    }

    public void TryConnect()
    {
        CreateConnection(host, port);
    }

    void CreateConnection(string host, int port)
    {
        try
        {
            client = new TcpClient();
            client.NoDelay = true;
            //client.Connect(IPAddress.Parse(host), port);
            IAsyncResult result = client.BeginConnect(IPAddress.Parse(host), port, null, null);
            __connected = result.AsyncWaitHandle.WaitOne(1000, false);

            Debug.Log("tryed");
            if (__connected)
            {
                Debug.Log("connect");
                client.EndConnect(result);
            }
            else
            {
                client.Close();
            }
        }
        catch (SocketException ex)
        {
            __connected = false;
            Debug.Log("connect error: " + ex.Message);
            client.Close();
            return;
        }

        if (__connected)
        {
            stream = client.GetStream();
            if (recvProcess == null)
            {
                recvProcess = new Thread(new ThreadStart(RecvProcess));
                recvProcess.IsBackground = true;
                recvProcess.Start();
            }

        }
    }

    public static void WriteMessage(string item)
    {
        if (netWork != null)
            netWork.Write(item);
    }

    private void Write(string item)
    {
        sendQueue.Enqueue(item);
    }

    //数据发送线程，
    void SendProcess()
    {
        while (!stopSendProcess)
        {
            if (!__connected)
            {
                TryConnect();
            }
            while (sendQueue.Count > 0 && __connected)
            {
                string item = sendQueue.Dequeue();
                byte[] buffer = System.Text.Encoding.Unicode.GetBytes(item);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
            Thread.Sleep(5);
        }
    }

    private void RecvProcess()
    {
        Debug.Log("start");
        byte[] recvBuf = new byte[maxBufferSize];
        while (!stopRecvProcess)
        {
            int bytesRead = stream.Read(recvBuf, 0, maxBufferSize);
            Debug.Log(recvBuf);
            // 解析消息加到 recvQueue
        }
        //Debug.Log("stopRecvProcess");
    }

    void OnApplicationQuit()
    {
        stopSendProcess = true;
        stopRecvProcess = true;

        if (__connected)
        {
            __connected = false;
            stream.Close();
            client.Close();
        }

        if (recvProcess != null)
        {
            //recvProcess.Abort();
            // 如果没有正确关闭线程，这里的Join就会阻塞，就会卡死编辑器
            // recvProcess.Join();
            Debug.Log("recvProcess: " + recvProcess.IsAlive);
        }

        if (sendProcess != null)
        {
            //sendProcess.Abort();

            // sendProcess.Join();
            Debug.Log("sendProcess: " + sendProcess.IsAlive);
        }

    }

}

