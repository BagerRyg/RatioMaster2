using System;
using System.Runtime.InteropServices;

namespace ProcessMemoryReaderLib;

internal class ProcessMemoryReaderApi
{
	public const uint PROCESS_VM_READ = 16u;

	[DllImport("kernel32.dll")]
	public static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

	[DllImport("kernel32.dll")]
	public static extern int CloseHandle(IntPtr hObject);

	[DllImport("kernel32.dll")]
	public static extern int ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In][Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesRead);
}
