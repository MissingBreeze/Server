using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Receive
{
    class Serv
    {
        /// <summary>
        /// 监听套接字
        /// </summary>
        public Socket listenfd;

        public Conn[] conns;

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int maxConn = 50;

        /// <summary>
        /// 获取连接池索引，返回负数表示获取失败
        /// </summary>
        /// <returns></returns>
        public int NewIndex() 
        {
            if (conns == null) 
            {
                return -1;
            }
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null) 
                {
                    conns[i] = new Conn();
                    return i;
                }
                else if (conns[i].isUse == false) 
                {
                    return i;
                }
            }
            return -1;
        }

        public void Start(string hosts, int port) 
        {
            conns = new Conn[maxConn];
            for (int i = 0; i < maxConn; i++)
            {
                conns[i] = new Conn();
            }
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse(hosts);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            listenfd.Bind(ipEp);
            // 监听，maxConn 监听的最大数量
            listenfd.Listen(maxConn);
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("服务器启动成功");
        }

        // 监听回调
        public void AcceptCb(IAsyncResult ar) 
        {
            try
            {
                Socket socket = listenfd.EndAccept(ar);
                int index = NewIndex();
                if (index < 0)
                {
                    socket.Close();
                    Console.Write("连接已满");
                }
                else 
                {
                    Conn conn = conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAddress();
                    Console.WriteLine("客户端已连接，ip:" + adr + "池ID" + index);
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                }
                listenfd.BeginAccept(AcceptCb,null);
            }
            catch (Exception e) 
            {
                Console.WriteLine("AcceptCb失败:" + e.Message);
            }
        }


        private void ReceiveCb(IAsyncResult ar) 
        {
            Conn conn = (Conn)ar.AsyncState;
            try 
            {
                int count = conn.socket.EndReceive(ar);
                if (count <= 0) 
                {
                    Console.WriteLine("收到" + conn.GetAddress() + "断开连接");
                    conn.Close();
                    return;
                }
                string str = System.Text.Encoding.UTF8.GetString(conn.readBuff,0,count);
                Console.WriteLine("收到" + conn.GetAddress() + "的数据：" + str);
                str = conn.GetAddress() + ":" + str;
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
                for (int i = 0; i < conns.Length; i++)
                {
                    if (conns[i] == null) 
                    {
                        continue;
                    }
                    if (!conns[i].isUse) 
                    {
                        continue;
                    }
                    Console.WriteLine("将消息传送给" + conns[i].GetAddress());
                    conns[i].socket.Send(bytes);
                }
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);

            }
            catch(Exception e)
            {
                Console.WriteLine("收到" + conn.GetAddress() + "断开连接");
                conn.Close();
            }

        }

    }
}
