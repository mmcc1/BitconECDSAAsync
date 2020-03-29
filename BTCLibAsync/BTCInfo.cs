using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BTCLibAsync
{
    public enum AddressType
    {
        PubKeyHashP2PKH,
        ScriptHashP2SH,
        Bech32,
        Unknown
    }

    public static class BTCInfo
    {
        public static Task<AddressType> DetermineAddressType(string publicAddress)
        {
            if (publicAddress.StartsWith("1"))
                return Task.FromResult(AddressType.PubKeyHashP2PKH);
            else if (publicAddress.StartsWith("3"))
                return Task.FromResult(AddressType.ScriptHashP2SH);
            else if (publicAddress.StartsWith("bc1"))
                return Task.FromResult(AddressType.Bech32);
            else
                return Task.FromResult(AddressType.Unknown);
        }

        public static Task<bool> ComparePublicAddresses(string pubAdd1, string pubAdd2)
        {
            return Task.FromResult(pubAdd1 == pubAdd2 ? true : false);
        }
    }
}
