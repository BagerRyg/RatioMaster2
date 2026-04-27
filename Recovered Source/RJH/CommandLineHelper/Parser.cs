using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RJH.CommandLineHelper;

public class Parser
{
	public class SwitchInfo
	{
		private object m_Switch;

		public string Name => (m_Switch as SwitchRecord).Name;

		public string Description => (m_Switch as SwitchRecord).Description;

		public string[] Aliases => (m_Switch as SwitchRecord).Aliases;

		public Type Type => (m_Switch as SwitchRecord).Type;

		public object Value => (m_Switch as SwitchRecord).Value;

		public object InternalValue => (m_Switch as SwitchRecord).InternalValue;

		public bool IsEnum => (m_Switch as SwitchRecord).Type.IsEnum;

		public string[] Enumerations => (m_Switch as SwitchRecord).Enumerations;

		public SwitchInfo(object rec)
		{
			if (rec is SwitchRecord)
			{
				m_Switch = rec;
				return;
			}
			throw new ArgumentException();
		}
	}

	private class SwitchRecord
	{
		private string m_name = "";

		private string m_description = "";

		private object m_value;

		private Type m_switchType = typeof(bool);

		private ArrayList m_Aliases;

		private string m_Pattern = "";

		private MethodInfo m_SetMethod;

		private MethodInfo m_GetMethod;

		private object m_PropertyOwner;

		public object Value
		{
			get
			{
				if (ReadValue != null)
				{
					return ReadValue;
				}
				return m_value;
			}
		}

		public object InternalValue => m_value;

		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}

		public string Description
		{
			get
			{
				return m_description;
			}
			set
			{
				m_description = value;
			}
		}

		public Type Type => m_switchType;

		public string[] Aliases
		{
			get
			{
				if (m_Aliases == null)
				{
					return null;
				}
				return (string[])m_Aliases.ToArray(typeof(string));
			}
		}

		public string Pattern => m_Pattern;

		public MethodInfo SetMethod
		{
			set
			{
				m_SetMethod = value;
			}
		}

		public MethodInfo GetMethod
		{
			set
			{
				m_GetMethod = value;
			}
		}

		public object PropertyOwner
		{
			set
			{
				m_PropertyOwner = value;
			}
		}

		public object ReadValue
		{
			get
			{
				object result = null;
				if (m_PropertyOwner != null && m_GetMethod != null)
				{
					result = m_GetMethod.Invoke(m_PropertyOwner, null);
				}
				return result;
			}
		}

		public string[] Enumerations
		{
			get
			{
				if (m_switchType.IsEnum)
				{
					return Enum.GetNames(m_switchType);
				}
				return null;
			}
		}

		private void Initialize(string name, string description)
		{
			m_name = name;
			m_description = description;
			BuildPattern();
		}

		private void BuildPattern()
		{
			string text = Name;
			if (Aliases != null && Aliases.Length > 0)
			{
				string[] aliases = Aliases;
				foreach (string text2 in aliases)
				{
					text = text + "|" + text2;
				}
			}
			string text3 = "(\\s|^)(?<match>(-{1,2}|/)(";
			string text4 = "(?=(\\s|$))";
			string text5;
			if (Type == typeof(bool))
			{
				text5 = ")(?<value>(\\+|-){0,1}))";
			}
			else if (Type == typeof(string))
			{
				text5 = ")(?::|\\s+))((?:\")(?<value>[^\"]+)(?:\")|(?<value>\\S+))";
			}
			else if (Type == typeof(int))
			{
				text5 = ")(?::|\\s+))((?<value>(-|\\+)[0-9]+)|(?<value>[0-9]+))";
			}
			else
			{
				if (!Type.IsEnum)
				{
					throw new ArgumentException();
				}
				string[] enumerations = Enumerations;
				string text6 = enumerations[0];
				for (int j = 1; j < enumerations.Length; j++)
				{
					text6 = text6 + "|" + enumerations[j];
				}
				text5 = ")(?::|\\s+))(?<value>" + text6 + ")";
			}
			m_Pattern = text3 + text + text5 + text4;
		}

		public SwitchRecord(string name, string description)
		{
			Initialize(name, description);
		}

		public SwitchRecord(string name, string description, Type type)
		{
			if (type == typeof(bool) || type == typeof(string) || type == typeof(int) || type.IsEnum)
			{
				m_switchType = type;
				Initialize(name, description);
				return;
			}
			throw new ArgumentException("Currently only Ints, Bool and Strings are supported");
		}

		public void AddAlias(string alias)
		{
			if (m_Aliases == null)
			{
				m_Aliases = new ArrayList();
			}
			m_Aliases.Add(alias);
			BuildPattern();
		}

		public void Notify(object value)
		{
			if (m_PropertyOwner != null && m_SetMethod != null)
			{
				object[] parameters = new object[1] { value };
				m_SetMethod.Invoke(m_PropertyOwner, parameters);
			}
			m_value = value;
		}
	}

	private string m_commandLine = "";

	private string m_workingString = "";

	private string m_applicationName = "";

	private string[] m_splitParameters;

	private ArrayList m_switches;

	public string ApplicationName => m_applicationName;

	public string[] Parameters => m_splitParameters;

	public SwitchInfo[] Switches
	{
		get
		{
			if (m_switches == null)
			{
				return null;
			}
			SwitchInfo[] array = new SwitchInfo[m_switches.Count];
			for (int i = 0; i < m_switches.Count; i++)
			{
				array[i] = new SwitchInfo(m_switches[i]);
			}
			return array;
		}
	}

	public object this[string name]
	{
		get
		{
			if (m_switches != null)
			{
				for (int i = 0; i < m_switches.Count; i++)
				{
					if (string.Compare((m_switches[i] as SwitchRecord).Name, name, ignoreCase: true) == 0)
					{
						return (m_switches[i] as SwitchRecord).Value;
					}
				}
			}
			return null;
		}
	}

	public string[] UnhandledSwitches
	{
		get
		{
			string pattern = "(\\s|^)(?<match>(-{1,2}|/)(.+?))(?=(\\s|$))";
			Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			MatchCollection matchCollection = regex.Matches(m_workingString);
			if (matchCollection != null)
			{
				string[] array = new string[matchCollection.Count];
				for (int i = 0; i < matchCollection.Count; i++)
				{
					array[i] = matchCollection[i].Groups["match"].Value;
				}
				return array;
			}
			return null;
		}
	}

	private void ExtractApplicationName()
	{
		Regex regex = new Regex("^(?<commandLine>(\"[^\"]+\"|(\\S)+))(?<remainder>.+)", RegexOptions.ExplicitCapture);
		Match match = regex.Match(m_commandLine);
		if (match != null && match.Groups["commandLine"] != null)
		{
			m_applicationName = match.Groups["commandLine"].Value;
			m_workingString = match.Groups["remainder"].Value;
		}
	}

	private void SplitParameters()
	{
		Regex regex = new Regex("((\\s*(\"(?<param>[^\"]+?)\"|(?<param>\\S+))))", RegexOptions.ExplicitCapture);
		MatchCollection matchCollection = regex.Matches(m_workingString);
		if (matchCollection != null)
		{
			m_splitParameters = new string[matchCollection.Count];
			for (int i = 0; i < matchCollection.Count; i++)
			{
				m_splitParameters[i] = matchCollection[i].Groups["param"].Value;
			}
		}
	}

	private void HandleSwitches()
	{
		if (m_switches == null)
		{
			return;
		}
		foreach (SwitchRecord @switch in m_switches)
		{
			Regex regex = new Regex(@switch.Pattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			MatchCollection matchCollection = regex.Matches(m_workingString);
			if (matchCollection != null)
			{
				for (int i = 0; i < matchCollection.Count; i++)
				{
					string text = null;
					if (matchCollection[i].Groups != null && matchCollection[i].Groups["value"] != null)
					{
						text = matchCollection[i].Groups["value"].Value;
					}
					if (@switch.Type == typeof(bool))
					{
						bool flag = true;
						if (matchCollection[i].Groups != null && matchCollection[i].Groups["value"] != null)
						{
							switch (text)
							{
							case "+":
								flag = true;
								break;
							case "-":
								flag = false;
								break;
							case "":
								if (@switch.ReadValue != null)
								{
									flag = !(bool)@switch.ReadValue;
								}
								break;
							}
						}
						@switch.Notify(flag);
						break;
					}
					if (@switch.Type == typeof(string))
					{
						@switch.Notify(text);
					}
					else if (@switch.Type == typeof(int))
					{
						@switch.Notify(int.Parse(text));
					}
					else if (@switch.Type.IsEnum)
					{
						@switch.Notify(Enum.Parse(@switch.Type, text, ignoreCase: true));
					}
				}
			}
			m_workingString = regex.Replace(m_workingString, " ");
		}
	}

	public void AddSwitch(string name, string description)
	{
		if (m_switches == null)
		{
			m_switches = new ArrayList();
		}
		SwitchRecord value = new SwitchRecord(name, description);
		m_switches.Add(value);
	}

	public void AddSwitch(string[] names, string description)
	{
		if (m_switches == null)
		{
			m_switches = new ArrayList();
		}
		SwitchRecord switchRecord = new SwitchRecord(names[0], description);
		for (int i = 1; i < names.Length; i++)
		{
			switchRecord.AddAlias(names[i]);
		}
		m_switches.Add(switchRecord);
	}

	public bool Parse()
	{
		ExtractApplicationName();
		HandleSwitches();
		SplitParameters();
		return true;
	}

	public object InternalValue(string name)
	{
		if (m_switches != null)
		{
			for (int i = 0; i < m_switches.Count; i++)
			{
				if (string.Compare((m_switches[i] as SwitchRecord).Name, name, ignoreCase: true) == 0)
				{
					return (m_switches[i] as SwitchRecord).InternalValue;
				}
			}
		}
		return null;
	}

	public Parser(string commandLine)
	{
		m_commandLine = commandLine;
	}

	public Parser(string commandLine, object classForAutoAttributes)
	{
		m_commandLine = commandLine;
		Type type = classForAutoAttributes.GetType();
		MemberInfo[] members = type.GetMembers();
		for (int i = 0; i < members.Length; i++)
		{
			object[] customAttributes = members[i].GetCustomAttributes(inherit: false);
			if (customAttributes.Length <= 0)
			{
				continue;
			}
			SwitchRecord switchRecord = null;
			object[] array = customAttributes;
			for (int j = 0; j < array.Length; j++)
			{
				Attribute attribute = (Attribute)array[j];
				if (attribute is CommandLineSwitchAttribute)
				{
					CommandLineSwitchAttribute commandLineSwitchAttribute = (CommandLineSwitchAttribute)attribute;
					if (members[i] is PropertyInfo)
					{
						PropertyInfo propertyInfo = (PropertyInfo)members[i];
						switchRecord = new SwitchRecord(commandLineSwitchAttribute.Name, commandLineSwitchAttribute.Description, propertyInfo.PropertyType)
						{
							SetMethod = propertyInfo.GetSetMethod(),
							GetMethod = propertyInfo.GetGetMethod(),
							PropertyOwner = classForAutoAttributes
						};
						break;
					}
				}
			}
			if (switchRecord != null)
			{
				object[] array2 = customAttributes;
				for (int k = 0; k < array2.Length; k++)
				{
					Attribute attribute2 = (Attribute)array2[k];
					if (attribute2 is CommandLineAliasAttribute)
					{
						CommandLineAliasAttribute commandLineAliasAttribute = (CommandLineAliasAttribute)attribute2;
						switchRecord.AddAlias(commandLineAliasAttribute.Alias);
					}
				}
			}
			if (switchRecord != null)
			{
				if (m_switches == null)
				{
					m_switches = new ArrayList();
				}
				m_switches.Add(switchRecord);
			}
		}
	}
}
