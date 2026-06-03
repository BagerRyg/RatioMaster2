using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RatioMaster;

internal class IniReader
{
	public string path;

	[DllImport("KERNEL32.DLL", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetPrivateProfileStringW", ExactSpelling = true, SetLastError = true)]
	private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpReturnString, int nSize, string lpFilename);

	[DllImport("KERNEL32.DLL", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "WritePrivateProfileStringW", ExactSpelling = true, SetLastError = true)]
	private static extern int WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFilename);

	public IniReader(string INIPath)
	{
		path = INIPath;
	}

	public void WriteString(string Section, string Key, string Value)
	{
		WritePrivateProfileString(Section, Key, Value, path);
	}

	public static string ReadStringFromIni(string Section, string Key, string FilePath)
	{
		string text = new string(' ', 1024);
		GetPrivateProfileString(Section, Key, "", text, 1024, FilePath);
		char[] array = new char[1];
		char[] separator = array;
		return text.Split(separator)[0];
	}

	public string ReadString(string Section, string Key)
	{
		string text = new string(' ', 1024);
		GetPrivateProfileString(Section, Key, "", text, 1024, path);
		char[] array = new char[1];
		char[] separator = array;
		return text.Split(separator)[0];
	}

	public string ReadString(string Section, string Key, string Def)
	{
		string text = new string(' ', 1024);
		GetPrivateProfileString(Section, Key, Def, text, 1024, path);
		char[] array = new char[1];
		char[] separator = array;
		return text.Split(separator)[0];
	}

	public List<string> GetSectionList()
	{
		string text = new string(' ', 65536);
		GetPrivateProfileString(null, null, null, text, 65536, path);
		char[] array = new char[1];
		char[] separator = array;
		List<string> list = new List<string>(text.Split(separator));
		list.RemoveRange(list.Count - 2, 2);
		return list;
	}

	public List<string> GetKeyList(string Section)
	{
		string text = new string(' ', 32768);
		GetPrivateProfileString(Section, null, null, text, 32768, path);
		char[] array = new char[1];
		char[] separator = array;
		List<string> list = new List<string>(text.Split(separator));
		list.RemoveRange(list.Count - 2, 2);
		return list;
	}
}
