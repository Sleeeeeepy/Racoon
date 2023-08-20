using System.Net;
using System.Net.Sockets;
using Racoon.Core.Correction;
using Racoon.Core.Packet;
using Racoon.Core.Enums;

namespace Racoon.Core.Net
{
    public class RacoonSocket
    {
        private Dictionary<string, ConnectionContext> contexts;
        private UdpClient udpClient;
        private IPEndPoint remoteEndpoint;
        private IPacketHandler packetHandler;

        public RacoonSocket(int localPort)
        {
            contexts = new Dictionary<string, ConnectionContext>();
            udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, localPort));
            remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            packetHandler = IPacketHandler.GetDefaultPacketHandler();
        }

        public void Receive()
        {
            var datagram = udpClient.Receive(ref remoteEndpoint);
            if (datagram == null || datagram.Length <= 0)
            {
                return;
            }

            // 헤더 만큼도 못받아오면 로스
            if (datagram.Length < PacketBase.HeaderSize)
            {
                return;
            }

            var handle = (byte[] dgram) =>
            {
                var decoded = BlockEncoder.Decode(dgram);
                var header = PacketBase.Deserialize(decoded, new PacketBase());
                
                if (header is null)
                {
                    return;
                }

                IPacket? body = default;
                switch (header.PacketType)
                {
                    case PacketType.ConnectionRequest:
                        break;
                    case PacketType.Ping:
                        break;
                    case PacketType.Pong:
                        break;
                    case PacketType.Normal:
                        break;
                }

                if (body is null)
                {
                    return;
                }

                packetHandler.HandlePacket(header, body);
            };

            Task.Run(() => handle(datagram));
        }

        private void beginConnection(string ip, string port)
        {

        }
    }
}
