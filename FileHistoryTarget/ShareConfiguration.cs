using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Newtonsoft.Json;

namespace J4JSoftware.FileHistory
{
    public class ShareConfiguration : ConfigurationBase, IShareConfiguration
    {
        public static ShareConfiguration Default { get; } = new ShareConfiguration()
        {
            BackupDurationSeconds = 600,
            MacAddress = PhysicalAddress.None,
            MaxStartupSeconds = 120,
            Server = String.Empty
        };

        public string Server { get; set; }

        public bool ServerIsIPAddress => IPAddress.TryParse( Server, out IPAddress address );

        [ JsonConverter( typeof(JsonMacConverter) ) ]
        public PhysicalAddress MacAddress { get; set; } = PhysicalAddress.None;
    }
}
