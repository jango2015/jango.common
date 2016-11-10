namespace Jango.Common.NetWork.Sockets
{
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Utilities;
    public class ClientSocket
    {
        private EndPoint _serverEndPoint;
        private EndPoint _localEndPoint;
        private Socket _socket;
        private readonly ManualResetEvent _waitConnectHandle;

        private readonly SocketSetting _setting;

        public ClientSocket(EndPoint serverEndPoint, EndPoint localEndPoint, SocketSetting setting)
        {
            Ensure.NotNull(serverEndPoint, "serverEndPoint");
            Ensure.NotNull(setting, "setting");

            _serverEndPoint = serverEndPoint;
            _localEndPoint = localEndPoint;
            _setting = setting;
            _waitConnectHandle = new ManualResetEvent(false);
            _socket = SocketUtils.CreateSocket();

        }

        public ClientSocket Start(int waitMilliseconds = 5000)
        {
            var socketArgs = new SocketAsyncEventArgs();
            socketArgs.AcceptSocket = _socket;
            socketArgs.RemoteEndPoint = _serverEndPoint;
            socketArgs.Completed += OnconnectAsyncCompleted;

            if (null != _localEndPoint)
            {
                _socket.Bind(_localEndPoint);
            }

            var firedAsync = _socket.ConnectAsync(socketArgs);
            if (!firedAsync)
            {
                ProcessConnect(socketArgs);
            }
            _waitConnectHandle.WaitOne(waitMilliseconds);
            return this;
        }

        private void OnconnectAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessConnect(e);
        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            e.Completed -= OnconnectAsyncCompleted;
            e.AcceptSocket = null;
            e.RemoteEndPoint = null;
            e.Dispose();
            if (SocketError.Success != e.SocketError)
            {
                SocketUtils.ShudownSocket(_socket);
                OnConnectionFailed(e.SocketError);
                return;
            }
            OnConnectionEstablished();
        }

        private void OnConnectionEstablished()
        {

        }
        private void OnConnectionFailed(SocketError socketError)
        {

        }
    }
}
