using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace RatioMaster;

public class LocalizationManager
{
	private Dictionary<string, Dictionary<string, string>> _currentLangDict = new Dictionary<string, Dictionary<string, string>>();

	private Form1 _mainForm;

	private string LangDir = "";

	private string _currentRightToLeft = "";

	public LocalizationManager(Form1 mainForm)
	{
		_mainForm = mainForm;
		string startupPath = Application.StartupPath;
		LangDir = startupPath + "\\lng\\";
	}

	public List<LangInfo> GetLangList()
	{
		List<LangInfo> list = new List<LangInfo>();
		if (!Directory.Exists(LangDir))
		{
			_mainForm.AddLogLine("Language directory (/lng) doesn't exists");
			return null;
		}
		string[] files = Directory.GetFiles(LangDir, "*.lng");
		string[] array = files;
		foreach (string text in array)
		{
			LangInfo langInfo = new LangInfo();
			langInfo.File = text;
			langInfo.Name = IniReader.ReadStringFromIni("General", "LanguageName", text);
			langInfo.Version = IniReader.ReadStringFromIni("General", "Version", text);
			langInfo.RightToLeft = IniReader.ReadStringFromIni("General", "RightToLeft", text);
			list.Add(langInfo);
		}
		return list;
	}

	public void LoadLanguageFromFile(string lngFile)
	{
		_currentLangDict.Clear();
		IniReader iniReader = new IniReader(lngFile);
		_currentRightToLeft = iniReader.ReadString("General", "RightToLeft");
		List<string> sectionList = iniReader.GetSectionList();
		foreach (string item in sectionList)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			List<string> keyList = iniReader.GetKeyList(item);
			foreach (string item2 in keyList)
			{
				dictionary[item2] = iniReader.ReadString(item, item2);
			}
			_currentLangDict[item] = dictionary;
		}
	}

	public void LocalizeForm(Form _form, bool adjustRightToLeft)
	{
		string name = _form.Name;
		foreach (Control control in _form.Controls)
		{
			switch (control.GetType().Name.ToLower())
			{
			case "label":
			case "checkbox":
			case "linklabel":
			case "button":
				TranslateControlText(control, name);
				break;
			case "tabcontrol":
				LocalizeChildControls(control, name);
				break;
			case "tabpage":
				TranslateControlText(control, name);
				LocalizeChildControls(control, name);
				break;
			case "groupbox":
				TranslateControlText(control, name);
				LocalizeChildControls(control, name);
				break;
			}
		}
		if (adjustRightToLeft)
		{
			adjustRightToLeftLayout(_form);
		}
	}

	public void LocalizeForm(Form _form)
	{
		LocalizeForm(_form, adjustRightToLeft: true);
	}

	private void adjustRightToLeftLayout(Form _form)
	{
		if (_currentRightToLeft == "true" && _form.RightToLeft != RightToLeft.Yes)
		{
			_form.RightToLeft = RightToLeft.Yes;
		}
		else if (_currentRightToLeft != "true" && _form.RightToLeft == RightToLeft.Yes)
		{
			_form.RightToLeft = RightToLeft.No;
		}
	}

	private void LocalizeChildControls(Control control, string section)
	{
		foreach (Control control2 in control.Controls)
		{
			switch (control2.GetType().Name.ToLower())
			{
			case "label":
			case "checkbox":
			case "linklabel":
			case "button":
				TranslateControlText(control2, section);
				break;
			case "tabcontrol":
				LocalizeChildControls(control2, section);
				break;
			case "tabpage":
				TranslateControlText(control2, section);
				LocalizeChildControls(control2, section);
				break;
			case "groupbox":
				TranslateControlText(control2, section);
				LocalizeChildControls(control2, section);
				break;
			}
		}
	}

	private void TranslateControlText(Control control, string section)
	{
		if (_currentLangDict.ContainsKey(section) && _currentLangDict[section].ContainsKey(control.Name))
		{
			control.Text = _currentLangDict[section][control.Name];
		}
	}

	public string TranslateMessageInner(string section, string messageID, string def)
	{
		if (_currentLangDict.ContainsKey(section) && _currentLangDict[section].ContainsKey(messageID))
		{
			return _currentLangDict[section][messageID].Replace("\\r\\n", "\r\n");
		}
		return def.Replace("\\r\\n", "\r\n");
	}

	public string TranslateMessage(string messageID, string def)
	{
		string section = "Messages";
		return TranslateMessageInner(section, messageID, def);
	}

	public string TranslateTooltip(string messageID, string def)
	{
		string section = "Tooltips";
		return TranslateMessageInner(section, messageID, def);
	}

	public string TranslateMessageWithParams(string messageID, string def, params string[] Params)
	{
		string key = "Messages";
		if (_currentLangDict.ContainsKey(key) && _currentLangDict[key].ContainsKey(messageID))
		{
			return string.Format(_currentLangDict[key][messageID].Replace("\\r\\n", "\r\n"), Params);
		}
		_mainForm.AddLogLine(messageID + "=" + def);
		return string.Format(def, Params);
	}
}
