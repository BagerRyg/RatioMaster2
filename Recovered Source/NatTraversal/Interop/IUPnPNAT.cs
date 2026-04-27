using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NatTraversal.Interop;

[ComImport]
[TypeLibType(4160)]
[Guid("B171C812-CC76-485A-94D8-B6B3A2794E99")]
internal interface IUPnPNAT
{
	[DispId(1)]
	IStaticPortMappingCollection StaticPortMappingCollection
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(2)]
	object DynamicPortMappingCollection
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(3)]
	INATEventManager NATEventManager
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(3)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}
}
