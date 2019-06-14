using System;
using System.IO;
using J4JSoftware.FileHistory;
using J4JSoftware.Logging;
using Serilog;

namespace FileHistoryService
{
    public class J4JSmsLoggerConfiguration : IJ4JSmsLoggerConfiguration
    {
        public J4JSmsLoggerConfiguration( IShareConfiguration config, ITwilioConfig twilio )
        {
            if( config == null )
                throw new NullReferenceException( nameof(config) );

            SmsWriter = new StringWriter();
            SourceRootPath = config.ProjectRootPath;
            IncludeSource = config.IncludeSource;
            IncludeAssemblyName = config.IncludeAssemblyName;

            SourceMessageTemplate = "({File}:{Line})";
            MemberMessageTemplate = "{SourceContext}::{Member}";
        }

        public string SourceMessageTemplate { get; }
        public string MemberMessageTemplate { get; }
        public StringWriter SmsWriter { get; }
        public string SourceRootPath { get; }
        public bool IncludeSource { get; }
        public bool IncludeAssemblyName { get; }
    }
}
