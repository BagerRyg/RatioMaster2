using System;

namespace RJH.CommandLineHelper;

[AttributeUsage(AttributeTargets.Property)]
public class CommandLineSwitchAttribute : Attribute
{
	private string m_name = "";

	private string m_description = "";

	public string Name => m_name;

	public string Description => m_description;

	public CommandLineSwitchAttribute(string name, string description)
	{
		m_name = name;
		m_description = description;
	}
}
