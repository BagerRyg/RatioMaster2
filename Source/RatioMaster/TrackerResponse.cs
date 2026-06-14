using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using BitTorrent;

namespace RatioMaster;

internal class TrackerResponse
{
	private const int MaxHeaderBytes = 64 * 1024;
	private const int MaxDecodedBodyBytes = 4 * 1024 * 1024;

	private string _headers = "";
	private ValueDictionary _dict;
	private string _contentEncoding = "";
	private string _contentType = "";
	private string _charset = "";
	private int _statusCode;

	public string RedirectionURL = "";
	public bool response_status_302;
	public bool doRedirect;

	public int StatusCode => _statusCode;
	public bool IsHttpError => _statusCode >= 400;
	public ValueDictionary Dict => _dict;
	public string Headers => _headers;
	public string Body => "";
	public string ContentEncoding => _contentEncoding;
	public string ContentType => _contentType;
	public string Charset => _charset;

	public void ClearSensitiveData()
	{
		BEncode.Clear(_dict);
		_dict = null;
		_headers = string.Empty;
		RedirectionURL = string.Empty;
	}

	public TrackerResponse(MemoryStream responseStream)
	{
		if (responseStream == null)
		{
			throw new ArgumentNullException(nameof(responseStream));
		}

		byte[] responseBytes = responseStream.ToArray();
		try
		{
			int bodyOffset = FindHeaderEnd(responseBytes);
			_headers = Encoding.ASCII.GetString(responseBytes, 0, bodyOffset);
			bool chunked = ParseHeaders(_headers);
			if (response_status_302 && !string.IsNullOrWhiteSpace(RedirectionURL))
			{
				doRedirect = true;
				return;
			}

			byte[] body = new byte[responseBytes.Length - bodyOffset];
			Buffer.BlockCopy(responseBytes, bodyOffset, body, 0, body.Length);
			try
			{
				byte[] decoded = chunked ? DecodeChunked(body) : body;
				if (!ReferenceEquals(decoded, body))
				{
					Array.Clear(body, 0, body.Length);
				}
				if (_contentEncoding == "gzip" || _contentEncoding == "x-gzip")
				{
					byte[] decompressed = Decompress(decoded);
					Array.Clear(decoded, 0, decoded.Length);
					decoded = decompressed;
				}
				if (decoded.Length > MaxDecodedBodyBytes)
				{
					throw new InvalidDataException("Decoded tracker response exceeded the 4 MB limit.");
				}
				using MemoryStream bodyStream = new MemoryStream(decoded, writable: false);
				_dict = BEncode.Parse(bodyStream) as ValueDictionary;
				Array.Clear(decoded, 0, decoded.Length);
			}
			catch
			{
				Array.Clear(body, 0, body.Length);
				throw;
			}
		}
		finally
		{
			Array.Clear(responseBytes, 0, responseBytes.Length);
		}
	}

	private bool ParseHeaders(string headers)
	{
		bool chunked = false;
		string[] lines = headers.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
		if (lines.Length > 0)
		{
			string[] statusParts = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (statusParts.Length > 1)
			{
				int.TryParse(statusParts[1], NumberStyles.None, CultureInfo.InvariantCulture, out _statusCode);
			}
		}
		response_status_302 = _statusCode == 301 || _statusCode == 302 || _statusCode == 303 || _statusCode == 307 || _statusCode == 308;
		for (int i = 1; i < lines.Length; i++)
		{
			int separator = lines[i].IndexOf(':');
			if (separator <= 0)
			{
				continue;
			}
			string name = lines[i].Substring(0, separator).Trim();
			string value = lines[i].Substring(separator + 1).Trim();
			if (name.Equals("Location", StringComparison.OrdinalIgnoreCase))
			{
				RedirectionURL = value;
			}
			else if (name.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase))
			{
				_contentEncoding = value.ToLowerInvariant();
			}
			else if (name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
			{
				_contentType = value.ToLowerInvariant();
				int charsetIndex = value.IndexOf("charset=", StringComparison.OrdinalIgnoreCase);
				if (charsetIndex >= 0)
				{
					_charset = value.Substring(charsetIndex + 8).Trim().ToLowerInvariant();
				}
			}
			else if (name.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase)
				&& value.IndexOf("chunked", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				chunked = true;
			}
		}
		return chunked;
	}

	private static int FindHeaderEnd(byte[] data)
	{
		int limit = Math.Min(data.Length, MaxHeaderBytes);
		for (int i = 0; i < limit - 1; i++)
		{
			if (i + 3 < limit && data[i] == 13 && data[i + 1] == 10 && data[i + 2] == 13 && data[i + 3] == 10)
			{
				return i + 4;
			}
			if (data[i] == 10 && data[i + 1] == 10)
			{
				return i + 2;
			}
		}
		throw new InvalidDataException("Tracker response does not contain a valid HTTP header terminator.");
	}

	private static byte[] DecodeChunked(byte[] body)
	{
		using MemoryStream output = new MemoryStream();
		int position = 0;
		while (true)
		{
			string sizeLine = ReadAsciiLine(body, ref position);
			int extension = sizeLine.IndexOf(';');
			if (extension >= 0)
			{
				sizeLine = sizeLine.Substring(0, extension);
			}
			if (!int.TryParse(sizeLine.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int chunkSize) || chunkSize < 0)
			{
				throw new InvalidDataException("Tracker returned an invalid chunk size.");
			}
			if (chunkSize == 0)
			{
				break;
			}
			if (chunkSize > MaxDecodedBodyBytes - output.Length || position > body.Length - chunkSize)
			{
				throw new InvalidDataException("Tracker chunk exceeded the response limit.");
			}
			output.Write(body, position, chunkSize);
			position += chunkSize;
			ConsumeLineEnding(body, ref position);
		}
		return output.ToArray();
	}

	private static string ReadAsciiLine(byte[] data, ref int position)
	{
		int start = position;
		while (position < data.Length && data[position] != 10 && data[position] != 13)
		{
			position++;
			if (position - start > 128)
			{
				throw new InvalidDataException("Tracker chunk header is too long.");
			}
		}
		if (position >= data.Length)
		{
			throw new InvalidDataException("Tracker chunk header is incomplete.");
		}
		string result = Encoding.ASCII.GetString(data, start, position - start);
		ConsumeLineEnding(data, ref position);
		return result;
	}

	private static void ConsumeLineEnding(byte[] data, ref int position)
	{
		if (position < data.Length && data[position] == 13)
		{
			position++;
		}
		if (position >= data.Length || data[position] != 10)
		{
			throw new InvalidDataException("Tracker chunk line ending is invalid.");
		}
		position++;
	}

	private static byte[] Decompress(byte[] compressed)
	{
		using MemoryStream input = new MemoryStream(compressed, writable: false);
		using GZipStream gzip = new GZipStream(input, CompressionMode.Decompress);
		using MemoryStream output = new MemoryStream();
		byte[] buffer = new byte[8192];
		try
		{
			int read;
			while ((read = gzip.Read(buffer, 0, buffer.Length)) > 0)
			{
				if (output.Length + read > MaxDecodedBodyBytes)
				{
					throw new InvalidDataException("Decompressed tracker response exceeded the 4 MB limit.");
				}
				output.Write(buffer, 0, read);
			}
			return output.ToArray();
		}
		finally
		{
			Array.Clear(buffer, 0, buffer.Length);
		}
	}
}
