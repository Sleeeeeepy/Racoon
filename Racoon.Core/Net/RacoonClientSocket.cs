using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Racoon.Core.Correction;
using Racoon.Core.Enums;
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
            try
            {
                connect();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                Debug.WriteLine("An error occurred while connecting.");
            }
        }

        private void connect()
        {
            var iv = new byte[16];
            RandomNumberGenerator.Create().GetNonZeroBytes(iv);
            HandshakePacket body = new(context.KeyExchange.PublicKey, iv);
            PacketHeader header = new(context.Sequence, body, body.Length, Identifier);

            int bufferSize = EncodeHelper.GetBlockSize(header);
            var buffer = new byte[bufferSize];
            SerializationHelper.Serialize(buffer, header, body);
            EncodeHelper.EncodeWithoutHeader(buffer, header);
            udpClient.Send(buffer, context.LastIP, context.LastPort);

            var datagram = udpClient.Receive(ref remoteEndpoint);
            var decodedBytes = BlockEncoder.Decode(datagram.AsSpan(PacketHeader.HeaderSize));
            var recvHeader = PacketHeader.Deserialize(datagram, new());

            if (recvHeader?.PacketType == PacketType.ConnectionRefuse)
            {
                Debug.WriteLine("Connection refuse");
                Console.WriteLine("Connection refuse");
                return;
            }

            var recvBody = HandshakePacket.Deserialize(decodedBytes, new());

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
