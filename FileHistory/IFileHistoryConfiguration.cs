using System;
using System.Collections.Generic;

namespace J4JSoftware.FileHistory
{
    public interface IFileHistoryConfiguration
    {
        void CreateConfiguration( bool overwrite );
        void SaveConfiguration();
        IReadOnlyList<FileHistoryRule> Rules { get; }
        TimeSpan SecondsBetweenBackups { get; set; }
        ulong RetentionAge { get; set; }
        ulong RetentionType { get; set; }
        TimeSpan SecondsSinceLastBackup { get; }
        BackupStatus Status { get; set; }
        string TargetUrl { get; }
        string TargetName { get; }
        TargetDriveType TargetType { get; }
        bool AddExcludeRule( ProtectedItemCategory category, string rule );
        bool RemoveExcludeRule( ProtectedItemCategory category, string rule );
        ulong GetLocalPolicy( LocalPolicy policy );
        bool SetLocalPolicy( LocalPolicy policy, ulong value );
        ValidationResult ProvisionAndSet( string url, string name );
        (ProtectionState state, DateTime lastBackup) UpdateProtectionStatus();
        ProtectionState State { get; }
        DateTime LastBackup { get; }
        bool BackupIsRunning { get; }
    }
}