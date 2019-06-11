using System.Net.NetworkInformation;
using Serilog.Events;

namespace J4JSoftware.FileHistory
{
    public interface IShareConfiguration : IConfigurationBase
    {
        string Server { get; set; }
        bool ServerIsIPAddress { get; }
        PhysicalAddress MacAddress { get; set; }
    }
}