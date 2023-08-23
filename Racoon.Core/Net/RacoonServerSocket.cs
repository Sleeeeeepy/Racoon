using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Racoon.Core.Correction;
using Racoon.Core.Enums;
using Racoon.Core.Packet;
using Racoon.Core.Util;

namespace Racoon.Core.Net
{
    public class RacoonServerSocket
    {
        private readonly Dictionary<string, ConnectionContext> contexts;
        private readonly UdpClient udpClient;
        private IPEndPoint remoteEndpoint;
        private readonly IPacketHandler packetHandler;
        private readonly byte[] Identifier;
        private bool Closed = false;

        public RacoonServerSocket(string ip, int localPort)
        {
            contexts = new Dictionary<string, ConnectionContext>();
            udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(ip), localPort));
            remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            packetHandler = IPacketHandler.GetDefaultPacketHandler();
            Identifier = Guid.NewGuid().ToByteArray();
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
            if (datagram.Length < PacketHeader.HeaderSize)
            {
                return;
            }

            var handle = (byte[] dgram) =>
            {
                var header = PacketHeader.Deserialize(dgram, new PacketHeader());
                Console.WriteLine(header?.PacketType);
                BlockEncoder.Decode(dgram.AsSpan()[PacketHeader.HeaderSize..]);
                if (header is null)
                {
                    Debug.WriteLine($"[{DateTime.UtcNow}] {remoteEndpoint.Address} - Header is null.");
                    return;
                }

                IPacket? body = default;
                if (header.PacketType == PacketType.ConnectionRequest)
                {
                    body = HandshakePacket.Deserialize(dgram.AsSpan()[PacketHeader.HeaderSize..], new HandshakePacket());
                    if (body is null)
                    {
                        sendConnectionRefusal(remoteEndpoint.Address.ToString(), remoteEndpoint.Port, header.Identifier);
                        return;
                    }

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

                var dcrypted = context.AesCryptography.Decrypt(dgram[PacketHeader.HeaderSize..]);
                switch (header.PacketType)
                {
                    case PacketType.Ping:
                        body = PingPacket.Deserialize(dcrypted.AsSpan()[PacketHeader.HeaderSize..], new PingPacket());
                        break;
                    case PacketType.Pong:
                        body = PongPacket.Deserialize(dcrypted.AsSpan()[PacketHeader.HeaderSize..], new PongPacket());
                        break;
                    case PacketType.Normal:
                        body = NormalPacket.Deserialize(dcrypted.AsSpan()[PacketHeader.HeaderSize..], new NormalPacket());
                        packetHandler.HandlePacket(context, header, body);
                        break;
                }

                Debug.WriteLine($"[{DateTime.UtcNow}] {remoteEndpoint.Address} - Unkown packet type.");
            };

            Task.Run(() => handle(datagram));
        }

        private void beginConnection(string ip, int port, byte[] identifier, HandshakePacket receivedPacket)
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

            context.OnConnection(receivedPacket.PublicKey, receivedPacket.InitializeVector);
            contexts.Add(context.Identifier, context);

            HandshakePacket sendPacket = new(context.KeyExchange.PublicKey, receivedPacket.InitializeVector);
            PacketHeader header = new(context.Sequence, sendPacket, sendPacket.Length, Identifier);

            // TODO: global send buffer 이용하기
            var buffer = new byte[EncodeHelper.GetBlockSize(header)];
            SerializationHelper.Serialize(buffer, header, sendPacket);
            EncodeHelper.EncodeWithoutHeader(buffer, header);

            udpClient.Send(buffer, ip, port);
            Debug.WriteLine($"Send conntection data to {connectionId}.");

            foreach (var b in context.KeyExchange.SharedKey)
            {
                Console.Write(b.ToString("X2"));
            }
        }

        private void sendConnectionRefusal(string ip, int port, byte[]? identifier)
        {
            var connectionId = identifier?.ToHexString();
            PacketHeader header = new(0, PacketType.ConnectionRefuse, 0, false, this.Identifier, 0);
            var buffer = new byte[PacketHeader.HeaderSize];
            header.Serialize(buffer, 0);
            udpClient.Send(buffer, ip, port);
            if (identifier is null)
            {
                Debug.WriteLine($"Send connection refusal to {ip}.");
                return;
            }
            Debug.WriteLine($"Send connection refusal to {connectionId}.");
        }
    }
}
