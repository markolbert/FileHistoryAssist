namespace J4JSoftware.FileHistory
{
    public interface IFileHistoryService
    {
        /// <summary>
        /// Starts a backup by the Windows File History service
        /// </summary>
        /// <param name="lowPriority">flag indicating whether or not the backup should
        /// be considered low priority by the system; defaults to true</param>
        void Start( bool lowPriority = true );

        /// <summary>
        /// Stops the current Windows File History service backup
        /// </summary>
        void Stop();

        void ReloadConfiguration();

        /// <summary>
        /// Disposes of the unmanaged resources used to interface with the Windows
        /// File History service, by closing the connection to it
        /// </summary>
        void Dispose();
    }
}