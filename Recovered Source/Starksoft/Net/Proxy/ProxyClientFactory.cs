namespace Starksoft.Net.Proxy;

public class ProxyClientFactory
{
	public IProxyClient CreateProxyClient(ProxyType type, string proxyHost, int proxyPort)
	{
		return type switch
		{
			ProxyType.Http => new HttpProxyClient(proxyHost, proxyPort), 
			ProxyType.Socks4 => new Socks4ProxyClient(proxyHost, proxyPort), 
			ProxyType.Socks4a => new Socks4aProxyClient(proxyHost, proxyPort), 
			ProxyType.Socks5 => new Socks5ProxyClient(proxyHost, proxyPort), 
			ProxyType.HttpDirect => new HttpProxyDirectClient(proxyHost, proxyPort), 
			_ => null, 
		};
	}
}
