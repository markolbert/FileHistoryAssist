using Serilog.Events;

namespace J4JSoftware.FileHistory
{
    public interface IConfigurationBase
    {
        int BackupDurationSeconds { get; set; }
        int MaxStartupSeconds { get; set; }
        LogEventLevel MinLogLevel { get; set; }
        string ProjectRootPath { get; set; }
        bool IncludeSource { get; set; }
        bool IncludeAssemblyName { get; set; }
    }
}