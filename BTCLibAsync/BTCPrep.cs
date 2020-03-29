using B58CheckAsync;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BTCLibAsync
{
    public static class BTCPrep
    {
        public static async Task<byte[]> PrepareAddress(string publicAddress)
        {
            //For now, only work with addresses of prefix '1'.
            if (await BTCInfo.DetermineAddressType(publicAddress) != AddressType.PubKeyHashP2PKH)
                return null;

            return await PreparePubKeyHashP2PKH(publicAddress);
        }

        public static async Task<byte[]> PreparePubKeyHashP2PKH(string publicAddress)
        {
            byte[] result = await Base58CheckEncoding.Decode(publicAddress);
            byte[] removedPrefix = new byte[result.Length - 1];
            Array.Copy(result, 1, removedPrefix, 0, removedPrefix.Length);
            return removedPrefix;
        }
    }
}
