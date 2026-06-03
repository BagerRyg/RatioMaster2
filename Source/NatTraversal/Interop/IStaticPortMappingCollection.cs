using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NatTraversal.Interop;

[ComImport]
[TypeLibType(4160)]
[Guid("CD1F3E77-66D6-4664-82C7-36DBB641D0F1")]
internal interface IStaticPortMappingCollection
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(65)]
	[DispId(-4)]
	[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler, CustomMarshalers, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	IEnumerator GetEnumerator();

	[DispId(0)]
	IStaticPortMapping this[int lExternalPort, string bstrProtocol]
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(0)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(1)]
	int Count
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	void Remove([In] int lExternalPort, [In][MarshalAs(UnmanagedType.BStr)] string bstrProtocol);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	[return: MarshalAs(UnmanagedType.Interface)]
	IStaticPortMapping Add([In] int lExternalPort, [In][MarshalAs(UnmanagedType.BStr)] string bstrProtocol, [In] int lInternalPort, [In][MarshalAs(UnmanagedType.BStr)] string bstrInternalClient, [In] bool bEnabled, [In][MarshalAs(UnmanagedType.BStr)] string bstrDescription);
}
