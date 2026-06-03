using System;

namespace RJH.CommandLineHelper;

[AttributeUsage(AttributeTargets.Property)]
public class CommandLineAliasAttribute : Attribute
{
	protected string m_Alias = "";

	public string Alias => m_Alias;

	public CommandLineAliasAttribute(string alias)
	{
		m_Alias = alias;
	}
}
