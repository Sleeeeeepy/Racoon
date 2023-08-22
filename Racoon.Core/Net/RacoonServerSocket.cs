using System.Net;
using System.Net.Sockets;
using Racoon.Core.Correction;
using Racoon.Core.Packet;
using Racoon.Core.Enums;
using System.Diagnostics;
using Racoon.Core.Util;
using System.Security.Cryptography;

namespace Racoon.Core.Net
{
    public class RacoonServerSocket
    {
        private Dictionary<string, ConnectionContext> contexts;
        private UdpClient udpClient;
        private IPEndPoint remoteEndpoint;
        private IPacketHandler packetHandler;
        private byte[] Identifier;
        private bool Closed = false;

        public RacoonServerSocket(string ip, int localPort)
        {
            contexts = new Dictionary<string, ConnectionContext>();
            udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(ip), localPort));
            remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            packetHandler = IPacketHandler.GetDefaultPacketHandler();
            Identifier = new Guid().ToByteArray();
        }

        public void Listen()
        {
            while (true)
            {
                if (Closed)
                    break;
                Receive();
            }
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
                var header = PacketBase.Deserialize(dgram, new PacketBase());
                Console.WriteLine(header?.PacketType);
                BlockEncoder.Decode(dgram.AsSpan()[PacketBase.HeaderSize..]);
                if (header is null)
                {
                    Debug.WriteLine($"[{DateTime.UtcNow}] {remoteEndpoint.Address} - Header is null.");
                    return;
                }

                IPacket? body = default;
                if (header.PacketType == PacketType.ConnectionRequest)
                {
                    body = HandshakePacket.Deserialize(dgram.AsSpan()[PacketBase.HeaderSize..], new HandshakePacket());
                    if (body is null) 
                        return;
                    beginConnection(remoteEndpoint.Address.ToString(), remoteEndpoint.Port, header.Identifier, (HandshakePacket)body);
                    return;
                }

                // Get context
                string connectionId = header.Identifier.ToHexString();
                if (!contexts.TryGetValue(connectionId, out var context))
                {
                    Debug.WriteLine($"[{DateTime.UtcNow}] {remoteEndpoint.Address} - Unkown user {connectionId}");
                    return;
                }

                if (!context.IsConnected || context.AesCryptography is null)
                {
                    Debug.WriteLine($"[{DateTime.UtcNow}] {remoteEndpoint.Address} - User {connectionId} sends a packet before key exchange.");
                    return;
                }

                var dcrypted = context.AesCryptography.Decrypt(dgram[PacketBase.HeaderSize..]);
                switch (header.PacketType)
                {
                    case PacketType.Ping:
                        body = PingPacket.Deserialize(dcrypted.AsSpan()[PacketBase.HeaderSize..], new PingPacket());
                        break;
                    case PacketType.Pong:
                        body = PongPacket.Deserialize(dcrypted.AsSpan()[PacketBase.HeaderSize..], new PongPacket());
                        break;
                    case PacketType.Normal:
                        body = NormalPacket.Deserialize(dcrypted.AsSpan()[PacketBase.HeaderSize..], new NormalPacket());
                        packetHandler.HandlePacket(context, header, body);
                        break;
                }

                Debug.WriteLine($"[{DateTime.UtcNow}] {remoteEndpoint.Address} - Unkown packet type.");
            };

            Task.Run(() => handle(datagram));
        }

        private void beginConnection(string ip, int port, byte[] identifier, HandshakePacket packet)
        {
            var connectionId = identifier.ToHexString();
            if (contexts.ContainsKey(connectionId))
            {
                Debug.WriteLine($"{connectionId} is already connected.");
                return;
            }

            var context = new ConnectionContext
            {
                Identifier = connectionId,
                LastIP = ip,
                LastPort = port
            };
            context.OnConnection(packet.PublicKey, packet.InitializeVector);
            contexts.Add(context.Identifier, context);

            HandshakePacket sendPacket = new(context.KeyExchange.PublicKey, packet.InitializeVector);
            PacketBase header = new(context.Sequence, sendPacket, sendPacket.Length, Identifier);

            // TODO: global send buffer 이용하기
            var buffer = new byte[PacketBase.HeaderSize + 255];
            int endIndex = PacketBase.HeaderSize + sendPacket.Length;
            header.Serialize(buffer, 0);
            sendPacket.Serialize(buffer, PacketBase.HeaderSize);
            BlockEncoder.Encode(buffer.AsSpan()[PacketBase.HeaderSize..endIndex], buffer.AsSpan()[PacketBase.HeaderSize..]);
            
            udpClient.Send(buffer, ip, port);
            Debug.WriteLine($"Send conntection data to {connectionId}.");
        }
    }
}
