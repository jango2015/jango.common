using Jango.Common.NetWork.Sockets;
using Jango.Common.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jango.Common.NetWork
{
    public interface ITCPConnection
    {
        bool IsConnected { get; }
        EndPoint LocalEndPoint { get; }
        EndPoint RemotingEndPoint { get; }
        void QueueMessage(byte[] message);
        void Close();
    }
    public class TCPConnection : ITCPConnection
    {
        private Socket _socket;
        private readonly SocketSetting _setting;
        private readonly EndPoint _localEndPoint;
        private readonly EndPoint _remotingEndPoint;
        private readonly SocketAsyncEventArgs _sendSocketArgs;
        private readonly SocketAsyncEventArgs _receiveSocketArgs;
        private readonly ConcurrentQueue<IEnumerable<ArraySegment<byte>>> _sendingQueue = new ConcurrentQueue<IEnumerable<ArraySegment<byte>>>();
        private readonly ConcurrentQueue<ReceiveData> _receiveQueue = new ConcurrentQueue<ReceiveData>();

        private Action<ITCPConnection, byte[]> _messageArrivedHandler;
        private Action<ITCPConnection, SocketError> _connectionClosedHandler;
        private readonly MemoryStream _sendingStream = new MemoryStream();

        private int _sending;
        private int _receiving;
        private int _parsing;
        private int _closing;
        private long _pendingMessageCount;
        public long PendingMessageCount
        {
            get { return _pendingMessageCount; }
        }


        public TCPConnection(Socket socket, SocketSetting setting, Action<ITCPConnection, byte[]> messageArrivedHandler, Action<ITCPConnection, SocketError> connectionClosedHandler)
        {
            Ensure.NotNull(socket, "socket");
            Ensure.NotNull(setting, "setting");
            Ensure.NotNull(connectionClosedHandler, "connectionClosedHandler");

            _socket = socket;
            _setting = setting;
            _localEndPoint = socket.LocalEndPoint;
            _remotingEndPoint = socket.RemoteEndPoint;

            //_messageArrivedHandler = messageArrivedHandler;
            //_connectionClosedHandler = connectionClosedHandler;

            _sendSocketArgs = new SocketAsyncEventArgs();
            _sendSocketArgs.AcceptSocket = socket;
            _sendSocketArgs.Completed += OnSendAsyncCompleted;

            _receiveSocketArgs = new SocketAsyncEventArgs();
            _receiveSocketArgs.AcceptSocket = socket;
            _receiveSocketArgs.Completed += OnReceiveAsyncCompleted;
        }

        public TCPConnection(Socket socket, SocketSetting setting)
        {
            Ensure.NotNull(socket, "socket");
            Ensure.NotNull(setting, "setting");

            _socket = socket;
            _setting = setting;
            _localEndPoint = socket.LocalEndPoint;
            _remotingEndPoint = socket.RemoteEndPoint;

            //_messageArrivedHandler = messageArrivedHandler;
            //_connectionClosedHandler = connectionClosedHandler;

            _receiveSocketArgs = new SocketAsyncEventArgs();
            _receiveSocketArgs.AcceptSocket = socket;
            _receiveSocketArgs.Completed += OnReceiveAsyncCompleted;
        }

        private void OnReceiveAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        private void ProcessReceive(SocketAsyncEventArgs socketArgs)
        {
            if (socketArgs.BytesTransferred == 0 || socketArgs.SocketError != SocketError.Success)
            {
                CloseInternal(socketArgs.SocketError, socketArgs.SocketError != SocketError.Success ? "socket reveive error" : "socket normal close", null);
                return;
            }
            try
            {
                var segment = new ArraySegment<byte>(socketArgs.Buffer, socketArgs.Offset, socketArgs.Count);
                _receiveQueue.Enqueue(new ReceiveData(segment, socketArgs.BytesTransferred));
                socketArgs.SetBuffer(null, 0, 0);

                TryParseReceiveData();
            }
            catch (Exception ex)
            {

                throw;
            }
            TryReceive();


        }
        private bool EnterReceiving()
        {
            return Interlocked.CompareExchange(ref _receiving, 1, 0) == 0;
        }
        private void ExitReceiving()
        {
            Interlocked.Exchange(ref _receiving, 0);
        }

        private void TryReceive()
        {
            if (!EnterReceiving()) return;

            var buffer = _receiveDataBufferPool.Get();
            if (buffer == null)
            {
                CloseInternal(SocketError.Shutdown, "Socket receive allocate buffer failed.", null);
                ExitReceiving();
                return;
            }

            try
            {
                _receiveSocketArgs.SetBuffer(buffer, 0, buffer.Length);
                if (_receiveSocketArgs.Buffer == null)
                {
                    CloseInternal(SocketError.Shutdown, "Socket receive set buffer failed.", null);
                    ExitReceiving();
                    return;
                }

                bool firedAsync = _receiveSocketArgs.AcceptSocket.ReceiveAsync(_receiveSocketArgs);
                if (!firedAsync)
                {
                    ProcessReceive(_receiveSocketArgs);
                }
            }
            catch (Exception ex)
            {
                CloseInternal(SocketError.Shutdown, "Socket receive error, errorMessage:" + ex.Message, ex);
                ExitReceiving();
            }
        }
        private bool EnterParsing()
        {
            return Interlocked.CompareExchange(ref _parsing, 1, 0) == 0;
        }
        private void ExitParsing()
        {
            Interlocked.Exchange(ref _parsing, 0);
        }
        private void TryParseReceiveData()
        {
            if (!EnterParsing()) return;
            try
            {
                var dataList = new List<ReceiveData>(_receiveQueue.Count);
                var segmentList = new List<ArraySegment<byte>>();
                ReceiveData data;
                while (_receiveQueue.TryDequeue(out data))
                {
                    dataList.Add(data);
                    segmentList.Add(new ArraySegment<byte>(data.Buf.Array, data.Buf.Offset, data.DataLength));
                }
                receivedMsgSegments = segmentList;
                DeParseData(segmentList);

            }
            catch (Exception)
            {
            }
            finally
            {
                ExitParsing();
            }
        }
        private void DeParseData(IEnumerable<ArraySegment<byte>> data)
        {
            if (!data.Any()) throw new ArgumentNullException("data");
            foreach (var buffer in data)
            {
                Parse(buffer);
            }
        }

        private List<ArraySegment<byte>> receivedMsgSegments = new List<ArraySegment<byte>>();
        public string Get_ReceivedMsg()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var receivedMsgSegment in receivedMsgSegments)
            {

                sb.AppendFormat("*** {0} *** ", System.Text.Encoding.Default.GetString(receivedMsgSegment.Array));
            }
            return sb.ToString();
        }
        private void Parse(ArraySegment<byte> bytes)
        {
            byte[] data = bytes.Array;
            var strMsg = System.Text.Encoding.Default.GetString(data);

            _messageArrivedHandler(this, data);

        }



        internal struct ReceiveData
        {
            public readonly ArraySegment<byte> Buf;
            public readonly int DataLength;

            public ReceiveData(ArraySegment<byte> buf, int dataLength)
            {
                Buf = buf;
                DataLength = dataLength;
            }
        }

        private void CloseInternal(SocketError socketError, string reason, Exception exception)
        {
        }







        public bool IsConnected
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public EndPoint LocalEndPoint
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public EndPoint RemotingEndPoint
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void QueueMessage(byte[] message)
        {
            if (message.Length == 0) return;
            var segments = this.FrameData(new ArraySegment<byte>(message, 0, message.Length));

            _sendingQueue.Enqueue(segments);
            Interlocked.Increment(ref _pendingMessageCount);

            TrySend();
        }

        private void TrySend()
        {
            if (_closing == 1) return;
            if (!EnterSending()) return;
            _sendingStream.SetLength(0);
            IEnumerable<ArraySegment<byte>> segments;
            while (_sendingQueue.TryDequeue(out segments))
            {
                Interlocked.Decrement(ref _pendingMessageCount);
                foreach (var segment in segments)
                {
                    _sendingStream.Write(segment.Array, segment.Offset, segment.Count);
                }
                if (_sendingStream.Length >= _setting.MaxSendPacketSize) break;

            }
            if (_sendingStream.Length == 0)
            {
                ExitSending();
                if (_sendingQueue.Count > 0)
                {
                    TrySend();
                }
                return;
            }

            try
            {
                _sendSocketArgs.SetBuffer(_sendingStream.GetBuffer(), 0, (int)_sendingStream.Length);
                var firedAsync = _sendSocketArgs.AcceptSocket.SendAsync(_sendSocketArgs);
                if (!firedAsync)
                {
                    ProcessSend(_sendSocketArgs);
                }
            }
            catch (Exception ex)
            {
                CloseInternal(SocketError.Shutdown, "socket send error:" + ex.Message, ex);
                ExitSending();
            }
        }
        private void ProcessSend(SocketAsyncEventArgs socketArgs)
        {
            if (_closing == 1) return;
            if (socketArgs.Buffer != null)
            {
                socketArgs.SetBuffer(null, 0, 0);
            }
            ExitSending();
            if (socketArgs.SocketError == SocketError.Success)
            {
                TrySend();
            }
            else
            {
                CloseInternal(socketArgs.SocketError, "socket send error", null);
            }
        }
        private void OnSendAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessSend(e);
        }
        private bool EnterSending()
        {
            return Interlocked.CompareExchange(ref _sending, 1, 0) == 0;
        }
        private void ExitSending()
        {
            Interlocked.Exchange(ref _sending, 0);
        }


        public IEnumerable<ArraySegment<byte>> FrameData(ArraySegment<byte> data)
        {
            yield return data;
        }
    }
}
