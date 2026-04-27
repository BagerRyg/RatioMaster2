namespace RatioMaster;

internal class VersionChecker
{
	private const int _ver = 2000;

	private const string _publicVer = "2.0";

	private Form1 _mainForm;

	public int Version => 2000;

	public string PublicVersion => "2.0";

	public VersionChecker(Form1 MainForm)
	{
		_mainForm = MainForm;
	}

	public void CheckNewVersion()
	{
		_mainForm.AddLogLine("Version checking is disabled.");
	}
}
