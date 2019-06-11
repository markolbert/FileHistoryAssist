using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using J4JSoftware.Logging;

namespace J4JSoftware.FileHistory
{
    public class FileHistoryShareTarget : FileHistoryTarget, IFileHistoryTarget
    {
        private static readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        private static System.Timers.Timer _timer;

        public FileHistoryShareTarget( IJ4JLogger<FileHistoryShareTarget> logger, IShareConfiguration config ) 
            : base( (IJ4JLogger<FileHistoryTarget> ) logger, config )
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += _timer_Elapsed;
            Logger.Information("set up interval timer");
        }

        public override bool Initialize()
        {
            var config = (ShareConfiguration) Configuration;

            // set MAC address if it's undefined (this also updates config file)
            if( config.MacAddress.Equals( PhysicalAddress.None ) )
            {
                Logger.Information("Determining MAC address of server...");
                if( !DeterminePhysicalAddress( config ) ) return false;
            }

            if( IsServerOnline( config ) ) return true;

            Logger.Information("Sending wake-on-LAN packet...");
            WakeOnLAN( config );

            var untilTime = DateTime.Now + TimeSpan.FromSeconds( config.MaxStartupSeconds );

            Logger.Information( "Waiting until {untilTime} ({MaxStartupSeconds} seconds) for server to boot...",
                untilTime, config.MaxStartupSeconds );

            Wait( config.MaxStartupSeconds );

            return IsServerOnline( config );
        }

        protected bool IsServerOnline( ShareConfiguration config )
        {
            var pinger = new Ping();
            var retVal = false;

            try
            {
                var reply = pinger.Send(config.Server);

                if (reply.Status == IPStatus.Success)
                {
                    retVal = true;
                    Logger.Information("Server is up");
                }
                else Logger.Information("Server is not up");
            }
            catch (PingException e)
            {
                Logger.Information($"Ping failed, message was {e.Message}");
            }
            finally
            {
                pinger.Dispose();
            }

            return retVal;
        }

        protected void WakeOnLAN( ShareConfiguration config )
        {
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                EnableBroadcast = true
            };

            // The magic packet is a broadcast frame containing anywhere within its payload
            // 6 bytes of all 255 (FF FF FF FF FF FF in hexadecimal), followed by sixteen
            // repetitions of the target computer's 48-bit MAC address, for a total of 102 bytes.
            List<byte> payload = new List<byte>();

            // Add 6 bytes with value 255 (FF) in our payload
            for (var i = 0; i < 6; i++)
            {
                payload.Add(255);
            }

            // Repeat the device MAC address sixteen times
            var macBytes = config.MacAddress.GetAddressBytes();

            for (var j = 0; j < 16; j++)
            {
                foreach (var curByte in macBytes)
                {
                    payload.Add(curByte);
                }
            }

            // Broadcast our packet
            sock.SendTo(payload.ToArray(), new IPEndPoint(IPAddress.Parse("255.255.255.255"), 0));
            sock.Close(10000);
        }

        protected bool DeterminePhysicalAddress( ShareConfiguration config )
        {
            // if Server isn't already an IP address, do a DNS lookup to get the
            // Server's IP address
            var ipAddr = IPAddress.None;

            if (config.ServerIsIPAddress)
            {
                // Server is already an IP address
                Logger.Information<string>("Server property is an IP address ({Server})", config.Server);

                if (IPAddress.TryParse(config.Server, out IPAddress temp))
                {
                    ipAddr = temp;
                    Logger.Information("Parsed server IP address");
                }
                else
                {
                    Logger.Error("Failed to parse server IP address");
                    return false;
                }
            }
            else
            {
                // Server isn't an IP address, do a DNS lookup
                IPHostEntry ipEntry = Dns.GetHostEntry(config.Server);

                switch (ipEntry.AddressList.Length)
                {
                    case 0:
                        Logger.Error("DNS lookup of server failed");
                        return false;

                    case 1:
                        Logger.Information(
                            "Server resolved to a single IP address ({0})", propertyValue : ipEntry.AddressList[ 0 ] );
                        ipAddr = ipEntry.AddressList[0];
                        break;

                    default:
                        Logger.Information(
                            "Server resolved to {0} addresses, using the first one {1}",
                            propertyValue0 : ipEntry.AddressList.Length, propertyValue1 : ipEntry.AddressList[ 0 ] );

                        ipAddr = ipEntry.AddressList[0];
                        break;
                }
            }

            var macText = ipAddr.GetMacAddress();

            try
            {
                config.MacAddress = PhysicalAddress.Parse(macText);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return false;
            }

            return true;
        }

        private void Wait(int seconds)
        {
            if (seconds > 0)
            {
                _resetEvent.Reset();
                _timer.Interval = seconds * 1000;
                //ThreadPool.QueueUserWorkItem(state => { _resetEvent.WaitOne(); });
                _resetEvent.WaitOne();
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _resetEvent.Set();
        }
    }
}
