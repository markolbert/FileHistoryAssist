using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;
using Serilog;

namespace J4JSoftware.FileHistory
{
    public class FileHistoryConfiguration : IFileHistoryConfiguration
    {
        private const string NoConfigError = "no file history configuration COM object";

        private readonly IJ4JLogger<FileHistoryConfiguration> _logger;
        private readonly IFHManager _fhMgr;
        private readonly FileHistoryService _fhSvc;

        private List<FileHistoryRule> _rules;

        public FileHistoryConfiguration( IJ4JLogger<FileHistoryConfiguration> histLogger, IJ4JLogger<FileHistoryService> svcLogger )
        {
            _logger = histLogger ?? throw new NullReferenceException( nameof(histLogger) );

            if( svcLogger == null ) throw new NullReferenceException( nameof(svcLogger) );

            _fhMgr = new FHManager() as IFHManager;
            if( _fhMgr != null )
            {
                try
                {
                    _fhMgr.LoadConfiguration();

                    _logger.Information( "Loaded File History configuration" );
                    _logger.Information(
                        "Backing up to {TargetName} ({TargetUrl}) -- {TargetType}", TargetName, TargetUrl, propertyValue2:TargetType );
                }
                catch( Exception e )
                {
                    var error = FileHistoryErrors.GetError( e );
                    _logger.Fatal( error );

                    throw new NullReferenceException( "Could not load File History configuration" );
                }
            }
            else
            {
                _logger.Error( NoConfigError );
                throw new NullReferenceException( "Could not load File History configuration" );
            }

            _fhSvc = new FileHistoryService( svcLogger );
        }

        public void CreateConfiguration( bool overwrite )
        {
            try
            {
                _fhMgr.CreateDefaultConfiguration( overwrite );
                _logger.Information( "Created default configuration" );
            }
            catch( Exception e )
            {
                var error = FileHistoryErrors.GetError(e);
                _logger.Error(error);

            }

            _rules = null; // force regeneration
            _logger.Information( "Cleared list of rules" );
        }

        public void SaveConfiguration()
        {
            try
            {
                _fhMgr.SaveConfiguration();
                _logger.Information( "Saved configuration" );
            }
            catch( Exception e )
            {
                var error = FileHistoryErrors.GetError(e);
                _logger.Error(error);
            }

            _rules = null; // force regeneration
            _logger.Information( "Cleared list of rules" );
        }

        public IReadOnlyList<FileHistoryRule> Rules
        {
            get
            {
                if( _rules == null )
                {
                    _logger.Information( "Regenerating list of rules" );
                    _rules = new List<FileHistoryRule>();

                    LoadRules( true, ProtectedItemCategory.Folder );
                    LoadRules( true, ProtectedItemCategory.Library );
                    LoadRules( false, ProtectedItemCategory.Folder );
                    LoadRules( false, ProtectedItemCategory.Library );
                    _logger.Information( "List of rules regenerated" );
                }

                return _rules;
            }
        }

        public bool AddExcludeRule( ProtectedItemCategory category, string rule )
        {
            if( String.IsNullOrEmpty( rule ) )
            {
                _logger.Error("empty or undefined exclude rule not added" );
                return false;
            }

            try
            {
                _fhMgr.AddRemoveExcludeRule( true, category, rule );
                _logger.Information( "Added exclude rule" );
            }
            catch( Exception e )
            {
                var error = FileHistoryErrors.GetError(e);
                _logger.Error(error);

                return false;
            }

            if( _rules == null ) _rules = new List<FileHistoryRule>();

            _rules.Add( new FileHistoryRule()
            {
                Category = category,
                Rule = rule,
                Type = RuleType.Exclude
            } );

            return true;
        }

        public bool RemoveExcludeRule( ProtectedItemCategory category, string rule )
        {
            if( String.IsNullOrEmpty( rule ) )
            {
                _logger.Error( "empty or undefined exclude rule not removed" );
                return false;
            }

            try
            {
                _fhMgr.AddRemoveExcludeRule( false, category, rule );
                _logger.Information( "Removed exclude rule" );
            }
            catch( Exception e )
            {
                var error = FileHistoryErrors.GetError(e);
                _logger.Error(error);

                return false;
            }

            if( _rules == null ) _rules = new List<FileHistoryRule>();

            var toRemove = _rules.FirstOrDefault( r => r.Rule.Equals( rule, StringComparison.OrdinalIgnoreCase ) );
            if( toRemove != null ) _rules.Remove( toRemove );

            return true;
        }

        public TimeSpan SecondsBetweenBackups
        {
            get => TimeSpan.FromSeconds( GetLocalPolicy( LocalPolicy.Frequency ) );
            set => SetLocalPolicy( LocalPolicy.Frequency, (ulong) value.TotalSeconds );
        }

        public ulong RetentionAge
        {
            get => GetLocalPolicy( LocalPolicy.RetentionAge );
            set => SetLocalPolicy( LocalPolicy.RetentionAge, value );
        }

        public ulong RetentionType
        {
            get => GetLocalPolicy( LocalPolicy.RetentionType );
            set => SetLocalPolicy( LocalPolicy.RetentionType, value );
        }

        public TimeSpan SecondsSinceLastBackup
        {
            get
            {
                UpdateProtectionStatus();

                return DateTime.Now - LastBackup;
            }
        }

        public ulong GetLocalPolicy( LocalPolicy policy )
        {
            var retVal = ulong.MaxValue;

            try
            {
                retVal = _fhMgr.GetLocalPolicy( policy );
                _logger.Information( "Retrieved local policy value for {policy}", policy );
            }
            catch( Exception e )
            {
                var error = FileHistoryErrors.GetError(e);
                _logger.Error(error);
            }

            return retVal;
        }

        public bool SetLocalPolicy( LocalPolicy policy, ulong value )
        {
            try
            {
                _fhMgr.SetLocalPolicy( policy, value );
                _logger.Information( "Set local policy {policy} to {value}", policy, value );

                _fhSvc.ReloadConfiguration();
                _logger.Information( "Reloaded configuration" );
            }
            catch( Exception e )
            {
                var error = FileHistoryErrors.GetError(e);
                _logger.Error(error);

                return false;
            }

            return true;
        }

        public BackupStatus Status
        {
            get
            {
                var retVal = BackupStatus.MaxBackupStatus;

                try
                {
                    retVal = _fhMgr.GetBackupStatus();
                    _logger.Information( "Retrieved backup status {retVal}", retVal );
                }
                catch( Exception e )
                {
                    var error = FileHistoryErrors.GetError(e);
                    _logger.Error(error);
                }

                return retVal;
            }

            set
            {
                try
                {
                    _fhMgr.SetBackupStatus( value );
                    _logger.Information( "Set backup status to {value}", value );
                }
                catch( Exception e )
                {
                    var error = FileHistoryErrors.GetError(e);
                    _logger.Error(error);
                }
            }
        }

        public string TargetUrl => (string) GetTargetProperty( TargetProperty.Url );
        public string TargetName => (string) GetTargetProperty( TargetProperty.Name );
        public TargetDriveType TargetType =>
            (TargetDriveType) Enum.ToObject( typeof(TargetDriveType),
                GetTargetProperty( TargetProperty.DriveType ) );

        public ValidationResult ProvisionAndSet( string url, string name )
        {
            if( String.IsNullOrEmpty( url ) )
            {
                _logger.Error<string>( "empty or undefined {url}", url );
                return ValidationResult.MaxValidationResult;
            }

            if( String.IsNullOrEmpty( name ) )
            {
                _logger.Error<string>( "empty or undefined {name}", name );
                return ValidationResult.MaxValidationResult;
            }

            var valResult = ValidationResult.MaxValidationResult;

            try
            {
                valResult = _fhMgr.ValidateTarget( url );
                _logger.Information("Validated File History target, result was {valResult}", valResult );
            }
            catch( Exception e )
            {
                var error = FileHistoryErrors.GetError(e);
                _logger.Error(error);

                return valResult;
            }

            if( valResult != ValidationResult.ValidTarget )
            {
                _logger.Error( "invalid url '{url}'", propertyValue : url );
                return valResult;
            }

            try
            {
                _fhMgr.ProvisionAndSetNewTarget( url, name );
                _logger.Information( "Provisioned and set File History target" );
            }
            catch( Exception e )
            {
                var error = FileHistoryErrors.GetError(e);
                _logger.Error(error);

                return ValidationResult.MaxValidationResult;
            }

            return ValidationResult.ValidTarget;
        }

        public ProtectionState State
        {
            get
            {
                var (state, lastBackup) = UpdateProtectionStatus();
                return state;
            }
        }

        public DateTime LastBackup
        {
            get
            {
                var (state, lastBackup) = UpdateProtectionStatus();
                return lastBackup;
            }
        }

        public bool BackupIsRunning => ( State & ProtectionState.Running ) == ProtectionState.Running;

        public (ProtectionState state, DateTime lastBackup) UpdateProtectionStatus()
        {
            var state = ProtectionState.UpdateProtectionStateFailed;
            var lastBackup = DateTime.MaxValue;

            try
            {
                _fhMgr.QueryProtectionStatus( out int protectionState, out string lastBackupText );
                _logger.Information(
                    "Retrieved query protection status ({protectionState}, {lastBackupText})", 
                    propertyValue0: protectionState,
                    propertyValue1 : lastBackupText );

                state = (ProtectionState) protectionState;

                // this line is critical...and bizarre. Turns out QueryProtectionStatus() returns a string
                // with Unicode 'text direction' (think Arabic) characters embedded in it. Which causes the
                // default DateTime parser to fail. This next line strips out all oddball characters from the 
                // lastBackup string.
                lastBackupText = new string( lastBackupText.Where( c => c < 128 ).ToArray() );

                if( DateTime.TryParse( lastBackupText, out lastBackup ) )
                    _logger.Information( "Parsed DateTime of last backup ({lastBackup})", lastBackup );
                else
                    _logger.Information(
                        "Couldn't parse DateTime of last backup ({lastBackupText})", propertyValue : lastBackupText );
            }
            catch( Exception e )
            {
                var error = FileHistoryErrors.GetError(e);
                _logger.Error(error);
            }

            return ( state, lastBackup );
        }

        private void LoadRules( bool include, ProtectedItemCategory category )
        {
            try
            {
                var iterator = _fhMgr.GetIncludeExcludeRules( include, category );

                while( true )
                {
                    iterator.GetItem( out string item );

                    _rules.Add( new FileHistoryRule()
                    {
                        Category = category,
                        Rule = item,
                        Type = include ? RuleType.Include : RuleType.Exclude
                    } );

                    iterator.MoveToNextItem();
                }
            }
            catch( Exception e )
            {
                var error = FileHistoryErrors.GetError(e);
                _logger.Error(error);
            }
        }

        protected object GetTargetProperty( TargetProperty property )
        {
            var tpt = property.GetTargetPropertyType();

            switch( tpt )
            {
                case TargetPropertyType.Numeric:
                    return GetNumericTargetProperty( property );

                case TargetPropertyType.Text:
                    return GetTextTargetProperty( property );
            }

            _logger.Error("No retrieval method defined for TargetPropertyType '{tpt}'", tpt );
            return null;
        }

        protected string GetTextTargetProperty( TargetProperty property )
        {
            try
            {
                var target = _fhMgr.GetDefaultTarget() as IFhTarget;

                if( target != null ) return target.GetStringProperty( property );
                else
                {
                    _logger.Error( "could not retrieve {property}",property );
                    return String.Empty;
                }
            }
            catch( Exception e )
            {
                var error = FileHistoryErrors.GetError(e);
                _logger.Error(error);

                return String.Empty;
            }
        }

        protected ulong GetNumericTargetProperty( TargetProperty property )
        {
            try
            {
                var target = _fhMgr.GetDefaultTarget() as IFhTarget;

                if( target != null )
                    return target.GetNumericalProperty( property );
                else
                {
                    _logger.Error( "could not retrieve {property}", property );
                    return ulong.MaxValue;
                }
            }
            catch( Exception e )
            {
                var error = FileHistoryErrors.GetError(e);
                _logger.Error(error);

                return ulong.MaxValue;
            }

        }
    }
}
