using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace J4JSoftware.FileHistory
{
    [Guid("D87965FD-2BAD-4657-BD3B-9567EB300CED"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFhTarget
    {
        [return:MarshalAs(UnmanagedType.BStr)]
        string GetStringProperty( TargetProperty property );
        ulong GetNumericalProperty( TargetProperty property );
    }
}