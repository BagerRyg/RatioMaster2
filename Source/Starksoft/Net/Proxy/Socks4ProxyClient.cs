using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Starksoft.Net.Proxy;

public class Socks4ProxyClient : IProxyClient
{
	private const int WAIT_FOR_DATA_INTERVAL = 50;

	private const int WAIT_FOR_DATA_TIMEOUT = 15000;

	private const string PROXY_NAME = "SOCKS4";

	internal const int SOCKS_PROXY_DEFAULT_PORT = 1080;

	internal const byte SOCKS4_VERSION_NUMBER = 4;

	internal const byte SOCKS4_CMD_CONNECT = 1;

	internal const byte SOCKS4_CMD_BIND = 2;

	internal const byte SOCKS4_CMD_REPLY_REQUEST_GRANTED = 90;

	internal const byte SOCKS4_CMD_REPLY_REQUEST_REJECTED_OR_FAILED = 91;

	internal const byte SOCKS4_CMD_REPLY_REQUEST_REJECTED_CANNOT_CONNECT_TO_IDENTD = 92;

	internal const byte SOCKS4_CMD_REPLY_REQUEST_REJECTED_DIFFERENT_IDENTD = 93;

	private TcpClient _tcpClient;

	private IPEndPoint _BindIpEndpoint;

	internal string _proxyHost;

	internal int _proxyPort;

	internal string _proxyUserId;

	private BackgroundWorker _asyncWorker;

	private Exception _asyncException;

	private bool _asyncCancelled;

	public IPEndPoint BindIpEndpoint
	{
		get
		{
			return _BindIpEndpoint;
		}
		set
		{
			_BindIpEndpoint = value;
		}
	}

	public string ProxyHost
	{
		get
		{
			return _proxyHost;
		}
		set
		{
			_proxyHost = value;
		}
	}

	public int ProxyPort
	{
		get
		{
			return _proxyPort;
		}
		set
		{
			_proxyPort = value;
		}
	}

	public virtual string ProxyName => "SOCKS4";

	public string ProxyUserId
	{
		get
		{
			return _proxyUserId;
		}
		set
		{
			_proxyUserId = value;
		}
	}

	public bool IsBusy
	{
		get
		{
			if (_asyncWorker != null)
			{
				return _asyncWorker.IsBusy;
			}
			return false;
		}
	}

	public bool IsAsyncCancelled => _asyncCancelled;

	public event EventHandler<CreateConnectionAsyncCompletedEventArgs> CreateConnectionAsyncCompleted;

	public Socks4ProxyClient()
	{
	}

	public Socks4ProxyClient(string proxyHost, string proxyUserId)
	{
		_proxyHost = proxyHost;
		_proxyPort = 1080;
		_proxyUserId = proxyUserId;
	}

	public Socks4ProxyClient(string proxyHost, int proxyPort, string proxyUserId)
	{
		_proxyHost = proxyHost;
		_proxyPort = proxyPort;
		_proxyUserId = proxyUserId;
	}

	public Socks4ProxyClient(string proxyHost)
	{
		_proxyHost = proxyHost;
		_proxyPort = 1080;
	}

	public Socks4ProxyClient(string proxyHost, int proxyPort)
	{
		_proxyHost = proxyHost;
		_proxyPort = proxyPort;
	}

	public void Close()
	{
		if (_tcpClient != null)
		{
			_tcpClient.Close();
		}
	}

	public TcpClient CreateConnection(string destinationHost, int destinationPort)
	{
		try
		{
			TcpClient tcpClient = new TcpClient();
			if (BindIpEndpoint != null)
			{
				tcpClient.Client.Bind(BindIpEndpoint);
			}
			_tcpClient = tcpClient;
			tcpClient.Connect(_proxyHost, _proxyPort);
			SendCommand(tcpClient.GetStream(), 1, destinationHost, destinationPort, _proxyUserId);
			_tcpClient = null;
			return tcpClient;
		}
		catch (Exception innerException)
		{
			_tcpClient = null;
			throw new ProxyException(string.Format(CultureInfo.InvariantCulture, "Connection to proxy host {0} on port {1} failed.", _proxyHost, _proxyPort.ToString(CultureInfo.InvariantCulture)), innerException);
		}
	}

	internal virtual void SendCommand(NetworkStream proxy, byte command, string destinationHost, int destinationPort, string userId)
	{
		if (userId == null)
		{
			userId = "";
		}
		byte[] iPAddressBytes = GetIPAddressBytes(destinationHost);
		byte[] destinationPortBytes = GetDestinationPortBytes(destinationPort);
		byte[] bytes = Encoding.ASCII.GetBytes(userId);
		byte[] array = new byte[9 + bytes.Length];
		array[0] = 4;
		array[1] = command;
		destinationPortBytes.CopyTo(array, 2);
		iPAddressBytes.CopyTo(array, 4);
		bytes.CopyTo(array, 8);
		array[8 + bytes.Length] = 0;
		proxy.Write(array, 0, array.Length);
		WaitForData(proxy);
		byte[] array2 = new byte[8];
		ReadExact(proxy, array2, 0, array2.Length);
		if (array2[1] != 90)
		{
			HandleProxyCommandError(array2, destinationHost, destinationPort);
		}
	}

	internal static void ReadExact(NetworkStream stream, byte[] buffer, int offset, int count)
	{
		int totalRead = 0;
		while (totalRead < count)
		{
			int read = stream.Read(buffer, offset + totalRead, count - totalRead);
			if (read <= 0)
			{
				throw new ProxyException("The proxy destination closed the connection before sending the expected response.");
			}
			totalRead += read;
		}
	}

	internal byte[] GetIPAddressBytes(string destinationHost)
	{
		IPAddress address = null;
		if (!IPAddress.TryParse(destinationHost, out address))
		{
			try
			{
				address = Dns.GetHostEntry(destinationHost).AddressList[0];
			}
			catch (Exception innerException)
			{
				throw new ProxyException(string.Format(CultureInfo.InvariantCulture, "A error occurred while attempting to DNS resolve the host name {0}.", destinationHost), innerException);
			}
		}
		return address.GetAddressBytes();
	}

	internal byte[] GetDestinationPortBytes(int value)
	{
		return new byte[2]
		{
			Convert.ToByte(value / 256),
			Convert.ToByte(value % 256)
		};
	}

	internal void HandleProxyCommandError(byte[] response, string destinationHost, int destinationPort)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		byte b = response[1];
		byte[] array = new byte[4];
		for (int i = 0; i < 4; i++)
		{
			array[i] = response[i + 4];
		}
		IPAddress iPAddress = new IPAddress(array);
		short num = BitConverter.ToInt16(new byte[2]
		{
			response[3],
			response[2]
		}, 0);
		string text = b switch
		{
			91 => "connection request was rejected or failed", 
			92 => "connection request was rejected because SOCKS destination cannot connect to identd on the client", 
			93 => "connection request rejected because the client program and identd report different user-ids", 
			_ => string.Format(CultureInfo.InvariantCulture, "proxy client received an unknown reply with the code value '{0}' from the proxy destination", b.ToString(CultureInfo.InvariantCulture)), 
		};
		string message = string.Format(CultureInfo.InvariantCulture, "The {0} concerning destination host {1} port number {2}.  The destination reported the host as {3} port {4}.", text, destinationHost, destinationPort, iPAddress.ToString(), num.ToString(CultureInfo.InvariantCulture));
		throw new ProxyException(message);
	}

	internal void WaitForData(NetworkStream stream)
	{
		int num = 0;
		while (!stream.DataAvailable)
		{
			Thread.Sleep(50);
			num += 50;
			if (num > 15000)
			{
				throw new ProxyException("A timeout while waiting for the proxy destination to respond.");
			}
		}
	}

	public void CancelAsync()
	{
		if (_asyncWorker != null && !_asyncWorker.CancellationPending && _asyncWorker.IsBusy)
		{
			_asyncCancelled = true;
			_asyncWorker.CancelAsync();
		}
	}

	private void CreateAsyncWorker()
	{
		if (_asyncWorker != null)
		{
			_asyncWorker.Dispose();
		}
		_asyncException = null;
		_asyncWorker = null;
		_asyncCancelled = false;
		_asyncWorker = new BackgroundWorker();
	}

	public void CreateConnectionAsync(string destinationHost, int destinationPort)
	{
		if (_asyncWorker != null && _asyncWorker.IsBusy)
		{
			throw new InvalidOperationException("The Socks4/4a object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");
		}
		CreateAsyncWorker();
		_asyncWorker.WorkerSupportsCancellation = true;
		_asyncWorker.DoWork += CreateConnectionAsync_DoWork;
		_asyncWorker.RunWorkerCompleted += CreateConnectionAsync_RunWorkerCompleted;
		object[] argument = new object[2] { destinationHost, destinationPort };
		_asyncWorker.RunWorkerAsync(argument);
	}

	private void CreateConnectionAsync_DoWork(object sender, DoWorkEventArgs e)
	{
		try
		{
			object[] array = (object[])e.Argument;
			e.Result = CreateConnection((string)array[0], (int)array[1]);
		}
		catch (Exception asyncException)
		{
			_asyncException = asyncException;
		}
	}

	private void CreateConnectionAsync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if (this.CreateConnectionAsyncCompleted != null)
		{
			this.CreateConnectionAsyncCompleted(this, new CreateConnectionAsyncCompletedEventArgs(_asyncException, _asyncCancelled, (TcpClient)e.Result));
		}
	}
}
