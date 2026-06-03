using System;
using System.Net;
using System.Net.Sockets;

namespace Starksoft.Net.Proxy;

public interface IProxyClient
{
	IPEndPoint BindIpEndpoint { get; set; }

	string ProxyHost { get; set; }

	int ProxyPort { get; set; }

	string ProxyName { get; }

	event EventHandler<CreateConnectionAsyncCompletedEventArgs> CreateConnectionAsyncCompleted;

	TcpClient CreateConnection(string destinationHost, int destinationPort);

	void CreateConnectionAsync(string destinationHost, int destinationPort);

	void Close();
}
