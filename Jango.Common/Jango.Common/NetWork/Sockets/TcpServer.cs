using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jango.Common.NetWork.Sockets
{
    public class TcpServer
    {


        /// <summary>
        /// 服务socket
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private volatile Socket listenSocket;

        /// <summary>
        /// 请求参数
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SocketAsyncEventArgs acceptArg = new SocketAsyncEventArgs();

        /// <summary>
        /// 接受会话失败的次数
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int acceptFailureTimes;
        /// <summary>
        /// 获取所监听的本地IP和端口
        /// </summary>
        public IPEndPoint LocalEndPoint { get; private set; }
        /// <summary>
        /// 获取服务是否已处在监听中
        /// </summary>
        public bool IsListening { get; private set; }

        public void StartListen(int port)
        {
            this.StartListen(new IPEndPoint(IPAddress.Any, port));
        }

        /// <summary>
        /// 开始启动监听
        /// 如果IsListening为true，将不产生任何作用
        /// </summary>
        /// <param name="localEndPoint">要监听的本地IP和端口</param>    
        /// <exception cref="SocketException"></exception>
        public void StartListen(IPEndPoint localEndPoint)
        {
            if (this.IsListening)
            {
                return;
            }

            this.listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.listenSocket.Bind(localEndPoint);
            this.listenSocket.Listen(100);

            this.acceptArg = new SocketAsyncEventArgs();
            this.acceptArg.Completed += (sender, e) => this.AcceptArgCompleted(e);

            this.PreProcessAccept(this.acceptArg);

            this.LocalEndPoint = localEndPoint;
            this.IsListening = true;
        }

        /// <summary>
        /// 开始一次接受连接请求操作
        /// </summary>
        /// <param name="arg">连接参数</param>     
        private void PreProcessAccept(SocketAsyncEventArgs arg)
        {
            if (this.listenSocket != null)
            {
                arg.AcceptSocket = null;
                if (!this.listenSocket.AcceptAsync(arg))
                {
                    this.AcceptArgCompleted(arg);
                }
            }
        }

        /// <summary>
        /// 连接请求IO完成
        /// </summary>
        /// <param name="arg">连接参数</param>
        private void AcceptArgCompleted(SocketAsyncEventArgs arg)
        {
            var socket = arg.AcceptSocket;
            if (arg.SocketError == SocketError.Success)
            {
                this.ProcessAccept(socket);
            }
            else
            {
                Interlocked.Increment(ref this.acceptFailureTimes);
                var innerException = new SocketException((int)arg.SocketError);
                //this.OnException(this, new SessionAcceptExcetion(innerException));

            }

            // 处理后继续接收
            this.PreProcessAccept(arg);
        }

        private void ProcessAccept(Socket socket)
        {
            Task.Factory.StartNew(() =>
            {
                if (socket.ReceiveAsync(this.recvArg) == false)
                {
                    this.RecvCompleted(socket, this.recvArg);
                }
            });
        }

        private void TryReceive()
        {

        }
    }
}
