using Racoon.Core.Cryptography;

namespace Racoon.Core.Net
{
    public class ConnectionContext
    {
        public string Identifier { get; set; }
        public int Sequence { get; set; }
        public string LastIP { get; set; }
        public int LastPort { get; set; }
        public KeyExchangeContext KeyExchange { get; private set; }
        public AesCryptography? AesCryptography { get; private set; }
        public bool IsConnected { get; private set; }
        public ConnectionContext()
        {
            this.Identifier = string.Empty;
            this.Sequence = 0;
            this.LastIP = string.Empty;
            this.LastPort = 0;
            KeyExchange = new KeyExchangeContext();
            IsConnected = false;
        }

        public void OnConnection(byte[] publicKey, byte[] initializeVector)
        {
            KeyExchange.ReceiveKey(publicKey);
            AesCryptography = new AesCryptography(KeyExchange.SharedKey, initializeVector);
            IsConnected = true;
        }
    }
}
