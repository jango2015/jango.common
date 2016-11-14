namespace Jango.Common.NetWork.Sockets
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Utilities;
    public class ServerSocket
    {

        private readonly Socket _socket;
        private readonly SocketSetting _setting;
        private readonly IPEndPoint _listeningEndPoint;
        private readonly int _defaultPort = 5000;
        private readonly SocketAsyncEventArgs _acceptSocketArgs;
        //private readonly
        public ServerSocket(IPEndPoint listeningEndPoint, SocketSetting setting)
        {
            Ensure.NotNull(listeningEndPoint, "listeningEndPoint");

            _listeningEndPoint = listeningEndPoint;
            _setting = setting;

            _socket = SocketUtils.CreateSocket();
            _acceptSocketArgs = new SocketAsyncEventArgs();
            _acceptSocketArgs.Completed += AcceptCompleted;

        }

        public void Start()
        {
            try
            {
                _socket.Bind(_listeningEndPoint);
                _socket.Listen(_defaultPort);
            }
            catch (Exception)
            {
                SocketUtils.ShudownSocket(_socket);
                throw;
            }

            StartAccepting();
        }


        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void StartAccepting()
        {
            try
            {
                var firedAsync = _socket.AcceptAsync(_acceptSocketArgs);
                if (!firedAsync)
                {
                    ProcessAccept(_acceptSocketArgs);
                }
            }
            catch (Exception ex)
            {
                //throw;
                StartAccepting();
            }
        }
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    var acceptSocket = e.AcceptSocket;
                    e.AcceptSocket = null;
                    OnSocketAccepted(acceptSocket);
                }
                else
                {
                    SocketUtils.ShudownSocket(e.AcceptSocket);
                    e.AcceptSocket = null;
                }
            }
            catch (ObjectDisposedException) { }
            catch (System.Exception)
            {
                throw;
            }
            finally
            {
                StartAccepting();
            }
        }


        private void OnSocketAccepted(Socket socket)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var connection = new TCPConnection(socket, _setting);
                    _serverConnection = connection;
                }
                catch (Exception ex)
                {
                    throw;
                }
            });
        }

        private TCPConnection _serverConnection;
        public string Get_Received_Msg()
        {
            if (_serverConnection == null) return string.Empty;
            return _serverConnection.Get_ReceivedMsg();
        }

        private void OnMessageArrivedHandler(ITCPConnection connection, byte[] message)
        {
            try
            {

            }
            catch (Exception ex)
            {


            }
        }
    }
}
