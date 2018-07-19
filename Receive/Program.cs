using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace Receive
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始了！");
            //Serv serv = new Serv();
            //serv.Start("127.0.0.1",2556);
            IPAddress ipAdr = IPAddress.Parse("172.17.0.5");
            IPEndPoint hosts = new IPEndPoint(ipAdr, 2556);
            TcpListener tcpLisyener = new TcpListener(hosts);
            tcpLisyener.Start();
            Console.WriteLine("开始监听...");
            while (true) 
            {
                string str = Console.ReadLine();
                switch(str)
                {
                    case "quit":
                        return;
                    case "send":
                        Thread thread = new Thread(SendFileFunc);
                        thread.Start(tcpLisyener);
                        thread.IsBackground = true;
                        break;
                }
            }
        }

        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="obj"></param>
        private static void SendFileFunc(object obj) 
        {
            TcpListener tcpListener = obj as TcpListener;
            while (true) 
            {
                try 
                {
                    // 接收请求
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    if (tcpClient.Connected) 
                    {
                        // 要传输的流
                        NetworkStream stream = tcpClient.GetStream();

                        FileStream fileStream = new FileStream("C:\\test.jar", FileMode.Open);

                        int fileReadSize = 0;
                        long fileLength = 0;

                        while (fileLength < fileStream.Length) 
                        {
                            byte[] buffer = new byte[2048];
                            fileReadSize = fileStream.Read(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, fileReadSize);
                            fileLength += fileReadSize;
                        }
                        fileStream.Flush();
                        stream.Flush();
                        fileStream.Close();
                        stream.Close();
                        Console.WriteLine("发送成功!");
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("发送失败" + e.Message);
                }
                
            }




        }
    }
}
