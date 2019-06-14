using System;
using System.IO;
using System.Threading;
using System.Timers;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.FileHistory;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Twilio.Types;

namespace FileHistoryService
{
    class Program
    {
        private static readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        private static AutofacServiceProvider _svcProvider;
        private static System.Timers.Timer _timer;
        private static IJ4JSmsLogger<Program> _logger;
        private static IFileHistoryConfiguration _fhConfig;
        private static IFileHistoryTarget _target;

        static void Main(string[] args)
        {
            ConfigureServices(args);

            _logger = _svcProvider.GetRequiredService<IJ4JSmsLogger<Program>>();

            Console.CancelKeyPress += Console_CancelKeyPress;
            _logger.IncludeSource().Information("set up CancelKeyPress handler");

            _timer = new System.Timers.Timer();
            _timer.Elapsed += _timer_Elapsed;
            _logger.Information("set up interval timer");

            _fhConfig = _svcProvider.GetRequiredService<IFileHistoryConfiguration>();
            _fhConfig.UpdateProtectionStatus();

            _target = _svcProvider.GetRequiredService<IFileHistoryTarget>();

            while( true )
            {
                if( _fhConfig.SecondsSinceLastBackup > _fhConfig.SecondsBetweenBackups )
                {
                    // missed a backup; initialize target and do a backup
                    _logger.Information( "Backup overdue, initializing..." );

                    if( !_target.Initialize() )
                    {
                        _logger.Information("Backup target initialization failed");
                        break;
                    }

                    _logger.Information("Backup target initialized");

                    var fhCtrl = _svcProvider.GetRequiredService<IFileHistoryService>();
                    _logger.Information("Starting backup...");
                    fhCtrl.Start();

                    var complChecks = 0;

                    while( complChecks < 3 )
                    {
                        var sleepFor = TimeSpan.FromSeconds(_target.Configuration.BackupDurationSeconds);
                        var sleepUntil = DateTime.Now + sleepFor;

                        _logger.Information("Waiting until {sleepUntil} (duration: {sleepFor}) for backup to complete...", sleepUntil, sleepFor);

                        Wait( _target.Configuration.BackupDurationSeconds );

                        _fhConfig.UpdateProtectionStatus();

                        if ( _fhConfig.BackupIsRunning ) _logger.Information("Backup still running...");
                        else
                        {
                            _logger.Information("Backup completed");
                            break;
                        }

                        complChecks++;
                    }

                    if( complChecks >= 3 )
                    {
                        _logger.Warning( "Backup taking {complChecks}x as long as expected, terminating", complChecks );
                        break;
                    }
                }

                // sleep until after the next backup, allowing for the 
                // time needed to do the backup itself
                var sleepFor2 = _fhConfig.SecondsBetweenBackups - _fhConfig.SecondsSinceLastBackup +
                                 TimeSpan.FromSeconds( _target.Configuration.BackupDurationSeconds );
                var sleepUntil2 = DateTime.Now + sleepFor2;

                if( sleepFor2 > TimeSpan.Zero )
                {
                    _logger.Information( "Sleeping until {sleepUntil2} (duration: {sleepFor2})...", sleepUntil2, sleepFor2 );

                    Wait( (int) sleepFor2.TotalSeconds );

                    _logger.Information("Woke from sleep");
                }
            }
        }

        private static void Wait( int seconds )
        {
            if( seconds > 0 )
            {
                _resetEvent.Reset();
                _timer.Interval = seconds * 1000;
                _resetEvent.WaitOne();
            }
        }

        private static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _resetEvent.Set();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _logger.SendSms().Information( "User hit control-C");
            Environment.Exit(0);
        }

        private static void ConfigureServices(string[] args)
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.Register<ShareConfiguration>( ( c, p ) =>
                {
                    var retVal = new ConfigurationBuilder()
                        .SetBasePath( Environment.CurrentDirectory )
                        .AddJsonFile( "config.json" )
                        .Build()
                        .Get<ShareConfiguration>();

                    return retVal;
                } )
                .As<IShareConfiguration>()
                .SingleInstance();

            containerBuilder.Register<TwilioConfig>( ( c, p ) =>
                {
                    var config = new ConfigurationBuilder()
                        .AddUserSecrets<TwilioConfig>()
                        .Build();

                    var retVal = new TwilioConfig();
                    config.GetSection("TwilioConfig").Bind(retVal);

                    retVal.Recipients.Add(new PhoneNumber("+1 (650) 868-3367"));

                    return retVal;
                })
                .As<ITwilioConfig>()
                .SingleInstance();

            containerBuilder.RegisterType<J4JSmsLoggerConfiguration>()
                .As<IJ4JSmsLoggerConfiguration>()
                .SingleInstance();

            containerBuilder.Register<ILogger>( ( c, p ) =>
            {
                var appConfig = c.Resolve<IShareConfiguration>();
                var j4jLoggerConfig = c.Resolve<IJ4JSmsLoggerConfiguration>();

                return new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        restrictedToMinimumLevel : appConfig.MinLogLevel
                    )
                    .WriteTo.File(
                        path : J4JLoggingExtensions.DefineLocalAppDataLogPath( "log.txt" ),
                        restrictedToMinimumLevel : appConfig.MinLogLevel
                    )
                    .WriteTo.TextWriter(
                        textWriter : j4jLoggerConfig.SmsWriter,
                        restrictedToMinimumLevel : appConfig.MinLogLevel
                    )
                    .CreateLogger();
            } );

            containerBuilder.RegisterType<J4JTwilio>()
                .As<IJ4JSms>()
                .SingleInstance();

            containerBuilder.RegisterGeneric( typeof(J4JSmsLogger<>) )
                .As( typeof(IJ4JSmsLogger<>) )
                .As( typeof(IJ4JLogger<>) )
                .SingleInstance();

            containerBuilder.RegisterModule<FileHistoryModule>();
            containerBuilder.RegisterModule<FileHistoryShareTargetModule>();

            var container = containerBuilder.Build();
            _svcProvider = new AutofacServiceProvider(container);
        }
    }
}
