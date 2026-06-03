using System.Runtime.InteropServices;

namespace NatTraversal.Interop;

[ComImport]
[Guid("9C416740-A34E-446F-BA06-ABD04C3149AE")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface INATExternalIPAddressCallback
{
	void NewExternalIPAddress([MarshalAs(UnmanagedType.BStr)] string newExternalIPAddress);
}
