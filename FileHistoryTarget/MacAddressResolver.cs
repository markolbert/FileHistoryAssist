using System;
using System.Net;
using System.Runtime.InteropServices;

namespace J4JSoftware.FileHistory
{
    public static class MacAddressResolver
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int DestIP, int SrcIP, [Out] byte[] pMacAddr, ref int PhyAddrLen);

        public static string GetMacAddress( this IPAddress ipAddress )
        {
            if( ipAddress == null ) return String.Empty;

            byte[] buffer = new byte[6];
            int bufferLength = buffer.Length;
            int numAddr = BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0);

            SendARP(numAddr, 0, buffer, ref bufferLength);

            return BitConverter.ToString( buffer, 0, bufferLength );
        }
    }
}