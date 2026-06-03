using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NatTraversal.Interop;

[ComImport]
[TypeLibType(4160)]
[Guid("624BD588-9060-4109-B0B0-1ADBBCAC32DF")]
internal interface INATEventManager
{
	[DispId(1)]
	object ExternalIPAddressCallback
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		[param: In]
		[param: MarshalAs(UnmanagedType.IUnknown)]
		set;
	}

	[DispId(2)]
	object NumberOfEntriesCallback
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		[param: In]
		[param: MarshalAs(UnmanagedType.IUnknown)]
		set;
	}
}
