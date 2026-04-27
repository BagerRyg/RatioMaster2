using System.Runtime.InteropServices;

namespace NatTraversal.Interop;

[ComImport]
[Guid("B171C812-CC76-485A-94D8-B6B3A2794E99")]
[CoClass(typeof(UPnPNATCreator))]
internal interface UPnPNAT : IUPnPNAT
{
}
