using Serilog.Events;

namespace J4JSoftware.FileHistory
{
    public class ConfigurationBase : IConfigurationBase
    {
        public int BackupDurationSeconds { get; set; }
        public int MaxStartupSeconds { get; set; }
        public LogEventLevel MinLogLevel { get; set; } = LogEventLevel.Information;
        public string ProjectRootPath { get; set; }
        public bool IncludeSource { get; set; }
        public bool IncludeAssemblyName { get; set; }
    }
}
