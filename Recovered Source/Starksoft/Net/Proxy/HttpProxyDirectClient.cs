using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace Starksoft.Net.Proxy;

public class HttpProxyDirectClient : IProxyClient
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

	public HttpProxyDirectClient()
	{
	}

	public HttpProxyDirectClient(string proxyHost)
	{
		_proxyHost = proxyHost;
		_proxyPort = 8080;
	}

	public HttpProxyDirectClient(string proxyHost, int proxyPort)
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
			_tcpClient = null;
			return tcpClient;
		}
		catch (SocketException innerException)
		{
			_tcpClient = null;
			throw new ProxyException(string.Format(CultureInfo.InvariantCulture, "Connection to proxy host {0} on port {1} failed.", _proxyHost, _proxyPort.ToString(CultureInfo.InvariantCulture)), innerException);
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
