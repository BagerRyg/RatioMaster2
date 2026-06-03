using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Starksoft.Net.Proxy;

public class Socks5ProxyClient : IProxyClient
{
	private enum SocksAuthentication
	{
		None,
		UsernamePassword
	}

	private const string PROXY_NAME = "SOCKS5";

	private const int SOCKS5_DEFAULT_PORT = 1080;

	private const byte SOCKS5_VERSION_NUMBER = 5;

	private const byte SOCKS5_RESERVED = 0;

	private const byte SOCKS5_AUTH_NUMBER_OF_AUTH_METHODS_SUPPORTED = 2;

	private const byte SOCKS5_AUTH_METHOD_NO_AUTHENTICATION_REQUIRED = 0;

	private const byte SOCKS5_AUTH_METHOD_GSSAPI = 1;

	private const byte SOCKS5_AUTH_METHOD_USERNAME_PASSWORD = 2;

	private const byte SOCKS5_AUTH_METHOD_IANA_ASSIGNED_RANGE_BEGIN = 3;

	private const byte SOCKS5_AUTH_METHOD_IANA_ASSIGNED_RANGE_END = 127;

	private const byte SOCKS5_AUTH_METHOD_RESERVED_RANGE_BEGIN = 128;

	private const byte SOCKS5_AUTH_METHOD_RESERVED_RANGE_END = 254;

	private const byte SOCKS5_AUTH_METHOD_REPLY_NO_ACCEPTABLE_METHODS = byte.MaxValue;

	private const byte SOCKS5_CMD_CONNECT = 1;

	private const byte SOCKS5_CMD_BIND = 2;

	private const byte SOCKS5_CMD_UDP_ASSOCIATE = 3;

	private const byte SOCKS5_CMD_REPLY_SUCCEEDED = 0;

	private const byte SOCKS5_CMD_REPLY_GENERAL_SOCKS_SERVER_FAILURE = 1;

	private const byte SOCKS5_CMD_REPLY_CONNECTION_NOT_ALLOWED_BY_RULESET = 2;

	private const byte SOCKS5_CMD_REPLY_NETWORK_UNREACHABLE = 3;

	private const byte SOCKS5_CMD_REPLY_HOST_UNREACHABLE = 4;

	private const byte SOCKS5_CMD_REPLY_CONNECTION_REFUSED = 5;

	private const byte SOCKS5_CMD_REPLY_TTL_EXPIRED = 6;

	private const byte SOCKS5_CMD_REPLY_COMMAND_NOT_SUPPORTED = 7;

	private const byte SOCKS5_CMD_REPLY_ADDRESS_TYPE_NOT_SUPPORTED = 8;

	private const byte SOCKS5_ADDRTYPE_IPV4 = 1;

	private const byte SOCKS5_ADDRTYPE_DOMAIN_NAME = 3;

	private const byte SOCKS5_ADDRTYPE_IPV6 = 4;

	private string _proxyHost;

	private int _proxyPort;

	private string _proxyUserName;

	private string _proxyPassword;

	private SocksAuthentication _proxyAuthMethod;

	private TcpClient _tcpClient;

	private IPEndPoint _BindIpEndpoint;

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

	public string ProxyName => "SOCKS5";

	public string ProxyUserName
	{
		get
		{
			return _proxyUserName;
		}
		set
		{
			_proxyUserName = value;
		}
	}

	public string ProxyPassword
	{
		get
		{
			return _proxyPassword;
		}
		set
		{
			_proxyPassword = value;
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

	public Socks5ProxyClient()
	{
	}

	public Socks5ProxyClient(string proxyHost)
	{
		_proxyHost = proxyHost;
		_proxyPort = 1080;
	}

	public Socks5ProxyClient(string proxyHost, int proxyPort)
	{
		_proxyHost = proxyHost;
		_proxyPort = proxyPort;
	}

	public Socks5ProxyClient(string proxyHost, string proxyUserName, string proxyPassword)
	{
		_proxyHost = proxyHost;
		_proxyPort = 1080;
		_proxyUserName = proxyUserName;
		_proxyPassword = proxyPassword;
	}

	public Socks5ProxyClient(string proxyHost, int proxyPort, string proxyUserName, string proxyPassword)
	{
		_proxyHost = proxyHost;
		_proxyPort = proxyPort;
		_proxyUserName = proxyUserName;
		_proxyPassword = proxyPassword;
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
			DetermineClientAuthMethod();
			NegotiateServerAuthMethod(tcpClient);
			SendCommand(tcpClient.GetStream(), 1, destinationHost, destinationPort);
			_tcpClient = null;
			return tcpClient;
		}
		catch (Exception innerException)
		{
			_tcpClient = null;
			throw new ProxyException(string.Format(CultureInfo.InvariantCulture, "Connection to proxy host {0} on port {1} failed.", _proxyHost, _proxyPort.ToString(CultureInfo.InvariantCulture)), innerException);
		}
	}

	private void DetermineClientAuthMethod()
	{
		if (_proxyUserName != null && _proxyPassword != null)
		{
			_proxyAuthMethod = SocksAuthentication.UsernamePassword;
		}
		else
		{
			_proxyAuthMethod = SocksAuthentication.None;
		}
	}

	private void NegotiateServerAuthMethod(TcpClient tcpClient)
	{
		NetworkStream stream = tcpClient.GetStream();
		byte[] array = new byte[4] { 5, 2, 0, 2 };
		stream.Write(array, 0, array.Length);
		byte[] array2 = new byte[2];
		stream.Read(array2, 0, array2.Length);
		byte b = array2[1];
		switch (b)
		{
		case byte.MaxValue:
			tcpClient.Close();
			throw new ProxyException("The proxy destination does not accept the supported proxy client authentication methods.");
		case 2:
			if (_proxyAuthMethod == SocksAuthentication.None)
			{
				tcpClient.Close();
				throw new ProxyException("The proxy destination requires a username and password for authentication.");
			}
			break;
		}
		if (b == 2)
		{
			byte[] array3 = new byte[_proxyUserName.Length + _proxyPassword.Length + 3];
			array3[0] = 5;
			array3[1] = (byte)_proxyUserName.Length;
			Array.Copy(Encoding.ASCII.GetBytes(_proxyUserName), 0, array3, 2, _proxyUserName.Length);
			array3[_proxyUserName.Length + 2] = (byte)_proxyPassword.Length;
			Array.Copy(Encoding.ASCII.GetBytes(_proxyPassword), 0, array3, _proxyUserName.Length + 3, _proxyPassword.Length);
		}
	}

	private byte GetDestAddressType(string host)
	{
		IPAddress address = null;
		if (!IPAddress.TryParse(host, out address))
		{
			return 3;
		}
		return address.AddressFamily switch
		{
			AddressFamily.InterNetwork => 1, 
			AddressFamily.InterNetworkV6 => 4, 
			_ => throw new ProxyException(string.Format(CultureInfo.InvariantCulture, "The host addess {0} of type '{1}' is not a supported address type.  The supported types are InterNetwork and InterNetworkV6.", host, Enum.GetName(typeof(AddressFamily), address.AddressFamily))), 
		};
	}

	private byte[] GetDestAddressBytes(byte addressType, string host)
	{
		switch (addressType)
		{
		case 1:
		case 4:
			return IPAddress.Parse(host).GetAddressBytes();
		case 3:
		{
			byte[] array = new byte[host.Length + 1];
			array[0] = Convert.ToByte(host.Length);
			Encoding.ASCII.GetBytes(host).CopyTo(array, 1);
			return array;
		}
		default:
			return null;
		}
	}

	private byte[] GetDestPortBytes(int value)
	{
		return new byte[2]
		{
			Convert.ToByte(value / 256),
			Convert.ToByte(value % 256)
		};
	}

	private void SendCommand(NetworkStream stream, byte command, string destinationHost, int destinationPort)
	{
		byte destAddressType = GetDestAddressType(destinationHost);
		byte[] destAddressBytes = GetDestAddressBytes(destAddressType, destinationHost);
		byte[] destPortBytes = GetDestPortBytes(destinationPort);
		byte[] array = new byte[4 + destAddressBytes.Length + 2];
		array[0] = 5;
		array[1] = command;
		array[2] = 0;
		array[3] = destAddressType;
		destAddressBytes.CopyTo(array, 4);
		destPortBytes.CopyTo(array, 4 + destAddressBytes.Length);
		stream.Write(array, 0, array.Length);
		byte[] array2 = new byte[255];
		stream.Read(array2, 0, array2.Length);
		if (array2[1] != 0)
		{
			HandleProxyCommandError(array2, destinationHost, destinationPort);
		}
	}

	private void HandleProxyCommandError(byte[] response, string destinationHost, int destinationPort)
	{
		byte b = response[1];
		byte b2 = response[3];
		string text = "";
		short num = 0;
		switch (b2)
		{
		case 3:
		{
			int num2 = Convert.ToInt32(response[4]);
			byte[] array3 = new byte[num2];
			for (int k = 0; k < num2; k++)
			{
				array3[k] = response[k + 5];
			}
			text = Encoding.ASCII.GetString(array3);
			num = BitConverter.ToInt16(new byte[2]
			{
				response[6 + num2],
				response[5 + num2]
			}, 0);
			break;
		}
		case 1:
		{
			byte[] array2 = new byte[4];
			for (int j = 0; j < 4; j++)
			{
				array2[j] = response[j + 4];
			}
			IPAddress iPAddress2 = new IPAddress(array2);
			text = iPAddress2.ToString();
			num = BitConverter.ToInt16(new byte[2]
			{
				response[9],
				response[8]
			}, 0);
			break;
		}
		case 4:
		{
			byte[] array = new byte[16];
			for (int i = 0; i < 16; i++)
			{
				array[i] = response[i + 4];
			}
			IPAddress iPAddress = new IPAddress(array);
			text = iPAddress.ToString();
			num = BitConverter.ToInt16(new byte[2]
			{
				response[21],
				response[20]
			}, 0);
			break;
		}
		}
		string text2 = b switch
		{
			1 => "a general socks destination failure occurred", 
			2 => "the connection is not allowed by proxy destination rule set", 
			3 => "the network was unreachable", 
			4 => "the host was unreachable", 
			5 => "the connection was refused by the remote network", 
			6 => "the time to live (TTL) has expired", 
			7 => "the command issued by the proxy client is not supported by the proxy destination", 
			8 => "the address type specified is not supported", 
			_ => string.Format(CultureInfo.InvariantCulture, "that an unknown reply with the code value '{0}' was received by the destination", b.ToString(CultureInfo.InvariantCulture)), 
		};
		string message = string.Format(CultureInfo.InvariantCulture, "The {0} concerning destination host {1} port number {2}.  The destination reported the host as {3} port {4}.", text2, destinationHost, destinationPort, text, num.ToString(CultureInfo.InvariantCulture));
		throw new ProxyException(message);
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
			throw new InvalidOperationException("The Socks4 object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");
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
