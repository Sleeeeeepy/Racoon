using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Racoon.Core.Correction;
using Racoon.Core.Packet;
using Racoon.Core.Util;

namespace Racoon.Core.Net
{
    public class RacoonClientSocket
    {
        private ConnectionContext context;
        private byte[] Identifier;
        private UdpClient udpClient;
        private IPEndPoint remoteEndpoint;

        public RacoonClientSocket(string ip, int port)
        {
            udpClient = new UdpClient();
            context = new()
            {
                LastIP = ip,
                LastPort = port
            };
            Identifier = Guid.NewGuid().ToByteArray();
            remoteEndpoint = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public void Send()
        {

        }

        public void Receive()
        {

        }

        public void Connect()
        {
            var iv = new byte[16];
            RandomNumberGenerator.Create().GetNonZeroBytes(iv);
            HandshakePacket packet = new(context.KeyExchange.PublicKey, iv);
            PacketBase header = new(context.Sequence, packet, packet.Length, Identifier);
            
            var buffer = new byte[PacketBase.HeaderSize + 255];
            int endIndex = PacketBase.HeaderSize + packet.Length;
            header.Serialize(buffer, 0);
            packet.Serialize(buffer, PacketBase.HeaderSize);
            BlockEncoder.Encode(buffer.AsSpan()[PacketBase.HeaderSize..endIndex], buffer.AsSpan()[PacketBase.HeaderSize..]);
            udpClient.Send(buffer, context.LastIP, context.LastPort);

            var datagram = udpClient.Receive(ref remoteEndpoint);
            var decodedBytes = BlockEncoder.Decode(datagram.AsSpan(PacketBase.HeaderSize));
            var recvHeader = PacketBase.Deserialize(decodedBytes, new());
            var recvBody = HandshakePacket.Deserialize(decodedBytes[PacketBase.HeaderSize..], new());

            if (recvHeader is null || recvBody is null)
            {
                Debug.WriteLine("An error occurred while connecting.");
                return;
            }

            context.Identifier = recvHeader.Identifier.ToHexString();
            context.OnConnection(recvBody.PublicKey, recvBody.InitializeVector);
            Debug.WriteLine("Connection done.");
        }
    }
}
