using System;

namespace Starksoft.Net.Proxy;

public class ProxyException : Exception
{
	public ProxyException()
	{
	}

	public ProxyException(string message)
		: base(message)
	{
	}

	public ProxyException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

}
