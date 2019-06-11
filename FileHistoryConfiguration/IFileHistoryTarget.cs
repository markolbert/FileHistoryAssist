using Serilog;

namespace J4JSoftware.FileHistory
{
    public interface IFileHistoryTarget
    { 
        bool Initialize();
        IConfigurationBase Configuration { get; }
    }
}
