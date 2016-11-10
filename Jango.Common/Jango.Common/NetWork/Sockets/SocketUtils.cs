namespace Jango.Common.NetWork.Sockets
{
    using System.Net;
    using System.Linq;
    using System.Net.Sockets;
    using Utilities;
    public class SocketUtils
    {
        #region IPAddress
        public static IPAddress GetLocalIPV4()
        {
            return Dns.GetHostEntry(GetLocalHostName()).AddressList.First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }

        public static IPAddress GetLocalIPV6()
        {
            return Dns.GetHostEntry(GetLocalHostName()).AddressList.First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
        }

        public static IPAddress[] GetLocalIPs()
        {
            return Dns.GetHostEntry(GetLocalHostName()).AddressList;
        }
        public static string GetLocalHostName()
        {
            return Dns.GetHostName();
        }
        #endregion

        /**
        //SocketType.Dgram
        //支持数据报，即最大长度固定（通常很小）的无连接、不可靠消息。消息可能会丢失或重复并可能在到达时不按顺序排列。 Dgram 类型的 Socket 在发送和接收数据之前不需要任何连接，并且可以与多个对方主机进行通信。 Dgram 使用数据报协议(Udp) 和 InterNetworkAddressFamily。
        //SocketType.Raw
        //支持对基础传输协议的访问。通过使用 SocketTypeRaw，可以使用 Internet 控制消息协议(Icmp) 和 Internet 组管理协议(Igmp) 这样的协议来进行通信。在发送时，您的应用程序必须提供完整的 IP 标头。所接收的数据报在返回时会保持其 IP 标头和选项不变。
        //SocketType.Rdm
        //支持无连接、面向消息、以可靠方式发送的消息，并保留数据中的消息边界。RDM（以可靠方式发送的消息）消息会依次到达，不会重复。此外，如果消息丢失，将会通知发送方。如果使用 Rdm 初始化 Socket，则在发送和接收数据之前无需建立远程主机连接。利用 Rdm，您可以与多个对方主机进行通信。
        //SocketType.Seqpacket
        //在网络上提供排序字节流的面向连接且可靠的双向传输。 Seqpacket 不重复数据，它在数据流中保留边界。 Seqpacket 类型的 Socket 与单个对方主机通信，并且在通信开始之前需要建立远程主机连接。
        //SocketType.Stream
        //支持可靠、双向、基于连接的字节流，而不重复数据，也不保留边界。此类型的 Socket 与单个对方主机通信，并且在通信开始之前需要建立远程主机连接。 Stream 使用传输控制协议(Tcp) ProtocolType 和 InterNetworkAddressFamily。
        //SocketType.Unknown
        //指定未知的 Socket 类型。
        */
        public static Socket CreateSocket()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.Blocking = false;
            //socket.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted);
            //SocketOptionLevel.IP          Socket 选项仅适用于 IP 套接字。
            //SocketOptionLevel.IPv6        Socket 选项仅适用于 IPv6 套接字。
            //SocketOptionLevel.Socket      Socket 选项适用于所有套接字。
            //SocketOptionLevel.Tcp         Socket 选项仅适用于 TCP 套接字。
            //SocketOptionLevel.Udp         Socket 选项仅适用于 UDP 套接字。

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            return socket;
        }

        public static void ShudownSocket(Socket socket)
        {
            if (socket == null) return;
            Exceptions.Eat(() => socket.Shutdown(SocketShutdown.Both));
            Exceptions.Eat(() => socket.Close(10000));
        }
        
    }
}
