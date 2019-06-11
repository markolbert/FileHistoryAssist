using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.FileHistory
{
    public static class FileHistoryErrors
    {
        public static ReadOnlyDictionary<int, string> Errors { get; }

        static FileHistoryErrors()
        {
            var tmp = new Dictionary<int, string>
            {
                // Error codes for the Configuration Manager. (0x0300 - 0x03ff)
                { 0x0300, "Corrupt configuration file" },
                { 0x0301, "Configuration file not found" },
                { 0x0302, "Configuration file already exists" },
                { 0x0303, "Configuration file not loaded" },
                { 0x0304, "Target is not connected" },
                { 0x0305, "Configuration already loaded" },
                { 0x0306, "Target verification failed" },
                { 0x0307, "Target not configured" },
                { 0x0308, "Target lacks sufficient free space" },
                { 0x0309, "Target not usable for File History" },
                { 0x30A, "Rehydration not possible in currently configured state" },
                { 0x0310, "Changing target recommendation is not allowed" },
                { 0X0311, "Target was rehydrated on another computer" },
                { 0X0312, "Legacy backup target not supported by File History" },
                { 0X0313, "Legacy backup target validation result is not supported" },
                { 0X0314, "Legacy backup user was fully excluded from backups" },
                { 0X0315, "Legacy backup not found" },

                // Error codes for the File History Service. (0x0600 - 0x06ff;
                { 0x0600, "Backups are blocked for the given configuration" },
                { 0x0601, "File History is not configured for the user" },
                { 0x0602, "Specified configuration is disabled by the user" },
                { 0x0603, "Specified configuration is disabled by Group Policy" },
                { 0x0604, "Fatal configuration error, backup cannot be started" },
                { 0x0605, "Specified configuration is undergoing rehydration" }
            };

            Errors = new ReadOnlyDictionary<int, string>( tmp );
        }

        public static string GetError( Exception e )
        {
            var lowWord = e?.HResult & 0x0000FFFF ?? 0;

            if (Errors.ContainsKey(lowWord)) return FileHistoryErrors.Errors[lowWord];

            return e == null ? "Undefined exception" : e.Message;
        }
    }
}
