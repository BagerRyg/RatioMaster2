using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Starksoft.Net.Proxy;

public class HttpProxyClient : IProxyClient
{
	private enum HttpResponseCodes
	{
		None = 0,
		Continue = 100,
		SwitchingProtocols = 101,
		OK = 200,
		Created = 201,
		Accepted = 202,
		NonAuthoritiveInformation = 203,
		NoContent = 204,
		ResetContent = 205,
		PartialContent = 206,
		MultipleChoices = 300,
		MovedPermanetly = 301,
		Found = 302,
		SeeOther = 303,
		NotModified = 304,
		UserProxy = 305,
		TemporaryRedirect = 307,
		BadRequest = 400,
		Unauthorized = 401,
		PaymentRequired = 402,
		Forbidden = 403,
		NotFound = 404,
		MethodNotAllowed = 405,
		NotAcceptable = 406,
		ProxyAuthenticantionRequired = 407,
		RequestTimeout = 408,
		Conflict = 409,
		Gone = 410,
		PreconditionFailed = 411,
		RequestEntityTooLarge = 413,
		RequestURITooLong = 414,
		UnsupportedMediaType = 415,
		RequestedRangeNotSatisfied = 416,
		ExpectationFailed = 417,
		InternalServerError = 500,
		NotImplemented = 501,
		BadGateway = 502,
		ServiceUnavailable = 503,
		GatewayTimeout = 504,
		HTTPVersionNotSupported = 505
	}

	private const int HTTP_PROXY_DEFAULT_PORT = 8080;

	private const string HTTP_PROXY_CONNECT_CMD = "CONNECT {0}:{1} HTTP/1.0 \r\nHost: {0}:{1}\r\n\r\n";

	private const int WAIT_FOR_DATA_INTERVAL = 50;

	private const int WAIT_FOR_DATA_TIMEOUT = 150000;

	private const string PROXY_NAME = "HTTP";

	private string _proxyHost;

	private int _proxyPort;

	private HttpResponseCodes _respCode;

	private string _respText;

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

	public string ProxyName => "HTTP";

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

	public HttpProxyClient()
	{
	}

	public HttpProxyClient(string proxyHost)
	{
		_proxyHost = proxyHost;
		_proxyPort = 8080;
	}

	public HttpProxyClient(string proxyHost, int proxyPort)
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
			SendConnectionCommand(tcpClient, destinationHost, destinationPort);
			_tcpClient = null;
			return tcpClient;
		}
		catch (SocketException innerException)
		{
			_tcpClient = null;
			throw new ProxyException(string.Format(CultureInfo.InvariantCulture, "Connection to proxy host {0} on port {1} failed.", _proxyHost, _proxyPort.ToString(CultureInfo.InvariantCulture)), innerException);
		}
	}

	private void SendConnectionCommand(TcpClient tcpClient, string host, int port)
	{
		NetworkStream stream = tcpClient.GetStream();
		string s = string.Format(CultureInfo.InvariantCulture, "CONNECT {0}:{1} HTTP/1.0 \r\nHost: {0}:{1}\r\n\r\n", host, port.ToString(CultureInfo.InvariantCulture));
		byte[] bytes = Encoding.ASCII.GetBytes(s);
		stream.Write(bytes, 0, bytes.Length);
		WaitForData(stream);
		byte[] array = new byte[tcpClient.ReceiveBufferSize];
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		long num2 = 0L;
		do
		{
			num = stream.Read(array, 0, tcpClient.ReceiveBufferSize);
			num2 += num;
			stringBuilder.Append(Encoding.UTF8.GetString(array, 0, num));
		}
		while (stream.DataAvailable);
		ParseResponse(stringBuilder.ToString());
		if (_respCode != HttpResponseCodes.OK)
		{
			HandleProxyCommandError(host, port);
		}
	}

	private void HandleProxyCommandError(string destinationHost, int destinationPort)
	{
		string message;
		switch (_respCode)
		{
		case HttpResponseCodes.None:
			message = string.Format(CultureInfo.InvariantCulture, "Proxy destination {0} on port {1} failed to return a recognized HTTP response code.  Server response: {2}", _proxyHost, _proxyPort.ToString(CultureInfo.InvariantCulture), _respText);
			break;
		case HttpResponseCodes.BadGateway:
			message = string.Format(CultureInfo.InvariantCulture, "Proxy destination {0} on port {1} responded with a 502 code - Bad Gateway.  If you are connecting to a Microsoft ISA destination please refer to knowledge based article Q283284 for more information.  Server response: {2}", _proxyHost, _proxyPort.ToString(CultureInfo.InvariantCulture), _respText);
			break;
		default:
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] array = new object[4]
			{
				_proxyHost,
				_proxyPort.ToString(CultureInfo.InvariantCulture),
				null,
				null
			};
			int respCode = (int)_respCode;
			array[2] = respCode.ToString(CultureInfo.InvariantCulture);
			array[3] = _respText;
			message = string.Format(invariantCulture, "Proxy destination {0} on port {1} responded with a {2} code - {3}", array);
			break;
		}
		}
		throw new ProxyException(message);
	}

	private void WaitForData(NetworkStream stream)
	{
		int num = 0;
		while (!stream.DataAvailable)
		{
			Thread.Sleep(50);
			num += 50;
			if (num > 150000)
			{
				throw new ProxyException("A timeout while waiting for the proxy destination to respond.");
			}
		}
	}

	private void ParseResponse(string response)
	{
		string[] array = null;
		array = response.Replace('\n', ' ').Split('\r');
		ParseCodeAndText(array[0]);
	}

	private void ParseCodeAndText(string line)
	{
		int num = 0;
		int num2 = 0;
		string text = null;
		if (line.IndexOf("HTTP") == -1)
		{
			throw new ProxyException("No HTTP response received from proxy destination: " + line);
		}
		num = line.IndexOf(" ") + 1;
		num2 = line.IndexOf(" ", num);
		text = line.Substring(num, num2 - num);
		int result = 0;
		if (!int.TryParse(text, out result))
		{
			throw new ProxyException("An invalid response code was received from proxy destination: " + line);
		}
		_respCode = (HttpResponseCodes)result;
		_respText = line.Substring(num2 + 1).Trim();
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
			throw new InvalidOperationException("The HttpProxy object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");
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
