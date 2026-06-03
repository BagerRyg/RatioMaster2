using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RatioMaster;

internal class TrackerSslClient
{
	private const int TrackerSocketTimeoutMs = 15000;

	private TcpClient _client;

	private int _port = 443;

	private string _host;

	private MainForm _mainForm;

	public TrackerSslClient(Uri reqUri, MainForm mainForm)
	{
		_host = reqUri.Host;
		_port = reqUri.Port;
		_mainForm = mainForm;
	}

	public MemoryStream ConnectToServer(string msg)
	{
		MemoryStream memoryStream = new MemoryStream();
		try
		{
			using (_client = new TcpClient())
			{
				_client.Connect(_host, _port);
				_client.ReceiveTimeout = TrackerSocketTimeoutMs;
				_client.SendTimeout = TrackerSocketTimeoutMs;
				using SslStream sslStream = new SslStream(_client.GetStream(), leaveInnerStreamOpen: false, CertificateValidationCallback);
				sslStream.AuthenticateAsClient(_host);
				sslStream.ReadTimeout = TrackerSocketTimeoutMs;
				sslStream.WriteTimeout = TrackerSocketTimeoutMs;
				byte[] bytes = Encoding.UTF8.GetBytes(msg);
				Console.WriteLine("Sending message to server: " + msg);
				sslStream.Write(bytes, 0, bytes.Length);
				sslStream.Flush();
				byte[] array = new byte[32768];
				while (true)
				{
					int num = sslStream.Read(array, 0, array.Length);
					if (num != 0)
					{
						memoryStream.Write(array, 0, num);
						continue;
					}
					break;
				}
			}
		}
		catch (Exception ex)
		{
			_mainForm.AddLogLine("There was an error talking to the server: " + ex.Message);
		}
		if (memoryStream.Length != 0)
		{
			return memoryStream;
		}
		return null;
	}

	private bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		switch (sslPolicyErrors)
		{
		case SslPolicyErrors.None:
			return true;
		case SslPolicyErrors.RemoteCertificateChainErrors:
			_mainForm.AddLogLine("The X509Chain.ChainStatus returned an array of X509ChainStatus objects containing error information.");
			_mainForm.AddLogLine(sslPolicyErrors.ToString());
			return true;
		case SslPolicyErrors.RemoteCertificateNameMismatch:
			_mainForm.AddLogLine("There was a mismatch of the name on a certificate.");
			return true;
		case SslPolicyErrors.RemoteCertificateNotAvailable:
			_mainForm.AddLogLine("No certificate was available.");
			break;
		default:
			_mainForm.AddLogLine("SSL Certificate Validation Error!");
			break;
		}
		_mainForm.AddLogLine(sslPolicyErrors.ToString());
		return false;
	}
}
