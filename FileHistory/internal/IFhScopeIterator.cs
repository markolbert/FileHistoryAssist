using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace J4JSoftware.FileHistory
{
    [Guid("3197ABCE-532A-44C6-8615-F3666566A720"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFhScopeIterator
    {
        void MoveToNextItem();
        string GetItem( [ MarshalAs( UnmanagedType.BStr ) ] out string item );
    }
}