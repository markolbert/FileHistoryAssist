
// Copyright (c) 2019 Mark A. Olbert some rights reserved
//
// This software is licensed under the terms of the MIT License
// (https://opensource.org/licenses/MIT)

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using J4JSoftware.Logging;

namespace J4JSoftware.FileHistory
{
    /// <summary>
    /// Defines methods for interacting with the Windows File History service
    /// </summary>
    public class FileHistoryService : IDisposable, IFileHistoryService
    {
        public const string ServiceName = "fhsvc";
        private const string NoPipeError = "no pipe to File History Service";

        private static Guid FHCLSID = new Guid( "{F6B6E965-E9B2-444B-9286-10C9152EDBC5}" );

        private readonly IJ4JLogger<FileHistoryService> _logger = null;
        private readonly ServiceController _svcCtrl;

        private IntPtr _pipe = IntPtr.Zero;

        /// <summary>
        /// Creates an instance of the class, opening a connection to the Windows
        /// File History service.
        /// </summary>
        public FileHistoryService( IJ4JLogger<FileHistoryService> logger )
        {
            _logger = logger ?? throw new NullReferenceException( nameof(logger) );

            try
            {
                FhServiceOpenPipe( true, ref _pipe );

                if( _pipe.Equals( IntPtr.Zero ) )
                {
                    _logger.Fatal( NoPipeError );
                    throw new NullReferenceException( "failed to open pipe to File History Service" );
                }

                _logger.Information("Opened pipe to File History Service");

                _svcCtrl = new ServiceController( ServiceName );
                _logger.Information("Created service controller");
            }
            catch ( Exception e )
            {
                var error = FileHistoryErrors.GetError( e );
                _logger.Fatal( error );

                throw e;
            }
        }

        /// <summary>
        /// Starts a backup by the Windows File History service
        /// </summary>
        /// <param name="lowPriority">flag indicating whether or not the backup should
        /// be considered low priority by the system; defaults to true</param>
        public void Start( bool lowPriority = true )
        {
            FhServiceStartBackup( _pipe, lowPriority );
            _logger.Information("Started a File History backup");
        }

        /// <summary>
        /// Stops the current Windows File History service backup
        /// </summary>
        public void Stop()
        {
            FhServiceStopBackup( _pipe, false );
            _logger.Information("Stopped File History backup");
        }

        public void ReloadConfiguration()
        {
            FhServiceReloadConfiguration( ref _pipe );
            _logger.Information("Reloaded File History configuration");
        }

        private void ReleaseUnmanagedResources()
        {
            if (!_pipe.Equals(IntPtr.Zero))
                FhServiceClosePipe(_pipe);
        }

        #region File History external methods

        [DllImport("FhSvcCtl.dll")]
        private static extern int FhServiceClosePipe([In] IntPtr pipe);

        [DllImport("FhSvcCtl.dll")]
        private static extern int FhServiceOpenPipe([In] bool startServiceIfStopped, ref IntPtr pipe);

        [DllImport("FhSvcCtl.dll")]
        private static extern int FhServiceReloadConfiguration([In] ref IntPtr pipe);

        [DllImport("FhSvcCtl.dll")]
        private static extern int FhServiceBlockBackup([In] ref IntPtr pipe);

        [DllImport("FhSvcCtl.dll")]
        private static extern int FhServiceStartBackup([In] IntPtr pipe, [In] bool lowPriorityIo);

        [DllImport("FhSvcCtl.dll")]
        private static extern int FhServiceStopBackup([In] IntPtr pipe, [In] bool stopTracking);

        [DllImport("FhSvcCtl.dll")]
        private static extern int FhServiceUnblockBackup([In] IntPtr pipe);

        [DllImport("Iphlpapi.dll")]
        private static extern int SendARP(Int32 dest, Int32 host, ref Int64 mac, ref Int32 len);

        #endregion

        /// <summary>
        /// Disposes of the unmanaged resources used to interface with the Windows
        /// File History service, by closing the connection to it
        /// </summary>
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize( this );
        }

        ~FileHistoryService()
        {
            ReleaseUnmanagedResources();
        }

    }
}
