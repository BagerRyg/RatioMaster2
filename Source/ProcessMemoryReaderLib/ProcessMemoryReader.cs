using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessMemoryReaderLib;

public sealed class ProcessMemoryReader : IDisposable
{
	private const uint MaxReadSize = 1024 * 1024;

	private Process m_ReadProcess;

	private IntPtr m_hProcess = IntPtr.Zero;

	public Process ReadProcess
	{
		get
		{
			return m_ReadProcess;
		}
		set
		{
			m_ReadProcess = value;
		}
	}

	public void OpenProcess()
	{
		if (m_ReadProcess == null || m_ReadProcess.HasExited)
		{
			throw new InvalidOperationException("The target process is not available.");
		}
		CloseHandle();
		m_hProcess = ProcessMemoryReaderApi.OpenProcess(16u, 0, (uint)m_ReadProcess.Id);
		if (m_hProcess == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to open the target process.");
		}
	}

	public void CloseHandle()
	{
		if (m_hProcess == IntPtr.Zero)
		{
			return;
		}
		IntPtr handle = m_hProcess;
		m_hProcess = IntPtr.Zero;
		if (!ProcessMemoryReaderApi.CloseHandle(handle))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to close the target process handle.");
		}
	}

	public byte[] ReadProcessMemory(IntPtr MemoryAddress, uint bytesToRead, out int bytesReaded)
	{
		if (m_hProcess == IntPtr.Zero)
		{
			throw new InvalidOperationException("The target process is not open.");
		}
		if (bytesToRead == 0 || bytesToRead > MaxReadSize)
		{
			throw new ArgumentOutOfRangeException(nameof(bytesToRead));
		}
		byte[] array = new byte[bytesToRead];
		bool success = ProcessMemoryReaderApi.ReadProcessMemory(m_hProcess, MemoryAddress, array, bytesToRead, out IntPtr bytesRead);
		long count = bytesRead.ToInt64();
		if (count < 0 || count > array.Length)
		{
			Array.Clear(array, 0, array.Length);
			throw new InvalidOperationException("The operating system returned an invalid memory read length.");
		}
		bytesReaded = (int)count;
		if (!success && bytesReaded == 0)
		{
			Array.Clear(array, 0, array.Length);
		}
		return array;
	}

	public void Dispose()
	{
		try
		{
			CloseHandle();
		}
		catch
		{
		}
		GC.SuppressFinalize(this);
	}

	~ProcessMemoryReader()
	{
		Dispose();
	}
}
