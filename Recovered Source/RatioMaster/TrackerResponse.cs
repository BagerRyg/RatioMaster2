using System;
using System.IO;
using System.IO.Compression;
using BitTorrent;

namespace RatioMaster;

internal class TrackerResponse
{
	private string _headers = "";

	private string _body = "";

	private ValueDictionary _dict;

	private string _contentEncoding = "";

	private string _contentType = "";

	private string _charset = "";

	private bool _chunkedEncoding;

	private int _statusCode;

	public string RedirectionURL = "";

	public bool response_status_302;

	public bool doRedirect;

	public int StatusCode => _statusCode;

	public bool IsHttpError => _statusCode >= 400;

	private GZipStream unzipStream;

	public ValueDictionary Dict => _dict;

	public string Headers => _headers;

	public string Body => _body;

	public string ContentEncoding => _contentEncoding;

	public string ContentType => _contentType;

	public string Charset => _charset;

	public TrackerResponse(MemoryStream responseStream)
	{
		Stream stream = new MemoryStream();
		StreamReader streamReader = new StreamReader(responseStream);
		responseStream.Position = 0L;
		string newLineStr = getNewLineStr(streamReader);
		_headers = "";
		string text;
		do
		{
			text = streamReader.ReadLine();
			int num;
			if (text.StartsWith("HTTP/"))
			{
				string[] array = text.Split(' ');
				if (array.Length > 1)
				{
					int.TryParse(array[1], out _statusCode);
				}
			}
			if ((num = text.IndexOf("302 Found")) >= 0 || (num = text.IndexOf("302 Moved")) >= 0)
			{
				response_status_302 = true;
			}
			else if ((num = text.IndexOf("Location: ")) >= 0)
			{
				RedirectionURL = text.Substring(num + 10);
			}
			else if ((num = text.IndexOf("Content-Encoding: ")) >= 0)
			{
				_contentEncoding = text.Substring(num + 18).ToLower();
			}
			else if ((num = text.IndexOf("Content-Type: ")) >= 0)
			{
				_contentType = text.Substring(num + 14).ToLower();
			}
			else if ((num = text.IndexOf("charset=")) >= 0)
			{
				_charset = text.Substring(num + 8).ToLower();
			}
			else if ((num = text.IndexOf("Transfer-Encoding: chunked")) >= 0)
			{
				_chunkedEncoding = true;
			}
			_headers = _headers + text + newLineStr;
		}
		while (text.Length != 0);
		responseStream.Position = _headers.Length;
		if (response_status_302 && RedirectionURL != "")
		{
			doRedirect = true;
			return;
		}
		if (_chunkedEncoding)
		{
			string text2 = "";
			text2 = streamReader.ReadLine();
			int num2 = Convert.ToInt32(text2.Split(' ')[0], 16);
			while (num2 > 0)
			{
				byte[] buffer = new byte[num2];
				responseStream.Position = responseStream.Position + text2.Length + newLineStr.Length;
				responseStream.Read(buffer, 0, num2);
				stream.Write(buffer, 0, num2);
				streamReader.ReadLine();
				text2 = streamReader.ReadLine();
				responseStream.Position += newLineStr.Length;
				try
				{
					num2 = Convert.ToInt32(text2.Split(' ')[0], 16);
				}
				catch (Exception)
				{
					num2 = 0;
				}
			}
		}
		else
		{
			byte[] array = new byte[responseStream.Length - responseStream.Position];
			responseStream.Read(array, 0, array.Length);
			stream.Write(array, 0, array.Length);
		}
		stream.Position = 0L;
		_dict = parseBEncodeDict((MemoryStream)stream);
		stream.Position = 0L;
		StreamReader streamReader2 = new StreamReader(stream);
		_body = streamReader2.ReadToEnd();
		stream.Dispose();
		streamReader2.Dispose();
		streamReader.Dispose();
		if (unzipStream != null)
		{
			unzipStream.Dispose();
		}
	}

	private string getNewLineStr(StreamReader streamReader)
	{
		long position = streamReader.BaseStream.Position;
		string result = "\r";
		char c;
		do
		{
			c = (char)streamReader.BaseStream.ReadByte();
		}
		while (c != '\r' && c != '\n');
		if (c == '\r' && (ushort)streamReader.BaseStream.ReadByte() == 10)
		{
			result = "\r\n";
		}
		streamReader.BaseStream.Position = position;
		return result;
	}

	private ValueDictionary parseBEncodeDict(MemoryStream responseStream)
	{
		ValueDictionary result = null;
		if (_contentEncoding == "gzip" || _contentEncoding == "x-gzip")
		{
			unzipStream = new GZipStream(responseStream, CompressionMode.Decompress);
			try
			{
				result = (ValueDictionary)BEncode.Parse(unzipStream);
			}
			catch (Exception ex)
			{
				Console.Write(ex.StackTrace);
				try
				{
					result = (ValueDictionary)BEncode.Parse(responseStream);
				}
				catch (Exception ex2)
				{
					Console.Write(ex2.StackTrace);
				}
			}
		}
		else
		{
			try
			{
				result = (ValueDictionary)BEncode.Parse(responseStream);
			}
			catch (Exception ex3)
			{
				Console.Write(ex3.StackTrace);
			}
		}
		return result;
	}

	private void saveArrayToFile(byte[] arr, string filename)
	{
		FileStream fileStream = File.OpenWrite(filename);
		fileStream.Write(arr, 0, arr.Length);
		fileStream.Close();
	}

	private void saveStreamToFile(MemoryStream ms, string filename)
	{
		FileStream fileStream = File.OpenWrite(filename);
		ms.WriteTo(fileStream);
		fileStream.Close();
	}
}
