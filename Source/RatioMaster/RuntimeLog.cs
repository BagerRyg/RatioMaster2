using System;
using System.IO;
using System.Windows.Forms;

namespace RatioMaster;

internal static class RuntimeLog
{
	private static readonly object SyncRoot = new object();
	private static string logPath;

	public static void Initialize()
	{
		try
		{
			logPath = Path.Combine(Application.StartupPath, "runtime.log");
			Write("Application starting");
		}
		catch
		{
		}
	}

	public static void Write(string message)
	{
		try
		{
			if (string.IsNullOrEmpty(logPath))
			{
				logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtime.log");
			}
			lock (SyncRoot)
			{
				File.AppendAllText(logPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + message + Environment.NewLine);
			}
		}
		catch
		{
		}
	}

	public static void WriteException(string message, Exception exception)
	{
		Write(message + Environment.NewLine + exception);
	}
}
