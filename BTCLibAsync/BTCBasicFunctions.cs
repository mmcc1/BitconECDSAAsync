using NBitcoin;
using System;
using System.Threading.Tasks;

namespace BTCLibAsync
{
    public class BTCKeyStore
    {
        public byte[] PrivateKeyByteArray { get; set; }
        public string PrivateKeyHex { get; set; }
        public string PrivateKeyWIF { get; set; }
        public string PublicAddress { get; set; }
        public bool IsCompressed { get; set; }
    }

    //Uses Nbitcoin
    public static class BTCBasicFunctions
    {

        public static Task<BTCKeyStore> CreateKeyPair()
        {
            BTCKeyStore btcKS = new BTCKeyStore();

            Key privateKey = new Key();
            btcKS.PrivateKeyByteArray = privateKey.ToBytes();
            BitcoinSecret mainNetPrivateKey = privateKey.GetBitcoinSecret(Network.Main);

            btcKS.PrivateKeyHex = mainNetPrivateKey.PrivateKey.ToHex();
            btcKS.PrivateKeyWIF = mainNetPrivateKey.ToWif();
            btcKS.PublicAddress = mainNetPrivateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main).ToString();
            btcKS.IsCompressed = mainNetPrivateKey.IsCompressed;

            return Task.FromResult(btcKS);
        }

        public static Task<BTCKeyStore> CreateKeyPair(byte[] privateKeyBytes)
        {
            BTCKeyStore btcKS = new BTCKeyStore();

            Key privateKey = new Key(privateKeyBytes);
            btcKS.PrivateKeyByteArray = privateKey.ToBytes();
            BitcoinSecret mainNetPrivateKey = privateKey.GetBitcoinSecret(Network.Main);

            btcKS.PrivateKeyHex = mainNetPrivateKey.PrivateKey.ToHex();
            btcKS.PrivateKeyWIF = mainNetPrivateKey.ToWif();
            btcKS.PublicAddress = mainNetPrivateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main).ToString();
            btcKS.IsCompressed = mainNetPrivateKey.IsCompressed;

            return Task.FromResult(btcKS);
        }
    }
}
