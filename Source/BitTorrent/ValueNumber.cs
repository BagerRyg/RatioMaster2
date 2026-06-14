using System;
using System.IO;
using System.Text;

namespace BitTorrent;

public class ValueNumber : BEncodeValue
{
	private const int MaxNumberLength = 32;

	private string v;

	private byte[] data;

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

	public long Integer
	{
		get
		{
			return long.Parse(v);
		}
		set
		{
			String = value.ToString();
		}
	}

	public byte[] Encode()
	{
		byte[] array = new byte[data.Length + 2];
		array[0] = 105;
		for (int i = 0; i < data.Length; i++)
		{
			array[i + 1] = data[i];
		}
		array[data.Length + 1] = 101;
		return array;
	}

	public ValueNumber(long number)
	{
		v = number.ToString();
		String = v;
	}

	public ValueNumber()
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
		string text = string.Empty;
		int num = s.ReadByte();
		char c = (char)num;
		while (c != 'e' && num != -1)
		{
			if (text.Length >= MaxNumberLength)
			{
				throw new TorrentException("Bencoded number exceeded the maximum length.");
			}
			text += c;
			num = s.ReadByte();
			c = (char)num;
		}
		if (num == -1)
		{
			throw new IncompleteTorrentData("Unexpected end of bencoded number.");
		}
		try
		{
			String = long.Parse(text).ToString();
		}
		catch (Exception ex)
		{
			throw new TorrentException("Invalid bencoded number: " + ex.Message);
		}
	}
}
