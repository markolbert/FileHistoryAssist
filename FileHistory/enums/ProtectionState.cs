using System;
using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace J4JSoftware.FileHistory
{
    [ Flags ]
    public enum ProtectionState
    {
        [ Description(
            "The File History protection state is unknown, because the File History service is not started or the current user is not tracked in it" ) ]
        NotTracked = 0x00,

        [ Description( "File History protection is not enabled for the current user" ) ]
        Off = 0x01,

        [ Description( "File History protection is disabled by Group Policy" ) ]
        DisabledByGroupPolicy = 0x02,

        [ Description(
            "There is a fatal error in one of the files that store internal File History information for the current user" ) ]
        FatalConfigurationError = 0x03,

        [ Description( "The current user does not have write permission for the currently assigned target" ) ]
        TargetAccessDenied = 0x0E,

        [ Description(
            "The currently assigned target has been marked as dirty (backup copies will not be created until after Chkdsk utility is run)" ) ]
        TargetVolumeDirty = 0x0F,

        [ Description(
            "The currently assigned target does not have sufficient space for storing backup copies of files from the File History protection scope, and retention is already set to the most aggressive policy (File History will provide a degraded level of protection)" ) ]
        TargetFullRetentionMax = 0x10,

        [ Description(
            "The currently assigned target does not have sufficient space for storing backup copies of files from the File History protection scope (File History will provide a degraded level of protection)" ) ]
        TargetFull = 0x11,

        [ Description(
            "The File History cache on one of the local disks does not have sufficient space for storing backup copies of files from the File History protection scope temporarily (File History will provide a degraded level of protection)" ) ]
        StagingFull = 0x12,

        [ Description(
            "The currently assigned target is running low on free space, and retention is already set to the most aggressive policy (level of File History protection is likely to degrade soon)" ) ]
        TargetLowSpaceRetentionMax = 0x13,

        [ Description(
            "The currently assigned target is running low on free space (level of File History protection is likely to degrade soon)" ) ]
        TargetLowSpace = 0x14,

        [ Description(
            "The currently assigned target has not been available for backups for a substantial period of time, causing File History level of protection to start degrading" ) ]
        TargetAbsent = 0x15,

        [ Description(
            "Too many changes have been made in the protected files or the protection scope (level of protection is likely to degrade, unless the user explicitly initiates an immediate backup instead of relying on regular backup cycles to be performed in the background)" ) ]
        TooMuchBehind = 0x16,

        [ Description(
            "File History backups are performed regularly, no error conditions are detected, an optimal level of File History protection is provided" ) ]
        NoError = 0xFF,

        // this next item is not part of the official API. It's here to allow for the possibility that
        // calling IFHManager.QueryProtectionStatus() fails for some unknown reason
        [Description(
            "Could not update protection state")]
        UpdateProtectionStateFailed = 0xEE,

        [Description( "Backup cycle is being performed for the current user right now" ) ]
        Running = 0X100
    }
}
