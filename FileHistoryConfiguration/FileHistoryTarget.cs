using System;
using J4JSoftware.Logging;

namespace J4JSoftware.FileHistory
{
    public abstract class FileHistoryTarget : IFileHistoryTarget
    {
        protected FileHistoryTarget( IJ4JLogger<FileHistoryTarget> logger, IConfigurationBase config )
        {
            Logger = logger ?? throw new NullReferenceException( nameof(logger) );
            Configuration = config ?? throw new NullReferenceException( nameof(config) );
        }

        protected IJ4JLogger<FileHistoryTarget> Logger { get; }
        public IConfigurationBase Configuration { get; }

        public abstract bool Initialize();
    }
}