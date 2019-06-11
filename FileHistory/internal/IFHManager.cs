using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace J4JSoftware.FileHistory
{
    [ Guid( "6A5FEA5B-BF8F-4EE5-B8C3-44D8A0D7331C" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown ) ]
    internal interface IFHManager
    {
        void LoadConfiguration();
        void CreateDefaultConfiguration( bool overwriteIfExists );
        void SaveConfiguration();
        void AddRemoveExcludeRule( bool add, ProtectedItemCategory category,
            [ MarshalAs( UnmanagedType.BStr ) ] string item );
        IFhScopeIterator GetIncludeExcludeRules( bool include, ProtectedItemCategory category );
        ulong GetLocalPolicy( LocalPolicy localPolicy );
        void SetLocalPolicy( LocalPolicy localPolicy, ulong policyValue );
        BackupStatus GetBackupStatus();
        void SetBackupStatus( BackupStatus status );
        IFhTarget GetDefaultTarget();
        ValidationResult ValidateTarget( [ MarshalAs( UnmanagedType.BStr ) ] string url );
        void ProvisionAndSetNewTarget( [ MarshalAs( UnmanagedType.BStr ) ] string url,
            [ MarshalAs( UnmanagedType.BStr ) ] string name );
        void ChangeDefaultTargetRecommendation( bool recommend );
        void QueryProtectionStatus( out int protectionState,
            [ MarshalAs( UnmanagedType.BStr ) ] out string protectedUntilTime );
    }
}