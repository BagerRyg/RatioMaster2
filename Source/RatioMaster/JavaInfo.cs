using Microsoft.Win32;

namespace RatioMaster;

internal class JavaInfo
{
	private static JavaInfo Instance;

	public string CurrentVersion { get; set; }

	public bool Installed { get; set; }

	public string Path { get; set; }

	protected JavaInfo()
	{
		CurrentVersion = "Java 1.6.0_14";
		Installed = false;
		Path = "";
	}

	public static JavaInfo Get()
	{
		if (Instance == null)
		{
			RegistryKey localMachine = Registry.LocalMachine;
			JavaInfo javaInfo = new JavaInfo();
			string name = "Software\\JavaSoft\\Java Runtime Environment";
			RegistryKey registryKey = localMachine.OpenSubKey(name);
			if (registryKey != null)
			{
				javaInfo.Installed = true;
				object value = registryKey.GetValue("BrowserJavaVersion");
				if (value == null)
				{
					value = registryKey.GetValue("Java6FamilyVersion");
				}
				if (value == null)
				{
					value = registryKey.GetValue("CurrentVersion");
				}
				if (value != null)
				{
					javaInfo.CurrentVersion = "Java " + value.ToString();
					RegistryKey registryKey2 = registryKey.OpenSubKey(value.ToString());
					if (registryKey2 != null)
					{
						value = registryKey2.GetValue("JavaHome");
						if (value != null)
						{
							javaInfo.Path = value.ToString();
						}
					}
				}
			}
			Instance = javaInfo;
		}
		return Instance;
	}
}
