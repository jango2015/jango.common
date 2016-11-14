using Jango.Common.NetWork.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest_client
{
    class Program
    {
        static void Main(string[] args)
        {
            var serverEndPoint = new IPEndPoint(SocketUtils.GetLocalIPV4(), 5000);
            //var localEndPoint = new IPEndPoint(SocketUtils.GetLocalIPV4(), 5001);
            //var setting = new SocketSetting();
            //var client = new ClientSocket(serverEndPoint, localEndPoint, setting);

            var sendMsg = "this is the client send msg";
            Jango.TCPClient.ClientSocket.Connect();
            Jango.TCPClient.ClientSocket.Send(sendMsg);
            //client.QueueMessage(System.Text.Encoding.Default.GetBytes(sendMsg));
            Console.ReadLine();
        }
    }
}
