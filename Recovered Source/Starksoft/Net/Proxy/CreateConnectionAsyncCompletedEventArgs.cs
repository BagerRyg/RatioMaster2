using System;
using System.ComponentModel;
using System.Net.Sockets;

namespace Starksoft.Net.Proxy;

public class CreateConnectionAsyncCompletedEventArgs : AsyncCompletedEventArgs
{
	private TcpClient _proxyConnection;

	public TcpClient ProxyConnection => _proxyConnection;

	public CreateConnectionAsyncCompletedEventArgs(Exception error, bool cancelled, TcpClient proxyConnection)
		: base(error, cancelled, null)
	{
		_proxyConnection = proxyConnection;
	}
}
