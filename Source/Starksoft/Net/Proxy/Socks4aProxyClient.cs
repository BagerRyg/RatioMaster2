using System.Net.Sockets;
using System.Text;

namespace Starksoft.Net.Proxy;

public class Socks4aProxyClient : Socks4ProxyClient
{
	private const string PROXY_NAME = "SOCKS4a";

	public override string ProxyName => "SOCKS4a";

	public Socks4aProxyClient(string proxyHost, string proxyUserId)
	{
		_proxyHost = proxyHost;
		_proxyPort = 1080;
		_proxyUserId = proxyUserId;
	}

	public Socks4aProxyClient(string proxyHost, int proxyPort, string proxyUserId)
	{
		_proxyHost = proxyHost;
		_proxyPort = proxyPort;
		_proxyUserId = proxyUserId;
	}

	public Socks4aProxyClient(string proxyHost)
	{
		_proxyHost = proxyHost;
		_proxyPort = 1080;
	}

	public Socks4aProxyClient(string proxyHost, int proxyPort)
	{
		_proxyHost = proxyHost;
		_proxyPort = proxyPort;
	}

	internal override void SendCommand(NetworkStream proxy, byte command, string destinationHost, int destinationPort, string userId)
	{
		if (userId == null)
		{
			userId = "";
		}
		byte[] array = new byte[4] { 0, 0, 0, 1 };
		byte[] destinationPortBytes = GetDestinationPortBytes(destinationPort);
		byte[] bytes = Encoding.ASCII.GetBytes(userId);
		byte[] bytes2 = Encoding.ASCII.GetBytes(destinationHost);
		byte[] array2 = new byte[10 + bytes.Length + bytes2.Length];
		array2[0] = 4;
		array2[1] = command;
		destinationPortBytes.CopyTo(array2, 2);
		array.CopyTo(array2, 4);
		bytes.CopyTo(array2, 8);
		array2[8 + bytes.Length] = 0;
		bytes2.CopyTo(array2, 9 + bytes.Length);
		array2[9 + bytes.Length + bytes2.Length] = 0;
		proxy.Write(array2, 0, array2.Length);
		WaitForData(proxy);
		byte[] array3 = new byte[8];
		ReadExact(proxy, array3, 0, array3.Length);
		if (array3[1] != 90)
		{
			HandleProxyCommandError(array3, destinationHost, destinationPort);
		}
	}
}
