using System;
using System.IO;
using System.Text;

namespace BitTorrent;

public class ValueString : BEncodeValue
{
	private const int MaxStringLength = 16 * 1024 * 1024;
	private const int MaxLengthDigits = 9;

	private string v;

	private byte[] data;

	public int Length => v.Length;

	public byte[] Bytes => data;

	public string String
	{
		get
		{
			return v;
		}
		set
		{
			v = value;
			data = Encoding.GetEncoding(28591).GetBytes(v);
		}
	}

	public byte[] Encode()
	{
		string text = v.Length + ":";
		byte[] bytes = Encoding.GetEncoding(28591).GetBytes(text);
		byte[] array = new byte[text.Length + data.Length];
		for (int i = 0; i < text.Length; i++)
		{
			array[i] = bytes[i];
		}
		for (int j = 0; j < data.Length; j++)
		{
			array[j + text.Length] = data[j];
		}
		return array;
	}

	public ValueString(string StringValue)
	{
		String = StringValue;
	}

	public ValueString()
	{
	}

	public void Clear()
	{
		if (data != null)
		{
			Array.Clear(data, 0, data.Length);
		}
		data = Array.Empty<byte>();
		v = string.Empty;
	}

	public void Parse(Stream s)
	{
		throw new TorrentException("Parse method not supported, the first byte must be passed into the string parse routine.");
	}

	public void Parse(Stream s, byte firstByte)
	{
		char c = (char)firstByte;
		string text = c.ToString();
		if (!char.IsNumber(text[0]))
		{
			throw new TorrentException("Invalid bencoded string length.");
		}
		int num = s.ReadByte();
		char c2 = (char)num;
		while (c2 != ':' && num != -1)
		{
			if (!char.IsNumber(c2) || text.Length >= MaxLengthDigits)
			{
				throw new TorrentException("Invalid bencoded string length.");
			}
			text += c2;
			num = s.ReadByte();
			c2 = (char)num;
		}
		if (num == -1)
		{
			throw new IncompleteTorrentData("Unexpected end of bencoded string length.");
		}
		try
		{
			int num2 = int.Parse(text);
			if (num2 < 0 || num2 > MaxStringLength)
			{
				throw new TorrentException("Bencoded string exceeded the maximum length.");
			}
			data = new byte[num2];
			int totalRead = 0;
			while (totalRead < num2)
			{
				int read = s.Read(data, totalRead, num2 - totalRead);
				if (read <= 0)
				{
					throw new IncompleteTorrentData("Unexpected end of bencoded string data.");
				}
				totalRead += read;
			}
			v = Encoding.GetEncoding(28591).GetString(data);
		}
		catch (TorrentException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new TorrentException("Invalid bencoded string: " + ex.Message);
		}
	}
}
