using System;
using System.IO;

namespace RatioMaster;

internal static class RuntimeLog
{
	private static readonly object SyncRoot = new object();
	private static string logPath;
	private static bool enabled;
	private static bool shutdownWritten;

	public static void Initialize()
	{
		try
		{
			AppPaths.EnsureDataDirectory();
			logPath = AppPaths.RuntimeLogPath;
			shutdownWritten = false;
			File.WriteAllText(logPath, string.Empty);
			WriteCore("INFO", "Application starting");
		}
		catch
		{
		}
	}

	public static void Write(string message)
	{
		if (enabled)
		{
			WriteCore("INFO", message);
		}
	}

	public static void SetEnabled(bool value)
	{
		enabled = value;
		WriteCore("INFO", value ? "Full logging enabled" : "Full logging disabled");
	}

	public static void WriteWarning(string message)
	{
		WriteCore("WARN", message);
	}

	public static void WriteError(string message)
	{
		WriteCore("ERROR", message);
	}

	public static void WriteException(string message, Exception exception)
	{
		WriteCore("ERROR", message + ": " + exception.GetType().Name + ": " + exception.Message);
	}

	public static void Shutdown()
	{
		if (shutdownWritten)
		{
			return;
		}
		shutdownWritten = true;
		WriteCore("INFO", "Application shutting down");
		enabled = false;
	}

	private static void WriteCore(string level, string message)
	{
		try
		{
			if (string.IsNullOrEmpty(logPath))
			{
				AppPaths.EnsureDataDirectory();
				logPath = AppPaths.RuntimeLogPath;
			}
			lock (SyncRoot)
			{
				File.AppendAllText(logPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + level + "] " + SensitiveDataRedactor.Sanitize(message) + Environment.NewLine);
			}
		}
		catch
		{
		}
	}

}
