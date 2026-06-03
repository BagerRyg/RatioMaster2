using System;
using System.Diagnostics;

namespace ProcessMemoryReaderLib;

public class ProcessMemoryReader
{
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
		m_hProcess = ProcessMemoryReaderApi.OpenProcess(16u, 1, (uint)m_ReadProcess.Id);
	}

	public void CloseHandle()
	{
		if (ProcessMemoryReaderApi.CloseHandle(m_hProcess) == 0)
		{
			throw new Exception("CloseHandle failed");
		}
	}

	public byte[] ReadProcessMemory(IntPtr MemoryAddress, uint bytesToRead, out int bytesReaded)
	{
		byte[] array = new byte[bytesToRead];
		ProcessMemoryReaderApi.ReadProcessMemory(m_hProcess, MemoryAddress, array, bytesToRead, out var lpNumberOfBytesRead);
		bytesReaded = lpNumberOfBytesRead.ToInt32();
		return array;
	}
}
