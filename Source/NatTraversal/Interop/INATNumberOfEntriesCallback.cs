using System.Runtime.InteropServices;

namespace NatTraversal.Interop;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("C83A0A74-91EE-41B6-B67A-67E0F00BBD78")]
internal interface INATNumberOfEntriesCallback
{
	void NewNumberOfEntries(int newNumberOfEntries);
}
